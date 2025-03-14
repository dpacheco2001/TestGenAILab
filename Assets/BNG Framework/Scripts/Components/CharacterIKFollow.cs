using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BNG {
    public class CharacterIKFollow : MonoBehaviour {

        [Tooltip("Typically the Camera / Eye to follow")]
        public Transform FollowTransform;

        // The reference for the player's actual up and forward direction
        [Tooltip("Use this to determine forward and upward direction")]
        public Transform PlayerTransform;

        // How tall the model is at 1.0 scale in meters / game units
        [Tooltip("How tall the model is at 1.0 scale in meters / game units")]
        public float BaseModelHeight = 1.65f;

        // User's current height in meters (default 1.65)
        [Tooltip("User's current height in meters (default 1.65)")]
        public float PlayerHeight = 1.65f;

        [Tooltip("Offset to place the character behind or in front of the PlayerTransform when looking straight ahead")]
        public Vector3 PositionOffset = new Vector3(0, 0, -0.1f);

        // Use this value when looking past degrees,
        [Tooltip("Offset to place the character behind or in front of the PlayerTransform when looking down at an angle, past 'DownLookAngle'")]
        public Vector3 LookDownOffset = new Vector3(0, 0, -0.2f);

        [Tooltip("The angle we should start lerping from between PositionOffset and LookDownOffset")]
        public float DownLookAngle = 45f;

        // Most we can look down
        [Tooltip("Max angle the player can look down. Typically <= 90 degrees")]
        public float MaxLookAngle = 90f;

        // Move the model up / down by this amount, independent of scale
        [Tooltip("Move the model up / down by this amount, independent of scale")]
        public float YPositionOffset = 0f;

        [Tooltip("Lerp for smooth movement, but may fall behind or jitter if moving too fast and LerpSpeed can't keep up. Set to false to update position in Update")]
        public bool UseLerp = true;
        public float LerpSpeed = 20f;
        public float RotationLerpSpeed = 10f;

        
        [Tooltip("Minimum degree change to trigger rotation lerp. When the player rotate past this number of degrees, then player will start to rotate. Useful to allow player to grab items off of their avatar, without the avatar rotating too quickly.")]
        public float ChangeDegrees = 5f;

        public bool UnparentOnStart = true;

        Quaternion lastRecordedRotation;
        Quaternion targetRotation;
        bool isRotating = false;

        void Start() {
            if (FollowTransform == null) {
                Debug.LogWarning("FollowTransform is not assigned. Please assign a camera or reference point.");
            }

            if (PlayerTransform == null) {
                Debug.LogWarning("PlayerTransform is not assigned. Please assign the player's reference point.");
            }

            if (UnparentOnStart) {
                transform.SetParent(null);
            }

            lastRecordedRotation = PlayerTransform.rotation;
            targetRotation = PlayerTransform.rotation;

            // KIck Off Initial Player Scale Adjustments
            AdjustPlayerScale();
        }

        void Update() {
            if (FollowTransform && PlayerTransform) {
                AdjustBodyPosition();
            }
        }

        Quaternion currentRotation;
        float angle = 0;

        public void AdjustBodyPosition() {
            Vector3 targetPosition = FollowTransform.position;
            Vector3 adjustOffset = PositionOffset;

            angle = Quaternion.Angle(FollowTransform.rotation, PlayerTransform.rotation);

            if (angle > DownLookAngle) {
                // Adjust LookdownOffset between DownLookAngle and 0, where  0 is full 
                adjustOffset = Vector3.Lerp(PositionOffset, LookDownOffset, (angle - DownLookAngle) / (MaxLookAngle - DownLookAngle));
            }

            // Add the offset to our target position 
            targetPosition += PlayerTransform.TransformDirection(adjustOffset);
            
            // Remove any Y offset
            targetPosition -= PlayerTransform.up * ((BaseModelHeight + YPositionOffset) * transform.localScale.y);

            // Could potentially use Unscaled time here
            if (UseLerp) {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * LerpSpeed);
            } 
            else {
                transform.position = targetPosition;
            }

            currentRotation = PlayerTransform.rotation;
            // player rotation is the target
            targetRotation = currentRotation;

            // Rotate if not in progress nad we gone past our angle threshold
            if (!isRotating && Quaternion.Angle(lastRecordedRotation, currentRotation) >= ChangeDegrees) {
                isRotating = true;
            }

            if (isRotating) {
                if (UseLerp) {
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * RotationLerpSpeed);
                } 
                else {
                    transform.rotation = targetRotation;
                }

                if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f) {
                    isRotating = false;
                    lastRecordedRotation = targetRotation;
                }
            }
            // Testing: Press Z to adjust scale
            //if (Input.GetKeyDown(KeyCode.Z)) {
            //    AdjustPlayerScale();
            //}
        }

        // Adjust player scale based on PlayerHeight and BaseModelHeight
        public void AdjustPlayerScale() {

            // May need an offset here.
            float playerScale = PlayerHeight / BaseModelHeight;
            transform.localScale = new Vector3(playerScale, playerScale, playerScale);

            Debug.Log(string.Format("Player scale adjusted. New scale factor: {0}. Applied scale: {1}", playerScale, transform.localScale));
        }

        public void UpdateBasePlayerHeight(float newPlayerHeight) {
            PlayerHeight = newPlayerHeight;
        }
    }
}



