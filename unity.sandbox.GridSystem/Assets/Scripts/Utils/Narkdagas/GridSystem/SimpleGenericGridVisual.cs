using System;
using CodeMonkey.Utils;
using UnityEngine;

namespace Utils.Narkdagas.GridSystem {

    public class SimpleGenericGridVisual<TGridType, T> {

        protected SimpleGenericGrid<TGridType, T> Grid;
        protected Mesh Mesh;
        protected Vector3 QuadSize;
        protected Func<TGridType, float> NormalizeFunc;
        private bool _updateVisual;

        public SimpleGenericGridVisual(SimpleGenericGrid<TGridType, T> grid, Mesh mesh, Func<TGridType, float> normalizeFunc) {
            Grid = grid;
            Mesh = mesh;
            QuadSize = new Vector3(1, 1) * Grid.CellSize;
            NormalizeFunc = normalizeFunc;
            Grid.OnGridValueChanged += GridOnValueChanged;
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
                Grid.Width * Grid.Height,
                out Vector3[] vertices,
                out Vector2[] uvs,
                out int[] triangles
            );

            for (int x = 0; x < Grid.Width; x++) {
                for (int y = 0; y < Grid.Height; y++) {
                    var index = Grid.GetFlatIndex(x, y);
                    var normalizedValue = NormalizeFunc(Grid.GetGridObject(x, y));
                    var uvValue = new Vector2(normalizedValue, 0f);
                    MeshUtils.AddToMeshArrays(vertices,
                        uvs,
                        triangles,
                        index,
                        Grid.GetWorldPosition(x, y) + QuadSize * 0.5f,
                        0,
                        QuadSize,
                        uvValue,
                        uvValue
                    );
                }
            }

            Mesh.vertices = vertices;
            Mesh.triangles = triangles;
            Mesh.uv = uvs;
        }
    }
}