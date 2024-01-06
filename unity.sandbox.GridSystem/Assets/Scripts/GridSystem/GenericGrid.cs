using CodeMonkey.Utils;
using UnityEngine;

namespace GridSystem {
    public class GenericGrid<TGridType> {
        private readonly Vector3 _origin;
        private int _width;
        private int _height;

        private float _cellSize;

        private readonly TGridType[,] _gridArray;

        public int Width {
            get => _width;
            set => _width = value;
        }

        public int Height {
            get => _height;
            set => _height = value;
        }

        public float CellSize {
            get => _cellSize;
            set => _cellSize = value;
        }

        public GenericGrid(Vector3 origin, int width, int height, float cellSize) {
            _origin = origin;
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _gridArray = new TGridType[_width, _height];
            InitGrid();
        }
        
        //TODO do we need this?
        private void InitGrid() {
            for (int x = 0; x < _width; x++) {
                for (int y = 0; y < _height; y++) {
                    Debug.Log($"Cell {x}, {y}");
                    _gridArray[x, y] = default;
                    // _gridArray.SetValue(y * _width + x, x, y);
                }
            }
        }
        
        public TGridType GetValue(int x, int y) {
            if (IsValidPosition(x, y)) {
                return _gridArray[x, y];
            }

            return default;
        }
        
        public TGridType GetValue(Vector3 worldPosition) {
            if (TryGetXY(worldPosition, out var x, out var y)) {
                return _gridArray[x, y];
            }
            return default;
        }
        
        public bool TryGetValue(Vector3 worldPosition, out TGridType value) {
            if (TryGetXY(worldPosition, out var x, out var y)) {
                value = _gridArray[x, y];
                return true;
            }
            value = default;
            return false;
        }
        
        public void SetValue(int x, int y, TGridType value) {
            if (IsValidPosition(x, y)) {
                _gridArray[x, y] = value;
            }
        }
        
        public void SetValue(Vector3 worldPosition, TGridType value) {
            if (TryGetXY(worldPosition, out var x, out var y)) {
                _gridArray[x, y] = value;
            }
        }
        
        public bool TrySetValue(Vector3 worldPosition, TGridType value) {
            if (TryGetXY(worldPosition, out var x, out var y)) {
                _gridArray[x, y] = value;
                return true;
            }

            return false;
        }

        private bool IsValidPosition(int x, int y) => x >= 0 && y >= 0 && x < _width && y < _height;

        //This is the left bottom corner of the cell
        private Vector3 GetWorldPosition(int x, int y) {
            return new Vector3(x, y) * _cellSize + _origin;
        }
        
        private bool TryGetXY(Vector3 worldPosition, out int x, out int y) {
            x = Mathf.FloorToInt((worldPosition -_origin).x / _cellSize);
            y = Mathf.FloorToInt((worldPosition - _origin).y / _cellSize);
            return IsValidPosition(x, y);
        }

        public void DebugGrid() {
            if (_gridArray == null) {
                return;
            }

            for (int x = 0; x < _gridArray.GetLength(0); x++) {
                for (int y = 0; y < _gridArray.GetLength(1); y++) {
                    UtilsClass.CreateWorldText(_gridArray[x, y].ToString(), null, GetWorldPosition(x, y) + new Vector3(_cellSize, _cellSize) * 0.5f, 20, Color.white,
                        TextAnchor.MiddleCenter, TextAlignment.Center);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
            }

            Debug.DrawLine(GetWorldPosition(0, _height), GetWorldPosition(_width, _height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(_width, 0), GetWorldPosition(_width, _height), Color.white, 100f);
        }
    }
}