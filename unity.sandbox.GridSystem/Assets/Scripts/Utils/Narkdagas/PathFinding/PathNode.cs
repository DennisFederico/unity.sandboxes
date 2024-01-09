using Unity.Mathematics;

namespace Utils.Narkdagas.PathFinding {
    public struct PathNode : IPathNode {
        public int Index { get; set; }
        public int2 GridPosition { get; set; }
        public bool IsWalkable { get; set; }
        public int GCost { get; set; }
        public int HCost { get; set; }
        public int FCost => GCost + HCost;
        public int ParentIndex { get; set; }
        
        public override string ToString() {
            // return $"{Index} [{GridPosition.x}, {GridPosition.y}]\n[{GCost},{HCost},{FCost}]";
            return $"{Index} [{GridPosition.x}, {GridPosition.y}]";
        }
    }

    public interface IPathNode {
        int Index { get; set; }
        int2 GridPosition { get; set; }
        bool IsWalkable { get; set; }
        int GCost { get; set; }
        int HCost { get; set; }
        int FCost { get; }
        int ParentIndex { get; set; }
        
        void ResetCosts(int hCost)  {
            GCost = int.MaxValue;
            HCost = hCost;
        }
    }
}