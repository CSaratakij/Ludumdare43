using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ludumdare43
{
    public class GameMenuController : MonoBehaviour
    {
        enum View
        {
            MainMenu,
            InGameMenu,
            Pause,
            GameOver
        }

        [SerializeField]
        RectTransform[] views;

        [SerializeField]
        UILogic uiLogic;


        bool isPause;


        void Awake()
        {
            GameController.OnGameStart += OnGameStart;
            GameController.OnGameOver += OnGameOver;
        }

        void OnDestroy()
        {
            GameController.OnGameStart -= OnGameStart;
            GameController.OnGameOver -= OnGameOver;
        }

        void Start()
        {
            HideAll();
            Show((int)View.MainMenu, true);
        }

        void Update()
        {
            UIHandler();
        }

        void UIHandler()
        {
            if (!GameController.IsGameStart)
                return;

            if (Input.GetButtonDown("Cancel")) {
                HideAll();
                Show((int)View.Pause, true);
                uiLogic.TogglePause();
            }
        }

        void OnGameStart()
        {
            HideAll();
            Show((int)View.InGameMenu, true);
        }

        void OnGameOver()
        {
            HideAll();
            Show((int)View.GameOver, true);
        }

        public void Show(int id, bool value)
        {
            HideAll();
            views[id].gameObject.SetActive(value);
        }

        public void HideAll()
        {
            foreach (RectTransform rect in views)
            {
                rect.gameObject.SetActive(false);
            }
        }
    }
}
