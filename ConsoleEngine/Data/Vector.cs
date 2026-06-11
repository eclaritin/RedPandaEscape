using System.Diagnostics.CodeAnalysis;

using ConsoleEngine.Services;

namespace ConsoleEngine.Data
{
  public struct Vector
  {
    // Props
    public int X;
    public int Y;
    public Vector Value { get { return new Vector(this.X, this.Y); } }

    // Constructors
    public Vector(int x = 0, int y = 0)
    {
      X = x;
      Y = y;
    }
    public Vector(Vector v)
    {
      this.X = v.X;
      this.Y = v.Y;
    }

    // Methods

    public Vector Abs()
    {
      return new Vector(Math.Abs(this.X), Math.Abs(this.Y));
    }

    // Magic Methods (that's the python term for this haha)
    public static Vector operator +(Vector a, Vector b)
    {
      return new Vector(a.X + b.X, a.Y + b.Y);
    }
    public static Vector operator -(Vector a, Vector b)
    {
      return new Vector(a.X - b.X, a.Y - b.Y);
    }
    public static Vector operator *(Vector a, Vector b)
    {
      return new Vector(a.X * b.X, a.Y * b.Y);
    }
    public static Vector operator /(Vector a, Vector b)
    {
      return new Vector(a.X / b.X, a.Y / b.Y);
    }
    public static Vector operator %(Vector a, Vector b)
    {
      return new Vector(a.X % b.X, a.Y % b.Y);
    }

    public override string ToString()
    {
      return $"Vector({this.X}, {this.Y})";
    }
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
      // make sure obj isnt null
      if (obj == null) { return false; }

      // if obj is a vector
      if (obj.GetType() == typeof(Vector))
      {
        Vector v = (Vector)obj;
        return (this.X == v.X && this.Y == v.Y);
      }

      // if obj is an int array
      if (obj.GetType() == (new int[2].GetType()))
      {
        int[] a = (int[])obj;
        if (a.Length > 2) { return false; }
        return (this.X == a[0] && this.Y == a[1]);
      }

      // return false if none of the checks apply
      return false;
    }

    // Static Methods & Vars

    public static Vector Null { get { return new Vector(-1, -1); } }
    public static Vector Center { get { return new Vector(Game.Size[0] / 2, Game.Size[1] / 2); } }
  }
}