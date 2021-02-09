using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Movement05
{
    public class OrbitCamera05 : MonoBehaviour
    {
        [SerializeField,Header("焦点")]
        Transform focus = default;

        [SerializeField, Range(1f, 20f),Header("离焦点距离")]
        float distance = 5f;

        [SerializeField, Min(0f), Header("焦点缓冲半径")]
        float focusRadius = 1F;

        [SerializeField, Range(0f, 1f), Header("中心化焦点过渡比例")]
        float focusCentering = 0.5f;

        Vector3 focusPoint, previousFocusPoint;//焦点位置，之前焦点位置

        Vector2 orbitAngles = new Vector2(45f, 0f);//轨道角度

        [SerializeField, Range(1f, 360f)]
        float rotationSpeed = 90f;//旋转速度，每秒钟旋转度数

        [SerializeField, Range(-89f, 89f)]
        float minVerticalAngle = -30f, maxVerticalAngle = 60f;//摄像机垂直方向的最大最小角度

        [SerializeField, Range(0f, 100f)]
        float alignDelay = 5;//对齐延迟

        float lastManualRotationTime;//最后手动旋转时间

        [SerializeField, Range(0f, 90f)]
        float alignSmoothRange = 45f;

        Camera regularCamera;

        [SerializeField]
        LayerMask obstructionMask = -1;

        Quaternion gravityAlignment = Quaternion.identity;
        Quaternion orbitRotation;

        private void OnValidate()
        {
            if(maxVerticalAngle < minVerticalAngle)
            {
                maxVerticalAngle = minVerticalAngle;
            }
        }

        private void Awake()
        {
            regularCamera = GetComponent<Camera>();
            focusPoint = focus.position;
            //transform.localRotation = Quaternion.Euler(orbitAngles);
            transform.localRotation = orbitRotation = Quaternion.Euler(orbitAngles);
        }

        private void LateUpdate()
        {
            gravityAlignment = Quaternion.FromToRotation(gravityAlignment * Vector3.up, CustomGravity05.GetUpAxis(focusPoint)) * gravityAlignment;


            UpdateFocusPoint();

            //Quaternion lookRotation;
            if (ManualRotation() || AutomationRotation())
            {
                ConstrainAngles();
                orbitRotation = Quaternion.Euler(orbitAngles);
            }
            //else
            //{
            //    lookRotation = transform.localRotation;
            //}

            Quaternion lookRotation = Quaternion.Euler(orbitAngles);
            Vector3 lookDirection = lookRotation * Vector3.forward;
            Vector3 lookPosition = focusPoint - lookDirection * distance;

            Vector3 rectOffset = lookDirection * regularCamera.nearClipPlane;
            Vector3 rectPosition = lookPosition + rectOffset;
            Vector3 castForm = focus.position;
            Vector3 castLine = rectPosition - castForm;
            float castDistance = castLine.magnitude;//向量 长度
            Vector3 castDirection = castLine / castDistance;//单位向量

            //碰撞检测 规避障碍
            if (Physics.BoxCast(castForm, CameraHalfExtends, castDirection, out RaycastHit hit,lookRotation, castDistance, obstructionMask))
            {
                rectPosition = castForm + castDirection * hit.distance;
                lookPosition = rectPosition - rectOffset;
            }

            transform.SetPositionAndRotation(lookPosition, lookRotation);
        }

        /// <summary>
        /// 更新焦点
        /// </summary>
        private void UpdateFocusPoint()
        {
            previousFocusPoint = focusPoint;//缓存下焦点信息
            Vector3 targetPoint = focus.position;
            if(focusRadius > 0f)
            {
                //存在缓存半径
                float distance = Vector3.Distance(targetPoint, focusPoint);//目标点 与焦点的距离
                float t = 1f;
                if(distance > 0.01f && focusCentering > 0f)
                {
                    //平滑过渡值
                    t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
                }

                if (distance > focusRadius)
                {
                    //大于缓冲半径 
                    //focusPoint = Vector3.Lerp(targetPoint, focusPoint, focusRadius / distance);
                    t = Mathf.Min(t, focusRadius / distance);
                }

                //焦点信息更新
                focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
            }
            else
            {
                focusPoint = targetPoint;
            }
        }
        
        /// <summary>
        /// 手动旋转
        /// </summary>
        /// <returns>是否有有效输入</returns>
        private bool ManualRotation()
        {
            Vector2 input = new Vector2
            (
                -Input.GetAxis("Mouse Y"),
                Input.GetAxis("Mouse X")
            );

            const float e = 0.001f;
            if(input.x <-e || input.x > e || input.y < -e || input.y > e)
            {
                //摄像机旋转
                orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
                lastManualRotationTime = Time.unscaledDeltaTime;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 约束角度
        /// </summary>
        private void ConstrainAngles()
        {
            orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

            if(orbitAngles.y < 0f)
            {
                orbitAngles.y += 360f;
            }
            else if (orbitAngles.y >= 360f)
            {
                orbitAngles.y -= 360f; 
            }
        }

        /// <summary>
        /// 自动旋转
        /// </summary>
        /// <returns></returns>
        private bool AutomationRotation()
        {
            if(Time.unscaledDeltaTime - lastManualRotationTime < alignDelay)
            {
                return false;
            }

            Vector3 alignedDelta = Quaternion.Inverse(gravityAlignment) * (focusPoint - previousFocusPoint);

            //运动向量
            //Vector2 movement = new Vector2(
            //    focusPoint.x = -previousFocusPoint.x,
            //    focusPoint.z = -previousFocusPoint.z
            //);
            Vector2 movement = new Vector2(alignedDelta.x, alignedDelta.z);

            //运动值
            float movementDeltaSqr = movement.sqrMagnitude;

            if(movementDeltaSqr < 0.000001f)
            {
                return false;
            }
            float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));//航向角，y方向
            float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
            float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
            if(deltaAbs < alignSmoothRange)
            {
                rotationChange *= deltaAbs / alignSmoothRange;
            }
            else if(180f - deltaAbs < alignSmoothRange)
            {
                rotationChange *= (180f - deltaAbs) / alignSmoothRange;
            }
            orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
            return true;
        }

        static float GetAngle(Vector2 direction)
        {
            float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
            //return angle;
            return direction.x < 0f ? 360f - angle : angle;
        }


        Vector3 CameraHalfExtends
        {
            get
            {
                Vector3 halfExtends;
                halfExtends.y = regularCamera.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
                halfExtends.x = halfExtends.y * regularCamera.aspect;
                halfExtends.z = 0f;
                return halfExtends;
            }
        }
    }
}