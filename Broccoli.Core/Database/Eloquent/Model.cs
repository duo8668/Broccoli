using Broccoli.Core.Database.Builder;
using Broccoli.Core.Database.Events;
using Broccoli.Core.Extensions;
using Broccoli.Core.Facade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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
            return QueryAll(_linq, withTrashed, args).FirstOrDefault();
        }

        public static TModel Find(Expression<Func<TModel, bool>> predicate, bool withTrashed = false, params object[] args)
        {
            return QueryAll(predicate, withTrashed, args).FirstOrDefault();
        }

        public static TModel Find(string _whereCondition, bool withTrashed = false, params object[] args)
        {
            return QueryAll(_whereCondition, withTrashed, args).FirstOrDefault();
        }

        /// <summary>
        /// Final entry of the "Find". This function call should maintain a final copy of the SQL & arguments to be passed to the PetaPoco.
        /// </summary>
        /// <param name="_whereCondition"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static TModel Find(string query, params object[] args)
        {
            return QueryAll(query, args).FirstOrDefault();
        }

        #region Async methods
        public static async Task<TModel> FindAsync(Expression<Func<TModel, bool>> predicate, bool withTrashed = false, params object[] args)
        {
            using (var _helper = new SqlWhereHelper<TModel>().VisitWhereCondition(predicate, args))
            {
                return await FindAsync(_helper.Sql, withTrashed, _helper.Parameters);
            }
        }

        public static async Task<TModel> FindAsync(string _whereCondition = "", bool withTrashed = false, params object[] args)
        {
            using (var test = FilterTrashed(withTrashed).Where(_whereCondition, args))
            {
                return await FindAsync(test.SQL, test.Arguments);
            }
        }

        public static async Task<TModel> FindAsync(string query, params object[] args)
        {
            return (await DbFacade.GetDatabaseConnection(ConnectionName).BroccoQuery<TModel>(string.Format("{0} {1}", SelectSqlCache, query), args)).SingleOrDefault();
        }
        #endregion

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
            return QueryAll(_whereCondition, withTrashed, args).ToList();
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
            if (predicate != null)
            {
                using (var _helper = new SqlWhereHelper<TModel>().VisitWhereCondition(predicate, args))
                {
                    return QueryAll(_helper.Sql, withTrashed, _helper.Parameters);
                }
            }
            else
            {
                return QueryAll("", withTrashed, args);
            }
        }

        public static IEnumerable<TModel> QueryAll(string _whereCondition = "", bool withTrashed = false, params object[] args)
        {
            using (var test = FilterTrashed(withTrashed).Where(_whereCondition, args))
            {
                return QueryAll(string.Format("{0} {1}", SelectSqlCache, test.SQL), test.Arguments);
            }
        }

        //* TODO: Fix for QueryAll issue to support caching
        public static IEnumerable<TModel> QueryAll(string query, params object[] args)
        {
            return DbFacade.GetDatabaseConnection(ConnectionName).Query<TModel>(query, args);
        }

        public static List<TModel> FindPage(long page, long itemsPerPage, string _sql, bool withTrashed = false, params object[] args)
        {
            return DbFacade.GetDatabaseConnection(ConnectionName).Page<TModel>(page, itemsPerPage, _sql, null).Items;
        }

        /// <summary>
        /// TODO : implement finding the record by all properties value POST INSERT
        /// </summary>
        /// <returns></returns>
        public void Save()
        {
            //* initialize an object for execute
            var toExecute = FilterTrashed();

            //* set the modified records. The logic behind will handle it without hassle
            ModifiedAt = DateTime.Now;

            if (Id == 0)
            {
                CreatedAt = DateTime.Now;
                //* Need to form value to save
                var recordsToInsert = from pocoCol in PocoColumns.Values
                                      where PropertyBag.ContainsKey(pocoCol.PropertyInfo.Name)
                                      select new { pocoCol.ColumnName, value = PropertyBag[pocoCol.PropertyInfo.Name] };
                //* Do insert here
                toExecute
                    .Insert((from c in recordsToInsert select c.ColumnName).ToArray())
                    .Into(TableName);
                foreach (var red in recordsToInsert)
                {
                    toExecute.Value(red.ColumnName, red.value);
                }
                // var ret = DbFacade.GetDatabaseConnection(ConnectionName).Insert(toExecute.SQL, toExecute.Arguments);
                var ret = DbFacade.GetDatabaseConnection(ConnectionName).Insert(TableName, this);
                //* implement finding the records by all matches
                if (ret != null && ret.IsNumber())
                {
                    Id = long.Parse(ret.ToString());
                }
                else
                {
                    Id = long.MinValue;
                }
            }
            else
            {
                UpdateResult = int.MinValue;
                //* Need to form value to save
                var recordsToUpdate = from ModifiedProp in ModifiedColumns
                                      let pocoCol = PocoColumns[ModifiedProp]
                                      select new { pocoCol.ColumnName, value = PropertyBag[ModifiedProp] };
                //* Do update here
                toExecute.Update().Where((model) => model.Id == Id);
                foreach (var red in recordsToUpdate)
                {
                    toExecute.Set(red.ColumnName, red.value);
                }
                UpdateResult = DbFacade.GetDatabaseConnection(ConnectionName).Update(this);
            }
        }

        private void RefreshEntity(Expression<Func<TModel, bool>> predicate, params object[] args)
        {
            if (predicate != null)
            {
                using (var _helper = new SqlWhereHelper<TModel>().VisitWhereCondition(predicate, args))
                {
                    RefreshEntity(_helper.Sql, _helper.Parameters);
                }
            }
            else
            {
                RefreshEntity("", args);
            }
        }

        private void RefreshEntity(string sql, params object[] args)
        {
            var newModel = Find(sql, false, args);
            PropertyBag = newModel.PropertyBag;
        }

        #region Special relationship
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
            return hasMany(predicate, withTrashed).SingleOrDefault();
        }
        #endregion
    }
}
