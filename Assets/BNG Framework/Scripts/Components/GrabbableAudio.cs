using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {
    public class GrabbableAudio : GrabbableEvents {

        [Header("On Grab Audio")]
        public AudioClip OnGrabClip;

        [Range(0.0f, 1.0f)]
        public float OnGrabVolume = 1f;

        public float OnGrabRandomizePitchMinimun = 1.0f;
        public float OnGrabRandomizePitchMaximum = 1.0f;

        [Range(0.0f, 1.0f)]
        public float OnGrabSpatialBlend = 1f;

        [Header("On Release Audio")]
        public AudioClip OnReleaseClip;
        [Range(0.0f, 1.0f)]
        public float OnReleaseVolume = 1f;

        public float OnReleaseRandomizePitchMinimun = 1.0f;
        public float OnReleaseRandomizePitchMaximum = 1.0f;

        [Range(0.0f, 1.0f)]
        public float OnReleaseSpatialBlend = 1f;

        public override void OnGrab(Grabber grabber) {
            if (OnGrabClip) {
                VRUtils.Instance.PlaySpatialClipAt(OnGrabClip, transform.position, OnGrabVolume, OnGrabSpatialBlend, OnGrabRandomizePitchMinimun, OnGrabRandomizePitchMaximum);
            }

            base.OnGrab(grabber);
        }

        public override void OnRelease() {
            if (OnReleaseClip) {
                VRUtils.Instance.PlaySpatialClipAt(OnReleaseClip, transform.position, OnReleaseVolume, OnReleaseSpatialBlend, OnReleaseRandomizePitchMinimun, OnReleaseRandomizePitchMaximum);
            }

            base.OnRelease();
        }
    }
}
