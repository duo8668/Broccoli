using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Eloquent
{
    interface IModel<TModel>
    {
        TModel Find(string primaryKey, bool withTrashed = false);
    }
}
