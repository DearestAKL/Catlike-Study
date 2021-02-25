using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Movement02
{
    public class MovingSphere02 : MonoBehaviour
    {
        private Vector2 m_PlayerInput;//用户输入
        private Vector3 velocity;//缓存速度
        private Vector3 desiredVelocity;//期望速度

        [SerializeField,Range(0f,100f), Header("最大速度")]
        private float maxSpeed = 10f;

        [SerializeField, Range(0f, 100f),Header("最大加速度")]
        private float maxAcceleration = 10f;

        [SerializeField, Range(0f, 100f), Header("最大空中加速度")]
        private float maxAirAcceleration = 1f;

        [SerializeField, Range(0f, 10f), Header("跳跃高度")]
        private float jumpHight = 2f;

        [SerializeField, Range(0, 5), Header("空中跳跃")]
        private int maxAirJumps = 0;

        [SerializeField, Range(0F, 90F), Header("最大地面角度")]
        private float maxGroundAngle = 25f;

        Rigidbody body;

        private bool desiredJump;
        private int jumpPhase;
        private float minGroundDotProduct;
        private int groundContactCount;//接触地面数量

        bool OnGround => groundContactCount > 0;

        /// <summary>
        /// 接触法线
        /// </summary>
        Vector3 contactNormal;

        private void OnValidate()
        {
            //Mathf.Deg2Rad 度转弧度 
            //获取最小的地面的点积
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            OnValidate();
        }

        void Update()
        {
            m_PlayerInput.x = Input.GetAxis("Horizontal");
            m_PlayerInput.y = Input.GetAxis("Vertical");

            //获取长度为1f 的向量副本
            m_PlayerInput = Vector2.ClampMagnitude(m_PlayerInput, 1f);

            //预期速度
            desiredVelocity = new Vector3(m_PlayerInput.x, 0f, m_PlayerInput.y) * maxSpeed;

            //按下jump 为true否则为false
            desiredJump |= Input.GetButtonDown("Jump");
           
            GetComponent<Renderer>().material.SetColor("_Color", Color.white * (groundContactCount * 0.25f)) ;
        }

        private void FixedUpdate()
        {
            UpdateState();
            AdjustVelocity();

            if (desiredJump)
            {
                desiredJump = false;
                Jump();
            }


            body.velocity = velocity;

            //onGround = false;
            ClearState();
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        private void UpdateState()
        {
            //获取当前速度
            velocity = body.velocity;

            if (OnGround)
            {
                jumpPhase = 0;
                if (groundContactCount > 1)
                {
                    contactNormal.Normalize();
                }
            }
            else
            {
                contactNormal = Vector3.up;
            }
        }

        //清除状态
        private void ClearState()
        {
            //onGround = false;
            groundContactCount = 0;
            contactNormal = Vector3.zero;
        }


        /// <summary>
        /// 跳跃
        /// </summary>
        private void Jump()
        {
            if (OnGround || jumpPhase < maxAirJumps)
            {
                jumpPhase += 1;
                //速度值
                float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHight);
                //对齐速度值
                float alignedSpeed = Vector3.Dot(velocity, contactNormal);
                if (velocity.y > 0)
                {
                    //获取实际的 基于法线的 跳跃速度值
                    jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
                }
                velocity += contactNormal * jumpSpeed;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            EvaluateCollision(collision);
        }


        private void OnCollisionStay(Collision collision)
        {
            //onGround = true;
            EvaluateCollision(collision);
        }

        /// <summary>
        /// 碰撞检测
        /// </summary>
        /// <param name="collision"></param>
        private void EvaluateCollision(Collision collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;
                if(normal.y >= minGroundDotProduct)
                {
                    //onGround = true;
                    groundContactCount += 1;
                    //累积法线
                    contactNormal += normal;
                }
            }
        }

        /// <summary>
        /// 求沿斜面 斜边 方向速度
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        private Vector3 ProjectOnContactPlane(Vector3 vector)
        {
            //v - n*(|v|*cos角) 把速度分解为斜边方向与斜边法线方向 求斜边方向向量
            return vector - contactNormal * Vector3.Dot(vector, contactNormal);
        }

        /// <summary>
        /// 调整速度
        /// </summary>
        /// <returns></returns>
        private void AdjustVelocity()
        {
            //获取映射的 相对斜边的 X 和 Z轴
            //相对于斜边
            Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
            Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

            //求在在世界坐标中X轴和Z轴投影长度 也就是相对斜边的X和Z的速度
            float currentX = Vector3.Dot(velocity, xAxis);
            float currentZ = Vector3.Dot(velocity, zAxis);

            //最大加速度
            float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
            //最大速度变化
            float maxSpeedChange = acceleration * Time.deltaTime;

            //新的方向速率
            float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
            float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

            //新的速率
            velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        }
    }
}