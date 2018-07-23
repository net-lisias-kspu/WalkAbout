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
using KspWalkAbout.Entities;
using KspWalkAbout.Extensions;
using KspWalkAbout.Guis;
using KspWalkAbout.WalkAboutFiles;
using UnityEngine;

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

            _config = new WalkAboutSettings();
            var loaded = _config.Load($"{WalkAbout.GetModDirectory()}/Settings.cfg");
            _config.StatusMessage.Log();
            if (!loaded) return;
            if (_config.Mode != "utility")
            {
                "Add Location utility deactivated: not in utility mode".Debug();
                return;
            }

            $"Add Location utility activated on EVA for {FlightGlobals.ActiveVessel.GetVesselCrew()[0].name}".Debug();

            _map = new KnownPlaces(); "created map object".Debug();
            _addUtilityGui = new AddUtilityGui(_map);
        }

        /// <summary>Called each time the game state is updated.</summary>
        public void Update()
        {
            if (_addUtilityGui == null) return;
            CheckForModUtilityActivation();
            _addUtilityGui.GuiCoordinates = GuiResizer.HandleResizing(_addUtilityGui.GuiCoordinates);
            SaveFiles();
        }

        /// <summary>Called each time the game's GUIs are to be refreshed.</summary>
        public void OnGUI()
        {
            _addUtilityGui?.Display();

            if (_addUtilityGui?.RequestedLocation == null) return;

            $"Request for new location {_addUtilityGui.RequestedLocation.Name} detected".Debug();
            _map.AddLocation(_addUtilityGui.RequestedLocation);
            _addUtilityGui.RequestedLocation = null;
        }

        /// <summary>Determines if the user has requested the WalkAbout mod's utility GUI.</summary>
        private void CheckForModUtilityActivation()
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                var requiredKeysPressed = _config.ActivationHotKeyModifiers.Count == 0;
                if (!requiredKeysPressed)
                {
                    foreach (var modifier in _config.ActivationHotKeyModifiers)
                    {
                        requiredKeysPressed |= Input.GetKey(modifier);
                    }
                }

                _addUtilityGui.IsActive = requiredKeysPressed;
                if (requiredKeysPressed)
                {
                    _map.Refresh();
                }
            }
        }

        /// <summary>Saves all settings files with pending changes.</summary>
        private void SaveFiles()
        {
            if (_map.IsChanged && !Input.GetMouseButton(0))
            {
                "Saving map changes".Debug();
                _map.Save();
            }
        }
    }
}