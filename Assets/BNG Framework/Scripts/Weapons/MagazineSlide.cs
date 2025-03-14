using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {

    /// <summary>
    /// Constrain a magazine when it enters this area. Attaches the magazine in place if close enough.
    /// </summary>
    public class MagazineSlide : MonoBehaviour {

        /// <summary>
        /// Clip transform name must contain this to be considered valid
        /// </summary>
        [Tooltip("Clip transform name must contain this to be considered valid")]
        public string AcceptableMagazineName = "Clip";

        /// <summary>
        /// The weapon this magazine is attached to (optional)
        /// </summary>RaycastWeapon
        public Grabbable AttachedWeapon;

        /// <summary>
        /// How fast to animate the Clip into the insert
        /// </summary>
        public float InsertSpeed = 10f;

        public float ClipSnapDistance = 0.075f;
        public float ClipUnsnapDistance = 0.15f;

        /// <summary>
        ///  How much force to apply to the inserted magazine if it is forcefully ejected
        /// </summary>
        public float EjectForce = 1f;
        public enum EjectDirectionOption { Down, Back }
        public EjectDirectionOption EjectDirection = EjectDirectionOption.Down;

        public bool OffsetMagOnEject = true;
        public Vector3 MagEjectOffset = new Vector3(0, -0.1f, 0);

        /// <summary>
        /// Check if Time.time - lastEjectTime is greater than this before reattaching a magazine
        /// </summary>
        public float ReattachInterval = 0.2f;

        public Grabbable HeldMagazine = null;
        // Used for magazine type reload
        protected Magazine heldMagazineComponent;

        Collider HeldCollider = null;

        public float MagazineDistance = 0f;

        /// <summary>
        /// Set to false if you don't want to be able to grab the clip out of the weapon
        /// </summary>
        public bool CanGrabMagazine = true;

        bool magazineInPlace = false;

        // Lock in place for physics
        bool lockedInPlace = false;

        public AudioClip ClipAttachSound;
        public AudioClip ClipDetachSound;

        Rigidbody magazineRigid;
        bool usedGravity;
        RaycastWeapon parentWeapon;
        GrabberArea grabClipArea;

        bool insertingMagazine = false;
        float lastEjectTime, requestedEjectForce;

        void Awake() {
            grabClipArea = GetComponentInChildren<GrabberArea>();

            if (transform.parent != null) {
                parentWeapon = transform.parent.GetComponentInParent<RaycastWeapon>();
            }

            // Check to see if we started with a loaded magazine
            if (HeldMagazine != null) {
                AttachGrabbableMagazine(HeldMagazine, HeldMagazine.GetComponent<Collider>());
            }
        }

        void Update() {
            // Are we trying to grab the clip from the weapon
            CheckGrabClipInput();

            PositionHeldMagazine();
        }

        public virtual void PositionHeldMagazine() {
            // There is a magazine inside the slide. Position it properly
            if (HeldMagazine != null) {

                // HeldMagazine.transform.parent = transform;
                if (magazineRigid) {
                    magazineRigid.linearVelocity = Vector3.zero;
                }

                // Lock in place immediately
                if (lockedInPlace) {
                    HeldMagazine.transform.position = transform.position;
                    HeldMagazine.transform.rotation = transform.rotation;
                    return;
                }

                Vector3 localPos = HeldMagazine.transform.localPosition;

                // Make sure magazine is aligned with MagazineSlide
                HeldMagazine.transform.localEulerAngles = Vector3.zero;

                // Only allow Y translation. Don't allow to go up and through clip area
                float localY = localPos.y;

                // Animate in
                if (localY < 0) {
                    localY += Time.deltaTime * InsertSpeed;
                }

                if (localY > 0) {
                    localY = 0;
                }

                moveMagazine(new Vector3(0, localY, 0));

                MagazineDistance = Vector3.Distance(transform.position, HeldMagazine.transform.position);

                bool clipRecentlyGrabbed = Time.time - HeldMagazine.LastGrabTime < 1f;

                // Snap Magazine In Place
                if (MagazineDistance <= ClipSnapDistance) {

                    // Snap in place
                    if (!magazineInPlace && !recentlyEjected() && !clipRecentlyGrabbed) {
                        attachMagazine();
                    }

                    // Make sure magazine stays in place if not being grabbed
                    if (!HeldMagazine.BeingHeld) {
                        moveMagazine(Vector3.zero);
                    }
                }
                // Stop aligning clip with slide if we exceed this distance
                else if (MagazineDistance >= ClipUnsnapDistance && !recentlyEjected() && !insertingMagazine) {
                    detachMagazine();
                }
            }
        }

        bool recentlyEjected() {
            return Time.time - lastEjectTime < 0.1f;
        }

        void moveMagazine(Vector3 localPosition) {
            HeldMagazine.transform.localPosition = localPosition;
        }

        public void CheckGrabClipInput() {

            // No need to check for grabbing a clip out if none exists
            if (CanGrabMagazine == false || HeldMagazine == null || grabClipArea == null) {
                return;
            }

            // Don't grab clip if the weapon isn't being held
            if (AttachedWeapon != null && !AttachedWeapon.BeingHeld) {
                return;
            }

            Grabber nearestGrabber = grabClipArea.GetOpenGrabber();
            if (grabClipArea != null && nearestGrabber != null) {
                if (nearestGrabber.HandSide == ControllerHand.Left && InputBridge.Instance.LeftGripDown) {
                    // grab clip
                    OnGrabClipArea(nearestGrabber);
                } else if (nearestGrabber.HandSide == ControllerHand.Right && InputBridge.Instance.RightGripDown) {
                    OnGrabClipArea(nearestGrabber);
                }
            }
        }

        void attachMagazine() {
            // Drop Item
            var grabber = HeldMagazine.GetPrimaryGrabber();
            HeldMagazine.DropItem(grabber, false, false);

            // Play Sound
            if (ClipAttachSound && Time.timeSinceLevelLoad > 0.1f) {
                VRUtils.Instance.PlaySpatialClipAt(ClipAttachSound, transform.position, 0.5f);
            }

            // Move to desired location before locking in place            
            moveMagazine(Vector3.zero);

            // Destroy RB  as we don't need it. We can add it back on grab later
            HeldMagazine.DestroyRigidbody();

            // If attached to a Raycast weapon, let it know we attached something
            if (parentWeapon && parentWeapon.ReloadMethod == ReloadType.Magazine) {
                Magazine mag = HeldMagazine.GetComponent<Magazine>();
                if (mag) {
                    parentWeapon.OnAttachedAmmo(mag);
                }
            } 
            else {
                parentWeapon.OnAttachedAmmo();
            }

            lockedInPlace = true;
            magazineInPlace = true;
            insertingMagazine = false;
        }

        /// <summary>
        /// Detach Magazine from it's parent. Removes joint, re-enables collider, and calls events
        /// </summary>
        /// <returns>Returns the magazine that was ejected or null if no magazine was attached</returns>
        protected virtual Grabbable detachMagazine() {

            if (HeldMagazine == null) {
                return null;
            }

            VRUtils.Instance.PlaySpatialClipAt(ClipDetachSound, transform.position, 1f, 0.5f);

            // Recreate the rigidbody
            HeldMagazine.RebuildRigidbody();

            HeldMagazine.transform.parent = null;

            // Reset Collider
            if (HeldCollider != null) {
                HeldCollider.enabled = true;
                HeldCollider = null;
            }

            // Let wep know we detached something
            if (parentWeapon) {
                parentWeapon.OnDetachedAmmo();
            }

            // Can be grabbed again
            HeldMagazine.enabled = true;
            magazineInPlace = false;
            lockedInPlace = false;
            lastEjectTime = Time.time;

            var returnGrab = HeldMagazine;
            HeldMagazine = null;
            heldMagazineComponent = null;

            return returnGrab;
        }

        public void EjectMagazine() {
            EjectMagazine(EjectForce);
        }

        public void EjectMagazine(float ejectForce) {
            Grabbable ejectedMag = detachMagazine();
            lastEjectTime = Time.time;
            requestedEjectForce = ejectForce;

            StartCoroutine(EjectMagRoutine(ejectedMag));
        }

        

        IEnumerator EjectMagRoutine(Grabbable ejectedMag) {

            if (ejectedMag != null && ejectedMag.GetComponent<Rigidbody>() != null) {

                Rigidbody ejectRigid = ejectedMag.GetComponent<Rigidbody>();

                // Wait before ejecting

                // Move clip down before we eject it
                if (OffsetMagOnEject) {
                    ejectedMag.transform.parent = transform;

                    if (ejectedMag.transform.localPosition.y > -ClipSnapDistance) {
                        ejectedMag.transform.localPosition = MagEjectOffset;
                    }
                }

                // Eject with physics force
                ejectedMag.transform.parent = null;
                ejectRigid.linearVelocity = Vector3.zero;

                if (requestedEjectForce != 0) {
                    if(EjectDirection == EjectDirectionOption.Down) {
                        ejectRigid.AddForce(-ejectedMag.transform.up * requestedEjectForce, ForceMode.VelocityChange);
                    }
                    else if (EjectDirection == EjectDirectionOption.Back) {
                        ejectRigid.AddForce(-ejectedMag.transform.forward * requestedEjectForce, ForceMode.VelocityChange);
                    }
                }

                yield return new WaitForFixedUpdate();

                if (ejectedMag.transform.parent != null) {
                    ejectedMag.transform.parent = null;
                }

                // ejectRigid.angularVelocity = Vector3.zero;
            }

            yield return null;
        }

        // Pull out magazine from clip area
        public void OnGrabClipArea(Grabber grabbedBy) {
            if (HeldMagazine != null) {

                // Move clip down before we eject it
                if (OffsetMagOnEject) {
                    HeldMagazine.transform.localPosition = MagEjectOffset;
                }

                // Store reference so we can eject the clip first
                Grabbable detachedMagazine = detachMagazine();

                // Now transfer grab to the grabber
                detachedMagazine.enabled = true;

                grabbedBy.GrabGrabbable(detachedMagazine);
            }
        }

        public virtual void AttachGrabbableMagazine(Grabbable mag, Collider magCollider) {

            // Drop the magazine if held - we'll animate it in from here
            if (mag.BeingHeld) {
                mag.DropItem(true, false);
            }

            insertingMagazine = true;
            HeldMagazine = mag;

            if (parentWeapon != null && parentWeapon.ReloadMethod == ReloadType.Magazine) {
                heldMagazineComponent = mag.GetComponent<Magazine>();
            }

            // Don't let anything try to grab the magazine while it's within the weapon
            // We will use a grabbable proxy to grab the clip back out instead
            HeldMagazine.enabled = false;

            magazineRigid = mag.GetComponent<Rigidbody>();

            if (magazineRigid) {
                usedGravity = magazineRigid.useGravity;
                magazineRigid.useGravity = false;
            }

            HeldMagazine.transform.parent = transform;

            HeldCollider = magCollider;

            // Disable the collider while we're sliding it in to the weapon
            if (HeldCollider != null) {
                HeldCollider.enabled = false;
            }
        }


        bool inTrigger;

        void OnTriggerEnter(Collider other) {
            Grabbable grab = other.GetComponent<Grabbable>();

            // Can we insert magazine
            if (HeldMagazine == null && grab != null && grab.transform.name.Contains(AcceptableMagazineName)) {

                // Attach if didn't recently eject this magazine
                if (Time.time - lastEjectTime > ReattachInterval) {
                    AttachGrabbableMagazine(grab, other);
                    inTrigger = true;
                }
            }
        }

        void OnTriggerExit(Collider other) {
            Grabbable grab = other.GetComponent<Grabbable>();

            if (inTrigger && grab != null) {
                inTrigger = false;
            }
        }
    }
}
