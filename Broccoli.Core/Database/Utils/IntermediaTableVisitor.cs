using Broccoli.Core.Database.Dynamic;
using Broccoli.Core.Database.Eloquent;
using Broccoli.Core.Facade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Utils
{

    public class IntermediaTableVisitor
    {
        public string CurrentModelName { get; protected set; }
        public string RelativeModelName { get; protected set; }
        public string IntermediateModelName { get; protected set; }
        public dynamic IntermediateModel { get; protected set; } 

        public void Visit(dynamic current, dynamic relative) 
        {
            //* Get this table and that table name
            var currentModel = ModelBase.Dynamic(current);
            var relativeModel = ModelBase.Dynamic(relative);

            //* set modelname
            CurrentModelName = currentModel.ModelName;
            RelativeModelName = relativeModel.ModelName;

            //* find intermediate model name
            IntermediateModelName = ModelFacade.GenerateIntermediateModelName(CurrentModelName, RelativeModelName);
            //* Initialize target model
            IntermediateModel = ModelBase.Dynamic(ModelFacade.GenerateIntermediateModel(IntermediateModelName));

            var currModelId = currentModel.Id;
            var relativeModelId = relativeModel.Id;

            IntermediateModel.Set(currModelId, CurrentModelName + "Id");
            IntermediateModel.Set(relativeModelId, RelativeModelName + "Id");

            var firstCol = DbFacade.PocoDatas[IntermediateModelName].GetColumnName(CurrentModelName + "Id");
            var secondCol = DbFacade.PocoDatas[IntermediateModelName].GetColumnName(RelativeModelName + "Id");

            IntermediateModel = IntermediateModel.Find(string.Format(" {0}.{1}={3} AND {0}.{2}={4} "
                , IntermediateModel.TableName
                , firstCol, secondCol
                , currModelId
                , relativeModelId));

        }
    }
}
