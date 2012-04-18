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
      var PQueue = new SkipList<Point>(PointComparer.Instance);
      foreach (var p in points) PQueue.Add(p);

      var result = new HalfEdgeStructure();

      //until the queue is empty...
      while (PQueue.Count > 0)
      {
        //dequeue
        Point item = PQueue.First();
        PQueue.Remove(item);

        var newFace = new Face(item);
        result.Faces.Add(newFace);
      }


      return result;
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
