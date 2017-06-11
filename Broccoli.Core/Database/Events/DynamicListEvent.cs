using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Events
{

    public class DynamicListEvent
    {
        public static object _listeningLock = new object();

        public static void triggerDynamicListListening<T>(IEnumerable<T> list, bool triggerChangeEvent = false)
        {
            Monitor.Enter(_listeningLock);
            if (triggerChangeEvent)
            {
                dynamic bindingList = Activator.CreateInstance
                 (
                     typeof(BindingList<>).MakeGenericType
                     (
                         list.GetType().GenericTypeArguments[0]
                     ),
                     new object[] { list }
                 );

                bindingList.ListChanged += new ListChangedEventHandler
                (
                    (sender, e) =>
                    {
                        if (!triggerChangeEvent) return;

                        switch (e.ListChangedType)
                        {
                            case ListChangedType.ItemAdded:
                            case ListChangedType.ItemDeleted:
                                {
                                    // this.FirePropertyChanged(prop);

                                }
                                break;
                        }
                    }
                );
            }
            Monitor.Exit(_listeningLock);
        }
    }
}
