using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BNG {

    /// <summary>
    /// An example weapon script that can fire Raycasts or Projectile objects
    /// </summary>
    public class RaycastWeapon : GrabbableEvents {

        [Header("General : ")]
        /// <summary>
        /// How far we can shoot in meters
        /// </summary>
        public float MaxRange = 25f;

        /// <summary>
        /// How much damage to apply to "Damageable" on contact
        /// </summary>
        public float Damage = 25f;

        /// <summary>
        /// Semi requires user to press trigger repeatedly, Auto to hold down
        /// </summary>
        [Tooltip("Semi requires user to press trigger repeatedly, Auto to hold down")]
        public FiringType FiringMethod = FiringType.Semi;

        /// <summary>
        /// How does the user reload once the Clip is Empty
        /// </summary>
        public ReloadType ReloadMethod = ReloadType.InfiniteAmmo;

        /// <summary>
        /// Ex : 0.2 = 5 Shots per second
        /// </summary>
        [Tooltip("Ex : 0.2 = 5 Shots per second")]
        public float FiringRate = 0.2f;
        float lastShotTime;

        [Tooltip("Amount of force to apply to a Rigidbody once damaged")]
        public float BulletImpactForce = 1000f;

        /// <summary>
        /// Maximum amount of internal ammo this weapon can hold. Does not account for attached clips.  For example, a shotgun has internal ammo
        /// </summary>
        [Tooltip("Current Internal Ammo if you are keeping track of ammo yourself. Firing will deduct from this number. Reloading will cause this to equal MaxInternalAmmo.")]
        public float InternalAmmo = 0;

        /// <summary>
        /// Maximum amount of internal ammo this weapon can hold. Does not account for attached clips.  For example, a shotgun has internal ammo
        /// </summary>
        [Tooltip("Maximum amount of internal ammo this weapon can hold. Does not account for attached clips.  For example, a shotgun has internal ammo")]
        public float MaxInternalAmmo = 10;

        /// <summary>
        /// Set true to automatically chamber a new round on fire. False to require charging. Example : Bolt-Action Rifle does not auto chamber.  
        /// </summary>
        [Tooltip("Set true to automatically chamber a new round on fire. False to require charging. Example : Bolt-Action Rifle does not auto chamber. ")]
        public bool AutoChamberRounds = true;

        /// <summary>
        /// Does it matter if rounds are chambered or not. Does the user have to charge weapon as soon as ammo is inserted
        /// </summary>
        [Tooltip("Does it matter if rounds are chambered or not. Does the user have to charge weapon as soon as ammo is inserted")]
        public bool MustChamberRounds = false;

        [Header("Projectile Settings : ")]

        [Tooltip("If true a projectile will always be used instead of a raycast")]
        public bool AlwaysFireProjectile = false;

        [Tooltip("If true the ProjectilePrefab will be instantiated during slowmo instead of using a raycast.")]
        public bool FireProjectileInSlowMo = true;

        [Tooltip("How fast to fire the weapon during slowmo. Keep in mind this is affected by Time.timeScale")]
        public float SlowMoRateOfFire = 0.3f;

        [Tooltip("Amount of force to apply to Projectile")]
        public float ShotForce = 10f;

        [Tooltip("Amount of force to apply to the BulletCasingPrefab object")]
        public float BulletCasingForce = 3f;

        [Header("Laser Guided Projectile : ")]

        [Tooltip("If true the projectile will be marked as Laser Guided and will follow a point from the ejection point")]
        public bool LaserGuided = false;

        [Tooltip("If specified the projectile will try to turn towards this object in world space. Otherwise will use a point from the muzzle of the raycast object")]
        public Transform LaserPoint;

        [Tooltip(" Set to true if you want to disallow firingt")]
        public bool SafetyOn = false;

        [Header("Recoil : ")]
        /// <summary>
        /// How much force to apply to the tip of the barrel
        /// </summary>
        [Tooltip("How much force to apply to the tip of the barrel")]
        public Vector3 RecoilForce = Vector3.zero;

        [Tooltip("How much force to apply to the tip of the barrel when two handing. The joint is looser when 2Handing a weapon, so this may actually need to be lower than RecoilForce")]
        public Vector3 RecoilForceTwoHanded = Vector3.zero;


        [Tooltip("Time in seconds to allow the gun to be springy")]
        public float RecoilDuration = 0.3f;

        Rigidbody weaponRigid;

        [Header("Raycast Options : ")]
        public LayerMask ValidLayers;

        [Header("Weapon Setup : ")]
        /// <summary>
        /// Transform of trigger to animate rotation of
        /// </summary>
        [Tooltip("Transform of trigger to animate rotation of")]
        public Transform TriggerTransform;

        [Tooltip("How quickly to lerp TriggerTransform when trigger is pressed")]
        public float TriggerRotateSpeed = 15f;

        /// <summary>
        /// Move this back on fire
        /// </summary>
        [Tooltip("Animate this back on fire")]
        public Transform SlideTransform;

        /// <summary>
        /// Where our raycast or projectile will spawn from
        /// </summary>
        [Tooltip("Where our raycast or projectile will start from.")]
        public Transform MuzzlePointTransform;

        /// <summary>
        /// Where our raycast or projectile will spawn from
        /// </summary>
        [Tooltip("Where our raycast or projectile will start from if IsSilenced is true.")]
        public Transform MuzzlePointSilencedTransform;

        /// <summary>
        /// Where to eject a bullet casing (optional)
        /// </summary>
        [Tooltip("Where to eject a bullet casing (optional)")]
        public Transform EjectPointTransform;

        /// <summary>
        /// Transform of Chambered Bullet. Hide this when no bullet is chambered
        /// </summary>
        [Tooltip("Transform of Chambered Bullet inside the weapon. Hide this when no bullet is chambered. (Optional)")]
        public Transform ChamberedBullet;

        /// <summary>
        /// Make this active on fire. Randomize scale / rotation
        /// </summary>
        [Tooltip("Make this active on fire. Randomize scale / rotation")]
        public GameObject MuzzleFlashObject;

        /// <summary>
        /// Make this active on fire if IsSilenced = true. Randomize scale / rotation
        /// </summary>
        [Tooltip("Make this active on fire if IsSilenced = true. Randomize scale / rotation")]
        public GameObject MuzzleFlashSilencedObject;

        /// <summary>
        /// Eject this at EjectPointTransform (optional)
        /// </summary>
        [Tooltip("Eject this at EjectPointTransform (optional)")]
        public GameObject BulletCasingPrefab;

        /// <summary>
        /// If time is slowed this object will be instantiated instead of using a raycast
        /// </summary>
        [Tooltip("If time is slowed this object will be instantiated at muzzle point instead of using a raycast")]
        public GameObject ProjectilePrefab;

        /// <summary>
        /// Hit Effects spawned at point of impact
        /// </summary>
        [Tooltip("Hit Effects spawned at point of impact")]
        public GameObject HitFXPrefab;

        /// <summary>
        /// Play this sound on shoot
        /// </summary>
        [Tooltip("Play this sound on shoot")]
        public AudioClip GunShotSound;

        [Tooltip("Volume to play the GunShotSound clip at. Range 0-1")]
        [Range(0.0f, 1f)]
        public float GunShotVolume = 0.75f;

        [Tooltip("If True use GunShotSilencedSound sound clip. Could also be used to lower damage, accuracy, range, etc.")]
        public bool IsSilenced = false;

        /// <summary>
        /// Play this sound on shoot  if IsSilenced is True
        /// </summary>
        [Tooltip("Play this sound on shoot if IsSilenced is True")]
        public AudioClip GunShotSilencedSound;

        [Tooltip("Volume to play the GunShotSilencedSound clip at. Range 0-1")]
        [Range(0.0f, 1f)]
        public float GunShotSilencedVolume = 0.75f;

        /// <summary>
        /// Play this sound if no ammo and user presses trigger
        /// </summary>
        [Tooltip("Play this sound if no ammo and user presses trigger")]
        public AudioClip EmptySound;

        [Tooltip("Volume to play the EmptySound clip at. Range 0-1")]
        [Range(0.0f, 1f)]
        public float EmptySoundVolume = 1f;

        [Header("Slide Configuration : ")]
        /// <summary>
        /// How far back to move the slide on fire
        /// </summary>
        [Tooltip("How far back to move the slide on fire")]
        public float SlideDistance = -0.028f;        

        /// <summary>
        /// Should the slide be forced back if we shoot the last bullet
        /// </summary>
        [Tooltip("Should the slide be forced back if we shoot the last bullet")]
        public bool ForceSlideBackOnLastShot = true;

        [Tooltip("How fast to move back the slide on fire. Default : 1")]
        public float slideSpeed = 1;

        [Header("Inputs : ")]
        [Tooltip("Controller Input used to eject clip")]
        public List<GrabbedControllerBinding> EjectInput = new List<GrabbedControllerBinding>() { GrabbedControllerBinding.Button2Down };

        [Tooltip("Controller Input used to release the charging mechanism.")]
        public List<GrabbedControllerBinding> ReleaseSlideInput = new List<GrabbedControllerBinding>() { GrabbedControllerBinding.Button1Down };

        [Tooltip("Controller Input used to release reload the weapon if ReloadMethod = InternalAmmo.")]
        public List<GrabbedControllerBinding> ReloadInput = new List<GrabbedControllerBinding>() { GrabbedControllerBinding.Button2Down };

        [Header("Shown for Debug : ")]
        /// <summary>
        /// Is there currently a bullet chambered and ready to be fired
        /// </summary>
        [Tooltip("Is there currently a bullet chambered and ready to be fired")]
        public bool BulletInChamber = false;

        /// <summary>
        /// Is there currently a bullet chambered and that must be ejected
        /// </summary>
        [Tooltip("Is there currently a bullet chambered and that must be ejected")]
        public bool EmptyBulletInChamber = false;

        [Header("Events")]

        [Tooltip("Unity Event called when Shoot() method is successfully called")]
        public UnityEvent onShootEvent;

        [Tooltip("Unity Event called when something attaches ammo to the weapon")]
        public UnityEvent onAttachedAmmoEvent;

        [Tooltip("Unity Event called when something detaches ammo from the weapon")]
        public UnityEvent onDetachedAmmoEvent;

        [Tooltip("Unity Event called when the charging handle is successfully pulled back on the weapon")]
        public UnityEvent onWeaponChargedEvent;

        [Tooltip("Unity Event called when weapon damaged something")]
        public FloatEvent onDealtDamageEvent;

        [Tooltip("Passes along Raycast Hit info whenever a Raycast hit is successfully detected. Use this to display fx, add force, etc.")]
        public RaycastHitEvent onRaycastHitEvent;

        [Tooltip("Unity Event called when PlayEmptyShotSound() method is called")]
        public UnityEvent onPlayedEmptyShotEvent;

        [Tooltip("Unity Event called when EjectMagazine() method is called")]
        public UnityEvent onEjectMagazineEvent;

        /// <summary>
        /// Is the slide / receiver forced back due to last shot
        /// </summary>
        protected bool slideForcedBack = false;

        protected WeaponSlide ws;

        protected bool readyToShoot = true;

        // Magainze currently equipped
        protected Magazine heldMagazine;


        protected MagazineSlide magazineSlide {
            get {
                if(_ms) {
                    return _ms;
                }
                return _ms = GetComponentInChildren<MagazineSlide>();
            }
        }
        private MagazineSlide _ms;

        void Start() {
            weaponRigid = GetComponent<Rigidbody>();

            if (MuzzleFlashObject) {
                MuzzleFlashObject.SetActive(false);
            }
            if (MuzzleFlashSilencedObject) {
                MuzzleFlashSilencedObject.SetActive(false);
            }

            ws = GetComponentInChildren<WeaponSlide>();

            updateChamberedBullet();
        }        

        public override void OnTrigger(float triggerValue) {

            // Sanitize for angles 
            triggerValue = Mathf.Clamp01(triggerValue);

            // Update trigger graphics
            if (TriggerTransform) {

                // Lerp current position to value between 0-15 degrees
                float updateAngle = Mathf.Lerp(TriggerTransform.localEulerAngles.x, triggerValue * 15, Time.deltaTime * TriggerRotateSpeed); // 10f is the speed factor

                // Apply the updated rotation to the transform
                TriggerTransform.localEulerAngles = new Vector3(updateAngle, 0, 0);
            }

            // Trigger up, reset values
            if (triggerValue <= 0.5) {
                readyToShoot = true;
                playedEmptySound = false;
            }

            // Fire gun if possible
            if (readyToShoot && triggerValue >= 0.75f) {
                Shoot();

                // Immediately ready to keep firing if 
                readyToShoot = FiringMethod == FiringType.Automatic;
            }

            // These are here for convenience. Could be called through GrabbableUnityEvents instead
            checkSlideInput();
            checkEjectInput();
            CheckReloadInput();

            updateChamberedBullet();

            base.OnTrigger(triggerValue);
        }

        void checkSlideInput() {
            // Check for bound controller button to release the charging mechanism
            for (int x = 0; x < ReleaseSlideInput.Count; x++) {
                if (InputBridge.Instance.GetGrabbedControllerBinding(ReleaseSlideInput[x], thisGrabber.HandSide)) {
                    UnlockSlide();
                    break;
                }
            }
        }

        void checkEjectInput() {
            // Check for bound controller button to eject magazine
            for (int x = 0; x < EjectInput.Count; x++) {
                if (InputBridge.Instance.GetGrabbedControllerBinding(EjectInput[x], thisGrabber.HandSide)) {
                    EjectMagazine();
                    break;
                }
            }
        }

        public virtual void CheckReloadInput() {
            if(ReloadMethod == ReloadType.InternalAmmo) {
                // Check for Reload input(s)
                for (int x = 0; x < ReloadInput.Count; x++) {
                    if (InputBridge.Instance.GetGrabbedControllerBinding(EjectInput[x], thisGrabber.HandSide)) {
                        Reload();
                        break;
                    }
                }
            }            
        }

        public virtual void UnlockSlide() {
            if (ws != null) {
                ws.UnlockBack();
            }
        }


        public virtual void EjectMagazine() {
            if (magazineSlide != null) {
                magazineSlide.EjectMagazine();
            }

            if (onEjectMagazineEvent != null) {
                onEjectMagazineEvent.Invoke();
            }
        }

        protected bool playedEmptySound = false;
        
        public virtual void Shoot() {

            if (SafetyOn) {
                return;
            }
            
            // Has enough time passed between shots
            float shotInterval = Time.timeScale < 1 ? SlowMoRateOfFire : FiringRate;
            if (Time.time - lastShotTime < shotInterval) {
                return;
            }

            // Need to Chamber round into weapon
            if(!BulletInChamber && MustChamberRounds) {
                // Only play empty sound once per trigger down
                if(!playedEmptySound) {
                    PlayEmptyShotSound();
                    playedEmptySound = true;
                }
                
                return;
            }
            // Weapon doesn't require chamber, but has no bullets
            else if(!MustChamberRounds && GetBulletCount() == 0 && ReloadMethod != ReloadType.InfiniteAmmo) {

                if (!playedEmptySound) {
                    PlayEmptyShotSound();
                    playedEmptySound = true;
                }
                return;
            }

            // Need to release slide
            if(ws != null && ws.LockedBack) {
                VRUtils.Instance.PlaySpatialClipAt(EmptySound, transform.position, EmptySoundVolume, 0.5f);
                return;
            }

            // Create our own spatial clip
            // Silenced vs. Unsilenced clip
            if(IsSilenced && GunShotSilencedSound) {
                VRUtils.Instance.PlaySpatialClipAt(GunShotSilencedSound, transform.position, GunShotSilencedVolume);
            }
            else {
                VRUtils.Instance.PlaySpatialClipAt(GunShotSound, transform.position, GunShotVolume);
            }

            // Haptics
            if (thisGrabber != null) {
                input.VibrateController(0.1f, 0.2f, 0.1f, thisGrabber.HandSide);
            }

            Transform muzzleTransform = GetMuzzlePointTransform();

            // Use projectile if Time has been slowed
            bool useProjectile = AlwaysFireProjectile || (FireProjectileInSlowMo && Time.timeScale < 1);
            if (useProjectile) {
                

                GameObject projectile = Instantiate(ProjectilePrefab, muzzleTransform.position, muzzleTransform.rotation) as GameObject;
                Rigidbody projectileRigid = projectile.GetComponentInChildren<Rigidbody>();
                projectileRigid.AddForce(muzzleTransform.forward * ShotForce, ForceMode.VelocityChange);
                
                Projectile proj = projectile.GetComponent<Projectile>();
                // Convert back to raycast if Time reverts
                if (proj && !AlwaysFireProjectile) {
                    proj.MarkAsRaycastBullet();
                }

                if(proj && LaserGuided) {
                    if(LaserPoint == null) {
                        LaserPoint = muzzleTransform;
                    }

                    proj.MarkAsLaserGuided(muzzleTransform);
                }

                // Make sure we clean up this projectile
                Destroy(projectile, 20);
            }
            else {
                // Raycast to hit
                RaycastHit hit;
                if (Physics.Raycast(muzzleTransform.position, muzzleTransform.forward, out hit, MaxRange, ValidLayers, QueryTriggerInteraction.Ignore)) {
                    OnRaycastHit(hit);
                }
            }

            // Apply recoil
            ApplyRecoil();

            // We just fired this bullet
            BulletInChamber = false;

            // Try to load a new bullet into chamber         
            if (AutoChamberRounds) {
                chamberRound();
            }
            else {
                EmptyBulletInChamber = true;
            }

            // Unable to chamber bullet, force slide back
            if(!BulletInChamber) {
                // Do we need to force back the receiver?
                slideForcedBack = ForceSlideBackOnLastShot;

                if (slideForcedBack && ws != null) {
                    ws.LockBack();
                }
            }

            // Call Shoot Event
            if(onShootEvent != null) {
                onShootEvent.Invoke();
            }

            // Store our last shot time to be used for rate of fire
            lastShotTime = Time.time;

            DoMuzzleFlash();

            if (AutoChamberRounds) {
                // Animate Slide, Eject Shell, Muzzle Flash
                if (ws) {
                    ws.BlowbackSlide(0.1f);
                }

                // Eject Shell Slightly after slide is back
                Invoke("EjectShell", 0.05f);
            }
        }

        public Transform GetMuzzlePointTransform() {
            if(IsSilenced && MuzzlePointSilencedTransform != null) {
                return MuzzlePointSilencedTransform;
            }

            return MuzzlePointTransform;
        }

        public void DoMuzzleFlash() {

            // Bail early
            if (MuzzleFlashObject == null && MuzzleFlashSilencedObject == null) {
                return;
            }

            // Stop previous routine
            if (muzzleFlashRoutine != null) {
                GetMuzzleFlashObject().SetActive(false);
                StopCoroutine(muzzleFlashRoutine);
            }

            // Start the routine again to show a new muzzle flash
            muzzleFlashRoutine = doMuzzleFlash();
            StartCoroutine(muzzleFlashRoutine);
        }

        public GameObject GetMuzzleFlashObject() {

            if (IsSilenced && MuzzleFlashSilencedObject != null) {
                return MuzzleFlashSilencedObject;
            }

            return MuzzleFlashObject;
        }

        public void EjectShell() {
            ejectCasing();
        }

        public virtual void PlayEmptyShotSound() {
            VRUtils.Instance.PlaySpatialClipAt(EmptySound, transform.position, EmptySoundVolume, 0.5f);

            // Call Shoot Event
            if (onPlayedEmptyShotEvent != null) {
                onPlayedEmptyShotEvent.Invoke();
            }
        }

        // Apply recoil by requesting sprinyness and apply a local force to the muzzle point
        public virtual void ApplyRecoil() {

            Vector3 recoilForce = grab.BeingHeldWithTwoHands ? RecoilForceTwoHanded : RecoilForce;

            if (weaponRigid != null && recoilForce != Vector3.zero) {

                // Make weapon springy for X seconds
                grab.RequestSpringTime(RecoilDuration);

                // Apply the Recoil Force
                Transform muzzleTransform = GetMuzzlePointTransform();
                
                weaponRigid.AddForceAtPosition(muzzleTransform.TransformDirection(recoilForce), muzzleTransform.position, ForceMode.VelocityChange);
            }
        }

        // Hit something without Raycast. Apply damage, apply FX, etc.
        public virtual void OnRaycastHit(RaycastHit hit) {

            ApplyParticleFX(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), hit.collider);

            // push object if rigidbody
            Rigidbody hitRigid = hit.collider.attachedRigidbody;
            if (hitRigid != null) {
                Transform muzzleTransform = GetMuzzlePointTransform();
                hitRigid.AddForceAtPosition(BulletImpactForce * muzzleTransform.forward, hit.point);
            }

            // Damage if possible
            Damageable d = hit.collider.GetComponent<Damageable>();
            if (d) {
                d.DealDamage(Damage, hit.point, hit.normal, true, gameObject, hit.collider.gameObject);

                if (onDealtDamageEvent != null) {
                    onDealtDamageEvent.Invoke(Damage);
                }
            }

            // Call event
            if (onRaycastHitEvent != null) {
                onRaycastHitEvent.Invoke(hit);
            }
        }

        public virtual void ApplyParticleFX(Vector3 position, Quaternion rotation, Collider attachTo) {
            if(HitFXPrefab) {
                GameObject impact = Instantiate(HitFXPrefab, position, rotation) as GameObject;

                // Attach bullet hole to object if possible
                BulletHole hole = impact.GetComponent<BulletHole>();
                if (hole) {
                    hole.TryAttachTo(attachTo);
                }
            }
        }

        /// <summary>
        /// Something attached ammo to us
        /// </summary>
        public virtual void OnAttachedAmmo() {

            // May have ammo loaded
            updateChamberedBullet();

            if(onAttachedAmmoEvent != null) {
                onAttachedAmmoEvent.Invoke();
            }
        }

        /// <summary>
        /// Used for magazine type reloads
        /// </summary>
        /// <param name="attachMagazine"></param>
        public virtual void OnAttachedAmmo(Magazine attachMagazine) {
            heldMagazine = attachMagazine;

            OnAttachedAmmo();
        }

        // Ammo was detached from the weapon
        public virtual void OnDetachedAmmo() {
            // May have ammo loaded / unloaded
            updateChamberedBullet();

            // Detached a magazine. Ready for a new one
            if (ReloadMethod == ReloadType.Magazine) {
                heldMagazine = null;
            }

            if (onDetachedAmmoEvent != null) {
                onDetachedAmmoEvent.Invoke();
            }
        }

        public float GetLastShotTime() {
            return lastShotTime;
        }

        /// <summary>
        /// Is the gun ready to fire?
        /// </summary>
        public bool GetReadyToShoot() {
            return readyToShoot;
        }

        public virtual int GetBulletCount() {
            if (ReloadMethod == ReloadType.InfiniteAmmo) {
                return 999;
            }
            else if (ReloadMethod == ReloadType.InternalAmmo) {
                return (int)InternalAmmo;
            }
            else if (ReloadMethod == ReloadType.ManualClip) {
                return GetComponentsInChildren<Bullet>(false).Length + (MustChamberRounds && BulletInChamber ? 1 : 0);
            } 
            else if (ReloadMethod == ReloadType.Magazine) {
                if(heldMagazine != null) {
                    return heldMagazine.CurrentBulletCount + (MustChamberRounds && BulletInChamber ? 1 : 0);
                }
                return (MustChamberRounds && BulletInChamber ? 1 : 0);
            }

            // Default to bullet count
            return GetComponentsInChildren<Bullet>(false).Length + (MustChamberRounds && BulletInChamber ? 1 : 0);
        }

        /// <summary>
        /// Returns number of total bullets attaches to gun, including active
        /// </summary>
        /// <returns></returns>
        public virtual int GetMaxBulletCount() {
            if (ReloadMethod == ReloadType.InfiniteAmmo) {
                return 999;
            } 
            else if (ReloadMethod == ReloadType.InternalAmmo) {
                return (int)MaxInternalAmmo;
            } 
            else if (ReloadMethod == ReloadType.Magazine) {
                if (heldMagazine != null) {
                    return heldMagazine.MaxBulletCount;
                }
            } 
            else if (ReloadMethod == ReloadType.ManualClip) {
                return GetComponentsInChildren<Bullet>(true).Length;
            }

            // Default to bullet count
            return GetComponentsInChildren<Bullet>(true).Length;
        }

        public virtual void RemoveBullet() {

            // Don't remove bullet here
            if (ReloadMethod == ReloadType.InfiniteAmmo) {
                return;
            }

            else if (ReloadMethod == ReloadType.InternalAmmo) {
                InternalAmmo--;
            } 
            else if (ReloadMethod == ReloadType.Magazine) {
                if(heldMagazine) {
                    heldMagazine.RemoveBullet();
                }
            } 
            else if (ReloadMethod == ReloadType.ManualClip) {
                Bullet firstB = GetComponentInChildren<Bullet>(false);
                // Deactivate gameobject as this bullet has been consumed
                if (firstB != null) {
                    firstB.gameObject.SetActive(false);
                    //Destroy(firstB.gameObject);
                }
            }

            // Whenever we remove a bullet is a good time to check the chamber
            updateChamberedBullet();
        }

        public virtual void Reload() {
            InternalAmmo = MaxInternalAmmo;
        }

        void updateChamberedBullet() {
            if (ChamberedBullet != null) {
                ChamberedBullet.gameObject.SetActive(BulletInChamber || EmptyBulletInChamber);
            }
        }

        void chamberRound() {

            if(BulletInChamber) {
                Debug.Log("Already chambered!");
                return;
            }

            int currentBulletCount = GetBulletCount();

            if(currentBulletCount > 0) {
                // Remove the first bullet we find in the clip                
                RemoveBullet();

                // That bullet is now in chamber
                BulletInChamber = true;
            }
            // Unable to chamber a bullet
            else {
                BulletInChamber = false;
            }
        }

        protected IEnumerator muzzleFlashRoutine;        

        // Randomly scale / rotate to make them seem different
        void randomizeMuzzleFlashScaleRotation() {
            Transform muzzleTransform = GetMuzzleFlashObject().transform;
            muzzleTransform.localScale = Vector3.one * UnityEngine.Random.Range(0.75f, 1.5f);
            muzzleTransform.localEulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(0, 90f));
        }       

        public virtual void OnWeaponCharged(bool allowCasingEject) {

            // Already bullet in chamber, eject it
            if (BulletInChamber && allowCasingEject) {
                BulletInChamber = false;
                ejectCasing();
            }
            else if (EmptyBulletInChamber && allowCasingEject) {
                ejectCasing();
                EmptyBulletInChamber = false;
            }

            chamberRound();

            // Slide is no longer forced back if weapon was just charged
            slideForcedBack = false;

            if(onWeaponChargedEvent != null) {
                onWeaponChargedEvent.Invoke();
            }
        }
        
        protected virtual void ejectCasing() {
            if(BulletCasingPrefab) {
                GameObject shell = Instantiate(BulletCasingPrefab, EjectPointTransform.position, EjectPointTransform.rotation) as GameObject;

                Rigidbody rb = shell.GetComponentInChildren<Rigidbody>();

                if (rb) {
                    rb.AddRelativeForce(Vector3.right * BulletCasingForce, ForceMode.VelocityChange);
                }

                // Bit of haptics
                if (thisGrabber != null) {
                    input.VibrateController(0.1f, 0.1f, 0.1f, thisGrabber.HandSide);
                }

                // Clean up shells
                GameObject.Destroy(shell, 5);
            }
        }

        protected virtual IEnumerator doMuzzleFlash() {

            GameObject muzzle = GetMuzzleFlashObject();
            muzzle.SetActive(true);
            yield return new  WaitForSeconds(0.075f);

            randomizeMuzzleFlashScaleRotation();
            yield return new WaitForSeconds(0.075f);

            muzzle.SetActive(false);
        }
        
        public void ToggleSilenced(bool isSilenced) {
            IsSilenced = isSilenced;
        }
    }

    public enum FiringType {
        Semi,
        Automatic
    }

    public enum ReloadType {
        InfiniteAmmo,
        ManualClip, 
        InternalAmmo,
        Magazine
    }
}

