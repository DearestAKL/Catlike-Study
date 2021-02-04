using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Movement01
{
    public class MovingSphere01 : MonoBehaviour
    {
        private Vector2 m_PlayerInput;
        private Vector3 velocity;

        [SerializeField,Range(0f,100f), Header("最大速度")]
        private float maxSpeed = 10f;

        [SerializeField, Range(0f, 100f),Header("最大加速度")]
        private float maxAcceleration = 10f;

        [SerializeField, Range(0f, 1f), Header("弹性系数")]
        private float bounciness = 0.5f;

        [SerializeField,Header("限制范围")]
        Rect allowedArea = new Rect(-5f, -5f, 10f, -10f);

        void Update()
        {
            m_PlayerInput.x = Input.GetAxis("Horizontal");
            m_PlayerInput.y = Input.GetAxis("Vertical");

            //m_PlayerInput.Normalize();
            m_PlayerInput = Vector2.ClampMagnitude(m_PlayerInput, 1f);

            //transform.localPosition = new Vector3(m_PlayerInput.x, 0f, m_PlayerInput.y);
            Vector3 desiredVelocity = new Vector3(m_PlayerInput.x, 0f, m_PlayerInput.y) * maxSpeed;
            float maxSpeedChange = maxAcceleration * Time.deltaTime;

            //if(velocity.x < desiredVelocity.x)
            //{
            //    velocity.x = Mathf.Min(velocity.x + maxSpeedChange, desiredVelocity.x);
            //}
            //else if(velocity.x > desiredVelocity.x)
            //{
            //    velocity.x = Mathf.Max(velocity.x - maxSpeedChange, desiredVelocity.x);
            //}
            velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
            velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

            Vector3 displacement = velocity * Time.deltaTime;

            Vector3 newPosition = transform.localPosition + displacement;
            //if (!allowedArea.Contains(new Vector2(newPosition.x, newPosition.y)))
            //{
            //    //newPosition = transform.localPosition;
            //    newPosition.x = Mathf.Clamp(newPosition.x, allowedArea.xMin, allowedArea.xMax);
            //    newPosition.z = Mathf.Clamp(newPosition.z, allowedArea.yMin, allowedArea.yMax);
            //}
            if(newPosition.x < allowedArea.xMin)
            {
                newPosition.x = allowedArea.xMin;
                //velocity.x = 0f;
                velocity.x = -velocity.x * bounciness;
            }
            else if(newPosition.x > allowedArea.xMax)
            {
                newPosition.x = allowedArea.xMax;
                //velocity.x = 0f;
                velocity.x = -velocity.x * bounciness;
            }

            if (newPosition.z < allowedArea.yMin)
            {
                newPosition.z = allowedArea.yMin;
                //velocity.z = 0f;
                velocity.z = -velocity.z * bounciness;
            }
            else if (newPosition.z > allowedArea.yMax)
            {
                newPosition.z = allowedArea.yMax;
                //velocity.z = 0f;
                velocity.z = -velocity.z * bounciness;
            }

            transform.position = newPosition;
        }
    }
}