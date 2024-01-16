using Unity.Mathematics;
using UnityEngine;
using Utils.Narkdagas.GridSystem;

namespace Utils.Narkdagas.PathFinding.MonoTester {
    public class PathfindingMonoTester : MonoBehaviour {

        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private float cellSize;
        [SerializeField] private bool debugEnabled;
        [SerializeField] private Material gradientMaterial;

        private Camera _camera;
        private Mesh _mesh;
        private GenericSimpleGrid<PathNode> _grid;
        private GenericSimpleGridVisual<PathNode> _gridVisual;

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

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                if (_grid.TryGetXY(_camera.ScreenToWorldPoint(Input.mousePosition), out var x, out var y)) {
                    Debug.Log("Mouse position: " + x + ", " + y);
                    new Pathfinding<PathNode>().TryFindPath(int2.zero, new int2(x, y), new int2(width, height), _grid, out var path);
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