using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ludumdare43
{
    public class UILogic : MonoBehaviour
    {
        bool isPause;

        public bool IsPause { get { return isPause; } }


        public void GameStart()
        {
            GameController.GameStart();
        }
        
        public void GameOver()
        {
            GameController.GameOver();
        }

        public void ExitGame()
        {
            Application.Quit();
        }

        public void Pause(bool value)
        {
            isPause = value;
            Time.timeScale = value ? 0.0f : 1.0f;
        }

        public void TogglePause()
        {
            isPause = !isPause;
            Pause(isPause);
        }

        public void ResetGameState()
        {
            GameController.Reset();
        }

        public void Restart()
        {
            //Hacks
            Time.timeScale = 1.0f;

            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
