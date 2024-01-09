using System;
using CodeMonkey.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace Utils.Narkdagas.GridSystem {

    public class SimpleGenericGrid<TGridType, T> {
        private readonly Vector3 _origin;
        private readonly TGridType[,] _gridArray;
        private readonly TextMesh[,] _debugTextArray;
        private int _width;
        private int _height;
        private float _cellSize;

        private readonly Action<TGridType, T> _setValueAction;
        private readonly Action<TGridType, T> _addValueAction;
        //private readonly Func<TGridType, T> _getValueFunc;

        public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;

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

        public SimpleGenericGrid(Vector3 origin, int width, int height, float cellSize,
            Func<int, int2, TGridType> createFunc,
            Action<TGridType, T> setValueAction,
            Action<TGridType, T> addValueAction,
            bool debugEnabled = false) {
            _origin = origin;
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _setValueAction = setValueAction;
            _addValueAction = addValueAction;
            _gridArray = new TGridType[_width, _height];
            InitGrid(createFunc);
            if (debugEnabled) {
                _debugTextArray = new TextMesh[_width, _height];
                OnGridValueChanged += (_, eventArgs) => { _debugTextArray[eventArgs.X, eventArgs.Y].text = _gridArray[eventArgs.X, eventArgs.Y]?.ToString(); };
            }
        }

        private void InitGrid(Func<int, int2, TGridType> createFunc) {
            for (int x = 0; x < _width; x++) {
                for (int y = 0; y < _height; y++) {
                    _gridArray[x, y] = createFunc(GetFlatIndex(x, y), new int2(x, y));
                }
            }
        }

        //This is the left bottom corner of the cell
        public Vector3 GetWorldPosition(int x, int y) => new Vector3(x, y) * _cellSize + _origin;

        public int GetFlatIndex(int x, int y) => y * _width + x;

        public int GetFlatIndexSafe(int x, int y) {
            if (IsValidPosition(x, y)) {
                return GetFlatIndex(x, y);
            }

            return -1;
        }

        public void SetGridObject(int x, int y, TGridType gridObject) {
            if (!IsValidPosition(x, y)) return;
            _gridArray[x, y] = gridObject;
            TriggerGridValueChanged(x, y);
        }

        public void SetGridObject(Vector3 worldPosition, TGridType gridObject) {
            if (!TryGetXY(worldPosition, out var x, out var y)) return;
            _gridArray[x, y] = gridObject;
            TriggerGridValueChanged(x, y);
        }

        public bool TrySetGridObject(Vector3 worldPosition, TGridType gridObject) {
            if (!TryGetXY(worldPosition, out var x, out var y)) return false;
            _gridArray[x, y] = gridObject;
            TriggerGridValueChanged(x, y);
            return true;
        }

        public void SetGridValue(int x, int y, T value) {
            if (!IsValidPosition(x, y)) return;
            _setValueAction(_gridArray[x, y], value);
            TriggerGridValueChanged(x, y);
        }

        public void SetGridValue(Vector3 worldPosition, T value) {
            if (!TryGetXY(worldPosition, out var x, out var y)) return;
            _setValueAction(_gridArray[x, y], value);
            TriggerGridValueChanged(x, y);
        }

        public bool TrySetGridValue(Vector3 worldPosition, T value) {
            if (!TryGetXY(worldPosition, out var x, out var y)) return false;
            _setValueAction(_gridArray[x, y], value);
            TriggerGridValueChanged(x, y);
            return true;
        }

        private void AddGridObjectValue(int x, int y, T value) {
            _addValueAction(GetGridObject(x, y), value);
            TriggerGridValueChanged(x, y);
        }

        public void AddGridObjectValue(Vector3 worldPosition, T value) {
            if (!TryGetXY(worldPosition, out var x, out var y)) return;
            _addValueAction(GetGridObject(x, y), value);
            TriggerGridValueChanged(x, y);
        }

        public void AddGridObjectValue(Vector3 worldPosition, T value, int range) {
            if (!TryGetXY(worldPosition, out var x, out var y)) return;
            for (int xRange = 0; xRange < range; xRange++) {
                for (int yRange = 0; yRange < range - xRange; yRange++) {
                    AddGridObjectValue(x + xRange, y + yRange, value);
                    if (xRange != 0) AddGridObjectValue(x - xRange, y + yRange, value);
                    if (yRange != 0) AddGridObjectValue(x + xRange, y - yRange, value);
                    if (yRange != 0 && xRange != 0) AddGridObjectValue(x - xRange, y - yRange, value);
                }
            }
        }

        // TODO FIX THIS
        // public void AddFallOffValue(Vector3 worldPosition, TGridType value, int range, int rangeAtMaxValue = 1) {
        //     int stepFallOffValue = value / (range - rangeAtMaxValue);
        //     if (TryGetXY(worldPosition, out var x, out var y)) {
        //         for (int xRange = 0; xRange < range; xRange++) {
        //             for (int yRange = 0; yRange < range - xRange; yRange++) {
        //                 var currentRange = xRange + yRange;
        //                 var amountToAdd = currentRange > rangeAtMaxValue ? value - stepFallOffValue * (currentRange - rangeAtMaxValue) : value;
        //                 
        //                 AddValue(x + xRange, y + yRange, amountToAdd);
        //                 if (xRange != 0) AddValue(x - xRange, y + yRange, amountToAdd);
        //                 if (yRange != 0) AddValue(x + xRange, y - yRange, amountToAdd);
        //                 if (yRange != 0 && xRange != 0) AddValue(x - xRange, y - yRange, amountToAdd);
        //             }
        //         }
        //     }
        // }


        public TGridType GetGridObject(int x, int y) {
            if (IsValidPosition(x, y)) {
                return _gridArray[x, y];
            }

            return default;
        }

        public TGridType GetGridObject(Vector3 worldPosition) {
            return TryGetXY(worldPosition, out var x, out var y) ? _gridArray[x, y] : default;
        }

        public bool TryGetGridObject(Vector3 worldPosition, out TGridType value) {
            if (TryGetXY(worldPosition, out var x, out var y)) {
                value = _gridArray[x, y];
                return true;
            }

            value = default;
            return false;
        }

        private void TriggerGridValueChanged(int x, int y) {
            OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { X = x, Y = y });
        }

        private bool IsValidPosition(int x, int y) => x >= 0 && y >= 0 && x < _width && y < _height;

        public bool TryGetXY(Vector3 worldPosition, out int x, out int y) {
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
                    _debugTextArray[x, y] = UtilsClass.CreateWorldText(_gridArray[x, y]?.ToString(), null,
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