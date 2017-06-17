using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Events
{
    public class ModelChangedEventArgs<TModel> : EventArgs
    {
        TModel _model;

        public dynamic dynamicObjects { get; set; }

        public TModel Model { get { return _model; } }

        public ModelChangedEventArgs()
        {

        }
        public ModelChangedEventArgs(TModel _m)
        {
            _model = _m;
        }
    }
}
