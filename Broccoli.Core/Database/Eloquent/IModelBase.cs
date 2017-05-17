using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Eloquent
{
    public interface IModelBase
    { 
        string RecordInfo { get; set; }         
        DateTime CreatedAt { get; set; }         
        DateTime ModifiedAt { get; set; }         
        DateTime? DeletedAt { get; set; }         
    }
}
