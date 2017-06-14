using PetaPoco;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Data.Common;
using Broccoli.Core.Database.Eloquent;
using System.Text.RegularExpressions;
using Broccoli.Core.Facade;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Broccoli.Core.Database
{
    public class BroccoliDatabase : PetaPoco.Database, IBroccoliDatabase
    {
        // Custom variable for action here
        private DbConnection _broccoSharedConnection;

        public static IProvider BroccoProvider
        {
            get; protected set;
        }
        // Member variables
        public BroccoliDatabase(string connectionStringName) : base(connectionStringName)
        {
            BroccoProvider = Provider;
        }

        public PocoData GetPocoDataForType(Type t)
        {
            return PocoData.ForType(t, DefaultMapper);
        }

        public PocoData GetPocoDataForObject(object poco)
        {
            var pd = GetPocoDataForType(poco.GetType());
            return PocoData.ForObject(poco, pd.TableInfo.PrimaryKey, DefaultMapper);
        }

        public async void OpenSharedConnectionAsync()
        {
            if (_sharedConnectionDepth == 0)
            {
                _broccoSharedConnection = Factory.CreateConnection();
                _broccoSharedConnection.ConnectionString = ConnectionString;

                if (_broccoSharedConnection.State == ConnectionState.Broken)
                    _broccoSharedConnection.Close();

                if (_broccoSharedConnection.State == ConnectionState.Closed)
                    await (_broccoSharedConnection as DbConnection).OpenAsync();

                _broccoSharedConnection = (DbConnection)OnConnectionOpened(_broccoSharedConnection);

                if (KeepConnectionAlive)
                    _sharedConnectionDepth++; // Make sure you call Dispose
            }
            _sharedConnectionDepth++;
        }

        public void CloseBroccoSharedConnection()
        {
            if (_sharedConnectionDepth > 0)
            {
                _sharedConnectionDepth--;
                if (_sharedConnectionDepth == 0)
                {
                    OnConnectionClosing(_broccoSharedConnection);
                    _broccoSharedConnection.Dispose();
                    _broccoSharedConnection = null;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public DbCommand CreateBroccoCommand(DbConnection connection, string sql, params object[] args)
        {
            // Perform named argument replacements
            if (EnableNamedParams)
            {
                var new_args = new List<object>();
                sql = ParametersHelper.ProcessParams(sql, args, new_args);
                args = new_args.ToArray();
            }

            // Perform parameter prefix replacements
            if (_paramPrefix != "@")
                sql = rxParamsPrefix.Replace(sql, m => _paramPrefix + m.Value.Substring(1));
            sql = sql.Replace("@@", "@"); // <- double @@ escapes a single @

            // Create the command and add parameters
            DbCommand cmd = connection.CreateCommand();
            cmd.Connection = connection;
            cmd.CommandText = sql;
            cmd.Transaction = _transaction as DbTransaction;
            foreach (var item in args)
            {
                AddParam(cmd, item, null);
            }

            // Notify the DB type
            Provider.PreExecute(cmd);

            // Call logging
            if (!String.IsNullOrEmpty(sql))
                DoPreExecute(cmd);

            return cmd;
        }

        public async Task<IEnumerable<T>> BroccoQuery<T>(string sql, params object[] args) where T : Model<T>, new()
        {
            List<T> retList = new List<T>();
            OpenSharedConnectionAsync();
            try
            {
                using (var cmd = CreateBroccoCommand(_broccoSharedConnection, sql, args))
                {
                    DbDataReader r;
                    var pd = DbFacade.PocoDatas[typeof(T).Name];
                    try
                    {
                        r = await cmd.ExecuteReaderAsync();
                        OnExecutedCommand(cmd);
                        var factory = pd.GetFactory(cmd.CommandText, _broccoSharedConnection.ConnectionString, 0, r.FieldCount, r, DefaultMapper) as Func<IDataReader, T>;
                        using (r)
                        {
                            while (true)
                            {
                                T poco;
                                try
                                {
                                    if (!await r.ReadAsync())
                                        break;
                                    poco = factory(r);
                                }
                                catch (Exception x)
                                {
                                    if (OnException(x))
                                        throw;
                                    break;
                                }

                                retList.Add(poco);
                            }
                        }
                    }
                    catch (Exception x)
                    {
                        if (OnException(x))
                            throw;
                    }
                }
            }
            finally
            {
                CloseBroccoSharedConnection();
            }
            return retList;
        }

        public IDbDataAdapter CreateAdapter(IDbCommand cmd)
        {
            IDbDataAdapter adapter = Provider.GetFactory().CreateDataAdapter();
            adapter.SelectCommand = cmd;
            return adapter;
        }

        public DbCommand CreateBroccoCommand(IDbConnection connection, string sql, params object[] args)
        {
            return base.CreateCommand(connection, sql, sql) as DbCommand;
        }

    }

    internal class BoxingT<TModel> where TModel : Model<TModel>, new()
    {
        public static TModel GenerateObject(DataRow dr)
        {
            TModel model = new TModel();
            model.DataRow = dr;
            return model;
        }
    }

    internal static class BroccoAutoSelectHelper
    {
        private static Regex rxSelect = new Regex(@"\A\s*(SELECT|EXECUTE|CALL|WITH|SET|DECLARE)\s",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static Regex rxFrom = new Regex(@"\A\s*FROM\s",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);

        private static Dictionary<string, string> _autoSelectCache = new Dictionary<string, string>();
        private static object _selectCacheLock = new object();

        public static string AddSelectClause<T>(IProvider provider)
        {
            string sql = "";
            if (sql.StartsWith(";"))
                return sql.Substring(1);

            if (!rxSelect.IsMatch(sql))
            {
                string storedSql;

                var pd = DbFacade.PocoDatas[typeof(T).Name];
                var tableName = provider.EscapeTableName(pd.TableInfo.TableName);
                Monitor.Enter(_selectCacheLock);
                if (!_autoSelectCache.TryGetValue(tableName, out storedSql))
                {

                    string cols = pd.Columns.Count != 0
                   ? string.Join(", ", (from c in pd.QueryColumns select tableName + "." + provider.EscapeSqlIdentifier(c)).ToArray())
                   : "NULL";
                    if (!rxFrom.IsMatch(sql))
                        storedSql = string.Format("SELECT {0} FROM {1}", cols, tableName);
                    else
                        storedSql = string.Format("SELECT {0}", cols);

                    _autoSelectCache.Add(tableName, storedSql);
                }
                Monitor.Exit(_selectCacheLock);
                sql = string.Format("{0} {1}", storedSql, sql);
            }
            return sql;
        }
    }
}
