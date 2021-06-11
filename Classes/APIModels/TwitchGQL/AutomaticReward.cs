using System; 
namespace Classes.APIModels.TwitchGQL{ 

    public class AutomaticReward
    {
        public string id { get; set; }
        public string backgroundColor { get; set; }
        public int? cost { get; set; }
        public string defaultBackgroundColor { get; set; }
        public int defaultCost { get; set; }
        public DefaultImage defaultImage { get; set; }
        public object image { get; set; }
        public bool isEnabled { get; set; }
        public bool isHiddenForSubs { get; set; }
        public int minimumCost { get; set; }
        public string type { get; set; }
        public string updatedForIndicatorAt { get; set; }
        public DateTime globallyUpdatedForIndicatorAt { get; set; }
        public string __typename { get; set; }
    }

}