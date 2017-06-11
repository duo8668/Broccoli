using Broccoli.Core.Database.Builder;
using Broccoli.Core.Database.Events;
using Broccoli.Core.Facade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
        protected Func<string, string, string> fnStdGenerateForeignKey = DbFacade.GenerateOnClauseForeignKey;

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
            if (predicate != null)
            {
                using (var _helper = new SqlWhereHelper<TModel>().VisitWhereCondition(predicate, args))
                {
                    return FindAll(_helper.Sql, withTrashed, _helper.Parameters);
                }
            }
            else
            {
                return FindAll("", withTrashed, args);
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
        public static List<TModel> FindAll(string _whereCondition, params object[] args)
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

        public static IEnumerable<TModel> QueryAll(string query, params object[] args)
        {
            return DbFacade.GetDatabaseConnection(ConnectionName).Query<TModel>(query, PocoData, args);
        }

        public static List<TModel> FindPage(long page, long itemsPerPage, string _sql, bool withTrashed = false, params object[] args)
        {
            return DbFacade.GetDatabaseConnection(ConnectionName).Page<TModel>(page, itemsPerPage, _sql, null).Items;
        }

        public TModel Save()
        {
            //* check if data exists, if not exists then update
            if (Id == 0)
            {

            }
            else
            {
                var recordsToUpdate = (from ModifiedProp in ModifiedProps
                                       select new { PocoColumns[ModifiedProp.Name].ColumnName, val = (dynamic)ModifiedProp.GetValue(this) }
                                     );
                var spare = FilterTrashed().Update();
                foreach (var red in recordsToUpdate)
                {
                    spare.Set(red.ColumnName, red.val);
                }
                spare = spare.Where((model) => model.Id == Id);
                var sql = PetaPoco.ParametersHelper.ProcessParams(spare.SQL, spare.Arguments);
               // var ret = DbFacade.GetDatabaseConnection(ConnectionName).Execute(sql, spare.Arguments);

                return Find((model) => model.Id == Id, args: Id);
            }

            return (TModel)this;
        }

        public virtual IEnumerable<T> hasMany<T>(Expression<Func<TModel, T, bool>> onPredicate, Expression<Func<T, bool>> predicate, bool withTrashed = false) where T : Model<T>, new()
        {

            return null;
        }

        public virtual IEnumerable<T> hasMany<T>(Expression<Func<T, bool>> predicate, bool withTrashed = false) where T : Model<T>, new()
        {
            //* Initialize target model
            var targetModel = DbFacade.DynamicModels[typeof(T).Name];

            //* Get this table and that table name
            var thisTableName = TableName;
            var thatTableName = targetModel.PocoData.TableInfo.TableName;

            //* Guessing intermediate table name
            var intermediaTable = DbFacade.GenerateIntermediateTable(thisTableName, thatTableName);

            // Call the FindAll to return the result
            var results = targetModel.FindAll<T>(
                (lin) => lin.Select(thatTableName + ".*")
                            .From(thisTableName)
                            .Join(intermediaTable)
                            .On(fnHasManyMyKey(PocoData.TableInfo), fnStdGenerateForeignKey(thisTableName, intermediaTable), "")
                            .Join(thatTableName)
                            .On(fnHasManyMyKey(targetModel.PocoData.TableInfo), fnStdGenerateForeignKey(thatTableName, intermediaTable), "")
                            .Where(predicate));

            DynamicListEvent.triggerDynamicListListening<T>(results);

            return results;
        }

        public virtual T hasOne<T>(Expression<Func<T, bool>> predicate, bool withTrashed = false) where T : Model<T>, new()
        {
            //* Initialize target model
            var targetModel = DbFacade.DynamicModels[typeof(T).Name];

            //* Get this table and that table name
            var thisTableName = TableName;
            var thatTableName = targetModel.PocoData.TableInfo.TableName;

            //* Guessing intermediate table name
            var intermediaTable = DbFacade.GenerateIntermediateTable(thisTableName, thatTableName);

            // Call the FindAll to return the result
            var results = targetModel.Find<T>(
                (lin) => lin.Select(thatTableName + ".*")
                            .From(thisTableName)
                            .Join(intermediaTable)
                            .On(fnHasManyMyKey(PocoData.TableInfo), fnStdGenerateForeignKey(thisTableName, intermediaTable), "")
                            .Join(thatTableName)
                            .On(fnHasManyMyKey(targetModel.PocoData.TableInfo), fnStdGenerateForeignKey(thatTableName, intermediaTable), "")
                            .Where(predicate));

            DynamicListEvent.triggerDynamicListListening<T>(results);

            return results;
        }

    }
}
