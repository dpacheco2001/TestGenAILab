using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BNG {
    public class WeaponSpinner : GrabbableEvents {

        // Spin this transform
        public Transform GraphicsTransform;

        public bool SpinOnButton1 = true;
        public float SpinSpeed = 720f;
        bool spinning = false;

        public bool IsSpinning {
            get {
                return spinning;
            }
        }

        public override void OnButton1Down() {
            // Kick off spin routine
            if (SpinOnButton1 && !spinning && GraphicsTransform != null) {
                DoSpin();
            }
        }
        public void DoSpin() {
            StartCoroutine(SpinGunRoutine());
        }

        public IEnumerator SpinGunRoutine() {

            spinning = true;

            float totalRotation = 0f;

            GraphicsTransform.localEulerAngles = Vector3.zero;

            bool reverseSpin = false;
            if (InputBridge.Instance.GetControllerAngularVelocity(thisGrabber.HandSide).x < 0) {
                reverseSpin = true;
                //Debug.Log("Reverse Spin");
                //Debug.Log("Angular Velocity of " + InputBridge.Instance.GetControllerAngularVelocity(thisGrabber.HandSide).magnitude);
            }

            // Set the direction of spin based on reverseSpin
            float direction = reverseSpin ? -1f : 1f;

            // Spin until 360 degrees is reached
            while (totalRotation < 360f) {
                // Calculate the rotation for this frame
                float currentRotation = SpinSpeed * Time.deltaTime * direction;

                // Check for overshot
                currentRotation = Mathf.Min(currentRotation, (360f - totalRotation) * Mathf.Abs(direction));

                // Apply rotation (note that we're using the modified currentRotation here)
                GraphicsTransform.Rotate(currentRotation, 0, 0, Space.Self);

                // Accumulate total rotation (use absolute value since totalRotation is always positive)
                totalRotation += Mathf.Abs(currentRotation);

                yield return new WaitForEndOfFrame();
            }


            // Fished rotation, so set it back
            GraphicsTransform.localEulerAngles = Vector3.zero;

            spinning = false;
        }
    }

}
