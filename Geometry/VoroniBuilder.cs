using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using DataStructures;
using System.Linq.Expressions;

namespace Geometry
{
  /// <summary>
  /// Builds a Voroni diagram from a set of points
  /// </summary>
  public static class VoroniBuilder
  {
    /// <summary>
    /// What is a more sensible place for this? HalfEdgeStructure itself? Maybe.
    /// </summary>
    public static HalfEdgeStructure MakeDiagram(IList<Point> points)
    {
      //fill the queue with the initial point events.
      //TODO: this probably changes to something smarter --  we differentiate face events from circle events
      var pQueue = new SkipList<Point>(PointComparer.Instance);
      var parabolic = new ParabolicComparer();
      var status = new SkipList<Point>(parabolic);
      foreach (var p in points) pQueue.Add(p);

      var result = new HalfEdgeStructure();

      //until the queue is empty...
      while (pQueue.Count > 0)
      {
        //dequeue
        Point item = pQueue.First();
        pQueue.Remove(item);

        //new face
        var newFace = new Face(item);
        result.Faces.Add(newFace);

        //add to status
        parabolic.Directix = item.Y;
      }


      return result;
    }

    private class ParabolicComparer : IComparer<Point>
    {
      /// <summary>
      /// Directix line of the parabolas
      /// </summary>
      public double Directix { get; set; }

      public int Compare(Point x, Point y)
      {
        throw new NotImplementedException();
      }
    }

    private class StatusStructure : SkipList<Parabola>
    {
      //When I extend this for Voroni, I'll have to more explicitly control 
      //insertions and deletions. It depends on finding node triplets that
      //are in the right place. I can't just rely on a simple comparer to keep the
      //right order.
      /// <summary>
      /// Find the node for which c.Compare() returns 0. If c lt 0, it assumes the 
      /// tested node is too "small." c gt 0 the tested node is too "big."
      /// </summary>
      public SkipNode<Parabola> FindNode(Func<Parabola, SkipNode<Parabola>, int> c, Parabola item)
      {
        var current = _Root;

        for (int i = current.Height - 1; i >= 0; i--)
        {

          //go either til we find it, or the last one at this level that is less than item
          while (current[i] != null && (c(item, current[i]) > 0))
          {
            current = current[i];
          }
        }

        if (current != _Root && c(item, current) == 0)
          return current;
        else
          return null;
      }
    }

    /// <summary>
    /// Compare 2 points by their Y value, high to low. if they are equal, compare by X, low to high.
    /// </summary>
    private class PointComparer : IComparer<Point>
    {
      public static readonly PointComparer Instance = new PointComparer();

      public int Compare(Point x, Point y)
      {
        double result = -1*(x.Y - y.Y);
        if (result == 0)
          result = x.X - y.X;//note this is reversed.

        if (result > 0) 
          return 1;
        else if (result < 0) 
          return -1;
        else 
          return 0;
      }
    }
  }
}
