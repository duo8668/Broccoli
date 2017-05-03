using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Eloquent
{
    public interface IModelBase
    {
        DateTime created_at { get; set; }
        DateTime modified_at { get; set; }
        DateTime? deleted_at { get; set; }
    }
}
