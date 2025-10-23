using Dapper;
using RiskAdjustment.Data.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Design.PluralizationServices;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RiskAdjustment.Data
{
    public enum DbAction
    {
        Insert,
        Update,
        Read,
        Delete
    }
    public enum DbType
    {
        HCC,
        PVP
    }
    public class DbActionEventArgs : EventArgs
    {
        public DbAction Action { get; set; }
        public Type EntityType { get; set; }
        public int RecordId { get; set; }
    }
    public class DbQuery
    {
        private ConnectionString _connectionString;
        public string AccessString;

        public EventHandler<DbQueryEventArgs> ActionExecuted;

        //No parameter = HCC database
        public DbQuery()
        {
            this._connectionString = new ConnectionString(DbType.HCC);
            AccessString = _connectionString.Value.ToString();
            
        }

        //this is wired in this way as a bit of a hack.  Initially, it was anticipated HCC and PVP tables would be in the same Db, however
        //that didn't happen and PVP is in a different DB, wiring it this way is the easiest way to call to the PVP database without having to rewrite a large portion
        //of HCC logic.

        public DbQuery(DbType dbType)
        {
            this._connectionString = new ConnectionString(DbType.PVP);
        }

        //Sychronous commands

        public List<T> Execute<T>(string query)
        {
            using (SqlConnection connection = new SqlConnection(this._connectionString.Value))
            {
                return connection.Query<T>(query).ToList();
            }
        }

        public List<T> Execute<T>(string query, string connectionString)
        {
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                return connection.Query<T>(query).ToList();
            }
        }

        public T ExecuteSingle<T>(string query)
        {
            using (SqlConnection connection = new SqlConnection(this._connectionString.Value))
            {
                return (T)connection.Query<T>(query).SingleOrDefault();
            }
        }

        public int ExecuteUpdate<T>(string query)
        {
            int recordId = ExecuteScalar(query);
            ActionExecuted?.Invoke(this, new DbQueryEventArgs()
            {
                RecordId = recordId
            });
            return recordId;
        }

        public int ExecuteInsert<T>(string query)
        {
            int recordId = ExecuteScalar(query);
            ActionExecuted?.Invoke(this, new DbQueryEventArgs()
            {
                RecordId = recordId
            });
            //AuditEntity<T>(query, DbAction.Insert, recordId);
            return recordId;
        }

        public int ExecuteCommand(string cmd)
        {
            using (SqlConnection connection = new SqlConnection(this._connectionString.Value))
            {
                return Convert.ToInt32(connection.Execute(cmd));
            }
        }

        public int ExecuteScalar(string cmd)
        {
            string amendedCmd = $"{cmd} SELECT CAST(SCOPE_IDENTITY() AS INT)";
            using (SqlConnection connection = new SqlConnection(this._connectionString.Value))
            {
                int entityId = Convert.ToInt32(connection.ExecuteScalar(amendedCmd));
                return entityId;
            }
        }

        //Asynchronous commands

        public async Task<List<T>> ExecuteAsync<T>(string query)
        {
                Task<List<T>> resultTask = Task.Run(() =>
                {
                    List<T> result = new List<T>();
                    using (SqlConnection connection = new SqlConnection(this._connectionString.Value))
                    {
                        result = connection.QueryAsync<T>(query).Result.ToList();
                    }

                    return result;
                });

                await Task.WhenAll(resultTask);
                return resultTask.Result;
        }

        //Bring your own connection string.  Basically to query other databases for stuff like the ICD-10 reference table.
        public async Task<List<T>> ExecuteAsync<T>(string query, string connectionString)
        {
            Task<List<T>> resultTask = Task.Run(() =>
            {
                List<T> result = new List<T>();
                using(SqlConnection connection = new SqlConnection(connectionString))
                {
                    result = connection.QueryAsync<T>(query).Result.ToList();
                }
                return result;
            });

            await Task.WhenAll(resultTask);
            return resultTask.Result;
        }

        public async Task<T> ExecuteSingleAsync<T>(string query)
        {
            Task<T> resultTask = Task.Run(() =>
            {
                using (SqlConnection connection = new SqlConnection(this._connectionString.Value))
                {
                    return connection.QueryAsync<T>(query).Result.SingleOrDefault();
                }
            });

            await Task.WhenAll(resultTask);
            return resultTask.Result;
        }

        public async void ExecuteCommandAsync(string cmd)
        {
            Task<int> t = Task.Run(() =>
            {
                using (SqlConnection connection = new SqlConnection(this._connectionString.Value))
                {
                    ActionExecuted?.Invoke(this, new DbQueryEventArgs());
                    return connection.QueryAsync<int>(cmd).Result.SingleOrDefault();
                }
            });
            await Task.WhenAll(t);
        }

        public async Task<int> ExecuteScalarAsync(string cmd, object entity)
        {
            Task<int> resultTask = Task.Run(async () =>
            {
                using(SqlConnection connection = new SqlConnection(this._connectionString.Value))
                {
                    return await connection.ExecuteScalarAsync<int>(cmd, entity);
                }
            });
            await Task.WhenAll(resultTask);
            ActionExecuted?.Invoke(this, new DbQueryEventArgs());
            return resultTask.Result;
        }

        public async Task<int> ExecuteScalarAsync<T>(string cmd)
        {
            Task<int> resultTask = Task.Run(async () =>
            {
                using (SqlConnection connection = new SqlConnection(this._connectionString.Value))
                {
                    return await connection.ExecuteScalarAsync<int>(cmd);
                }
            });
            await Task.FromResult(resultTask);
            ActionExecuted?.Invoke(this, new DbQueryEventArgs());
            return resultTask.Result;
        }

        //TODO:  remove these two methods.  They are for testing only.
        public int ExecuteScalar(string cmd, object entity)
        {
            cmd = cmd.Replace("\r\n", " ");
            using(SqlConnection connection = new SqlConnection(this._connectionString.Value))
            {
                return connection.ExecuteScalar<int>(cmd, entity);
            }
        }

        public int Execute(string cmd, object entity)
        {
            cmd = cmd.Replace("\r\n", " ");
            using(SqlConnection connection = new SqlConnection(this._connectionString.Value))
            {
                return connection.Execute(cmd, entity);
            }
        }
    }
}
