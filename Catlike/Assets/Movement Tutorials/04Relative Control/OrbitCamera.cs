using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Movement04
{
    public class OrbitCamera : MonoBehaviour
    {
        [SerializeField]
        Transform focus = default;

        [SerializeField, Range(1f, 20f)]
        float distance = 5f;

        [SerializeField, Min(0f)]
        float focusRadius = 1F;

        [SerializeField, Range(0f, 1f)]
        float focusCentering = 0.5f;

        Vector3 focusPoint;

        Vector2 orbitAngles = new Vector2(45f, 0f);

        private void Awake()
        {
            focusPoint = focus.position;
        }

        private void LateUpdate()
        {
            //Vector3 focusPoint = focus.position;
            UpdateFocusPoint();
            Quaternion lookRotation = Quaternion.Euler(orbitAngles);
            Vector3 lookDirection = lookRotation * transform.forward;
            Vector3 lookPosition = focusPoint - lookDirection * distance;
            transform.SetPositionAndRotation(lookPosition, lookRotation);
        }

        /// <summary>
        /// 更新焦点
        /// </summary>
        private void UpdateFocusPoint()
        {
            Vector3 targetPoint = focus.position;
            if(focusRadius > 0f)
            {
                float distance = Vector3.Distance(targetPoint, focusPoint);
                float t = 1f;
                if(distance > 0.01f && focusCentering > 0f)
                {
                    t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
                }
                if (distance > focusRadius)
                {
                    //focusPoint = Vector3.Lerp(targetPoint, focusPoint, focusRadius / distance);
                    t = Mathf.Min(t, focusRadius / distance);
                }
                focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
            }
            else
            {
                focusPoint = targetPoint;
            }
        }
    }
}