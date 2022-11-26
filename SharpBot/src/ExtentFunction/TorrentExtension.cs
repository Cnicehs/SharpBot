using QBittorrent.Client;

namespace SharpBot.ExtentFunction;

public static class TorrentExtension
{
    public static string GetFileName(this TorrentContent content)
    {
        return content.Name.Split("/")[^1];
    }
}