using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {

    public class GrabPoint : MonoBehaviour {

        [Header("Hand Pose")]
        public HandPoseType handPoseType = HandPoseType.HandPose;

        [Tooltip("If HandPoseType = 'HandPose', this HandPose object will be applied to the hand when this grab point is in use")]
        public HandPose SelectedHandPose;

        /// <summary>
        /// Set to Default to inherit Grabbable's HandPose. Otherwise this HandPose will be used
        /// </summary>
        [Tooltip("If HandPoseType = 'AnimatorID', this id will be set on the hand animator when grabbed. Set to 'Default' to inherit the Grabbable's HandPose. Otherwise this HandPose ID will be used.")]
        public HandPoseId HandPose;

        [Header("Valid Hands")]
        [Tooltip("Can this Grab Point be used by a left-handed Grabber?")]
        public bool LeftHandIsValid = true;
       

        [Tooltip("Can this Grab Point be used by a right-handed Grabber?")]
        public bool RightHandIsValid = true;

        [Header("Left Hand Inversion")]
        [Tooltip("Allows you to use one grab point for both left and right hands. If left and right hands are both valid and this option is enabled, you can choose which axis to invert (typically local pos X, and local rotation Y and / or Z.")]
        public bool InvertLeftHand = false;

        // These are only used if InvertLeftHand = true
        // Typical default values are to invert
        [Header("Invert Local Position :")]
        public bool InvertLeftPositionX = true;
        public bool InvertLeftPositionY = false;
        public bool InvertLeftPositionZ = false;

        [Header("Invert Local Rotation :")]
        public bool InvertLeftEulerX = false;
        public bool InvertLeftEulerY = true;
        public bool InvertLeftEulerZ = true;

        [Header("Parent to")]
        /// <summary>
        /// If specified, the Hand Model will be placed here when snapped
        /// </summary>
        [Tooltip("If specified, the Hand Model will be parented here when snapped")]
        public Transform HandPosition;

        [Header("Angle Restriction")]
        /// <summary>
        /// GrabPoint is not considered valid if the angle between the GrabPoint and Grabber is greater than this amount
        /// </summary>
        [Tooltip("GrabPoint is not considered valid if the angle between the GrabPoint and Grabber is greater than this amount")]
        [Range(0.0f, 360.0f)]
        public float MaxDegreeDifferenceAllowed = 360;

        [Header("Finger Blending")]
        [Tooltip("Minimum value Hand Animator will blend to. Example : If IndexBlendMin = 0.4 and Trigger button is not held down, the LayerWeight will be set to 0.4")]
        [Range(0.0f, 1.0f)]
        public float IndexBlendMin = 0;

        [Tooltip("Maximum value Hand Animator will blend to. Example : If IndexBlendMax = 0.6 and Trigger button is held all the way down, the LayerWeight will be set to 0.6")]
        [Range(0.0f, 1.0f)]
        public float IndexBlendMax = 0;

        [Tooltip("Minimum value Hand Animator will blend to if thumb control is not being touched.")]
        [Range(0.0f, 1.0f)]
        public float ThumbBlendMin = 0;

        [Tooltip("Maximum value Hand Animator will blend to if thumb control is being touched.")]
        [Range(0.0f, 1.0f)]
        public float ThumbBlendMax = 0;

        // Taken from defaults in Demo - offset between "Models" and Grabber
        Vector3 previewModelOffsetLeft = new Vector3(0.007f, -0.0179f, 0.0071f);// Old Offset = new Vector3(0.007f, -0.0179f, 0.0071f);
        Vector3 previewModelOffsetRight = new Vector3(-0.029f, 0.0328f, 0.044f);// Old Offset = new Vector3(-0.01f, -0.0179f, 0.0071f);

        [Header("Editor")]
        [Tooltip("Show a green arc in the Scene view representing MaxDegreeDifferenceAllowed")]
        public bool ShowAngleGizmo = true;

        [Tooltip("GameObject to use for previewing a grabpoint. Defaults is found in /Editor/Resources/")]
        public GameObject LeftHandPreviewPrefab;
        [Tooltip("GameObject to use for previewing a grabpoint. Defaults is found in /Editor/Resources/")]
        public GameObject RightHandPreviewPrefab;

        static string _defaultRightHandResourceName = "RightHandModelsEditorPreview";
        static string _defaultLeftHandResourceName = "LeftHandModelsEditorPreview";

        #region Editor
#if UNITY_EDITOR
        // Make sure animators update in the editor mode to show hand positions
        // By using OnDrawGizmosSelected we only call this function if the object is selected in the editor
        void OnDrawGizmosSelected() {
            DrawEditorArc();

            UpdatePreviews();
            //if (!Application.isPlaying) {
            //    UpdatePreviews();
            //}
        }

        // Update preview transform in editor in play mode as well
        //void Update() {
        //    UpdatePreviews();
        //}

        public void UpdatePreviews() {
            UpdateChildAnimators();
            UpdatePreviewTransforms();
            UpdateHandPosePreview();
            UpdateAutoPoserPreview();
        }

        /// <summary>
        /// Draw an arc in the editor representing MaxDegreeDifferenceAllowed
        /// </summary>
        public void DrawEditorArc() {

            // Draw arc representing the MaxDegreeDifferenceAllowed of the Grab Point
            if (ShowAngleGizmo && MaxDegreeDifferenceAllowed != 0 && MaxDegreeDifferenceAllowed != 360) {
                Vector3 from = Quaternion.AngleAxis(-0.5f * MaxDegreeDifferenceAllowed, transform.up) * (-transform.forward - Vector3.Dot(-transform.forward, transform.up) * transform.up);

                UnityEditor.Handles.color = new Color(0, 1, 0, 0.1f);
                UnityEditor.Handles.DrawSolidArc(transform.position, transform.up, from, MaxDegreeDifferenceAllowed, 0.05f);
            }
        }
#endif
        public GrabPoint SetupMirroredGrabPoint() {
            if (InvertLeftHand && LeftHandIsValid && RightHandIsValid) {

                // Don't invert since we can use this transform now
                LeftHandIsValid = false;
                InvertLeftHand = false;

                // Create new tranform object using this one as a reference
                GrabPoint leftHand = Instantiate(this.gameObject).GetComponent<GrabPoint>();

                leftHand.transform.parent = transform.parent;

                // Set up the left hand
                leftHand.LeftHandIsValid = true;
                leftHand.RightHandIsValid = false;

                leftHand.transform.localPosition = new Vector3(
                    InvertLeftPositionX ? -transform.localPosition.x : transform.localPosition.x,
                    InvertLeftPositionY ? -transform.localPosition.y : transform.localPosition.y,
                    InvertLeftPositionZ ? -transform.localPosition.z : transform.localPosition.z
                );


                leftHand.transform.localEulerAngles = new Vector3(
                    InvertLeftEulerX ? -transform.localEulerAngles.x : transform.localEulerAngles.x,
                    InvertLeftEulerY ? -transform.localEulerAngles.y : transform.localEulerAngles.y,
                    InvertLeftEulerZ ? -transform.localEulerAngles.z : transform.localEulerAngles.z
                );

                return leftHand;
            }

            return null;
        }

        bool offsetFound = false;

        public void UpdatePreviewTransforms() {
            Transform leftHandPreview = FindLeftHandPreview();
            Transform rightHandPreview = FindRightHandPreview();

            if (!offsetFound) {
                // If there is a Hand in the scene, use that offset instead of our defaults
                if (GameObject.Find("LeftController/Grabber") != null) {
                    Grabber LeftGrabber = GameObject.Find("LeftController/Grabber").GetComponent<Grabber>();
                    previewModelOffsetLeft = Vector3.zero - LeftGrabber.transform.localPosition;
                    // offsetFound = true;
                }

                if (GameObject.Find("RightController/Grabber") != null) {
                    Grabber RightGrabber = GameObject.Find("RightController/Grabber").GetComponent<Grabber>();
                    previewModelOffsetRight = Vector3.zero - RightGrabber.transform.localPosition;
                    // offsetFound = true;
                }
            }

            if (leftHandPreview) {
                leftHandPreview.localPosition = previewModelOffsetLeft;
                leftHandPreview.localEulerAngles = Vector3.zero;
            }

            if(rightHandPreview) {
                rightHandPreview.localPosition = previewModelOffsetRight;
                rightHandPreview.localEulerAngles = Vector3.zero;
            }
        }

        public void UpdateHandPosePreview() {
            if(handPoseType == HandPoseType.HandPose) {
                Transform leftHandPreview = FindLeftHandPreview();
                Transform rightHandPreview = FindRightHandPreview();

                if (leftHandPreview) {
                    HandPoser hp = leftHandPreview.GetComponentInChildren<HandPoser>(false);
                    if (hp != null) {
                        hp.CurrentPose = SelectedHandPose;
                    }
                }

                if (rightHandPreview) {
                    HandPoser hp = rightHandPreview.GetComponentInChildren<HandPoser>(false);
                    if (hp != null) {
                        hp.CurrentPose = SelectedHandPose;
                    }
                }

                //HandPoser hp2 = transform.GetComponentInChildren<HandPoser>(false);
                //if (hp2 != null) {
                //    hp2.CurrentPose = SelectedHandPose;
                //}
            }
        }

        public void UpdateAutoPoserPreview() {

            Transform leftHandPreview = FindLeftHandPreview();
            Transform rightHandPreview = FindRightHandPreview();

            if (handPoseType == HandPoseType.AutoPoseContinuous || handPoseType == HandPoseType.AutoPoseOnce) {
                // Update in editor
                if (leftHandPreview) {
                    AutoPoser ap = leftHandPreview.GetComponentInChildren<AutoPoser>();
                    if (ap != null) {
                        ap.UpdateContinuously = true;
                    }
                }

                if (rightHandPreview) {
                    AutoPoser ap = rightHandPreview.GetComponentInChildren<AutoPoser>();
                    if (ap != null) {
                        ap.UpdateContinuously = true;
                    }
                }
            }
            else {
               
                // Update in editor
                if (leftHandPreview) {
                    AutoPoser ap = leftHandPreview.GetComponentInChildren<AutoPoser>();
                    if (ap != null) {
                        ap.UpdateContinuously = false;
                    }
                }

                if (rightHandPreview) {
                    AutoPoser ap = rightHandPreview.GetComponentInChildren<AutoPoser>();
                    if (ap != null) {
                        ap.UpdateContinuously = false;
                    }
                }
            }
        }

        public Transform FindLeftHandPreview() {
            return transform.Find(GetLeftHandPreviewName());
        }

        public Transform FindRightHandPreview() {
            return transform.Find(GetRightHandPreviewName());
        }

        public virtual string GetLeftHandPreviewName() {
            if (LeftHandPreviewPrefab) {
                return LeftHandPreviewPrefab.name;
            }
            else {
                return _defaultLeftHandResourceName;
            }
        }

        public virtual string GetRightHandPreviewName() {
            if (RightHandPreviewPrefab) {
                return RightHandPreviewPrefab.name;
            } 
            else {
                return _defaultRightHandResourceName;
            }
        }

        public void UpdateChildAnimators() {
            var animators = transform.GetComponentsInChildren<Animator>(true);
            for (int x = 0; x < animators.Length; x++) {
                if(handPoseType == HandPoseType.AnimatorID) {
                    animators[x].enabled = true;
                    if(animators[x].isActiveAndEnabled) {
                        animators[x].Update(Time.deltaTime);
                    }

#if UNITY_EDITOR && (UNITY_2019 || UNITY_2020)
                    // Only set dirty if not in prefab mode
                    if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() == null) {
                        UnityEditor.EditorUtility.SetDirty(animators[x].gameObject);
                    }
#endif
                }
                // Disable the animator in editor mode if using handpose
                else if (handPoseType == HandPoseType.HandPose && SelectedHandPose != null) {
                    animators[x].enabled = false;
                }
                // Disable the animator in editor mode if using auto pose
                else if (handPoseType == HandPoseType.AutoPoseOnce || handPoseType == HandPoseType.AutoPoseContinuous) {
                    animators[x].enabled = false;
                }
            }
        }
        #endregion
    }
}