using Broccoli.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.POC
{

    public class BroccoDbTester
    {
        public void hasManyPerformanceTest(Invoice search)
        {
            //var custs = search.hasMany<Customer>((cccc) => cccc.FirstName == "%a%", true).ToList();
            var custs = search.hasMany<Customer>((inv, cust) => inv.Id == cust.Id,null,false).ToList();
        }

        public Invoice pureQueryPerformancetest()
        {
            return Invoice.Find((myInv) => myInv.InvoiceNum == "INV-222222");
            // return DbFacade.GetDatabaseConnection(Invoice.ConnectionName).Query<Invoice>(@"select * from sales__invoice WHERE invoice_num='INV-222222'").SingleOrDefault();
            // return Invoice.Find((lin) => lin.Where((myInv) => myInv.InvoiceNum == "INV-33333"));
        }

        public void pureInsertPerformanceTest()
        {
            var invToAdd = new Invoice();
            invToAdd.InvoiceNum = "INV-555555";
            invToAdd.InvoiceDateTime = DateTime.Parse("2017-06-01 15:22");
            invToAdd.Save();
        }

        public void pureUpdatePerformanceTest(Invoice search)
        {
            search.InvoiceNum = "INV-333333";
            search.Save();
            var stringTest = search.InvoiceNum;
            search.InvoiceNum = "INV-222222";
            search.Save();
        }

        public void explicitlySavePerformanceTest(Invoice search)
        {
            // var custs = search.hasMany<Customer>((cccc) => cccc.FirstName == "%a%", true);

            foreach (var cust in search.Customers)
            {
                cust.LastName += "_";
            }
            search.Save();
            foreach (var cust in search.Customers)
            {
                cust.LastName = cust.LastName.Replace("_", "");
            }
            //    search.Save();
        }
    }
}
