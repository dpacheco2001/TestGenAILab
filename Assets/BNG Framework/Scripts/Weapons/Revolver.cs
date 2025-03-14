using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BNG {
    public class Revolver : GrabbableEvents {

        // Rotate this to show / hide the bullet spindle
        public Transform CylinderArm;

        public float CylinderArmOpenAngle = -90f;

        // This is the transform we can rotate that contains the bullet objects
        public Transform CylinderTransform;

        // If true, animate the CylinderTransform on fire
        public bool RotateCylinderOnFire = true;

        // Should typically match number of BulletInserts. Used to calculate how many degrees to rotate cylinder
        int bulletCylinderCount = 6;
        int currentCylinder = 0;

        // Play this when open / closing the cylinder arm
        public AudioClip CylinderOpenSound;
        public AudioClip CylinderCloseSound;

        public List<MagazineSlide> BulletInserts = new List<MagazineSlide>();

        public float OpenSpeed = 360f;
        public float CloseSpeed = 720f;

        // Angular velocity required to close the Cylinder
        public float FlickCloseMagnitude = 10f;

        // If true, use angular X velocity instead of Z. IE, flick foward / back to close, not left to right
        public bool UseAngularXForFlickClose = false;

        public bool AutoEjectEmptyShells = false;

        /// <summary>
        /// If true, disables SecondHandTrigger collider. 
        /// This makes it easier to grab bullets from the spindle when it's open, instead of snapping the hand to the gun
        /// </summary>
        public bool DisableHandTriggerOnSpindleOpen = true;

        public Collider SecondHandTrigger;

        // Animate this on trigger / fire
        public Transform HammerTransform;

        // Angle at which hammer is considered open / cocked
        // Will use this to charge the gun, and set back to 0 on fire
        public float HammerOpenAngle = -45f;


        [Header("Weapon Spinning")]
        public bool SpinOnButton1 = true;

        // Can't fire while spindle is open
        bool spindleOpen = false;

        RaycastWeapon raycastWeapon;

        Grabbable parentGrabbable;

        bool animatingSpindle = false;

        bool readyToShoot = false;

        Vector3 originalLocalAngle;
        
        WeaponSpinner weaponSpinner;


        void Start() {
            raycastWeapon = GetComponent<RaycastWeapon>();

            weaponSpinner = GetComponent<WeaponSpinner>();

            raycastWeapon.onShootEvent.AddListener(OnShoot);
            raycastWeapon.onPlayedEmptyShotEvent.AddListener(OnPlayedEmptyShot);

            spindleOpen = CylinderArm != null && CylinderArm.transform.localEulerAngles.x == CylinderArmOpenAngle;

            // Add any magazine slides found in the weapon
            if (BulletInserts == null || BulletInserts.Count == 0) {
                BulletInserts = GetComponentsInChildren<MagazineSlide>().ToList();
            }

            bulletCylinderCount = BulletInserts.Count;

            originalLocalAngle = CylinderArm.localEulerAngles;
        }

        // We can use this like an Update that method that only runs while the object is being held
        public override void OnTrigger(float triggerValue) {

            // Update our local readyToShoot status
            // Trigger released back, we can pull the hammer back again
            if(!readyToShoot && triggerValue < 0.05f) {
                readyToShoot = true;
            }

            // Animate the hammer based on trigger position
            if (HammerTransform != null && !spindleOpen) {
                // base angle on how far we are pulling the trigger in
                float hammerAngle = HammerOpenAngle * triggerValue;

                // Keep Hammer Down if recently fired
                if(Time.time - raycastWeapon.GetLastShotTime() < 0.2f) {
                    hammerAngle = 0;
                }                
                // Keep Hammer Down if no ammo / not ready to shoot
                else if(!raycastWeapon.GetReadyToShoot() || !readyToShoot) {
                    hammerAngle = 0;
                }

                HammerTransform.localEulerAngles = new Vector3(hammerAngle, 0, 0);
            }

            // Animate the cylinder based on hammer position
            if (RotateCylinderOnFire && !spindleOpen && CylinderTransform != null) {

                float cylinderDegrees = 360 / bulletCylinderCount; // Ex : 6 cylinders = 60 degrees
                float targetAngle = (currentCylinder * cylinderDegrees) + (cylinderDegrees * triggerValue);

                // Keep Hammer Down if recently fired
                if (Time.time - raycastWeapon.GetLastShotTime() < 0.2f) {
                    targetAngle = (currentCylinder * cylinderDegrees);
                }

                // Keep cylinder locked if not yet ready to shoot
                if (!raycastWeapon.GetReadyToShoot() || !readyToShoot) {
                    targetAngle = 0;
                }

                CylinderTransform.localEulerAngles = new Vector3(0, 0, targetAngle);
            }

            // Check flick-to close spindle
            if (spindleOpen && !animatingSpindle) {
                // Check if controller "flicked" and we can close the spindle
                if(UseAngularXForFlickClose) {
                    MagnitudeShower = InputBridge.Instance.GetControllerAngularVelocity(thisGrabber.HandSide).x;
                    if (InputBridge.Instance.GetControllerAngularVelocity(thisGrabber.HandSide).x <= FlickCloseMagnitude) {
                        ToggleSpindle();
                    }
                }
                else {
                    if (InputBridge.Instance.GetControllerAngularVelocity(thisGrabber.HandSide).z <= FlickCloseMagnitude) {
                        // Debug.Log("Angular Velocity of " + InputBridge.Instance.GetControllerAngularVelocity(thisGrabber.HandSide).magnitude);
                        ToggleSpindle();
                    }
                }
                
            }
        }

        public float MagnitudeShower = 0;

        public override void OnButton1Down() {
            // Toggle Spindle
            if (spindleOpen && !animatingSpindle) {
                EjectBullets();
            }
            else if(SpinOnButton1 && weaponSpinner != null && !weaponSpinner.IsSpinning && !spindleOpen && !animatingSpindle) {
                weaponSpinner.DoSpin();
            }
        }

        public override void OnButton2Down() {
            // Toggle Spindle
            if (!animatingSpindle) {
                ToggleSpindle();
            }
        }

        public void OnShoot() {

            // Keep track of which cylinder to fire from
            currentCylinder++;
            if(currentCylinder > bulletCylinderCount) {
                currentCylinder = 1;
            }

            // Not ready to fire until we release the trigger again
            readyToShoot = false;
        }

        // Tried to shoot, but gun was empty. Essentially the same as OnShoot, but we'll separate for now
        public void OnPlayedEmptyShot() {
            // Keep track of which cylinder to fire from
            currentCylinder++;
            if (currentCylinder > bulletCylinderCount) {
                currentCylinder = 1;
            }

            // Not ready to fire until we release the trigger again
            readyToShoot = false;
        }

        

        public void ToggleSpindle() {
            // Close spindle
            if (spindleOpen) {
                StartCoroutine(CloseCylinderRoutine());
            }
            // Open
            else {
                StartCoroutine(OpenCylinderRoutine());
            }
        }

        public void EnableInserts() {
            for (int x = 0; x < BulletInserts.Count; x++) {
                if (BulletInserts[x] != null) {
                    BulletInserts[x].CanGrabMagazine = true;
                }
            }
        }

        public void DisableInserts() {
            for (int x = 0; x < BulletInserts.Count; x++) {
                if (BulletInserts[x] != null) {
                    BulletInserts[x].CanGrabMagazine = false;
                }
            }
        }

        public IEnumerator OpenCylinderRoutine() {
            animatingSpindle = true;

            VRUtils.Instance.PlaySpatialClipAt(CylinderOpenSound, transform.position, 0.4f, 0.5f);

            // Can't fire while spindle if open
            raycastWeapon.SafetyOn = true;
            float currentX = CylinderArm.localEulerAngles.x;

            yield return new WaitForEndOfFrame();

            while (currentX >= CylinderArmOpenAngle) {
                currentX -= Time.deltaTime * OpenSpeed;
                CylinderArm.localEulerAngles = new Vector3(originalLocalAngle.x, originalLocalAngle.y, currentX);
                yield return new WaitForEndOfFrame();
            }

            // Can grab bullets out now
            EnableInserts();

            // Disable the second hand trigger attachment to make it easier to grab bullets out
            if(DisableHandTriggerOnSpindleOpen && SecondHandTrigger != null) {
                SecondHandTrigger.enabled = false;
            }

            CylinderArm.localEulerAngles = new Vector3(originalLocalAngle.x, originalLocalAngle.y, CylinderArmOpenAngle);
            
            spindleOpen = true;

            animatingSpindle = false;

            CheckAutoEject();
        }

        public void CheckAutoEject() {
            if(AutoEjectEmptyShells) {
                for (int x = 0; x < BulletInserts.Count; x++) {
                    // Check if shell has been fire via check for active Bullet component
                    if (BulletInserts[x] != null && BulletInserts[x].HeldMagazine != null && BulletInserts[x].HeldMagazine.GetComponentInChildren<Bullet>() == null) {
                        BulletInserts[x].EjectMagazine();
                    }
                }
            }
        }

        public IEnumerator CloseCylinderRoutine() {

            animatingSpindle = true;

            // Disable Grabbable on Bullet Inserts so we can't grab bullets out while it's closed
            DisableInserts();

            // Animate spindle closed
            float currentX = CylinderArmOpenAngle;
            while (currentX < 0) {
                currentX += Time.deltaTime * CloseSpeed;
                CylinderArm.localEulerAngles = new Vector3(originalLocalAngle.x, originalLocalAngle.y, currentX);
                yield return new WaitForEndOfFrame();
            }

            CylinderArm.localEulerAngles = originalLocalAngle;

            VRUtils.Instance.PlaySpatialClipAt(CylinderCloseSound, transform.position, 0.35f, 0f);

            // Re-enable the second hand trigger attachment to so we can 2-hand the gun while spindle is closed
            if (DisableHandTriggerOnSpindleOpen && SecondHandTrigger != null) {
                SecondHandTrigger.enabled = true;
            }

            // Close the spindle and disengage safety
            spindleOpen = false;

            // Can fire now that the spindle is closed
            raycastWeapon.SafetyOn = false;

            animatingSpindle = false;
        }

        public void EjectBullets() {
            for (int x = 0; x < BulletInserts.Count; x++) {
                if (BulletInserts[x] != null) {
                    BulletInserts[x].EjectMagazine();
                }
            }
        }
    }
}

