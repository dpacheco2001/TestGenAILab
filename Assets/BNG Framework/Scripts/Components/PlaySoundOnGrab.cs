using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {
    public class PlaySoundOnGrab : GrabbableEvents {

        public AudioClip SoundToPlay;

        public float Volume = 1f;

        public float RandomizePitchMinimun = 1.0f;
        public float RandomizePitchMaximum = 1.0f;

        public float SpatialBlend = 1f;

        public override void OnGrab(Grabber grabber) {
            if(SoundToPlay) {
                VRUtils.Instance.PlaySpatialClipAt(SoundToPlay, transform.position, Volume, SpatialBlend, RandomizePitchMinimun, RandomizePitchMaximum);
            }

            base.OnGrab(grabber);
        }
    }
}