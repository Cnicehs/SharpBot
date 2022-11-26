using System.Diagnostics;
using Microsoft.Extensions.Options;
using SharpBot;
using SharpBot.BotPlugin;
using SharpBot.DB.Baidu;

namespace SharpBot_Test;

public class ProcessTest
{
    [Test]
    public async Task APITest()
    {
        var app = new App();
        var api = app.GetService<BaiduClient>();
        var res = await api.TransFile("https://pan.baidu.com/s/1tRqpIjwJfqFanObjMfI4tA", "jxrx");
        Console.WriteLine(res);
    }
}