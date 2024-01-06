using CodeMonkey.Utils;
using UnityEngine;

namespace GridSystem {
    public class GridOfInts {
        private readonly Vector3 _origin;
        private int _width;
        private int _height;

        private float _cellSize;

        private readonly int[,] _gridArray;

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

        public GridOfInts(Vector3 origin, int width, int height, float cellSize) {
            _origin = origin;
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _gridArray = new int[_width, _height];
            InitGrid();
        }
        
        private void InitGrid() {
            for (int x = 0; x < _width; x++) {
                for (int y = 0; y < _height; y++) {
                    Debug.Log($"Cell {x}, {y}");
                    _gridArray[x, y] = y * _width + x;
                    // _gridArray.SetValue(y * _width + x, x, y);
                }
            }
        }

        //This is the left bottom corner of the cell
        private Vector3 GetWorldPosition(int x, int y) {
            return new Vector3(x, y) * _cellSize + _origin;
        }

        public void SetValue(int x, int y, int value) {
            if (IsValidPosition(x, y)) {
                _gridArray[x, y] = value;
            }
        }
        
        public int GetValue(int x, int y) {
            if (IsValidPosition(x, y)) {
                return _gridArray[x, y];
            }

            return -1;
        }
        
        public void SetValue(Vector3 worldPosition, int value) {
            if (TryGetXY(worldPosition, out var x, out var y)) {
                _gridArray[x, y] = value;
            }
        }
        
        public bool TrySetValue(Vector3 worldPosition, int value) {
            if (TryGetXY(worldPosition, out var x, out var y)) {
                _gridArray[x, y] = value;
                return true;
            }

            return false;
        }
        
        public int GetValue(Vector3 worldPosition) {
            if (TryGetXY(worldPosition, out var x, out var y)) {
                return _gridArray[x, y];
            }
            return -1;
        }
        
        public bool TryGetValue(Vector3 worldPosition, out int value) {
            if (TryGetXY(worldPosition, out var x, out var y)) {
                value = _gridArray[x, y];
                return true;
            }
            value = -1;
            return false;
        }

        private bool IsValidPosition(int x, int y) => x >= 0 && y >= 0 && x < _width && y < _height;

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