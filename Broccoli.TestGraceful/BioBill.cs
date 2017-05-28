using Graceful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.TestGraceful
{
    [SqlTableName("BioBill")]
    public class BioBill : Model<BioBill>
    {
        public BioBill()
        {

        }

        public string BillNumber { get { return Get<string>(); } set { Set(value); } }

        public DateTime AppDate { get { return Get<DateTime>(); } set { Set(value); } }
        public int CustomerID { get { return Get<int>(); } set { Set(value); } }
        public double TotalAmount { get { return Get<double>(); } set { Set(value); } }
        public double NetAmount { get { return Get<double>(); } set { Set(value); } }
        public double CollectedAMT { get { return Get<double>(); } set { Set(value); } }

        public IList<BioBillDetail> BioBillDetails { get { return Get<IList<BioBillDetail>>(loadFromDiscovered:false); } set { Set(value); } }
    }
}
