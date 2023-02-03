using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace player {
    public class CrossHairPlayer : NetworkBehaviour {
    
        [SerializeField] private Image crossHairImage;
    
        void Start() {
            Cursor.visible = false;
        }

        // Update is called once per frame
        void Update() {
            Rect mouseAreaRect = new Rect(0, 0, Screen.width, Screen.height);
            if (!IsOwner || !mouseAreaRect.Contains(Input.mousePosition)) return;
            crossHairImage.transform.position = Input.mousePosition;
        }
    }
}