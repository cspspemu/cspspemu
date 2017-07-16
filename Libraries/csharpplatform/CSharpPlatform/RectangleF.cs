using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlatform
{
    public class RectangleF
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Width;
        public readonly float Height;

        public float Left
        {
            get { return X; }
        }

        public float Right
        {
            get { return X + Width; }
        }

        public float Top
        {
            get { return Y; }
        }

        public float Bottom
        {
            get { return Y + Height; }
        }

        public RectangleF(float X, float Y, float Width, float Height)
        {
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
        }

        public float[] GetFloat2TriangleStripCoords()
        {
            return new float[]
            {
                this.Left, this.Top,
                this.Right, this.Top,
                this.Left, this.Bottom,
                this.Right, this.Bottom,
            };
        }

        public static RectangleF FromCoords(float Left, float Top, float Right, float Bottom)
        {
            return new RectangleF(Left, Top, Right - Left, Bottom - Top);
        }

        public RectangleF VFlip()
        {
            return FromCoords(this.Left, this.Bottom, this.Right, this.Top);
        }

        public override string ToString()
        {
            return String.Format("RectangleF({0}, {1}, {2}, {3})", Left, Top, Width, Height);
        }
    }
}