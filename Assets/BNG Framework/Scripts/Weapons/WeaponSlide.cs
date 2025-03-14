using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {

    /// <summary>
    /// Weapon slide on a pistol. Charges weapon and ejects casings.
    /// </summary>
    public class WeaponSlide : MonoBehaviour {

        /// <summary>
        /// Minimum distance slide will travel on Z axis
        /// </summary>
        public float MinLocalZ = -0.03f;

        /// <summary>
        /// Max distance slide will travel on Z axis
        /// </summary>
        public float MaxLocalZ = 0;

        // Keep track of which way we are sliding
        bool slidingBack = true;

        /// <summary>
        /// If true , lock the slide back on last shot
        /// </summary>
        public bool LockSlide = true;


        /// <summary>
        /// How fast to animate the slide back to 0 when slide is released. Set to 0 if you don't want to animate it
        /// </summary>
        public float AnimateSlideReturn = 10f;

        /// <summary>
        /// Is the Slide locked back due to last shot
        /// </summary>
        public bool LockedBack = false;

        /// <summary>
        /// Sound to play when slide is released back into position
        /// </summary>
        public AudioClip SlideReleaseSound;

        /// <summary>
        /// Sound to play after last shot has fired and slide is forced back
        /// </summary>
        public AudioClip LockedBackSound;

        /// <summary>
        /// When true, the slide will be set to 0 mass when not being held. This fixes jitter caused by the slide having a configurable joint attached to the weapon
        /// </summary>
        public bool ZeroMassWhenNotHeld = true;

        RaycastWeapon parentWeapon;
        Grabbable parentGrabbable;
        Vector3 initialLocalPos;
        Grabbable thisGrabbable;
        AudioSource audioSource;
        Rigidbody rigid;
        float initialMass;

        bool simulateRigidbody = false;

        /// <summary>
        /// Lock the slide position in place
        /// </summary>
        Vector3 _lockPosition;
        /// <summary>
        /// If true then the slides position is locked in Update and cannot be moved
        /// </summary>
        bool lockSlidePosition;

        float lastFireTime = 0f;

        void Start() {
            initialLocalPos = transform.localPosition;
            audioSource = GetComponent<AudioSource>();
            parentWeapon = transform.parent.GetComponent<RaycastWeapon>();
            parentGrabbable = transform.parent.GetComponent<Grabbable>();
            thisGrabbable = GetComponent<Grabbable>();
            rigid = GetComponent<Rigidbody>();

            if (rigid != null) {
                initialMass = rigid.mass;
            } 
            else {
                simulateRigidbody = true;
            }

            if (parentWeapon != null) {
                Physics.IgnoreCollision(GetComponent<Collider>(), parentWeapon.GetComponent<Collider>());
            }
        }

        public virtual void OnEnable() {
            // Lock the slide in place when teleporting or snap turning
            if (!simulateRigidbody) {
                PlayerTeleport.OnBeforeTeleport += LockSlidePosition;
                PlayerTeleport.OnAfterTeleport += UnlockSlidePosition;
            }
        }

        public virtual void OnDisable() {
            if (!simulateRigidbody) {
                PlayerTeleport.OnBeforeTeleport -= LockSlidePosition;
                PlayerTeleport.OnAfterTeleport -= UnlockSlidePosition;
            }
        }

        void Update() {

            // If our slide is currently locked just set it and return early
            if(lockSlidePosition) {
                transform.localPosition = _lockPosition;
                return;
            }

            float localZ = transform.localPosition.z;

            if (LockSlide && LockedBack) {
                transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, MinLocalZ);

                // Not locking back if hand is holding this
                if (thisGrabbable != null && thisGrabbable.BeingHeld) {
                    UnlockBack();
                }
            }

            if (!LockedBack) {

                // Move towards held grabber
                if(thisGrabbable.GrabPhysics == GrabPhysics.None) {
                    if(thisGrabbable.BeingHeld && !doingBlowBack) {
                        // Move towards grabber holding it
                        UpdateHeldSlide();
                    }
                    else {
                        // Slide back into place or do blowback animatino
                        UpdateSlideAnimation();
                    }
                }

                // Check if we reached the end and can fire an event when 
                if (transform.localPosition.z <= MinLocalZ) {
                    transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, MinLocalZ);
                    if (slidingBack && !doingBlowBack && !blowBackReturn) {
                        onSlideBack();
                    }                    
                }
                else if (transform.localPosition.z >= MaxLocalZ) {
                    transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, MaxLocalZ);
                    // Moving forward
                    if (!slidingBack) {
                        if(blowBackReturn) {
                            blowBackReturn = false;
                        }

                        if (!doingBlowBack && !blowBackReturn) {
                            onSlideForward();
                        }
                    }
                }
            }
        }

        bool blowBackReturn;

        void FixedUpdate() {
            // No rigidbody adjustments if we're simulating in Update
            if(simulateRigidbody) {
                return;
            }

            // Change mass of slider rigidbody. This prevents stuttering when the object is not held and the slide is back
            if (ZeroMassWhenNotHeld && parentGrabbable.BeingHeld && rigid) {
                rigid.mass = initialMass;
            }
            else if (ZeroMassWhenNotHeld && rigid) {
                // Set mass to very low to prevent stuttering when not held
                rigid.mass = 0.0001f;
            }           
        }

        // how long to hang out at the end 
        public float fireSlideBackDuration = 0.1f;

        // How fast to return to 0 whenot held
        public float BlowbackSpeed = 10f;
        public float slideSpeedForward = 5f;
        public bool doingBlowBack;

        /// <summary>
        /// Animate slide backwards
        /// </summary>
        public void BlowbackSlide(float timeDelay = 0) {
            lastFireTime = Time.time;
            doingBlowBack = true;
            blowBackReturn = false;
        }

        public virtual void UpdateSlideAnimation() {
            // Recently fired weapon, aniamtethe slide back
            if (doingBlowBack) {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, new Vector3(initialLocalPos.x, initialLocalPos.y, MinLocalZ), BlowbackSpeed * Time.deltaTime);
                //transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(initialLocalPos.x, initialLocalPos.y, MinLocalZ), BlowbackSpeed * Time.deltaTime);

                // Check if we finished blowback and can return
                if (transform.localPosition.z <= MinLocalZ) {
                    if (Time.time >= lastFireTime + fireSlideBackDuration) {
                        doingBlowBack = false;
                        blowBackReturn = true;
                    }
                }
            }
            // Return forward if not recently fired
            else {
                if (Time.time >= lastFireTime + fireSlideBackDuration) {
                    transform.localPosition = Vector3.MoveTowards(transform.localPosition, initialLocalPos, slideSpeedForward * Time.deltaTime);
                    //transform.localPosition = Vector3.Lerp(transform.localPosition, initialLocalPos, slideSpeedForward * Time.deltaTime);

                    // Reached end
                    if (transform.localPosition.z >= initialLocalPos.z) {
                        if(blowBackReturn) {
                            blowBackReturn = false;
                        }

                        // slidingForward = false;
                    }
                }
            }
        }

        public virtual void UpdateHeldSlide() {

            // Cancel blowback anim as we are holding the object now
            if(doingBlowBack) {
                doingBlowBack = false;
            }

            // Move towards held grabber
            if(thisGrabbable.BeingHeld) {
                if(simulateRigidbody) { 

                    // Move towards grabber
                    float moveSlideSpeed = 5f;
                    transform.position = Vector3.MoveTowards(transform.position, thisGrabbable.GetGrabberVector3(thisGrabbable.GetPrimaryGrabber(), false), moveSlideSpeed * Time.deltaTime);

                    // Only move on local axis
                    transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, transform.localPosition.z);
                }
            }
            else {
                // Move back to 0
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, initialLocalPos, AnimateSlideReturn * Time.deltaTime);
            }

            // Check for on slide back event while held
            //// Make sure we cap min / max z
            //if (transform.localPosition.z <= MinLocalZ) {
            //    transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, MinLocalZ);
            //    if (slidingBack) {
            //        onSlideBack();
            //    }
            //} 
            //else if (transform.localPosition.z >= MaxLocalZ) {
            //    transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y, MaxLocalZ);

            //    // Moving forward
            //    if (!slidingBack) {
            //        onSlideForward();
            //    }
            //}
        }

        public virtual void LockBack() {

            if (!LockedBack) {
                if (thisGrabbable.BeingHeld || parentGrabbable.BeingHeld) {
                    VRUtils.Instance.PlaySpatialClipAt(LockedBackSound, transform.position, 1f, 0.8f);
                }

                LockedBack = true;
            }
        }

        public virtual void UnlockBack() {

            if (LockedBack) {
                if (thisGrabbable.BeingHeld || parentGrabbable.BeingHeld) {
                    VRUtils.Instance.PlaySpatialClipAt(SlideReleaseSound, transform.position, 1f, 0.9f);
                }

                LockedBack = false;

                // This is considered a charge
                if (parentWeapon != null) {
                    parentWeapon.OnWeaponCharged(false);
                }
            }
        }

        void onSlideBack() {

            if (thisGrabbable.BeingHeld || parentGrabbable.BeingHeld) {
                playSoundInterval(0, 0.2f, 0.9f);
            }

            if (parentWeapon != null) {
                parentWeapon.OnWeaponCharged(true);
            }

            slidingBack = false;
        }

        void onSlideForward() {

            if (thisGrabbable.BeingHeld || parentGrabbable.BeingHeld) {
                playSoundInterval(0.2f, 0.35f, 1f);
            }

            slidingBack = true;            
        }

        public virtual void LockSlidePosition() {
            // Lock the slide position if we aren't holding the object
            if (parentGrabbable.BeingHeld && !thisGrabbable.BeingHeld && !lockSlidePosition) {
                _lockPosition = transform.localPosition;
                lockSlidePosition = true;
            }
        }

        public virtual void UnlockSlidePosition() {
            if (lockSlidePosition) {
                StartCoroutine(UnlockSlideRoutine());
            }
        }

        public IEnumerator UnlockSlideRoutine() {
            yield return new WaitForSeconds(0.2f);
            lockSlidePosition = false;
        }

        void playSoundInterval(float fromSeconds, float toSeconds, float volume) {
            if (audioSource) {

                if (audioSource.isPlaying) {
                    audioSource.Stop();
                }

                audioSource.pitch = Time.timeScale;
                audioSource.time = fromSeconds;
                audioSource.volume = volume;
                audioSource.Play();
                audioSource.SetScheduledEndTime(AudioSettings.dspTime + (toSeconds - fromSeconds));
            }
        }
    }
}
