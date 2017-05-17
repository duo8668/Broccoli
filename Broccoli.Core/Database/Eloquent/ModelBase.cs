using Newtonsoft.Json;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Eloquent
{
    public abstract class ModelBase : IModelBase
    {
        protected static string ConnectionHash;
        protected static string ConnectionName;
        protected static string TableName;

        private List<object> _DiscoveredEntities;

        public ModelBase()
        {
          
        }

        [JsonIgnore]
        [PetaPoco.Ignore]
        public List<object> DiscoveredEntities
        {
            get
            {
                if (this._DiscoveredEntities == null)
                {
                    // Add ourselves to the discovered list.
                    this._DiscoveredEntities = new List<object> { this };
                }

                return this._DiscoveredEntities;
            }

            set
            {
                this._DiscoveredEntities = value;
            }
        }

        [PetaPoco.Column]
        public long id { get; set; }
        
        [PetaPoco.Column("record_info")]
        public string RecordInfo { get; set; }
        
        [PetaPoco.Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [PetaPoco.Column("modified_at")]
        public DateTime ModifiedAt { get; set; }
        
        [PetaPoco.Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }
}
