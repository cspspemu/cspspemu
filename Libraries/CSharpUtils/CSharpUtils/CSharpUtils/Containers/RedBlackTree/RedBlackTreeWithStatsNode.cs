using System;
using CountType = System.Int32;

namespace CSharpUtils.Containers.RedBlackTree
{
    public partial class RedBlackTreeWithStats<TElement>
    {
        static void DebugAssert(bool assertion)
        {
            if (!assertion) throw new InvalidOperationException();
        }

        static void Assert(bool assertion)
        {
            DebugAssert(assertion);
        }

        internal enum Color
        {
            Red,
            Black
        }

        /// <summary>
        /// 
        /// </summary>
        public class Node
        {
            internal Node _LeftNode;
            internal Node _RightNode;
            internal Node _ParentNode;

            internal int ChildCountLeft;
            internal int ChildCountRight;

            internal TElement Value;
            internal Color Color;

            internal Node LeftNode
            {
                get => _LeftNode;
                set
                {
                    _LeftNode = value;
                    if (value != null) value._ParentNode = this;
                }
            }

            internal Node RightNode
            {
                get => _RightNode;
                set
                {
                    _RightNode = value;
                    if (value != null) value._ParentNode = this;
                }
            }

            internal Node ParentNode => _ParentNode;

            internal Node RootNode
            {
                get
                {
                    var current = this;
                    while (current.ParentNode != null) current = current.ParentNode;
                    return current;
                }
            }

            internal int DebugValidateStatsNodeSubtree()
            {
                int totalChildCountLeft = 0;
                var totalChildCountRight = 0;
                if (LeftNode != null) totalChildCountLeft = LeftNode.DebugValidateStatsNodeSubtree();
                if (RightNode != null) totalChildCountRight = RightNode.DebugValidateStatsNodeSubtree();
                DebugAssert(ChildCountLeft == totalChildCountLeft);
                DebugAssert(ChildCountRight == totalChildCountRight);
                return 1 + ChildCountLeft + ChildCountRight;
            }

            internal void UpdateCurrentAndAncestors(int countIncrement)
            {
                //return;

                var previousNode = this;
                var currentNode = ParentNode;

                while (currentNode != null)
                {
                    // @TODO: Change
                    // prev.isLeftNode

                    if (previousNode.IsLeftNode)
                    {
                        currentNode.ChildCountLeft += countIncrement;
                    }
                    else if (previousNode.IsRightNode)
                    {
                        currentNode.ChildCountRight += countIncrement;
                    }
                    else
                    {
                        Assert(false);
                    }
                    previousNode = currentNode;
                    currentNode = currentNode.ParentNode;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Format(
                    "RedBlackTreeWithStats.Node(Value={0}, Color={1}, ChildCountLeft={2}, ChildCountRight={3})",
                    Value,
                    Enum.GetName(typeof(Color), Color),
                    ChildCountLeft,
                    ChildCountRight
                );
            }

            internal void PrintTree(Node markNode = null, int level = 0, string label = "L")
            {
                var info = "";
                if (this == markNode) info = " (mark)";

                Console.WriteLine(
                    "{0}- {1}:{2}{3}",
                    new string(' ', level * 2),
                    label, this, info
                );

                LeftNode?.PrintTree(markNode, level + 1, "L");
                RightNode?.PrintTree(markNode, level + 1, "R");
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

            internal Node RotateL() => RotateLeft();

            internal Node RotateR() => RotateRight();

            internal Node RotateRight()
            {
                Assert(LeftNode != null);

                if (IsLeftNode)
                {
                    ParentNode.LeftNode = LeftNode;
                }
                else
                {
                    ParentNode.RightNode = LeftNode;
                }

                ChildCountLeft = LeftNode.ChildCountRight;
                LeftNode.ChildCountRight = ChildCountLeft + ChildCountRight + 1;

                var tempNode = LeftNode.RightNode;
                LeftNode.RightNode = this;
                LeftNode = tempNode;

                return this;
            }

            Node RotateLeft()
            {
                Assert(RightNode != null);

                // sets _right._parent also
                if (IsLeftNode)
                {
                    ParentNode.LeftNode = RightNode;
                }
                else
                {
                    ParentNode.RightNode = RightNode;
                }

                ChildCountRight = RightNode.ChildCountLeft;
                RightNode.ChildCountLeft = ChildCountLeft + ChildCountRight + 1;

                var tempNode = RightNode.LeftNode;
                RightNode.LeftNode = this;
                RightNode = tempNode;

                return this;
            }

            internal void SetColor(Node end)
            {
                //writefln("Updating tree...");
                // test against the marker node
                if (ParentNode == end)
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

                var cur = this;

                while (true)
                {
                    // because root is always black, _parent._parent always exists
                    if (cur.ParentNode.IsLeftNode)
                    {
                        // parent is left node, y is 'uncle', could be null
                        var y = cur._ParentNode._ParentNode._RightNode;
                        if (y != null && y.Color == Color.Red)
                        {
                            cur._ParentNode.Color = Color.Black;
                            y.Color = Color.Black;
                            cur = cur._ParentNode._ParentNode;
                            if (cur._ParentNode == end)
                            {
                                // root node
                                cur.Color = Color.Black;
                                break;
                            }
                            // not root node
                            cur.Color = Color.Red;
                            if (cur._ParentNode.Color == Color.Black)
                            {
                                // satisfied, exit the loop
                                break;
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
                        var y = cur._ParentNode._ParentNode._LeftNode;
                        if (y != null && y.Color == Color.Red)
                        {
                            cur._ParentNode.Color = Color.Black;
                            y.Color = Color.Black;
                            cur = cur._ParentNode._ParentNode;
                            if (cur._ParentNode == end)
                            {
                                // root node
                                cur.Color = Color.Black;
                                break;
                            }
                            // not root node
                            cur.Color = Color.Red;

                            if (cur._ParentNode.Color == Color.Black)
                            {
                                // satisfied, exit the loop
                                break;
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
                    var y = NextNode;
                    var yp = y._ParentNode;
                    var yl = y._LeftNode;
                    var yr = y._RightNode;
                    var yChildCountLeft = y.ChildCountLeft;
                    var yChildCountRight = y.ChildCountRight;
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
                    y.RightNode = _RightNode == y ? this : _RightNode;
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

                    ChildCountLeft = yChildCountLeft;
                    ChildCountRight = yChildCountRight;

                    //
                    // set return value
                    //
                    ret = y;
                }

                UpdateCurrentAndAncestors(-1);

                // if this has less than 2 children, remove it
                x = _LeftNode ?? _RightNode;

                // remove this from the tree at the end of the procedure
                var removeThis = false;
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
                                    // ReSharper disable once PossibleNullReferenceException
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
                                x._ParentNode.RotateR();
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
                                    w.RotateL();
                                    w = x._ParentNode._LeftNode;
                                }

                                w.Color = x._ParentNode.Color;
                                x._ParentNode.Color = Color.Black;
                                w._LeftNode.Color = Color.Black;
                                x._ParentNode.RotateR();
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

            internal Node RightMostNode => RightNode == null ? this : RightNode.RightMostNode;

            internal Node NextNode
            {
                get
                {
                    var node = this;
                    if (node._RightNode == null)
                    {
                        while (!node.IsLeftNode) node = node._ParentNode;
                        return node._ParentNode;
                    }
                    return node._RightNode.LeftMostNode;
                }
            }

            internal Node PreviousNode
            {
                get
                {
                    var n = this;
                    if (n.LeftNode == null)
                    {
                        while (n.IsLeftNode) n = n._ParentNode;
                        return n._ParentNode;
                    }
                    return n.LeftNode.RightMostNode;
                }
            }

            internal Node Clone()
            {
                var that = new Node();
                that.Value = Value;
                that.Color = Color;
                that.ChildCountLeft = ChildCountLeft;
                that.ChildCountRight = ChildCountRight;
                if (_LeftNode != null) that.LeftNode = _LeftNode.Clone();
                if (_RightNode != null) that.RightNode = _RightNode.Clone();
                return that;
            }
        }
    }
}