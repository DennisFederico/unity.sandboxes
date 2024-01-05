using UnityEngine;
using Grid = GridSystem.Grid;

public class GridTester : MonoBehaviour {
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;

    private Camera _camera;
    private Grid _grid;

    private void Start() {
        _camera = Camera.main;
        _grid = new Grid(width, height, cellSize);
        
        for (int x = 0; x < _grid.Width; x++) {
            for (int y = 0; y < _grid.Height; y++) {
                _grid.SetValue(x, y,  y * width + x);
            }
        }

        _grid.DebugGrid();
    }
    
    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            _grid.SetValue(worldPosition, 99);
            _grid.DebugGrid();
        }
        
        if (Input.GetMouseButtonDown(1)) {
            Vector3 worldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(_grid.GetValue(worldPosition));
        }
    }
}