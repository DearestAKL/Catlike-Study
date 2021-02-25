using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Movement07
{
    public class GravitySource07 : MonoBehaviour
    {
        public virtual Vector3 GetGravity(Vector3 position)
        {
            return Physics.gravity;
        }

        private void OnEnable()
        {
            CustomGravity07.Register(this);
        }

        private void OnDisable()
        {
            CustomGravity07.Unregister(this);
        }
    }
}