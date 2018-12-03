using UnityEngine;
using UnityEngine.UI;

namespace Ludumdare43
{
    public class UIRoundInfo : MonoBehaviour
    {
        [SerializeField]
        Text txtTime;

        [SerializeField]
        Image imgTargetColor;

        [SerializeField]
        Timer roundTimer;

        [SerializeField]
        SacrificeController sacrificeController;


        void Update()
        {
            txtTime.text = "Time : " + (int)roundTimer.Current;
            imgTargetColor.color = sacrificeController.PickPlayer.Color;
        }
    }
}
