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
using KspWalkAbout.Locations;
using System.Collections.Generic;
using UnityEngine;

namespace KspWalkAbout.Guis
{
    internal class MainGui
    {
        private Rect _coordinates;
        private Vector2 _kerbalSelectorScrollPosition;
        private Vector2 _facilitySelectorScrollPosition;
        private Vector2 _locationSelectorScrollPosition;
        private ProtoCrewMember _selectedKerbal;
        private string _selectedFacility;
        private Location _selectedLocation;
        private GUIStyle _actionableButtonStyle;
        private GUIStyle _validButtonStyle;
        private GUIStyle _invalidButtonStyle;
        private bool _needStyles;
        private bool _showTopFewOnly;
        private string _windowTitle;
        private Vector2 _minGuiSize;

        internal MainGui()
        {
            _kerbalSelectorScrollPosition = Vector2.zero;
            _facilitySelectorScrollPosition = Vector2.zero;
            _locationSelectorScrollPosition = Vector2.zero;

            _needStyles = true;
            _showTopFewOnly = false;
            _windowTitle = $"{Constants.ModName} v{Constants.Version}";
            _minGuiSize = new Vector2(200, 50);

            IsActive = false;
            Locations = new List<Location>();
        }

        internal Rect GuiCoordinates
        {
            get { return _coordinates; }
            set
            {
                if (value == new Rect()) "given empty screen position - defaulting to 1/4 screen".Debug();
                _coordinates = (value == new Rect())
                    ? new Rect(Screen.width / 4, Screen.height / 4, Screen.width / 3, Screen.height / 4)
                    : value;
            }
        }
        internal bool IsActive { get; set; }
        internal List<string> Facilities { get; set; }
        internal List<Location> Locations { get; set; }
        public int TopFew { get; internal set; }
        public PlacementRequest RequestedPlacement { get; set; }

        internal bool Display(ref GuiState state)
        {
            if (!IsActive) return false;

            GuiCoordinates = GUI.Window(0, GuiCoordinates, DrawSelectorHandler, _windowTitle);
            state = GuiState.displayed;
            return true;
        }

        private void DrawSelectorHandler(int id)
        {
            if (_needStyles) GetGuiElementStyles();

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            var haveKerbals = DrawKerbalSelector();
            DrawFacilitySelector();
            var haveLocations = DrawLocationSelector();
            GUILayout.EndHorizontal();
            DrawActionButton(haveKerbals, haveLocations);
            DrawCancelButton();
            GUILayout.EndVertical();
            GuiResizer.DrawResizingButton(_coordinates, _minGuiSize);
            GUI.DragWindow(new Rect(0, 0, GuiCoordinates.width, GuiCoordinates.height));
        }

        private bool DrawKerbalSelector()
        {
            var drewKerbalButtons = false;
            _kerbalSelectorScrollPosition = GUILayout.BeginScrollView(_kerbalSelectorScrollPosition);
            GUILayout.BeginVertical();
            foreach (var crew in HighLogic.CurrentGame.CrewRoster.Crew)
            {
                if (crew.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                {
                    if (GUILayout.Button(crew.name))
                    {
                        _selectedKerbal = crew; $"selected {_selectedKerbal.name}".Debug();
                    }
                    drewKerbalButtons = true;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            return drewKerbalButtons;
        }

        private void DrawFacilitySelector()
        {
            _facilitySelectorScrollPosition = GUILayout.BeginScrollView(_facilitySelectorScrollPosition);
            GUILayout.BeginVertical();
            foreach (var facilityName in Facilities)
            {
                if (GUILayout.Button(
                    facilityName.Substring(facilityName.IndexOf('/') + 1),
                    (facilityName == (_selectedFacility ?? facilityName)) ? _actionableButtonStyle : _validButtonStyle))
                {
                    _selectedFacility = (facilityName == _selectedFacility) ? null : facilityName;
                    $"selected {_selectedFacility}".Debug();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        private bool DrawLocationSelector()
        {
            GUILayout.BeginVertical();
            if ((TopFew > 0) && (Locations.Count > TopFew))
            {
                _showTopFewOnly = (GUILayout.Toggle(_showTopFewOnly, $"Top {TopFew} Only"));
            }
            else
            {
                _showTopFewOnly = false;
            }

            var locationButtonsDrawn = 0;
            _locationSelectorScrollPosition = GUILayout.BeginScrollView(_locationSelectorScrollPosition);
            GUILayout.BeginVertical();
            foreach (var location in Locations)
            {
                if (string.IsNullOrEmpty(_selectedFacility) || (location.FacilityName == _selectedFacility))
                {
                    if (GUILayout.Button(location.LocationName))
                    {
                        _selectedLocation = location; $"selected {_selectedLocation.LocationName}".Debug();
                    }
                    if ((++locationButtonsDrawn == TopFew) && _showTopFewOnly) break;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            return locationButtonsDrawn > 0;
        }

        private void DrawActionButton(bool haveKerbals, bool haveLocations)
        {
            if (!haveKerbals || !haveLocations)
            {
                GUILayout.Button(haveKerbals ? "No locations found" : "No available kerbals", _invalidButtonStyle);
            }
            else if (_selectedKerbal == null || _selectedLocation == null)
            {
                GUILayout.Button(string.Format(
                    "Select a {0}{1}",
                    _selectedKerbal == null ? "kerbal" : "location",
                    _selectedKerbal == null && _selectedLocation == null ? " and a location" : string.Empty),
                    _invalidButtonStyle);
            }
            else
            {
                if (GUILayout.Button($"Place {_selectedKerbal.name} outside {_selectedLocation.LocationName}", _actionableButtonStyle))
                {
                    RequestedPlacement = new PlacementRequest { Kerbal = _selectedKerbal, Location = _selectedLocation };
                    "PlacementRequest created - deactivating GUI".Debug();
                    IsActive = false;
                }
            }
        }

        private void DrawCancelButton()
        {
            if (GUILayout.Button("Cancel"))
            {
                _selectedKerbal = null;
                _selectedLocation = null;
                IsActive = false;
            }
        }

        private void GetGuiElementStyles()
        {
            _actionableButtonStyle = new GUIStyle(GUI.skin.button) { normal = { textColor = Color.green } };
            _validButtonStyle = new GUIStyle(GUI.skin.button);
            _invalidButtonStyle = new GUIStyle(GUI.skin.button) { normal = { textColor = Color.yellow } };

            _needStyles = false;
        }
    }
}