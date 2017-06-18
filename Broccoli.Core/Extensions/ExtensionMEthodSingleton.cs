using Broccoli.Core.Database.Eloquent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Extensions
{
    public class ExtensionMethodSingleton
    {
        private ExtensionMethodSingleton()
        {

        }

        public static MethodInfo GetIEnumerableSaveMethod()
        {
            return typeof(ModelExtensionMethods).GetMethod("Save");
        }

        public static string GetIEnumerableSaveMethodName(MethodInfo _m = null)
        {
            if (_m == null)
            {
                _m = GetIEnumerableSaveMethod();
            }

            return _m.ReflectedType.FullName + "_" + _m.Name;
        }

    }
}
