using System;
using CodeMonkey.Utils;
using UnityEngine;

namespace GridSystem {
    public class HeatMapGrid {
        public const int HEAT_MAP_MAX_VALUE = 100;
        public const int HEAT_MAP_MIN_VALUE = 0;
        private readonly Vector3 _origin;
        private readonly int[,] _gridArray;
        private readonly TextMesh[,] _debugTextArray;
        private int _width;
        private int _height;
        private float _cellSize;

        //Event Handling (TODO. Could we move this out of here?)
        public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;

        public class OnGridValueChangedEventArgs : EventArgs {
            public int x;
            public int y;
        }

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

        public HeatMapGrid(Vector3 origin, int width, int height, float cellSize, bool debugEnabled = false) {
            _origin = origin;
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _gridArray = new int[_width, _height];
            _debugTextArray = new TextMesh[_width, _height];
            InitGrid();
            if (debugEnabled) {
                OnGridValueChanged += (object sender, OnGridValueChangedEventArgs eventArgs) => {
                    _debugTextArray[eventArgs.x, eventArgs.y].text = _gridArray[eventArgs.x, eventArgs.y].ToString();
                };
            }
        }

        private void InitGrid() {
            for (int x = 0; x < _width; x++) {
                for (int y = 0; y < _height; y++) {
                    _gridArray[x, y] = 0;
                }
            }
        }

        //This is the left bottom corner of the cell
        public Vector3 GetWorldPosition(int x, int y) {
            return new Vector3(x, y) * _cellSize + _origin;
        }

        public int GetFlatIndexSafe(int x, int y) {
            if (IsValidPosition(x, y)) {
                return GetFlatIndex(x, y);
            }

            return -1;
        }

        public int GetFlatIndex(int x, int y) => y * _width + x;

        public void SetValue(int x, int y, int value) {
            if (IsValidPosition(x, y)) {
                _gridArray[x, y] = ClampValue(value);
                TriggerGridValueChanged(x, y);
            }
        }

        public void SetValue(Vector3 worldPosition, int value) {
            if (TryGetXY(worldPosition, out var x, out var y)) {
                _gridArray[x, y] = ClampValue(value);
                TriggerGridValueChanged(x, y);
            }
        }

        public bool TrySetValue(Vector3 worldPosition, int value) {
            if (TryGetXY(worldPosition, out var x, out var y)) {
                _gridArray[x, y] = ClampValue(value);
                TriggerGridValueChanged(x, y);
                return true;
            }

            return false;
        }

        private void AddValue(int x, int y, int value) {
            SetValue(x, y, GetValue(x, y) + value);
        }

        public void AddValue(Vector3 worldPosition, int value, int range) {
            if (TryGetXY(worldPosition, out var x, out var y)) {
                //TODO This triggers the change event multiple times
                for (int xRange = 0; xRange < range; xRange++) {
                    for (int yRange = 0; yRange < range - xRange; yRange++) {
                        AddValue(x + xRange, y + yRange, value);
                        if (xRange != 0) AddValue(x - xRange, y + yRange, value);
                        if (yRange != 0) AddValue(x + xRange, y - yRange, value);
                        if (yRange != 0 && xRange != 0) AddValue(x - xRange, y - yRange, value);
                    }
                }
            }
        }
        
        public void AddFallOffValue(Vector3 worldPosition, int value, int range, int rangeAtMaxValue = 1) {
            int stepFallOffValue = value / (range - rangeAtMaxValue);
            if (TryGetXY(worldPosition, out var x, out var y)) {
                for (int xRange = 0; xRange < range; xRange++) {
                    for (int yRange = 0; yRange < range - xRange; yRange++) {
                        var currentRange = xRange + yRange;
                        var amountToAdd = currentRange > rangeAtMaxValue ? value - stepFallOffValue * (currentRange - rangeAtMaxValue) : value;
                        
                        AddValue(x + xRange, y + yRange, amountToAdd);
                        if (xRange != 0) AddValue(x - xRange, y + yRange, amountToAdd);
                        if (yRange != 0) AddValue(x + xRange, y - yRange, amountToAdd);
                        if (yRange != 0 && xRange != 0) AddValue(x - xRange, y - yRange, amountToAdd);
                    }
                }
            }
        }
        

        public int GetValue(int x, int y) {
            if (IsValidPosition(x, y)) {
                return _gridArray[x, y];
            }

            return -1;
        }

        public float GetNormalizedValue(int x, int y) {
            return NormalizeValue(GetValue(x, y));
        }

        public int GetValue(Vector3 worldPosition) {
            if (TryGetXY(worldPosition, out var x, out var y)) {
                return _gridArray[x, y];
            }

            return -1;
        }

        public float GetNormalizedValue(Vector3 worldPosition) {
            return NormalizeValue(GetValue(worldPosition));
        }

        public bool TryGetValue(Vector3 worldPosition, out int value) {
            if (TryGetXY(worldPosition, out var x, out var y)) {
                value = _gridArray[x, y];
                return true;
            }

            value = -1;
            return false;
        }

        private void TriggerGridValueChanged(int x, int y) {
            OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { x = x, y = y });
        }

        private int ClampValue(int value) => Mathf.Clamp(value, HEAT_MAP_MIN_VALUE, HEAT_MAP_MAX_VALUE);

        private float NormalizeValue(int value) => (float)value / HEAT_MAP_MAX_VALUE;

        private bool IsValidPosition(int x, int y) => x >= 0 && y >= 0 && x < _width && y < _height;

        private bool TryGetXY(Vector3 worldPosition, out int x, out int y) {
            x = Mathf.FloorToInt((worldPosition - _origin).x / _cellSize);
            y = Mathf.FloorToInt((worldPosition - _origin).y / _cellSize);
            return IsValidPosition(x, y);
        }

        public void DebugGrid() {
            if (_gridArray == null) {
                return;
            }

            for (int x = 0; x < _gridArray.GetLength(0); x++) {
                for (int y = 0; y < _gridArray.GetLength(1); y++) {
                    _debugTextArray[x, y] = UtilsClass.CreateWorldText(_gridArray[x, y].ToString(), null,
                        GetWorldPosition(x, y) + new Vector3(_cellSize, _cellSize) * 0.5f, 20, Color.white,
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