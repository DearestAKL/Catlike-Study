using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Base01
{
    public class Clock : MonoBehaviour
    {
        public Transform HoursArm;
        public Transform MinutesArm;
        public Transform SecondsArm;


        const float degreesPerHour = 30f;//360度/60/60/24
        const float degreesPerMinutes = 6f;//360度/60/60
        const float degreesPerSeconds = 6f;//360度/60
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            DateTime time = DateTime.Now;
            HoursArm.localRotation = Quaternion.Euler(0f, (float)time.Hour * degreesPerHour, 0f);
            MinutesArm.localRotation = Quaternion.Euler(0f, (float)time.Minute * degreesPerMinutes, 0f);
            SecondsArm.localRotation = Quaternion.Euler(0f, (float)time.Second * degreesPerSeconds, 0f);
        }
    }
}
