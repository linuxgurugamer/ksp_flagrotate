using UnityEngine;

namespace FlagRotate
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class ModuleFlagRotator : PartModule
    {
        private FlagSite _flagModule;

// ReSharper disable ConvertToConstant.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable MemberCanBePrivate.Global
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
            // will happen if we try to rotate

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
               
            // note: rotate around the axis defined by the ground pivot. Otherwise the flag will try to rotate around
            // its approximate center which doesn't make much sense to the player
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
                                "This flag appears to be defective. Or incorrectly planted. Probably the second one.",
                                "Rotating a fallen flag doesn't seem like it will work."
                                
                    }
                    [UnityEngine.Random.Range(0, 6)],
                    5f,
                    ScreenMessageStyle.UPPER_CENTER));
        }


        private void BadRootPartError()
        {
            Debug.LogWarning("Can't rotate flag: flag is not root part in vessel");
        }
    }
}
