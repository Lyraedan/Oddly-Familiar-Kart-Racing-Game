using System;
using Modio.API;
using Modio.Monetization;
using Modio.Unity.Settings;
using UnityEngine;

namespace Modio.Unity.UI.Components
{
    public class ModioUIMonetizationHider : MonoBehaviour
    {
        
        [Serializable, Flags,]
        enum MonetizationType
        {
            VirtualCurrency = 1 << ModioMonetizationType.VirtualCurrency,
            UsdMarketplace = 1 << ModioMonetizationType.UsdMarketplace,
        }

        bool _isOffline;
        bool _isMonetizationDisabled;
        
        
        [SerializeField]
        MonetizationType _shownOnMonetizationType = MonetizationType.VirtualCurrency;
        
        void Start()
        {
            ModioClient.OnInitialized += OnPluginInitialized;
            ModioAPI.OnOfflineStatusChanged += OnOfflineStatusChanged;
        }

        void OnDestroy()
        {
            ModioClient.OnInitialized -= OnPluginInitialized;
            ModioAPI.OnOfflineStatusChanged -= OnOfflineStatusChanged;
        }

        void OnOfflineStatusChanged(bool isOffline)
        {
            _isOffline = isOffline;
            
            ChangeActiveStateIfNeeded();
        }

        void OnPluginInitialized()
        {
            var settings = ModioServices.Resolve<ModioSettings>();

            // Check if the current monetization type is enabled on this platform
            
            // If there are no monetization settings, monetization is disabled
            // Otherwise, check if the current monetization type is included in the shown types
            _isMonetizationDisabled = !settings.TryGetPlatformSettings(out MonetizationSettings platformMonetizationSettings)
                || ((1 << (int)platformMonetizationSettings.MonetizationType) & (int)_shownOnMonetizationType) == 0;

            ChangeActiveStateIfNeeded();
        }

        void ChangeActiveStateIfNeeded() => gameObject.SetActive(!(_isOffline || _isMonetizationDisabled));
    }
}
