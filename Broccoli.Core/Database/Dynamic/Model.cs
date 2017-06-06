using Broccoli.Core.Database.Builder;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Dynamic
{
    public class Model
    {
        public Type ModelType { get; protected set; }

        public dynamic Instance { get; protected set; }

        public Model(Type modelType)
        {
            this.ModelType = modelType;
        }

        public Model(object entity = null)
        {
            if (entity != null)
            {
                this.ModelType = entity.GetType();
                this.Instance = (dynamic)entity;
            }
        }

        public dynamic InvokeStatic(string methodName, params object[] args)
        {
            var types = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                types[i] = args[i].GetType();
            }

            return this.ModelType.GetMethod
            (
                methodName,
                BindingFlags.FlattenHierarchy |
                BindingFlags.Public |
                BindingFlags.Static,
                null,
                types,
                null
            ).Invoke(null, args);
        }

        public T InvokeStatic<T>(string methodName, params object[] args)
        {
            return (T)this.InvokeStatic(methodName, args);
        }

        public dynamic GetStatic(string propName)
        {
            return this.ModelType.GetProperty
            (
                propName,
                BindingFlags.FlattenHierarchy |
                BindingFlags.Public |
                BindingFlags.Static
            ).GetValue(null);


        }
        public dynamic GetStaticField(string fieldName)
        {
            return this.ModelType.GetField(fieldName, BindingFlags.Static
               | BindingFlags.FlattenHierarchy
               | BindingFlags.Public
               | BindingFlags.NonPublic
               ).GetValue(null);
        }
        public dynamic SetStatic(string propName, dynamic value)
        {
            return this.ModelType.GetProperty
            (
                propName,
                BindingFlags.FlattenHierarchy |
                BindingFlags.Public |
                BindingFlags.Static
            ).SetValue(null, value);
        }

        public T GetStatic<T>(string propName)
        {
            return (T)this.GetStatic(propName);
        }

        public T GetStaticField<T>(string fieldName)
        {
            return (T)this.GetStaticField(fieldName);
        }


        public string TableName
        {
            get
            {
                return this.GetStatic("TableName");
            }
        }

        public dynamic Linq
        {
            get
            {
                return this.GetStatic("Linq");
            }
        }

        public JSchema JsonSchema
        {
            get
            {
                return this.GetStatic<JSchema>("JsonSchema");
            }
        }

        public int Id
        {
            get { return this.Instance.Id; }
            set { this.Instance.Id = value; }
        }

        public string RecordInfo
        {
            get { return this.Instance.RecordInfo; }
            set { this.Instance.RecordInfo = value; }

        }

        public PetaPoco.PocoData PocoData
        {
            get { return this.GetStatic<PetaPoco.PocoData>("PocoData"); }
        }

        public Dictionary<string, object> OriginalPropertyBag
        {
            get { return this.GetStatic<Dictionary<string, object>>("OriginalPropertyBag"); }
        }

        public Dictionary<string, PetaPoco.PocoColumn> ColumnInfos
        {
            get { return this.GetStatic<Dictionary<string, PetaPoco.PocoColumn>>("ColumnInfos"); }
        }
        public DateTime CreatedAt
        {
            get { return this.Instance.CreatedAt; }
            set { this.Instance.CreatedAt = value; }
        }

        public DateTime ModifiedAt
        {
            get { return this.Instance.ModifiedAt; }
            set { this.Instance.ModifiedAt = value; }
        }

        public DateTime? DeletedAt
        {
            get { return this.Instance.DeletedAt; }
            set { this.Instance.DeletedAt = value; }
        }


        public string ToJson()
        {
            return this.Instance.ToJson();
        }

        public dynamic FromJson(string json)
        {
            return this.InvokeStatic("FromJson", json);
        }

        public dynamic FromJsonArray(string json)
        {
            return this.InvokeStatic("FromJsonArray", json);
        }

        public override string ToString()
        {
            return this.Instance.ToString();
        }

        public T Get<T>(string propName, bool loadFromDiscovered = true, bool LoadFromDb = true)
        {
            return this.Instance.Get<T>(propName, loadFromDiscovered, LoadFromDb);
        }

        public void Set<T>(T value, string propName, bool triggerChangeEvent = true)
        {
            this.Instance.Set<T>(value, propName, triggerChangeEvent);
        }

        public dynamic Hydrate(Dictionary<string, object> record, bool fromUser = false)
        {
            return this.InvokeStatic("Hydrate", record, fromUser);
        }

        public dynamic Hydrate(List<Dictionary<string, object>> records, bool fromUser = false, params object[] args)
        {
            return this.InvokeStatic("Hydrate", records, fromUser);
        }

        public dynamic FilterTrashed(bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic("FilterTrashed", withTrashed);
        }

        public dynamic Find(int key, bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic("Find", key, withTrashed, args);
        }

        public dynamic Find(object entity, bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic("Find", entity, withTrashed);
        }

        public dynamic FindAll(string _whereCondition = "", bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic("FindAll", _whereCondition, withTrashed, args);
        }

        public dynamic FindAll<T>(Expression<Func<T, bool>> predicate, bool withTrashed = false, params object[] args) where T : Eloquent.Model<T>, new()
        {
            return this.InvokeStatic("FindAll", predicate, withTrashed, args);
        }

        public dynamic FindAll<T>(Func<LinqSql<T>, LinqSql<T>> _linq, bool withTrashed = false, params object[] args) where T: Eloquent.Model<T>,new()
        {
            return this.InvokeStatic("FindAll", _linq, withTrashed, args);
        }
        public bool Exists(int key, bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic<bool>("Exists", key, withTrashed);
        }

        public bool Exists(object entity, bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic<bool>("Exists", entity, withTrashed);
        }

        public bool All(string predicate, bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic<bool>("All", predicate, withTrashed);
        }

        public bool Any(bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic<bool>("Any", withTrashed);
        }

        public bool Any(string predicate, bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic<bool>("Any", predicate, withTrashed);
        }

        public int Count(bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic<int>("Count", withTrashed);
        }

        public int Count(string predicate, bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic<int>("Count", predicate, withTrashed);
        }

        public dynamic First(bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic("First", withTrashed);
        }

        public dynamic First(string predicate, bool withTrashed = false)
        {
            return this.InvokeStatic("First", predicate, withTrashed);
        }

        public dynamic FirstOrDefault(bool withTrashed = false)
        {
            return this.InvokeStatic("FirstOrDefault", withTrashed);
        }

        public dynamic FirstOrDefault(string predicate, bool withTrashed = false)
        {
            return this.InvokeStatic("FirstOrDefault", predicate, withTrashed);
        }

        public dynamic Single(bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic("Single", withTrashed);
        }

        public dynamic Single(string predicate, bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic("Single", predicate, withTrashed);
        }

        public dynamic SingleOrDefault(bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic("SingleOrDefault", withTrashed);
        }

        public dynamic SingleOrDefault(string predicate, bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic("SingleOrDefault", predicate, withTrashed);
        }

        public dynamic Where(string predicate, bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic("Where", predicate, withTrashed);
        }

        public dynamic Like(string predicate, bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic("Like", predicate, withTrashed);
        }
        /*
        public dynamic OrderBy(string predicate, OrderDirection direction = OrderDirection.ASC, bool withTrashed = false)
        {
            return this.InvokeStatic("OrderBy", predicate, direction, withTrashed);
        }
        */
        public dynamic Skip(int count, bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic("Skip", count, withTrashed);
        }

        public dynamic Take(int count, bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic("Take", count, withTrashed);
        }

        public dynamic ToArray(bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic("ToArray", withTrashed);
        }

        public dynamic ToList(bool withTrashed = false, params object[] args)
        {
            return this.InvokeStatic("ToList", withTrashed);
        }

        public dynamic Create(object entity, params object[] args)
        {
            return this.InvokeStatic("Create", entity);
        }

        public dynamic Create(Dictionary<string, object> record, params object[] args)
        {
            return this.InvokeStatic("Create", record);
        }

        public dynamic Create(string json, params object[] args)
        {
            return this.InvokeStatic("Create", json);
        }

        public dynamic CreateMany(string json, params object[] args)
        {
            return this.InvokeStatic("CreateMany", json);
        }

        public dynamic SingleOrCreate(object entity, params object[] args)
        {
            return this.InvokeStatic("SingleOrCreate", entity);
        }

        public dynamic SingleOrCreate(Dictionary<string, object> record, params object[] args)
        {
            return this.InvokeStatic("SingleOrCreate", record);
        }

        public dynamic SingleOrCreate(string json)
        {
            return this.InvokeStatic("SingleOrCreate", json);
        }

        public dynamic SingleOrNew(object entity, params object[] args)
        {
            return this.InvokeStatic("SingleOrNew", entity);
        }

        public dynamic SingleOrNew(Dictionary<string, object> record, params object[] args)
        {
            return this.InvokeStatic("SingleOrNew", record);
        }

        public dynamic SingleOrNew(string json, params object[] args)
        {
            return this.InvokeStatic("SingleOrNew", json);
        }

        public dynamic FirstOrCreate(object entity, params object[] args)
        {
            return this.InvokeStatic("FirstOrCreate", entity);
        }

        public dynamic FirstOrCreate(Dictionary<string, object> record, params object[] args)
        {
            return this.InvokeStatic("FirstOrCreate", record);
        }

        public dynamic FirstOrCreate(string json, params object[] args)
        {
            return this.InvokeStatic("FirstOrCreate", json);
        }

        public dynamic FirstOrNew(object entity, params object[] args)
        {
            return this.InvokeStatic("FirstOrNew", entity);
        }

        public dynamic FirstOrNew(Dictionary<string, object> record, params object[] args)
        {
            return this.InvokeStatic("FirstOrNew", record);
        }

        public dynamic FirstOrNew(string json, params object[] args)
        {
            return this.InvokeStatic("FirstOrNew", json);
        }

        public void Update(Dictionary<string, object> record, params object[] args)
        {
            this.InvokeStatic("Update", record);
        }

        public void Update(string json, params object[] args)
        {
            this.InvokeStatic("Update", json);
        }

        public void UpdateMany(string json)
        {
            this.InvokeStatic("UpdateMany", json);
        }

        public void Update(string assignments, bool withTrashed = false, params object[] args)
        {
            this.InvokeStatic("Update", assignments, withTrashed);
        }

        public dynamic UpdateOrCreate(object find, object update, params object[] args)
        {
            return this.InvokeStatic("UpdateOrCreate", find, update);
        }

        public dynamic UpdateOrCreate(string find, object update, params object[] args)
        {
            return this.InvokeStatic("UpdateOrCreate", find, update);
        }

        public void Destroy(bool hardDelete = false, params object[] args)
        {
            this.InvokeStatic("Destroy", hardDelete);
        }

        public void Destroy(int key, bool hardDelete = false, params object[] args)
        {
            this.InvokeStatic("Destroy", key, hardDelete);
        }

        public void Destroy(bool hardDelete = false, params int[] keys)
        {
            this.InvokeStatic("Destroy", hardDelete, keys);
        }

        public void Delete(bool hardDelete = false)
        {
            this.Instance.Delete(hardDelete);
        }

        public dynamic Restore()
        {
            return this.Instance.Restore();
        }

        public dynamic Save()
        {
            return this.Instance.Save();
        }
    }
}
