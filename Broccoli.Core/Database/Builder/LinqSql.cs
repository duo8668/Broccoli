using Broccoli.Core.Database.Eloquent;
using Broccoli.Core.Database.Utils.Converters;
using Broccoli.Core.Facade;
using Broccoli.Core.Extensions;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Builder
{

    public class LinqSql<TModel> : IDisposable where TModel : Model<TModel>, new()
    {
        private string _sql = "";
        private List<object> _args;

        private static string prefix = "a";
        private int paramCount = 0;

        private static Regex rxSelectStatement = new Regex(@"select(?<selectCols>[a-zA-Z0-9*\s]+)from(?<tbl>[a-zA-Z0-9*_\s]+)", RegexOptions.Compiled);

        protected static readonly char[] selectSpliter = ",".ToCharArray();

        public static Dictionary<string, string> QueryCache
        {
            get; protected set;
        }

        private Queue<SqlWhereHelper<TModel>> _whereCondition = new Queue<SqlWhereHelper<TModel>>();
        private Dictionary<string, string> _selectCols = new Dictionary<string, string>();
        private Dictionary<string, SqlJoinHelper<TModel>> _joinClause;
        private Dictionary<string, object> _updateClause;
        private PredicateConverter _predicateConverter;

        string _tableName, _exclusiveFromTable, _modelName;

        public string SQL
        {
            get
            {
                Build();
                return _sql;
            }
        }

        public object[] Arguments
        {
            get
            {
                return _args.ToArray();
            }
        }

        public LinqSql() : this(typeof(TModel).Name)
        {
            _args = new List<object>();
            _predicateConverter = new PredicateConverter();
        }

        public LinqSql(string modelName)
        {
            _tableName = DbFacade.TableNames[modelName];
            _modelName = modelName;
        }

        #region SQL Clause

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columns">Columns to choose</param>
        /// <returns></returns>
        public LinqSql<TModel> Select(string[] columns)
        {
            foreach (var col in columns)
            {
                _selectCols.Add(col, col);
            }

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="csvCols">CSV pattern separated columns to be used</param>
        /// <returns>An instance of this</returns>
        public LinqSql<TModel> Select(string csvCols)
        {
            return Select(csvCols.Split(selectSpliter));
        }

        public LinqSql<TModel> From(string thisTableName)
        {
            _exclusiveFromTable = thisTableName;
            return this;
        }

        public SqlJoinHelper<TModel> Join<T>() where T : Model<T>, new()
        {
            return Join(DbFacade.TableNames[typeof(T).Name]);
        }

        public SqlJoinHelper<TModel> Join(string targetTable)
        {
            if (_joinClause == null)
            {
                _joinClause = new Dictionary<string, SqlJoinHelper<TModel>>();
            }

            return new SqlJoinHelper<TModel>(this, _tableName, targetTable);
        }

        public LinqSql<TModel> On(SqlJoinHelper<TModel> _on)
        {
            _joinClause.Add(_on.TargetTable, _on);
            return this;
        }

        public Builder.LinqSql<TModel> Where(string sql, params object[] args)
        {
            if (!string.IsNullOrEmpty(sql))
            {
                _whereCondition.Enqueue(new SqlWhereHelper<TModel>(sql, args));
            }

            return this;
        }

        public Builder.LinqSql<TModel> Where(Expression<Func<TModel, bool>> predicate, params object[] args)
        {
            if (predicate != null)
            {
                _predicateConverter.Visit(predicate.Body);
                return Where(_predicateConverter.Sql, _predicateConverter.Parameters);
            }
            else
            {
                return this;
            }
        }

        public LinqSql<TModel> Update()
        {
            _updateClause = new Dictionary<string, object>();
            return this;
        }

        public LinqSql<TModel> Set(string sql, dynamic param)
        {
            _updateClause.Add(sql, param);
            return this;
        }

        #endregion

        private void Build()
        {
            _sql = "";
            var new_args = new List<object>();
            BuildSelect();
            BuildUpdate();
            BuildJoin();
            BuildWhere();
            Dispose();
        }

        private void BuildSelect()
        {
            if (_selectCols.Count() > 0)
            {
                string cols = DbFacade.PocoDatas[_modelName].Columns.Count != 0
               ? string.Join(", ", (from c in DbFacade.PocoDatas[_modelName].QueryColumns select _tableName + "." + c).ToArray())
               : "NULL";

                _sql = "SELECT " + cols + " FROM " + (string.IsNullOrEmpty(_exclusiveFromTable) ? _tableName : _exclusiveFromTable);
            }
        }

        private void BuildUpdate()
        {
            if (_updateClause != null)
            {
                string setQuery = "UPDATE " + _tableName + " SET ";
                foreach (var iii in _updateClause.Keys)
                {
                    setQuery += iii + "=@" + paramCount + ", ";
                    paramCount++;
                }

                setQuery = setQuery.Trim().TrimEnd(",".ToCharArray()) + " ";

                _args.AddRange(_updateClause.Values.ToList());
                _sql += setQuery;
            }
        }

        private void BuildJoin()
        {
            if (_joinClause != null)
            {
                if (_joinClause.Count() > 0)
                {
                    string onQuery = "";
                    foreach (var iii in _joinClause.Values)
                    {
                        onQuery += iii.ToString() + " ";
                    }
                    _sql += onQuery;

                }
            }
        }

        private void BuildWhere()
        {
            string whereQuery = "";
            var copyCond = new Queue<SqlWhereHelper<TModel>>(_whereCondition);
          
            if (copyCond.Count() > 0)
            {
                if (!_sql.ToLower().Contains("where"))
                {
                    whereQuery += " WHERE ";
                }
            }
            while (copyCond.Count() > 0)
            {
                var theWhere = copyCond.Dequeue();
                _args.AddRange(theWhere.Arguments);
                whereQuery += theWhere.SQLCondition + " ";
                if (copyCond.Count() > 0)
                {
                    whereQuery += "AND ";
                }
            }

            _sql += whereQuery;
        }

        public void Dispose()
        {
            _whereCondition.Clear();
            _selectCols.Clear();
            //_joinClause.Clear(); 
        }

        #region Additional LINQ handling

        public Builder.LinqSql<TModel> WhereIn(string columnName, List<object> values, params object[] args)
        {
            return this;
        }

        #endregion
    }

    public class SqlJoinHelper<TModel> : IDisposable where TModel : Model<TModel>, new()
    {
        private readonly LinqSql<TModel> _sql;
        public string MyTable { get; private set; }
        public string TargetTable { get; private set; }
        public string MyKey { get; private set; }
        public string HerKey { get; private set; }
        public string AbsoluteCondition { get; private set; }
        public string AdditionalCondition { get; private set; }

        public Func<TableInfo, string> fnMyKey;
        public Func<TableInfo, string> fnHerKey;

        private Queue<SqlWhereHelper<TModel>> _whereCondition = new Queue<SqlWhereHelper<TModel>>();

        public SqlJoinHelper(LinqSql<TModel> sql, string myTable, string targetTable)
        {
            _sql = sql;
            MyTable = myTable;
            TargetTable = targetTable;
        }

        public LinqSql<TModel> Join(string targetTable)
        {
            return _sql;
        }

        public LinqSql<TModel> On(Expression<Func<TModel, string>> myOwnKey = null, Expression<Func<TModel, string>> herOwnKey = null, Expression<Func<TModel, bool>> predicate = null, params object[] args)
        {
            return _sql.On(this);
        }

        public LinqSql<TModel> On(Expression<Func<TModel, bool>> predicate = null, params object[] args)
        {
            if (predicate != null)
            {
                using (var prc = new PredicateConverter())
                {
                    prc.Visit(predicate.Body);
                    return On(prc.Sql, prc.Parameters);
                }
            }
            else
            {
                return _sql.On(this);
            }
        }

        public LinqSql<TModel> On(string myKey = "", string herKey = "", string additionalCondition = "", params object[] args)
        {
            MyKey = myKey;
            HerKey = herKey;
            AdditionalCondition = additionalCondition;
            return _sql.On(this);
        }

        public LinqSql<TModel> On(string condition = "", params object[] args)
        {
            AbsoluteCondition = condition;
            return _sql.On(this);
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(AbsoluteCondition))
            {
                AbsoluteCondition = MyKey + "=" + HerKey + (string.IsNullOrEmpty(AdditionalCondition) ? "" : " AND " + AdditionalCondition);
            }

            return " JOIN " + TargetTable + " ON " + AbsoluteCondition + " ";
        }

        public void Dispose()
        {
            _whereCondition.Clear();
            _sql.Dispose();
        }
    }

    public class SqlWhereHelper<TModel> : IDisposable where TModel : Model<TModel>, new()
    {
        private readonly string _sql;
        public object[] Arguments
        {
            get; protected set;
        }

        public string SQLCondition
        {
            get
            {
                return _sql;
            }
        }

        public SqlWhereHelper()
        {

        }

        public SqlWhereHelper(string sql, params object[] args)
        {
            _sql = sql;
            Arguments = args;
        }

        public PredicateConverter VisitWhereCondition(Expression<Func<TModel, bool>> predicate, params object[] args)
        {
            if (predicate != null)
            {
                var _predicateConverter = new PredicateConverter();
                _predicateConverter.Visit(predicate.Body);
                return _predicateConverter;
            }
            else
            {
                return null;
            }
        }

        public void Dispose()
        {
            Arguments = null;
        }
    }

    public class SqlUpdateHelper<TModel> : IDisposable where TModel : Model<TModel>, new()
    {
        private readonly LinqSql<TModel> _sql;
        public string MyTable { get; private set; }
        public string AbsoluteCondition { get; private set; }

        public SqlUpdateHelper(LinqSql<TModel> sql, string myTable)
        {
            _sql = sql;
            MyTable = myTable;
        }

        public LinqSql<TModel> Set(Expression<Func<TModel, bool>> predicate = null, params object[] args)
        {
            if (predicate != null)
            {
                using (var prc = new UpdatePredicateConverter())
                {
                    prc.Visit(predicate.Body);
                    return Set(prc.Sql, prc.Parameters);
                }
            }
            return _sql;
        }

        public LinqSql<TModel> Set(string condition = "", params object[] args)
        {
            AbsoluteCondition = condition;
            return null;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
