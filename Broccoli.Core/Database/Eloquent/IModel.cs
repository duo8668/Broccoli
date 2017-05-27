using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Eloquent
{
    interface IModel
    { 
    }
    interface IModel<TModel>: IModel
    {
        TModel Find(string primaryKey, bool withTrashed = false);
    }
}
