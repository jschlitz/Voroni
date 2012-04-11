using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Voroni
{
  public class Polygon
  {
    public Polygon(Point[] verticies)
    {
      if (verticies.Length < 3)
        throw new ArgumentException("Can't make a polygon with less than 3 points.");
      Verticies = new Point[verticies.Length];
      Array.Copy(verticies, Verticies, verticies.Length);
    }


    /// <summary>
    /// The verticies.
    /// </summary>
    public Point[] Verticies { get; protected set; }

    /// <summary>
    /// Does this polygon contain this point?
    /// </summary>
    public bool Contains(Point p0)
    {
      //edges?
      var sides = new List<LineSegment>(Verticies.Length);
      for (int i = 0; i < Verticies.Length; i++)
      {
        sides.Add(new LineSegment(Verticies[i], Verticies[(i + 1) % Verticies.Length]));
        
        //on edge?
        if (sides[i].Intersects(p0)) return true;
      }

      //find a decent line, count crossings.
      var p1 = new Point(Verticies.Max(p => p.X) + 1.0, p0.Y);

      int crossings = 0;
      var check = new LineSegment(p0, p1);
      Point crossesAt;
      foreach (var edge in sides)
      {
        //if we hit a vertex, it only counts if the other point is _below_ This makes concave polygons work out.
        if (check.GetIntersection(edge, out crossesAt))
        {
          if (crossesAt == edge.P1)
            crossings += (edge.P2.Y < edge.P1.Y) ? 1 : 0;
          else if (crossesAt == edge.P2)
            crossings += (edge.P1.Y < edge.P2.Y) ? 1 : 0;
          else
            crossings++;
        }
      }

      return crossings % 2 != 0;
    }

    public void Translate(Vector v)
    {
      for (int i = 0; i < Verticies.Length; i++)
        Verticies[i] = Point.Add(Verticies[i], v);
    }
  }
}
