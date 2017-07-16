using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpUtils.Ext.SpaceAssigner
{
    // http://www.yoda.arachsys.com/csharp/genericoperators.html
    // http://www.lambda-computing.com/publications/articles/generics2/
    /// <summary>
    /// 
    /// </summary>
    public class SpaceAssigner1D
    {
        // Immutable.
        /// <summary>
        /// 
        /// </summary>
        public class Space : IComparable, IComparable<Space>, IEqualityComparer<Space>, IEquatable<Space>
        {
            /// <summary>
            /// 
            /// </summary>
            public readonly long Min;
            /// <summary>
            /// 
            /// </summary>
            public readonly long Max;

            /// <summary>
            /// 
            /// </summary>
            public long Length => Max - Min;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="min"></param>
            /// <param name="max"></param>
            /// <exception cref="Exception"></exception>
            public Space(long min, long max)
            {
                Min = min;
                Max = max;
                if (min > max)
                    throw(new Exception(String.Format("Space(Min={0}, Max={1}). Min is bigger than Max!!", min, max)));
                //Debug.Assert(Min <= Max);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public bool Contains(long value) => (value >= Min) && (value < Max);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static bool Intersects(Space a, Space b) => (a.Max > b.Min) && (b.Max > a.Min);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="that"></param>
            /// <returns></returns>
            public bool Intersects(Space that) => Intersects(this, that);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Space Intersection(Space a, Space b)
            {
                if (!Intersects(a, b)) return null;
                var min = b.Contains(a.Min) ? a.Min : b.Min;
                var max = b.Contains(a.Max) ? a.Max : b.Max;
                return new Space(min, max);
                //return new Space(0, 0);
                //return !((a.To < b.From) || (b.To < a.From));
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="that"></param>
            /// <returns></returns>
            public Space Intersection(Space that) => Intersection(this, that);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="baseSpace"></param>
            /// <param name="spaceToSubstract"></param>
            /// <returns></returns>
            public static Space[] operator -(Space baseSpace, Space spaceToSubstract)
            {
                Space left = null, right = null;

                var intersect = Intersection(baseSpace, spaceToSubstract);

                //if (((SpaceToSubstract as Object) != null) && SpaceToSubstract.Length > 0)
                if (spaceToSubstract != null && spaceToSubstract.Length > 0)
                {
                    if (intersect == null)
                    {
                        // No intersection (Nothing subtracted).
                        left = baseSpace;
                    }
                    else
                    {
                        if (baseSpace.Min < intersect.Min)
                        {
                            // Some space will left after substraction at the left side.
                            left = new Space(baseSpace.Min, intersect.Min);
                        }

                        if (baseSpace.Max > intersect.Max)
                        {
                            // Some space will left after substraction at the right side.
                            right = new Space(intersect.Max, baseSpace.Max);
                        }
                    }
                }
                else
                {
                    left = baseSpace;
                }

                return new[] {left, right}.Where(space => (space != null) && (space.Length > 0)).ToArray();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"Space(Min={Min}, Max={Max})";

            /// <summary>
            /// 
            /// </summary>
            /// <param name="that"></param>
            /// <returns></returns>
            public int CompareTo(Space that) => (Min.CompareTo(that.Min) != 0) ? Max.CompareTo(that.Max) : 0;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="space1"></param>
            /// <param name="space2"></param>
            /// <returns></returns>
            public static bool operator ==(Space space1, Space space2)
            {
                //return (Space1.Min == Space2.Min) && (Space1.Max == Space2.Max);
                if ((space1 as object) == null) return ((space2 as object) == null);
                if ((space2 as object) == null) return false;
                return space1.CompareTo(space2) == 0;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="space1"></param>
            /// <param name="space2"></param>
            /// <returns></returns>
            public static bool operator !=(Space space1, Space space2) => !(space1 == space2);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public bool Equals(Space x, Space y) => x.CompareTo(y) == 0;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public int GetHashCode(Space obj) => (int) obj.Min ^ (int) obj.Max;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="that"></param>
            /// <returns></returns>
            public bool Equals(Space that) => (CompareTo(that) == 0);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj) => Equals(obj as Space);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public int CompareTo(object obj) => CompareTo(obj as Space);

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode() => (int) (Min ^ Max);
        }

        protected SortedSet<Space> AvailableSpaces;

        /// <summary>
        /// 
        /// </summary>
        public SpaceAssigner1D()
        {
            AvailableSpaces = new SortedSet<Space>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Space[] GetAvailableSpaces() => AvailableSpaces.ToArray();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="space"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Space Intersection(Space space) => throw new NotImplementedException();

        /// <summary>
        /// 
        /// </summary>
        protected void PerformCombine()
        {
            bool found;
            if (AvailableSpaces.Count >= 2)
            {
                do
                {
                    found = false;

                    var previousSpace = AvailableSpaces.ElementAt(0);

                    foreach (var currentSpace in AvailableSpaces.Skip(1))
                    {
                        // Contiguous.
                        if (previousSpace.Max == currentSpace.Min)
                        {
                            AvailableSpaces.Remove(previousSpace);
                            AvailableSpaces.Remove(currentSpace);
                            AvailableSpaces.Add(new Space(previousSpace.Min, currentSpace.Max));
                            found = true;
                            break;
                        }

                        previousSpace = currentSpace;
                    }
                } while (found);
            }

            do
            {
                found = false;
                // Remove empty spaces.
                foreach (var space in AvailableSpaces.Where(space => (space.Length == 0)))
                {
                    AvailableSpaces.Remove(space);
                    found = true;
                    break;
                }
            } while (found);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spaceToSubstract"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public SpaceAssigner1D Substract(Space spaceToSubstract)
        {
            bool Continue;
            var maxSteps = AvailableSpaces.Count + 10;
            do
            {
                //Console.WriteLine(this);
                Continue = false;
                if (AvailableSpaces.Count > 0)
                {
                    foreach (var space in AvailableSpaces)
                    {
                        if (!spaceToSubstract.Intersects(space)) continue;
                        //Console.WriteLine("Intersects: " + SpaceToSubstract + " (*) " + Space);
                        var leftSpaces = space - spaceToSubstract;
                        AvailableSpaces.Remove(space);
                        foreach (var leftSpace in leftSpaces)
                        {
                            AvailableSpaces.Add(leftSpace);
                        }
                        Continue = true;
                        break;
                    }
                }
                if (maxSteps-- <= 0) throw(new Exception("Infinite loop detected"));
            } while (Continue);

            PerformCombine();

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spaceToCheckIfIntersects"></param>
        /// <returns></returns>
        public bool Intersects(Space spaceToCheckIfIntersects)
        {
            foreach (var space in AvailableSpaces)
            {
                if (spaceToCheckIfIntersects.Intersects(space))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spaceToAdd"></param>
        /// <returns></returns>
        public SpaceAssigner1D AddAvailable(Space spaceToAdd)
        {
            if (Intersects(spaceToAdd))
            {
                //throw (new NotImplementedException("Overlapping not implemented yet!"));
                Substract(spaceToAdd);
            }

            AvailableSpaces.Add(spaceToAdd);
            PerformCombine();

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public SpaceAssigner1D AddAvailableWithBounds(long min, long max) => AddAvailable(new Space(min, max));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public SpaceAssigner1D AddAvailableWithLength(long min, long length) => AddAvailableWithBounds(min, min + length);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public SpaceAssigner1D AddAllPositiveAvailable() => AddAvailable(new Space(0, long.MaxValue));

        /**
         * Finds an Available Space Chunk that has a length greater or equals to
         * the specified one.
         */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requiredLength"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Space Allocate(long requiredLength)
        {
            Space foundSpace = null;

            foreach (var space in AvailableSpaces)
            {
                if (space.Length >= requiredLength)
                {
                    foundSpace = space;
                    // Greedy. First found, first used.
                    break;
                }
            }

            if (foundSpace == null)
            {
                throw(new Exception("Can't allocate a space of length " + requiredLength + "."));
            }

            AvailableSpaces.Remove(foundSpace);
            var spaceLeft = new Space(foundSpace.Min, foundSpace.Min + requiredLength);
            var spaceRight = new Space(foundSpace.Min + requiredLength, foundSpace.Max);
            AvailableSpaces.Add(spaceRight);

            PerformCombine();

            return spaceLeft;
        }

        /// <summary>
        /// @TODO In order to avoid a greedy behaviour, Allocate[] should have the code and
        ///       Allocate should use this function with a single item.
        /// </summary>
        /// <param name="requiredLengths"></param>
        /// <returns></returns>
        public Space[] Allocate(long[] requiredLengths)
        {
            var spaces = new Space[requiredLengths.Length];
            for (var n = 0; n < requiredLengths.Length; n++)
            {
                spaces[n] = Allocate(requiredLengths[n]);
            }
            return spaces;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"SpaceAssigner1D({string.Join(",", AvailableSpaces)})";
    }
}