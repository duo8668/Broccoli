using Broccoli.Core.Database.Eloquent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Entities
{
    [PetaPoco.TableName("sales_invoice")]
    [PetaPoco.ExplicitColumns]
    public class Invoice : Model<Invoice>
    {
        [PetaPoco.Column] public string invoice_num { get; set; }
        [PetaPoco.Column] public DateTime? invoice_datetime { get; set; }

    }
}
