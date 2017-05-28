using Broccoli.Core.Facade;
using Broccoli.Core.Utils;
using Broccoli.Core.Extensions;
using Inflector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Broccoli.Core.Database.Utils;
using Broccoli.Core.Database.Dynamic;
using System.Diagnostics;
using Broccoli.Core.Database.Utils.Converters;
using Broccoli.Core.Database.Builder;

namespace Broccoli.Core.Database.Eloquent
{
    /*
     * The challenge part of the Model is about the relationship discoverer.
     * Ideally, we should save the discovered relationship but we might end up recursive calls.
     * We need to handle this case.
     */
    [PetaPoco.PrimaryKey("id")]
    public class Model<TModel> : ModelBase<TModel> where TModel : Model<TModel>, new()
    {
        public static LinqSql<TModel> FilterTrashed(bool withTrashed = false)
        {
            return Linq.FilterTrashed(withTrashed);
        }

        //Expression<Func<TModel, bool>>
        //Expression<Func<LinqSql<TModel>, bool>>
        public static TModel Find(Func<LinqSql<TModel>, LinqSql<TModel>> _linq, bool withTrashed = false, params object[] args)
        {
            var theLINQ = _linq(new LinqSql<TModel>(TableName, ColumnInfos));
            var result = DbFacade.GetDatabaseConnection(ConnectionName).FirstOrDefault<TModel>(theLINQ.FinalSQL, theLINQ.Arguments);
            return result;
        }

        public static TModel Find(Expression<Func<TModel, bool>> predicate, bool withTrashed = false, params object[] args)
        {
            var test = FilterTrashed(withTrashed).Where(predicate);
            var result = DbFacade.GetDatabaseConnection(ConnectionName).FirstOrDefault<TModel>(test.FinalSQL, test.Arguments);
            return result;
        }

        public static TModel Find(string _whereCondition, bool withTrashed = false, params object[] args)
        {
            return Find(ExpressionBuilder.BuildPredicateExpression<TModel>(_whereCondition), withTrashed, args);
        }

        public static List<TModel> FindAll(Func<LinqSql<TModel>, LinqSql<TModel>> _linq, bool withTrashed = false, params object[] args)
        {
            return QueryAll(_linq, withTrashed, args).ToList();
        }

        public static List<TModel> FindAll(Expression<Func<TModel, bool>> predicate, bool withTrashed = false, params object[] args)
        {
            return QueryAll(predicate, withTrashed, args).ToList();
        }

        public static List<TModel> FindAll(string _whereCondition = "", bool withTrashed = false, params object[] args)
        {
            return QueryAll(ExpressionBuilder.BuildPredicateExpression<TModel>(_whereCondition), withTrashed, args).ToList();
        }

        public static IEnumerable<TModel> QueryAll(Func<LinqSql<TModel>, LinqSql<TModel>> _linq, bool withTrashed = false, params object[] args)
        {
            var theLINQ = _linq(new LinqSql<TModel>(TableName, ColumnInfos));
            var result = DbFacade.GetDatabaseConnection(ConnectionName).Query<TModel>(theLINQ.FinalSQL, theLINQ.Arguments);
            return result;
        }

        public static IEnumerable<TModel> QueryAll(Expression<Func<TModel, bool>> predicate, bool withTrashed = false, params object[] args)
        {
            var test = FilterTrashed(withTrashed).Where(predicate);
            var result = DbFacade.GetDatabaseConnection(ConnectionName).Query<TModel>(test.FinalSQL, test.Arguments);
            return result;
        }

        public static IEnumerable<TModel> QueryAll(string _whereCondition = "", bool withTrashed = false, params object[] args)
        {
            return QueryAll(ExpressionBuilder.BuildPredicateExpression<TModel>(_whereCondition), withTrashed, args).ToList();
        }

        public static List<TModel> FindPage(long page, long itemsPerPage, bool withTrashed = false, params object[] args)
        {
            var _sql = PetaPoco.Sql.Builder
                .Select("*");
            if (!withTrashed)
            {
                _sql = _sql.Where("date_created IS NOT NULL ");
            }
            //(long page, long itemsPerPage, string sqlCount, object[] countArgs, string sqlPage, object[] pageArgs)
            return DbFacade.GetDatabaseConnection(ConnectionName).Page<TModel>(page, itemsPerPage, _sql, null).Items;
        }

        public static TModel Save(TModel _data)
        {
            //* check if data exists, if not exists then update

            return default(TModel);
        }

        public virtual List<T> hasMany<T>(Expression<Func<T, bool>> predicate, bool withTrashed = false) where T : Model<T>, new()
        {
            //* Initialize target model
            var targetModel = Dynamic(typeof(T));

            var currentPd = PocoData;
            var targetPd = targetModel.PocoData;

            //* Get this table and that table name
            var thisTableName = TableName;
            var thatTableName = targetPd.TableInfo.TableName;

            //* Guessing intermediate table name
            var intermediaTable = DbFacade.GenerateIntermediateTable(thisTableName, thatTableName);

            var tname = Entities.Customer.TableName;
            /*
            var testSql = targetModel.FilterTrashed(withTrashed)
                    .Select(thatTableName + ".*")
                   .From(thisTableName)
                   .InnerJoin(intermediaTable)
                   .On(DbFacade.GenerateOnClauseForForeignKey(thisTableName, intermediaTable))
                   .InnerJoin(thatTableName)
                   .On(DbFacade.GenerateOnClauseForForeignKey(thatTableName, intermediaTable));

            if (!withTrashed)
            {
                testSql = testSql.Where(thatTableName + ".created_at IS NOT NULL ");
            }
            */
            var ret = targetModel.FindAll<T>(
                (lin) => ((LinqSql<T>)lin.FilterTrashed(withTrashed).Select(thatTableName + ".*")
                   .From(thisTableName)
                   .InnerJoin(intermediaTable)
                   .On(DbFacade.GenerateOnClauseForForeignKey(thisTableName, intermediaTable))
                   .InnerJoin(thatTableName)
                   .On(DbFacade.GenerateOnClauseForForeignKey(thatTableName, intermediaTable)))
                   .Where(predicate)
                   );

            currentPd = null;
            targetPd = null;

            return ret;
        }
    }
}
