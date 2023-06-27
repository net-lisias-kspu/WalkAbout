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

using System;

namespace KspWalkAbout.Values
{
    /// <summary>Enumerates the possible combinations of facility upgrade levels.</summary>
    [Flags]
    public enum FacilityLevels
    {
        None = 0x00,
        Level_1 = 0x01,
        Level_2 = 0x02,
        Levels_1_2 = 0x03,
        Level_3 = 0x04,
        Levels_1_3 = 0x05,
        Levels_2_3 = 0x06,
        Levels_1_2_3 = 0x07,
    }

    /// <summary>Enumerates the GUI's internal processing states.</summary>
    public enum GuiState { force, offNotActive, offPauseOpen, displayed };

    /// <summary>Enumerates the modes of walking/running/swimming used by the mod.</summary>
    public enum MotionState { perpetual, stopping, normal };

    /// <summary>Enumerates the actions that can taken after placing a kerbal at a location.</summary>
    public enum PostPlacementMode { reload, noreload, jumpto };
}
