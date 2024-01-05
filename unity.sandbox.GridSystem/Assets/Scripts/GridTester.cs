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
        _grid = new Grid(transform.localPosition, width, height, cellSize);
        _grid.DebugGrid();
        
        new Grid(new Vector3(50, 20), 2, 5, 5).DebugGrid();
        new Grid(new Vector3(20, 40), 5, 3, 8).DebugGrid();
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