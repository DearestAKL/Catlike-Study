using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Movement06
{
    public class GravitySource06 : MonoBehaviour
    {
        public virtual Vector3 GetGravity(Vector3 position)
        {
            return Physics.gravity;
        }

        private void OnEnable()
        {
            CustomGravity06.Register(this);
        }

        private void OnDisable()
        {
            CustomGravity06.Unregister(this);
        }
    }
}