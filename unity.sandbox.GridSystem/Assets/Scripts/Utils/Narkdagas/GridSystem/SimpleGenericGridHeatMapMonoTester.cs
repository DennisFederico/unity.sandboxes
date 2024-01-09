using UnityEngine;

namespace Utils.Narkdagas.GridSystem {
    public class SimpleGenericGridHeatMapMonoTester : MonoBehaviour {

        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private float cellSize;
        [SerializeField] private bool debugEnabled;

        private Camera _camera;
        private Mesh _mesh;
        private SimpleGenericGrid<SimpleGridHeatMapObject, int> _grid;
        private SimpleGenericGridVisual<SimpleGridHeatMapObject, int> _gridVisual;

        private void Awake() {
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
        }

        void Start() {
            _camera = Camera.main;
            _grid = new SimpleGenericGrid<SimpleGridHeatMapObject, int>(transform.position, width, height, cellSize,
                (_, _) => new SimpleGridHeatMapObject(0),
                SimpleGridHeatMapObject.SetValue,
                SimpleGridHeatMapObject.AddValue,
                debugEnabled);
            _gridVisual = new SimpleGenericGridVisual<SimpleGridHeatMapObject, int>(_grid, _mesh, SimpleGridHeatMapObject.GetNormalizedValue);

            if (debugEnabled) _grid.DebugGrid();
        }

        void Update() {
            if (Input.GetMouseButtonDown(0)) {
                Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                _grid.AddGridObjectValue(worldPosition, 5);
            }

            if (Input.GetMouseButtonDown(1)) {
                Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                _grid.AddGridObjectValue(worldPosition, -5);
            }
        }

        private void LateUpdate() {
            _gridVisual.LateUpdateVisual();
        }

        private class SimpleGridHeatMapObject {
            private const int MinValue = 0;
            private const int MaxValue = 100;
            private int _value;

            public SimpleGridHeatMapObject(int value) {
                SetValue(this, value);
            }

            public static void SetValue(SimpleGridHeatMapObject heatMapObject, int value) {
                heatMapObject._value = Mathf.Clamp(value, MinValue, MaxValue);
            }

            public static void AddValue(SimpleGridHeatMapObject heatMapObject, int value) {
                SetValue(heatMapObject, heatMapObject._value + value);
            }

            public static float GetNormalizedValue(SimpleGridHeatMapObject heatMapObject) {
                return (float)heatMapObject._value / MaxValue;
            }

            public override string ToString() {
                return _value.ToString();
            }
        }
    }
}