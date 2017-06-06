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

        protected Func<PetaPoco.TableInfo, string> fnHasManyMyKey = tblInfo => tblInfo.TableName + "." + tblInfo.PrimaryKey;


        public static LinqSql<TModel> FilterTrashed(bool withTrashed = false)
        {
            if (withTrashed)
            {
                return new LinqSql<TModel>().Where(e => e.DeletedAt == null);
            }
            else
            {
                return new LinqSql<TModel>();
            }
        }

        public static TModel Find(Func<LinqSql<TModel>, LinqSql<TModel>> _linq, bool withTrashed = false, params object[] args)
        {
            using (var test = _linq(FilterTrashed(withTrashed)))
            {
                return Find(test.SQL, test.Arguments);
            }
        }

        public static TModel Find(Expression<Func<TModel, bool>> predicate, bool withTrashed = false, params object[] args)
        {
            using (var _helper = new SqlWhereHelper<TModel>().VisitWhereCondition(predicate, args))
            {
                return Find(_helper.Sql, withTrashed, _helper.Parameters);
            }
        }

        public static TModel Find(string _whereCondition, bool withTrashed = false, params object[] args)
        {
            using (var test = FilterTrashed(withTrashed).Where(_whereCondition, args))
            {
                return Find(test.SQL, test.Arguments);
            }
        }

        /// <summary>
        /// Final entry of the "Find". This function call should maintain a final copy of the SQL & arguments to be passed to the PetaPoco.
        /// </summary>
        /// <param name="_whereCondition"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TModel Find(string _whereCondition, params object[] args)
        {
            return DbFacade.GetDatabaseConnection(ConnectionName).FirstOrDefault<TModel>(_whereCondition, args);
        }

        public static List<TModel> FindAll(Func<LinqSql<TModel>, LinqSql<TModel>> _linq, bool withTrashed = false, params object[] args)
        {
            using (var test = _linq(FilterTrashed(withTrashed)))
            {
                return FindAll(test.SQL, test.Arguments);
            }
        }

        public static List<TModel> FindAll(Expression<Func<TModel, bool>> predicate, bool withTrashed = false, params object[] args)
        {
            using (var _helper = new SqlWhereHelper<TModel>().VisitWhereCondition(predicate, args))
            {
                return FindAll(_helper.Sql, withTrashed, _helper.Parameters);
            }
        }

        public static List<TModel> FindAll(string _whereCondition = "", bool withTrashed = false, params object[] args)
        {
            using (var test = FilterTrashed(withTrashed).Where(_whereCondition, args))
            {
                return FindAll(test.SQL, test.Arguments);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_whereCondition"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static List<TModel> FindAll(string _whereCondition = "", params object[] args)
        {
            return QueryAll(_whereCondition, args).ToList();
        }

        public static IEnumerable<TModel> QueryAll(Func<LinqSql<TModel>, LinqSql<TModel>> _linq, bool withTrashed = false, params object[] args)
        {
            using (var test = _linq(FilterTrashed(withTrashed)))
            {
                return QueryAll(test.SQL, test.Arguments);
            }
        }

        public static IEnumerable<TModel> QueryAll(Expression<Func<TModel, bool>> predicate, bool withTrashed = false, params object[] args)
        {
            using (var _helper = new SqlWhereHelper<TModel>().VisitWhereCondition(predicate, args))
            {
                return QueryAll(_helper.Sql, withTrashed, _helper.Parameters);
            }
        }

        public static IEnumerable<TModel> QueryAll(string _whereCondition = "", bool withTrashed = false, params object[] args)
        {
            using (var test = FilterTrashed(withTrashed).Where(_whereCondition, args))
            {
                return QueryAll(test.SQL, test.Arguments);
            }
        }

        public static IEnumerable<TModel> QueryAll(string query = "", params object[] args)
        {
            return DbFacade.GetDatabaseConnection(ConnectionName).Query<TModel>(query, args).ToList();
        }

        public static List<TModel> FindPage(long page, long itemsPerPage, bool withTrashed = false, params object[] args)
        {
            var _sql = "";
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
            try
            {
                //* Initialize target model
                var targetModel = DbFacade.DynamicModels[typeof(T).Name];

                //* Get this table and that table name
                var thisTableName = TableName;
                var thatTableName = targetModel.PocoData.TableInfo.TableName;

                //* Guessing intermediate table name
                var intermediaTable = DbFacade.GenerateIntermediateTable(thisTableName, thatTableName);

                // PocoData.TableInfo.TableName + "." + PocoData.TableInfo.PrimaryKey
                return targetModel.FindAll<T>(
                    (lin) => lin.Select(thatTableName + ".*")
                                .From(thisTableName)
                                .Join(intermediaTable)
                                .On(fnHasManyMyKey(PocoData.TableInfo), DbFacade.GenerateOnClauseForeignKey(thisTableName, intermediaTable), "")
                                .Join(thatTableName)
                                .On(fnHasManyMyKey(targetModel.PocoData.TableInfo), DbFacade.GenerateOnClauseForeignKey(thatTableName, intermediaTable), "")
                                .Where(predicate));

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

    }
}
