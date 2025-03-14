using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {

    /// <summary>
    /// This Component allows a generic humanoid rig to follow the Player's controller's and HMD using Unity's IK system
    /// </summary>
    public class CharacterIK : MonoBehaviour {
        
        /// <summary>
        /// The Left Controller our Left Hand IK should track
        /// </summary>
        public Transform FollowLeftController;

        /// <summary>
        /// The Right Controller our Right Hand IK should track
        /// </summary>
        public Transform FollowRightController;

        public Transform FollowLeftFoot;
        public Transform FollowRightFoot;
        public Transform FollowHead;


        [Tooltip("(Optional) Assign this transform to check if mounted in vehicle. If this transform's local position is 0, it will have no velocity to prevent the legs from moving quickly while mounted to an object ")]
        public Transform PlayerTransform;  


        public bool RaycastFeetPosition = true;
        public LayerMask GroundedLayers;

        // How far to raycast down from the intended foot position
        public float RaycastDistance = 0.5f;

        // May need to adjust to make sure not clipping through the floor
        public float footOffset = 0.1f;

        /// <summary>
        /// If false the IK layers will be deactivated
        /// </summary>
        public bool IKActive = true;

        /// <summary>
        /// Should the player's feet follow our given Y axis using IK
        /// </summary>
        public bool IKFeetActive = true;
        
        public bool HideHead = true;
        public bool HideLeftArm = false;
        public bool HideRightArm = false;
        public bool HideLeftHand = false;
        public bool HideRightHand = false;
        public bool HideLegs = false;

        /// <summary>
        /// The Hips joint of the Character. Used for hiding the legs by scaling the joint to 0
        /// </summary>
        public Transform HipsJoint;

        /// <summary>
        /// (Legacy) The player our Body will follow
        /// </summary>        
        [HideInInspector]
        public CharacterController FollowPlayer;        

        Transform headBone;
        Transform leftShoulderJoint;
        Transform rightShoulderJoint;
        Transform leftHandJoint;
        Transform rightHandJoint;

        Animator animator;

        //public float HipOffset = 0;

        public bool ApplyVelocityToAnimator = true;
        // Keep track of velocity to animate
        VelocityTracker velocityTracker;
        
        void Start() {
            animator = GetComponent<Animator>();
            headBone = animator.GetBoneTransform(HumanBodyBones.Head);
            leftHandJoint = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightHandJoint = animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftShoulderJoint = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            rightShoulderJoint = animator.GetBoneTransform(HumanBodyBones.RightShoulder);

            velocityTracker = GetComponent<VelocityTracker>(); 
            if(ApplyVelocityToAnimator && velocityTracker == null) {
                velocityTracker = gameObject.AddComponent<VelocityTracker>();
                // Use per frame tracking for simple velocity check
                velocityTracker.trackingType = VelocityTracker.VelocityTrackingType.PerFrame; 
            }

            // Legacy support
            if(FollowPlayer != null && PlayerTransform == null) {
                PlayerTransform = FollowPlayer.transform;
            }
        }

        public Vector3 hideBoneScale = new Vector3(0.0001f, 0.0001f, 0.0001f);

        protected RaycastHit hit;

        bool hitLeftFoot = false;
        Vector3 leftFootHitPosition;
        Quaternion leftFootHitRotation;

        bool hitRightFoot = false;
        Vector3 rightFootHitPosition;
        Quaternion rightFootHitRotation;

        Vector3 lastPosition;
        Quaternion lastRotation;

        void Update() {

            // Hide Headbone
            if (headBone != null) {
                headBone.localScale = HideHead ? Vector3.zero : Vector3.one;
            }

            // Hide Left Arm
            if (leftShoulderJoint != null) {
                leftShoulderJoint.localScale = HideLeftArm ? hideBoneScale : Vector3.one;
            }
            // Hide Right Arm
            if (rightShoulderJoint != null) {
                rightShoulderJoint.localScale = HideRightArm ? hideBoneScale : Vector3.one;
            }

            // Hide Left Hand
            if (leftHandJoint != null) {
                leftHandJoint.localScale = HideLeftHand ? Vector3.zero : Vector3.one;
            }
            // Hide Right Hand
            if (rightHandJoint != null) {
                rightHandJoint.localScale = HideRightHand ? Vector3.zero : Vector3.one;
            }

            // Hide Legs
            if(HipsJoint) {
                HipsJoint.localScale = HideLegs ? Vector3.zero : Vector3.one;
            }

            // Transform hipJoint = animator.GetBoneTransform(HumanBodyBones.RightShoulder);

            if(ApplyVelocityToAnimator) {
                // Set "Walking" velocity if we're moving more 
                if (transform.position != lastPosition && velocityTracker.GetAveragedVelocity().magnitude > 0.1f) {
                    if(PlayerTransform != null && PlayerTransform.parent != null && PlayerTransform.localPosition == Vector3.zero) {
                        // We are attached to something moving us along; set velocity to 0
                        animator.SetFloat("ForwardVelocity", 0f);
                    }
                    else {
                        animator.SetFloat("ForwardVelocity", 0.4f);
                    }
                }
                // Stand still or very low velocity
                else {
                    animator.SetFloat("ForwardVelocity", 0f);
                }
            }

            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }
       
        void OnAnimatorIK() {
            UpdateAnimatorIK();

            if(RaycastFeetPosition) {
                AlignFootToGround(AvatarIKGoal.LeftFoot);
                AlignFootToGround(AvatarIKGoal.RightFoot);
            }
        }

        Vector3 leftHandDestination;
        Quaternion leftHandRotationDestination;

        public virtual void UpdateAnimatorIK() {
            if (animator) {

                //if the IK is active, set the position and rotation directly to the goal. 
                if (IKActive) {

                    // Head
                    if (FollowHead != null) {
                        animator.SetLookAtWeight(1);
                        animator.SetLookAtPosition(FollowHead.position);
                    }

                    // Left Hand
                    if (FollowLeftController != null) {
                        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);

                        animator.SetIKPosition(AvatarIKGoal.LeftHand, FollowLeftController.position);
                        animator.SetIKRotation(AvatarIKGoal.LeftHand, FollowLeftController.rotation);
                    }
                    // Right Hand
                    if (FollowRightController != null) {
                        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                        animator.SetIKPosition(AvatarIKGoal.RightHand, FollowRightController.position);
                        animator.SetIKRotation(AvatarIKGoal.RightHand, FollowRightController.rotation);
                    }

                    // Left Foot
                    if (IKFeetActive) {
                        // Left Foot
                        if (FollowLeftFoot != null) {
                            // Todo ; separate hit
                            if(hitLeftFoot) {
                                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                                animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootHitPosition);

                                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                                animator.SetIKRotation(AvatarIKGoal.LeftFoot, FollowLeftFoot.rotation);
                            }
                            else {
                                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
                                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
                            }
                        }

                        // Right Foot
                        if (FollowRightFoot != null) {
                            if(hitRightFoot) {
                                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                                animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootHitPosition);

                                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
                                animator.SetIKRotation(AvatarIKGoal.RightFoot, FollowRightFoot.rotation);
                            }
                            else {
                                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
                                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
                            }
                        }
                        // Testing body IK
                        //animator.bodyPosition = new Vector3(animator.bodyPosition.x, animator.bodyPosition.y + HipOffset + FollowPlayer.height, animator.bodyPosition.z);
                    } 
                    else {
                        // Left Foot
                        if (FollowLeftFoot != null) {
                            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
                            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
                        }

                        // Right Foot
                        if (FollowRightFoot != null) {
                            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
                            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
                        }
                    }
                }
                // IK not active, release weight for hands / head
                else {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);

                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);

                    animator.SetLookAtWeight(0);
                }
            }
        }

        public virtual void AlignFootToGround(AvatarIKGoal foot) {
            Vector3 footPos = animator.GetIKPosition(foot);
            Quaternion footRot = animator.GetIKRotation(foot);

            //Ground Align foot 
            if (Physics.Raycast(footPos + Vector3.up * RaycastDistance, Vector3.down, out RaycastHit hit, RaycastDistance * 2, GroundedLayers, QueryTriggerInteraction.Ignore)) {
                animator.SetIKPositionWeight(foot, 1f);
                animator.SetIKRotationWeight(foot, 1f);

                // Add a bit of offset in case of boots and such
                animator.SetIKPosition(foot, hit.point + hit.normal * footOffset);
                animator.SetIKRotation(foot, Quaternion.FromToRotation(Vector3.up, hit.normal) * footRot);
            } 
            else {
                animator.SetIKPositionWeight(foot, 0f);
                animator.SetIKRotationWeight(foot, 0f);
            }
        }
    }
}