using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Text;

namespace Geometry
{
  public class Parabola
  {
    public Parabola(Point p, double yLine)
    {
      Focus = p;
      Directix = yLine;
    }

    public override string ToString()
    {
      return Focus.X.ToString("f2") + "," + Focus.Y.ToString("f2") + "|" + Directix.ToString("f2");
    }

    public Point Focus { get; set; }
    public double Directix { get; set; }

    private bool Cannonicalize(out double a, out double b, out double c)
    {
      if (Directix == Focus.Y)
      {
        a = double.NaN;
        b = double.NaN;
        c = double.NaN;
        return false;
      }
      else
      {
        a = 1 / (2 * (Focus.Y - Directix));
        b = (-1 * Focus.X) / (Focus.Y - Directix);
        c = (Sq(Focus.X) + Sq(Focus.Y) - Sq(Directix)) / (2 * (Focus.Y - Directix));
        return true;
      }
    }

    /// <summary>
    /// For given x, get y.
    /// </summary>
    public double GetY(double x)
    {
      double a, b, c;
      if (!Cannonicalize(out a, out b, out c))
        return double.NaN;
      return a * x * x + b * x + c;
    }

    /// <summary>
    /// 0, 1 or 2 intersections between the 2 paraboloas
    /// </summary>
    public Tuple<Point, Point> Intersect(Parabola other)
    {

      //put in cannonical form
      double a1, a2, b1, b2, c1, c2;
      bool ok1 = Cannonicalize(out a1, out b1, out c1);
      bool ok2 = other.Cannonicalize(out a2, out b2, out c2);

      //some degenerate cases
      if (!ok1 && !ok2) return new Tuple<Point, Point>(NO_INTERSECTION, NO_INTERSECTION);
      if (ok1 && !ok2)
        return new Tuple<Point, Point>(CalcPoint(other.Focus.X, a1, b1, c1), NO_INTERSECTION);
      if (!ok1 && ok2)
        return new Tuple<Point, Point>(CalcPoint(Focus.X, a2, b2, c2), NO_INTERSECTION);

      //now subtract the cannonicals and put through quadratic
      Tuple<double, double> xes = Quadratic(a1 - a2, b1 - b2, c1 - c2);

      return new Tuple<Point, Point>(CalcPoint(xes.Item1, a1, b1, c1), CalcPoint(xes.Item2, a1, b1, c1));
    }

    private static Point CalcPoint(double x, double a1, double b1, double c1)
    {
      if (double.IsNaN(x))
        return NO_INTERSECTION;
      else
        return new Point(x, a1 * Sq(x) + b1 * x + c1);
    }

    public static readonly Point NO_INTERSECTION = new Point(double.NaN, double.NaN);

    /// <summary>
    /// Is this a pair of NaN?
    /// </summary>
    public static bool IsNoIntersection(Point p)
    {
      return double.IsNaN(p.X) && double.IsNaN(p.Y);
    }

    /// <summary>
    /// Perform quadratic on a,b,c. if a==0, solve for xb+c=0, and return NaN 
    /// for the 2nd. if a==b==0, NaN for both. 
    /// Also, don't consider imaginary solutions. if b*b &lt; 4*a*c, you get NaNs
    /// </summary>
    private Tuple<double, double> Quadratic(double a, double b, double c)
    {
      if (a == 0)
      {
        if (b == 0)
          return new Tuple<double, double>(double.NaN, double.NaN);
        else
          return new Tuple<double, double>(-1 * c / b, double.NaN);
      }
      else
      {
        var disc = b * b - 4 * a * c;
        if (disc < 0) return new Tuple<double, double>(double.NaN, double.NaN);
        disc = Math.Sqrt(disc);

        return new Tuple<double, double>((-1 * b + disc) / (2 * a), (-1 * b - disc) / (2 * a));
      }
    }

    private static double Sq(double lf) { return lf * lf; }
  }
}
