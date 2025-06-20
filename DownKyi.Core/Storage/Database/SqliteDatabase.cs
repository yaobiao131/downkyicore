using System.Data;
using DownKyi.Core.Logging;
using Microsoft.Data.Sqlite;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.Storage.Database;

/// <summary>
/// SQLite 数据库操作助手，支持加密数据库操作
/// </summary>
public sealed class SqliteDatabase : IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _connection;

    /// <summary>
    /// 创建或连接一个SQLite数据库
    /// </summary>
    /// <param name="dbPath">数据库文件路径</param>
    public SqliteDatabase(string dbPath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            Mode = SqliteOpenMode.ReadWriteCreate,
            DataSource = dbPath,
            Pooling = false
        }.ToString();
    }

    /// <summary>
    /// 创建或连接一个加密的SQLite数据库
    /// </summary>
    /// <param name="dbPath">数据库文件路径</param>
    /// <param name="secretKey">加密密钥</param>
    public SqliteDatabase(string dbPath, string secretKey)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            Mode = SqliteOpenMode.ReadWriteCreate,
            Password = secretKey,
            DataSource = dbPath
        }.ToString();
    }
    
    
    /// <summary>
    /// 执行非查询SQL语句
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="parametersAction">参数设置委托</param>
    public void ExecuteNonQuery(string sql, Action<SqliteParameterCollection>? parametersAction = null)
    {
        try
        {
            using var connection = CreateConnection();
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            
            command.CommandText = sql;
            command.Transaction = transaction;
            parametersAction?.Invoke(command.Parameters);
            
            command.ExecuteNonQuery();
            transaction.Commit();
        }
        catch (SqliteException ex)
        {
            Console.PrintLine("ExecuteNonQuery() 发生异常: {0}", ex);
            LogManager.Error("SqliteDatabase.ExecuteNonQuery()", ex);
            throw;
        }
    }

    /// <summary>
    /// 执行查询SQL语句
    /// </summary>
    /// <param name="sql">SQL语句</param>
    /// <param name="readAction">读取数据的委托</param>
    public void ExecuteQuery(string sql, Action<SqliteDataReader> readAction)
    {
        try
        {
            using var connection = CreateConnection();
            using var command = connection.CreateCommand();
            
            command.CommandText = sql;
            using var reader = command.ExecuteReader();
            
            readAction(reader);
        }
        catch (SqliteException ex)
        {
            Console.PrintLine("ExecuteQuery() 发生异常: {0}", ex);
            LogManager.Error("SqliteDatabase.ExecuteQuery()", ex);
            throw;
        }
    }

    /// <summary>
    /// 创建并打开一个新的数据库连接
    /// </summary>
    private SqliteConnection CreateConnection()
    {
        _connection = new SqliteConnection(_connectionString);
        _connection.Open();
        return _connection;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
        _connection = null;
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        try 
        {
            SqliteConnection.ClearAllPools();
        }
        catch {  }
    }
}