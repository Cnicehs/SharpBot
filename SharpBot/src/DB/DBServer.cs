using SharpBot.IOC;
using Dapper;
using System.Data.SQLite;
using System.Text.Json;

namespace SharpBot.DB;

[Singlonton]
public class DBServer
{
    private Dictionary<string, IDB> dbMap;

    public DBServer(IEnumerable<IDB> dbs)
    {
        dbMap = dbs.ToDictionary(x => x.DBName);
    }

    public List<object> GetDBData(string db)
    {
        return this.GetDB(db)?.GetAllRawData();
    }

    public object UpdateDBData(string db, JsonElement jsonObj)
    {
        return this.GetDB(db)?.UpdateData(jsonObj);
    }

    private IDB GetDB(string db)
    {
        if (!dbMap.ContainsKey(db))
        {
            return null;
        }

        return dbMap[db];
    }
}