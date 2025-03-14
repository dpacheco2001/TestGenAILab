using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {
    public class SliderLinear : MonoBehaviour {

        /// <summary>
        /// Minimum distance slide will travel on Z axis
        /// </summary>
        [Tooltip("Minimum distance slide will travel on Z axis")]
        public float MinLocalZ = -0.03f;

        /// <summary>
        /// Max distance slide will travel on Z axis
        /// </summary>
        [Tooltip("Max distance slide will travel on Z axis")]
        public float MaxLocalZ = 0;

        /// <summary>
        /// How fast to animate the slide back to 0 when slide is released. Set to 0 if you don't want to animate it
        /// </summary>
        [Tooltip("How fast to animate the slide back to 0 when slide is released. Set to 0 if you don't want to animate it")]
        public float AnimateSlideReturn = 0f;

        Grabbable thisGrabbable;
        Vector3 initialLocalPos;

        void Start() {
            thisGrabbable = GetComponent<Grabbable>();
            initialLocalPos = transform.localPosition;
        }

        void Update() {

            // Move towards held grabber
            if (thisGrabbable.GrabPhysics == GrabPhysics.None) {
                // Move towards grabber holding it
                UpdateHeldSlide();
            }

            // Clamp values
            if (transform.localPosition.z <= MinLocalZ) {
                transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, MinLocalZ);
                
            } 
            else if (transform.localPosition.z >= MaxLocalZ) {
                transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, MaxLocalZ);
            }
        }

        public virtual void UpdateHeldSlide() {

            // Move towards held grabber
            if (thisGrabbable.BeingHeld) {
                float moveSlideSpeed = 5f;
                transform.position = Vector3.MoveTowards(transform.position, thisGrabbable.GetGrabberVector3(thisGrabbable.GetPrimaryGrabber(), false), moveSlideSpeed * Time.deltaTime);

                // Only move on local axis
                transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, transform.localPosition.z);
                
            } 
            else {
                // Move back to 0
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, initialLocalPos, AnimateSlideReturn * Time.deltaTime);
            }

            // Make sure we cap min / max z
            if (transform.localPosition.z <= MinLocalZ) {
                transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, MinLocalZ);
            } 
            else if (transform.localPosition.z >= MaxLocalZ) {
                transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, MaxLocalZ);
            }
        }
    }
}
