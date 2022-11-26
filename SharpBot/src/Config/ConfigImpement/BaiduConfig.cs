using SharpBot.BotPlugin;

namespace SharpBot;

[Config]
public class BaiduConfig
{
    public string ClientID { get; set; }
    public string ClientSecret { get; set; }
    public string DownloadPath { get; set; }
}