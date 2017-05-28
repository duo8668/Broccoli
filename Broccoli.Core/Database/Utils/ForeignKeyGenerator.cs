using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.Core.Database.Utils
{
    public class ForeignKeyGenerator : IForeignKeyGenerator
    {
        public static string IdentitySeparator { get; protected set; }

        public ForeignKeyGenerator()
        {

        }

        public static void InitGenerator(string _separator)
        {
            IdentitySeparator = _separator;
        }

        public string GenerateIntermediateTable(string thisTable, string thatTable)
        {
            var thisTablesSplit = thisTable.Split(IdentitySeparator.ToCharArray());
            var thatTablesSplit = thatTable.Split(IdentitySeparator.ToCharArray());

            if (thisTablesSplit.Length > 1 && thatTablesSplit.Length > 1)
            {
                return string.Concat(thisTablesSplit[thisTablesSplit.Length - 1], IdentitySeparator, thatTablesSplit[thatTablesSplit.Length - 1]);
            }
            else
            {
                return null;
            }
        }

        public string GenerateOnClauseForForeignKey(string thisTable, string thatTable)
        {
            var thisTablesSplit = thisTable.Split(IdentitySeparator.ToCharArray());
            var thatTablesSplit = thatTable.Split(IdentitySeparator.ToCharArray());

            if (thisTablesSplit.Length > 1 && thatTablesSplit.Length > 1)
            {
                return string.Concat(thisTable, ".id", "=", thatTable, ".", thisTablesSplit[thisTablesSplit.Length - 1], "Id");
            }
            else
            {
                return null;
            }
        }
    }

    public interface IForeignKeyGenerator
    {
        string GenerateIntermediateTable(string thisTable, string thatTable);
        string GenerateOnClauseForForeignKey(string thisTable, string thatTable);
    }
}
