using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {
    public class FollowTransform : MonoBehaviour {

        public bool UseUpdate = true;
        public bool UseLateUpdate = false;

        public Transform FollowTarget;
        public bool MatchRotation = true;

        public bool UnparentOnStart = false;

        public float YOffset = 0;

        void Start() {
            if(UnparentOnStart) {
                transform.parent = null;
            }
        }

        void Update() {
            if(UseUpdate) {
                MoveObject();
            }
        }

        public virtual void LateUpdate() {
            if(UseLateUpdate) {
                MoveObject();
            }
        }

        public virtual void MoveObject() {
            if (FollowTarget) {
                transform.position = FollowTarget.position;

                if (YOffset != 0) {
                    transform.position += new Vector3(0, YOffset, 0);
                }

                if (MatchRotation) {
                    transform.rotation = FollowTarget.rotation;
                }
            }
        }
    }
}