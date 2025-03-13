using System.Data;
using DownKyi.Core.Logging;
using Microsoft.Data.Sqlite;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.Storage.Database;

public class DbHelper
{
    private readonly string _connStr;
    private readonly SqliteConnection? _conn;

    private static readonly Dictionary<string, SqliteConnection> Database = new();

    /// <summary>
    /// 创建一个数据库
    /// </summary>
    /// <param name="dbPath"></param>
    public DbHelper(string dbPath)
    {
        _connStr = new SqliteConnectionStringBuilder
        {
            Mode = SqliteOpenMode.ReadWriteCreate,
            DataSource = dbPath
        }.ToString();
        if (Database.TryGetValue(_connStr, out var value))
        {
            _conn = value;

            if (_conn != null)
            {
                return;
            }
        }

        _conn = new SqliteConnection(_connStr);
        Database.Add(_connStr, _conn);
    }

    /// <summary>
    /// 创建一个带密码的数据库
    /// </summary>
    /// <param name="dbPath"></param>
    /// <param name="secretKey"></param>
    public DbHelper(string dbPath, string secretKey)
    {
        _connStr = new SqliteConnectionStringBuilder
        {
            Mode = SqliteOpenMode.ReadWriteCreate,
            Password = secretKey,
            DataSource = dbPath
        }.ToString();
        if (Database.TryGetValue(_connStr, out var value))
        {
            _conn = value;

            if (_conn != null)
            {
                return;
            }
        }

        _conn = new SqliteConnection(_connStr);
        // conn.SetPassword(secretKey);
        Database.Add(_connStr, _conn);
    }

    /// <summary>
    /// 连接是否开启
    /// </summary>
    /// <returns></returns>
    public bool IsOpen()
    {
        return _conn?.State == ConnectionState.Open;
    }

    /// <summary>
    /// 开启连接
    /// </summary>
    public void Open()
    {
        if (_conn == null)
        {
            return;
        }

        if (!IsOpen())
        {
            _conn.Open();
        }
    }

    /// <summary>
    /// 关闭数据库
    /// </summary>
    public void Close()
    {
        if (_conn == null)
        {
            return;
        }

        if (!IsOpen()) return;
        _conn.Close();

        Database.Remove(_connStr);
    }

    /// <summary>
    /// 执行一条SQL语句
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="action"></param>
    public void ExecuteNonQuery(string sql, Action<SqliteParameterCollection>? action = null)
    {
        if (_conn == null)
        {
            return;
        }

        try
        {
            lock (_conn)
            {
                Open();
                using var tr = _conn.BeginTransaction();
                using (var command = _conn.CreateCommand())
                {
                    command.CommandText = sql;
                    // 添加参数
                    action?.Invoke(command.Parameters);
                    command.ExecuteNonQuery();
                }

                tr.Commit();
            }
        }
        catch (SqliteException e)
        {
            Console.PrintLine("DbHelper ExecuteNonQuery()发生异常: {0}", e);
            LogManager.Error("DbHelper ExecuteNonQuery()", e);
        }
    }

    /// <summary>
    /// 执行一条SQL语句，并执行提供的操作，一般用于查询
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="action"></param>
    public void ExecuteQuery(string sql, Action<SqliteDataReader> action)
    {
        if (_conn == null)
        {
            return;
        }

        try
        {
            lock (_conn)
            {
                Open();
                using var command = _conn.CreateCommand();
                command.CommandText = sql;
                var reader = command.ExecuteReader();
                action(reader);
            }
        }
        catch (SqliteException e)
        {
            Console.PrintLine("DbHelper ExecuteQuery()发生异常: {0}", e);
            LogManager.Error("DbHelper ExecuteQuery()", e);
        }
    }
}