using Firebase;
using Firebase.Analytics;
using UnityEngine;

public class ControllerFirebase : MonoBehaviour
{
    public static ControllerFirebase Instance;

    private void Awake() {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true); 
            
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                // Crashlytics will use the DefaultInstance, as well;
                // this ensures that Crashlytics is initialized.
                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here for indicating that your project is ready to use Firebase.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}",dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
        
         IronSourceEvents.onImpressionDataReadyEvent += OnImpressionTrackedEvent;
    }
        
    private void OnImpressionTrackedEvent(IronSourceImpressionData impressionData)
    {
        var impressionParameters = new[] {
                new Parameter("ad_platform", "ironSource"),
                new Parameter("ad_source", impressionData.adNetwork),
                new Parameter("ad_unit_name", impressionData.instanceName),
                new Parameter("ad_format", impressionData.adUnit),
                new Parameter("value", (double)impressionData.revenue),
                new Parameter("currency", "USD")
            };

        FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
        FirebaseAnalytics.LogEvent("custom_ad_impression", impressionParameters);
    }
}