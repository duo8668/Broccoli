using Broccoli.Core.Database.Eloquent;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Entities
{
    [PetaPoco.TableName("sales__invoice")]
    [PetaPoco.ExplicitColumns]
    public class Invoice : Model<Invoice>
    {
        public Invoice()
        {

        }

        [PetaPoco.Column("invoice_num")]
        public string InvoiceNum
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

        [PetaPoco.Column("invoice_datetime")]
        public DateTime? InvoiceDateTime
        {
            get
            {
                return Get<DateTime?>();
            }
            set
            {
                Set<DateTime?>(value);
            }
        }

    }
}
