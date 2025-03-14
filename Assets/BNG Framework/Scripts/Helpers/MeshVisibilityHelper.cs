using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;


namespace BNG {
    public class MeshVisibilityHelper : MonoBehaviour {

        public SkinnedMeshRenderer skinnedMeshRenderer;

        public ShadowCastingMode InitialShadowCastingMode = ShadowCastingMode.On;

        void Awake() {
            skinnedMeshRenderer.shadowCastingMode = InitialShadowCastingMode;
        }
    }
}

