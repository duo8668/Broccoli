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
        [PetaPoco.Column("cust_name")]
        public string CustomerName { get; set; }

        [PetaPoco.Column("cust_firstname")]
        public string FirstName { get; set; }
    }
}
