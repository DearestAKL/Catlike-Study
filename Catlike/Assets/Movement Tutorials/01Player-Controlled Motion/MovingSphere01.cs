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

            //获取长度为1f 的向量副本
            m_PlayerInput = Vector2.ClampMagnitude(m_PlayerInput, 1f);

            //预期速度
            Vector3 desiredVelocity = new Vector3(m_PlayerInput.x, 0f, m_PlayerInput.y) * maxSpeed;

            //最大变化量
            float maxSpeedChange = maxAcceleration * Time.deltaTime;

            //velocity.x 向 desiredVelocity.x 过渡 最大变化量为maxSpeedChange，velocity.z同理
            velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
            velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

            //位移
            Vector3 displacement = velocity * Time.deltaTime;

            //新位置
            Vector3 newPosition = transform.localPosition + displacement;

            if(newPosition.x < allowedArea.xMin)
            {
                newPosition.x = allowedArea.xMin;
                velocity.x = -velocity.x * bounciness;
            }
            else if(newPosition.x > allowedArea.xMax)
            {
                newPosition.x = allowedArea.xMax;
                velocity.x = -velocity.x * bounciness;
            }

            if (newPosition.z < allowedArea.yMin)
            {
                newPosition.z = allowedArea.yMin;
                velocity.z = -velocity.z * bounciness;
            }
            else if (newPosition.z > allowedArea.yMax)
            {
                newPosition.z = allowedArea.yMax;
                velocity.z = -velocity.z * bounciness;
            }

            transform.position = newPosition;
        }
    }
}