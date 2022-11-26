using System.Text.Json;

namespace SharpBot.DB;

public interface IDB
{
    string DBName { get; }
    List<object> GetAllRawData();
    object UpdateData(JsonElement data);
}