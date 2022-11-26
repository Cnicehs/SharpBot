using System.Data.SQLite;

namespace SharpBot.DB;

public class BaseDB : SingaltonInstance<BaseDB>
{
    public SQLiteConnection Connection { get; private set; }
    public SQLite.SQLiteConnection SqLiteConnection { get; set; }

    [RunningOnAwake]
    void Init()
    {
        string cs = @"URI=file:./config/meta.db";
        Connection = new SQLiteConnection(cs);
        Connection.Open();

        SqLiteConnection = new SQLite.SQLiteConnection("./config/meta.db");
    }

    public void CreateTable<T>()
    {
        SqLiteConnection.CreateTable<T>();
    }

    ~BaseDB()
    {
        Connection?.Close();
        SqLiteConnection?.Close();
    }
}