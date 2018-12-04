using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ludumdare43
{
    public class UIGameOver : MonoBehaviour
    {
        const string TEXT_FORMAT = "P{0} : Win";

        [SerializeField]
        Text txtWinner;

        [SerializeField]
        Image imgWinner;

        [SerializeField]
        PlayerController[] players;


        void Update()
        {
            UpdateUI();
        }

        void UpdateUI()
        {
            if (SacrificeController.Winner != null) {
                txtWinner.text = string.Format(TEXT_FORMAT, SacrificeController.Winner.PlayerIndex + 1);
                imgWinner.color = SacrificeController.Winner.Color;
            }

        }
    }
}
