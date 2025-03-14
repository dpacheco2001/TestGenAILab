using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BNG {
    public class CameraDisabler : MonoBehaviour {

        [Header("Camera")]
        public Camera TargetCamera;

        [Header("Visibility Checks")]
        public bool MeshRenderCheck = false;

        public MeshRenderer RenderCheck;

        public bool DistanceCheck = true;
        public float MaxDistance = 20f;
        Camera mainCamera;

        void Awake() {
            mainCamera = Camera.main;
        }

        void Update() {
            // Show if mesh is visible
            bool showCamera = true;
            if(MeshRenderCheck == true && RenderCheck != null && !RenderCheck.isVisible) {                
                showCamera = false;
            }

            if(DistanceCheck) {
                if(mainCamera == null) {
                    mainCamera = Camera.main;
                }

                if(mainCamera != null) {
                    if(Vector3.Distance(transform.position, mainCamera.transform.position) > MaxDistance) {
                        showCamera = false;
                    }

                }
            }
            TargetCamera.enabled = showCamera;
        }
    }
}

