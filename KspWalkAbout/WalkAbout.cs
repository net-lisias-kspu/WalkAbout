/*  Copyright 2016 Clive Pottinger
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
using KspAccess;
using KspWalkAbout.Entities;
using KspWalkAbout.Extensions;
using KspWalkAbout.Guis;
using KspWalkAbout.Values;
using KspWalkAbout.WalkAboutFiles;
using UnityEngine;
using Upgradeables;
using static KspAccess.CommonKspAccess;
using static KspWalkAbout.Entities.WalkAboutPersistent;

namespace KspWalkAbout
{
    /// <summary>Module to allow user to place kerbals at specific locations. </summary>
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class WalkAbout : MonoBehaviour
    {
        private WalkAboutSettings _config;
        private KnownPlaces _map;
        private InventoryItems _items;
        private static MainGui _mainGui;

        /// <summary>
        /// Called when the game is loaded. Used to set up all persistent objects and properties.
        /// </summary>
        public void Start()
        {
            $"Started [Version={Constants.Version}, Debug={DebugExtensions.DebugIsOn}]".Log();

            _config = GetModConfig(); "obtained config".Debug();
            if (_config == null) { return; }

            _map = GetLocationMap(); "obtained map object".Debug();
            _items = GetAllItems(); "obtained items object".Debug();

            _mainGui = _mainGui ??
                new MainGui
                {
                    GuiCoordinates = _config.GetScreenPosition(),
                    TopFew = _config.TopFew,
                    MaxItems = _config.MaxInventoryItems,
                    MaxVolume = _config.MaxInventoryVolume,
                }; $"created MainGui object".Debug();

            GameEvents.OnTechnologyResearched.Remove(ItemRefresh);
            GameEvents.OnKSCFacilityUpgraded.Remove(MapRefresh);
            GameEvents.OnTechnologyResearched.Add(ItemRefresh);
            GameEvents.OnKSCFacilityUpgraded.Add(MapRefresh);
        }

        private void ItemRefresh(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> data)
        {
            if (data.target == RDTech.OperationResult.Successful)
            {
                $"Refreshing items due to new technologies".Debug();
                _items.RefreshItems();
            }
        }

        private void MapRefresh(UpgradeableFacility data0, int data1)
        {
            $"Refreshing map due to new facility upgrade".Debug();
            _map.RefreshLocations();
        }

        /// <summary>Called each time the game state is updated.</summary>
        public void Update()
        {
            if (_mainGui == null) return;
            if (CheckForModActivation())
            {
                _mainGui.GuiCoordinates = GuiResizer.HandleResizing(_mainGui.GuiCoordinates);
                SaveFiles();
            }
        }

        /// <summary>Called each time the game's GUIs are to be refreshed.</summary>
        public void OnGUI()
        {
            if (_mainGui?.Display() ?? false)
            {
                _config.SetScreenPosition(_mainGui.GuiCoordinates);
            }

            if (_mainGui?.RequestedPlacement == null) return;

            "placing kerbal".Debug();
            WalkAboutKspAccess.PlaceKerbal(_mainGui.RequestedPlacement);
            "Saving game".Log();
            GamePersistence.SaveGame("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);

            switch (_config.PostPlacementAction)
            {
                case PostPlacementMode.noreload:
                    "Suppressed reload of SPACECENTER".Log();
                    break;

                case PostPlacementMode.jumpto:
                    var vessel = FindVesselByName(_mainGui.RequestedPlacement.Kerbal.name);
                    if (vessel == null)
                    {
                        $"Unable to jump to vessel - no vessel found".Log();
                    }
                    else
                    {
                        $"Loading Flight scene for {vessel.name}".Log();
                        FlightDriver.StartAndFocusVessel("persistent", FlightGlobals.Vessels.IndexOf(vessel));
                    }
                    break;

                case PostPlacementMode.reload:
                default:
                    "Reloading SPACECENTER".Log();
                    HighLogic.LoadScene(GameScenes.SPACECENTER);
                    break;
            }

            _items.UpdateQueueing(_mainGui.RequestedPlacement.Items);
            _map.UpdateQueuing(_mainGui.RequestedPlacement.Location.LocationName);
            _mainGui.RequestedPlacement = null;
        }

        /// <summary>Obtains the directory where the WalkAbout mod is currently installed.</summary>
        /// <returns>Returns a directoy path.</returns>
        internal static string GetModDirectory()
        {
            return CommonKspAccess.GetModDirectory(Constants.ModName);
        }

        /// <summary>
        /// Finds a vessel in the game.
        /// </summary>
        /// <param name="name">The name of vessel.</param>
        private static Vessel FindVesselByName(string name)
        {
            Vessel found = null;
            var searchName = $"{name} (unloaded)";
            foreach (var vessel in FlightGlobals.Vessels)
            {
                if ((vessel.name == name) || (vessel.name == searchName))
                {
                    found = vessel;
                    break;
                }
            }

            return found;
        }

        /// <summary>Determines if the user has requested the WalkAbout mod's GUI.</summary>
        private bool CheckForModActivation()
        {
            _mainGui.IsActive |= CheckForKeyCombo(_config.ActivationHotKey, _config.ActivationHotKeyModifiers);
            return _mainGui.IsActive;
        }

        /// <summary>Saves all settings files with pending changes.</summary>
        private void SaveFiles()
        {
            if (_config.IsChanged && !GuiResizer.IsResizing && !Input.GetMouseButton(0))
            {
                _config.Save();
                $"saved settings to {_config.FilePath}".Log();
            }

            if (_map.IsChanged)
            {
                _map.Save();
            }

            if (_items.IsChanged)
            {
                _items.Save();
            }
        }
    }
}
