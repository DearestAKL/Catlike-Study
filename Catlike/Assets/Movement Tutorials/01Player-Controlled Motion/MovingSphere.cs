using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Movement02
{
    public class MovingSphere : MonoBehaviour
    {
        private Vector2 m_PlayerInput;
        private Vector3 velocity;
        private Vector3 desiredVelocity;

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

        [SerializeField, Range(0F, 90F), Header("角度")]
        private float maxGroundAngle = 25f;

        Rigidbody body;

        bool desiredJump;
        bool onGround;
        int jumpPhase;
        float minGroundDotProduct;

        Vector3 contactNormal;

        private void OnValidate()
        {
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
            m_PlayerInput = Vector2.ClampMagnitude(m_PlayerInput, 1f);

            desiredVelocity = new Vector3(m_PlayerInput.x, 0f, m_PlayerInput.y) * maxSpeed;

            desiredJump |= Input.GetButtonDown("Jump");
        }

        private void FixedUpdate()
        {
            UpdateState();
            if (desiredJump)
            {
                desiredJump = false;
                Jump();
            }

            float acceleration = onGround ? maxAcceleration : maxAirAcceleration;

            float maxSpeedChange = acceleration * Time.fixedDeltaTime;
            velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
            velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

            body.velocity = velocity;

            onGround = false;
        }

        void UpdateState()
        {
            velocity = body.velocity;
            if (onGround)
            {
                jumpPhase = 0;
            }
            else
            {
                contactNormal = Vector3.up;
            }
        }

        private void Jump()
        {
            if (onGround || jumpPhase < maxAirJumps)
            {
                jumpPhase += 1;
                float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHight);
                float alignedSpeed = Vector3.Dot(velocity, contactNormal);
                if (velocity.y > 0)
                {
                    jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
                }
                //velocity.y += jumpSpeed;
                velocity += contactNormal * jumpSpeed;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            //onGround = true;
            EvaluateCollision(collision);
        }

        //private void OnCollisionExit(Collision collision)
        //{
        //    onGround = false;
        //}

        private void OnCollisionStay(Collision collision)
        {
            //onGround = true;
            EvaluateCollision(collision);
        }

        private void EvaluateCollision(Collision collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;
                //onGround |= normal.y >= minGroundDotProduct;
                if(normal.y >= minGroundDotProduct)
                {
                    onGround = true;
                    contactNormal = normal;
                }
            }
        }
    }
}