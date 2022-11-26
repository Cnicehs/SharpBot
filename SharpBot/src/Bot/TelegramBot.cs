using Cysharp.Threading.Tasks;
using SharpBot.IOC;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SharpBot.Notify;

[Singlonton(RunningOnAwake = true)]
public class TelegramBot
{
    private TelegramBotClient client;
    public event Action<long, string> OnRecvMessage;    //接收到消息时
    public event Action<long, Message> AfterRecvMessage; //处理完接收的消息，可以删除或什么的
    static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
    private ILogger<TelegramBot> logger;

    public TelegramBot(ILogger<TelegramBot> logger, IOptionsMonitor<TelegramConfig> tgConfig)
    {
        this.logger = logger;
        this.InitTGBot(tgConfig);
        tgConfig.OnChange(((config, s) => { InitTGBot(tgConfig); }));

        async Task CheckHealth()
        {
            await UniTask.SwitchToTaskPool();
            await semaphoreSlim.WaitAsync();
            try
            {
                if (!await client.TestApiAsync())
                {
                    lock (client)
                    {
                        logger.LogWarning("Try Re Init TG Bot");
                        this.InitTGBot(tgConfig);
                    }
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }

            await Task.Delay(1000);
            CheckHealth();
        }

        CheckHealth();
    }

    public void InitTGBot(IOptionsMonitor<TelegramConfig> tgConfig)
    {
        client = new TelegramBotClient(tgConfig.CurrentValue.BotToken);
        client.StartReceiving(HandleUpdateAsync, ((botClient, exception, arg3) => { return default; }));
    }

    public async Task<bool> SendText(string msg, string chatId)
    {
        await semaphoreSlim.WaitAsync();
        try
        {
            await client.SendTextMessageAsync(new ChatId(long.Parse(chatId)), msg);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e);
            return false;
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        try
        {
            this.logger.LogDebug("收到新消息了From " + update.Message.From.Id);
            OnRecvMessage?.Invoke(update.Message.From.Id, update.Message.Text);
            AfterRecvMessage?.Invoke(update.Message.From.Id, update.Message);
        }
        catch (Exception e)
        {
            this.logger.LogError(e.ToString());
        }
    }
}