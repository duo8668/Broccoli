using Broccoli.Core.Database.Attributes;
using Broccoli.Core.Facade;
using Inflector;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Eloquent
{
    public class Model : ModelBase
    {

    }
    /*
     * The challenge part of the Model is about the relationship discoverer.
     * Ideally, we should save the discovered relationship but we might end up recursive calls.
     * We need to handle this case.
     */
    [PetaPoco.PrimaryKey("id")]
    public class Model<TModel> : Model
    {
        public static TModel Find(object primaryKey, bool withTrashed = false)
        {
            var priKey = Convert.ChangeType(primaryKey, typeof(int));
            var _sql = PetaPoco.Sql.Builder
                .Select("*")
                .From(TableName)
                .Where("id = @0 ", primaryKey);
            if (!withTrashed)
            {
                _sql = _sql.Where("created_at IS NOT NULL ");
            }

            return DbFacade.GetDatabaseConnection(ConnectionName).FirstOrDefault<TModel>(_sql);
        }

        public static List<TModel> FindAll(bool withTrashed = false)
        {
            return QueryAll(withTrashed).ToList();
        }

        public static IEnumerable<TModel> QueryAll(bool withTrashed = false)
        {
            var _sql = PetaPoco.Sql.Builder
                 .Select("*")
                .From(TableName);
            if (!withTrashed)
            {
                _sql = _sql.Where("created_at IS NOT NULL ");
            }

            return DbFacade.GetDatabaseConnection(ConnectionName).Query<TModel>(_sql);
        }

        public static List<TModel> FindPage(long page, long itemsPerPage, bool withTrashed = false)
        {
            var _sql = PetaPoco.Sql.Builder
                .Select("*");
            if (!withTrashed)
            {
                _sql = _sql.Where("date_created IS NOT NULL ");
            }
            //(long page, long itemsPerPage, string sqlCount, object[] countArgs, string sqlPage, object[] pageArgs)
            return DbFacade.GetDatabaseConnection("broccoli_db").Page<TModel>(page, itemsPerPage, _sql, null).Items;
        }


        public static List<T> Get<T>()
        {
            //* check if data exists, if not exists then update

            return null;
        }

        public static TModel Save(TModel _data)
        {
            //* check if data exists, if not exists then update

            return default(TModel);
        }
    }
}
