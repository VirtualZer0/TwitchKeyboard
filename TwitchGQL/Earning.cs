using System.Collections.Generic; 
namespace TwitchGQL{ 

    public class Earning
    {
        public string id { get; set; }
        public int averagePointsPerHour { get; set; }
        public int cheerPoints { get; set; }
        public int claimPoints { get; set; }
        public int followPoints { get; set; }
        public int passiveWatchPoints { get; set; }
        public int raidPoints { get; set; }
        public int subscriptionGiftPoints { get; set; }
        public List<WatchStreakPoint> watchStreakPoints { get; set; }
        public List<Multiplier> multipliers { get; set; }
        public string __typename { get; set; }
    }

}