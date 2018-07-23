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
using UnityEngine;

namespace KspWalkAbout
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class WalkAbout : MonoBehaviour
    {
        private KspFiles.Settings _config;
        private KnownPlaces _map;
        private MainGui _mainGui;
        private GuiState _guiState;
        private GuiState _lastGuiState;

        public void Start()
        {
            DebugExtensions.DebugOn = System.IO.File.Exists($"{GetModDirectory()}/debug.flg");
            $"Started [Version={Constants.Version}, Debug={DebugExtensions.DebugOn}]".Log();

            _config = new KspFiles.Settings();
            var loaded = _config.Load($"{GetModDirectory()}/Settings.cfg", Constants.DefaultSettings); "loaded config = {loaded}".Debug();
            _config.StatusMessage.Log(); "printed status message".Debug();
            if (!loaded) return;

            _map = new KnownPlaces(); "created map object".Debug();

            _mainGui = new MainGui { GuiCoordinates = _config.GetScreenPosition(), TopFew = _config.TopFew }; $"created MainGui object".Debug();
            _lastGuiState = GuiState.force;

        }

        public void Update()
        {
            if (_mainGui == null) return;
            CheckForModActivation();
            _mainGui.GuiCoordinates = GuiResizer.HandleResizing(_mainGui.GuiCoordinates);
            SaveFiles();
        }

        public void OnGUI()
        {
            if (_mainGui?.Display(ref _guiState) ?? false)
            {
                _config.SetScreenPosition(_mainGui.GuiCoordinates);
            }
            if (_guiState != _lastGuiState)
            {
                if (_lastGuiState == GuiState.force)
                    $"Current MainGUI state is {_guiState}".Debug();
                else
                    $"MainGui display changed from {_lastGuiState} to {_guiState}".Debug();
                _lastGuiState = _guiState;
            }

            if (_mainGui?.RequestedPlacement == null) return;

            WalkAboutKspAccess.PlaceKerbal(_mainGui.RequestedPlacement);
            GamePersistence.SaveGame("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
            HighLogic.LoadScene(GameScenes.SPACECENTER);

            _map.UpdateQueuing(_mainGui.RequestedPlacement.Location.LocationName);
            _mainGui.RequestedPlacement = null;
            LoadGuiWithMapData();
        }

        internal static string GetModDirectory()
        {
            return $"{KSPUtil.ApplicationRootPath}GameData/{Constants.ModName}";
        }

        private void CheckForModActivation()
        {
            if (Input.GetKeyDown(_config.ActivationHotKey))
            {
                var requiredKeysPressed = _config.ActivationHotKeyModifiers.Count == 0;
                if (!requiredKeysPressed)
                {
                    foreach (var modifier in _config.ActivationHotKeyModifiers)
                    {
                        requiredKeysPressed |= Input.GetKey(modifier);
                    }
                }

                if (requiredKeysPressed)
                {
                    $"Required key combination pressed".Debug();
                    LoadGuiWithMapData();
                    _lastGuiState = GuiState.force;
                }
                _mainGui.IsActive = requiredKeysPressed;
            }
        }

        private void LoadGuiWithMapData()
        {
            _map.Refresh();
            _mainGui.Facilities = _map.AvailableFacilities;
            _mainGui.Locations = _map.AvailableLocations;
        }

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
        }
    }
}
