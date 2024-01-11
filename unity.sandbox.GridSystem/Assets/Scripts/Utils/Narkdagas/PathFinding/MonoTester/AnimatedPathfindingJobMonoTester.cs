using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utils.Narkdagas.GridSystem;
using Random = Unity.Mathematics.Random;

namespace Utils.Narkdagas.PathFinding.MonoTester {
    public class AnimatedPathfindingJobMonoTester : MonoBehaviour {
        
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private float cellSize;
        [SerializeField] private bool debugEnabled;
        [SerializeField] private Material gradientMaterial;
        [SerializeField] private PathfindingMovement prefab;
        
        public event EventHandler<NewGridPathRequestEvent> OnNewGridPathRequestEvent;

        private Camera _camera;
        private Mesh _mesh;
        private GenericSimpleGrid<PathNode> _grid;
        private GenericSimpleGridVisual<PathNode> _gridVisual;
        private PathfindingMovement _player;

        private void OnEnable() {
            OnNewGridPathRequestEvent += OnOnNewGridPathRequestEvent;
        }

        private void OnOnNewGridPathRequestEvent(object sender, NewGridPathRequestEvent e) {
            var targetGameObject = (PathfindingEventMovement)sender;
        }

        private void OnDisable() {
            OnNewGridPathRequestEvent -= OnOnNewGridPathRequestEvent;
        }


        private void Start() {
            _camera = Camera.main;
            _mesh = new Mesh();
            GetComponent<MeshRenderer>().material = gradientMaterial;
            GetComponent<MeshFilter>().mesh = _mesh;
            
            _grid = new GenericSimpleGrid<PathNode> (transform.position, width, height, cellSize,
                (index, gridPos) => new PathNode {
                    Index = index,
                    XY = gridPos,
                    IsWalkable = true
                },
                debugEnabled);
            _gridVisual = new GenericSimpleGridVisual<PathNode>(_grid, _mesh, (node) => {
                if (!node.IsWalkable) return 0f;
                if (node.ParentIndex != -1) return .5f;
                return 0.25f;
            });

            _grid.PaintDebugGrid();
        }

        private Vector3 _startDragPosition;
        private void Update() {
            
            if (Input.GetKeyDown(KeyCode.D)) {
                int numJobs = 500;
                var startTime = Time.realtimeSinceStartup;
                Debug.Log($"Start {numJobs} jobs at {startTime}");
                var gridAsArray = _grid.GetGridAsArray(Allocator.TempJob);
                var random = Random.CreateFromIndex((uint)Time.frameCount);
                var results = new NativeArray<NativeList<int2>>(numJobs, Allocator.TempJob);
                var handlers = new NativeArray<JobHandle>(numJobs, Allocator.TempJob);
                for (int i = 0; i < numJobs; i++) {
                    results[i] = new NativeList<int2>(numJobs, Allocator.TempJob);
                    var startPos = random.NextInt2 (new int2(0,0), new int2(width/4, height/4));
                    var endPos = random.NextInt2(new int2(width/2, height/2), new int2(width,height));
                    var jobHandle = new PathfindingJob() {
                        GridArray = gridAsArray,
                        GridSize = new int2(width, height),
                        FromPosition = startPos,
                        ToPosition = endPos,
                        ResultPath = results[i]
                    }.Schedule();    
                    handlers[i] = jobHandle;
                }
                JobHandle.CompleteAll(handlers);
                foreach (var result in results) {
                    result.Dispose();
                }

                results.Dispose();
                handlers.Dispose();
                gridAsArray.Dispose();
                
                var endTime = Time.realtimeSinceStartup;
                Debug.Log($"End {numJobs} jobs at {endTime} in {endTime - startTime}s");
            }
            
            if (Input.GetMouseButtonDown(0)) {
                _startDragPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            }
            
            if (Input.GetMouseButtonUp(0)) {
                var endDragPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
                
                if (_grid.TryGetXY(_startDragPosition, out var startPos) && _grid.TryGetXY(endDragPosition, out var endPos)) {
                    
                    if (math.distance(startPos, endPos) < 1) {
                        startPos = int2.zero;
                    }
                   
                    var gridAsArray = _grid.GetGridAsArray(Allocator.TempJob);
                    var resultPath = new NativeList<int2>(Allocator.TempJob);
                    var jobHandle = new PathfindingJob() {
                        GridArray = gridAsArray,
                        GridSize = new int2(width, height),
                        FromPosition = startPos,
                        ToPosition = endPos,
                        ResultPath = resultPath
                    }.Schedule();
                    jobHandle.Complete();
                    
                    if (jobHandle.IsCompleted) {
                        DebugPath(resultPath.AsArray().ToArray());
                        var path3 = TransformPath(resultPath, cellSize);
                        if (!_player) {
                            _player = Instantiate(prefab);
                        }
                        _player.SetPath(path3);
                        resultPath.Dispose();
                        gridAsArray.Dispose();
                    }
                }
            }
            
            if (Input.GetMouseButtonDown(1)) {
                if (!_grid.TryGetXY(_camera.ScreenToWorldPoint(Input.mousePosition), out var x, out var y)) return;
                var node = _grid.GetGridObject(x, y);
                node.IsWalkable = !node.IsWalkable;
                _grid.SetGridObject(x, y, node);
            }
        }

        private Vector3[] TransformPath(NativeList<int2> path, float gridCellSize) {
            var path3 = new Vector3[path.Length];
            int index = 0;
            foreach (var step in path) {
                path3[index++] = _grid.GetWorldPosition(step.x , step.y) + new Vector3(gridCellSize/2, gridCellSize/2, 0);
            }
            return path3;
        }

        private void LateUpdate() {
            _gridVisual.LateUpdateVisual();
        }

        private void DebugPath(int2[] path) {
            var offset = new Vector3(cellSize/2, cellSize/2, 0);
            for (int i = 0; i < path.Length - 1; i++) {
                Debug.DrawLine(
                    _grid.GetWorldPosition(path[i].x, path[i].y) + offset,
                    _grid.GetWorldPosition(path[i+1].x, path[i+1].y) + offset,
                    Color.red, 15f
                );
            }
        }
    }
}