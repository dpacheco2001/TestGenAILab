using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BNG {
    public class XRHandsController : MonoBehaviour {

        public Grabber LeftPinchGrabber;
        public Grabber RightPinchGrabber;

        // Input Action for left / right pinch. 
        // Ex : <MetaAimHand>/{rightHand}/pinchStrengthIndex or <XRHandDevice>{RightHand}/pinchPosition        
        // https://docs.unity3d.com/Packages/com.unity.xr.hands@1.3/manual/features/metahandtrackingaim.html
        public InputAction OnLeftPinchAction;
        public InputAction OnRightPinchAction;

        // Show for debug
        public float LeftPinchAmount, RightPinchAmount;

        void OnEnable() {
            OnLeftPinchAction.Enable();
            OnRightPinchAction.Enable();
        }

        void OnDisable() {
            OnLeftPinchAction.Disable();
            OnRightPinchAction.Disable();
        }
     
        void Update() {
            LeftPinchAmount = OnLeftPinchAction.ReadValue<float>();
            RightPinchAmount = OnRightPinchAction.ReadValue<float>();
            
            if(RightPinchGrabber != null) {
                RightPinchGrabber.ForceGrab = RightPinchAmount == 1f;
            }

            if (LeftPinchGrabber != null) {
                LeftPinchGrabber.ForceGrab = LeftPinchAmount == 1f;
            }
        }
    }
}

