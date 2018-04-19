using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAL_Synchronizer.Model
{
    public enum AnimeStatus
    {
        Watching,
        Completed,
        OnHold,
        Dropped,
        PlanToWatch
    }
    public class Anime
    {
        public string Title { get; set; }
        public AnimeStatus Status { get; set; }
        public int OverallScore { get; set; } = -1;
        public int Progress { get; set; }
        public string Url { get; set; }
    }
}
