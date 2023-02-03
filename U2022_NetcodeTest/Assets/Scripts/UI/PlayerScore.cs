using TMPro;
using UnityEngine;

namespace UI {
    public class PlayerScore : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI scoreText;

        public void InitScore() {
            scoreText.text = "0";
        }

        public void DisplayScore(int value) {
            scoreText.text = $"{value}";
        }
    }
}