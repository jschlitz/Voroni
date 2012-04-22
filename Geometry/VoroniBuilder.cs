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
      var status = new StatusStructure();
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
        status.Directix = item.Y;
      }


      return result;
    }


    private class StatusStructure : SkipList<Point>
    {
      //When I extend this for Voroni, I'll have to more explicitly control 
      //insertions and deletions. It depends on finding node triplets that
      //are in the right place. I can't just rely on a simple comparer to keep the
      //right order.
      /// <summary>
      /// Find the node for which c.Compare() returns 0. If c lt 0, it assumes the 
      /// tested node is too "small." c gt 0 the tested node is too "big."
      /// </summary>
      public SkipNode<Parabola> FindNode(Point item)
      {
        var current = _Root;

        for (int i = current.Height - 1; i >= 0; i--)
        {

          //go either til we find it, or the last one at this level that is less than item
          while (current[i] != null && (Checker(item, current[i]) > 0))
          {
            current = current[i];
          }
        }

        if (current != _Root && Checker(item, current) == 0)
          return current;
        else
          return null;
      }

      /// <summary>
      /// The sweep line
      /// </summary>
      public double Directix { get; set; }

      /// <summary>
      /// does item bisect this this parabola (0), one before it (-1)  or after (1)?
      /// </summary>
      /// <returns></returns>
      private int Checker(Point item, SkipNode<Point> n)
      {
        var nParabola = new Parabola(n.Value, Directix);

        double lBound;
        //if this is the first item, let -infinity be lower bound
        //otherwise find the intersection w. previous node
        if (n.Previous == _Root)
          lBound = double.NegativeInfinity;
        else
        {
          var prev = new Parabola(n.Previous.Value, Directix);
          var intersections = nParabola.Intersect(prev);
          if (Parabola.IsNoIntersection(intersections.Item2))
          {
            //0-assertfail
            System.Diagnostics.Debug.Assert(!Parabola.IsNoIntersection(intersections.Item1), 
              String.Format("No interesections between consecutive items: {0} and {1}", prev, nParabola);
            
            lBound = intersections.Item1.X;
          }
          else //2, largest 
            lBound = Math.Max(intersections.Item1.X, intersections.Item2.X);
        }
        //item < lowerbound? -1
        if(item.X < lBound)
          return -1;

        //last item? upper bound is +inifinity
        //otherwise intersection w/ next parabola
        double uBound;
        if(n.Next() == null)
          uBound = double.PositiveInfinity;
        else
        {
          var next = new Parabola(n.Next().Value, Directix);
          var intersections = nParabola.Intersect(next);
          if (Parabola.IsNoIntersection(intersections.Item2))
          {
            //0-assertfail
            System.Diagnostics.Debug.Assert(!Parabola.IsNoIntersection(intersections.Item1), 
              String.Format("No interesections between consecutive items: {0} and {1}", nParabola, next);
            
            uBound = intersections.Item1.X;
          }
          else
            uBound = Math.Min(intersections.Item1.X, intersections.Item2.X);
        }
        //item >= upperbound? +1
        if (item.X >= uBound)
          return 1;

        //we got here? this must be the place.
        return 0;
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
