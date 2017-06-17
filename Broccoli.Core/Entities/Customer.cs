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
        public Customer()
        {

        }

        [PetaPoco.Column("firstname")]
        public string FirstName
        {
            get
            {
                return Get<string>();
            }
            set
            {
                Set<string>(value);
            }
        }

        [PetaPoco.Column("lastname")]
        public string LastName
        {
            get
            {
                return Get<string>();
            }
            set
            {
                Set<string>(value);
            }
        }
    }
}
