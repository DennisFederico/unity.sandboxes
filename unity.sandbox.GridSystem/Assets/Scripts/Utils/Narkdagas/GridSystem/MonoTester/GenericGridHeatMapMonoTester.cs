using UnityEngine;

namespace Utils.Narkdagas.GridSystem.MonoTester {
    public class GenericGridHeatMapMonoTester : MonoBehaviour {

        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private float cellSize;
        [SerializeField] private bool debugEnabled;

        private Camera _camera;
        private Mesh _mesh;
        private GenericGrid<HeatMapValueObject, int> _grid;
        private GenericGridVisual<HeatMapValueObject, int> _gridVisual;

        private void Awake() {
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
        }

        void Start() {
            _camera = Camera.main;
            _grid = new GenericGrid<HeatMapValueObject, int>(transform.position, width, height, cellSize, () => new HeatMapValueObject(0), debugEnabled);
            _gridVisual = new GenericGridVisual<HeatMapValueObject, int>(_grid, _mesh);

            if (debugEnabled) _grid.DebugGrid();
        }

        void Update() {
            if (Input.GetMouseButtonDown(0)) {
                Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                _grid.AddGridObject(worldPosition, 5);
            }

            if (Input.GetMouseButtonDown(1)) {
                Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                _grid.AddGridObject(worldPosition, -5);
            }
        }

        private void LateUpdate() {
            _gridVisual.LateUpdateVisual();
        }

        private class HeatMapValueObject : IGridObject<int> {
            private const int MinValue = 0;
            private const int MaxValue = 100;
            private int _value;

            public HeatMapValueObject(int value) {
                SetValue(value);
            }

            public void SetValue(int value) {
                _value = Mathf.Clamp(value, MinValue, MaxValue);
            }

            public void AddValue(int value) {
                SetValue(_value + value);
            }

            public float GetNormalizedValue() {
                return (float)_value / MaxValue;
            }

            public override string ToString() {
                return _value.ToString();
            }
        }
    }
}