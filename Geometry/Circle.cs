using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Geometry
{
  /// <summary>
  /// Simple circle on a 2d plane.
  /// </summary>
  public class Circle
  {
    /// <summary>
    /// Create a simple circle
    /// </summary>
    public Circle(Point center, double radius)
    {
      Center = center;
      Radius = radius;
    }

    /// <summary>
    /// From 3 points, draw a circle.
    /// </summary>
    public Circle(Point p1, Point p2, Point p3)
    {
      double a = p1.X;
      double b = p1.Y;
      double c = p2.X;
      double d = p2.Y;
      double e = p3.X;
      double f = p3.Y;
      double h, k;
      //k = (1/2)((a²+b²)(e-c) + (c²+d²)(a-e) + (e²+f²)(c-a)) / (b(e-c)+d(a-e)+f(c-a))
      double num = Chunk(a, b, e, c) + Chunk(c, d, a, e) + Chunk(e, f, c, a);
      double denom = 2 * (b * (e - c) + d * (a - e) + f * (c - a));
      k = num / denom;

      //h = (1/2)((a²+b²)(f-d) + (c²+d²)(b-f) + (e²+f²)(d-b)) / (a(f-d)+c(b-f)+e(d-b))
      num = Chunk(a, b, f, d) + Chunk(c, d, b, f) + Chunk(e, f, d, b);
      denom = 2 * (a * (f - d) + c * (b - f) + e * (d - b));
      h = num / denom;

      Center = new Point(h, k);
      Radius = Math.Sqrt(Sq(a - h) + Sq(b - k));
    }

    private static double Chunk(double x, double y, double d1, double d2)
    {
      return (x * x + y * y) * (d1 - d2);
    }

    private static double Sq(double lf) { return lf * lf; }


    /// <summary>
    /// Center of the circle
    /// </summary>
    public Point Center { get; protected set; }

    /// <summary>
    /// Radius of the circle
    /// </summary>
    public double Radius { get; protected set; }
  }
}
