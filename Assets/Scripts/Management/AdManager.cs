using System;
using UnityEngine;
//using GoogleMobileAds.Api;

namespace Chess.Ads
{
    public class AdManager : MonoBehaviour
    {
        public string id = "ca-app-pub-1721773800800991/9010700560";

        public static AdManager instance;

        public bool adsEnabled;

        //InterstitialAd interstitialAd;
        Action onFinish;

        void Awake()
        {
            if(instance != null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(this);

            //if (adsEnabled) MobileAds.Initialize(status => { });
        }

        int debugNum = 0;
        [ContextMenu("Debug Ad Status")]
        void DebugAdStatus()
        {
            debugNum++;
            //if (interstitialAd == null) Debug.Log($"{debugNum} No ad so far");
            //else if (!interstitialAd.IsLoaded()) Debug.Log($"{debugNum} Ad exists but is not loaded");
            //else if (interstitialAd.IsLoaded()) Debug.Log($"{debugNum} Ad is loaded");
        }

        [ContextMenu("Load Ad")]
        public void RequestInterstitial(Action onFinish)
        {
            if (!adsEnabled)
            {
                onFinish();
                return;
            }

            this.onFinish = onFinish;

            /*
            interstitialAd = new InterstitialAd(id);
            interstitialAd.OnAdLoaded += OnAdLoaded;
            interstitialAd.OnAdFailedToLoad += OnAdFailedToLoad;
            interstitialAd.OnAdOpening += OnAdOpened;
            interstitialAd.OnAdClosed += OnAdClosed;

            var request = new AdRequest.Builder().Build();

            interstitialAd.LoadAd(request);

            Invoke("ShowInterstitialAd", 0.5f);
            */
        }
        
        void ShowInterstitialAd()
        {
            //if (interstitialAd.IsLoaded()) interstitialAd.Show();
        }

        #region Ad Events
        void LogAdEvent(string message)
        {
            //Debug.Log($"Ad {message} event!");
        }

        void OnAdLoaded(object sender, System.EventArgs e)
        {
            LogAdEvent("loaded");
        }

        void OnAdFailedToLoad(object sender, System.EventArgs e)
        {
            LogAdEvent("failed to load");
            onFinish();
        }

        void OnAdOpened(object sender, System.EventArgs e)
        {
            LogAdEvent("opened");
        }

        void OnAdClosed(object sender, System.EventArgs e)
        {
            LogAdEvent("closed");
            onFinish();
        }
        #endregion
    }
}