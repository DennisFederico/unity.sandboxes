using System;
using UnityEngine;

namespace narkdagas.gameobjects.controllers {

    [RequireComponent(typeof(Animator))]
    public class PlayerLookAt : MonoBehaviour {
        
        // [SerializeField] private Camera mainCamera;
        [SerializeField] private GameObject lookAtTarget;
        private Animator _animator;
        
        private void Start() {
            // mainCamera ??= Camera.main;
            _animator = GetComponent<Animator>();
        }

        private void OnAnimatorIK(int layerIndex) {
            _animator.SetLookAtWeight(1f, .5f, 1f, 1f, .5f);
            _animator.SetLookAtPosition(lookAtTarget.transform.position);
        }
    }
}