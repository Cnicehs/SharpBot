using System.Collections.Specialized;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpBot.DB.Baidu;
using SharpBot.IOC;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SharpBot.BotPlugin;

public class VerifyResp
{
    public int errno { get; set; }
    public string err_msg { get; set; }
    public long request_id { get; set; }
    public string randsk { get; set; }
}

[Singlonton] //先写成单例
public class BaiduClient
{
    public static Dictionary<string, string> headers = new Dictionary<string, string>()
    {
        {
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36"
        },
        { "Referer", "pan.baidu.com" }
    };

    public static Dictionary<string, string> ErrorCodeMap = new Dictionary<string, string>()
    {
        { "2", "参数错误。检查必填字段；get/post 参数位置" },
        { "-6", "身份验证失败。access_token 是否有效；部分接口需要申请对应的网盘权限" },
        { "31034", "命中接口频控。核对频控规则;稍后再试;申请单独频控规则" },
        { "42000", "访问过于频繁" },
        { "42001", "rand校验失败" },
        { "42999", "功能下线" },
        { "9100", "一级封禁" },
        { "9200", "二级封禁" },
        { "9300", "三级封禁" },
        { "9400", "四级封禁" },
        { "9500", "五级封禁" }
    };

    private IOptionsMonitor<BaiduConfig> _config;
    private BaiduConfig config => _config.CurrentValue;
    private string accessToken;
    private string refreshToken;
    private DateTime lastUpdate = default;
    private BaiduDB baiduDB;


    public BaiduClient(IOptionsMonitor<BaiduConfig> config, BaiduDB baiduDB)
    {
        this._config = config;
        this.baiduDB = baiduDB;
    }

    public string GetTokenRegisterUrl()
    {
        return
            $"https://openapi.baidu.com/oauth/2.0/authorize?response_type=code&client_id={this.config.ClientID}&redirect_uri=oob&scope=netdisk";
    }

    public async Task<string> ApplyCode(string code)
    {
        var url = "https://openapi.baidu.com/oauth/2.0/token?grant_type=authorization_code";

        NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

        queryString.Add("code", code);
        queryString.Add("client_id", this.config.ClientID);
        queryString.Add("client_secret", this.config.ClientSecret);
        queryString.Add("redirect_uri", "oob");
        url = url + "&" + queryString.ToString();

        var client = this.GetDefaultClient();
        var jString = await client.GetStringAsync(url);
        var json = JObject.Parse(jString);
        if (json.SelectToken("error", false) != null)
        {
            Console.WriteLine("获取token失败");
            return null;
        }

        if (json.SelectToken("access_token") != null && json.SelectToken("refresh_token") != null)
        {
            var proto = new BaiduProto();
            proto.id = this.config.ClientID;
            proto.refresh_token = (string)json.SelectToken("refresh_token");
            refreshToken = proto.refresh_token;
            proto.access_token = (string)json["access_token"];
            this.accessToken = proto.access_token;
            proto.last_update = DateTime.Now;
            lastUpdate = proto.last_update;
            this.baiduDB.InsertOrReplace(proto);
            Console.WriteLine($"Token 获取成功 {this.accessToken}");
            return this.accessToken;
        }

        return null;
    }

    public async Task<string> GetToken()
    {
        if (lastUpdate == default)
        {
            var proto = this.baiduDB.GetProto(this.config.ClientID);
            if (proto != null)
            {
                this.lastUpdate = proto.last_update;
                this.refreshToken = proto.refresh_token;
                this.accessToken = proto.access_token;
            }
            else
            {
                //还未注册过
                return null;
            }
        }

        if (!string.IsNullOrEmpty(accessToken) && DateTime.Now.Subtract(lastUpdate).Days < 27)
        {
            return accessToken;
        }

        var url = "https://openapi.baidu.com/oauth/2.0/token?grant_type=refresh_token";

        NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

        queryString.Add("refresh_token", this.refreshToken);
        queryString.Add("client_id", this.config.ClientID);
        queryString.Add("client_secret", this.config.ClientSecret);
        url = url + "&" + queryString.ToString();

        var client = this.GetDefaultClient();
        var jString = await client.GetStringAsync(url);
        var json = JObject.Parse(jString);
        if (json.SelectToken("error", false) != null)
        {
            Console.WriteLine("刷新token失败");
            return null;
        }

        if (json.SelectToken("access_token") != null && json.SelectToken("refresh_token") != null)
        {
            var proto = new BaiduProto();
            proto.id = this.config.ClientID;
            proto.refresh_token = (string)json.SelectToken("refresh_token");
            refreshToken = proto.refresh_token;
            proto.access_token = (string)json["access_token"];
            this.accessToken = proto.access_token;
            proto.last_update = DateTime.Now;
            lastUpdate = proto.last_update;
            this.baiduDB.InsertOrReplace(proto);
            Console.WriteLine($"Token 刷新成功 {this.accessToken}");
            return this.accessToken;
        }

        return null;
    }

    private async Task<string> GetSUrl(string shareUrl)
    {
        shareUrl = shareUrl.Trim();
        var res = Regex.Match(shareUrl, @"https://pan\.baidu\.com/share/init\?surl=(.+?)$");
        if (res.Success)
        {
            return res.Groups[1].Value;
        }

        // https://pan.baidu.com/s/1GQuNCeAzb92rl2KB68X3wA
        res = Regex.Match(shareUrl, @"https://pan\.baidu\.com/s/\S(.+?)$");
        if (res.Success)
        {
            return res.Groups[1].Value;
        }

        throw new Exception($"通过 {shareUrl} 无法得到surl");
    }

    private async Task<string> GetSeKey(string surl, string pwd)
    {
        string url =
            $"https://pan.baidu.com/rest/2.0/xpan/share?method=verify&surl={surl}&access_token={await this.GetToken()}";
        var client = this.GetDefaultClient();
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("pwd", pwd),
        });
        var resp = await client.PostAsync(url, formContent);
        var result = await resp.Content.ReadFromJsonAsync<VerifyResp>();
        if (result.errno == 0)
        {
            var randsk = result.randsk;
            var sekey = HttpUtility.UrlDecode(randsk, Encoding.UTF8);
            return sekey;
        }
        else
        {
            var errormsg = result.errno switch
            {
                105 => "链接地址错误",
                -12 => "非会员用户达到转存文件数目上限",
                -9 => "pwd错误",
                2 => "参数错误,或者判断是否有referer",
                _ => $"GetSeKey时发生未知错误错误码：{result.errno}"
            };
            throw new Exception($"通过{surl}和{pwd}发生{errormsg}无法得到sekey");
        }

        return "";
    }

    private async Task<(long shareid, long uk, List<string> fsidList)?> GetTransFileInfo(string surl, string sekey)
    {
        var url = "https://pan.baidu.com/rest/2.0/xpan/share?method=list";
        NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

        queryString.Add("shorturl", surl);
        queryString.Add("page", "1");
        queryString.Add("num", "100");
        queryString.Add("fid", "0");
        queryString.Add("root", "1");
        queryString.Add("sekey", sekey);
        queryString.Add("access_token", await this.GetToken());
        url = url + "&" + queryString.ToString();

        var client = GetDefaultClient();
        var jstring = await client.GetStringAsync(url);
        var res = JsonNode.Parse(jstring);
        var errno = res["errno"].GetValue<int>();
        if (errno == 0)
        {
            long shareid = res["share_id"].GetValue<long>();
            long uk = res["uk"].GetValue<long>();
            List<string> fsidList = new List<string>();
            foreach (var fs in res["list"].AsArray())
            {
                fsidList.Add(fs["fs_id"].GetValue<string>());
            }

            return (shareid, uk, fsidList);
        }
        else
        {
            var error_msg = errno switch
            {
                110 => "有其他转存任务在进行",
                105 => "非会员用户达到转存文件数目上限",
                -7 => "达到高级会员转存上限",
                _ => $"未知错误{errno}",
            };
            throw new Exception($"通过{surl}和{sekey}获取文件数据时发生{error_msg}无法得到sekey");
        }
    }

    private async Task<int> FileTransfer(long shareid, long uk, string sekey, List<string> fsidlist, string path)
    {
        var url = "http://pan.baidu.com/rest/2.0/xpan/share?method=transfer";
        NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("shareid", shareid.ToString());
        queryString.Add("from", uk.ToString());
        queryString.Add("access_token", await this.GetToken());
        url = url + "&" + queryString.ToString();

        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("sekey", sekey),
            new KeyValuePair<string, string>("fsidlist", $"[{string.Join(",", fsidlist)}]"),
            new KeyValuePair<string, string>("path", path),
        });

        var client = GetDefaultClient();
        var jresp = await client.PostAsync(url, formContent);
        var jString = await jresp.Content.ReadAsStringAsync();
        var res = JsonNode.Parse(jString);

        int errno = res["errno"].GetValue<int>();
        if (errno == 0)
        {
            Console.WriteLine("文件转存成功");
        }
        else
        {
            var error_msg = errno switch
            {
                111 => "有其他转存任务在进行",
                120 => "非会员用户达到转存文件数目上限",
                130 => "达到高级会员转存上限",
                -33 => "达到转存文件数目上限",
                12 => "批量操作失败，可能文件已经存在在该目录中了",
                -3 => "转存文件不存在",
                -9 => "密码错误",
                5 => "分享文件夹等禁止文件",
                _ => "",
            };
            throw new Exception(error_msg);
        }

        return errno;
    }

    public async Task<int> TransFile(string url, string pwd)
    {
        var surl = await this.GetSUrl(url);
        var seKey = await this.GetSeKey(surl, pwd);
        var res = await this.GetTransFileInfo(surl, seKey);
        if (res.HasValue)
        {
            var v = res.Value;
            return await this.FileTransfer(v.shareid, v.uk, seKey, v.fsidList, this.config.DownloadPath);
        }

        return 999;
    }

    private HttpClient GetDefaultClient()
    {
        var client = new HttpClient();
        foreach (var VARIABLE in headers)
        {
            client.DefaultRequestHeaders.Add(VARIABLE.Key, VARIABLE.Value);
        }

        return client;
    }
}