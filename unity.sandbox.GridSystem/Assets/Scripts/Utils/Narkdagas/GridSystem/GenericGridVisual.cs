using CodeMonkey.Utils;
using UnityEngine;

namespace Utils.Narkdagas.GridSystem {

    public class GenericGridVisual<TGridType, T> where TGridType : IGridObject<T> {

        private readonly GenericGrid<TGridType, T> _grid;
        private readonly Mesh _mesh;
        private readonly Vector3 _quadSize;
        private bool _updateVisual;

        public GenericGridVisual(GenericGrid<TGridType, T> grid, Mesh mesh) {
            _grid = grid;
            _mesh = mesh;
            _quadSize = new Vector3(1, 1) * _grid.CellSize;
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

        private void PaintVisual() {
            MeshUtils.CreateEmptyMeshArrays(
                _grid.Width * _grid.Height,
                out Vector3[] vertices,
                out Vector2[] uvs,
                out int[] triangles
            );

            for (int x = 0; x < _grid.Width; x++) {
                for (int y = 0; y < _grid.Height; y++) {
                    var index = _grid.GetFlatIndex(x, y);
                    var normalizedValue = _grid.GetGridObject(x, y).GetNormalizedValue();
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