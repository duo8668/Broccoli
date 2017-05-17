using Broccoli.Core.Configuration;
using Broccoli.Core.Database.Eloquent;
using Broccoli.Core.Extensions;
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
        private static int _maxConnection = 100;
        private static ConcurrentDictionary<string, ConcurrentQueue<PetaPoco.IDatabase>> _dbConnPool;
        private static HashSet<Type> _AllModels;

        /*
         * 
         */
        public static int GetAvailableConnections(string provider)
        {
            if (_dbConnPool != null)
            {
                ConcurrentQueue<PetaPoco.IDatabase> tmp;
                if (_dbConnPool.TryGetValue(provider, out tmp))
                {
                    return tmp.Count();
                }
            }
            return 0;
        }

        /*
         * 
         */
        public static bool HasAvailableConnection(string provider)
        {
            return GetAvailableConnections(provider) > 0;
        }

        /*
         *
         */
        public static void Initialize()
        {
            /*
            _dbConnPool = new ConcurrentDictionary<string, ConcurrentQueue<PetaPoco.IDatabase>>();

            //* Here will need to ensure another hash key collection array to contains all the md5 hash value
            //* The key here will update to the Model's DB HashKey connection and there after, avaiable resources will be retrieved through here
            //* This given flexibility of different database required by diff model in future
            var connStrings = ConfigurationManager.ConnectionStrings;

            foreach (ConnectionStringSettings connString in connStrings)
            {
                var _connQueue = new ConcurrentQueue<PetaPoco.IDatabase>();
                for (int i = 0; i < _maxConnection; i++)
                {
                    var db = DatabaseConfiguration.Build()
                              .UsingConnectionStringName(connString.Name)
                              .UsingDefaultMapper<ConventionMapper>(m =>
                              {
                                  // Produces order_line
                                  m.InflectTableName = (inflector, tn) => inflector.Underscore(tn);
                                  // Produces order_line_id
                                  m.InflectColumnName = (inflector, cn) => inflector.Underscore(cn);
                              })
                              .Create();
                    db.KeepConnectionAlive = true;
                    _connQueue.Enqueue(db);
                }
                _dbConnPool.TryAdd(connString.Name, _connQueue);
            }
            */
            ReflectionAssignModelsConnectionName();
            ReflectionAssignModelsTableName();
        }


        public static HashSet<Type> GetAllModels()
        {
            if (_AllModels != null) return _AllModels;

            _AllModels = new HashSet<Type>();

            AppDomain.CurrentDomain.GetAssemblies().ToList().ForEach(assembly =>
            {
                assembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(Model)))
                .Where(type => type.IsPublic)
                .Where(type => !type.ContainsGenericParameters)
                .ToList().ForEach(type => _AllModels.Add(type));
            });

            return _AllModels;
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
        //*
        public static PetaPoco.IDatabase GetDatabaseConnection(string connectionStringName)
        {
            return new PetaPoco.Database(connectionStringName: connectionStringName);
        }

        public static void ReturnDatabaseConnection(string provider, PetaPoco.IDatabase db)
        {
            ConcurrentQueue<PetaPoco.IDatabase> tmp;
            if (_dbConnPool != null)
            {
                if (_dbConnPool.TryGetValue(provider, out tmp))
                {
                    tmp.Enqueue(db);
                }
            }
        }

    }
}
