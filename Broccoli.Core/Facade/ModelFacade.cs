using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Broccoli.Core.Configuration;

namespace Broccoli.Core.Facade
{

    public class ModelFacade
    {
        public static void LoadClassConfig()
        {
            var c2 = DbSchemaConfiguration.Deserialize("ModelSchema.config");
        }
    }
}
