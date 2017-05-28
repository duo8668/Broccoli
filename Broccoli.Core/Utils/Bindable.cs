using Broccoli.Core.Database.Eloquent;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Utils
{
    public abstract class Bindable<TModel>
    {

        public static List<PropertyInfo> MappedProps
        {
            get
            {
                if (_MappedProps == null)
                {
                    // Grab the public instance properties
                    _MappedProps = typeof(TModel).GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();

                    // We only want properties with public setters
                    _MappedProps = _MappedProps.Where(prop => prop.GetSetMethod() != null).ToList();

                    // Ignore any properties that have the NotMappedAttribute.
                    _MappedProps = _MappedProps.Where(prop => prop.GetCustomAttribute<NotMappedAttribute>(false) == null).ToList();

                    // Because the Id Property is inherited from the Model,
                    // it will be one of the last properties in the list. This
                    // is not ideal and the Id field needs to be first, so it is
                    // the first column in the db table.
                    var idx = _MappedProps.FindIndex(p => p.Name == "Id");
                    var item = _MappedProps[idx];
                    _MappedProps.RemoveAt(idx);
                    _MappedProps.Insert(0, item);
                }

                // Return a new list, and leave the cached copy as is.
                return _MappedProps.ToList();
            }
        }

        private static List<PropertyInfo> _MappedProps;

        [JsonIgnore]
        public Dictionary<string, object> PropertyBag { get; protected set; }

        /**
         * When a property is first set, we store a shallow clone of the value.
         * Used in the _"Save"_ method to determin what relationships should be removed.
         *
         * > NOTE: Combine this with a Before and AfterSave event, makes for simple change detection.
         */
        [JsonIgnore]
        public Dictionary<string, object> OriginalPropertyBag
        {
            get
            {
                if (this._OriginalPropertyBag == null)
                {
                    // Here we create _"THE"_ original property bag.
                    // Think about it the original values of all properties are
                    // their defaults. Lists are initialised so we don't have to
                    // check for null, we can just loop over an empty list.

                    this._OriginalPropertyBag = new Dictionary<string, object>();

                    MappedProps.ForEach(prop =>
                    {
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
                    });
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
         * ```cs
         * 	class Foo : Model<Foo>
         * 	{
         * 		public string Bar { get { return Get<string>(); } set... }
         * 	}
         * ```
         *
         * > TODO: Investigate IL Weaving... or possibly just a super simple
         * > pre compilation script (grunt/gulp task) to automatically add
         * > the needed method calls.
         */
        public virtual T Get<T>([CallerMemberName] string propName = "", bool loadFromDiscovered = true, bool loadFromDb = true)
        {
            // If the property bag hasn't been created yet then obviously we
            // won't find anything in it. Even if someone asks for a related
            // entity, we must either have an Id or the entity / entities will
            // have been "Set" and thus the the PropertyBag will exist.
            if (this.PropertyBag == null) return default(T);

            // Lets attempt to get the value from the PropertyBag Dict.
            object value = null;
            if (this.PropertyBag.TryGetValue(propName, out value))
            {
                return value == null ? default(T) : (T)value;
            }

            // Bail out if we have been told not to load anything
            // from our discovered list or from the database.
            if (!loadFromDiscovered || !loadFromDb)
            {
                return default(T);
            }


            // If we get to here and we have not managed to load the requested
            // entity or entities from our discovered list, then the last place
            // to look is obviously the database. However if we ourselves do not
            // have an Id then we can not possibly have any related entities.
            if (!this.PropertyBag.ContainsKey("Id") || !this.Hydrated)
            {
                return default(T);
            }

            // If we get to hear, we have checked the property bag for a value,
            // the discovered entities list and the database and found nothing
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
        * ```cs
        * 	class Foo : Model<Foo>
        * 	{
        * 		public string Bar { get... set { Set(value); } }
        * 	}
        * ```
        *
        * > TODO: Investigate IL Weaving... or possibly just a super simple
        * > pre compilation script (grunt/gulp task) to automatically add
        * > the needed method calls.
        */
        public virtual void Set<T>(T value, [CallerMemberName] string propName = "", bool triggerChangeEvent = true)
        {
            // Grab the property
            var prop = MappedProps.Single(p => p.Name == propName);

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
            if (this.Hydrated && this.Get<object>(propName, loadFromDiscovered: false, loadFromDb: false) == null)
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

        /**
        * Sometimes we need to know if an entity has been "newed" up by us
        * and hydrated with valid data from the database. or if it has been
        * created else where with un-validated data.
        */
        [JsonIgnore]
        public bool Hydrated { get; protected set; }

        /**
          * Fired when ever a _"Mapped"_ property changes on the entity.
          *
          * > NOTE: Lists are automatically wrapped in BindingLists and setup
          * > to fire this event whenever an entity is added or removed from
          * > the list.
          */
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
    }
}
