using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Voroni
{
  public class LineSegment
  {
    public LineSegment(Point p1, Point p2)
    {
      P1 = p1;
      P2 = p2;
    }

    public Point P1 { get; set; }
    public Point P2 { get; set; }
    public double Slope
    {
      get
      {
        var v = P1 - P2;
        return v.Y / v.X;
      }
    }

    public bool Intersects (Point p)
    {
      //less than 1, greater than the other, or on one of the endpoints
      int pc = PointComparer.Comparer.Compare(P1, p) * PointComparer.Comparer.Compare(P2, p);
      if (pc > 0)
        return false;
      else if (pc == 0)
        return true;


      Vector v1 = P1 - P2;
      Vector v2 = P1 - p;
      double lineDist = Math.Abs(Vector.CrossProduct(v1, v2) / v1.Length);

      return lineDist <= CLOSE_ENOUGH;
    }

    /// <summary>
    /// See if this line intersects with another
    /// </summary>
    public bool GetIntersection(LineSegment other, out Point result)
    {
      result = NO_INTERSECTION;
      /*Based on these equations:
       * Pa = P1 + ua*(P2-P1)
       * Pb = P3 + ub*(P4-P3)
       * then solving for Pa == Pb, which is a nasty bit a algerbra.
       */

      //for sanity
      var p3 = other.P1;
      var p4 = other.P2;
      
      double denom = (p4.Y - p3.Y) * (P2.X - P1.X) - (p4.X - p3.X) * (P2.Y - P1.Y);
      
      //0 means that they're parallel. They could still "intersect" if they are colinear...
      if (denom == 0)
      {
        if (Intersects(p3))
        {
          result = (PointComparer.Comparer.Compare(p3, p4) <= 0) ? p3 : p4;
          return true;
        }
        else
          return false;
      }

      double ua = ((p4.X - p3.X) * (P1.Y - p3.Y) - (p4.Y - p3.Y) * (P1.X - p3.X)) / denom;
      double ub = ((P2.X - P1.X) * (P1.Y - p3.Y) - (P2.Y - P1.Y) * (P1.X - p3.X)) / denom;

      if ((ua < 0) || (ua > 1) || (ub < 0) || (ub > 1))
        return false;
      else
      {
        result = new Point(P1.X + ua * (P2.X - P1.X), P1.Y + ua * (P2.Y - P1.Y));
        return true;
      }
    }

    public const double CLOSE_ENOUGH = 0.000001;
    public static readonly Point NO_INTERSECTION = new Point(double.NaN, double.NaN);
  }

  public class PointComparer : IComparer<Point>
  {
    public static readonly PointComparer Comparer = new PointComparer();

    public int Compare(Point x, Point y)
    {
      if (x.X != y.X)
        return x.X < y.X ? -1 : 1;
      else if (x.Y != y.Y)
        return x.Y < y.Y ? -1 : 1;
      else
        return 0;
    }
  }

}
