using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Movement06
{
    [RequireComponent(typeof(Rigidbody))]
    public class CustomGravityRigidbody06 : MonoBehaviour
    {
        Rigidbody body;

        float floatDelay;

        [SerializeField]
        bool floatToSleep = false;

        private Material material;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;

            material = GetComponent<Renderer>().material;
        }

        private void FixedUpdate()
        {
            if (floatToSleep)
            {
                if (body.IsSleeping())
                {
                    floatDelay = 0f;
                    material.SetColor("_Color", Color.gray);
                    return;
                }

                if (body.velocity.sqrMagnitude < 0.0001f)
                {
                    floatDelay += Time.fixedDeltaTime;
                    material.SetColor("_Color", Color.yellow);
                    if (floatDelay >= 1f)
                        return;
                }
                else
                {
                    floatDelay = 0f;
                    material.SetColor("_Color", Color.red);
                }
            }

            body.AddForce(CustomGravity06.GetGravity(body.position), ForceMode.Acceleration);
        }
    }
}
