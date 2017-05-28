using Broccoli.Core.Database.Eloquent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Entities.Core
{
    [PetaPoco.ExplicitColumns]
    public class InformationSchema : Model<InformationSchema>
    {
        [PetaPoco.Column("TABLE_SCHEMA")]
        public string TABLE_SCHEMA { get; set; }

        [PetaPoco.Column("TABLE_NAME")]
        public string TABLE_NAME { get; set; }
    }
}
