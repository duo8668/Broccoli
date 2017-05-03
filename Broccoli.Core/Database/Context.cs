using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Database
{
    public class Context
    {
        public Context(string cs, bool migrate = false, bool log = false, bool inject = true)
        {

        }
        public static void InitEloquent()
        {
            //* First thing to think, do we inject the whole schema in

            //* Secondly, where do we fill in relationships? Here or 
        }
    }
}
