using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer.Profiling
{

    // Credit to Krypt for this algorithm in its entirety 

    public enum ChangeType
    {
        Removed = -1,
        Unmodified = 0,
        Added = 1,
    }

    public class Change<T>
    {
        public T value { get; private set; }
        public ChangeType change { get; private set; }

        public Change(T value, ChangeType change)
        {
            this.value = value;
            this.change = change;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            switch (change)
            {
                case ChangeType.Added:
                    sb.Append("+");
                    break;
                case ChangeType.Removed:
                    sb.Append("-");
                    break;
                default:
                    sb.Append(" ");
                    break;
            }
            sb.Append(value.ToString());

            return sb.ToString();
        }
    }

    public class Myers<T>
    {
        T[] left;
        T[] right;
        EqualityComparer<T> comparer;
        public List<Change<T>> changeSet { get; private set; } = null;

        public Myers(T[] left, T[] right, EqualityComparer<T> comparer = null)
        {
            this.left = left;
            this.right = right;
            this.comparer = comparer != null ? comparer : EqualityComparer<T>.Default;
        }

        public void Compute()
        {
            int[,] grid = BuildGrid(left, right);
            int[] path = Search(grid, left, right);
            changeSet = Translate(left, right, path);
        }

        private int[,] BuildGrid(T[] left, T[] right)
        {
            int[,] grid = new int[left.Length + 1, right.Length + 1];

            for (int y = 0; y <= right.Length; y++)
            {
                for (int x = 0; x <= left.Length; x++)
                {
                    if (x == 0 && y == 0)
                    {
                        grid[x, y] = 0;
                    }
                    else
                    {
                        int value = int.MaxValue;
                        if (x > 0)
                        {
                            value = Math.Min(value, grid[x - 1, y]);
                        }
                        if (y > 0)
                        {
                            value = Math.Min(value, grid[x, y - 1]);
                        }
                        if (x > 0 && y > 0)
                        {
                            if (comparer.Equals(left[x - 1], right[y - 1]))
                            {
                                value = Math.Min(value, grid[x - 1, y - 1]);
                            }
                        }

                        grid[x, y] = value + 1;
                    }
                }
            }


            return grid;
        }

        private int[] Search(int[,] grid, T[] left, T[] right)
        {
            int idx(int X, int Y) => X + Y * (left.Length + 1);

            //int currentIdx = idx(oldSet.Length, newSet.Length);
            int x = left.Length, y = right.Length;
            int[] foundPath = new int[grid[left.Length, right.Length] + 1];

            while (x + y > 0)
            {
                int value = grid[x, y];

                foundPath[value] = idx(x, y);

                if (x > 0 && y > 0)
                {
                    if (comparer.Equals(left[x - 1], right[y - 1]))
                    {
                        if (grid[x - 1, y - 1] < value)
                        {
                            x = x - 1;
                            y = y - 1;
                            continue;
                        }
                    }
                }
                if (y > 0)
                {
                    if (grid[x, y - 1] < value)
                    {
                        y = y - 1;
                        continue;
                    }
                }
                if (x > 0)
                {
                    if (grid[x - 1, y] < value)
                    {
                        x = x - 1;
                        continue;
                    }
                }
                throw new Exception("Invalid search grid");
            }

            return foundPath;
        }

        private static List<Change<T>> Translate(T[] left, T[] right, int[] foundPath)
        {
            List<Change<T>> changeSet = new List<Change<T>>();

            int lineLen = left.Length + 1;

            int X(int idx_) => idx_ % lineLen;
            int Y(int idx_) => idx_ / lineLen;
            int pX = 0, pY = 0;

            for (int i = 0; i < foundPath.Length - 1; i++)
            {
                int cX = X(foundPath[i]);
                int cY = Y(foundPath[i]);
                int nX = X(foundPath[i + 1]);
                int nY = Y(foundPath[i + 1]);

                if (cX != nX && cY != nY)
                {
                    changeSet.Add(new Change<T>(left[pX], ChangeType.Unmodified));
                    pX++; pY++;
                }
                else if (cX != nX)
                {
                    changeSet.Add(new Change<T>(left[pX], ChangeType.Removed));
                    pX++;
                }
                else if (cY != nY)
                {
                    changeSet.Add(new Change<T>(right[pY], ChangeType.Added));
                    pY++;
                }
            }
            return changeSet;
        }

    }
}


