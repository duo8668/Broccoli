using Broccoli.Core.Facade;
using Broccoli.Core.Utils;
using Broccoli.Core.Extensions;
using Inflector;
using Newtonsoft.Json;
using PetaPoco;
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

namespace Broccoli.Core.Database.Eloquent
{
    /*
     * The challenge part of the Model is about the relationship discoverer.
     * Ideally, we should save the discovered relationship but we might end up recursive calls.
     * We need to handle this case.
     */
    [PetaPoco.PrimaryKey("id")]
    public class Model<TModel> : ModelBase<TModel>
    {
        public static TModel Find(string _whereCondition, bool withTrashed = false, params object[] args)
        {
            var db = DbFacade.GetDatabaseConnection(ConnectionName);

            var pd2 = db.GetPocoDataForType(typeof(TModel));

            var _sql = PetaPoco.Sql.Builder
                .Select("*")
                .From(TableName)
                .Where(_whereCondition, args)
                ;
            if (!withTrashed)
            {
                _sql = _sql.Where("created_at IS NOT NULL ");
            }

            return DbFacade.GetDatabaseConnection(ConnectionName).FirstOrDefault<TModel>(_sql.SQL, args);
        }

        public static List<TModel> FindAll(string _whereCondition = "", bool withTrashed = false)
        {
            return QueryAll(_whereCondition, withTrashed).ToList();
        }

        public static IEnumerable<TModel> QueryAll(string _whereCondition = "", bool withTrashed = false)
        {
            var _sql = PetaPoco.Sql.Builder
                 .Select("*")
                .From(TableName);
            if (!string.IsNullOrEmpty(_whereCondition))
            {
                _sql = _sql.Where(_whereCondition);
            }
            if (!withTrashed)
            {
                _sql = _sql.Where("created_at IS NOT NULL ");
            }

            return DbFacade.GetDatabaseConnection(ConnectionName).Query<TModel>(_sql);
        }

        public static IEnumerable<TModel> QueryAll(Sql _whereCondition = null, bool withTrashed = false)
        {
            var _sql = new Sql();
            if (_whereCondition == null)
            {
                _sql = _whereCondition;
            }
            else
            {
                _sql = PetaPoco.Sql.Builder.Select("*").From(TableName);
            }

            if ((_whereCondition) != null)
            {
                _sql = _sql.Where(_whereCondition);
            }
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

        public static TModel Save(TModel _data)
        {
            //* check if data exists, if not exists then update

            return default(TModel);
        }

        public List<TModel> InvokeFindAll(string _whereCondition = "", bool withTrashed = false)
        {
            return Dynamic(typeof(TModel)).FindAll(_whereCondition, withTrashed);
            // return FindAll(_whereCondition, withTrashed);
        }


        public virtual List<T> hasMany<T>(string _additionalWhereCondition = "", bool withTrashed = false) where T : Model<T>, new()
        {
            //List<T> ttt = new List<T>();
            //T targetType = new T();

            var currentPd = PocoData;
            var pd2 = Dynamic(typeof(T)).PocoData;

            var thisTableName = TableName;
            var thatTableName = pd2.TableInfo.TableName;
            var intermediaTable = GenerateIntermediateTable(thisTableName, thatTableName);
            //var ret = targetType.InvokeFindAll();
            var ret = Dynamic(typeof(T)).FindAll(_additionalWhereCondition);
            return ret;
        }
    }
}
