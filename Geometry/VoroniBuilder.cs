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

    public class Triple  
    {
      public Point? Left { get; set; }
      public Point? Right { get; set; }
      public Point Center { get; set; }
      public bool IsIndex { get; private set; }
      public Triple(Point? l, Point c, Point? r)
      {
        Left = l;
        Right = r;
        Center = c;
        IsIndex = false;
      }
      public Triple(Point index)
      {
        Center = index;
        IsIndex = true;
      }

      public Triple Copy()
      {
        if (IsIndex)
          return new Triple(Center);
        else
          return new Triple(
            Left == null ? null : (Point?)Left.Value, 
            Center, 
            Right == null ? null : (Point?)Right.Value);
      }
      public override string ToString()
      {
        var sb = new StringBuilder();
        sb.Append("(");
        sb.Append(Left.HasValue ? Left.ToString() : "x");
        sb.Append(")");
        sb.Append("[");
        sb.Append(Center.ToString());
        sb.Append("]");
        sb.Append("(");
        sb.Append(Right.HasValue ? Right.ToString() : "x");
        sb.Append(")");
        return sb.ToString();
      }

      /// <summary>
      /// givent the parabolas described by Center & Right (and the directix) 
      /// find the boundry intersection
      /// </summary>
      public double RightBound(double directix)
      {
        var nParabola = new Parabola(Center, directix);
        double uBound;
        if (!Right.HasValue)
          uBound = double.PositiveInfinity;
        else
        {
          var next = new Parabola(Right.Value, directix);
          var intersections = nParabola.Intersect(next);
          if (Parabola.IsNoIntersection(intersections.Item2))
          {
            //0-assertfail
            System.Diagnostics.Debug.Assert(!Parabola.IsNoIntersection(intersections.Item1),
              String.Format("No interesections between consecutive items: {0} and {1}", nParabola, next));

            uBound = intersections.Item1.X;
          }
          else
            uBound = Math.Min(intersections.Item1.X, intersections.Item2.X);
        }
        return uBound;
      }

      /// <summary>
      /// givent the parabolas described by Center & Left (and the directix) 
      /// find the boundry intersection
      /// </summary>
      public double LeftBound(double directix)
      {
        var nParabola = new Parabola(Center, directix);

        double lBound;
        //if this is the first item, let -infinity be lower bound
        //otherwise find the intersection w. previous node
        if (!Left.HasValue)
          lBound = double.NegativeInfinity;
        else
        {
          var prev = new Parabola(Left.Value, directix);
          var intersections = nParabola.Intersect(prev);
          if (Parabola.IsNoIntersection(intersections.Item2))
          {
            //0-assertfail
            System.Diagnostics.Debug.Assert(!Parabola.IsNoIntersection(intersections.Item1),
              String.Format("No interesections between consecutive items: {0} and {1}", prev, nParabola));

            lBound = intersections.Item1.X;
          }
          else //2, largest 
            lBound = Math.Max(intersections.Item1.X, intersections.Item2.X);
        }
        return lBound;
      }


      ///// <summary>
      ///// Find a point on the parabolic arc described by the center point, 
      ///// bound by the left and right.
      ///// </summary>
      //public double FindAnX(double directix)
      //{ 
      //}
    }

    public  class StatusStructure : SkipList<Triple>
    {

      public StatusStructure() : base(BeachLineComparer.Instance) { }
      public StatusStructure(int seed) : base(seed, BeachLineComparer.Instance) { }
      
      //hide these.
      private StatusStructure(IComparer<Triple> c) : base(BeachLineComparer.Instance) { }
      private StatusStructure(int seed, IComparer<Triple> c) : base(seed, BeachLineComparer.Instance) { }

      //When I extend this for Voroni, I'll have to more explicitly control 
      //insertions and deletions. It depends on finding node triplets that
      //are in the right place. I can't just rely on a simple comparer to keep the
      //right order.
      /// <summary>
      /// Find the node for which c.Compare() returns 0. If c lt 0, it assumes the 
      /// tested node is too "small." c gt 0 the tested node is too "big."
      /// </summary>
      //public SkipNode<Parabola> FindNode(Point item)
      //{
      //  var current = _Root;

      //  for (int i = current.Height - 1; i >= 0; i--)
      //  {

      //    //go either til we find it, or the last one at this level that is less than item
      //    while (current[i] != null && (Checker(item, current[i]) > 0))
      //    {
      //      current = current[i];
      //    }
      //  }

      //  if (current != _Root && Checker(item, current) == 0)
      //    return current;
      //  else
      //    return null;
      //}

      private BeachLineComparer BLComparer
      { get {return (BeachLineComparer) Comparer;} }

      /// <summary>
      /// The sweep line
      /// </summary>
      public double Directix 
      {
        get { return BLComparer.Directix; }
        set { BLComparer.Directix = value; }
      }

      /*
      //TODO: this subclass needs to use a comparer that operates not on T, but SkipNode<T>. 
      Probably using something like Checker(). perhaps a dummy SkipNode<T> for the insertion item.
      or maybe instead of T being the single item, it is a tripple with the preceeding and succeeding 
      segments
      Since the comparerer needs 2 items of the same type, maybe mark the new item as a triple that is all the same, 
      since that can't happen in the real data structure.
       * 
       * 
       * 
       * 
       * No wait! I know!
       * override Add/Remove. the ones with the nodes as output. then, do a fixup on the node's prev & next. 
       * 
       * Brilliant!
      */
      public override SkipNode<Triple>[] Add(Triple item, out SkipNode<Triple> itemNode)
      {
        //some work to set up the node that we'll be inserting before the main one.
        var predecessors = base.Add(item, out itemNode);

        //the node we dup is itemnode.Next, and we make it previous. hmm..

        if (itemNode.Next() != null) //any time but during the first insertion
        {
          var prevNode = new SkipNode<Triple>(ReadjustHeight(), itemNode.Next().Value.Copy());

          //we may need to fix up predecessors...
          if (predecessors.Length < _Root.Height)
          {
            var old = predecessors;

            predecessors = new SkipNode<Triple>[_Root.Height];
            for (int i = 0; i < old.Length; i++)
            {
              //Don't leave things orphanned with an old dead root.
              if(old[i].Value == null)
                predecessors[i] = _Root;
              else
                predecessors[i] = old[i];
            }
            predecessors[predecessors.Length - 1] = _Root;
          }

          //rethread
          Rethread(prevNode, predecessors);

          Count++;
        }

        //modify values
        Point? l = null;
        if (itemNode.Previous != _Root)
        {
          itemNode.Previous.Value.Right = (Point?) item.Center;
          l = itemNode.Previous.Value.Center;
        }

        Point? r = null;
        if (itemNode.Next() != null)
        {
          itemNode.Next().Value.Left = (Point?)item.Center;
          r = itemNode.Next().Value.Center;
        }

        itemNode.Value = new Triple(l, itemNode.Value.Center, r);

        return predecessors;
      }

      public override bool Remove(Triple item, out SkipNode<Triple> removed)
      {

        if (base.Remove(item, out removed))
        {

          if (removed.Previous != _Root)
          {
            if (removed.Next() != null)
            {
              removed.Next().Value.Left = removed.Previous.Value.Center;
              removed.Previous.Value.Right = removed.Next().Value.Center;
            }
            else
              removed.Previous.Value.Right = null;
          }
          else 
          {
            if (removed.Next() != null)
              removed.Next().Value.Left = null;
          }
          return true;
        }
        return false;
      }
    }

    public class BeachLineComparer : IComparer<Triple>
    {
      static BeachLineComparer()
      {
        Instance = new BeachLineComparer();
      }

      public static BeachLineComparer Instance;

      /// <summary>
      /// The sweep line
      /// </summary>
      public double Directix { get; set; }

      public int Compare(Triple index, Triple n)
      {
        //swap if needed.
        if(!index.IsIndex)
        {
          System.Diagnostics.Debug.Assert(n.IsIndex, "BeachLineComparer.Compare called without an index triplet");
          var tmp = n;
          n = index;
          index = tmp;
        }
        
        var nParabola = new Parabola(n.Center, Directix);

        double lBound = n.LeftBound(Directix);

        //item < lowerbound? -1
        if(index.Center.X < lBound)
          return -1;

        //last item? upper bound is +inifinity
        //otherwise intersection w/ next parabola
        double uBound = n.RightBound(Directix);

        //item >= upperbound? +1
        if (index.Center.X >= uBound)
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
