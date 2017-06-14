using Broccoli.Core.Database.Eloquent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Database
{
    public interface IBroccoliDatabase : PetaPoco.IDatabase
    {
        bool KeepConnectionAlive { get; set; }

        PetaPoco.PocoData GetPocoDataForType(Type type);
        PetaPoco.PocoData GetPocoDataForObject(object poco);

        Task<IEnumerable<T>> BroccoQuery<T>(string sql, params object[] args) where T : Model<T>, new();

    }
}
