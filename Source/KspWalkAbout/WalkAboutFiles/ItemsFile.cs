/*
	This file is part of Walk About /L Unleashed
		© 2023 Lisias T : http://lisias.net <support@lisias.net>
		© 2016-2017 Antipodes (Clive Pottinger)

	Walk About /L Unleashed is licensed as follows:
		* GPL 3.0 : https://www.gnu.org/licenses/gpl-3.0.txt

	Walk About /L Unleashed is distributed in the hope that
	it will be useful, but WITHOUT ANY WARRANTY; without even the implied
	warranty of	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the GNU General Public License 3.0
	along with Walk About /L Unleashed. If not, see <https://www.gnu.org/licenses/>.

*/

using KspWalkAbout.Entities;
using KspWalkAbout.KspFiles;
using System.Collections.Generic;

namespace KspWalkAbout.WalkAboutFiles
{
    /// <summary>
    /// Represents a settings file containing information on all parts that can be used with the mod.
    /// </summary>
    public class ItemsFile : SettingsFile
    {
        /// <summary>Collection of all items that a kerbal may possibly have in inventory.</summary>
        [Persistent]
        public List<InventoryItem> Items;
    }
}