using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Base02
{
    public class Graph : MonoBehaviour
    {
        public Transform pointPrefab;

        [SerializeField, Range(10, 100)]
        int resolution = 10;

        Transform[] points;

        private void Awake()
        {
            float step = 2f / resolution;
            Vector3 scale = Vector3.one * step;
            Vector3 position;
            position.z = 0f;

            points = new Transform[resolution];
            for (int i = 0; i < resolution; i++)
            {
                Transform point = Instantiate(pointPrefab);
                position.x = (i + 0.5f) * step - 1f;
                position.y = Mathf.Sin(position.x);
                point.localPosition = position;
                point.localScale = scale;
                point.SetParent(transform);

                points[i] = point;
            }

        }

        private void Update()
        {
            for (int i = 0; i < points.Length; i++) 
            {
                Transform point = points[i];
                Vector3 position = point.localPosition;
                position.y = Mathf.Sin(Mathf.PI * (position.x + Time.time));
                point.localPosition = position;
            }
        }
    }
}
