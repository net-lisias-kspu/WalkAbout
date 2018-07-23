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

using KspWalkAbout.Entities;
using KspWalkAbout.Extensions;
using KspWalkAbout.Guis;
using KspWalkAbout.WalkAboutFiles;
using UnityEngine;
using static KspAccess.CommonKspAccess;
using static KspWalkAbout.Entities.WalkAboutPersistent;

namespace KspWalkAbout
{
    /// <summary>
    /// Module to allow the user to add new locations for kerbals to be placed.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class WalkAboutAddUtility : MonoBehaviour
    {
        private WalkAboutSettings _config;
        private KnownPlaces _map;
        private AddUtilityGui _addUtilityGui;

        /// <summary>
        /// Called when the game is loaded. Used to set up all persistent objects and properties.
        /// </summary>
        public void Start()
        {
            if (!(FlightGlobals.ActiveVessel?.isEVA ?? false))
            {
                "Add Location utility deactivated: not an EVA".Debug();
                return;
            }
            var crew = FlightGlobals.ActiveVessel.GetVesselCrew();
            if ((crew?.Count ?? 0) != 1)
            {
                "Add Location utility deactivated: invalid crew count".Debug();
                return;
            }

            _config = GetModConfig(); "Add Location utility obtained config".Debug();
            if (_config == null) { return; }

            if (_config.Mode != "utility")
            {
                "Add Location utility deactivated: not in utility mode".Debug();
                return;
            }

            $"Add Location utility activated on EVA for {FlightGlobals.ActiveVessel.GetVesselCrew()[0].name}".Debug();

            _map = GetLocationMap(); "Add Location utility obtained map object".Debug();
            _addUtilityGui = AddUtilityGui.Instance;
        }

        /// <summary>
        /// Called each time the game state is updated.
        /// </summary>
        public void Update()
        {
            if (_addUtilityGui == null)
            {
                return;
            }

            if (CheckForModUtilityActivation())
            {
                _addUtilityGui.GuiCoordinates = GuiResizer.HandleResizing(_addUtilityGui.GuiCoordinates);
                SaveFiles();
            }

            if (!Input.GetMouseButton(0))
            {
                SaveFiles();
            }
        }

        /// <summary>
        /// Called each time the game's GUIs are to be refreshed.
        /// </summary>
        public void OnGUI()
        {
            _addUtilityGui?.Display();

            if (_addUtilityGui?.RequestedLocation == null) return;

            $"Request for new location {_addUtilityGui.RequestedLocation.Name} detected".Debug();
            _map.AddLocation(_addUtilityGui.RequestedLocation);
            _addUtilityGui.RequestedLocation = null;
        }

        /// <summary>
        /// Determines if the user has requested the WalkAbout mod's utility GUI.
        /// </summary>
        private bool CheckForModUtilityActivation()
        {
            var wasActive = _addUtilityGui.IsActive;
            _addUtilityGui.IsActive |= IsKeyCombinationPressed(_config.AUActivationHotKey, _config.AUActivationHotKeyModifiers);

            if (wasActive != _addUtilityGui.IsActive)
            {
                _map.RefreshLocations();
            }

            return _addUtilityGui.IsActive;
        }

        /// <summary>
        /// Saves all settings files with pending changes.
        /// </summary>
        private void SaveFiles()
        {
            if (_map.IsChanged)
            {
                "Saving map changes".Debug();
                _map.Save();
            }
        }
    }
}