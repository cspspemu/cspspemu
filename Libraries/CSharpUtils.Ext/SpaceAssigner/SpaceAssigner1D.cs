using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CSharpUtils.SpaceAssigner
{
	// http://www.yoda.arachsys.com/csharp/genericoperators.html
	// http://www.lambda-computing.com/publications/articles/generics2/
	public class SpaceAssigner1D
	{
		// Immutable.
		public class Space : IComparable, IComparable<Space>, IEqualityComparer<Space>, IEquatable<Space>
		{
			readonly public long Min;
			readonly public long Max;

			public long Length
			{
				get
				{
					return Max - Min;
				}
			}

			public Space(long Min, long Max)
			{
				this.Min = Min;
				this.Max = Max;
				Debug.Assert(Min <= Max);
			}

			public bool Contains(long Value)
			{
				return (Value >= Min) && (Value < Max);
			}

			public static bool Intersects(Space a, Space b)
			{
				//return !((a.Max < b.Min) || (b.Max < a.Min));
				return (a.Max > b.Min) && (b.Max > a.Min);
			}

			public bool Intersects(Space that)
			{
				return Intersects(this, that);
			}

			public static Space Intersection(Space a, Space b)
			{
				if (Intersects(a, b))
				{
					var Min = b.Contains(a.Min) ? a.Min : b.Min;
					var Max = b.Contains(a.Max) ? a.Max : b.Max;
					return new Space(Min, Max);
				}
				return null;
				//return new Space(0, 0);
				//return !((a.To < b.From) || (b.To < a.From));
			}

			public Space Intersection(Space that)
			{
				return Intersection(this, that);
				//return !((a.To < b.From) || (b.To < a.From));
			}

			public static Space[] operator -(Space BaseSpace, Space SpaceToSubstract) 
			{
				Space Left = null, Right = null;

				Space Intersect = Intersection(BaseSpace, SpaceToSubstract);

				//if (((SpaceToSubstract as Object) != null) && SpaceToSubstract.Length > 0)
				if (SpaceToSubstract != null && SpaceToSubstract.Length > 0)
				{
					if (Intersect == null)
					{
						// No intersection (Nothing subtracted).
						Left = BaseSpace;
					}
					else
					{
						if (BaseSpace.Min < Intersect.Min)
						{
							// Some space will left after substraction at the left side.
							Left = new Space(BaseSpace.Min, Intersect.Min);
						}

						if (BaseSpace.Max > Intersect.Max)
						{
							// Some space will left after substraction at the right side.
							Right = new Space(Intersect.Max, BaseSpace.Max);
						}
					}
				}
				else
				{
					Left = BaseSpace;
				}

				return new Space[] { Left, Right }.Where(Space => (Space != null) && (Space.Length > 0)).ToArray();
			}

			public override string ToString()
			{
				return "Space(Min=" + Min + ", Max=" + Max + ")";
			}

			public int CompareTo(Space that)
			{
				/*if (that == null)
				{
					return (this.Length == 0) ? 0 : -1;
				}*/
				return (this.Min.CompareTo(that.Min) != 0) ? this.Max.CompareTo(that.Max) : 0;
			}

			public static bool operator ==(Space Space1, Space Space2)
			{
				//return (Space1.Min == Space2.Min) && (Space1.Max == Space2.Max);
				if ((Space1 as Object) == null) return ((Space2 as Object) == null);
				if ((Space2 as Object) == null) return false;
				return Space1.CompareTo(Space2) == 0;
			}

			public static bool operator !=(Space Space1, Space Space2)
			{
				return !(Space1 == Space2);
			}

			public bool Equals(Space x, Space y)
			{
				return x.CompareTo(y) == 0;
			}

			public int GetHashCode(Space obj)
			{
				return (int)obj.Min ^ (int)obj.Max;
			}

			public bool Equals(Space that)
			{
				return (this.CompareTo(that) == 0);
			}

			public override bool Equals(object obj)
			{
				return this.Equals(obj as Space);
			}

			public int CompareTo(object obj)
			{
				return this.CompareTo(obj as Space);
			}

			public override int GetHashCode()
			{
				return (int)(Min ^ Max);
			}
		}

		protected SortedSet<Space> AvailableSpaces;

		public SpaceAssigner1D()
		{
			AvailableSpaces = new SortedSet<Space>();
		}

		public Space Intersection(Space Space)
		{
			throw(new NotImplementedException());
		}

		protected void PerformCombine()
		{
			bool Found;
			if (AvailableSpaces.Count >= 2)
			{
				do
				{
					Found = false;

					var PreviousSpace = AvailableSpaces.ElementAt(0);

					foreach (var CurrentSpace in AvailableSpaces.Skip(1))
					{
						// Contiguous.
						if (PreviousSpace.Max == CurrentSpace.Min)
						{
							AvailableSpaces.Remove(PreviousSpace);
							AvailableSpaces.Remove(CurrentSpace);
							AvailableSpaces.Add(new Space(PreviousSpace.Min, CurrentSpace.Max));
							Found = true;
							break;
						}

						PreviousSpace = CurrentSpace;
					}
				} while (Found);
			}

			do
			{
				Found = false;
				// Remove empty spaces.
				foreach (var Space in AvailableSpaces.Where(Space => (Space.Length == 0)))
				{
					AvailableSpaces.Remove(Space);
					Found = true;
					break;
				}
			} while (Found);
		}

		public SpaceAssigner1D Substract(Space SpaceToSubstract)
		{
			bool Continue;
			int MaxSteps = AvailableSpaces.Count + 10;
			do {
				//Console.WriteLine(this);
				Continue = false;
				if (AvailableSpaces.Count > 0)
				{
					foreach (var Space in AvailableSpaces)
					{
						if (SpaceToSubstract.Intersects(Space))
						{
							//Console.WriteLine("Intersects: " + SpaceToSubstract + " (*) " + Space);
							var LeftSpaces = Space - SpaceToSubstract;
							AvailableSpaces.Remove(Space);
							foreach (var LeftSpace in LeftSpaces)
							{
								AvailableSpaces.Add(LeftSpace);
							}
							Continue = true;
							break;
						}
					}
				}
				if (MaxSteps-- <= 0) throw(new Exception("Infinite loop detected"));
			} while (Continue);

			PerformCombine();

			return this;
		}

		public bool Intersects(Space SpaceToCheckIfIntersects)
		{
			foreach (var Space in AvailableSpaces)
			{
				if (SpaceToCheckIfIntersects.Intersects(Space))
				{
					return true;
				}
			}
			return false;
		}

		public SpaceAssigner1D AddAvailable(Space SpaceToAdd)
		{
			if (this.Intersects(SpaceToAdd))
			{
				//throw (new NotImplementedException("Overlapping not implemented yet!"));
				this.Substract(SpaceToAdd);
			}

			AvailableSpaces.Add(SpaceToAdd);
			PerformCombine();

			return this;
		}

		public SpaceAssigner1D AddAvailable(long Min, long Max)
		{
			return AddAvailable(new Space(Min, Max));
		}

		public SpaceAssigner1D AddAllPositiveAvailable()
		{
			return AddAvailable(new Space(0, long.MaxValue));
		}

		/**
		 * Finds an Available Space Chunk that has a length greater or equals to
		 * the specified one.
		 */
		public Space Allocate(long RequiredLength)
		{
			Space FoundSpace = null;

			foreach (var Space in AvailableSpaces)
			{
				if (Space.Length >= RequiredLength)
				{
					FoundSpace = Space;
					// Greedy. First found, first used.
					break;
				}
			}

			if (FoundSpace == null)
			{
				throw(new Exception("Can't allocate a space of length " + RequiredLength + "."));
			}

			AvailableSpaces.Remove(FoundSpace);
			var SpaceLeft = new Space(FoundSpace.Min, FoundSpace.Min + RequiredLength);
			var SpaceRight = new Space(FoundSpace.Min + RequiredLength, FoundSpace.Max);
			AvailableSpaces.Add(SpaceRight);

			PerformCombine();

			return SpaceLeft;
		}

		/// <summary>
		/// @TODO In order to avoid a greedy behaviour, Allocate[] should have the code and
		///       Allocate should use this function with a single item.
		/// </summary>
		/// <param name="RequiredLengths"></param>
		/// <returns></returns>
		public Space[] Allocate(long[] RequiredLengths)
		{
			var Spaces = new Space[RequiredLengths.Length];
			for (int n = 0; n < RequiredLengths.Length; n++)
			{
				Spaces[n] = Allocate(RequiredLengths[n]);
			}
			return Spaces;
		}

		public override string ToString()
		{
			return "SpaceAssigner1D(" + String.Join(",", AvailableSpaces) + ")";
		}

	}
}
