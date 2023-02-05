using System;
using managers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace player {
    public class CrossHairPlayer : NetworkBehaviour {
    
        [SerializeField] private Image crossHairImage;
        [SerializeField] private TextMeshProUGUI localScoreText;
    
        void Start() {
            Cursor.visible = false;
            ScoreManager.Singleton.NewLocalScore += OnLocalScoreChange;
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            Color aimColor = Color.black;
            switch (Convert.ToInt32(OwnerClientId)) {
                case 0:
                    aimColor = Color.red;
                    break;
                case 1:
                    aimColor = Color.green;
                    break;
                case 2:
                    aimColor = Color.blue;
                    break;
            }
            crossHairImage.color = aimColor;
            localScoreText.color = aimColor;

            if (IsOwner) {
                localScoreText.gameObject.SetActive(true);
                ScoreManager.Singleton.NewLocalScore += OnLocalScoreChange;
            }
        }

        public override void OnNetworkDespawn() {
            if (ScoreManager.Singleton != null) {
                ScoreManager.Singleton.NewLocalScore -= OnLocalScoreChange;
            }
            base.OnNetworkDespawn();
        }

        // Update is called once per frame
        void Update() {
            Rect mouseAreaRect = new Rect(0, 0, Screen.width, Screen.height);
            if (!IsOwner || !mouseAreaRect.Contains(Input.mousePosition)) return;
            crossHairImage.transform.position = Input.mousePosition;
        }
        
        private void OnLocalScoreChange(int newScore) {
            localScoreText.text = $"{newScore}";
        }
    }
}