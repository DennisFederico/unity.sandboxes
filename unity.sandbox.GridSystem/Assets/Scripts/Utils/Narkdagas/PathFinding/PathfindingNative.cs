using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Utils.Narkdagas.PathFinding {
    //TODO BURST COMPILE THIS
    public class PathfindingNative {
        private const int DiagonalCost = 14;
        private const int StraightCost = 10;
        
        public static NativeArray<PathNode> NewGridArray(int2 gridSize) {
            var gridArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);
            //Initialize the PathNodes
            for (var x = 0; x < gridSize.x; x++) {
                for (var y = 0; y < gridSize.y; y++) {
                    var gridPosition = new int2(x, y);
                    var pathNode = new PathNode {
                        Index = PathNodeIndex(gridPosition, gridSize),
                        GridPosition = gridPosition,
                        IsWalkable = true,
                        GCost = int.MaxValue,
                        HCost = int.MaxValue,
                        ParentIndex = -1
                    };
                    gridArray[pathNode.Index] = pathNode;
                }
            }
            return gridArray;
        }
        
        public bool TryFindPath(int2 fromPosition, int2 toPosition, int2 gridSize, NativeArray<PathNode> gridArray, out int2[] path) {
            int2[] result = null;
            //Create a Native (thread-safe) "Flat" Array of PathNodes
            //var gridArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);

            //Initialize the PathNodes
            for (var x = 0; x < gridSize.x; x++) {
                for (var y = 0; y < gridSize.y; y++) {
                    var gridPosition = new int2(x, y);
                    var pathNodeIndex = PathNodeIndex(gridPosition, gridSize);
                    var pathNode = gridArray[pathNodeIndex];
                    pathNode.GCost = int.MaxValue;
                    pathNode.HCost = DistanceCost(gridPosition, toPosition);
                    pathNode.ParentIndex = -1;
                    gridArray[pathNode.Index] = pathNode;
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
            var startNode = gridArray[startNodeIndex];
            startNode.GCost = 0;
            gridArray[startNodeIndex] = startNode;

            openList.Add(startNodeIndex);

            while (openList.Length > 0) {
                if (TryGetNodeIndexWithLowestFCost(openList, gridArray, out var openListIndex)) {
                    var currentNodeIndex = openList[openListIndex];
                    var currentNode = gridArray[currentNodeIndex];
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

                        var neighborNode = gridArray[neighborNodeIndex];
                        if (!neighborNode.IsWalkable) {
                            //This neighbor is not walkable
                            continue;
                        }

                        var tentativeGCost = currentNode.GCost + DistanceCost(currentNode.GridPosition, neighborNode.GridPosition);
                        if (tentativeGCost < neighborNode.GCost) {
                            //This is a better path to the neighbor
                            neighborNode.GCost = tentativeGCost;
                            neighborNode.ParentIndex = currentNodeIndex;
                            gridArray[neighborNodeIndex] = neighborNode;
                            if (!openList.Contains(neighborNodeIndex)) {
                                //Add the neighbor to the open list
                                openList.Add(neighborNodeIndex);
                            }
                        }
                    }
                }
            }

            //We have either found the path or there is no path
            if (gridArray[PathNodeIndex(toPosition, gridSize)].ParentIndex == -1) {
                //There is no path
                Debug.Log("No Path");
            }
            else {
                //There is a path
                var backtrackPath = BacktrackPathFromEndNode(PathNodeIndex(toPosition, gridSize), gridArray);
                //var nativeArray = backtrackPath.ToArray(Allocator.Temp);
                result = backtrackPath.ToArray();
                //nativeArray.Dispose();
                backtrackPath.Dispose();
            }

            gridArray.Dispose();
            offsets.Dispose();
            openList.Dispose();
            closedList.Dispose();

            path = result;
            return path != null;
        }

        private static int PathNodeIndex(int2 gridPosition, int2 gridSize) => gridPosition.x + (gridPosition.y * gridSize.x);

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
        
        private bool TryGetNodeIndexWithLowestFCost(NativeList<int> openList, NativeArray<PathNode> pathNodes, out int openListIndex) {
            var lowestCost = int.MaxValue;
            var lowestCostIndex = -1;
            for (var i = 0; i < openList.Length; i++) {
                var pathNode = pathNodes[openList[i]];
                if (pathNode.FCost < lowestCost) {
                    lowestCost = pathNode.FCost;
                    lowestCostIndex = i;
                }
            }

            openListIndex = lowestCostIndex;
            return openListIndex >= 0;
        }
        
        private NativeList<int2> BacktrackPathFromEndNode(int endNodeIndex, NativeArray<PathNode> pathNodes) {
            var path = new NativeList<int2>(Allocator.Temp);
            var currentNodeIndex = endNodeIndex;
            while (currentNodeIndex != -1) {
                var currentNode = pathNodes[currentNodeIndex];
                path.Add(currentNode.GridPosition);
                currentNodeIndex = currentNode.ParentIndex;
            }

            return path;
        }
    }
}