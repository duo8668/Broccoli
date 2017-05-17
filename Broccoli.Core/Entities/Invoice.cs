using Broccoli.Core.Database.Eloquent;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Entities
{
    [PetaPoco.TableName("sales_invoice")]
    [PetaPoco.ExplicitColumns]
    public class Invoice : Model<Invoice>
    { 
        public Invoice()
        {

        }

        [PetaPoco.Column("invoice_num")]
        public string InvoiceNum { get; set; }
        [PetaPoco.Column("invoice_datetime")]
        public DateTime? InvoiceDateTime { get; set; }

        //* Extended Property
        public List<Customer> Customers
        {
            get
            {
                return Get<Customer>();
            }
        }

    }
}
