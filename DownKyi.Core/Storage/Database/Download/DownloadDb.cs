﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization.Formatters.Binary;
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
            // 定义一个流
            Stream stream = new MemoryStream();
            // 定义一个格式化器
            BinaryFormatter formatter = new BinaryFormatter();
            // 序列化
            formatter.Serialize(stream, obj);

            byte[] array = null;
            array = new byte[stream.Length];

            //将二进制流写入数组
            stream.Position = 0;
            stream.Read(array, 0, (int)stream.Length);

            //关闭流
            stream.Close();

            string sql = $"insert into {tableName}(id, data) values (@id, @data)";
            dbHelper.ExecuteNonQuery(sql, new Action<SqliteParameterCollection>((para) =>
            {
                para.Add("@id", SqliteType.Text).Value = uuid;
                para.Add("@data", SqliteType.Blob).Value = array;
            }));
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
            // 定义一个流
            Stream stream = new MemoryStream();
            // 定义一个格式化器
            BinaryFormatter formatter = new BinaryFormatter();
            // 序列化
            formatter.Serialize(stream, obj);

            byte[] array = null;
            array = new byte[stream.Length];

            //将二进制流写入数组
            stream.Position = 0;
            stream.Read(array, 0, (int)stream.Length);

            //关闭流
            stream.Close();

            string sql = $"update {tableName} set data=@data where id glob @id";
            dbHelper.ExecuteNonQuery(sql, new Action<SqliteParameterCollection>((para) =>
            {
                para.Add("@id",  SqliteType.Text).Value = uuid;
                para.Add("@data", SqliteType.Blob).Value = array;
            }));
        }
        catch (Exception e)
        {
            Console.PrintLine("Insert()发生异常: {0}", e);
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
    public object? QueryById(string uuid)
    {
        var sql = $"select * from {tableName} where id glob '{uuid}'";
        var query = Query(sql);

        if (query.ContainsKey(uuid))
        {
            query.TryGetValue(uuid, out var obj);
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
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Dictionary<string,object>))]
    private Dictionary<string, object> Query(string sql)
    {
        var objects = new Dictionary<string, object>();

        dbHelper.ExecuteQuery(sql, reader =>
        {
            while (reader.Read())
            {
                try
                {
                    // 读取字节数组
                    var array = (byte[])reader["data"];
                    // 定义一个流
                    var stream = new MemoryStream(array);
                    //定义一个格式化器
                    var formatter = new BinaryFormatter();
                    // 反序列化
#pragma warning disable SYSLIB0011
                    var obj = formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011

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
        var sql = $"create table if not exists {tableName} (id varchar(255) unique, data blob)";
        dbHelper.ExecuteNonQuery(sql);
    }
}