using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Utils.Narkdagas.GridSystem;
using int2 = Unity.Mathematics.int2;

namespace Utils.Narkdagas.PathFinding {
    //TODO BURST COMPILE THIS
    public class Pathfinding {
        private const int DiagonalCost = 14;
        private const int StraightCost = 10;

        public bool TryFindPath(int2 fromPosition, int2 toPosition, int2 gridSize, GenericSimpleGrid<IPathNode> grid, out int2[] path) {
            int2[] result = null;
            //Initialize the PathNodes
            for (var x = 0; x < gridSize.x; x++) {
                for (var y = 0; y < gridSize.y; y++) {
                    var node = grid.GetGridObject(x, y);
                    node.ResetCosts(DistanceCost(new int2(x, y), toPosition));
                    node.ParentIndex = -1;
                    grid.SetGridObject(x, y, node);
                }
            }

            //Initialize the algorithm
            var offsets = new NativeArray<int2>(8, Allocator.Temp);
            offsets[0] = new(-1, 1); //Top Left
            offsets[1] = new(0, 1); //Top
            offsets[2] = new(1, 1); //Top Right
            offsets[3] = new(1, 0); //Right
            offsets[4] = new(1, -1); //Bottom Right
            offsets[5] = new(0, -1); //Bottom
            offsets[6] = new(-1, -1); //Bottom Left
            offsets[7] = new(-1, 0); //Left

            var openList = new NativeList<int>(Allocator.Temp);
            var closedList = new NativeList<int>(Allocator.Temp);

            var startNodeIndex = PathNodeIndex(fromPosition, gridSize);
            var startNode = grid.GetGridObject(fromPosition.x, fromPosition.y);
            startNode.GCost = 0;
            grid.SetGridObject(fromPosition.x, fromPosition.y, startNode);

            openList.Add(startNodeIndex);

            while (openList.Length > 0) {
                if (TryGetNodeIndexWithLowestFCost(openList, grid, gridSize, out var openListIndex)) {
                    var currentNodeIndex = openList[openListIndex];
                    var gridPos = PathNodeGridPosition(currentNodeIndex, gridSize);
                    var currentNode = grid.GetGridObject(gridPos.x, gridPos.y);
                    if (currentNode.GridPosition.Equals(toPosition)) {
                        //We found the path
                        break;
                    }

                    //Remove the current node from the open list
                    openList.RemoveAtSwapBack(openListIndex);

                    //Add the current node to the closed list
                    closedList.Add(currentNodeIndex);

                    //Loop through the neighbors of the current node
                    foreach (var offset in offsets) {
                        var neighborGridPosition = currentNode.GridPosition + offset;
                        if (!IsPositionInsideGrid(neighborGridPosition, gridSize)) {
                            //This neighbor is outside the grid
                            continue;
                        }

                        var neighborNodeIndex = PathNodeIndex(neighborGridPosition, gridSize);
                        if (closedList.Contains(neighborNodeIndex)) {
                            //This neighbor is already in the closed list
                            continue;
                        }

                        var neighborNode = grid.GetGridObject(neighborGridPosition.x, neighborGridPosition.y);
                        if (!neighborNode.IsWalkable) {
                            //This neighbor is not walkable
                            continue;
                        }

                        var tentativeGCost = currentNode.GCost + DistanceCost(currentNode.GridPosition, neighborNode.GridPosition);
                        if (tentativeGCost < neighborNode.GCost) {
                            //This is a better path to the neighbor
                            neighborNode.GCost = tentativeGCost;
                            neighborNode.ParentIndex = currentNodeIndex;
                            grid.SetGridObject(neighborGridPosition.x, neighborGridPosition.y, neighborNode);
                            if (!openList.Contains(neighborNodeIndex)) {
                                //Add the neighbor to the open list
                                openList.Add(neighborNodeIndex);
                            }
                        }
                    }
                }
            }

            //We have either found the path or there is no path
            if (grid.GetGridObject(toPosition.x, toPosition.y).ParentIndex == -1) {
                //There is no path
                Debug.Log("No Path");
            }
            else {
                //There is a path
                var backtrackPath = BacktrackPathFromEndNode(toPosition, grid, gridSize);
                //var nativeArray = backtrackPath.ToArray(Allocator.Temp);
                result = backtrackPath.ToArray();
                //nativeArray.Dispose();
                backtrackPath.Dispose();
            }

            offsets.Dispose();
            openList.Dispose();
            closedList.Dispose();

            path = result;
            return path != null;
        }

        private static int PathNodeIndex(int2 gridPosition, int2 gridSize) => gridPosition.x + (gridPosition.y * gridSize.x);
        private static int2 PathNodeGridPosition(int index, int2 gridSize) => new(index % gridSize.x, index / gridSize.x);

        private static int DistanceCost(int2 a, int2 b) {
            var xDistance = math.abs(a.x - b.x);
            var yDistance = math.abs(a.y - b.y);

            //This is the amount we would move in a straight line
            var straight = math.abs(xDistance - yDistance);
            //This is the amount we would move diagonally
            var diagonally = math.min(xDistance, yDistance);

            return DiagonalCost * diagonally + StraightCost * straight;
        }

        private static bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize) =>
            gridPosition is { x: >= 0, y: >= 0 } && gridPosition.x < gridSize.x && gridPosition.y < gridSize.y;

        private bool TryGetNodeIndexWithLowestFCost(NativeList<int> openList, GenericSimpleGrid<IPathNode> grid, int2 gridSize, out int openListIndex) {
            var lowestCost = int.MaxValue;
            var lowestCostIndex = -1;
            for (var i = 0; i < openList.Length; i++) {
                var gridPos = PathNodeGridPosition(openList[i], gridSize);
                var pathNode = grid.GetGridObject(gridPos.x, gridPos.y);
                if (pathNode.FCost < lowestCost) {
                    lowestCost = pathNode.FCost;
                    lowestCostIndex = i;
                }
            }

            openListIndex = lowestCostIndex;
            return openListIndex >= 0;
        }

        private NativeList<int2> BacktrackPathFromEndNode(int2 endPosition, GenericSimpleGrid<IPathNode> grid, int2 gridSize) {
            var path = new NativeList<int2>(Allocator.Temp);

            var nextNodeIndex = PathNodeIndex(endPosition, gridSize);
            while (nextNodeIndex != -1) {
                var gridPos = PathNodeGridPosition(nextNodeIndex, gridSize);
                var currentNode = grid.GetGridObject(gridPos.x, gridPos.y);
                path.Add(currentNode.GridPosition);
                nextNodeIndex = currentNode.ParentIndex;
            }

            return path;
        }
    }
}