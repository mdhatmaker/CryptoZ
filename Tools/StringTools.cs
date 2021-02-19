using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CryptoZ.Tools
{
    public static class StringTools
    {

    } // class


    public class CompareIgnoreCase : IEqualityComparer<string>
    {
        private static CompareIgnoreCase _instance;

        public static CompareIgnoreCase Instance
        {
            get => _instance ?? (_instance = new CompareIgnoreCase());
            /*{
                if (_instance == null) _instance = new CompareIgnoreCase();
                return _instance;
            }*/
        }
       
        public bool Equals([AllowNull] string x, [AllowNull] string y)
        {
            return (string.Compare(x, y, true) == 0);
        }

        public int GetHashCode([DisallowNull] string obj)
        {
            return obj.GetHashCode();
        }
    }


} // namespace
