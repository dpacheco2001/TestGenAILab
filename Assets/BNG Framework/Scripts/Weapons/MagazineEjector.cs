using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {
    public class MagazineEjector : MonoBehaviour {

        public RaycastWeapon ParentWeapon;
        public MagazineSlide magazineSlide;

        void OnTriggerEnter(Collider other) {
            Grabbable grab = other.GetComponent<Grabbable>();

            // Knock off magazine
            if (magazineSlide.HeldMagazine != null && grab != null && grab != magazineSlide.HeldMagazine) {
                var otherMag = grab.GetComponent<Magazine>();
                if (otherMag != null && otherMag != grab) {
                    magazineSlide.EjectMagazine(0);
                }
            }
        }
    }
}

