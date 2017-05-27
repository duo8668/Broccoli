using Broccoli.Core.Configuration;
using Broccoli.Core.Database.Eloquent;
using Broccoli.Core.Database.Utils;
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
        private static HashSet<Type> _AllModels;

        /*
         *
         */
        public static void Initialize()
        {
            ForeignKeyGenerator.InitGenerator("__");
            ReflectionAssignModelsConnectionName();
            ReflectionAssignModelsTableName();
            ReflectionAssignGenerators();
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

        protected static void ReflectionAssignModelsTableName()
        {
            GetAllModels().ForEach(model =>
            {
                var tna = model.GetCustomAttribute<PetaPoco.TableNameAttribute>();
                var field = model.GetField("TableName", BindingFlags.Static
                  | BindingFlags.FlattenHierarchy
                  | BindingFlags.Public
                  | BindingFlags.NonPublic);
                field.SetValue(null, tna.Value);
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

                var field = model.GetField("ConnectionName", BindingFlags.Static
                   | BindingFlags.FlattenHierarchy
                   | BindingFlags.Public
                   | BindingFlags.NonPublic);
                field.SetValue(null, _name);
            });
        }

        protected static void ReflectionAssignGenerators()
        {
            /*
            ForeignKeyGenerator fkg = new ForeignKeyGenerator();
          
            GetAllModels().ForEach(model =>
            {
                var field = model.GetField("_foreignKeyGenerator", BindingFlags.FlattenHierarchy
                  | BindingFlags.Instance
                  | BindingFlags.NonPublic);
                field.SetValue(null, fkg);
                
            });
        */
        }
        //*
        public static PetaPoco.IDatabase GetDatabaseConnection(string connectionStringName)
        {
            return new PetaPoco.Database(connectionStringName: connectionStringName);
        }
    }
}
