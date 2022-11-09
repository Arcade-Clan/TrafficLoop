using System.Runtime.InteropServices;

namespace RollicGames.Advertisements
{
    public class RollicAdsIos
    {
#if UNITY_IOS
        [DllImport ("__Internal")]
        public static extern void updateConversionValue(int value);
        
        [DllImport ("__Internal")]
        public static extern void setTrackingEnabled(bool isEnabled);

#endif
    }
}