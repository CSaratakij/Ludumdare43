using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ludumdare43
{
    public class SacrificePointController : MonoBehaviour
    {
        [SerializeField]
        SacrificeController sacrificeController;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) {

                PlayerController player = other.GetComponent<PlayerController>();
                player.gameObject.SetActive(false);

                if (player.IsTarget) {
                    sacrificeController.Sacrifice(player);
                }

                sacrificeController.MarkSpawn(player.PlayerIndex);

                player.ClearThrow();
                player.UnPickOldOne();

                player.Stunt();
            }
        }
    }
}
