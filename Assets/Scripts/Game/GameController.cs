namespace Ludumdare43
{
    public class GameController
    {
        public delegate void _Func();

        public static event _Func OnGameStart;
        public static event _Func OnGameOver;

        static bool isGameStart;


        public static void GameStart()
        {
            if (isGameStart)
                return;

            isGameStart = true;

            if (OnGameStart != null)
                OnGameStart();
        }

        public static void GameOver()
        {
            if (!isGameStart)
                return;

            isGameStart = false;

            if (OnGameOver != null)
                OnGameOver();
        }
    }
}
