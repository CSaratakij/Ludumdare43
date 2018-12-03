using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ludumdare43
{
    public class SacrificeController : MonoBehaviour
    {
        [SerializeField]
        PlayerController[] players;

        [SerializeField]
        Transform[] spawnPoint;

        [SerializeField]
        Transform[] sacrificePoint;

        [SerializeField]
        Timer roundTimer;


        public PlayerController PickPlayer { get { return players[pickPlayerIndex]; } }

        int pickPlayerIndex;
        int pickListIndex; //0 - 3, random 0 - 2 (element of 3 is a previous pick index)

        List<PlayerController> pickLists;

        bool isHasSacrifice = false;
        PlayerController sacrificePlayer;


        void Awake()
        {
            pickLists = new List<PlayerController>();

            for (int i = 0; i < players.Length; ++i)
            {
                pickLists.Add(players[i]);
            }

            SubscribeEvents();
        }

        void OnDestroy()
        {
            UnsubscribeEvents();
        }

        void SubscribeEvents()
        {
            GameController.OnGameStart += OnGameStart;
            roundTimer.OnTimerStop += roundTimer_OnTimerStop;
        }

        void UnsubscribeEvents()
        {
            GameController.OnGameStart -= OnGameStart;
            roundTimer.OnTimerStop -= roundTimer_OnTimerStop;
        }

        void OnGameStart()
        {
            StartNewRound();
        }

        void roundTimer_OnTimerStop()
        {
            CheckGameOver();
        }

        void CheckGameOver()
        {
            if (isHasSacrifice)
            {
                PlayerController spareLiveTarget = (sacrificePlayer.LastCarrier);

                if (spareLiveTarget == null) {
                    sacrificePlayer.Health.Remove(1);
                }
                else {
                    foreach (PlayerController player in players)
                    {
                        if (player.Equals(spareLiveTarget))
                            continue;

                        player.Health.Remove(1);

                        if (player.Health.IsEmpty && pickLists.Contains(player))
                            pickLists.Remove(player);
                    }
                }
            }
            else
            {
                foreach (PlayerController player in players)
                {
                    if (player.PlayerIndex == pickPlayerIndex)
                        continue;

                    player.Health.Remove(1);

                    if (player.Health.IsEmpty && pickLists.Contains(player))
                        pickLists.Remove(player);
                }
            }

            if (IsOnePlayerLeft()) {
                GameController.GameOver();
            }
            else {
                StartNewRound();
            }
        }

        void StartNewRound()
        {
            isHasSacrifice = false;
            sacrificePlayer = null;

            RandomPickTarget();
            EliminateDeathPlayer();

            roundTimer.Reset();
            roundTimer.Countdown();
        }

        void RandomPickTarget()
        {
            pickListIndex = Random.Range(0, pickLists.Count);
            pickPlayerIndex = pickLists[pickListIndex].PlayerIndex;
            PickTarget(pickPlayerIndex);
        }

        void PickTarget(int index)
        {
            foreach (PlayerController player in players)
            {
                if (player.PlayerIndex == index) {
                    player.SetTarget(true);
                }
                else {
                    player.SetTarget(false);
                }
            }
        }

        bool IsOnePlayerLeft()
        {
            int total = 0;

            foreach (PlayerController player in players)
            {
                if (!player.Health.IsEmpty) {
                    total += 1;
                }
            }

            return (total == 1);
        }

        void EliminateDeathPlayer()
        {
            foreach (PlayerController player in players)
            {
                if (player.Health.IsEmpty) {
                    player.gameObject.SetActive(false);

                    if (pickLists.Contains(player)) {
                        pickLists.Remove(player);
                    }

                    continue;
                }

                player.transform.position = spawnPoint[player.PlayerIndex].position;
                player.gameObject.SetActive(true);
                player.Stunt();
            }
        }

        public void Sacrifice(PlayerController player)
        {
            isHasSacrifice = true;
            roundTimer.Pause(true);

            sacrificePlayer = player;
            CheckGameOver();
        }

        public void MarkSpawn(int playerIndex)
        {
            if (players[playerIndex].Health.IsEmpty)
                return;

            players[playerIndex].transform.position = spawnPoint[playerIndex].position;
            players[playerIndex].gameObject.SetActive(true);
        }
    }
}
