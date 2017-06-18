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

        public static string GenerateIntermediateModelName(string thisModel, string thatModel)
        {
            if (DbFacade.DynamicModels.ContainsKey(string.Concat(thisModel, thatModel)))
            {
                return string.Concat(thisModel, thatModel);
            }
            else if (DbFacade.DynamicModels.ContainsKey(string.Concat(thatModel, thisModel)))
            {
                return string.Concat(thatModel, thisModel);
            }
            else
            {
                return null;
            }
        }

        public static dynamic GenerateIntermediateModel(string model)
        {
            dynamic ret = Activator.CreateInstance
                (
                DbFacade.DynamicModels[model].ModelType
                );

            return ret;
        }
    }
}
