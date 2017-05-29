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
        public static char[] _identitySeparator;

        public ForeignKeyGenerator()
        {

        }

        public static void InitGenerator(string _separator)
        {
            IdentitySeparator = _separator;
            _identitySeparator = _separator.ToArray();
        }

        public virtual string GenerateIntermediateTable(string thisTable, string thatTable)
        {
            var thisTablesSplit = thisTable.Split(_identitySeparator);
            var thatTablesSplit = thatTable.Split(_identitySeparator);
            var len1 = thisTablesSplit.Length;
            var len2 = thatTablesSplit.Length;
            if (len1 > 1 && len2 > 1)
            {
                return string.Concat(thisTablesSplit[len1--], IdentitySeparator, thatTablesSplit[len2--]);
            }
            else
            {
                return null;
            }
        }

        public virtual string GenerateOnClauseForForeignKey(string thisTable, string thatTable)
        {
            var thisTablesSplit = thisTable.Split(_identitySeparator);
            var thatTablesSplit = thatTable.Split(_identitySeparator);
            var len1 = thisTablesSplit.Length;
            var len2 = thatTablesSplit.Length;
            if (len1 > 1 && len2 > 1)
            {
                return string.Concat(thisTable, ".id", "=", thatTable, ".", thisTablesSplit[len1--], "Id");
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
