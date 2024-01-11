using System;
using Unity.Mathematics;
using UnityEngine;

namespace Utils.Narkdagas.PathFinding.MonoTester {
    
    public class NewGridPathRequestEvent : EventArgs {
        public int2 FromGridPosition;
        public int2 ToGridPosition;
    }
    
    public class PathfindingEventMovement : MonoBehaviour {

        [SerializeField] private float speed = 10f;
        [SerializeField] private float distance = .5f;
        private Vector3[] _path;
        private int _pathIndex;

        public void SetPath(Vector3[] path) {
            _path = path;
            _pathIndex = 0;
            transform.position = path[0];
        }

        private void Update() {
            if (_path != null && _pathIndex < _path.Length) {
                var currentPosition = transform.position;
                var targetPosition = _path[_pathIndex];
                var newPosition = Vector3.MoveTowards(currentPosition, targetPosition, speed * Time.deltaTime);
                transform.position = newPosition;
                if (Vector3.Distance(newPosition, targetPosition) < distance) {
                    _pathIndex++;
                }
            }
            else {
                StopMoving();
            }
        }

        private void StopMoving() {
            _path = null;
        }
    }
}