using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Movement07
{
    public static class CustomGravity07
    {
        static List<GravitySource07> sources = new List<GravitySource07>();

        public static Vector3 GetGravity(Vector3 position)
        {
            //return position.normalized * Physics.gravity.y;

            Vector3 g = Vector3.zero;
            for (int i = 0; i < sources.Count; i++)
            {
                g += sources[i].GetGravity(position);
            }
            return -g.normalized;
        }

        public static Vector3 GetUpAxis(Vector3 position)
        {
            //return position.normalized;
            //Vector3 up = position.normalized;
            //return Physics.gravity.y < 0F ? up : -up;

            Vector3 g = Vector3.zero;
            for (int i = 0; i < sources.Count; i++)
            {
                g += sources[i].GetGravity(position);
            }
            return g;
        }

        public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
        {
            //upAxis = position.normalized;
            //return upAxis * Physics.gravity.y;

            //Vector3 up = position.normalized;
            //upAxis = Physics.gravity.y < 0f ? up : -up;
            //return up * Physics.gravity.y;

            Vector3 g = Vector3.zero;
            for (int i = 0; i < sources.Count; i++)
            {
                g += sources[i].GetGravity(position);
            }
            upAxis = -g.normalized;
            return g;
        }

        public static void Register(GravitySource07 source)
        {
            Debug.Assert(!sources.Contains(source), "重复注册引力", source);
            sources.Add(source);
        }

        public static void Unregister(GravitySource07 source)
        {
            Debug.Assert(!sources.Contains(source), "引力数据不存在", source);
            sources.Remove(source);
        }
    }
}
