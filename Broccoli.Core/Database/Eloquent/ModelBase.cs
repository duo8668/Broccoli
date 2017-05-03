using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Eloquent
{
    public abstract class ModelBase: IModelBase
    {
        protected int _recordsPerPage;

        [DataType(DataType.Text)]
        [PetaPoco.Column]
        public string record_info { get; set; }

        [DataType(DataType.DateTime)]
        [PetaPoco.Column]
        public DateTime created_at { get; set; }

        [DataType(DataType.DateTime)]
        [PetaPoco.Column]
        public DateTime modified_at { get; set; }

        [DataType(DataType.DateTime)]
        [PetaPoco.Column]
        public DateTime? deleted_at { get; set; }

        public static void Init()
        {

        }
    }
}
