using System;

namespace QuadTreeMapEngine.Data
{
    public abstract class QuadTreeNode
    {
        public QuadTreeNode Parent { get; protected set; }
        public int Level { get; protected set; }
        public int X { get; protected set; }
        public int Y { get; protected set; }

        public string Key => $"{Level}_{X}_{Y}";

        protected QuadTreeNode[] Children { get; } = new QuadTreeNode[4];

        protected QuadTreeNode(QuadTreeNode parent, int level, int x, int y)
        {
            Parent = parent;
            Level = level;
            X = x;
            Y = y;
        }

        public void AddChild(QuadTreeNode child)
        {
            if (child.Level != Level + 1)
                throw new Exception($"child level {child.Level} does not match parent's {Level}");
            if (FloorDiv(child.X, 2) != X || FloorDiv(child.Y, 2) != Y)
                throw new Exception($"child x/y pair ({child.X}, {child.Y}) does not match parent's ({X}, {Y})");

            int x = child.X > 0 ? child.X % 2 : -child.X % 2;
            int y = child.Y > 0 ? child.Y % 2 : -child.Y % 2;

            Children[y * 2 + x] = child;
        }

        public QuadTreeNode GetChild(int x, int y)
        {
            return Children[y * 2 + x];
        }

        public QuadTreeNode FindParent(int level)
        {
            return level == Level ? this : Parent.FindParent(level);
        }

        public override string ToString()
        {
            var indent = new string(' ', (1 + Level) * 2);

            var subtree = "";
            foreach (var child in Children)
            {
                if (child != null)
                    subtree += $"\n{child}";
            }

            return $"{indent}{Key}{subtree}";
        }

        /// <summary>
        /// Performs integer division rounding towards negative
        /// infinity (as opposed to rounding towards zero).
        /// </summary>
        /// <param name="a">Dividend in the calculation a/b.</param>
        /// <param name="b">Divisor in the calculation a/b.</param>
        /// <returns>Quotient rounded towards -infinity.</returns>
        protected int FloorDiv(int a, int b)
        {
            return (a / b - Convert.ToInt32(((a < 0) ^ (b < 0)) && (a % b != 0)));
        }
    }
}
