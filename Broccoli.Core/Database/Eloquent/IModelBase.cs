﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        T Get<T>([CallerMemberName] string propName = "", bool loadFromDb = true, bool isAList = false);
        void Set<T>(T value, [CallerMemberName] string propName = "", bool triggerChangeEvent = true, bool isAList = false);
        void FirePropertyChanged(PetaPoco.PocoColumn prop);
    }
}
