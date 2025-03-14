using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {
    public class SnapZonePreview : MonoBehaviour {

        public string ObjectPrefix;

        SnapZone parentSnapZoneReference;

        public GameObject PreviewObject;

        public Grabbable OtherGrabbableMustBeGrabbed;

        Grabber leftGrabber;
        Grabber rightGrabber;

        bool wasEnabled;

        void Start() {

            parentSnapZoneReference = GetComponentInParent<SnapZone>();

            AssignGrabbers();

            wasEnabled = PreviewObject.activeSelf;
        }

        void Update() {
            if (parentSnapZoneReference != null) {
                UpdatePreviewVisibility();
            }
        }

        public virtual void UpdatePreviewVisibility() {

            bool shouldEnable = false;

            // Only enable if nothing currently in snapzone
            if(parentSnapZoneReference.HeldItem == null) {

                // If OtherGrabbableMustBeGrabbed is defined, make sure it's being held
                bool passesHoldCheck = OtherGrabbableMustBeGrabbed == null || (OtherGrabbableMustBeGrabbed != null && OtherGrabbableMustBeGrabbed.BeingHeld);

                // Check right grabber
                if (passesHoldCheck && rightGrabber != null && rightGrabber.HeldGrabbable != null && rightGrabber.HeldGrabbable.transform.name.StartsWith(ObjectPrefix)) {
                    shouldEnable = true;
                }
                // Check left grabber
                else if (passesHoldCheck && leftGrabber != null && leftGrabber.HeldGrabbable != null && leftGrabber.HeldGrabbable.transform.name.StartsWith(ObjectPrefix)) {
                    shouldEnable = true;
                }
            }

            // Update Status / Change enable
            if(shouldEnable != wasEnabled) {
                PreviewObject.SetActive(shouldEnable);

                wasEnabled = PreviewObject.activeSelf;
            }
        }

        public virtual void AssignGrabbers() {
            var player = GameObject.FindGameObjectWithTag("Player");
            if(player) {
                var grabbers = player.GetComponentsInChildren<Grabber>();
                foreach (var grabber in grabbers) {
                    if (grabber != null) {
                        if (grabber.HandSide == ControllerHand.Left) {
                            leftGrabber = grabber;
                        } 
                        else if (grabber.HandSide == ControllerHand.Right) {
                            rightGrabber = grabber;
                        }
                    }
                }
            }
        }
    }
}

