using Foundation;
using Google.MobileAds;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UIKit;

namespace MarcTron.Plugin.Services
{
    class InterstitialService: FullScreenContentDelegate
    {
        private Dictionary<string, InterstitialAd> interstitials = new Dictionary<string, InterstitialAd>();
        private MTAdmobImplementation mTAdmobImplementation;

        public InterstitialService(MTAdmobImplementation mTAdmobImplementation)
        {
            this.mTAdmobImplementation = mTAdmobImplementation;
        }

        /*private void CreateInterstitialAd(string adUnit)
        {
            try
            {
                if (_adInterstitial != null)
                {
                    _adInterstitial.AdReceived -= _adInterstitial_AdReceived;
                    _adInterstitial.WillPresentScreen -= _adInterstitial_WillPresentScreen;
                    _adInterstitial.WillDismissScreen -= _adInterstitial_WillDismissScreen;
                    _adInterstitial = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            _adInterstitial = new Interstitial(adUnit);

            _adInterstitial.AdReceived += _adInterstitial_AdReceived;
            _adInterstitial.WillPresentScreen += _adInterstitial_WillPresentScreen;
            _adInterstitial.WillDismissScreen += _adInterstitial_WillDismissScreen;
        }*/

        private void InterstitialLoadComplete(InterstitialAd ad, NSError error)
        {
            if (error != null)
            {
                Debug.WriteLine("Failed to load interstitial ad with error: {0}:{1}", error.LocalizedFailureReason, error.LocalizedDescription);
                return;
            }

            if (ad != null)
            {
                Debug.WriteLine("Ad " + ad.AdUnitID + " loaded");
                ad.ContentDelegate = this;
                interstitials[ad.AdUnitID] = ad;
            }
        }

        public void LoadInterstitial(string adUnit)
        {
            if (!CrossMTAdmob.Current.IsEnabled)
                return;

            var request = new Request();
            InterstitialAd.Load(adUnitID: adUnit,
                                request: request,
                                completionHandler: new InterstitialAdLoadCompletionHandler(InterstitialLoadComplete));
        }

        public void ShowInterstitial(string adUnit)
        {
            if (!CrossMTAdmob.Current.IsEnabled)
                return;

            if (interstitials.TryGetValue(adUnit, out InterstitialAd _adInterstitial))
            {
                var window = UIApplication.SharedApplication.KeyWindow;
                var vc = window.RootViewController;
                while (vc.PresentedViewController != null)
                {
                    vc = vc.PresentedViewController;
                }

                if (vc is UINavigationController { ViewControllers: { } } navController)
                {
                    vc = navController.ViewControllers.Last();
                }
                _adInterstitial.Present(vc);
            }
        }

        internal bool IsLoaded(string adUnit)
        {
            return interstitials.ContainsKey(adUnit);
        }

        private void _adInterstitial_WillDismissScreen(object sender, EventArgs e)
        {
            mTAdmobImplementation.MOnInterstitialClosed();
        }

        private void _adInterstitial_WillPresentScreen(object sender, EventArgs e)
        {
            mTAdmobImplementation.MOnInterstitialOpened();
        }

        private void _adInterstitial_AdReceived(object sender, EventArgs e)
        {
            mTAdmobImplementation.MOnInterstitialLoaded();
        }
    }
}
