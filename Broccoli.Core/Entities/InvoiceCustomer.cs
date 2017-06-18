using Broccoli.Core.Database.Eloquent;

namespace Broccoli.Core.Entities
{
    [PetaPoco.TableName("invoice__customer")]
    [PetaPoco.ExplicitColumns]
    public class InvoiceCustomer : Model<InvoiceCustomer>
    {
        [PetaPoco.Column("invoiceId")]
        public long InvoiceId { get; set; }

        [PetaPoco.Column("customerId")]
        public long CustomerId { get; set; }
    }
}
