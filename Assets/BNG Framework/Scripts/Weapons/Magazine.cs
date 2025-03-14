using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BNG {
    public class Magazine : MonoBehaviour {

        public int CurrentBulletCount = 10;
        public int MaxBulletCount = 10;

        // public bool RemoveOnMagazineCollision = false;

        RaycastWeapon mountedToWeapon;

        /// <summary>
        /// Optionally supply a list of GameObjects that correspond to bullets in the magazine. When bullets are removed, these gameobjects are disabled. When added, they are enabled. 
        /// </summary>
        public Transform BulletGraphicsParent;

        /// <summary>
        /// Set to true to check all bulllet graphics on start
        /// </summary>
        public bool RefreshBulletGraphicsOnStart = false;

        void Awake() {
            UpdateAllBulletGraphics();
        }

        public virtual void RemoveBullet() {
            CurrentBulletCount--;

            // Disable gameObject representing this bullet, if available
            int bulletIndex = MaxBulletCount - (MaxBulletCount - CurrentBulletCount);
            if (BulletGraphicsParent != null && BulletGraphicsParent.childCount > bulletIndex) {
                BulletGraphicsParent.GetChild(bulletIndex).gameObject.SetActive(false);
            }

            if (CurrentBulletCount < 0) {
                CurrentBulletCount = 0;
            }
        }

        public virtual void AddBullet() {
            CurrentBulletCount++;

            // Disable gameObject representing this bullet, if available
            int bulletIndex = MaxBulletCount - (MaxBulletCount - CurrentBulletCount);
            if (BulletGraphicsParent != null && BulletGraphicsParent.childCount >= bulletIndex) {
                BulletGraphicsParent.GetChild(bulletIndex - 1).gameObject.SetActive(true);
            }

            if (CurrentBulletCount > MaxBulletCount) {
                CurrentBulletCount = MaxBulletCount;
            }
        }

        public virtual void UpdateAllBulletGraphics() {
            for(int x = 0; x < MaxBulletCount; x++) {
                if (BulletGraphicsParent != null && BulletGraphicsParent.childCount > x) {
                    BulletGraphicsParent.GetChild(x).gameObject.SetActive(x < CurrentBulletCount);
                }
            }
        }
    }
}


