using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeAtChVotebot
{
    public static class Extensions
    {
        public static List<string> Copy(this List<string> list)
        {
            var l = new List<string>();
            foreach (var i in list) l.Add(i);
            return l;
        }
    }
}
