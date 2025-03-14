using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BNG {
    public class RotateObject : MonoBehaviour {

        public Vector3 RotationAxis = Vector3.up;

        public float RotateSpeed = 10f;
        
        void Update() {
            if(RotateSpeed != 0) {
                transform.Rotate(RotationAxis * RotateSpeed * Time.deltaTime);
            }
        }
    }
}

