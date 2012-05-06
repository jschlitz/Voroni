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
      var pQueue = new SkipList<IEvent>(PointComparer.Instance);
      var result = new HalfEdgeStructure();
      var status = new StatusStructure(result);
      foreach (var p in points) pQueue.Add(new SiteEvent(p));


      //until the queue is empty...
      while (pQueue.Count > 0)
      {
        //dequeue
        IEvent item = pQueue.First();
        pQueue.Remove(item);

        if (item is SiteEvent)
        {
          //add to status
          status.Directix = item.P.Y;
          status.Add(new Triple(item.P));

        }
      }


      return result;
    }

    /// <summary>
    /// An event in the queue
    /// </summary>
    public interface IEvent
    {
      Point P { get; set; }
    }

    /// <summary>
    /// An event originating from a site in the diagram
    /// </summary>
    public class SiteEvent : IEvent
    {
      public SiteEvent(Point p) { P = p; }
      public Point P { get; set; }
    }

    public class CircleEvent : IEvent
    {
      public CircleEvent(Triple t)
      {
        T = t;
        //TODO compute circle, get point w/ lowest Y.
      }
      public Point P { get; set; }
      /// <summary>
      /// Triple, the center of which will vanish when this event hits.
      /// </summary>
      public Triple T{get; protected set;}
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
      //TODO: CircleEvent where this disappears
      /// <summary>
      /// Edge between center & right. Can be null
      /// </summary>
      public HalfEdge RightEdge { get; set; }

      /// <summary>
      /// Edge between center & left. Can be null
      /// </summary>
      public HalfEdge LeftEdge { get; set; }

      /// <summary>
      /// Face associated with this arc
      /// </summary>
      public Face MyFace { get; set; }

      public Triple Copy()
      {
        //Triple result;
        //if (IsIndex)
        //  result = new Triple(Center);
        //else
        //  result = new Triple(
        //    Left == null ? null : (Point?)Left.Value, 
        //    Center, 
        //    Right == null ? null : (Point?)Right.Value);

        return (Triple)MemberwiseClone();
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


      /// <summary>
      /// Find a point on the parabolic arc described by the center point, 
      /// bound by the left and right.
      /// </summary>
      public double FindAnX(double directix)
      {
        var l = LeftBound(directix);
        var r = RightBound(directix);
        if (double.IsInfinity(l))
          return (double.IsInfinity(r) ? Center.X : r - 1);
        else if (double.IsInfinity(r))
          return l + 1; //taken care of infinte lereturn (double.IsInfinity(l) ? Center.X : l + 1);
        else
          return (l + r) / 2;
      }
    }

    public  class StatusStructure : SkipList<Triple>
    {

      #region constructors
      public StatusStructure(HalfEdgeStructure finalResult) : base(BeachLineComparer.Instance) 
      {
        FinalResult = finalResult;
      }
      public StatusStructure(int seed, HalfEdgeStructure finalResult) : base(seed, BeachLineComparer.Instance) 
      {
        FinalResult = finalResult;
      }
      #endregion

      #region hide these
      private StatusStructure(IComparer<Triple> c) : base(BeachLineComparer.Instance) { }
      private StatusStructure(int seed, IComparer<Triple> c) : base(seed, BeachLineComparer.Instance) { }
      private StatusStructure() : base(BeachLineComparer.Instance) { }
      private StatusStructure(int seed) : base(seed, BeachLineComparer.Instance) { }
      #endregion

      /// <summary>
      /// Structure that contains faces, edges and verticies found as we proceed.
      /// </summary>
      public HalfEdgeStructure FinalResult { get; protected set; }
      
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

      public override SkipNode<Triple>[] Add(Triple item, out SkipNode<Triple> itemNode)
      {
        //some work to set up the node that we'll be inserting before the main one.
        var predecessors = base.Add(item, out itemNode);

        //the node we dup is itemnode.Next, and we make it previous. hmm..
        if (itemNode.Next() == null) //only during first insertion
        {
          var newFace = new Face(itemNode.Value.Center);
          FinalResult.Faces.Add(newFace);
          itemNode.Value = new Triple(null, itemNode.Value.Center, null);
          itemNode.Value.MyFace = newFace;
        }
        else
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
              if (old[i].Value == null)
                predecessors[i] = _Root;
              else
                predecessors[i] = old[i];
            }

            for (int i = old.Length; i < predecessors.Length; i++)
              predecessors[i] = _Root;

          }

          //rethread
          Rethread(prevNode, predecessors);
          Count++;

          //modify values
          Point? l = null;
          itemNode.Previous.Value.Right = (Point?)item.Center;
          l = itemNode.Previous.Value.Center;

          Point? r = null;
          itemNode.Next().Value.Left = (Point?)item.Center;
          r = itemNode.Next().Value.Center;

          System.Diagnostics.Debug.Assert(l==r); // we've bisected an arc. This should be the same! Sanity check.

          //new face!
          var newFace = new Face(itemNode.Value.Center);
          FinalResult.Faces.Add(newFace);
          itemNode.Value = new Triple(l, itemNode.Value.Center, r) { MyFace = newFace };

          //new edges
          var newEdge = new HalfEdge(newFace, itemNode.Previous.Value.MyFace);//prev/next are equiv 
          FinalResult.Edges.Add(newEdge);
          FinalResult.Edges.Add(newEdge.Twin);
          itemNode.Value.LeftEdge = newEdge;
          itemNode.Value.RightEdge = newEdge;
          itemNode.Previous.Value.RightEdge = newEdge.Twin;
          itemNode.Next().Value.LeftEdge = newEdge.Twin;

          //set edge for the faces
          newFace.OuterEdge = newEdge;
          itemNode.Previous.Value.MyFace.OuterEdge = newEdge.Twin;
        }

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
              
              //new edgepair
              var newEdge = new HalfEdge(removed.Previous.Value.MyFace, removed.Next().Value.MyFace);
              removed.Previous.Value.RightEdge = newEdge;
              removed.Next().Value.LeftEdge = newEdge.Twin;
              FinalResult.Edges.Add(newEdge);
              FinalResult.Edges.Add(newEdge.Twin);

              //edge.nexts?
              removed.Value.LeftEdge.Next = removed.Value.RightEdge;
              removed.Value.RightEdge.Twin.Next = removed.Next().Value.LeftEdge;
              removed.Previous.Value.RightEdge.Next = removed.Value.LeftEdge.Twin;

              //TODO: vertex?
              
            }
            else
            {
              removed.Previous.Value.Right = null;
              removed.Previous.Value.RightEdge = null;
            }
          }
          else 
          {
            if (removed.Next() != null)
            {
              removed.Next().Value.Left = null;
              removed.Next().Value.LeftEdge = null;
            }
          }
          return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Compare an arc and a ray that wants to bisect an arc
    /// </summary>
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
    private class PointComparer : IComparer<IEvent>
    {
      public static readonly PointComparer Instance = new PointComparer();

      public int Compare(IEvent x, IEvent y)
      {
        double result = -1 * (x.P.Y - y.P.Y);
        if (result == 0)
          result = x.P.X - y.P.X;//note this is reversed.

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
