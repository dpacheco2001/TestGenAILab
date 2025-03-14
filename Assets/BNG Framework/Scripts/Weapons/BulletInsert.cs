using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {
    public class BulletInsert : MonoBehaviour {

        /// <summary>
        /// The weapon we will be adding Bullets to
        /// </summary>
        [Tooltip("If specified thsi Weapon will have it's InternalAmmo counter increase on bullet insert")]
        public RaycastWeapon Weapon;


        /// <summary>
        /// Magazine to add to blulet count
        /// </summary>
        [Tooltip("If specified this magazine will have it's amom counter increase on bullet insert")]
        public Magazine magazine;

        /// <summary>
        /// Only transforms that contains this name will be accepted as bullets
        /// </summary>
        public string AcceptBulletName = "Bullet";

        public AudioClip InsertSound;

        void OnTriggerEnter(Collider other) {

            Grabbable grab = other.GetComponent<Grabbable>();
            if (grab != null) {
                if (grab.transform.name.Contains(AcceptBulletName)) {

                    if (Weapon != null) {
                        DoInternalWeaponInsert(grab);
                    }

                    if (magazine != null) {
                        DoMagazineInsert(grab);
                    }
                }
            }
        }

        /// <summary>
        /// Add ammo to the InternalAmmo counter
        /// </summary>
        /// <param name="grab">The ammo we are trying to nisert</param>
        /// <returns>Returns true on successful insert</returns>
        public bool DoInternalWeaponInsert(Grabbable grab) {
            // Weapon is full
            if (Weapon.ReloadMethod == ReloadType.InternalAmmo && Weapon.GetBulletCount() >= Weapon.MaxInternalAmmo) {
                Debug.Log("Weapon ammo is full");
                return false;
            }

            // Drop the bullet and add ammo to gun
            grab.DropItem(false, true);
            grab.transform.parent = null;
            GameObject.Destroy(grab.gameObject);

            // Up Ammo Count
            if (Weapon.ReloadMethod == ReloadType.ManualClip) {
                GameObject b = new GameObject();
                b.AddComponent<Bullet>();
                b.transform.parent = Weapon.transform;
            } 
            else if (Weapon.ReloadMethod == ReloadType.InternalAmmo) {
                Weapon.InternalAmmo++;
            }

            // Play Sound
            if (InsertSound) {
                VRUtils.Instance.PlaySpatialClipAt(InsertSound, transform.position, 1f, 0.5f);
            }

            return true;
        }

        public bool DoMagazineInsert(Grabbable grab) {
            // Magazine is full
            if (magazine.CurrentBulletCount >= magazine.MaxBulletCount) {
                Debug.Log("Weapon ammo is full");
                return false;
            }

            // Destroy the bullet and add ammo to magazine
            grab.DropItem(false, true);
            grab.transform.parent = null;
            GameObject.Destroy(grab.gameObject);

            // Up Ammo Count
            magazine.AddBullet();

            // Play Sound
            if (InsertSound) {
                VRUtils.Instance.PlaySpatialClipAt(InsertSound, transform.position, 1f, 0.5f);
            }

            return true;
        }
    }
}

