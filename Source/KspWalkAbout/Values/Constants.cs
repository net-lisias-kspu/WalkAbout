/*
	This file is part of Walk About /L Unleashed
		© 2023 Lisias T : http://lisias.net <support@lisias.net>
		© 2016-2017 Antipodes (Clive Pottinger)

	Walk About /L Unleashed is licensed as follows:
		* GPL 3.0 : https://www.gnu.org/licenses/gpl-3.0.txt

	Walk About /L Unleashed is distributed in the hope that
	it will be useful, but WITHOUT ANY WARRANTY; without even the implied
	warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the GNU General Public License 3.0
	along with Walk About /L Unleashed. If not, see <https://www.gnu.org/licenses/>.

*/

using KspWalkAbout.Entities;
using KspWalkAbout.WalkAboutFiles;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KspWalkAbout.Values
{
    /// <summary>
    /// Represents immutable values used by the WalkAbout mod.
    /// </summary>
    internal class Constants
    {
        internal static readonly string ModName = "WalkAbout";

        internal static readonly string Version =
            System.Diagnostics.FileVersionInfo
            .GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location)
            .ProductVersion;

        internal static readonly ConfigNode DefaultSettings =
            ConfigNode.CreateConfigFromObject(
                new WalkAboutSettings
                {
                    ActivationHotKey = KeyCode.W,
                    ActivationHotKeyModifiers = new List<KeyCode> { KeyCode.LeftControl, KeyCode.RightControl, },
                    Mode = "normal",
                    AUActivationHotKey = KeyCode.X,
                    AUActivationHotKeyModifiers = new List<KeyCode> { KeyCode.LeftControl, KeyCode.RightControl, },
                    ScreenX = (int)new Rect().xMin,
                    ScreenY = (int)new Rect().yMin,
                    ScreenWidth = (int)new Rect().width,
                    ScreenHeight = (int)new Rect().height,
                },
                new ConfigNode());

        internal static readonly ConfigNode DefaultItems =
            ConfigNode.CreateConfigFromObject(
                new ItemsFile
                {
                    Items = new List<InventoryItem>(),
                },
                new ConfigNode());

        internal static readonly Dictionary<double, FacilityLevels> LevelConversion =
            new Dictionary<double, FacilityLevels>()
            {
                { 0, FacilityLevels.Level_1 },
                { 0.5, FacilityLevels.Level_2 },
                { 1, FacilityLevels.Level_3 },
            };

        internal static readonly double DegreesToRadiansFactor = Math.PI / 180;
        internal static readonly double RadiansToDegreesFactor = 180 / Math.PI;
        internal static readonly double BaseBearingFlagToVAB = 94.3833870273274d;
        internal static readonly double BaseDistanceFlagToVAB = 357.331095037448d;
        internal static readonly double BaseDeltaAltFlagToVAB = 3.16247489920352d;
        internal static readonly int RoundingAccuracy = 7;

        internal static readonly string LocationFileExtension = "loc";
        internal static readonly string UserLocationFilename = $"user.{LocationFileExtension}";
        internal static readonly string UserLocationSubdirectory = "locFiles";
    }
}