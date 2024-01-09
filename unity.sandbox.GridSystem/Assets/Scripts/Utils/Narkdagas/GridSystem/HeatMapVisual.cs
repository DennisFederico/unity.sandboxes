using CodeMonkey.Utils;
using UnityEngine;

namespace Utils.Narkdagas.GridSystem {
    public class HeatMapVisual : MonoBehaviour {

        private HeatMapGrid _grid;
        private Mesh _mesh;
        private Vector3 _quadSize;
        private bool _updateVisual;

        public void SetGrid(HeatMapGrid grid) {
            _grid = grid;
            _quadSize = new Vector3(1, 1) * _grid.CellSize;
            _grid.OnGridValueChanged += GridOnValueChanged;
            UpdateHeatMapVisual();
        }

        private void GridOnValueChanged(object sender, HeatMapGrid.OnGridValueChangedEventArgs e) {
            _updateVisual = true;
        }

        private void Awake() {
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
        }

        private void LateUpdate() {
            if (!_updateVisual) return;
            _updateVisual = false;
            UpdateHeatMapVisual();
        }

        private void UpdateHeatMapVisual() {
            MeshUtils.CreateEmptyMeshArrays(
                _grid.Width * _grid.Height,
                out Vector3[] vertices,
                out Vector2[] uvs,
                out int[] triangles
            );

            for (int x = 0; x < _grid.Width; x++) {
                for (int y = 0; y < _grid.Height; y++) {
                    var index = _grid.GetFlatIndex(x, y);
                    var normalizedValue = _grid.GetNormalizedValue(x, y);
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