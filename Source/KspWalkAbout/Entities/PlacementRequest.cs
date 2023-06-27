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

using System.Collections.Generic;

namespace KspWalkAbout.Entities
{
    /// <summary>Represents a user's request to place a kerbal at a location.</summary>
    internal class PlacementRequest
    {
        /// <summary>The kerbal to be placed at the location.</summary>
        public ProtoCrewMember Kerbal { get; set; }

        /// <summary>The location where the kerbal is to be placed.</summary>
        public Location Location { get; set; }

        /// <summary>The items to be included in the kerbal's inventory.</summary>
        public List<InventoryItem> Items { get; set; }
    }
}