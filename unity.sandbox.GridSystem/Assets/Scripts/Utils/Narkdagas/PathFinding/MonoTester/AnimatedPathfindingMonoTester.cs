using Unity.Mathematics;
using UnityEngine;
using Utils.Narkdagas.GridSystem;

namespace Utils.Narkdagas.PathFinding.MonoTester {
    public class AnimatedPathfindingMonoTester : MonoBehaviour {
        
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private float cellSize;
        [SerializeField] private bool debugEnabled;
        [SerializeField] private Material gradientMaterial;
        [SerializeField] private PathfindingMovement player;

        private Camera _camera;
        private Mesh _mesh;
        private GenericSimpleGrid<PathNode> _grid;
        private GenericSimpleGridVisual<PathNode> _gridVisual;

        private void Start() {
            _camera = Camera.main;
            _mesh = new Mesh();
            GetComponent<MeshRenderer>().material = gradientMaterial;
            GetComponent<MeshFilter>().mesh = _mesh;
            
            _grid = new GenericSimpleGrid<PathNode> (transform.position, width, height, cellSize,
                (index, gridPos) => new PathNode {
                    Index = index,
                    GridPosition = gridPos,
                    IsWalkable = true
                },
                debugEnabled);
            _gridVisual = new GenericSimpleGridVisual<PathNode>(_grid, _mesh, (node) => {
                if (!node.IsWalkable) return 0f;
                if (node.ParentIndex != -1) return .5f;
                return 0.25f;
            });

            _grid.PaintDebugGrid();
        }

        private Vector3 _startDragPosition;
        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                _startDragPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                Debug.Log($"Down - {_startDragPosition}");
            }
            
            if (Input.GetMouseButtonUp(0)) {
                var endDragPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                Debug.Log($"Up - {endDragPosition}");
                
                if (_grid.TryGetXY(_startDragPosition, out var startPos) && _grid.TryGetXY(endDragPosition, out var endPos)) {
                    Debug.Log($"Path from {startPos} to {endPos}");
                    
                    if (math.distance(startPos, endPos) < 1) {
                        startPos = int2.zero;
                    }
                    
                    if (new Pathfinding<PathNode>().TryFindPath(startPos, endPos, new int2(width, height), _grid, out var path)) {
                        var path3 = TransformPath(path, cellSize);
                        player.SetPath(path3);
                        DebugPath(path);
                        Debug.Log($"Found Path: {string.Join(", ", path)}");
                    }
                }
            }
            
            if (Input.GetMouseButtonDown(1)) {
                if (!_grid.TryGetXY(_camera.ScreenToWorldPoint(Input.mousePosition), out var x, out var y)) return;
                Debug.Log("Mouse position: " + x + ", " + y);
                var node = _grid.GetGridObject(x, y);
                node.IsWalkable = !node.IsWalkable;
                _grid.SetGridObject(x, y, node);
            }
        }

        private Vector3[] TransformPath(int2[] path, float gridCellSize) {
            var path3 = new Vector3[path.Length];
            int index = 0;
            for (int i=path.Length-1; i>=0; i--) {
                path3[index++] = _grid.GetWorldPosition(path[i].x , path[i].y) + new Vector3(gridCellSize/2, gridCellSize/2, 0);
            }
            return path3;
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
                    Color.red, 15f
                );
            }
        }
    }
}