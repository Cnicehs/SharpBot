using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using CliWrap;
using Microsoft.Extensions.Options;
using SharpBot.IOC;
using SharpBot.Notify;

namespace SharpBot.BotPlugin;

[Singlonton(RunningOnAwake = true)]
public class Baidu
{
    private ILogger<Baidu> logger;
    private TelegramBot bot;
    private BaiduClient client;
    private IOptionsMonitor<TelegramConfig> tgConfig;

    public Baidu(ILogger<Baidu> logger, IOptionsMonitor<TelegramConfig> tgConfig, BaiduClient client, TelegramBot bot)
    {
        this.logger = logger;
        this.bot = bot;
        this.client = client;
        this.tgConfig = tgConfig;
        bot.OnRecvMessage += RecvMessage;

        var token = this.client.GetToken().GetAwaiter().GetResult();
        if (string.IsNullOrEmpty(token))
        {
            waitApply = true;
            bot.SendText($"请点击下方链接申请应用,并将授权码发送给该机器人\n{client.GetTokenRegisterUrl()}", tgConfig.CurrentValue.ChatID);
            return;
        }
    }
    
    private bool waitApply;

    void RecvMessage(long uid, string msg)
    {
        if (waitApply)
        {
            var code = Regex.Match(msg, "[a-z0-9]+");
            if (code.Success)
            {
                var result = client.ApplyCode(code.Value).GetAwaiter().GetResult();
                if (result != null)
                {
                    bot.SendText("获取Token成功", tgConfig.CurrentValue.ChatID);
                }

                waitApply = false;
            }
        }

        var shareLinkMatch = Regex.Match(msg, "链接.\\s*(?<t>https://pan.baidu.com/s/\\S+)");
        if (shareLinkMatch.Success)
        {
            var token = this.client.GetToken().GetAwaiter().GetResult();
            if (string.IsNullOrEmpty(token))
            {
                waitApply = true;
                bot.SendText($"请点击下方链接申请应用,并将授权码发送给该机器人\n{client.GetTokenRegisterUrl()}", tgConfig.CurrentValue.ChatID);
                return;
            }

            var pwdMatch = Regex.Match(msg, "提取码.\\s*(?<t>\\S+)");
            if (pwdMatch.Success)
            {
                var shareLink = shareLinkMatch.Groups["t"].Value;

                //可能存在这种带query的，暂时统一一下
                //先不支持https://pan.baidu.com/s/1-rqkYYYzUE2NUsfkd9RCtQ?pwd=wdcv直接这样输入
                // 链接: https://pan.baidu.com/s/1-rqkYYYzUE2NUsfkd9RCtQ?pwd=wdcv 提取码: wdcv 复制这段内容后打开百度网盘手机App，操作更方便哦
                if (shareLink.Contains("?"))
                {
                    shareLink = shareLink.Split("?")[0];
                }

                var pwd = pwdMatch.Groups["t"].Value;
                logger.LogDebug($"{shareLink} -> {pwd}");
                try
                {
                    var result = client.TransFile(shareLink, pwd).GetAwaiter().GetResult();
                    this.bot.SendText($"转存成功\n{tgConfig.CurrentValue.TransFinishAddtionDesc}", uid.ToString());
                }
                catch (Exception e)
                {
                    this.bot.SendText($"{e.Message}\n{tgConfig.CurrentValue.TransFinishAddtionDesc}", uid.ToString());
                }
            }
        }
    }
}