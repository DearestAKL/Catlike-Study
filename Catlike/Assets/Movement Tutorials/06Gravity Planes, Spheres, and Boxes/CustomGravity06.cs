using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Movement06
{
    public static class CustomGravity06
    {
        public static Vector3 GetGravity(Vector3 position)
        {
            return position.normalized * Physics.gravity.y;
        }

        public static Vector3 GetUpAxis(Vector3 position)
        {
            //return position.normalized;
            Vector3 up = position.normalized;
            return Physics.gravity.y < 0F ? up : -up;
        }

        public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
        {
            //upAxis = position.normalized;
            //return upAxis * Physics.gravity.y;

            Vector3 up = position.normalized;
            upAxis = Physics.gravity.y < 0f ? up : -up;
            return up * Physics.gravity.y;
        }
    }
}
