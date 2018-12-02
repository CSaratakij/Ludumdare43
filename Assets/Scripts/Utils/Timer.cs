using UnityEngine;

namespace Ludumdare43
{
    public class Timer : MonoBehaviour
    {
        [SerializeField]
        float current;

        [SerializeField]
        float max;


        public delegate void _Func();

        public event _Func OnTimerStart;
        public event _Func OnTimerStop;

        bool isStart;
        bool isPause;


        void Update()
        {
            TickHandler();
        }

        void TickHandler()
        {
            if (!isStart || isPause)
                return;

            current -= 1.0f * Time.deltaTime;

            if (current <= 0.0f) {
                Stop();
            }
        }

        void FireEvent_OnTimerStart()
        {
            if (OnTimerStart != null)
                OnTimerStart();
        }

        void FireEvent_OnTimerStop()
        {
            if (OnTimerStop != null)
                OnTimerStop();
        }

        public void Countdown()
        {
            if (isStart)
                return;

            Reset();
            isStart = true;

            FireEvent_OnTimerStart();
        }

        public void Stop()
        {
            if (!isStart)
                return;

            isStart = false;
            FireEvent_OnTimerStop();
        }

        public void Reset()
        {
            current = max;
        }

        public void Pause(bool value)
        {
            isPause = value;
        }
    }
}
