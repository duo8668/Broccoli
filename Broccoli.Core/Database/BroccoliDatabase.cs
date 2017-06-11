using PetaPoco;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Broccoli.Core.Database
{
    public class BroccoliDatabase : PetaPoco.Database, IBroccoliDatabase
    {
        // Member variables
        public BroccoliDatabase(string connectionStringName) : base(connectionStringName)
        {
        }

        public PocoData GetPocoDataForType(Type t)
        {
            return PocoData.ForType(t, DefaultMapper);
        }

        public PocoData GetPocoDataForObject(object poco)
        {
            var pd = GetPocoDataForType(poco.GetType());
            return PocoData.ForObject(poco, pd.TableInfo.PrimaryKey, DefaultMapper);
        }

    }
}
