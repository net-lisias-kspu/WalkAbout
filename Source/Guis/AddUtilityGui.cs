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

using UnityEngine;
using KspWalkAbout.Locations;
using KspAccess;

namespace KspWalkAbout.Guis
{
    internal class AddUtilityGui
    {
        private Rect _coordinates;
        private Vector2 _facilitySelectorScrollPosition;
        private string _enteredLocationName;
        private readonly string _unenteredText;
        private string _previousText;
        private bool _textChanged;
        private bool _existingName;

        private string _selectedFacility;
        private GUIStyle _validLabelStyle;
        private GUIStyle _invalidLabelStyle;
        private GUIStyle _actionableButtonStyle;
        private GUIStyle _invalidButtonStyle;
        private bool _isStylesLoaded;

        internal AddUtilityGui(KnownPlaces locationMap)
        {
            _facilitySelectorScrollPosition = Vector2.zero;
            _unenteredText = "Location Name";

            _enteredLocationName = _unenteredText;
            _selectedFacility = string.Empty;
            _isStylesLoaded = true;

            Map = locationMap;
            GuiCoordinates = new Rect();
            IsActive = false;
        }

        internal Rect GuiCoordinates
        {
            get { return _coordinates; }
            set
            {
                _coordinates = (value == new Rect())
                    ? new Rect(Screen.width / 4, Screen.height / 4, Screen.width / 4, Screen.height / 2)
                    : value;
            }
        }
        internal bool IsActive { get; set; }
        internal KnownPlaces Map { get; set; }
        internal LocationRequest RequestedLocation { get; set; }

        internal bool Display(ref GuiState state)
        {
            state = IsActive 
                ? (CommonKspAccess.IsPauseMenuOpen ? GuiState.offPauseOpen : state)
                : GuiState.offNotActive;
            if (!IsActive || CommonKspAccess.IsPauseMenuOpen) return false;

            GuiCoordinates = GUI.Window(0, GuiCoordinates, DrawSelectorHandler, $"{Constants.ModName} - Add");
            state = GuiState.displayed;
            return true;
        }

        private void DrawSelectorHandler(int id)
        {
            if (_isStylesLoaded) GetGuiElementStyles();

            GUILayout.BeginVertical();
            DrawLocationNameInput();
            DrawFacilitySelector();
            DrawActionButton();
            DrawCancelButton();
            DrawClosestLocation();
            GUILayout.EndVertical();
            GuiResizer.DrawResizingButton(GuiCoordinates);
            GUI.DragWindow(new Rect(0, 0, GuiCoordinates.width, GuiCoordinates.height));
        }

        private void DrawLocationNameInput()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Enter new location Name");
            var beganWithUserText = (_enteredLocationName != _unenteredText);
            _enteredLocationName = GUILayout.TextField(_enteredLocationName, beganWithUserText ? _validLabelStyle : _invalidLabelStyle).Trim();
            GUILayout.EndHorizontal();

            _textChanged = (_enteredLocationName != _previousText);
            _previousText = _enteredLocationName;

            if (!_textChanged) return;

            if (string.IsNullOrEmpty(_enteredLocationName))
            {
                _enteredLocationName = _unenteredText;
            }
            else if (!beganWithUserText)
            {
                foreach (var letter in _unenteredText)
                {
                    var index = _enteredLocationName.IndexOf(letter);
                    if (index != -1)
                    {
                        _enteredLocationName = _enteredLocationName.Remove(index, 1);
                    }
                }
            }

            _existingName = Map.HasLocation(_enteredLocationName);
        }

        private void DrawFacilitySelector()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Facilities:");
            _facilitySelectorScrollPosition = GUILayout.BeginScrollView(_facilitySelectorScrollPosition);
            GUILayout.BeginVertical();
            foreach (var facilityName in ScenarioUpgradeableFacilities.protoUpgradeables.Keys)
            {
                if (GUILayout.Button(facilityName))
                {
                    _selectedFacility = facilityName; $"selected {_selectedFacility}".Debug();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }

        private void DrawActionButton()
        {
            string label = null;
            if (_enteredLocationName == _unenteredText)
            {
                label = "Enter a new location name";
            }
            if (string.IsNullOrEmpty(_selectedFacility))
            {
                label = (string.IsNullOrEmpty(label) ? "Select" : $"{label} and select") + " a facility";
            }

            if (GUILayout.Button(
                label ?? $"Add location {_enteredLocationName}",
                ((string.IsNullOrEmpty(label) || _existingName) ? _actionableButtonStyle : _invalidButtonStyle))
                && string.IsNullOrEmpty(label))
            {
                $"Action selected: create location {_enteredLocationName} for {_selectedFacility}".Debug();
                RequestedLocation = new LocationRequest { Name = _enteredLocationName, AssociatedFacility = _selectedFacility };
                _enteredLocationName = _unenteredText;
                _selectedFacility = string.Empty;
                IsActive = false;
                "Location request created - closing GUI".Debug();
            }
        }

        private void DrawCancelButton()
        {
            if (GUILayout.Button("Cancel"))
            {
                IsActive = false;
            }
        }

        private void DrawClosestLocation()
        {
            var closest = KnownPlaces.FindClosest(FlightGlobals.ActiveVessel.latitude,
                                              FlightGlobals.ActiveVessel.longitude,
                                              FlightGlobals.ActiveVessel.altitude,
                                              Map);
            var text = "Closest known locations:";
            var suffix = " none found";
            for (var level = 1; level < 4; level++)
            {
                if (string.IsNullOrEmpty(closest[level].Name)) continue;

                text += $"\nLevel {level} ({closest[level].Name} dh:{closest[level].Horizontal:0.00}m dv:{closest[level].Vertical:0.00}m";
                suffix = string.Empty;
            }

            text += suffix;
            GUILayout.TextArea(text);
        }

        private void GetGuiElementStyles()
        {
            _validLabelStyle = new GUIStyle(GUI.skin.label);
            _invalidLabelStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.yellow } };

            _actionableButtonStyle = new GUIStyle(GUI.skin.button) { normal = { textColor = Color.green } };
            _invalidButtonStyle = new GUIStyle(GUI.skin.button) { normal = { textColor = Color.yellow } };

            _isStylesLoaded = false;
        }
    }
}