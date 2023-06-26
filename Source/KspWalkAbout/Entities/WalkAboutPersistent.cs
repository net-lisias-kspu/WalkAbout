/*  Copyright 2017 Clive Pottinger
    This file is part of the WalkAbout Mod.

    WalkAbout is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    WalkAbout is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with WalkAbout.  If not, see<http://www.gnu.org/licenses/>.
*/

using KspWalkAbout.Extensions;
using KspWalkAbout.Values;
using KspWalkAbout.WalkAboutFiles;
using System.Collections.Generic;

namespace KspWalkAbout.Entities
{
    /// <summary>
    /// Contains items that are to remain available for use across multiple invocations of the mod.
    /// </summary>
    internal static class WalkAboutPersistent
    {
        /// <summary>
        /// The list of Kerbal Inventory System modules which should be searched for inventory configuration information.
        /// </summary>
        public static List<string> KisModuleNames = new List<string> { "ModuleKISItem", "ModuleKISItemEvaTweaker" };

        /// <summary>
        /// The list of resources that are filled when a kerbal is placed.
        /// </summary>
        public static List<string> EvaResources = new List<string> { "Food", "Water", "Oxygen", "ElectricCharge" };

        /// <summary>
        /// The configuration file information for this mod.
        /// </summary>
        public static WalkAboutSettings _modConfig = null;

        /// <summary>
        /// The collection of all locations where a kerbal can be placed.
        /// </summary>
        public static KnownPlaces _locationMap = null;

        /// <summary>
        /// The collection of all items that a kerbal can have in his/her inventory.
        /// </summary>
        public static InventoryItems _inventoryItems = null;

        /// <summary>
        /// Catalogue of items assigned to the inventories of kerbals that have been placed but not yet been
        /// physically created (i.e. user has not started their EVA FlightScene).
        /// </summary>
        public static Dictionary<string, List<string>> AllocatedItems = new Dictionary<string, List<string>>();

        /// <summary>
        /// The point from which all placement locations are measured from.
        /// </summary>
        private static Centrum _centrum;

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

        public static Centrum GetCentrum()
        {
            _centrum = _centrum ?? new Centrum();
            return _centrum;
        }
    }
}