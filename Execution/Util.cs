using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SqlUtils
{
    internal class Util
    {
        internal static string FormatBytesAsHex(byte[] bytes)
        {
            if ((bytes == null) || (bytes.Length == 0))
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder(bytes.Length * 2);
            foreach (byte num in bytes)
            {
                builder.Append(num.ToString("X2"));
            }
            return builder.ToString();
        }

        internal static IList MergeLists(IList list1, IList list2, IComparer comparer)
        {
            List<string> list = new List<string>();
            if ((list1 != null) || (list1.Count > 0))
            {
                foreach (string str in list1)
                {
                    list.Add(str);
                }
            }
            if ((list2 != null) && (list2.Count > 0))
            {
                foreach (string str2 in list2)
                {
                    bool flag = false;
                    foreach (string str3 in list)
                    {
                        if (comparer.Compare(str2, str3) == 0)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        list.Add(str2);
                    }
                }
            }
            return list;
        }

        internal static int NumberOfDigits(int number)
        {
            return (int) (Math.Truncate(Math.Log10((double) number)) + 1.0);
        }
    }
}

