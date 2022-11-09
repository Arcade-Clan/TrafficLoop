using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using com.adjust.sdk;
using ElephantSDK;
using RollicGames.Advertisements.Ads;
#if UNITY_IOS && !UNITY_EDITOR
using UnityEngine.iOS;
#endif


namespace RollicGames.Advertisements
{
    /**
     * DO NOT MODIFY THIS FILE!
     */
    public class RLAdvertisementManager : MonoBehaviour
    {
        public static event Action OnRollicAdsSdkInitializedEvent;
        public static event Action OnRollicAdsAdLoadedEvent;
        public static event Action<string> OnRollicAdsAdFailedEvent;
        public static event Action OnRollicAdsAdClickedEvent;
        public static event Action<string> OnRollicAdsAdExpandedEvent;
        public static event Action<string> OnRollicAdsAdCollapsedEvent;
        public static event Action<string> OnRollicAdsInterstitialLoadedEvent;
        public static event Action<IronSourceError> OnRollicAdsInterstitialFailedEvent;
        public static event Action OnRollicAdsInterstitialDismissedEvent;
        public static event Action<string> OnRollicAdsInterstitialExpiredEvent;
        public static event Action OnRollicAdsInterstitialShownEvent;
        public static event Action OnRollicAdsInterstitialClickedEvent;
        public static event Action<string> OnRollicAdsRewardedVideoLoadedEvent;
        public static event Action<string, string> OnRollicAdsRewardedVideoFailedEvent;
        public static event Action OnRollicAdsRewardedVideoShownEvent;
        public static event Action OnRollicAdsRewardedVideoClickedEvent;
        public static event Action OnRollicAdsRewardedVideoFailedToPlayEvent;
        public static event Action<IronSourcePlacement> OnRollicAdsRewardedVideoReceivedRewardEvent;
        public static event Action OnRollicAdsRewardedVideoClosedEvent;
        public static event Action<string> OnRollicAdsRewardedVideoLeavingApplicationEvent;
        
        
        private static RLAdvertisementManager instance = null;
        private bool isRewardAvailable = false;
        private bool isMediationInitialized;
        private bool hasInitMediationStarted = false;

        public Action<RLRewardedAdResult> rewardedAdResultCallback { get; set; }
        public Action onInterstitialAdClosedEvent;
        public Action onInterstitialAdOpenedEvent;

        private string _appKey;

        private string backUpInterstitialAdUnit = "";
        private string backUpRewardedVideoAdUnit = "";

        private bool _isInterstitialReady = false;
        private bool _isBackUpInterstitialReady = false;
        
        private int bannerRequestTimerIndex = 0;
        private int interstitialRequestTimerIndex = 0;
        private int backUpInterstitialRequestTimerIndex = 0;
        private int rewardedRequestTimerIndex = 0;
        private int backUpRewardedRequestTimerIndex = 0;
        private List<int> timers;
        private List<string> defaultTimerList = new List<string>{"2","4","8","16"};
        
        public static RLAdvertisementManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<RLAdvertisementManager>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(RLAdvertisementManager).Name;
                        instance = obj.AddComponent<RLAdvertisementManager>();
                    }


#if UNITY_IOS && !UNITY_EDITOR
                    if (RemoteConfig.GetInstance().IsFirstOpen())
                    {
                        try
                        {
                            var iOSSystemVersion = Device.systemVersion;
                            double returnVal = double.TryParse(iOSSystemVersion,NumberStyles.Any, CultureInfo.InvariantCulture, out returnVal) ? returnVal : 0.0;
                            if (returnVal >= 14f)
                            {
                                Elephant.Event("first_open_conversion_value_event", -1);
                            }
                        }
                        catch (Exception e)
                        {
                           
                        }
                        RollicAdsIos.updateConversionValue(0);
                    }
#endif
                }

                return instance;
            }
        }
        
        void Awake()
        {
            if (instance == null)
            {
                instance = this as RLAdvertisementManager;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void init(string appKey)
        {
            this._appKey = appKey;
            
            var timerStringList = AdConfig.GetInstance().GetList("retry_periods", defaultTimerList);
            
            timers = timerStringList
                .Select(s => Int32.TryParse(s, out int n) ? n : 0)
                .ToList();
        }

        void Start()
        {
            if (ElephantCore.Instance != null)
            {
                var openResponse = ElephantCore.Instance.GetOpenResponse();
                var elephantComplianceManager = ElephantComplianceManager.GetInstance(openResponse);
            
                elephantComplianceManager.OnGDPRStateChangeEvent += OnGdprStateChange;
                elephantComplianceManager.OnCCPAStateChangeEvent += OnCcpaStateChange;
            }
            
            
#if UNITY_ANDROID || UNITY_EDITOR
            InitMediation("consent_disabled");
#elif UNITY_IOS
            if (!InternalConfig.GetInstance().idfa_consent_enabled)
            {
                InitMediation("consent_disabled");
            } else {
                StartCoroutine(CheckIdfaStatus());
            }
#endif
        }
        
        private IEnumerator CheckIdfaStatus()
        {
            while (IdfaConsentResult.GetInstance().GetStatus() == IdfaConsentResult.Status.Waiting)
            {
                yield return null;
            }
            
            InitMediation(IdfaConsentResult.GetInstance().GetIdfaResultValue());
        }

        private void InitMediation(string message)
        {
            if (hasInitMediationStarted) return;
            hasInitMediationStarted = true;
#if UNITY_IOS && !UNITY_EDITOR
            if (message.Equals("consent_disabled"))
            {
                Elephant.Event("facebook_tracking_enabled", -1);
                RollicAdsIos.setTrackingEnabled(true);
            }
            else
            {
                if (message.Equals("Authorized"))
                {
                    Elephant.Event("facebook_tracking_enabled", -1);
                    RollicAdsIos.setTrackingEnabled(true);
                }
                else
                {
                    Elephant.Event("facebook_tracking_disabled", -1);
                    RollicAdsIos.setTrackingEnabled(false);
                }
            }
#endif
            
            this.isMediationInitialized = false;
            IronSource.Agent.SetPauseGame(true);
            IronSource.Agent.init(_appKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
            
            IronSourceEvents.onSdkInitializationCompletedEvent += OnSdkInitializationCompleted;

            IronSourceEvents.onInterstitialAdOpenedEvent += OnInterstitialShownEvent;
            IronSourceInterstitialEvents.onAdClosedEvent += OnInterstitialDismissedEvent;
            IronSourceInterstitialEvents.onAdClickedEvent += OnInterstitialClickedEvent;
            IronSourceEvents.onInterstitialAdLoadFailedEvent += OnInterstitialFailedEvent;
            IronSourceEvents.onInterstitialAdReadyEvent += OnInterstitialAdReadyEvent;

            IronSourceEvents.onRewardedVideoAdClosedEvent += OnRewardedVideoClosedEvent;
            IronSourceEvents.onRewardedVideoAdOpenedEvent += OnRewardedVideoShownEvent;
            IronSourceEvents.onRewardedVideoAdEndedEvent += OnRewardedVideoEndedEvent;
            IronSourceEvents.onRewardedVideoAdRewardedEvent += OnRewardedVideoReceivedRewardEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent += OnRewardedVideoFailedToPlayEvent;

            IronSourceEvents.onBannerAdLoadedEvent += OnAdLoadedEvent;
            IronSourceEvents.onBannerAdClickedEvent += OnAdClickedEvent;
            IronSourceEvents.onBannerAdLoadFailedEvent += OnAdFailedEvent;

            IronSourceEvents.onImpressionDataReadyEvent += OnImpressionTrackedEvent;

        }
        
        private void OnSdkInitializationCompleted()
        {
            Debug.Log("IS SDK Initialized");

            if (Elephant.UserGDPRConsent())
            {
                // consent here
            }
            
            StartCoroutine(LoadAdsAfterInitialization());
            this.isMediationInitialized = true;

            StartCoroutine(SendSdkInitEvents());
        }

        #region AdEvents

        private IEnumerator SendSdkInitEvents()
        {
            yield return new WaitForSecondsRealtime(1.0f);
            Elephant.AdEvent("OnSdkInitializedEvent");
            Elephant.Event("OnSdkInitializedEvent", -1,  null);
            Elephant.Event("ironsource_app_key", -1,  Params.New().Set("appKey", _appKey));
            var evnt = OnRollicAdsSdkInitializedEvent;
            evnt?.Invoke();
            
            while (string.IsNullOrEmpty(Adjust.getAdid()))
            {
                yield return null;
            }

            if (ElephantCore.Instance == null) yield break;
            ElephantCore.Instance.adjustId = string.IsNullOrEmpty(Adjust.getAdid()) ? "" : Adjust.getAdid();
            Adjust.addSessionCallbackParameter("elephant_id", ElephantCore.Instance.userId);
        }

        private void OnGdprStateChange(bool isAccepted)
        {
            IronSource.Agent.setConsent(isAccepted);
        }
        
        private void OnCcpaStateChange(bool isAccepted)
        {
            IronSource.Agent.setMetaData("do_not_sell", isAccepted ? "false" : "true");
        }

        #endregion

        #region InterstitialEvents

        private void OnInterstitialFailedEvent(IronSourceError error)
        {
            _isInterstitialReady = false;
            StartCoroutine(RequestInterstitialAgain());

            Elephant.AdEvent("OnInterstitialFailedEvent", errorCode: error.getDescription());
            var evnt = OnRollicAdsInterstitialFailedEvent;
            evnt?.Invoke(error);
        }
        
        private void OnInterstitialShownEvent()
        {
            _isInterstitialReady = false;
            onInterstitialAdOpenedEvent?.Invoke();

            Elephant.AdEvent("OnInterstitialShownEvent");
            var evnt = OnRollicAdsInterstitialShownEvent;
            evnt?.Invoke();
        }
        
        private void OnInterstitialDismissedEvent(IronSourceAdInfo adInfo)
        {
            _isInterstitialReady = false;
            onInterstitialAdClosedEvent?.Invoke();
            RequestInterstitial();

            var ilrd = new Ilrd(adInfo);
            Elephant.AdEvent("OnInterstitialDismissedEvent");
            Elephant.AdEventV2("OnInterstitialDismissedEvent", JsonUtility.ToJson(ilrd));
            var evnt = OnRollicAdsInterstitialDismissedEvent;
            evnt?.Invoke();
        }
        
        private void OnInterstitialClickedEvent(IronSourceAdInfo adInfo)
        {
            var ilrd = new Ilrd(adInfo);
            
            Elephant.AdEvent("OnInterstitialClickedEvent");
            Elephant.AdEventV2("OnInterstitialClickedEvent", JsonUtility.ToJson(ilrd));
            var evnt = OnRollicAdsInterstitialClickedEvent;
            evnt?.Invoke();
        }

        private void OnInterstitialAdReadyEvent()
        {
            interstitialRequestTimerIndex = 0;
        }

        #endregion

        #region RewardedEvents

        private void OnRewardedVideoShownEvent()
        {
            isRewardAvailable = false;

            Elephant.AdEvent("OnRewardedVideoShownEvent");
            var evnt = OnRollicAdsRewardedVideoShownEvent;
            evnt?.Invoke();
        }
        
        private void OnRewardedVideoEndedEvent()
        {
            Elephant.AdEvent("OnRewardedVideoEndedEvent");
        }
        
        private void OnRewardedVideoClosedEvent()
        {
            CheckReward();

            Elephant.AdEvent("OnRewardedVideoClosedEvent");
            var evnt = OnRollicAdsRewardedVideoClosedEvent;
            evnt?.Invoke();
        }
        
        private void OnRewardedVideoFailedToPlayEvent(IronSourceError error)
        {
            rewardedAdResultCallback?.Invoke(RLRewardedAdResult.Failed);

            Elephant.AdEvent("OnRewardedVideoFailedToPlayEvent", errorCode: error.getDescription());
            var evnt = OnRollicAdsRewardedVideoFailedToPlayEvent;
            evnt?.Invoke();
        }
        
        private void OnRewardedVideoReceivedRewardEvent(IronSourcePlacement placement)
        {
            isRewardAvailable = true;
            
            Elephant.AdEvent("OnRewardedVideoReceivedRewardEvent");
            var evnt = OnRollicAdsRewardedVideoReceivedRewardEvent;
            evnt?.Invoke(placement);
        }

        #endregion

        #region BannerEvents

        private void OnAdLoadedEvent()
        {
            bannerRequestTimerIndex = 0;
            
            Elephant.AdEvent("OnRollicAdsAdLoadedEvent");
            var evnt = OnRollicAdsAdLoadedEvent;
            evnt?.Invoke();
        }
        
        private void OnAdClickedEvent()
        {
            Elephant.AdEvent("OnRollicAdsAdClickedEvent");
            var evnt = OnRollicAdsAdClickedEvent;
            evnt?.Invoke();
        }
        
        private void OnAdFailedEvent(IronSourceError error)
        {
            StartCoroutine(RequestBannerAgain());

            Elephant.AdEvent("OnRollicAdsAdFailedEvent", errorCode: error.getDescription());
            var evnt = OnRollicAdsAdFailedEvent;
            evnt?.Invoke(error.getDescription());
        }

        #endregion
        
        #region ILRDEvents

        private void OnImpressionTrackedEvent(IronSourceImpressionData ironSourceImpressionData)
        {
            
            var adRevenue = new AdjustAdRevenue(AdjustConfig.AdjustAdRevenueSourceIronSource);
            if (ironSourceImpressionData.revenue != null)
            {
                var revNonnull = (double) ironSourceImpressionData.revenue;
                adRevenue.setRevenue(revNonnull, "USD");
            }

            adRevenue.setAdRevenueNetwork(ironSourceImpressionData.adNetwork);
            adRevenue.setAdRevenueUnit(ironSourceImpressionData.adUnit);
            adRevenue.setAdRevenuePlacement(ironSourceImpressionData.placement);
            adRevenue.addCallbackParameter("ad_format", ironSourceImpressionData.instanceName);
            adRevenue.addCallbackParameter("instance_name", ironSourceImpressionData.instanceName);
            adRevenue.addCallbackParameter("instance_id", ironSourceImpressionData.instanceId);
            adRevenue.addCallbackParameter("auction_id", ironSourceImpressionData.auctionId);

            adRevenue.addCallbackParameter("ab",
                !string.IsNullOrEmpty(ironSourceImpressionData.ab) 
                    ? ironSourceImpressionData.ab 
                    : "");

            adRevenue.addCallbackParameter("segment_name",
                !string.IsNullOrEmpty(ironSourceImpressionData.segmentName)
                    ? ironSourceImpressionData.segmentName
                    : "");
            
            Adjust.trackAdRevenue(adRevenue);
            Elephant.IronsourceAdRevenueEvent(ironSourceImpressionData.allData);
        }

        #endregion
        

        IEnumerator LoadAdsAfterInitialization()
        {
            yield return new WaitForSecondsRealtime(2.0f);
            RequestInterstitial();
        }

        #region Rewarded
        

        public bool isRewardedVideoAvailable()
        {
            if (!IsMediationReady()) return false;
      
            return IronSource.Agent.isRewardedVideoAvailable();
        }

        public void showRewardedVideo()
        {
            if (!IsMediationReady()) return;
            
            Elephant.AdEvent("Rollic_showRewardedVideo");
            IronSource.Agent.showRewardedVideo();
        }


        #endregion

        #region Banner

        public void loadBanner()
        {
            if (!IsMediationReady()) return;
            
            Elephant.AdEvent("Rollic_loadBanner");
            IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
        }

        public void showBanner()
        {
            IronSource.Agent.displayBanner();
        }

        public void hideBanner()
        {
            IronSource.Agent.hideBanner();
        }

        public void destroyBanner()
        {
            IronSource.Agent.destroyBanner();
        }

        #endregion

        #region Interstitial

        private void RequestInterstitial()
        {
            Elephant.AdEvent("Rollic_RequestInterstitial");
            IronSource.Agent.loadInterstitial();
        }
        
        public bool isInterstitialReady()
        {
            return IronSource.Agent.isInterstitialReady();
        }

        public void loadInterstitial()
        {
            RequestInterstitial();
        }

        public void showInterstitial()
        {
            Elephant.AdEvent("Rollic_showInterstitial");
            IronSource.Agent.showInterstitial();
        }
        

        #endregion
        

        IEnumerator RequestInterstitialAgain()
        {
            if (timers == null) throw new Exception("RLAdvertisementManager has not been initialized!");
            
            yield return new WaitForSecondsRealtime(timers[interstitialRequestTimerIndex]);
            if (interstitialRequestTimerIndex < timers.Count - 1)
            {
                interstitialRequestTimerIndex++;    
            }
            else
            {
                interstitialRequestTimerIndex = 0;
            }

            loadInterstitial();
        }
        
        IEnumerator RequestBannerAgain()
        {
            if (timers == null) throw new Exception("RLAdvertisementManager has not been initialized!");
            
            yield return new WaitForSecondsRealtime(timers[bannerRequestTimerIndex]);
            if (bannerRequestTimerIndex < timers.Count - 1)
            {
                bannerRequestTimerIndex++;    
            }
            else
            {
                bannerRequestTimerIndex = 0;
            }

            loadBanner();
        }
        
        void CheckReward()
        {
            
            if (isRewardAvailable)
            {
                rewardedAdResultCallback(RLRewardedAdResult.Finished);
            }
            else
            {
                rewardedAdResultCallback(RLRewardedAdResult.Skipped);
            }
        }

        public void SuccessResponse(string responseJson)
        {
            try
            {

                var response = JsonUtility.FromJson<AdRevenueResponse>(responseJson);
                if (response.update_conversion)
                {
#if UNITY_IOS && !UNITY_EDITOR
                    RollicAdsIos.updateConversionValue(response.value);
#endif
                    SendConversionValueEvent(response);
                }
                else
                {
                    if (response.send_false_events)
                    {
                        SendConversionValueEvent(response);
                    }
                }
            }
            catch (Exception e)
            {
                
                Debug.Log(e);
            }
        }

        private static void SendConversionValueEvent(AdRevenueResponse response)
        {
            var parameters = Params.New();
            parameters.Set("value", response.value);
            parameters.Set("d0_rev", response.d0_rev);
            Elephant.Event("update_conversion_value_event", MonitoringUtils.GetInstance().GetCurrentLevel(),
                parameters);
        }

        private bool IsMediationReady()
        {
            if (isMediationInitialized)
            {
                return true;
            }

            Debug.LogError("RLAdvertisementManager is not initialized properly! Please make sure that you registered OnSdkInitializedEvent event");
            return false;
        }
        
        void OnApplicationPause(bool pauseStatus)
        {
            IronSource.Agent.onApplicationPause(pauseStatus);
        }
    }
}