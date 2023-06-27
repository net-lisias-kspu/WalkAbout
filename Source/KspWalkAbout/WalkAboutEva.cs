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
using KspWalkAbout.Extensions;
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
                "WalkAboutEva deactivated: not a valid Kerbal EVA".Debug();
                return;
            }

            SetPerpetualMotionKeyCombo();
            _currentMotion = new MotionSettings()
            {
                State = MotionState.normal,
                IsRunning = false,
            };

            ProtoCrewMember kerbalPcm = FlightGlobals.ActiveVessel.GetVesselCrew()[0];
            $"Flight scene started for {kerbalPcm.name}".Debug();

            System.Reflection.Assembly KisMod = null;
            if (!WalkAboutKspAccess.TryGetKisMod(ref KisMod))
            {
                $"KIS not installed".Debug();
                return;
            }
            $"obtained KIS mod assembly [{KisMod}]".Debug();

            if (WalkAboutPersistent.AllocatedItems.ContainsKey(kerbalPcm.name))
            {
                AddInventoryItems(kerbalPcm, KisMod);
            }
            else
            {
                $"{kerbalPcm.name} has no items to add to inventory".Debug();
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
                $"Perpetual Motion Key set to {(_perpetualMotionKeys?.Key ?? KeyCode.None)}".Debug();
            }
        }

        private void AddInventoryItems(ProtoCrewMember kerbalPcm, System.Reflection.Assembly KIS)
        {
            $"{kerbalPcm.name} has {WalkAboutPersistent.AllocatedItems[kerbalPcm.name].Count} items to be assigned".Debug();
            System.Type ModuleKISInventoryType = KIS.GetType("KIS.ModuleKISInventory");
            "found KIS inventory type".Debug();
            Component inventory = FlightGlobals.ActiveVessel.GetComponent(ModuleKISInventoryType);
            "obtained modules for the active vessel".Debug();

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
            $"{kerbalPcm.name} has a {itemName} to be added".Debug();

            Part part = PartLoader.getPartInfoByName(itemName)?.partPrefab;
            if (part == null)
            {
                "Cannot add item to inventory".Debug();
                return;
            }

            $"invoking AddItem member using (part [{part.GetType()}])".Debug();
            object item =
                KisType.InvokeMember(
                    "AddItem",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    inventory,
                    new object[] { part, 1f, -1 });
            $"{itemName} is in the inventory as {item}".Debug();
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
                $"Set motion state to {motion.State}".Debug();
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
                $"Forcing TimeWarp from mode:HIGH rate:{TimeWarp.CurrentRateIndex} to mode:LOW, rate {rate}".Debug();
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