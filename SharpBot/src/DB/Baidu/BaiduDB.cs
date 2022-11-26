using System.Text.Json;
using Dapper;
using SharpBot.IOC;
using SQLite;

namespace SharpBot.DB.Baidu;

[Table("baidu")]
public class BaiduProto
{
    [PrimaryKey] public string id { get; set; }
    public string refresh_token { get; set; }
    [NotNull] public string access_token { get; set; }
    [NotNull] public DateTime last_update { get; set; }
    public string next_token { get; set; }
}

[Singlonton]
public class BaiduDB : IDB
{
    public string DBName => "baidu";

    public BaiduDB()
    {
        BaseDB.Instance.CreateTable<BaiduProto>();
    }

    public void AddNew(BaiduProto from, BaiduProto to)
    {
        if (from == null)
        {
            this.InsertOrReplace(to);
            return;
        }

        from.next_token = to.access_token;
        this.InsertOrReplace(from);
        this.InsertOrReplace(to);
    }

    public bool InsertOrReplace(BaiduProto proto)
    {
        var result = BaseDB.Instance.SqLiteConnection.InsertOrReplace(proto, typeof(BaiduProto));
        if (result > 0)
        {
            return true;
        }

        return false;
    }

    public BaiduProto GetProto(string id)
    {
        try
        {
            var record = BaseDB.Instance.SqLiteConnection.Get<BaiduProto>(id);
            return record;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public List<object> GetAllRawData()
    {
        var select = @"SELECT * FROM baidu";
        return BaseDB.Instance.Connection.Query<BaiduProto>(select).Select(x => x as object).ToList();
    }

    public object UpdateData(JsonElement data)
    {
        var result =
            BaseDB.Instance.SqLiteConnection.Update(JsonSerializer.Deserialize<BaiduProto>(data), typeof(BaiduProto));
        if (result > 0)
        {
            return data;
        }

        return data;
    }
}