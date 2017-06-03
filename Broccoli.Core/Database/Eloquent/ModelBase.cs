using Broccoli.Core.Configuration;
using Broccoli.Core.Database.Builder;
using Broccoli.Core.Database.Utils;
using Broccoli.Core.Facade;
using Broccoli.Core.Utils;
using Inflector;
using Newtonsoft.Json;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
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

    public class ModelBase<TModel> : ModelBase, IModelBase where TModel : Model<TModel>, new()
    {
        private List<object> _DiscoveredEntities;

        protected static string _modelName;

        public ModelBase()
        {

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

        [PetaPoco.Column]
        public long id
        {
            get
            {
                return Get<long>();
            }
            set
            {
                Set<long>(value);
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

        [JsonIgnore]
        [PetaPoco.Ignore]
        public Dictionary<string, object> PropertyBag { get; protected set; }
        
        /**
         * Entity Property Getter.
         * All _"mapped"_ properties need to implement this as their Getter.
         *
         * 	class Foo : Model<Foo>
         * 	{
         * 		public string Bar { get { return Get<string>(); } set... }
         * 	}
         */
        public virtual T Get<T>([CallerMemberName] string propName = "", bool loadFromDiscovered = true, bool loadFromDb = true)
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
            if (!loadFromDiscovered || !loadFromDb)
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
            if (TypeMapper.IsList(typeof(T)))
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
                return default(T);
            }
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
        * 
        */
        public virtual void Set<T>(T value, [CallerMemberName] string propName = "", bool triggerChangeEvent = true)
        {
            // Grab the property
            var kyo = ColumnInfos.Single(p => p.Value.PropertyInfo.Name == propName);
            var prop = kyo.Value.PropertyInfo;
            // Create the property bag dict if it doesn't exist yet.
            if (this.PropertyBag == null)
            {
                this.PropertyBag = new Dictionary<string, object>();
            }

            // If the value is an entity or list of entities
            // we will save it to our discovered list.
            if (value != null && !TypeMapper.IsClrType(value))
            {
                //   this.SaveDiscoveredEntities(prop, value);
            }

            // If the property does not already have
            // a value, set it's original value.
            if (this.Get<object>(propName, loadFromDiscovered: false, loadFromDb: false) == null)
            {
                if (value != null && TypeMapper.IsListOfEntities(value))
                {
                    var clone = (value as IEnumerable<object>)
                    .Cast<IModel<TModel>>().ToList();

                    OriginalPropertyBag[propName] = clone;
                }
                else
                {
                    OriginalPropertyBag[propName] = value;
                }
            }

            // Wrap any normal Lists in a BindingList so that we can track when
            // new entities are added so that we may save those entities to our
            // discovered list.
            dynamic propertyBagValue;
            if (value != null && TypeMapper.IsList(value))
            {
                dynamic bindingList = Activator.CreateInstance
                (
                    typeof(BindingList<>).MakeGenericType
                    (
                        value.GetType().GenericTypeArguments[0]
                    ),
                    new object[] { value }
                );

                bindingList.ListChanged += new ListChangedEventHandler
                (
                    (sender, e) =>
                    {
                        //if (!triggerChangeEvent) return;

                        switch (e.ListChangedType)
                        {
                            case ListChangedType.ItemAdded:
                            case ListChangedType.ItemDeleted:
                                {
                                    this.FirePropertyChanged(prop);
                                }
                                break;
                        }
                    }
                );

                propertyBagValue = bindingList;
            }
            else
            {
                propertyBagValue = value;
            }

            // Save the new value
            this.PropertyBag[propName] = propertyBagValue;

            // Trigger the change event
            if (triggerChangeEvent) this.FirePropertyChanged(prop);
        }

        [JsonIgnore]
        [PetaPoco.Ignore]
        public List<PropertyInfo> ModifiedProps
        {
            get
            {
                return this._ModifiedProps;
            }
        }

        private List<PropertyInfo> _ModifiedProps = new List<PropertyInfo>();

        /**
         * This just keeps a list of all the mapped properties that have
         * changed since hydration.
         */
        protected void UpdateModified(PropertyInfo changedProp)
        {
            if (!this.ModifiedProps.Contains(changedProp))
            {
                this.ModifiedProps.Add(changedProp);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyInfo prop) { }
        public void FirePropertyChanged(PropertyInfo prop)
        {
            // Run some of our own code first.
            this.UpdateModified(prop);
            // this.SaveDiscoveredEntities(prop);

            // Run the OnPropertyChanged method. This allows models to override
            // the method and not have to worry about calling the base method.
            this.OnPropertyChanged(prop);

            // Now fire off any other attached handlers
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(prop.Name));
            }
        }
        #endregion


        #region PHASING OUT
        //* Phasing out the codes below, to move to retrieve necessary information from DbFacade for speedier performance and lighter MODEL
 

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
        public static Dictionary<string, PocoColumn> ColumnInfos
        {
            get
            {
                return DbFacade.ColumnInfos[ModelName];
            }
        }

        /**
         * When a property is first set, we store a shallow clone of the value. Used in the _"Save"_ method to determin what relationships should be removed.
         *
         * > NOTE: Combine this with a Before and AfterSave event, makes for simple change detection.
         */
        [JsonIgnore]
        [PetaPoco.Ignore]
        public static Dictionary<string, object> OriginalPropertyBag
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
                }

                return _OriginalPropertyBag;
            }
        }

        private static Dictionary<string, object> _OriginalPropertyBag;
        #endregion
    }
}
