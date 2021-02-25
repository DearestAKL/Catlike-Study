using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Movement07
{
    public class MovingSphere07 : MonoBehaviour
    {

        private Vector2 playerInput;  //用户输入
        private Vector3 velocity;       //缓存速度
        private Vector3 desiredVelocity;    //目标速度
        private Vector3 connectionVelocity;    //当前连接体的速度

        private Vector3 connectionWorldPosition;//连接体的坐标

        [SerializeField,Range(0f,100f), Header("最大速度")]
        private float maxSpeed = 10f;

        [SerializeField, Range(0f, 100f),Header("最大加速度")]
        private float maxAcceleration = 10f;

        [SerializeField, Range(0f, 100f), Header("最大空中加速度")]
        private float maxAirAcceleration = 1f;

        [SerializeField, Range(0f, 10f), Header("跳跃高度")]
        private float jumpHight = 2f;

        [SerializeField, Range(0, 5), Header("空中跳跃次数")]
        private int maxAirJumps = 0;

        [SerializeField, Range(0F, 90F), Header("最大斜坡角度")]
        private float maxGroundAngle = 25f;

        [SerializeField, Range(0F, 90F), Header("最大楼梯角度")]
        private float maxStairsAngle = 50f;

        [SerializeField, Range(0F, 100F), Header("最大捕捉速度")]
        private float maxSnapSpeed = 100f;

        [SerializeField,Min(0F), Header("探测距离")]
        private float probeDistance = 1f;

        [SerializeField, Header("探测Layer")]
        private LayerMask probeMask = -1;

        [SerializeField, Header("楼梯Layer")]
        private LayerMask stairsMask = -1;

        [SerializeField]
        Transform playerInputSpace = default;

        private Rigidbody body;
        private Rigidbody connectedBody;//连接的rigidbody
        private Rigidbody previousConnectedBody;//当前连接的rigidbody

        private bool desiredJump;   //想要跳跃
        private int jumpPhase;  //跳跃阶段
        private float minGroundDotProduct;  //地面点积
        private float minStairsDotProduct;  //楼梯点积
        private int groundContactCount; //地面接触数
        private int steepContactCount;  //楼梯接触数
        private int stepsSinceLastGrounded; //自上次接地的步数
        private int stepsSinceLastJump; //自上次跳跃的步数

        bool OnGround => groundContactCount > 0;
        bool OnSteep => steepContactCount > 0;

        // 法线
        Vector3 contactNormal, steepNormal;

        Vector3 upAxis, rightAxis, forwardAxis;


        private void OnValidate()
        {
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            minStairsDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            OnValidate();
        }

        void Update()
        {
            playerInput.x = Input.GetAxis("Horizontal");
            playerInput.y = Input.GetAxis("Vertical");
            playerInput = Vector2.ClampMagnitude(playerInput, 1f);

            if (playerInputSpace)
            {
                rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
                forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);


                //Vector3 forward = playerInputSpace.forward;
                //forward.y = 0f;
                //forward.Normalize();
                //Vector3 right = playerInputSpace.right;
                //right.y = 0f;
                //right.Normalize();
                //desiredVelocity = (forward * playerInput.y + right * playerInput.x) * maxSpeed;
            }
            else
            {
                rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
                forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
            }

            desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

            desiredJump |= Input.GetButtonDown("Jump");

            GetComponent<Renderer>().material.SetColor("_Color", OnGround ? Color.black : Color.white);
        }

        private void FixedUpdate()
        {
            //upAxis = -Physics.gravity.normalized;
            Vector3 gravity = CustomGravity07.GetGravity(body.position, out upAxis);

            UpdateState();
            AdjustVelocity();

            if (desiredJump)
            {
                desiredJump = false;
                Jump(gravity);
            }

            velocity += gravity * Time.deltaTime;

            body.velocity = velocity;

            ClearState();
        }

        /// <summary>
        /// 更新状态
        /// </summary>
        private void UpdateState()
        {
            stepsSinceLastGrounded += 1;
            stepsSinceLastJump += 1;
            velocity = body.velocity;
            if (OnGround || SnapToGround()|| CheckSteepContacts())
            {
                stepsSinceLastGrounded = 0;
                if (stepsSinceLastJump > 1)
                {
                    jumpPhase = 0;
                }
                if (groundContactCount > 1)
                {
                    contactNormal.Normalize();
                }
            }
            else
            {
                contactNormal = upAxis;
            }

            if (connectedBody)
            {
                if (connectedBody.isKinematic || connectedBody.mass >= body.mass)
                {
                    UpdateConnectionState();
                }
            }
        }

        /// <summary>
        /// 更新连接体的状态
        /// </summary>
        private void UpdateConnectionState()
        {
            Vector3 connectionMovement = connectedBody.position - connectionWorldPosition;
            connectionVelocity = connectionMovement / Time.deltaTime;

            connectionWorldPosition = connectedBody.position;
        }

        /// <summary>
        /// 清空 状态
        /// </summary>
        private void ClearState()
        {
            groundContactCount = steepContactCount = 0;
            contactNormal = steepNormal = connectionVelocity  = Vector3.zero;

            previousConnectedBody = connectedBody;
            connectedBody = null;

        }


        /// <summary>
        /// 跳跃
        /// </summary>
        private void Jump(Vector3 gravity)
        {
            Vector3 jumpDirection;
            if (OnGround)
            {
                jumpDirection = contactNormal;
            }
            else if (OnSteep)
            {
                jumpDirection = steepNormal;
                jumpPhase = 0;
            }
            else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
            {
                if (jumpPhase == 0)
                {
                    jumpPhase = 1;
                }
                jumpDirection = contactNormal;
            }
            else
            {
                return;
            }

            jumpDirection = (jumpDirection + upAxis).normalized;

            jumpPhase += 1;
            stepsSinceLastJump = 0;
            float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHight);
            float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
            if (velocity.y > 0)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }
            velocity += jumpDirection * jumpSpeed;
        }

        private void OnCollisionEnter(Collision collision)
        {
            EvaluateCollision(collision);
        }


        private void OnCollisionStay(Collision collision)
        {
            EvaluateCollision(collision);
        }

        /// <summary>
        /// 碰撞检测
        /// </summary>
        /// <param name="collision"></param>
        private void EvaluateCollision(Collision collision)
        {

            float minDot = GetMinDot(collision.gameObject.layer);

            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;
                float upDot = Vector3.Dot(upAxis, normal);
                if(upDot >= minDot)
                {
                    //onGround = true;
                    groundContactCount += 1;
                    //累积法线
                    contactNormal += normal;

                    connectedBody = collision.rigidbody;
                }
                else if(upDot > -0.01f)
                {
                    //陡峭
                    steepContactCount += 1;
                    steepNormal += normal;

                    if(groundContactCount == 0)
                    {
                        connectedBody = collision.rigidbody;
                    }
                }
            }
        }

        /// <summary>
        /// 调整速度
        /// </summary>
        /// <returns></returns>
        private void AdjustVelocity()
        {
            //获取映射的 相对斜边的 X 和 Z轴
            //相对于斜边
            Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
            Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

            //求在在世界坐标中X轴和Z轴投影长度 也就是相对的X和Z的速度
            //相对于地面
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

        /// <summary>
        /// 得到最小点积
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        private float GetMinDot(int layer)
        {
            //return stairsMask != layer ? minGroundDotProduct : minStairsDotProduct;
            return (stairsMask & (1 << layer)) == 0 ? minGroundDotProduct : minStairsDotProduct;
        }

        ///// <summary>
        ///// 求沿斜面 斜边 方向速度
        ///// </summary>
        ///// <param name="vector"></param>
        ///// <returns></returns>
        //private Vector3 ProjectOnContactPlane(Vector3 vector)
        //{
        //    //v - n*(|v|*cos角) 求斜边方向向量
        //    return vector - contactNormal * Vector3.Dot(vector, contactNormal);
        //}

        /// <summary>
        /// ss
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        private Vector3 ProjectDirectionOnPlane(Vector3 direction,Vector3 normal)
        {
            return (direction - normal * Vector3.Dot(direction, normal)).normalized;
        }

        /// <summary>
        /// 捕捉到地面
        /// </summary>
        /// <returns></returns>
        private bool SnapToGround()
        {
            if (stepsSinceLastGrounded > 1 || stepsSinceLastGrounded <= 2)
            {
                return false;
            }
            float speed = velocity.magnitude;
            if(speed > maxSnapSpeed)
            {
                return false;
            }

            if (!Physics.Raycast(body.position, -upAxis,out RaycastHit hit, probeDistance, probeMask))
            {
                return false;
            }

            float upDot = Vector3.Dot(upAxis, hit.normal);
            if(upDot < GetMinDot(hit.collider.gameObject.layer))
            {
                return false;
            }

            groundContactCount = 1;
            contactNormal = hit.normal;
            //float speed = velocity.magnitude;
            float dot = Vector3.Dot(velocity, hit.normal);
            if (dot > 0)
            {
                velocity = (velocity - hit.normal * dot).normalized * speed;
            }

            connectedBody = hit.rigidbody;
            return true;
        }

        /// <summary>
        /// 检测是否陡峭
        /// </summary>
        /// <returns></returns>
        private bool CheckSteepContacts()
        {
            if(steepContactCount > 1)
            {
                steepNormal.Normalize();
                if(steepNormal.y >= minGroundDotProduct)
                {
                    groundContactCount = 1;
                    contactNormal = steepNormal;
                    return true;
                }
            }

            return false;
        }
    }
}