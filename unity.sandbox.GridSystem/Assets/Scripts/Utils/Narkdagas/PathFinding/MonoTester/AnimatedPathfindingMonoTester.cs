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
        [SerializeField] private PathfindingMovement prefab;

        private Camera _camera;
        private Mesh _mesh;
        private GenericSimpleGrid<PathNode> _grid;
        private GenericSimpleGridVisual<PathNode> _gridVisual;
        private PathfindingMovement _player;

        private void Start() {
            _camera = Camera.main;
            _mesh = new Mesh();
            GetComponent<MeshRenderer>().material = gradientMaterial;
            GetComponent<MeshFilter>().mesh = _mesh;

            var originOffset = transform.position;
            _grid = new GenericSimpleGrid<PathNode> (originOffset, width, height, cellSize,
                (index, gridPos) => new PathNode {
                    Index = index,
                    XY = gridPos,
                    IsWalkable = true
                });
            _gridVisual = new GenericSimpleGridVisual<PathNode>(_grid, _mesh, (node) => {
                if (!node.IsWalkable) return 0f;
                if (node.ParentIndex != -1) return .5f;
                return 0.25f;
            }, originOffset);

            _grid.PaintDebugGrid();
        }

        private Vector3 _startDragPosition;
        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                _startDragPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            }
            
            if (Input.GetMouseButtonUp(0)) {
                var endDragPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                
                if (_grid.TryGetXY(_startDragPosition, out var startPos) && _grid.TryGetXY(endDragPosition, out var endPos)) {
                    
                    if (math.distance(startPos, endPos) < 1) {
                        startPos = int2.zero;
                    }
                    
                    if (new Pathfinding<PathNode>().TryFindPath(startPos, endPos, new int2(width, height), _grid, out var path)) {
                        var path3 = TransformPath(path);
                        if (!_player) {
                            _player = Instantiate(prefab);
                        }
                        _player.SetPath(path3);
                        DebugPath(path);
                    }
                }
            }
            
            if (Input.GetMouseButtonDown(1)) {
                if (!_grid.TryGetXY(_camera.ScreenToWorldPoint(Input.mousePosition), out var x, out var y)) return;
                var node = _grid.GetGridObject(x, y);
                node.IsWalkable = !node.IsWalkable;
                _grid.SetGridObject(x, y, node);
            }
        }

        private Vector3[] TransformPath(int2[] path) {
            var path3 = new Vector3[path.Length];
            int index = 0;
            foreach (var step in path) {
                path3[index++] = _grid.GetWorldPosition(step.x , step.y);
            }
            return path3;
        }

        private void LateUpdate() {
            _gridVisual.LateUpdateVisual();
        }

        private void DebugPath(int2[] path) {
            for (int i = 0; i < path.Length - 1; i++) {
                Debug.DrawLine(
                    _grid.GetWorldPosition(path[i].x, path[i].y),
                    _grid.GetWorldPosition(path[i+1].x, path[i+1].y),
                    Color.red, 15f
                );
            }
        }
    }
}