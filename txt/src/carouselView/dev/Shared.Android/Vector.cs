using Xamarin.Forms.Platform.Extensions;
using Int = System.Drawing;


namespace Xamarin.Forms.Platform
{
    internal struct Vector
    {
        public static explicit operator Vector(Int.Size size)
        {
            return new Vector(size.Width, size.Height);
        }
        public static explicit operator Vector(Int.Point point)
        {
            return new Vector(point.X, point.Y);
        }
        public static implicit operator Int.Point(Vector vector)
        {
            return new Int.Point(vector.X, vector.Y);
        }
        public static implicit operator Int.Size(Vector vector)
        {
            return new Int.Size(vector.X, vector.Y);
        }

        public static bool operator ==(Vector lhs, Vector rhs)
        {
            return lhs.X == rhs.X && lhs.Y == rhs.Y;
        }
        public static bool operator !=(Vector lhs, Vector rhs)
        {
            return !(lhs == rhs);
        }
        public static Int.Rectangle operator -(Int.Rectangle source, Vector vector) => source + -vector;
        public static Int.Rectangle operator +(Int.Rectangle source, Vector vector) =>
            new Int.Rectangle(source.Location + vector, source.Size);

        public static Vector operator -(Vector vector, Vector other) => vector + -other;
        public static Vector operator +(Vector vector, Vector other) =>
            new Vector(
                x: vector.X + other.X,
                y: vector.Y + other.Y
            );

        public static Int.Point operator -(Int.Point point, Vector delta) => point + -delta;
        public static Int.Point operator +(Int.Point point, Vector delta) =>
            new Int.Point(
                x: point.X + delta.X,
                y: point.Y + delta.Y
            );

        public static Vector operator -(Vector vector) => vector * -1;
        public static Vector operator *(Vector vector, int scaler) =>
            new Vector(
                x: vector.X * scaler,
                y: vector.Y * scaler
            );
        public static Vector operator /(Vector vector, int scaler) =>
            new Vector(
                x: vector.X / scaler,
                y: vector.Y / scaler
            );

        public static Vector operator *(Vector vector, double scaler) =>
            new Vector(
                x: (int)(vector.X * scaler),
                y: (int)(vector.Y * scaler)
            );
        public static Vector operator /(Vector vector, double scaler) => vector * (1 / scaler);

        internal static Vector Origin = new Vector(0, 0);
        internal static Vector XUnit = new Vector(1, 0);
        internal static Vector YUnit = new Vector(0, 1);

        #region Fields
        readonly int _x;
        readonly int _y;
        #endregion

        internal Vector(int x, int y)
        {
            _x = x;
            _y = y;
        }

        internal int X => _x;
        internal int Y => _y;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        internal Vector Clamp(Int.Rectangle bound)
        {
            return new Vector(
                x: X.Clamp(bound.Left, bound.Right),
                y: Y.Clamp(bound.Top, bound.Bottom)
            );
        }


        public override string ToString()
        {
            return $"{X},{Y}";
        }
    }
}
