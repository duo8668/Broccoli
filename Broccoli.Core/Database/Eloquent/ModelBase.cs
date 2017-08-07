using Broccoli.Core.Database.Builder;
using Broccoli.Core.Database.Events;
using Broccoli.Core.Extensions;
using Broccoli.Core.Facade;
using Broccoli.Core.Utils;
using Newtonsoft.Json;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Eloquent
{
    public abstract class ModelBase
    {
        public static Dynamic.Model Dynamic(Type modelType)
        {
            return new Dynamic.Model(modelType);
        }

        public static Dynamic.Model Dynamic(string modelName)
        {
            return new Dynamic.Model(DbFacade.GetModel(modelName));
        }

        public static Dynamic.Model Dynamic(object entity)
        {
            return new Dynamic.Model(entity);
        }

        public static Dynamic.Model Dynamic<TModel>()
        {
            return new Dynamic.Model(typeof(TModel));
        }
    }

    [PetaPoco.PrimaryKey("id")]
    public class ModelBase<TModel> : ModelBase, IModelBase, IModelSave<TModel> where TModel : Model<TModel>, new()
    {

        protected static string _modelName;
        private static string _selectSqlCache;
        protected List<string> _loadedProps = new List<string>();

        //* Special method for calling Extended Save on IEnumerable type
        protected static Dictionary<string, MethodInfo> _extMethodInfos;
        protected static Dictionary<string, MethodInfo> ExtMethodInfos
        {
            get
            {
                if (_extMethodInfos == null)
                {
                    _extMethodInfos = new Dictionary<string, MethodInfo>();
                }
                return _extMethodInfos;
            }
        }

        public ModelBase()
        {
            PropertyBag = new Dictionary<string, object>();
            ModelSavedEvent += Model_ModelSavedEvent;
        }

        public static void Init()
        {
            // LoadIEnumerable();
        }
        public static void LoadIEnumerable()
        {
            typeof(TModel).GetProperties().ForEach((prop) =>
            {
                if (prop.PropertyType.Name.Equals(typeof(IEnumerable<>).Name))
                {
                    //* if the property return type if IEnumerable, then most likely it is one of the Entity
                    // ExtMethodInfos.Add(ExtensionMethodSingleton.GetIEnumerableSaveMethodName(), ExtensionMethodSingleton.GetIEnumerableSaveMethod());
                    //DynamicListPropertyInfos.Add(prop.Name, prop);
                    //m.Invoke(prop.GetValue(this, null),null);
                }
            });
        }

        [JsonIgnore]
        [PetaPoco.Ignore]
        public static string ModelName
        {
            get
            {
                if (_modelName == null)
                {
                    _modelName = typeof(TModel).Name;
                }

                return _modelName;
            }
        }

        [JsonIgnore]
        [PetaPoco.Ignore]
        public static string ConnectionName
        {
            get
            {
                return DbFacade.ConnectionNames[ModelName];
            }
        }

        [JsonIgnore]
        [PetaPoco.Ignore]
        public static string TableName
        {
            get
            {
                return DbFacade.TableNames[ModelName];
            }
        }

        [JsonIgnore]
        [PetaPoco.Ignore]
        public static string SelectSqlCache
        {
            get
            {
                if (string.IsNullOrEmpty(_selectSqlCache))
                {
                    _selectSqlCache = BroccoAutoSelectHelper.AddSelectClause<TModel>(BroccoliDatabase.BroccoProvider);
                }
                return _selectSqlCache;
            }
        }

        [JsonIgnore]
        [PetaPoco.Ignore]
        public static PocoData PocoData
        {
            get
            {
                // Return a new list, and leave the cached copy as is.
                return DbFacade.PocoDatas[ModelName];
            }
        }

        [JsonIgnore]
        [PetaPoco.Ignore]
        public static Dictionary<String, PocoColumn> PocoColumns
        {
            get
            {
                // Return a new list, and leave the cached copy as is.
                return DbFacade.ColumnInfos[ModelName];
            }
        }

        [PetaPoco.Column("id")]
        public long? Id
        {
            get
            {
                return Get<long>();
            }
            set
            {
                Set<long?>(value);
            }
        }

        [PetaPoco.Column("record_info")]
        public string RecordInfo
        {
            get
            {
                return Get<string>();
            }
            set
            {
                Set<string>(value);
            }
        }

        [PetaPoco.Column("created_at")]
        public DateTime CreatedAt
        {
            get
            {
                return Get<DateTime>();
            }
            set
            {
                Set<DateTime>(value);
            }
        }

        [PetaPoco.Column("modified_at")]
        public DateTime ModifiedAt
        {
            get
            {
                return Get<DateTime>();
            }
            set
            {
                Set<DateTime>(value);
            }
        }

        [PetaPoco.Column("deleted_at")]
        public DateTime? DeletedAt
        {
            get
            {
                return Get<DateTime?>();
            }
            set
            {
                Set<DateTime?>(value);
            }
        }

        [JsonIgnore]
        [PetaPoco.Ignore]
        public int UpdateResult
        {
            get; set;
        }

        #region Custom property management
        [JsonIgnore]
        [PetaPoco.Ignore]
        public Dictionary<string, object> PropertyBag { get; protected set; }

        [JsonIgnore]
        [PetaPoco.Ignore]
        public Dictionary<string, object> OriginalPropertyBag
        {
            get
            {
                if (_OriginalPropertyBag == null)
                {
                    // Here we create _"THE"_ original property bag. Think about it the original values of all properties are
                    // their defaults. Lists are initialised so we don't have to check for null, we can just loop over an empty list.

                    _OriginalPropertyBag = new Dictionary<string, object>();

                    foreach (var kyp in PocoData.Columns)
                    {
                        var prop = kyp.Value.PropertyInfo;
                        if (TypeMapper.IsList(prop.PropertyType))
                        {
                            _OriginalPropertyBag[prop.Name] =
                            Activator.CreateInstance
                            (
                                typeof(List<>).MakeGenericType
                                (
                                    prop.PropertyType.GenericTypeArguments[0]
                                )
                            );
                        }
                        else if (prop.PropertyType.IsValueType)
                        {
                            _OriginalPropertyBag[prop.Name] = Activator
                            .CreateInstance(prop.PropertyType);
                        }
                        else
                        {
                            _OriginalPropertyBag[prop.Name] = null;
                        }
                    }
                    _OriginalPropertyBagCount = _OriginalPropertyBag.Count();
                }

                return _OriginalPropertyBag;
            }
        }

        private Dictionary<string, object> _OriginalPropertyBag;
        private static int _OriginalPropertyBagCount;

        /**
         * Entity Property Getter.
         * All _"mapped"_ properties need to implement this as their Getter.
         *
         * 	class Foo : Model<Foo>
         * 	{
         * 		public string Bar { get { return Get<string>(); } set... }
         * 	}
         */
        public virtual T Get<T>([CallerMemberName] string propName = "", bool loadFromDb = true, bool isAList = false)
        {
            // Lets attempt to get the value from the PropertyBag Dict.
            object value = null;
            if (this.PropertyBag.TryGetValue(propName, out value))
            {
                return value == null ? default(T) : (T)value;
            }

            // Bail out if we have been told not to load anything from our discovered list or from the database.
            if (!loadFromDb)
            {
                return default(T);
            }

            // If we get to here and we have not managed to load the requested entity or entities from our discovered list, then the last place
            // to look is obviously the database. However if we ourselves do not have an Id then we can not possibly have any related entities.
            if (!this.PropertyBag.ContainsKey("id"))
            {
                return default(T);
            }

            // If we get to hear, we have checked the property bag for a value, the discovered entities list and the database and found nothing
            // so lets set the value to null and move on.

            if (isAList)
            {
                dynamic tmp = Activator.CreateInstance
                (
                    typeof(List<>).MakeGenericType
                    (
                        typeof(T).GenericTypeArguments[0]
                    )
                );

                this.Set(tmp, propName, false);
                return tmp;
            }
            else
            {
                this.Set(default(T), propName, false);
            }
            return default(T);
        }

        /**
        * Entity Property Setter.
        *
        * All _"mapped"_ properties need to implement this as their Setter.
        *
        * 	class Foo : Model<Foo>
        * 	{
        * 		public string Bar { get... set { Set(value); } }
        * 	}
        */
        public virtual void Set<T>(T value, [CallerMemberName] string propName = "", bool triggerChangeEvent = true, bool isAList = false)
        {
            // If the property does not already have a value, set it's original value.
            this.PropertyBag[propName] = value;

            if (!_loadedProps.Contains(propName))
            {
                triggerChangeEvent = false;
                OriginalPropertyBag[propName] = value;
                _loadedProps.Add(propName);
            }
            else
            {
                triggerChangeEvent = !PropertyBag[propName].Equals(OriginalPropertyBag[propName]);
            }

            // Trigger the change event
            if (triggerChangeEvent) this.FirePropertyChanged(PocoColumns[propName]);
        }
        /// <summary>
        /// 
        /// </summary>
        private static List<string> _modifiedColumns = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        [PetaPoco.Ignore]
        public List<string> ModifiedColumns
        {
            get
            {
                return _modifiedColumns;
            }
        }

        /**
         * This just keeps a list of all the mapped properties that have
         * changed since hydration.
         */
        protected void AddModified(PocoColumn changedCol)
        {
            if (!this.ModifiedColumns.Contains(changedCol.PropertyInfo.Name))
            {
                this.ModifiedColumns.Add(changedCol.PropertyInfo.Name);
            }
        }
        protected void RemoveModified(PocoColumn changedCol)
        {
            if (this.ModifiedColumns.Contains(changedCol.PropertyInfo.Name))
            {
                this.ModifiedColumns.Remove(changedCol.PropertyInfo.Name);
            }
        }
        #endregion

        #region Fire Property event
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PocoColumn prop) { }
        public void FirePropertyChanged(PocoColumn pocoCol)
        {
            // Run some of our own code first.            
            if (_OriginalPropertyBagCount > 0)
            {
                if (PropertyBag[pocoCol.PropertyInfo.Name] != OriginalPropertyBag[pocoCol.PropertyInfo.Name])
                {
                    this.AddModified(pocoCol);

                    // Run the OnPropertyChanged method. This allows models to override the method and not have to worry about calling the base method.
                    this.OnPropertyChanged(pocoCol);

                    // Now fire off any other attached handlers
                    PropertyChangedEventHandler handler = this.PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs(pocoCol.PropertyInfo.Name));
                    }
                }
                else
                {
                    this.RemoveModified(pocoCol);
                }
            }
        }
        #endregion

        #region DYNAMIC LIST HANDLING

        public delegate void ModelSavedEventHandler(object sender, ModelChangedEventArgs<TModel> e);
        public event ModelSavedEventHandler ModelSavedEvent;

        protected void OnModelSaved(object sender, ModelChangedEventArgs<TModel> e)
        {
            ModelSavedEvent?.Invoke(sender, e);
        }

        public virtual void Model_ModelSavedEvent(object sender, ModelChangedEventArgs<TModel> e)
        {
           
        }

        #endregion
    }

    public interface IModelSave<TModel>
    {
        void Model_ModelSavedEvent(object sender, Database.Events.ModelChangedEventArgs<TModel> e);
    }


    public static class ModelExtensionMethods
    {
        public static void Save<T>(this IEnumerable<T> enumerable, dynamic parent = null, dynamic references = null) where T : Model<T>, new()
        {
            if (enumerable == null) return;

            if (enumerable.Count() > 500)
            {
                Parallel.ForEach(enumerable, (item) =>
                {
                    item.Save(parent, references);
                });
            }
            else
            {
                foreach (var item in enumerable)
                {
                    item.Save(parent, references);
                }
            }
        }

    }
}
