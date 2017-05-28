using Graceful;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.TestGraceful
{
    [SqlTableName("BioBillDetail")]
    public class BioBillDetail : Model<BioBillDetail>
    {
        public string BillNumber { get { return Get<string>(); } set { Set(value); } }
        public string TranType { get { return Get<string>(); } set { Set(value); } }
        public string ID { get { return Get<string>(); } set { Set(value); } }
        public double QTY { get { return Get<double>(); } set { Set(value); } }
        public double Price { get { return Get<double>(); } set { Set(value); } }
        public string BranchCode { get { return Get<string>(); } set { Set(value); } }
        public string PackageID { get { return Get<string>(); } set { Set(value); } }
        public double RealPrice { get { return Get<double>(); } set { Set(value); } }
        public double PaidPrice { get { return Get<double>(); } set { Set(value); } }


        public BioBill BioBill { get { return Get<BioBill>(); } set { Set(value); } }
    }
}
