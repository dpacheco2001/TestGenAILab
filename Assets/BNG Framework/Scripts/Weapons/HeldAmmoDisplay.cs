using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace BNG {
    public class HeldAmmoDisplay : MonoBehaviour {

        public Grabber LeftGrabber;
        public Grabber RightGrabber;

        public Text AmmoDisplayText;

        int currentAmmo = 0;
        int maxAmmo = 0;

        private int _prevAmmo, _prevMaxAmmo;

        // Cache these for performance
        RaycastWeapon rightWep;
        RaycastWeapon leftWep;

        //void Start() {

        //}

        void Update() {

            // Start at 0 and update accordingly
            currentAmmo = 0;
            maxAmmo = 0;

            bool hasRightWep = false;

            // Check Right grabber for ammo
            if (RightGrabber != null && RightGrabber.HeldGrabbable != null) {
                rightWep = RightGrabber.HeldGrabbable.GetComponent<RaycastWeapon>();
                if (rightWep != null) {
                    hasRightWep = true;
                    currentAmmo = rightWep.GetBulletCount();
                    maxAmmo = rightWep.GetMaxBulletCount();
                }
            }


            // Check Left grabber for ammo if nothing in right grabber
            if(!hasRightWep) {
                if (LeftGrabber != null && LeftGrabber.HeldGrabbable != null) {
                    leftWep = LeftGrabber.HeldGrabbable.GetComponent<RaycastWeapon>();
                    if(leftWep != null) {
                        currentAmmo = leftWep.GetBulletCount();
                        maxAmmo = leftWep.GetMaxBulletCount();
                    }
                }
            }
           
        }

        void OnGUI() {

            // Update text if values have changed
            if(_prevAmmo != currentAmmo || _prevAmmo != _prevMaxAmmo) {
                UpdateTextCount(currentAmmo, maxAmmo);

                _prevAmmo = currentAmmo;
                _prevMaxAmmo = maxAmmo;
            }
        }

        public void UpdateTextCount(int currentAmmo, int maxAmmo) {
            if(AmmoDisplayText != null) {
                if (currentAmmo == 0 && maxAmmo == 0) {
                    AmmoDisplayText.text = "-- / --";
                }
                else {
                    AmmoDisplayText.text = string.Format("{0} / {1}", currentAmmo, maxAmmo);
                }
            }
        }
    }
}

