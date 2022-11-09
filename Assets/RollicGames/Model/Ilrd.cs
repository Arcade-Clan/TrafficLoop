using System;

namespace ElephantSDK
{
    [Serializable]
    public class Ilrd
    {
        public string auctionId;
        public string adUnit;
        public string country;
        public string ab;
        public string segmentName;
        public string adNetwork;
        public string instanceName;
        public string instanceId;
        public double? revenue;
        public string precision;
        public double? lifetimeRevenue;
        public string encryptedCPM;
        
        public Ilrd(IronSourceAdInfo adInfo)
        {
            this.revenue = adInfo.revenue;
            this.auctionId = adInfo.auctionId;
            this.country = adInfo.country;
            this.ab = adInfo.ab;
            this.segmentName = adInfo.segmentName;
            this.adNetwork = adInfo.instanceName;
            this.instanceId = adInfo.instanceId;
            this.revenue = adInfo.revenue;
            this.precision = adInfo.precision;
            this.lifetimeRevenue = adInfo.lifetimeRevenue;
            this.encryptedCPM = adInfo.encryptedCPM;
        }
    }
}