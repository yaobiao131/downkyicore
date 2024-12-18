using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using DownKyi.Core.Logging;
using Microsoft.Data.Sqlite;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.Storage.Database.Download;

public class DownloadDb
{
    private const string key = "bdb8eb69-3698-4af9-b722-9312d0fba623";
    protected string tableName = "download";

#if DEBUG
    private readonly DbHelper dbHelper = new DbHelper(StorageManager.GetDownload().Replace(".db", "_debug.db"));
#else
    private readonly DbHelper dbHelper = new DbHelper(StorageManager.GetDownload(), key);
#endif

    /// <summary>
    /// 关闭数据库连接
    /// </summary>
    public void Close()
    {
        dbHelper.Close();
    }

    /// <summary>
    /// 插入新的数据
    /// </summary>
    /// <param name="obj"></param>
    public void Insert(string uuid, object obj)
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(obj);

            string sql = $"insert into {tableName}(id, data) values (@id, @data)";
            dbHelper.ExecuteNonQuery(sql, (para) =>
            {
                para.AddWithValue("@id", uuid);
                para.AddWithValue("@data", jsonString);
            });
        }
        catch (Exception e)
        {
            Console.PrintLine("Insert()发生异常: {0}", e);
            LogManager.Error($"{tableName}", e);
        }
    }

    /// <summary>
    /// 删除uuid对应的数据
    /// </summary>
    /// <param name="uuid"></param>
    public void Delete(string uuid)
    {
        try
        {
            string sql = $"delete from {tableName} where id glob '{uuid}'";
            dbHelper.ExecuteNonQuery(sql);
        }
        catch (Exception e)
        {
            Console.PrintLine("Delete()发生异常: {0}", e);
            LogManager.Error($"{tableName}", e);
        }
    }

    public void Update(string uuid, object obj)
    {
        try
        {
            // 将对象序列化为JSON字符串
            string jsonString = JsonSerializer.Serialize(obj);

            string sql = $"update {tableName} set data=@data where id=@id";
            dbHelper.ExecuteNonQuery(sql, (para) =>
            {
                para.AddWithValue("@id", uuid);
                para.AddWithValue("@data", jsonString);
            });
        }
        catch (Exception e)
        {
            Console.PrintLine("Update()发生异常: {0}", e);
            LogManager.Error($"{tableName}", e);
        }
    }

    /// <summary>
    /// 查询所有数据
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public Dictionary<string, object> QueryAll()
    {
        string sql = $"select * from {tableName}";
        return Query(sql);
    }

    /// <summary>
    /// 查询uuid对应的数据
    /// </summary>
    /// <param name="uuid"></param>
    /// <returns></returns>
    public object QueryById(string uuid)
    {
        string sql = $"select * from {tableName} where id glob '{uuid}'";
        Dictionary<string, object> query = Query(sql);

        if (query.ContainsKey(uuid))
        {
            query.TryGetValue(uuid, out object obj);
            return obj;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 查询数据
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    private Dictionary<string, object> Query(string sql)
    {
        Dictionary<string, object> objects = new Dictionary<string, object>();

        dbHelper.ExecuteQuery(sql, reader =>
        {
            while (reader.Read())
            {
                try
                {
                    byte[] array = (byte[])reader["data"];
                    string jsonString = System.Text.Encoding.UTF8.GetString(array);
                    object obj = JsonSerializer.Deserialize<object>(jsonString);

                    objects.Add((string)reader["id"], obj);
                }
                catch (Exception e)
                {
                    Console.PrintLine("Query()发生异常: {0}", e);
                    LogManager.Error($"{tableName}", e);
                }
            }
        });

        return objects;
    }

    /// <summary>
    /// 如果表不存在则创建表
    /// </summary>
    protected void CreateTable()
    {
        string sql = $"create table if not exists {tableName} (id varchar(255) unique, data blob)";
        dbHelper.ExecuteNonQuery(sql);
    }
}