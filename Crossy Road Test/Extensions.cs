using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtensionMethods
{
    public static class Extensions
    {
        public static int encrypt(this int num) => (num + 0x4E876) ^ 0x4E876;

        public static int decrypt(this int num) => (num ^ 0x4E876) - 0x4E876;
    }
}
