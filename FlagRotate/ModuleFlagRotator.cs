using System.Linq;
using UnityEngine;

namespace FlagRotate
{
    public class ModuleFlagRotator : PartModule
    {
        private FlagSite _flagModule;

        [KSPField] public float DeltaAngle = 5f;
        [KSPField] public string ClockwiseName = "Clockwise";
        [KSPField] public string CounterclockwiseName = "Counterclockwise";

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
        
            _flagModule = GetComponent<FlagSite>();

            if (!string.IsNullOrEmpty(ClockwiseName)) Events["Clockwise"].guiName = ClockwiseName;
            if (!string.IsNullOrEmpty(CounterclockwiseName)) Events["Counterclockwise"].guiName = CounterclockwiseName;
        }



        [KSPEvent(guiActive = true, guiName = "Counter-Clockwise", active = true, guiActiveUnfocused = true,
            unfocusedRange = 2f, externalToEVAOnly = true)]
        public void Counterclockwise()
        {
            Rotate(-DeltaAngle);
        }


        [KSPEvent(guiActive = true, guiName = "Clockwise", active = true, guiActiveUnfocused = true,
            unfocusedRange = 2f, externalToEVAOnly = true)]
        public void Clockwise()
        {
            Rotate(DeltaAngle);
        }



        private void Rotate(float angle)
        {
            // test for existence of the ground joint. If it doesn't exist, there's no telling what
            // will happen if we try to rotate so 

            if (gameObject.GetComponent<ConfigurableJoint>() == null)
            {
                JointNotSetError();
                return;
            }


            // a bit more defensive coding. If we're somehow not in the root part of the vessel, who knows what we might
            // be rotating?
            if (vessel.rootPart != part)
            {
                BadRootPartError();
                return;
            }

            
        
            _flagModule.SendMessage("UnsetJoint");

            var deltaRotation = Quaternion.AngleAxis(angle, transform.up);
               
            // note: it's important to fix position else the flag won't rotate around the pole as the player would expect
            vessel.SetPosition(deltaRotation * (vessel.vesselTransform.position - _flagModule.groundPivot.position) +
                               _flagModule.groundPivot.position);
            vessel.SetRotation(deltaRotation * vessel.vesselTransform.rotation);

            _flagModule.SendMessage("SetJoint");
        }


        private void JointNotSetError()
        {
            ScreenMessages.PostScreenMessage(
                new ScreenMessage(
                    new[] { "That seems pointless. The insolent flag remains motionless.",
                                "Manufacturer's tip: place pointy end into the ground before adjusting flag orientation.",
                                string.Format("User error: {0} lacks the intelligence for this task", FlightGlobals.ActiveVessel.vesselName),
                                "User error: replace user and try again",
                                "This flag appears to be defective. Or incorrectly planted. Probably the second one." }
                    [UnityEngine.Random.Range(0, 5)],
                    5f,
                    ScreenMessageStyle.UPPER_CENTER));
        }


        private void BadRootPartError()
        {
            Debug.LogWarning("Can't rotate flag: flag is not root part in vessel");
        }
    }
}
