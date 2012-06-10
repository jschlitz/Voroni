using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using DataStructures;
using System.IO;
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

          SkipNode<Triple> addedNode;
          status.Add(new Triple(item.P), out addedNode);

          //Add circle events for the left and right arcs
          AddCircleEvent(pQueue, status.Directix, addedNode.Previous);
          AddCircleEvent(pQueue, status.Directix, addedNode.Next());
        }
      }

      return result;
    }

    private static void AddCircleEvent(SkipList<IEvent> pQueue, double directix, SkipNode<Triple> centerNode)
    {
      if (pQueue == null) throw new ArgumentNullException("pQueue");
      
      if (centerNode == null) return;
      var arc = centerNode.Value;

      if (arc == null) return;//should only happen for the 1st node.

      //assert !null, has value, is not index...
      if (arc.Left.HasValue && arc.Right.HasValue &&
        (arc.Right.Value != arc.Left.Value) && (arc.Center != arc.Left.Value) &&
        (arc.Right.Value != arc.Center))
      {
        var c = new Circle(arc.Left.Value, arc.Center, arc.Right.Value);
        if (c.Center.Y + c.Radius >= directix)
        {
          //point ce at node
          var ce = new CircleEvent(centerNode, new Point(c.Center.X, c.Center.Y + c.Radius));

          //point node.value at ce
          if (arc.VanishEvent != null)
            pQueue.Remove(arc.VanishEvent);
          arc.VanishEvent = ce;

          pQueue.Add(ce);
        }
      }
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
      public override string ToString()
      {
        return string.Format("Site:({0})", P.ToString());
      }
    }

    /// <summary>
    /// An event for the queue: an arc might dissapear!
    /// </summary>
    public class CircleEvent : IEvent
    {
      public CircleEvent(SkipNode<Triple> t, Point p)
      {
        TargetTripleNode = t;
        P = p;
      }
      public Point P { get;  set; }
      /// <summary>
      /// The node corresponding to the arc we may delete.
      /// </summary>
      public SkipNode<Triple> TargetTripleNode { get; set; }
      public override string ToString()
      {
        return string.Format("Circle:({0} - {1})", P.ToString(), TargetTripleNode.Value.ToString());
      }

    }

    /// <summary>
    /// Represents an arc, OR a ray striking the arc
    /// </summary>
    public class Triple  
    {
      public Point? Left { get; set; }
      public Point? Right { get; set; }
      public Point Center { get; set; }
      public bool IsIndex { get; private set; }
      /// <summary>
      /// Event where this arc may vanish
      /// </summary>
      public CircleEvent VanishEvent { get; set; }
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
          else //2
          {
            if (Left == Right) //special case when we're intersecting one on both sides 
              uBound = Math.Max(intersections.Item1.X, intersections.Item2.X);
            else 
              uBound = Math.Min(intersections.Item1.X, intersections.Item2.X);
          }
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
          else //2
          {
            if (Left == Right) //special case when we're intersecting one on both sides
              lBound = Math.Min(intersections.Item1.X, intersections.Item2.X);
            else
              lBound = Math.Max(intersections.Item1.X, intersections.Item2.X);
          }
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

    /// <summary>
    /// a series of parabolic arcs that monotonicly increase in x.
    /// </summary>
    public class StatusStructure : SkipList<Triple>
    {

      #region constructors
      public StatusStructure(HalfEdgeStructure finalResult)
        : base(StrategicComparer.Instance)
      {
        FinalResult = finalResult;
      }
      public StatusStructure(int seed, HalfEdgeStructure finalResult)
        : base(seed, StrategicComparer.Instance)
      {
        FinalResult = finalResult;
      }
      #endregion

      #region hide these
      private StatusStructure(IComparer<Triple> c) : base(StrategicComparer.Instance) { }
      private StatusStructure(int seed, IComparer<Triple> c) : base(seed, StrategicComparer.Instance) { }
      private StatusStructure() : base(StrategicComparer.Instance) { }
      private StatusStructure(int seed) : base(seed, StrategicComparer.Instance) { }
      #endregion

      /// <summary>
      /// Structure that contains faces, edges and verticies found as we proceed.
      /// </summary>
      public HalfEdgeStructure FinalResult { get; protected set; }

      private StrategicComparer SComparer
      { get { return (StrategicComparer)Comparer; } }


      /// <summary>
      /// The sweep line
      /// </summary>
      public double Directix
      {
        get { return SComparer.Directix; }
        set { SComparer.Directix = value; }
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

          System.Diagnostics.Debug.Assert(l == r); // we've bisected an arc. This should be the same! Sanity check.

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

      #region nix?
      /// <summary>
      /// Remove a particular node and (this is the tricky bit) fix up the predecessors
      /// </summary>
      //public bool RemoveNode(SkipNode<Triple> toRemove)
      //{
      //  var index = new Triple(new Point(toRemove.Value.FindAnX(Directix), Directix));
      //  SkipNode<Triple> actuallyRemoved;
      //  if (Remove(index, out actuallyRemoved))
      //  {
      //    if (actuallyRemoved != toRemove)
      //    {
      //      string tmp =Path.GetTempFileName();
      //      using (var f = File.CreateText(tmp))
      //      {
      //        f.Write("Tried to remove: ");
      //        f.WriteLine(toRemove.Value.ToString());
      //        f.Write("Actually removed: ");
      //        f.WriteLine(actuallyRemoved.ToString());
      //        f.Write("Directix: ");
      //        f.WriteLine(Directix);

      //        f.WriteLine(ToString());
      //      }
      //      System.Diagnostics.Debug.Assert(false, "Failure in RemovedNode() " + tmp);
      //    }

      //    return true;
      //  }
      //  else
      //    return false;
      //}
      #endregion

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

              //TODO: now to test all of this edge stuff. man.
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
    /// If the first argument to the comparer has IsIndex == true, we use 
    /// BeachLineComparer. Otherwise we use ArcComparer
    /// </summary>
    public class StrategicComparer : IComparer<Triple>
    {

      static StrategicComparer()
      {
        Instance = new StrategicComparer();
      }

      public static StrategicComparer Instance;


      public int Compare(Triple x, Triple y)
      {
        if (x.IsIndex)
          return BeachLineComparer.Instance.Compare(x, y);
        else
          return ArcComparer.Instance.Compare(x, y);
      }

      /// <summary>
      /// The sweep line
      /// </summary>
      public double Directix 
      {
        get { return _Directix; }
        set 
        {
          _Directix = value;
          BeachLineComparer.Instance.Directix = _Directix;
          ArcComparer.Instance.Directix = _Directix;
        }
      }
      public double _Directix;
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
        }
        
        var nParabola = new Parabola(n.Center, Directix);

        double lBound = n.LeftBound(Directix);

        //last item? upper bound is +inifinity
        //otherwise intersection w/ next parabola
        double uBound = n.RightBound(Directix);

        //TODO: for some reason this blows up the add test
        //special case: if lbound >= uBound, we've collapsed. 
        //OR THERE IS A RAY INTERSECTING THE POLYGON ELSEWHERE. DAMMIT.
        //if (lBound >= uBound)
        //  return 0;

        //.000 000 03

        //item < lowerbound? -1
        if(index.Center.X < lBound)
          return -1;


        //item > upperbound? +1
        if (index.Center.X > uBound)
          return 1;

        //we got here? this must be the place.
        return 0;
      }
    }

    /// <summary>
    /// Look for a particular arc. If you don't find it, is the given arc left (-1) or right (+1) of it?
    /// </summary>
    public class ArcComparer : IComparer<Triple>
    {
      static ArcComparer()
      {
        Instance = new ArcComparer();
      }

      public static ArcComparer Instance;

      /// <summary>
      /// The sweep line
      /// </summary>
      public double Directix { get; set; }

      public int Compare(Triple target, Triple n)
      {
        //swap if needed.
        if (target.IsIndex)
        {
          System.Diagnostics.Debug.Assert(n.IsIndex, "ArcComparer.Compare called by index triplet. did you mean to use BeachLineComparer?");
        }

        if (target == n)
          return 0;
        {
          return target.FindAnX(Directix) - n.FindAnX(Directix) < 0 ? -1 : 1;
        }
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
