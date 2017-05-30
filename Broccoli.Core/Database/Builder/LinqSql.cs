using Broccoli.Core.Database.Eloquent;
using Broccoli.Core.Database.Utils.Converters;
using Broccoli.Core.Facade;
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

    public class LinqSql<TModel> where TModel : Model<TModel>, IDisposable, new()
    {
        private object[] _args;
        private object[] _argsFinal;
        private LinqSql<TModel> _theSql;

        private string _sql;
        private string _sqlFinal;

        private static string prefix = "a";
        private int count = 0;

        private Queue<string> _whereCondition = new Queue<string>();
        private Dictionary<string, string> _selectCols = new Dictionary<string, string>();
        private Dictionary<string, SqlJoinHelper<TModel>> _joinClause = new Dictionary<string, SqlJoinHelper<TModel>>();

        string _tableName;
        string _modelName;

        public string SQL
        {
            get
            {
                Build();
                return _sqlFinal;
            }
        }

        public LinqSql()
        {
            _theSql = new LinqSql<TModel>();
        }

        public LinqSql(string tableName)
        {
            _tableName = tableName;
            _modelName = DbFacade.TableToModelNames[_tableName];
        }

        #region SQL Clause

        public LinqSql<TModel> Select(string[] columns)
        {
            return this;
        }
        public LinqSql<TModel> Join(string targetTable)
        {
            return this;
        }

        public LinqSql<TModel> On(SqlJoinHelper<TModel> _on)
        {
            _joinClause.Add(_on.TargetTable, _on);
            return this;
        }

        public Builder.LinqSql<TModel> Where(string sql, params object[] args)
        {
            return this;
        }
        #endregion


        private void Build()
        {
            _sqlFinal = null;
        }

        public void Dispose()
        {
            _whereCondition.Clear();
            _selectCols.Clear();
            _joinClause.Clear();
            _theSql = null;
        }

        #region Additional LINQ handling

        public LinqSql<TModel> FilterTrashed(bool withTrashed = false)
        {
            if (withTrashed)
            {
                return _theSql;
            }
            else
            {
                return _theSql.Where(e => e.DeletedAt == null);
            }
        }

        public Builder.LinqSql<TModel> WhereIn(string columnName, List<object> values, params object[] args)
        {
            return this;
        }

        public Builder.LinqSql<TModel> Where(Expression<Func<TModel, bool>> predicate, params object[] args)
        {
            if (predicate != null)
            {
                var new_args = new List<object>();
                var converter = new PredicateConverter();
                converter.Visit(predicate.Body);
                var hold = PetaPoco.ParametersHelper.ProcessParams(converter.Sql, converter.Parameters, new_args);
                _theSql = _theSql.Where(hold, new_args.ToArray());

                new_args = null; converter = null; hold = null;
            }

            return this;
        }

        #endregion
    }

    public class SqlJoinHelper<TModel> where TModel : Model<TModel>, IDisposable, new()
    {
        private readonly LinqSql<TModel> _sql;
        public string TargetTable { get; private set; }
        public string TurnCode;

        public SqlJoinHelper(LinqSql<TModel> sql, string targetTable)
        {
            _sql = sql;
            TargetTable = targetTable;
        }

        public LinqSql<TModel> On(string myKey = "", string herKey = "", string additionalCondition = "", params object[] args)
        {
            return _sql.On(this);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public void Dispose()
        {
            _sql.Dispose();
        }
    }

    public static class ParametersHelper
    {
        private static Regex rxParams = new Regex(@"(?<!@)@\w+", RegexOptions.Compiled);
        // Helper to handle named parameters from object properties
        public static string ProcessParams(string sql, object[] args_src, List<object> args_dest)
        {
            string ret = rxParams.Replace(sql, m =>
            {
                string param = m.Value.Substring(1);

                object arg_val;

                int paramIndex;
                if (int.TryParse(param, out paramIndex))
                {
                    // Numbered parameter
                    if (paramIndex < 0 || paramIndex >= args_src.Length)
                        throw new ArgumentOutOfRangeException(string.Format("Parameter '@{0}' specified but only {1} parameters supplied (in `{2}`)", paramIndex,
                            args_src.Length, sql));
                    arg_val = args_src[paramIndex];
                }
                else
                {
                    // Look for a property on one of the arguments with this name
                    bool found = false;
                    arg_val = null;
                    foreach (var o in args_src)
                    {
                        var pi = o.GetType().GetProperty(param);
                        if (pi != null)
                        {
                            arg_val = pi.GetValue(o, null);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        throw new ArgumentException(
                            string.Format("Parameter '@{0}' specified but none of the passed arguments have a property with this name (in '{1}')", param, sql));
                }

                // Expand collections to parameter lists
                if ((arg_val as System.Collections.IEnumerable) != null &&
                    (arg_val as string) == null &&
                    (arg_val as byte[]) == null)
                {
                    var sb = new StringBuilder();
                    foreach (var i in arg_val as System.Collections.IEnumerable)
                    {
                        sb.Append((sb.Length == 0 ? "@" : ",@") + args_dest.Count.ToString());
                        args_dest.Add(i);
                    }
                    return sb.ToString();
                }
                else
                {
                    args_dest.Add(arg_val);
                    return "@" + (args_dest.Count - 1).ToString();
                }
            });

            return ret;
        }
    }


    /*
    /// <summary>
    ///     A simple helper class for build SQL statements
    /// </summary>
    public class SqlTwo
    {
        private object[] _args;
        private object[] _argsFinal;
        private SqlTwo _rhs;

        private string _sql;
        private string _sqlFinal;

        /// <summary>
        ///     Instantiate a new SQL Builder object.  Weirdly implemented as a property but makes
        ///     for more elegantly readable fluent style construction of SQL Statements
        ///     eg: db.Query(Sql.Builder.Append(....))
        /// </summary>
        public static SqlTwo Builder
        {
            get { return new SqlTwo(); }
        }

        /// <summary>
        ///     Returns the final SQL statement represented by this builder
        /// </summary>
        public string SQL
        {
            get
            {
                Build();
                return _sqlFinal;
            }
        }

        /// <summary>
        ///     Gets the complete, final set of arguments collected by this builder.
        /// </summary>
        public object[] Arguments
        {
            get
            {
                Build();
                return _argsFinal;
            }
        }

        /// <summary>
        ///     Default, empty constructor
        /// </summary>
        public SqlTwo()
        {
        }

        /// <summary>
        ///     Construct an SQL statement with the supplied SQL and arguments
        /// </summary>
        /// <param name="sql">The SQL statement or fragment</param>
        /// <param name="args">Arguments to any parameters embedded in the SQL</param>
        public SqlTwo(string sql, params object[] args)
        {
            _sql = sql;
            _args = args;
        }

        private void Build()
        {
            // already built?
            if (_sqlFinal != null)
                return;

            // Build it
            var sb = new StringBuilder();
            var args = new List<object>();
            Build(sb, args, null);
            _sqlFinal = sb.ToString();
            _argsFinal = args.ToArray();
        }

        /// <summary>
        ///     Append another SQL builder instance to the right-hand-side of this SQL builder
        /// </summary>
        /// <param name="sql">A reference to another SQL builder instance</param>
        /// <returns>A reference to this builder, allowing for fluent style concatenation</returns>
        public SqlTwo Append(SqlTwo sql)
        {
            if (_rhs != null)
                _rhs.Append(sql);
            else
                _rhs = sql;

            _sqlFinal = null;
            return this;
        }

        /// <summary>
        ///     Append an SQL fragment to the right-hand-side of this SQL builder
        /// </summary>
        /// <param name="sql">The SQL statement or fragment</param>
        /// <param name="args">Arguments to any parameters embedded in the SQL</param>
        /// <returns>A reference to this builder, allowing for fluent style concatenation</returns>
        public SqlTwo Append(string sql, params object[] args)
        {
            return Append(new SqlTwo(sql, args));
        }

        private static bool Is(SqlTwo sql, string sqltype)
        {
            return sql != null && sql._sql != null && sql._sql.StartsWith(sqltype, StringComparison.InvariantCultureIgnoreCase);
        }

        private void Build(StringBuilder sb, List<object> args, SqlTwo lhs)
        {
            if (!string.IsNullOrEmpty(_sql))
            {
                // Add SQL to the string
                if (sb.Length > 0)
                {
                    sb.Append("\n");
                }

                var sql = ParametersHelper.ProcessParams(_sql, _args, args);

                if (Is(lhs, "WHERE ") && Is(this, "WHERE "))
                    sql = "AND " + sql.Substring(6);
                if (Is(lhs, "ORDER BY ") && Is(this, "ORDER BY "))
                    sql = ", " + sql.Substring(9);
                // add set clause
                if (Is(lhs, "SET ") && Is(this, "SET "))
                    sql = ", " + sql.Substring(4);

                sb.Append(sql);
            }

            // Now do rhs
            if (_rhs != null)
                _rhs.Build(sb, args, this);
        }

        /// <summary>
        ///     Appends an SQL SET clause to this SQL builder
        /// </summary>
        /// <param name="sql">The SET clause like "{field} = {value}"</param>
        /// <param name="args">Arguments to any parameters embedded in the supplied SQL</param>
        /// <returns>A reference to this builder, allowing for fluent style concatenation</returns>
        public SqlTwo Set(string sql, params object[] args)
        {
            return Append(new Sql("SET " + sql, args));
        }

        /// <summary>
        ///     Appends an SQL WHERE clause to this SQL builder
        /// </summary>
        /// <param name="sql">The condition of the WHERE clause</param>
        /// <param name="args">Arguments to any parameters embedded in the supplied SQL</param>
        /// <returns>A reference to this builder, allowing for fluent style concatenation</returns>
        public SqlTwo Where(string sql, params object[] args)
        {
            return Append(new SqlTwo("WHERE (" + sql + ")", args));
        }

        /// <summary>
        ///     Appends an SQL ORDER BY clause to this SQL builder
        /// </summary>
        /// <param name="columns">A collection of SQL column names to order by</param>
        /// <returns>A reference to this builder, allowing for fluent style concatenation</returns>
        public SqlTwo OrderBy(params object[] columns)
        {
            return Append(new SqlTwo("ORDER BY " + string.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        /// <summary>
        ///     Appends an SQL SELECT clause to this SQL builder
        /// </summary>
        /// <param name="columns">A collection of SQL column names to select</param>
        /// <returns>A reference to this builder, allowing for fluent style concatenation</returns>
        public SqlTwo Select(params object[] columns)
        {
            return Append(new SqlTwo("SELECT " + string.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        /// <summary>
        ///     Appends an SQL FROM clause to this SQL builder
        /// </summary>
        /// <param name="tables">A collection of table names to be used in the FROM clause</param>
        /// <returns>A reference to this builder, allowing for fluent style concatenation</returns>
        public SqlTwo From(params object[] tables)
        {
            return Append(new SqlTwo("FROM " + string.Join(", ", (from x in tables select x.ToString()).ToArray())));
        }

        /// <summary>
        ///     Appends an SQL GROUP BY clause to this SQL builder
        /// </summary>
        /// <param name="columns">A collection of column names to be grouped by</param>
        /// <returns>A reference to this builder, allowing for fluent style concatenation</returns>
        public Sql GroupBy(params object[] columns)
        {
            return Append(new SqlTwo("GROUP BY " + string.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        private SqlJoinClause Join(string joinType, string table)
        {
            return new SqlJoinClause(Append(new SqlTwo(joinType + table)));
        }

        /// <summary>
        ///     Appends an SQL INNER JOIN clause to this SQL builder
        /// </summary>
        /// <param name="table">The name of the table to join</param>
        /// <returns>A reference an SqlJoinClause through which the join condition can be specified</returns>
        public SqlJoinClause InnerJoin(string table)
        {
            return Join("INNER JOIN ", table);
        }

        /// <summary>
        ///     Appends an SQL LEFT JOIN clause to this SQL builder
        /// </summary>
        /// <param name="table">The name of the table to join</param>
        /// <returns>A reference an SqlJoinClause through which the join condition can be specified</returns>
        public SqlJoinClause LeftJoin(string table)
        {
            return Join("LEFT JOIN ", table);
        }

        /// <summary>
        ///     Returns the SQL statement.
        /// </summary>
        /// <summary>
        ///     Returns the final SQL statement represented by this builder
        /// </summary>
        public override string ToString()
        {
            return SQL;
        }

        /// <summary>
        ///     The SqlJoinClause is a simple helper class used in the construction of SQL JOIN statements with the SQL builder
        /// </summary>
        public class SqlJoinClause
        {
            private readonly Sql _sql;

            public SqlJoinClause(Sql sql)
            {
                _sql = sql;
            }

            /// <summary>
            ///     Appends a SQL ON clause after a JOIN statement
            /// </summary>
            /// <param name="onClause">The ON clause to be appended</param>
            /// <param name="args">Arguments to any parameters embedded in the supplied SQL</param>
            /// <returns>A reference to the parent SQL builder, allowing for fluent style concatenation</returns>
            public SqlTwo On(string onClause, params object[] args)
            {
                return _sql.Append("ON " + onClause, args);
            }
        }
    }
    */
}
