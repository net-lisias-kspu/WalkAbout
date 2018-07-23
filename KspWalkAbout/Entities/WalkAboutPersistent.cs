using KspAccess;
using KspWalkAbout.Extensions;
using KspWalkAbout.Values;
using KspWalkAbout.WalkAboutFiles;
using System.Collections.Generic;

namespace KspWalkAbout.Entities
{
    /// <summary>Contains items that are to remain available for use across multiple invocations of the mod.</summary>
    internal static class WalkAboutPersistent
    {
        /// <summary>The configuration file information for this mod.</summary>
        public static WalkAboutSettings _modConfig = null;

        /// <summary>The collection of all locations where a kerbal can be placed.</summary>
        public static KnownPlaces _locationMap = null;

        /// <summary>The collection of all items that a kerbal can have in his/her inventory.</summary>
        public static InventoryItems _inventoryItems = null;

        /// <summary>
        /// Catalogue of items assigned to the inventories of kerbals that have been placed but not yet been 
        /// physically created (i.e. user has not started their EVA FlightScene).
        /// </summary>
        public static Dictionary<string, List<string>> AllocatedItems = new Dictionary<string, List<string>>();

        /// <summary>Gets the configuration information for this mod.</summary>
        /// <returns>An object representing the information in the configuration file</returns>
        public static WalkAboutSettings GetModConfig()
        {
            if (_modConfig == null)
            {
                _modConfig = new WalkAboutSettings();
                var loaded = _modConfig.Load($"{WalkAbout.GetModDirectory()}/Settings.cfg", Constants.DefaultSettings);
                _modConfig.StatusMessage.Log();

                if (!loaded) { return null; }
            }

            return _modConfig;
        }

        /// <summary>Gets the list of all locations that this mod can use.</summary>
        /// <returns>An object representing all locations.</returns>
        public static KnownPlaces GetLocationMap()
        {
            _locationMap = _locationMap ?? new KnownPlaces();
            return _locationMap;
        }

        /// <summary>Get the list of all items that can be added to kerbal's inventory.</summary>
        /// <returns>An object representing all available items.</returns>
        public static InventoryItems GetAllItems()
        {
            _inventoryItems = _inventoryItems ?? new InventoryItems();
            return _inventoryItems;
        }
    }
}
