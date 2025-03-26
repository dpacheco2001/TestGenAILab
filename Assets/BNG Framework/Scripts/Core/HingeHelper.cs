using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BNG {
    public class HingeHelper : GrabbableEvents {

        [Header("Snap Options")]
        [Tooltip("If True the SnapGraphics tranfsorm will have its local Y rotation snapped to the nearest degrees specified in SnapDegrees")]
        public bool SnapToDegrees = false;

        [Tooltip("Snap the Y rotation to the nearest")]
        public float SnapDegrees = 5f;

        [Tooltip("The Transform of the object to be rotated if SnapToDegrees is true")]
        public Transform SnapGraphics;

        [Tooltip("Play this sound on snap")]
        public AudioClip SnapSound;

        [Tooltip("Randomize pitch of SnapSound by this amount")]
        public float RandomizePitch = 0.001f;

        [Tooltip("Add haptics amount (0-1) to controller if SnapToDegrees is True. Set this to 0 for no Haptics.")]
        public float SnapHaptics = 0.5f;

        [Header("Text Label (Optional)")]
        public Text LabelToUpdate;

        [Header("Change Events")]
        public FloatEvent onHingeChange;
        public FloatEvent onHingeSnapChange;

        [Header("Angle Limits")]
        [Tooltip("Enable angle limits")]
        public bool useLimits = false;
        public float minAngle = -180f;
        public float maxAngle = 180f;

        Rigidbody rigid;

        private float _lastDegrees = 0;
        private float _lastSnapDegrees = 0;
        private float _accumulatedAngle = 0f;
        private bool _isFirstUpdate = true;

        void Start() {
            rigid = GetComponent<Rigidbody>();
            _accumulatedAngle = ConvertTo180Range(transform.localEulerAngles.y);
        }

        void Update() {
            float rawDegrees = transform.localEulerAngles.y;
            float currentAngle;

            if (useLimits) {
                // Convertir a rango -180 a 180
                currentAngle = ConvertTo180Range(rawDegrees);
                
                // Aplicar límites
                currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

                // Si está en el límite, mantener el ángulo acumulado
                if (currentAngle == minAngle || currentAngle == maxAngle) {
                    currentAngle = _accumulatedAngle;
                }
            }
            else {
                // Calcular el cambio de ángulo desde la última actualización
                float deltaAngle = Mathf.DeltaAngle(_lastDegrees, rawDegrees);
                _accumulatedAngle += deltaAngle;
                currentAngle = _accumulatedAngle;
            }

            // Solo actualizar si hay cambio real en el ángulo
            if (currentAngle != _lastDegrees) {
                OnHingeChange(currentAngle);
            }

            _lastDegrees = currentAngle;
            _accumulatedAngle = currentAngle;

            // Check for snapping
            if (SnapToDegrees) {
                float nearestSnap = Mathf.Round(currentAngle / SnapDegrees) * SnapDegrees;
                if (nearestSnap != _lastSnapDegrees) {
                    OnSnapChange(nearestSnap);
                }
                _lastSnapDegrees = nearestSnap;
            }

            // Update label
            if (LabelToUpdate) {
                float val = SnapToDegrees ? _lastSnapDegrees : currentAngle;
                LabelToUpdate.text = val.ToString("n0");
            }
        }

        public void OnSnapChange(float yAngle) {

            if(SnapGraphics) {
                SnapGraphics.localEulerAngles = new Vector3(SnapGraphics.localEulerAngles.x, yAngle, SnapGraphics.localEulerAngles.z);
            }

            if(SnapSound) {
                VRUtils.Instance.PlaySpatialClipAt(SnapSound, transform.position, 1f, 1f, RandomizePitch);
            }

            if(grab.BeingHeld && SnapHaptics > 0) {
                InputBridge.Instance.VibrateController(0.5f, SnapHaptics, 0.01f, thisGrabber.HandSide);                    
            }

            // Call event
            if (onHingeSnapChange != null) {
                onHingeSnapChange.Invoke(yAngle);
            }
        }

        public override void OnRelease() {
            rigid.linearVelocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;

            base.OnRelease();
        }

        public void OnHingeChange(float hingeAmount) {
            // Call event
            if (onHingeChange != null) {
                onHingeChange.Invoke(hingeAmount);
            }
        }

        // Convert angle from 0-360 to -180 to +180 range
        private float ConvertTo180Range(float angle) {
            if (angle > 180f) {
                return angle - 360f;
            }
            return angle;
        }
    }
}

