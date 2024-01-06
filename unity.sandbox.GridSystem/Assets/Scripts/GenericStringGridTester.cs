using GridSystem;
using UnityEngine;

public class GenericStringGridTester : MonoBehaviour {

    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;
    [SerializeField] private bool debugEnabled;

    private Camera _camera;
    private Mesh _mesh;
    private GenericGrid<GridStringValue, string> _grid;
    private GenericGridVisual<GridStringValue, string> _gridVisual;

    private void Awake() {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
    }

    void Start() {
        _camera = Camera.main;
        _grid = new GenericGrid<GridStringValue, string>(transform.position, width, height, cellSize, () => new GridStringValue(), debugEnabled);
        _gridVisual = new GenericGridVisual<GridStringValue, string>(_grid, _mesh);
        if (debugEnabled) _grid.DebugGrid();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.A)) {
            Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            _grid.AddGridObject(worldPosition, "A");
        }
        if (Input.GetKeyDown(KeyCode.B)) {
            Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            _grid.AddGridObject(worldPosition, "B");
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            _grid.AddGridObject(worldPosition, "C");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            _grid.SetGridObject(worldPosition, _grid.GetGridObject(worldPosition).AddNumber(1));
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            _grid.SetGridObject(worldPosition, _grid.GetGridObject(worldPosition).AddNumber(2));
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            _grid.SetGridObject(worldPosition, _grid.GetGridObject(worldPosition).AddNumber(3));
        }
    }

    private void LateUpdate() {
        _gridVisual.LateUpdateVisual();
    }

    public class GridStringValue : IGridObject<string> {
        private string _letters = "";
        private string _numbers = "";

        public void SetValue(string value) {
            _letters = value;
        }

        public void AddValue(string value) {
            SetValue(_letters + value);
        }
        
        public GridStringValue AddLetter(char value) {
            _letters += value;
            return this;
        }
        
        public GridStringValue AddNumber(int value) {
            _numbers += value;
            return this;
        }
        
        public float GetNormalizedValue() {
            return 0.1f;
        }

        public override string ToString() {
            return $"{_letters}\n{_numbers}";
        }
    }
}