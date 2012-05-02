using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CountType = System.Int32;

namespace CSharpUtils.Containers.RedBlackTree
{
	public partial class RedBlackTreeWithStats<TElement>
	{
		static void DebugAssert(bool Assertion)
		{
			if (!Assertion) throw (new InvalidOperationException());
		}

		static void Assert(bool Assertion)
		{
			DebugAssert(Assertion);
		}

		internal enum Color
		{
			Red, Black
		}

		public class Node
		{
			internal Node _LeftNode;
			internal Node _RightNode;
			internal Node _ParentNode;

			internal CountType ChildCountLeft;
			internal CountType ChildCountRight;

			internal TElement Value;
			internal Color Color;

			internal Node LeftNode
			{
				get
				{
					return _LeftNode;
				}
				set
				{
					_LeftNode = value;
					if (value != null) value._ParentNode = this;
				}
			}

			internal Node RightNode
			{
				get
				{
					return _RightNode;
				}
				set
				{
					_RightNode = value;
					if (value != null) value._ParentNode = this;
				}
			}

			internal Node ParentNode
			{
				get
				{
					return _ParentNode;
				}
			}

			internal Node RootNode
			{
				get
				{
					var Current = this;
					while (Current.ParentNode != null) Current = Current.ParentNode;
					return Current;
				}
			}

			internal CountType DebugValidateStatsNodeSubtree()
			{
				CountType TotalChildCountLeft = 0;
				CountType TotalChildCountRight = 0;
				if (LeftNode != null) TotalChildCountLeft = LeftNode.DebugValidateStatsNodeSubtree();
				if (RightNode != null) TotalChildCountRight = RightNode.DebugValidateStatsNodeSubtree();
				DebugAssert(ChildCountLeft == TotalChildCountLeft);
				DebugAssert(ChildCountRight == TotalChildCountRight);
				return 1 + this.ChildCountLeft + this.ChildCountRight;
			}

			internal void UpdateCurrentAndAncestors(CountType CountIncrement)
			{
				//return;

				var PreviousNode = this;
				var CurrentNode = this.ParentNode;

				while (CurrentNode != null)
				{
					// @TODO: Change
					// prev.isLeftNode

					if (PreviousNode.IsLeftNode)
					{
						CurrentNode.ChildCountLeft += CountIncrement;
					}
					else if (PreviousNode.IsRightNode)
					{
						CurrentNode.ChildCountRight += CountIncrement;
					}
					else
					{
						Assert(false);
					}
					PreviousNode = CurrentNode;
					CurrentNode = CurrentNode.ParentNode;
				}
			}

			public override string ToString()
			{
				return String.Format(
					"RedBlackTreeWithStats.Node(Value={0}, Color={1}, ChildCountLeft={2}, ChildCountRight={3})",
					Value,
					Enum.GetName(typeof(Color), Color),
					ChildCountLeft,
					ChildCountRight
				);
			}

			internal void PrintTree(Node MarkNode = null, int Level = 0, String Label = "L")
			{
				string Info = "";
				if (this == MarkNode) Info = " (mark)";

				Console.WriteLine(
					"{0}- {1}:{2}{3}",
					new String(' ', Level * 2),
					Label, this, Info
				);

				if (LeftNode != null)
				{
					LeftNode.PrintTree(MarkNode, Level + 1, "L");
				}

				if (RightNode != null)
				{
					RightNode.PrintTree(MarkNode, Level + 1, "R");
				}
			}

			internal bool IsLeftNode
			{
				get
				{
					Assert(ParentNode != null);
					return this == ParentNode.LeftNode;
				}
			}

			internal bool IsRightNode
			{
				get
				{
					Assert(ParentNode != null);
					return this == ParentNode.RightNode;
				}
			}

			internal Node rotateL()
			{
				return RotateLeft();
			}

			internal Node rotateR()
			{
				return RotateRight();
			}

			internal Node RotateRight()
			{
				Assert(LeftNode != null);

				if (this.IsLeftNode)
				{
					ParentNode.LeftNode = this.LeftNode;
				}
				else
				{
					ParentNode.RightNode = this.LeftNode;
				}

				this.ChildCountLeft = this.LeftNode.ChildCountRight;
				LeftNode.ChildCountRight = this.ChildCountLeft + this.ChildCountRight + 1;

				Node TempNode = LeftNode.RightNode;
				LeftNode.RightNode = this;
				LeftNode = TempNode;

				return this;
			}

			Node RotateLeft()
			{
				Assert(RightNode != null);

				// sets _right._parent also
				if (this.IsLeftNode)
				{
					ParentNode.LeftNode = this.RightNode;
				}
				else
				{
					ParentNode.RightNode = this.RightNode;
				}

				this.ChildCountRight = this.RightNode.ChildCountLeft;
				RightNode.ChildCountLeft = this.ChildCountLeft + this.ChildCountRight + 1;

				Node TempNode = RightNode.LeftNode;
				RightNode.LeftNode = this;
				RightNode = TempNode;

				return this;
			}

			internal void SetColor(Node End)
			{
				//writefln("Updating tree...");
				// test against the marker node
				if (ParentNode == End)
				{
					//
					// this is the root node, color it black
					//
					Color = Color.Black;
					return;
				}

				if (ParentNode.Color != Color.Red)
				{
					return;
				}

				Node cur = this;

				while (true)
				{
					// because root is always black, _parent._parent always exists
					if (cur.ParentNode.IsLeftNode)
					{
						// parent is left node, y is 'uncle', could be null
						Node y = cur._ParentNode._ParentNode._RightNode;
						if (y != null && y.Color == Color.Red)
						{
							cur._ParentNode.Color = Color.Black;
							y.Color = Color.Black;
							cur = cur._ParentNode._ParentNode;
							if (cur._ParentNode == End)
							{
								// root node
								cur.Color = Color.Black;
								break;
							}
							else
							{
								// not root node
								cur.Color = Color.Red;
								if (cur._ParentNode.Color == Color.Black)
								{
									// satisfied, exit the loop
									break;
								}
							}
						}
						else
						{
							if (!cur.IsLeftNode) cur = cur._ParentNode.RotateLeft();
							cur._ParentNode.Color = Color.Black;
							cur = cur._ParentNode._ParentNode.RotateRight();
							cur.Color = Color.Red;
							// tree should be satisfied now
							break;
						}
					}
					else
					{
						// parent is right node, y is 'uncle'
						Node y = cur._ParentNode._ParentNode._LeftNode;
						if (y != null && y.Color == Color.Red)
						{
							cur._ParentNode.Color = Color.Black;
							y.Color = Color.Black;
							cur = cur._ParentNode._ParentNode;
							if (cur._ParentNode == End)
							{
								// root node
								cur.Color = Color.Black;
								break;
							}
							else
							{
								// not root node
								cur.Color = Color.Red;

								if (cur._ParentNode.Color == Color.Black)
								{
									// satisfied, exit the loop
									break;
								}
							}
						}
						else
						{
							if (cur.IsLeftNode)
							{
								cur = cur._ParentNode.RotateRight();
							}
							cur._ParentNode.Color = Color.Black;
							cur = cur._ParentNode._ParentNode.RotateLeft();
							cur.Color = Color.Red;
							// tree should be satisfied now
							break;
						}
					}
				}

			}

			internal void SetParentThisChild(Node newThis)
			{
				if (IsLeftNode)
				{
					_ParentNode.LeftNode = newThis;
				}
				else
				{
					_ParentNode.RightNode = newThis;
				}
			}

			internal Node NonSynchronizedRemove(Node end)
			{
				//
				// remove this node from the tree, fixing the color if necessary.
				//
				Node x;
				Node ret;

				if (_LeftNode == null || _RightNode == null)
				{
					//static if (hasStats) updateCurrentAndAncestors(-1);
					ret = NextNode;
				}
				else
				{
					//
					// normally, we can just swap this node's and y's value, but
					// because an iterator could be pointing to y and we don't want to
					// disturb it, we swap this node and y's structure instead.  This
					// can also be a benefit if the value of the tree is a large
					// struct, which takes a long time to copy.
					//
					Node yp, yl, yr;
					Node y = NextNode;
					yp = y._ParentNode;
					yl = y._LeftNode;
					yr = y._RightNode;
					var y_childCountLeft = y.ChildCountLeft;
					var y_childCountRight = y.ChildCountRight;
					var yc = y.Color;
					var isyleft = y.IsLeftNode;

					//
					// replace y's structure with structure of this node.
					//
					SetParentThisChild(y);
					//
					// need special case so y doesn't point back to itself
					//
					y.LeftNode = _LeftNode;
					if (_RightNode == y)
					{
						y.RightNode = this;
					}
					else
					{
						y.RightNode = _RightNode;
					}
					y.Color = Color;

					y.ChildCountLeft = ChildCountLeft;
					y.ChildCountRight = ChildCountRight;

					//
					// replace this node's structure with structure of y.
					//
					LeftNode = yl;
					RightNode = yr;
					if (_ParentNode != y)
					{
						if (isyleft)
						{
							yp.LeftNode = this;
						}
						else
						{
							yp.RightNode = this;
						}
					}
					Color = yc;

					ChildCountLeft = y_childCountLeft;
					ChildCountRight = y_childCountRight;

					//
					// set return value
					//
					ret = y;
				}

				UpdateCurrentAndAncestors(-1);

				// if this has less than 2 children, remove it
				if (_LeftNode != null)
				{
					x = _LeftNode;
				}
				else
				{
					x = _RightNode;
				}

				// remove this from the tree at the end of the procedure
				bool removeThis = false;
				if (x == null)
				{
					// pretend this is a null node, remove this on finishing
					x = this;
					removeThis = true;
				}
				else if (IsLeftNode)
				{
					_ParentNode.LeftNode = x;
				}
				else
				{
					_ParentNode.RightNode = x;
				}

				// if the color of this is black, then it needs to be fixed
				if (Color == Color.Black)
				{
					// need to recolor the tree.
					while (x._ParentNode != end && x.Color == Color.Black)
					{
						if (x.IsLeftNode)
						{
							// left node
							var w = x._ParentNode._RightNode;
							if (w.Color == Color.Red)
							{
								w.Color = Color.Black;
								x._ParentNode.Color = Color.Red;
								x._ParentNode.RotateLeft();
								w = x._ParentNode._RightNode;
							}

							var wl = w.LeftNode;
							var wr = w.RightNode;

							if (
								(wl == null || wl.Color == Color.Black) &&
								(wr == null || wr.Color == Color.Black)
							)
							{
								w.Color = Color.Red;
								x = x._ParentNode;
							}
							else
							{
								if (wr == null || wr.Color == Color.Black)
								{
									// wl cannot be null here
									wl.Color = Color.Black;
									w.Color = Color.Red;
									w.RotateRight();
									w = x._ParentNode._RightNode;
								}

								w.Color = x._ParentNode.Color;
								x._ParentNode.Color = Color.Black;
								w._RightNode.Color = Color.Black;
								x._ParentNode.RotateLeft();
								x = end.LeftNode; // x = root
							}
						}
						else
						{
							// right node
							var w = x._ParentNode._LeftNode;
							if (w.Color == Color.Red)
							{
								w.Color = Color.Black;
								x._ParentNode.Color = Color.Red;
								x._ParentNode.rotateR();
								w = x._ParentNode._LeftNode;
							}
							var wl = w.LeftNode;
							var wr = w.RightNode;
							if (
								(wl == null || wl.Color == Color.Black) &&
								(wr == null || wr.Color == Color.Black)
							)
							{
								w.Color = Color.Red;
								x = x._ParentNode;
							}
							else
							{
								if (wl == null || wl.Color == Color.Black)
								{
									// wr cannot be null here
									wr.Color = Color.Black;
									w.Color = Color.Red;
									w.rotateL();
									w = x._ParentNode._LeftNode;
								}

								w.Color = x._ParentNode.Color;
								x._ParentNode.Color = Color.Black;
								w._LeftNode.Color = Color.Black;
								x._ParentNode.rotateR();
								x = end.LeftNode; // x = root
							}
						}
					}
					x.Color = Color.Black;
				}

				if (removeThis)
				{
					//
					// clear this node out of the tree
					//
					if (IsLeftNode)
					{
						_ParentNode.LeftNode = null;
					}
					else
					{
						_ParentNode.RightNode = null;
					}
				}

				return ret;
			}

			// Tail recursion.
			internal Node LeftMostNode
			{
				get
				{
					/*
					var result = this;
					while (result._LeftNode != null) result = result._LeftNode;
					return result;
					*/
					//if (_LeftNode == null) return this;
					//return _LeftNode.LeftMostNode;
					return (LeftNode == null) ? this : LeftNode.LeftMostNode;
				}
			}

			internal Node RightMostNode
			{
				get
				{
					/*
					var result = this;
					while (result._RightNode != null) result = result._RightNode;
					return result;
					*/
					//if (_RightNode == null) return this;
					//return _RightNode.RightMostNode;
					return (RightNode == null) ? this : RightNode.RightMostNode;
				}
			}

			internal Node NextNode
			{
				get
				{
					Node Node = this;
					if (Node._RightNode == null)
					{
						while (!Node.IsLeftNode) Node = Node._ParentNode;
						return Node._ParentNode;
					}
					else
					{
						return Node._RightNode.LeftMostNode;
					}
				}
			}

			internal Node PreviousNode
			{
				get
				{
					Node n = this;
					if (n.LeftNode == null)
					{
						while (n.IsLeftNode) n = n._ParentNode;
						return n._ParentNode;
					}
					else
					{
						return n.LeftNode.RightMostNode;
					}
				}
			}

			internal Node Clone()
			{
				Node that = new Node();
				that.Value = this.Value;
				that.Color = this.Color;
				that.ChildCountLeft = this.ChildCountLeft;
				that.ChildCountRight = this.ChildCountRight;
				if (this._LeftNode != null) that.LeftNode = _LeftNode.Clone();
				if (this._RightNode != null) that.RightNode = _RightNode.Clone();
				return that;
			}
		}
	}
}
