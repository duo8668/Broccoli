using Broccoli.Core.Database.Attributes;
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
    /*
     * The challenge part of the Model is about the relationship discoverer.
     * Ideally, we should save the discovered relationship but we might end up recursive calls.
     * We need to handle this case.
     */
    [PetaPoco.PrimaryKey("id")]
    public class Model<TModel> : ModelBase
    {
        protected static PetaPoco.Database dbCtx;

        public static List<TModel> FindAll(string primaryKey, bool withTrashed = false)
        {
            var _sql = PetaPoco.Sql.Builder
                .Select("*");
            if (!withTrashed)
            {
                _sql = _sql.Where("date_created IS NOT NULL");
            }

            return dbCtx.Fetch<TModel>(_sql);
        }

        public static TModel Find(string primaryKey, bool withTrashed = false)
        {
            var _sql = PetaPoco.Sql.Builder
                .Select("*")
                .Where("id = @0", primaryKey);
            if (!withTrashed)
            {
                _sql = _sql.Where("date_created IS NOT NULL");
            }

            return dbCtx.First<TModel>(_sql);
        }

        public static TModel Save(TModel _data)
        {
            //* check if data exists, if not exists then update

            return default(TModel);
        }
    }
}
