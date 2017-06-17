using Broccoli.Core.Configuration;
using Broccoli.Core.Database;
using Broccoli.Core.Database.Eloquent;
using Broccoli.Core.Database.Utils;
using Broccoli.Core.Entities.Core;
using Broccoli.Core.Extensions;
using Inflector;
using PetaPoco;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Broccoli.Core.Facade
{
    public class DbFacade : Facade
    {
        //* Storage for all models
        private static HashSet<Type> _AllModels;
        private static object _dbConnectionsCachelock = new object();

        // Handling foreign key
        private static ForeignKeyGenerator _foreignKeyGenerator;

        //* All Database tables cache
        private static Dictionary<string, string> _dbAllTablesCache = new Dictionary<string, string>();

        //* All mapped connection names cache
        private static Dictionary<string, string> _connectionNamesCache = new Dictionary<string, string>();

        //* All mapped table names cache
        private static Dictionary<string, string> _tableNamesCache = new Dictionary<string, string>();

        //* All reverse mapped model names cache
        private static Dictionary<string, string> _tableToModelNamesCache = new Dictionary<string, string>();

        // the key is the modelName and the value is the PocoData
        private static Dictionary<string, PocoData> _pocoDatas = new Dictionary<string, PocoData>();

        // the key is the modelName and the value is the PocoData
        private static Dictionary<string, Database.Dynamic.Model> _dynamicModelCache = new Dictionary<string, Database.Dynamic.Model>();

        // the key is the modelName and the value is the dictionary of PocoColumn
        private static Dictionary<string, Dictionary<string, PocoColumn>> _columnInfos = new Dictionary<string, Dictionary<string, PocoColumn>>();

        public static void Initialize()
        {
            ForeignKeyGenerator.InitGenerator("__");
            _foreignKeyGenerator = new ForeignKeyGenerator();

            InitModelsCache();
            InitDbAllTablesCache();
        }

        public static HashSet<Type> GetAllModels()
        {
            if (_AllModels != null) return _AllModels;

            _AllModels = new HashSet<Type>();

            AppDomain.CurrentDomain.GetAssemblies().ToList().ForEach(assembly =>
            {
                assembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(ModelBase)))
                .Where(type => type.IsPublic)
                .Where(type => !type.ContainsGenericParameters)
                .ToList().ForEach(type => _AllModels.Add(type));
            });

            return _AllModels;
        }

        public static Dictionary<string, string> ConnectionNames
        {
            get
            {
                return _connectionNamesCache;
            }
        }

        public static Dictionary<string, string> TableNames
        {
            get
            {
                return _tableNamesCache;
            }
        }

        public static Dictionary<string, string> TableToModelNames
        {
            get
            {
                return _tableToModelNamesCache;
            }
        }

        public static Dictionary<string, PocoData> PocoDatas
        {
            get
            {
                return _pocoDatas;
            }
        }

        public static Dictionary<string, Dictionary<string, PocoColumn>> ColumnInfos
        {
            get
            {
                return _columnInfos;
            }
        }

        public static Dictionary<string, Database.Dynamic.Model> DynamicModels
        {
            get
            {
                return _dynamicModelCache;
            }
        }

        public static Type GetModel(string modelName)
        {
            modelName = modelName.ToLower();

            return GetAllModels().Single(model =>
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

        protected static void InitModelsCache()
        {
            GetAllModels().ForEach(model =>
            {
                InitConnectionNamesCache(model);
                InitTableNamesCache(model);
                DoPocoDatasInitialization(model);
                DoPocoColumnsInitialization(model);
                DoDynamicModelInitialization(model);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        protected static void DoPocoDatasInitialization(Type model)
        {
            var db = DbFacade.GetDatabaseConnection(_connectionNamesCache[model.Name]);
            var pd = db.GetPocoDataForType(model);

            _pocoDatas.Add(model.Name, pd);
            db.Dispose();
            pd = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        protected static void DoPocoColumnsInitialization(Type model)
        {
            var cis = new Dictionary<string, PocoColumn>();

            foreach (var kyp in _pocoDatas[model.Name].Columns)
            {
                var prop = kyp.Value.PropertyInfo;
                cis[prop.Name] = kyp.Value;
            }
            _columnInfos.Add(model.Name, cis);

            cis = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        protected static void DoDynamicModelInitialization(Type model)
        {
            var mdl = ModelBase.Dynamic(model);

            _dynamicModelCache.Add(model.Name, mdl);
        }

        /// <summary>
        /// 
        /// </summary>
        protected static void InitDbAllTablesCache()
        {
            var defaultDbConnection = ConfigurationManager.AppSettings["defaultDbConnection"].ToString();

            //* define a holder to store all db connection exists
            List<string> _listOfDbConnNamesToLoad = new List<string>();

            //* add the default one to load
            _listOfDbConnNamesToLoad.Add(defaultDbConnection);

            //* check for additional names to load
            if (DbSchemaConfiguration.Configs.Count() > 0)
            {
                var items = DbSchemaConfiguration.Configs.Select(kvp => kvp.Value).ToList();
                foreach (var item in items)
                {
                    if (item.DatabaseConnectionName != defaultDbConnection)
                    {
                        _listOfDbConnNamesToLoad.Add(item.DatabaseConnectionName);
                    }
                }
            }

            //* Finally, query against the database to load
            string _sql = "SELECT TABLE_SCHEMA,TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE  TABLE_SCHEMA = DATABASE()";
            foreach (var dbConn in _listOfDbConnNamesToLoad)
            {
                var petaDb = new PetaPoco.Database(connectionStringName: dbConn);
                var tableSchemas = petaDb.Query<InformationSchema>(_sql);

                foreach (var item in tableSchemas)
                {
                    var specialKey = item.TABLE_SCHEMA + "_" + item.TABLE_NAME;
                    if (!_dbAllTablesCache.ContainsKey(specialKey))
                    {
                        _dbAllTablesCache.Add(specialKey, item.TABLE_NAME);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        protected static void InitConnectionNamesCache(Type model)
        {
            var defaultDbConnection = ConfigurationManager.AppSettings["defaultDbConnection"].ToString();
            var _name = defaultDbConnection;
            if (DbSchemaConfiguration.Configs.ContainsKey(model.Name))
            {
                var schemaConfig = DbSchemaConfiguration.Configs[model.Name];
                _name = schemaConfig.DatabaseConnectionName;
            }
            _connectionNamesCache.Add(model.Name, _name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        protected static void InitTableNamesCache(Type model)
        {
            var tna = model.GetCustomAttribute<PetaPoco.TableNameAttribute>();
            if (tna != null)
            {
                _tableNamesCache.Add(model.Name, tna.Value);
                _tableToModelNamesCache.Add(tna.Value, model.Name);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected static void ReflectionAssignModelsTableName()
        {
            GetAllModels().ForEach(model =>
            {
                var tna = model.GetCustomAttribute<PetaPoco.TableNameAttribute>();
                if (tna != null)
                {
                    var field = model.GetProperty("TableName", BindingFlags.Static
                        | BindingFlags.FlattenHierarchy
                        | BindingFlags.Public
                        | BindingFlags.NonPublic);
                    field.SetValue(model, tna.Value);
                }

                /*          
                model.GetProperty
                (
                    "TableName",
                    BindingFlags.FlattenHierarchy |
                    BindingFlags.Public |
                    BindingFlags.Static
                ).SetValue(null, tna.Value);
                */
            });
        }

        /// <summary>
        /// 
        /// </summary>
        protected static void ReflectionAssignModelsConnectionName()
        {
            var defaultDbConnection = ConfigurationManager.AppSettings["defaultDbConnection"].ToString();
            GetAllModels().ForEach(model =>
            {
                var _name = defaultDbConnection;
                if (DbSchemaConfiguration.Configs.ContainsKey(model.Name))
                {
                    var schemaConfig = DbSchemaConfiguration.Configs[model.Name];
                    _name = schemaConfig.DatabaseConnectionName;
                }

                var field = model.GetProperty("ConnectionName", BindingFlags.Static
                   | BindingFlags.FlattenHierarchy
                   | BindingFlags.Public
                   | BindingFlags.NonPublic);
                field.SetValue(null, _name);
            });
        }

        //*
        public static IBroccoliDatabase GetDatabaseConnection(string connectionStringName)
        {
            return new BroccoliDatabase(connectionStringName) { EnableAutoSelect = false };
        }

        //* 
        public static string GenerateIntermediateTable(string thisTable, string thatTable)
        {
            try
            {
                var foreignTable = _foreignKeyGenerator.GenerateIntermediateTable(thisTable, thatTable);

                if (!_dbAllTablesCache.ContainsValue(foreignTable))
                {
                    foreignTable = _foreignKeyGenerator.GenerateIntermediateTable(thatTable, thisTable);
                }

                if (!_dbAllTablesCache.ContainsValue(foreignTable))
                {
                    foreignTable = null;
                }
                return foreignTable;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //*
        public static string GenerateOnClauseForeignKey(string thisTable, string thatTable)
        {
            return _foreignKeyGenerator.GenerateOnClauseForeignKey(thisTable, thatTable);
        }

    }
}
