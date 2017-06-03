﻿using Broccoli.Core.Database.Eloquent;
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
        private string _sql;
        private List<object> _args;

        private LinqSql<TModel> _theSql;

        private static string prefix = "a";
        private int count = 0;

        protected static readonly char[] selectSpliter = ",".ToCharArray();

        private Queue<SqlWhereHelper<TModel>> _whereCondition = new Queue<SqlWhereHelper<TModel>>();
        private Dictionary<string, string> _selectCols = new Dictionary<string, string>();
        private Dictionary<string, SqlJoinHelper<TModel>> _joinClause = new Dictionary<string, SqlJoinHelper<TModel>>();
        private PredicateConverter _predicateConverter;

        string _tableName;
        string _modelName;

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
            return this;
        }

        public SqlJoinHelper<TModel> Join<T>() where T : Model<T>, new()
        {
            return Join(DbFacade.TableNames[typeof(T).Name]);
        }

        public SqlJoinHelper<TModel> Join(string targetTable)
        {
            return new SqlJoinHelper<TModel>(this, targetTable);
        }

        public LinqSql<TModel> On(SqlJoinHelper<TModel> _on)
        {
            _joinClause.Add(_on.TargetTable, _on);
            return this;
        }

        public Builder.LinqSql<TModel> Where(string sql, params object[] args)
        {
            _whereCondition.Enqueue(new SqlWhereHelper<TModel>(sql, args));

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

        #endregion


        private void Build()
        {
            _sql = null;
            var new_args = new List<object>();
            BuildSelect();
            BuildJoin();
            BuildWhere();
            //var hold = PetaPoco.ParametersHelper.ProcessParams(_predicateConverter.Sql, _predicateConverter.Parameters, new_args);
            //_theLinqSql = _theLinqSql.Where(hold, new_args.ToArray());
            Dispose();
        }

        private void BuildSelect()
        {
            string cols = DbFacade.PocoDatas[_modelName].Columns.Count != 0
                ? string.Join(", ", (from c in DbFacade.PocoDatas[_modelName].QueryColumns select _tableName + "." + c).ToArray())
                : "NULL";

            _sql = "SELECT " + cols + " FROM " + _tableName;
        }
        private void BuildJoin()
        {

        }
        private void BuildWhere()
        {
            string whereQuery = " WHERE ";

            while (_whereCondition.Count() > 0)
            {
                var theWhere = _whereCondition.Dequeue();
                _args.AddRange(theWhere.Arguments);
                whereQuery += theWhere.SQLCondition + " ";
                if (_whereCondition.Count() > 0)
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
            _joinClause.Clear();
            _predicateConverter = null;
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
        public string TargetTable { get; private set; }
        public string TurnCode;

        public SqlJoinHelper(LinqSql<TModel> sql, string targetTable)
        {
            _sql = sql;
            TargetTable = targetTable;
        }

        public LinqSql<TModel> Join(string targetTable)
        {
            return _sql;
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

        public SqlWhereHelper(string sql, params object[] args)
        {
            _sql = sql;
            Arguments = args;
        }


        public void Dispose()
        {
            Arguments = null;
        }
    }
}