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

using KspAccess;
using KspWalkAbout.Entities;
using KspWalkAbout.Values;
using UnityEngine;
using static KspAccess.CommonKspAccess;
using static KspWalkAbout.Entities.WalkAboutPersistent;

namespace KspWalkAbout
{
    /// <summary>
    /// Module to manage walking and inventory for a kerbal on EVA.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class WalkAboutEva : MonoBehaviour
    {
        private static KeyCombination _perpetualMotionKeys;
        private static MotionSettings _currentMotion;

        /// <summary>Detects if the scene is for a kerbal on EVA and adds any outstanding inventory items.</summary>
        public void Start()
        {
            KerbalEVA kerbalEva = GetKerbalEva();
            if (kerbalEva == null)
            {
                Log.detail("WalkAboutEva deactivated: not a valid Kerbal EVA");
                return;
            }

            SetPerpetualMotionKeyCombo();
            _currentMotion = new MotionSettings()
            {
                State = MotionState.normal,
                IsRunning = false,
            };

            ProtoCrewMember kerbalPcm = FlightGlobals.ActiveVessel.GetVesselCrew()[0];
            Log.detail("Flight scene started for {0}", kerbalPcm.name);

            System.Reflection.Assembly KisMod = null;
            if (!WalkAboutKspAccess.TryGetKisMod(ref KisMod))
            {
                Log.detail("KIS not installed");
                return;
            }
            Log.detail("obtained KIS mod assembly [{0}]", KisMod);

            if (WalkAboutPersistent.AllocatedItems.ContainsKey(kerbalPcm.name))
            {
                AddInventoryItems(kerbalPcm, KisMod);
            }
            else
            {
                Log.detail("{0} has no items to add to inventory", kerbalPcm.name);
            }
        }

        /// <summary>Called each time the game state is updated.</summary>
        public void FixedUpdate()
        {
            KerbalEVA kerbalEva = GetKerbalEva();
            if (!AreFlightConditionsMet(kerbalEva))
            {
                return;
            }

            AlterMotionAsPerUserKeystrokes(_currentMotion, _perpetualMotionKeys);

            if (_currentMotion.State != MotionState.normal)
            {
                ChangeHighWarpToPhysicsWarp();
                MoveKerbal(kerbalEva, _currentMotion);
            }
        }

        private KerbalEVA GetKerbalEva()
        {
            if (!(FlightGlobals.ActiveVessel?.isEVA ?? false))
            {
                return null;
            }

            System.Collections.Generic.List<ProtoCrewMember> crew = FlightGlobals.ActiveVessel.GetVesselCrew();
            if ((crew?.Count ?? 0) != 1)
            {
                return null;
            }

            return FlightGlobals.ActiveVessel.GetComponent<KerbalEVA>();
        }

        private void SetPerpetualMotionKeyCombo()
        {
            if (_perpetualMotionKeys == null)
            {
                _perpetualMotionKeys = new KeyCombination()
                {
                    Key = GetModConfig().PmActivationHotKey,
                    Modifiers = GetModConfig().PmActivationHotKeyModifiers
                };
                Log.detail("Perpetual Motion Key set to {0}", _perpetualMotionKeys?.Key ?? KeyCode.None);
            }
        }

        private void AddInventoryItems(ProtoCrewMember kerbalPcm, System.Reflection.Assembly KIS)
        {
            Log.detail("{0} has {1} items to be assigned", kerbalPcm.name, WalkAboutPersistent.AllocatedItems[kerbalPcm.name].Count);
            System.Type ModuleKISInventoryType = KIS.GetType("KIS.ModuleKISInventory");
            Log.detail("found KIS inventory type");
            Component inventory = FlightGlobals.ActiveVessel.GetComponent(ModuleKISInventoryType);
            Log.detail("obtained modules for the active vessel");

            if (inventory != null)
            {
                foreach (string itemName in WalkAboutPersistent.AllocatedItems[kerbalPcm.name])
                {
                    AddItemToInventory(kerbalPcm, itemName, ModuleKISInventoryType, inventory);
                }
            }

            WalkAboutPersistent.AllocatedItems.Remove(kerbalPcm.name);
        }

        private void AddItemToInventory(ProtoCrewMember kerbalPcm, string itemName, System.Type KisType, Component inventory)
        {
            Log.detail("{0} has a {1} to be added", kerbalPcm.name, itemName);

            Part part = PartLoader.getPartInfoByName(itemName)?.partPrefab;
            if (part == null)
            {
                Log.detail("Cannot add item to inventory");
                return;
            }

            Log.detail("invoking AddItem member using (part [{0}])", part.GetType());
            object item =
                KisType.InvokeMember(
                    "AddItem",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    inventory,
                    new object[] { part, 1f, -1 });
            Log.detail("{0} is in the inventory as {1}", itemName, item);
        }

        private bool AreFlightConditionsMet(KerbalEVA kerbalEva)
        {
            return ((kerbalEva != null) // not a kerbal on EVA
                && (FlightGlobals.ActiveVessel.LandedOrSplashed)
                && (!kerbalEva.isRagdoll)
                && (FlightGlobals.currentMainBody?.GeeASL != null)); // avoids null reference on exit
        }

        private void AlterMotionAsPerUserKeystrokes(MotionSettings motion, KeyCombination perpetualMotionKeys)
        {
            if (IsKeyCombinationPressed(perpetualMotionKeys))
            {
                motion.State = (motion.State == MotionState.perpetual) ? MotionState.stopping : MotionState.perpetual;
                Log.detail("Set motion state to {0}", motion.State);
            }

            if (GameSettings.EVA_Run.GetKeyDown())
            {
                motion.IsRunning = !motion.IsRunning;
            }
        }

        private void ChangeHighWarpToPhysicsWarp()
        {
            if ((TimeWarp.WarpMode == TimeWarp.Modes.HIGH) && (TimeWarp.CurrentRate != 1))
            {
                int rate = Mathf.Min(4, TimeWarp.CurrentRateIndex);
                Log.detail("Forcing TimeWarp from mode:HIGH rate:{0} to mode:LOW, rate {1}", TimeWarp.CurrentRateIndex, rate);
                TimeWarp.fetch.Mode = TimeWarp.Modes.LOW;
                TimeWarp.SetRate(rate, true, true);
            }
        }

        private void MoveKerbal(KerbalEVA kerbalEva, MotionSettings currentMotion)
        {
            Animation currentAnimation = null;
            kerbalEva.GetComponentCached<Animation>(ref currentAnimation);

            Rigidbody rigidbody = null;
            kerbalEva.GetComponentCached<Rigidbody>(ref rigidbody);

            if ((currentAnimation == null) || (rigidbody == null))
            {
                return;
            }

            currentMotion = GetNewMotionSettings(kerbalEva, currentMotion);

            Quaternion orientation = kerbalEva.part.vessel.transform.rotation;
            Vector3 deltaPosition = orientation * Vector3.forward.normalized * (TimeWarp.deltaTime * currentMotion.Speed);

            currentAnimation.CrossFade(currentMotion.Animation);
            rigidbody.interpolation = RigidbodyInterpolation.Extrapolate;
            rigidbody.MovePosition(rigidbody.position + deltaPosition);
        }

        private MotionSettings GetNewMotionSettings(KerbalEVA kerbal, MotionSettings currentMotion)
        {
            double gforce = FlightGlobals.currentMainBody.GeeASL;
            MotionSettings newMotion = new MotionSettings
            {
                Animation = currentMotion.Animation,
                IsRunning = currentMotion.IsRunning,
                Speed = currentMotion.Speed,
                State = currentMotion.State,
            };

            if (currentMotion.State == MotionState.stopping)
            {
                newMotion.Animation = (kerbal.part.WaterContact) ? "swim_idle" : "idle";
                newMotion.Speed = 0;
                newMotion.State = MotionState.normal;
            }
            else if (kerbal.part.WaterContact)
            {
                newMotion.Animation = "swim_forward";
                newMotion.Speed = kerbal.swimSpeed;
            }
            else if (currentMotion.IsRunning && (gforce >= kerbal.minRunningGee))
            {
                newMotion.Animation = "wkC_run";
                newMotion.Speed = kerbal.runSpeed;
            }
            else if (gforce < kerbal.minWalkingGee)
            {
                newMotion.Animation = "wkC_loG_forward";
                newMotion.Speed = kerbal.boundSpeed;
            }
            else {
                newMotion.Animation = "wkC_forward";
                newMotion.IsRunning = false;
                newMotion.Speed = kerbal.walkSpeed;
            };

            return newMotion;
        }
    }
}