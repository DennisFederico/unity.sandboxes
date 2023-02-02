using TMPro;
using UnityEngine;

namespace DefaultNamespace {
    public class PlayerScore : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI scoreText;
        private int _currentScore;

        public void InitScore() {
            _currentScore = 0;
            scoreText.text = $"{_currentScore}";
        }

        public void AddScore(int value) {
            _currentScore += value;
            scoreText.text = $"{_currentScore}";
        }
    }
}