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
using System.Threading.Tasks;

namespace Broccoli.Core.Facade
{
    public class DbFacade : Facade
    {
        //* Storage for all models
        private static HashSet<Type> _AllModels;

        // Handling foreign key
        private static ForeignKeyGenerator _foreignKeyGenerator;

        //* All tables cache
        private static Dictionary<string, string> _tableNamesCache = new Dictionary<string, string>();

        public static void Initialize()
        {
            ForeignKeyGenerator.InitGenerator("__");
            _foreignKeyGenerator = new ForeignKeyGenerator();

            ReflectionAssignModelsConnectionName();
            ReflectionAssignModelsTableName();
            InitTableNamesCache();
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

        protected static void InitTableNamesCache()
        {
            var schemaConfigs = DbSchemaConfiguration.Deserialize("ModelSchema.config");
            var defaultDbConnection = ConfigurationManager.AppSettings["defaultDbConnection"].ToString();

            //* define a holder to store all db connection exists
            List<string> _listOfDbConnNamesToLoad = new List<string>();

            //* add the default one to load
            _listOfDbConnNamesToLoad.Add(defaultDbConnection);

            //* check for additional names to load
            if (schemaConfigs.Count() > 0)
            {
                var items = schemaConfigs.Select(kvp => kvp.Value).ToList();
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
                    if (!_tableNamesCache.ContainsKey(specialKey))
                    {
                        _tableNamesCache.Add(specialKey, item.TABLE_NAME);
                    }
                }
            }
        }

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
                    field.SetValue(null, tna.Value);
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

        protected static void ReflectionAssignModelsConnectionName()
        {
            var schemaConfigs = DbSchemaConfiguration.Deserialize("ModelSchema.config");
            var defaultDbConnection = ConfigurationManager.AppSettings["defaultDbConnection"].ToString();
            GetAllModels().ForEach(model =>
            {
                var _name = defaultDbConnection;
                if (schemaConfigs.ContainsKey(model.Name))
                {
                    var schemaConfig = schemaConfigs[model.Name];
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
            return new BroccoliDatabase(connectionStringName);
        }

        //* 
        public static string GenerateIntermediateTable(string thisTable, string thatTable)
        {
            var foreignTable = _foreignKeyGenerator.GenerateIntermediateTable(thisTable, thatTable);

            if (!_tableNamesCache.ContainsValue(foreignTable))
            {
                foreignTable = _foreignKeyGenerator.GenerateIntermediateTable(thatTable, thisTable);
            }

            if (!_tableNamesCache.ContainsValue(foreignTable))
            {
                foreignTable = null;
            }
            return foreignTable;
        }

        public static string GenerateOnClauseForForeignKey(string thisTable, string thatTable)
        {
            return _foreignKeyGenerator.GenerateOnClauseForForeignKey(thisTable, thatTable);
        }
    }
}
