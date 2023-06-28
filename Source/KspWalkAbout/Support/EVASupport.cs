/*
	This file is part of Walk About /L Unleashed
		© 2023 Lisias T : http://lisias.net <support@lisias.net>

	Walk About /L Unleashed is licensed as follows:
		* GPL 3.0 : https://www.gnu.org/licenses/gpl-3.0.txt

	Walk About /L Unleashed is distributed in the hope that
	it will be useful, but WITHOUT ANY WARRANTY; without even the implied
	warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the GNU General Public License 3.0
	along with Walk About /L Unleashed. If not, see <https://www.gnu.org/licenses/>.

*/

namespace KspWalkAbout
{
	public class EVASupport
	{
		public interface Interface
		{
			bool isAchored(Vessel v);
			void removeAnchor(Vessel v);
		}

		internal static readonly Interface Instance;
		private static Interface GetInstance()
		{
			Log.dbg("Looking for {0}", typeof(Interface).Name);
			Interface r = (Interface)KSPe.Util.SystemTools.Interface.CreateInstanceByInterface(typeof(Interface));
			if (null == r) Log.error("No realisation for the EVASupport Interface found! We are doomed!");
			return r;
		}
		static EVASupport()
		{
			Instance = GetInstance();
		}

		internal bool IsAnchored(Vessel v) => Instance.isAchored(v);
		internal void RemoveAnchor(Vessel v) => Instance.removeAnchor(v);
	}
}
