using UnityEngine;

namespace Ludumdare43
{
    public class Status : MonoBehaviour, IStatus<int>
    {
        [SerializeField]
        int current;

        [SerializeField]
        int max;


        public int Current { get { return current; } }
        public int Max { get { return max; } }


        public void Clear()
        {
            current = 0;
        }

        public void FullRestore()
        {
            current = max;
        }

        public void Remove(int value)
        {
            current = ((current - value) < 0) ? 0 : (current - value);
        }

        public void Restore(int value)
        {
            current = ((current + value) > max) ? max : (current + value);
        }
    }
}
