using Broccoli.Core.Database.Builder;
using Broccoli.Core.Facade;
using Broccoli.Core.Utils;
using Newtonsoft.Json;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class ModelBase<TModel> : ModelBase, IModelBase where TModel : Model<TModel>, new()
    {
        private List<object> _DiscoveredEntities;

        protected static string _modelName;

        public ModelBase()
        {
            PropertyBag = new Dictionary<string, object>();
        }

        public static void Init()
        {

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

        public static LinqSql<TModel> _linq;

        [JsonIgnore]
        [PetaPoco.Ignore]
        public static LinqSql<TModel> Linq
        {
            get
            {
                if (_linq == null)
                {

                    _linq = new LinqSql<TModel>();
                }
                return _linq;
            }
            protected set
            {
                _linq = value;
            }
        }

        [JsonIgnore]
        [PetaPoco.Ignore]
        public List<object> DiscoveredEntities
        {
            get
            {
                if (this._DiscoveredEntities == null)
                {
                    // Add ourselves to the discovered list.
                    this._DiscoveredEntities = new List<object> { this };
                }
                return this._DiscoveredEntities;
            }
            set
            {
                this._DiscoveredEntities = value;
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

        #region Custom property management

        protected static string[] SpecialAttributes = { "CreatedAt", "ModifiedAt", "DeletedAt" };
        [JsonIgnore]
        [PetaPoco.Ignore]
        public Dictionary<string, object> PropertyBag { get; protected set; }


        protected static Dictionary<string, PropertyInfo> _propertyInfos;
        [PetaPoco.Ignore]
        public static Dictionary<string, PropertyInfo> PropertyInfos
        {
            get
            {
                if (_propertyInfos == null)
                {
                    _propertyInfos = DbFacade.ColumnInfos[ModelName].Values.Select(kyp => kyp.PropertyInfo).ToDictionary(item => item.Name, item => item);
                }

                return _propertyInfos;
            }
        }

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
            // If the property bag hasn't been created yet then obviously we won't find anything in it. Even if someone asks for a related
            // entity, we must either have an Id or the entity / entities will have been "Set" and thus the the PropertyBag will exist.
            if (this.PropertyBag == null) return default(T);

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
            // Grab the property
            var prop = PropertyInfos[propName];

            // If the property does not already have a value, set it's original value.
            if ((this.Get<T>(propName, loadFromDb: false) == null && (typeof(T).IsPrimitive || !TypeMapper.IsClrType(typeof(T)) || TypeMapper.IsNullable(value)))
                || (typeof(T).Equals(typeof(int)) && this.Get<int>(propName, loadFromDb: false)  == 0)
                || (typeof(T).Equals(typeof(DateTime)) && this.Get<DateTime>(propName, loadFromDb: false) == DateTime.MinValue))
            {
                triggerChangeEvent = false;
                if (value != null && isAList)
                {
                    var clone = (value as IEnumerable<object>).Cast<IModel<TModel>>().ToList();

                    OriginalPropertyBag[propName] = clone;
                }
                else
                {
                    OriginalPropertyBag[propName] = value;
                }
            }

            // Save the new value
            this.PropertyBag[propName] = value;

            // Trigger the change event
            if (triggerChangeEvent) this.FirePropertyChanged(prop);
        }

        /// <summary>
        /// 
        /// </summary>
        private List<PropertyInfo> _modifiedProps = new List<PropertyInfo>();

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        [PetaPoco.Ignore]
        public List<PropertyInfo> ModifiedProps
        {
            get
            {
                return this._modifiedProps;
            }
        }

        /**
         * This just keeps a list of all the mapped properties that have
         * changed since hydration.
         */
        protected void AddModified(PropertyInfo changedProp)
        {
            if (!this.ModifiedProps.Contains(changedProp))
            {
                this.ModifiedProps.Add(changedProp);
            }
        }
        protected void RemoveModified(PropertyInfo changedProp)
        {
            if (this.ModifiedProps.Contains(changedProp))
            {
                this.ModifiedProps.Remove(changedProp);
            }
        }
        #endregion

        #region Fire Property event
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyInfo prop) { }
        public void FirePropertyChanged(PropertyInfo prop)
        {
            // Run some of our own code first.            
            if (_OriginalPropertyBagCount > 0)
            {
                if (PropertyBag[prop.Name] != OriginalPropertyBag[prop.Name])
                {
                    this.AddModified(prop);

                    // Run the OnPropertyChanged method. This allows models to override the method and not have to worry about calling the base method.
                    this.OnPropertyChanged(prop);

                    // Now fire off any other attached handlers
                    PropertyChangedEventHandler handler = this.PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs(prop.Name));
                    }
                }
                else
                {
                    this.RemoveModified(prop);
                }
            }
        }
        #endregion

        #region PHASING OUT
        //* Phasing out the codes below, to move to retrieve necessary information from DbFacade for speedier performance and lighter MODEL

        /**
         * When a property is first set, we store a shallow clone of the value. Used in the _"Save"_ method to determin what relationships should be removed.
         *
         * > NOTE: Combine this with a Before and AfterSave event, makes for simple change detection.
         */
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
        #endregion
    }

    public static class ModelExtensionMethods
    {
        public static void Save<T>(this IEnumerable<T> enumerable) where T : Model<T>, new()
        {
            if (enumerable == null) return;

            if (enumerable.Count() > 500)
            {
                Parallel.ForEach(enumerable, (item) =>
                {
                    item.Save();
                });
            }
            else
            {
                foreach (var item in enumerable)
                {
                    item.Save();
                }
            }
        }
    }
}
