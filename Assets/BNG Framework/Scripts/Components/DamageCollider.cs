using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {

    /// <summary>
    /// This collider will Damage a Damageable object on impact
    /// </summary>
    public class DamageCollider : MonoBehaviour {

        /// <summary>
        /// How much damage to apply to the Damageable object
        /// </summary>
        public float Damage = 25f;

        /// <summary>
        /// Minimum Amount of force necessary to do damage. Expressed as relativeVelocity.magnitude
        /// </summary>
        public float MinForce = 0.1f;

        public bool UseRelativeVelocity = false;

        /// <summary>
        /// Our previous frame's last relative velocity value
        /// </summary>
        public float LastRelativeVelocity = 0;

        // How much impulse force was applied last onCollision enter
        public float LastDamageForce = 0;

        /// <summary>
        /// Should this take damage if this collider collides with something? For example, pushing a box off of a ledge and it hits the ground 
        /// </summary>
        public bool TakeCollisionDamage = false;

        /// <summary>
        /// How much damage to apply if colliding with something at speed
        /// </summary>
        public float CollisionDamage = 5;

        Damageable thisDamageable;

        void Start() {
            thisDamageable = GetComponent<Damageable>();
        }

        private void OnCollisionEnter(Collision collision) {

            if(!this.isActiveAndEnabled) {
                return;
            }

            OnCollisionEvent(collision);
        }

        public virtual void OnCollisionEvent(Collision collision) {
            LastDamageForce = collision.impulse.magnitude;
            LastRelativeVelocity = collision.relativeVelocity.magnitude;

            bool validCollision = UseRelativeVelocity ? LastRelativeVelocity >= MinForce : LastDamageForce >= MinForce;

            if (validCollision) {

                // Can we damage what we hit?
                Damageable d = collision.gameObject.GetComponent<Damageable>();
                if (d) {
                    d.DealDamage(Damage, collision.GetContact(0).point, collision.GetContact(0).normal, true, gameObject, collision.gameObject);
                }
                // Otherwise, can we take damage ourselves from this collision?
                else if (TakeCollisionDamage && thisDamageable != null) {
                    thisDamageable.DealDamage(CollisionDamage, collision.GetContact(0).point, collision.GetContact(0).normal, true, gameObject, collision.gameObject);
                }
            }
        }
    }
}