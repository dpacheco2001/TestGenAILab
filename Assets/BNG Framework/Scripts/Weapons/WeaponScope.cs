using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BNG {
    public class WeaponScope : GrabbableEvents {

        public Camera scopeCamera;

        // Max distance to render away from player eye
        public float MaxCameraDistance = 10f;

        Camera mainCamera;
        
        bool awaitingEnable = false;

        void Start() {
            // Start disabled for better performance. We'll enable while held or attached to something
            scopeCamera.enabled = false;
            
            mainCamera = Camera.main;
        }

        void Update() {
            // Disable if far away
            if(awaitingEnable == false && scopeCamera.enabled && mainCamera != null) {
                if(Vector3.Distance(scopeCamera.transform.position, mainCamera.transform.position) > MaxCameraDistance) {
                    mainCamera.enabled = false;
                    awaitingEnable = true;
                }
            }

            // Awaiting on enable
            if(awaitingEnable) {
                if (Vector3.Distance(scopeCamera.transform.position, mainCamera.transform.position) < MaxCameraDistance) {
                    mainCamera.enabled = true;
                    awaitingEnable = false;
                }
            }
        }

        public override void OnGrab(Grabber grabber) {
            scopeCamera.enabled = true;
            base.OnGrab(grabber);
        }

        public override void OnRelease() {
            scopeCamera.enabled = false;
            base.OnRelease();
        }

        public override void OnSnapZoneEnter() {
            scopeCamera.enabled = true;
            base.OnSnapZoneEnter();
        }

       
        public override void OnSnapZoneExit() {
            scopeCamera.enabled = false;
            base.OnSnapZoneExit();
        }
    }
}

