using Unity.Mathematics;
using UnityEngine;

namespace Utils.Narkdagas.GridSystem {
    public class GenericSimpleGridHeatMapMonoTester : MonoBehaviour {

        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private float cellSize;
        [SerializeField] private bool debugEnabled;

        private Camera _camera;
        private Mesh _mesh;
        private GenericSimpleGrid<int> _grid;
        private GenericSimpleGridVisual<int> _gridVisual;

        private void Awake() {
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
        }

        void Start() {
            _camera = Camera.main;
            _grid = new GenericSimpleGrid<int>(transform.position, width, height, cellSize, (_, _) => 0, debugEnabled);
            _gridVisual = new GenericSimpleGridVisual<int>(_grid, _mesh, (value) => value / 100f);

            if (debugEnabled) _grid.DebugGrid();
        }

        void Update() {
            if (Input.GetMouseButtonDown(0)) {
                
                Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                _grid.SetGridObject(worldPosition, math.clamp(_grid.GetGridObject(worldPosition) + 5, 0, 100));
            }

            if (Input.GetMouseButtonDown(1)) {
                Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                _grid.SetGridObject(worldPosition, math.clamp(_grid.GetGridObject(worldPosition) - 5, 0, 100));
            }
        }

        private void LateUpdate() {
            _gridVisual.LateUpdateVisual();
        }
    }
}