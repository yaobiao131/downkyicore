using DownKyi.Core.Logging;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using BinaryFormatDataStructure;
using System.Reflection;
using Avalonia.Controls;
using System.IO;
using System.Text.Json;
using Console = DownKyi.Core.Utils.Debugging.Console;

namespace DownKyi.Core.Storage.Database
{
    public class DataProcessor
    {
        private readonly SqliteConnectionStringBuilder _connString;
        public DataProcessor(string connString)
        {
            _connString = new SqliteConnectionStringBuilder(connString);
        }

        public SqliteConnection GetConnection(string connStr)
        {
            var conn = new SqliteConnection(connStr);
            conn.Open();
            return conn;
        }

        public void InitializeDatabase()
        {
            try
            {
                using (var connection = GetConnection(_connString.ConnectionString))
                {
                    connection.Open();
                    var tableExists = TableExists(connection, "DataProcessor");
                    if (tableExists)
                    {
                        return;
                    }
                }
                ProcessColumnData();

            }
            catch (Exception e)
            {
                Console.PrintLine("Processor", e);
                LogManager.Error("Processor", e);
            }
        }

        private void ProcessColumnData()
        {
            string databasePath = _connString.DataSource;
            if (File.Exists(databasePath))
            {
                var desDirectory = Path.Combine(Path.GetDirectoryName(databasePath), "backup");
                Directory.CreateDirectory(desDirectory);
                var desFileName = string.Concat(Path.GetFileNameWithoutExtension(databasePath),
                    DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"),".db");
                var desFilePath = Path.Combine(desDirectory, desFileName);
                File.Create(desFilePath).Close();
                File.Copy(_connString.DataSource, desFilePath, true);
            }

            using (var conn = GetConnection(_connString.ConnectionString))
            {
                var tables = GetAllTables(conn);
                foreach (var tableName in tables)
                {
                    ProcessTableWithBlob(conn, tableName);
                }
                CreatDataProcessorTable(conn);
            }

        }


        private Assembly _assembly = Assembly.LoadFrom("DownKyi");


        private void ProcessTableWithBlob(SqliteConnection sourceConnection, string tableName)
        {
            var sql = $"SELECT * FROM {tableName};";
            HashSet<int> blobColumns = new HashSet<int>();
            const string pragmaTableInfoSql = @"
            PRAGMA table_info({0});";

            ExecuteQuery(string.Format(pragmaTableInfoSql, tableName), sourceConnection, reader =>
            {
                while (reader.Read())
                {
                    string typeName = reader["type"].ToString().ToLower();
                    if (typeName.Contains("blob"))
                    {
                        int columnIndex = reader.GetInt32(0);
                        blobColumns.Add(columnIndex);
                    }
                }
            });

            if (blobColumns.Count == 0) return;
            string updateSql = $"UPDATE {tableName} SET data = @1 WHERE id = @0;";
            int primaryKeyIndex = 0;

            ExecuteQuery(sql, sourceConnection, reader =>
            {
                while (reader.Read())
                {
                    using (var command = sourceConnection.CreateCommand())
                    {
                        command.CommandText = updateSql;
                        var data = reader[1] as byte[];
                        if(data != null)
                        {
                            using var stream = new MemoryStream(data);

                            var bObj = (BinaryObject)NRBFReader.ReadStream(stream);
                            var targetType = _assembly.GetType(bObj.TypeName);
                            var instance = Activator.CreateInstance(targetType);

                            foreach (var prop in targetType.GetProperties())
                            {
                                try
                                {
                                    string fieldName = $"<{prop.Name}>k__BackingField";
                                    if (bObj.TryGetValue(fieldName, out var val))
                                    {
                                        prop.SetValue(instance, val);
                                    }
                                }
                                catch (Exception e)
                                {

                                    Console.PrintLine("ProcessTableWithBlob Properties Set发生异常: {0}", e);
                                    LogManager.Error("ProcessTableWithBlob Properties Set()", e);
                                }
                            }

                            var json = JsonSerializer.Serialize(instance);
                            command.Parameters.AddWithValue($"@{1}", json);

                            command.Parameters.AddWithValue($"@{primaryKeyIndex}", reader[primaryKeyIndex]);

                            command.ExecuteNonQuery();
                        }
                     
                    }
                }
            });
        }




        public void ExecuteQuery(string sql, SqliteConnection conn, Action<SqliteDataReader> action)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new ArgumentException("SQL query cannot be null or whitespace.", nameof(sql));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action), "Action cannot be null.");
            }
            using (var command = conn.CreateCommand())
            {
                command.CommandText = sql;
                var reader = command.ExecuteReader();
                action(reader);
            }
        }


        private List<string> GetAllTables(SqliteConnection connection)
        {
            const string getAllTablesSql = @"
            SELECT name 
            FROM sqlite_master 
            WHERE type='table';";

            List<string> tables = new List<string>();

            ExecuteQuery(getAllTablesSql, connection, reader =>
            {
                while (reader.Read())
                {
                    tables.Add(reader.GetString(0));
                }
            });

            return tables;
        }



        private bool TableExists(SqliteConnection connection, string tableName)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";
            var result = command.ExecuteScalar();
            return result != null;
        }

        private void CreatDataProcessorTable(SqliteConnection connection)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS DataProcessor (
            MigrationId NVARCHAR(150) NOT NULL PRIMARY KEY,
            AppliedOn DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
            Description TEXT);";
            command.ExecuteNonQuery();
        }
    }
}
