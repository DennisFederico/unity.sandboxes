using System;
using CodeMonkey.Utils;
using UnityEngine;

namespace Utils.Narkdagas.GridSystem {

    public class GenericSimpleGridVisual<TGridType> where TGridType : struct {

        private GenericSimpleGrid<TGridType> _grid;
        private Mesh _mesh;
        private Vector3 _quadSize;
        private Func<TGridType, float> _normalizeFunc;
        private bool _updateVisual;

        public GenericSimpleGridVisual(GenericSimpleGrid<TGridType> grid, Mesh mesh, Func<TGridType, float> normalizeFunc) {
            _grid = grid;
            _mesh = mesh;
            _quadSize = new Vector3(1, 1) * _grid.CellSize;
            _normalizeFunc = normalizeFunc;
            _grid.OnGridValueChanged += GridOnValueChanged;
            PaintVisual();
        }

        private void GridOnValueChanged(object sender, OnGridValueChangedEventArgs onGridValueChangedEventArgs) {
            _updateVisual = true;
        }

        public void LateUpdateVisual() {
            if (!_updateVisual) return;
            _updateVisual = false;
            PaintVisual();
        }

        protected void PaintVisual() {
            MeshUtils.CreateEmptyMeshArrays(
                _grid.Width * _grid.Height,
                out Vector3[] vertices,
                out Vector2[] uvs,
                out int[] triangles
            );

            for (int x = 0; x < _grid.Width; x++) {
                for (int y = 0; y < _grid.Height; y++) {
                    var index = _grid.GetFlatIndex(x, y);
                    var normalizedValue = _normalizeFunc(_grid.GetGridObject(x, y));
                    var uvValue = new Vector2(normalizedValue, 0f);
                    MeshUtils.AddToMeshArrays(vertices,
                        uvs,
                        triangles,
                        index,
                        _grid.GetWorldPosition(x, y) + _quadSize * 0.5f,
                        0,
                        _quadSize,
                        uvValue,
                        uvValue
                    );
                }
            }

            _mesh.vertices = vertices;
            _mesh.triangles = triangles;
            _mesh.uv = uvs;
        }
    }
}