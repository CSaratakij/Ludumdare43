using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ludumdare43
{
    public class SacrificeController : MonoBehaviour
    {
        //Hacks
        public static Vector3 PickupTargetPos = Vector3.zero;

        //Hacks
        public static PlayerController Winner = null;


        [SerializeField]
        PlayerController[] players;

        [SerializeField]
        Transform[] spawnPoint;

        [SerializeField]
        Transform[] sacrificeGates;

        [SerializeField]
        Transform[] sacrificeObjectPoints;

        [SerializeField]
        Timer roundTimer;


        public PlayerController PickPlayer { get { return players[pickPlayerIndex]; } }

        int pickPlayerIndex;
        int pickListIndex;

        int oldSacrificePointIndex;

        List<PlayerController> pickLists;

        bool isHasSacrifice = false;
        bool isTargetHasBeenCarry = false;
        bool isOpenGateAtleastOne = false;

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

        void Start()
        {
            CloseAllGates();
            CloseAllSacrificePoints();
        }

        void Update()
        {
            isTargetHasBeenCarry = players[pickPlayerIndex].IsPickedUp || players[pickPlayerIndex].IsHasBeenThrowed;

            if (isTargetHasBeenCarry) {
                if (!isOpenGateAtleastOne) {
                    OpenAClosetGate(PickupTargetPos);
                    isOpenGateAtleastOne = true;
                }
            }
            else {
                if (isOpenGateAtleastOne) {
                    CloseAllGates();
                    isOpenGateAtleastOne = false;
                }
            }
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

        void CloseAllGates()
        {
            for (int i = 0; i < sacrificeGates.Length; ++i)
            {
                sacrificeGates[i].gameObject.SetActive(true);
            }
        }

        void CloseAllSacrificePoints()
        {
            for (int i = 0; i < sacrificeObjectPoints.Length; ++i)
            {
                sacrificeObjectPoints[i].gameObject.SetActive(false);
            }
        }

        void OpenAClosetGate(Vector3 pickupTargetPos)
        {
            int pickGateIndex = 0;
            float distance = Vector3.Distance(pickupTargetPos, sacrificeGates[0].position);

            for (int i = 1; i < sacrificeGates.Length; ++i)
            {
                float temp = Vector3.Distance(pickupTargetPos, sacrificeGates[i].position);

                if (temp > distance) {
                    distance = temp;
                    pickGateIndex = i;
                }
            }

            sacrificeGates[pickGateIndex].gameObject.SetActive(false);
            sacrificeObjectPoints[pickGateIndex].gameObject.SetActive(true);

            if (oldSacrificePointIndex != pickGateIndex) {
                sacrificeObjectPoints[oldSacrificePointIndex].gameObject.SetActive(false);
                oldSacrificePointIndex = pickGateIndex;
            }
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

                foreach (PlayerController player in players)
                {
                    if (player.Health.IsEmpty)
                        continue;

                    Winner = player;
                }

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

            ResetLastCarrier();
            ResetCarrySomeone();

            isTargetHasBeenCarry = false;
            isOpenGateAtleastOne = false;

            CloseAllGates();

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

        void ResetLastCarrier()
        {
            foreach (PlayerController player in players)
            {
                player.SetLastCarrier(null);
            }
        }

        void ResetCarrySomeone()
        {
            foreach (PlayerController player in players)
            {
                player.UnPickOldOne();
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
