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
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Eloquent
{
    public abstract class ModelBase
    {
        protected static string ConnectionHash;
        protected static string ConnectionName;
        protected static string TableName;
        protected ForeignKeyGenerator _foreignKeyGenerator;

        /**
         * Given a model name, we will return the model type.
         *
         * The name can be a fully qualified type name:
         *
         * ```cs
         * 	Model.GetModel("Aceme.Models.Person");
         * ```
         *
         * Or you may provide just the class name:
         *
         * ```cs
         *  Model.GetModel("Person");
         * ```
         *
         * Or you may provide the plurized version:
         *
         * ```cs
         *  Model.GetModel("Persons");
         * ```
         *
         * > NOTE: This is case-insensitive.
         */
        public static Type GetModel(string modelName)
        {
            modelName = modelName.ToLower();

            return DbFacade.GetAllModels().Single(model =>
            {
                var modelNameToCheck = model.ToString().ToLower();

                // Do we have a complete full namespace match
                if (modelNameToCheck == modelName)
                {
                    return true;
                }
                else
                {
                    // Check for a class name match
                    var typeParts = modelNameToCheck.Split('.');
                    var className = typeParts[typeParts.Length - 1];
                    if (className == modelName)
                    {
                        return true;
                    }

                    // We will also check for the pluralized version
                    else if (className.Pluralize() == modelName)
                    {
                        return true;
                    }
                }

                return false;
            });
        }

        /**
         * Return a new DModel instance from the given Type.
         *
         * ```cs
         *  Model.Dynamic(typeof(Foo)).SqlTableName;
         * ```
         */
        public static Dynamic.Model Dynamic(Type modelType)
        {
            return new Dynamic.Model(modelType);
        }

        /**
         * Return a new DModel instance from the given model name.
         *
         * ```cs
         *  Model.Dynamic("Foo").SqlTableName;
         * ```
         */
        public static Dynamic.Model Dynamic(string modelName)
        {
            return new Dynamic.Model(GetModel(modelName));
        }

        /**
         * Return a new DModel instance from the given entity.
         *
         * ```cs
         *  Model.Dynamic(entity).SqlTableName;
         * ```
         */
        public static Dynamic.Model Dynamic(object entity)
        {
            return new Dynamic.Model(entity);
        }

        /**
         * Return a new DModel instance from the given generic type parameter.
         *
         * ```cs
         *  Model.Dynamic<Foo>().SqlTableName;
         * ```
         */
        public static Dynamic.Model Dynamic<TModel>()
        {
            return new Dynamic.Model(typeof(TModel));
        }
    }

    public abstract class ModelBase<TModel> : ModelBase, IModelBase
    {
        private List<object> _DiscoveredEntities;

        public ModelBase()
        {

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

        public string GenerateIntermediateTable(string thisTable, string thatTable)
        {
            _foreignKeyGenerator = new ForeignKeyGenerator();
            return _foreignKeyGenerator.GenerateIntermediateTable(thisTable, thatTable);
        }

        #region Custom property management

        [JsonIgnore]
        [PetaPoco.Ignore]
        public static PocoData PocoData
        {
            get
            {
                if (_PocoData == null)
                {
                    var db = DbFacade.GetDatabaseConnection(ConnectionName);
                    _PocoData = db.GetPocoDataForType(typeof(TModel));
                }

                // Return a new list, and leave the cached copy as is.
                return _PocoData;
            }
        }

        private static PocoData _PocoData;

        public static PocoData GetPocoData()
        {
            return PocoData;
        }

        [JsonIgnore]
        [PetaPoco.Ignore]
        public Dictionary<string, object> PropertyBag { get; protected set; }

        /**
         * When a property is first set, we store a shallow clone of the value.
         * Used in the _"Save"_ method to determin what relationships should be removed.
         *
         * > NOTE: Combine this with a Before and AfterSave event, makes for simple change detection.
         */
        [JsonIgnore]
        [PetaPoco.Ignore]
        public Dictionary<string, object> OriginalPropertyBag
        {
            get
            {
                if (this._OriginalPropertyBag == null)
                {
                    // Here we create _"THE"_ original property bag. Think about it the original values of all properties are
                    // their defaults. Lists are initialised so we don't have to check for null, we can just loop over an empty list.

                    this._OriginalPropertyBag = new Dictionary<string, object>();

                    foreach (var kyp in PocoData.Columns)
                    {
                        var prop = kyp.Value.PropertyInfo;
                        if (TypeMapper.IsList(prop.PropertyType))
                        {
                            this._OriginalPropertyBag[prop.Name] =
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
                            this._OriginalPropertyBag[prop.Name] = Activator
                            .CreateInstance(prop.PropertyType);
                        }
                        else
                        {
                            this._OriginalPropertyBag[prop.Name] = null;
                        }
                    }

                }

                return this._OriginalPropertyBag;
            }
        }
        private Dictionary<string, object> _OriginalPropertyBag;

        /**
         * Entity Property Getter.
         *
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
            var kyo = PocoData.Columns.Single(p => p.Value.PropertyInfo.Name == propName);
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

                    this.OriginalPropertyBag[propName] = clone;
                }
                else
                {
                    this.OriginalPropertyBag[propName] = value;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyInfo prop) { }
        public void FirePropertyChanged(PropertyInfo prop)
        {
            // Run some of our own code first.
            // this.UpdateModified(prop);
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
    }
}
