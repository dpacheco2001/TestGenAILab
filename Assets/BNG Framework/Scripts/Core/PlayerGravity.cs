using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {

    /// <summary>
    /// Apply Gravity to a CharacterController or RigidBody
    /// </summary>
    public class PlayerGravity : MonoBehaviour {

        [Header("Gravity Settings")]
        [Tooltip("If true, will apply gravity to the CharacterController component, or RigidBody if no CC is present.")]
        public bool GravityEnabled = true;

        [Tooltip("Amount of Gravity to apply to the CharacterController or Rigidbody. Default is 'Physics.gravity'.")]
        public Vector3 Gravity = Physics.gravity;

        CharacterController characterController;
        SmoothLocomotion smoothLocomotion;

        Rigidbody playerRigidbody;

        private float _movementY;
        private Vector3 _initialGravityModifier, _previousGravityModifier;

        // Save us a null check in FixedUpdate
        private bool _validRigidBody = false;

        [Header("Debug")]
        [Tooltip("Optional tooltip to output current gravity value. Useful for testing.")]
        public TMPro.TMP_Text LabelToUpdate;


        void Start() {
            characterController = GetComponent<CharacterController>();
            smoothLocomotion = GetComponentInChildren<SmoothLocomotion>();
            playerRigidbody = GetComponent<Rigidbody>();

            _validRigidBody = playerRigidbody != null;

            _initialGravityModifier = Gravity;
            _previousGravityModifier = Gravity;
        }

        // Apply Gravity in LateUpdate to ensure it gets applied after any character movement is applied in Update
        void LateUpdate() {

            // Apply Gravity to Character Controller
            if (GravityEnabled && characterController != null && characterController.enabled) {
                _movementY += Gravity.y * Time.deltaTime;

                // Default to smooth locomotion
                if(smoothLocomotion) {
                    smoothLocomotion.MoveCharacter(new Vector3(0, _movementY, 0) * Time.deltaTime);
                }
                // Fallback to character controller
                else if(characterController) {
                    characterController.Move(new Vector3(0, _movementY, 0) * Time.deltaTime);
                }
                
                // Reset Y movement if we are grounded
                if (characterController.isGrounded) {
                    _movementY = 0;
                }
            }

            if(LabelToUpdate != null) {
                LabelToUpdate.text = "Player Gravity : " + Gravity.y;
            }
        }

        void FixedUpdate() {
            // Apply Gravity to Rigidbody Controller
            if (_validRigidBody && GravityEnabled) {
                //playerRigidbody.AddRelativeForce(Gravity, ForceMode.VelocityChange);
                //playerRigidbody.AddForce(new Vector3(0, -Gravity.y * playerRigidbody.mass, 0));

                if(smoothLocomotion && smoothLocomotion.ControllerType == PlayerControllerType.Rigidbody && smoothLocomotion.GroundContacts < 1) {
                    
                }

                playerRigidbody.AddForce(Gravity * playerRigidbody.mass);
            }
        }

        public void ToggleGravity(bool gravityOn) {

            GravityEnabled = gravityOn;

            if (gravityOn) {
                Gravity = _previousGravityModifier;
            }
            else {
                // store gravity effect before update
                _previousGravityModifier = Gravity;
                Gravity = Vector3.zero;
            }
        }

        // GravityValue = 0-100f range
        public void SetGravityFromZeroToHundred(float GravityValue) {
            Gravity = new Vector3(0, -10 * (GravityValue / 100f), 0);
            GravityEnabled = GravityValue > 0;
        }
    }
}
