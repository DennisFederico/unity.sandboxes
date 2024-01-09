using System;
using Unity.Mathematics;
using UnityEngine;
using Utils.Narkdagas.GridSystem;

namespace Utils.Narkdagas.PathFinding {
    public class PathfindingMonoTester : MonoBehaviour {

        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private float cellSize;
        [SerializeField] private bool debugEnabled;
        [SerializeField] private Material gradientMaterial;

        private Camera _camera;
        private Mesh _mesh;
        private GenericSimpleGrid<IPathNode> _grid;
        private GenericSimpleGridVisual<IPathNode> _gridVisual;

        private void Start() {
            _camera = Camera.main;
            _mesh = new Mesh();
            GetComponent<MeshRenderer>().material = gradientMaterial;
            GetComponent<MeshFilter>().mesh = _mesh;
            
            _grid = new GenericSimpleGrid<IPathNode> (transform.position, width, height, cellSize,
                (index, gridPos) => new PathNode {
                    Index = index,
                    GridPosition = gridPos,
                    IsWalkable = true
                },
                true);
            _gridVisual = new GenericSimpleGridVisual<IPathNode>(_grid, _mesh, (node) => {
                if (!node.IsWalkable) return 0f;
                if (node.ParentIndex != -1) return .5f;
                return 0.25f;
            });

            if (debugEnabled) _grid.DebugGrid();
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                if (_grid.TryGetXY(_camera.ScreenToWorldPoint(Input.mousePosition), out var x, out var y)) {
                    Debug.Log("Mouse position: " + x + ", " + y);
                    new Pathfinding().TryFindPath(int2.zero, new int2(x, y), new int2(width, height), _grid, out var path);
                    DebugPath(path);
                    Debug.Log($"Path: {string.Join(", ", path)}");
                }
            }
            if (Input.GetMouseButtonDown(1)) {
                if (_grid.TryGetXY(_camera.ScreenToWorldPoint(Input.mousePosition), out var x, out var y)) {
                    Debug.Log("Mouse position: " + x + ", " + y);
                    var node = _grid.GetGridObject(x, y);
                    node.IsWalkable = !node.IsWalkable;
                    _grid.SetGridObject(x, y, node);
                }
            }
        }

        private void LateUpdate() {
            _gridVisual.LateUpdateVisual();
        }

        private void DebugPath(int2[] path) {
            var offset = new Vector3(cellSize/2, cellSize/2, 0);
            for (int i = 0; i < path.Length - 1; i++) {
                Debug.DrawLine(
                    _grid.GetWorldPosition(path[i].x, path[i].y) + offset,
                    _grid.GetWorldPosition(path[i+1].x, path[i+1].y) + offset,
                    Color.red, 100f
                );
            }
        }
    }
}