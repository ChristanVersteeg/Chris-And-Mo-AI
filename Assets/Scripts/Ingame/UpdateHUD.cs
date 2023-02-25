using UnityEngine;
using UnityEngine.UI;

namespace InGame
{
    public class UpdateHUD : MonoBehaviour
    {
        private Player player;

        public Text daggerText;
        public Text keyText;
        public Text infoText;

        private void DisableStartupText() => infoText.enabled = false;

        private void Start()
        {
            Invoke(nameof(DisableStartupText), 3);
        }

        void Update()
        {
            if (player == null)
            {
                infoText.enabled = true;
                player = FindObjectOfType<Player>();
            }
            else
            {
                daggerText.text = $"x {player.daggers}";
                keyText.text = $"x {player.keys}";
            }
        }
    }
}