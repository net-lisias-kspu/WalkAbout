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
using KspWalkAbout.Values;
using UnityEngine;

namespace KspWalkAbout
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class WalkAboutAddUtility : MonoBehaviour
    {
        private KspFiles.Settings _config;
        private KnownPlaces _map;
        private AddUtilityGui _addUtilityGui;
        private GuiState _guiState;
        private GuiState _lastGuiState;

        public void Start()
        {
            DebugExtensions.DebugOn = System.IO.File.Exists($"{KSPUtil.ApplicationRootPath}GameData/WalkAbout/debug.flg");

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

            _config = new KspFiles.Settings();
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
            _lastGuiState = GuiState.force;
        }

        public void Update()
        {
            if (_addUtilityGui == null) return;
            CheckForModUtilityActivation();
            _addUtilityGui.GuiCoordinates = GuiResizer.HandleResizing(_addUtilityGui.GuiCoordinates);
            SaveFiles();
        }

        public void OnGUI()
        {
            _addUtilityGui?.Display(ref _guiState);
            if (_guiState != _lastGuiState)
            {
                if (_lastGuiState == GuiState.force)
                    $"Current AddUtilityGUI state is {_guiState}".Debug();
                else
                    $"AddUtilityGUI display changed from {_lastGuiState} to {_guiState}".Debug();
                _lastGuiState = _guiState;
            }

            if (_addUtilityGui?.RequestedLocation == null) return;

            $"Request for new location {_addUtilityGui.RequestedLocation.Name} detected".Debug();
            _map.AddLocation(_addUtilityGui.RequestedLocation);
            _addUtilityGui.RequestedLocation = null;
        }

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
                    _lastGuiState = GuiState.force;
                }
            }
        }

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