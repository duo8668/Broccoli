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
    public class Model<TModel>
    {
        protected PetaPoco.Database dbCtx;

        protected string primaryKey { get; set; }

        private string _SqlTableName;

        protected virtual string SqlTableName
        {
            get
            {
                if (_SqlTableName == null)
                {
                    try
                    {
                        // First lets see if the model has it's own table name.
                        _SqlTableName = this.GetType()
                           .GetCustomAttribute<SqlTableNameAttribute>(false)
                           .Value;
                    }
                    catch (NullReferenceException)
                    {
                        // Calculate the table name based on the class name.
                        var typeString = this.GetType().ToString();
                        var typeParts = typeString.Split('.');
                        _SqlTableName = typeParts[typeParts.Length - 1];
                        _SqlTableName = _SqlTableName.Pluralize();
                    }
                }

                return _SqlTableName;
            }
        }

        public TModel Find(string primaryKey, bool withTrashed = false)
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

        [DataType(DataType.Text)]
        public string RecordInfo { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime ModifiedAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DeletedAt { get; set; }
    }
}
