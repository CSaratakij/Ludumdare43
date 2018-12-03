using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ludumdare43
{
    public class UIHealth : MonoBehaviour
    {
        const string TEXT_FORMAT = "P{0} : ({1})";

        [SerializeField]
        Text txtHealth;

        [SerializeField]
        Image imgColor;

        [SerializeField]
        PlayerController player;


        void Update()
        {
            if (player.Health.IsEmpty) {
                gameObject.SetActive(false);
            }

            txtHealth.text = string.Format(TEXT_FORMAT, player.PlayerIndex + 1, player.Health.Current);
            imgColor.color = player.Color;
        }
    }
}
