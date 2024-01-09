using UnityEngine;

namespace Utils.Narkdagas.GridSystem {
    [RequireComponent(typeof(HeatMapVisual))]
    public class HeatMapMonoTester : MonoBehaviour {
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private float cellSize;
        [SerializeField] private bool debugEnabled;

        private Camera _camera;
        private HeatMapGrid _grid;

        private void Start() {
            _camera = Camera.main;
            _grid = new HeatMapGrid(transform.localPosition, width, height, cellSize, debugEnabled);
            GetComponent<HeatMapVisual>().SetGrid(_grid);
            if (debugEnabled) _grid.DebugGrid();
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                var value = _grid.GetValue(worldPosition);
                _grid.SetValue(worldPosition, value + 5);
            }

            if (Input.GetMouseButtonDown(1)) {
                Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                _grid.AddFallOffValue(worldPosition, 30, 7, 2);
            }

            if (Input.GetMouseButtonDown(2)) {
                Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                _grid.AddValue(worldPosition, 5, 5);
            }
        }
    }
}