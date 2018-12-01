using UnityEngine;

namespace Ludumdare43
{
    public class UILogic : MonoBehaviour
    {
        public void GameStart()
        {
            GameController.GameStart();
        }
        
        public void GameOver()
        {
            GameController.GameOver();
        }
    }
}
