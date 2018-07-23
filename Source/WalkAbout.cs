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

namespace KspWalkAbout
{
    /// <summary>Module to allow user to place kerbals at specific locations. </summary>
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class WalkAbout : MonoBehaviour
    {
        private WalkAboutSettings _config;
        private KnownPlaces _map;
        private InventoryItems _items;
        private MainGui _mainGui;

        /// <summary>
        /// Called when the game is loaded. Used to set up all persistent objects and properties.
        /// </summary>
        public void Start()
        {
            $"Started [Version={Constants.Version}, Debug={DebugExtensions.DebugIsOn}]".Log();

            _config = new WalkAboutSettings();
            var loaded = _config.Load($"{GetModDirectory()}/Settings.cfg", Constants.DefaultSettings); 
            _config.StatusMessage.Log(); 
            if (!loaded) return;

            _map = new KnownPlaces(); "created map object".Debug();
            _items = new InventoryItems() { MaxVolume = _config.MaxInventoryVolume }; "created items object".Debug();

            _mainGui =
                new MainGui
                {
                    GuiCoordinates = _config.GetScreenPosition(),
                    TopFew = _config.TopFew,
                    MaxItems = _config.MaxInventoryItems,
                    MaxVolume = _config.MaxInventoryVolume,
                }; $"created MainGui object".Debug();
        }

        /// <summary>Called each time the game state is updated.</summary>
        public void Update()
        {
            if (_mainGui == null) return;
            CheckForModActivation();
            _mainGui.GuiCoordinates = GuiResizer.HandleResizing(_mainGui.GuiCoordinates);
            SaveFiles();
        }

        /// <summary>Called each time the game's GUIs are to be refreshed.</summary>
        public void OnGUI()
        {
            if (_mainGui?.Display() ?? false)
            {
                _config.SetScreenPosition(_mainGui.GuiCoordinates);
            }

            if (_mainGui?.RequestedPlacement == null) return;

            WalkAboutKspAccess.PlaceKerbal(_mainGui.RequestedPlacement);
            GamePersistence.SaveGame("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
            HighLogic.LoadScene(GameScenes.SPACECENTER);

            _items.UpdateQueueing(_mainGui.RequestedPlacement.Items);
            _map.UpdateQueuing(_mainGui.RequestedPlacement.Location.LocationName);
            _mainGui.RequestedPlacement = null;
            LoadGuiWithCurrentData();
        }

        /// <summary>Obtains the directory where the WalkAbout mod is currently installed.</summary>
        /// <returns>Returns a directoy path.</returns>
        internal static string GetModDirectory()
        {
            return CommonKspAccess.GetModDirectory(Constants.ModName);
        }

        /// <summary>Determines if the user has requested the WalkAbout mod's GUI.</summary>
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
                    LoadGuiWithCurrentData();
                }
                _mainGui.IsActive = requiredKeysPressed;
            }
        }

        /// <summary>Injects current information into the GUI for facilities and locations.</summary>
        private void LoadGuiWithCurrentData()
        {
            _map.Refresh();
            _mainGui.Facilities = _map.AvailableFacilities;
            _mainGui.Locations = _map.AvailableLocations;

            _items.Refresh();
            _mainGui.Items = _items;
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
