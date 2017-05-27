using Broccoli.Core.Database.Eloquent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Entities
{
    [PetaPoco.TableName("customer__customer")]
    [PetaPoco.ExplicitColumns]
    public class Customer : Model<Customer>
    {
        [PetaPoco.Column("firstname")]
        public string FirstName { get; set; }

        [PetaPoco.Column("lastname")]
        public string LastName { get; set; }
    }
}
