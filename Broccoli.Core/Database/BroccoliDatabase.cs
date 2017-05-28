using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetaPoco;

namespace Broccoli.Core.Database
{
    public class BroccoliDatabase:PetaPoco.Database,IBroccoliDatabase
    {
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
