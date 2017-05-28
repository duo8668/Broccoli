using Broccoli.Core.Database.Eloquent;
using Broccoli.Core.Database.Utils.Converters;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Builder
{

    public class LinqSql<TModel> : Sql where TModel : Model<TModel>, new()
    {
        string _tableName;
        Dictionary<string, PocoColumn> _columnInfo;
        LinqSql<TModel> thisSql;

        public string FinalSQL { get { return thisSql.SQL; } }

        public LinqSql()
        {

        }

        public LinqSql(string tableName, Dictionary<string, PocoColumn> columnInfos)
        {
            _tableName = tableName;
            _columnInfo = columnInfos;
            thisSql = (LinqSql<TModel>)Select("*").From(_tableName);
        }

        public LinqSql<TModel> FilterTrashed(bool withTrashed = false)
        {
            if (withTrashed)
            {
                return thisSql;
            }
            else
            {
                return thisSql.Where(e => e.DeletedAt == null);
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
                thisSql = thisSql.Where(hold, new_args.ToArray());

                //* Not sure below still needed. Will have to check when we do more query
                //args = args.Concat(new_args.ToArray()).ToArray();
                new_args = null; converter = null; hold = null;
            }

            return this;
        }

        public new Builder.LinqSql<TModel> Where(string sql, params object[] args)
        {
            return (LinqSql<TModel>)base.Where(sql, args);
        }


    }
}
