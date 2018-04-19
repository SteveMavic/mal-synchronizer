using MAL_Synchronizer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAL_Synchronizer.Comparers
{
    class AnimeComparer : IEqualityComparer<Anime>
    {
        public bool Equals(Anime x, Anime y)
        {
            string firstTitle = x.Title.ToLower();
            string secondTitle = y.Title.ToLower();
            return firstTitle.Equals(secondTitle);
        }

        public int GetHashCode(Anime obj)
        {
            return 0;
        }
    }
}
