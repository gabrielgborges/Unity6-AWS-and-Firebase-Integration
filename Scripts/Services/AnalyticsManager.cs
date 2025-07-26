using System;
using Firebase;
using Firebase.Analytics;
using UnityEngine;

//Firebase SDK should be correctly integrated with the project through the Firebase website and Unity
public class AnalyticsManager : ServiceBase, IAnalyticsService
{
    private FirebaseApp _app = null;
    
    private void Start()
    {
      ServiceLocator.AddService<IAnalyticsService>(this);
      Setup();
    }

    public override void Setup()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available) {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                _app = FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            } else {
                Debug.LogError(String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
            
            //Fire first data collection (devices and location + custom level start event)
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelStart, 
                new Parameter(FirebaseAnalytics.ParameterLevel, 1), 
                new Parameter(FirebaseAnalytics.ParameterLevelName, "LevelName :)"));
        });
    }

    public override void Dispose()
    {
    }
}
