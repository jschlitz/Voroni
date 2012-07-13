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
      var pQueue = new SkipList<IEvent>(PointEventComparer.Instance);
      var result = new HalfEdgeStructure();
      var status = new StatusStructure(result);
      foreach (var p in points) pQueue.Add(new SiteEvent(p));


      //until the queue is empty...
      while (pQueue.Count > 0)
      {
        //dequeue
        IEvent item = pQueue.First();
        pQueue.Remove(item);
        System.Diagnostics.Trace.WriteLine("Dequeued " + item);

        if (item is SiteEvent)
        {
          //add to status
          status.Directix = item.P.Y;

          SkipNode<Triple> addedNode;
          //TODO: some special handling when we have initial points that are colinear on the same Y.
          status.Add(new Triple(item.P), out addedNode);

          //Add circle events for the left and right arcs
          AddCircleEvent(pQueue, status.Directix, addedNode.Previous);
          AddCircleEvent(pQueue, status.Directix, addedNode.Next());
        }
        else if (item is CircleEvent)
        {
          var ce = item as CircleEvent;
          //drop the Triple
          status.Remove(ce.TargetTripleNode.Value);

          //drop any circleEvents involving this Triple
          DropRelatedCircleEvents(pQueue, ce.TargetTripleNode.Previous);
          DropRelatedCircleEvents(pQueue, ce.TargetTripleNode.Next());

          //TODO: possibly the above calls are redundant due to these. But I 
          //think there is something subtle that requires the calls DropRelatedCircleEvents.
          AddCircleEvent(pQueue, ce.P.Y, ce.TargetTripleNode.Previous);
          AddCircleEvent(pQueue, ce.P.Y, ce.TargetTripleNode.Next());
        }
        else System.Diagnostics.Debug.Assert(false, "pQueue had non-circle, non-site event.");
      }

      AddBoundingBox(result);

      System.Diagnostics.Trace.WriteLine("Final: " + status);
      return result;
    }

    private static void AddBoundingBox(HalfEdgeStructure result)
    {
      //maxes and mins
      var possibleBoundPoints = result.Verticies.Select(v => v.Coordinates)
        .Concat(result.Faces.Select(f => f.Site));
      Point fl = possibleBoundPoints.Aggregate((acc, p) => new Point(Math.Min(acc.X, p.X), Math.Min(acc.Y, p.Y)));
      Point br = possibleBoundPoints.Aggregate((acc, p) => new Point(Math.Max(acc.X, p.X), Math.Max(acc.Y, p.Y)));

      fl = new Point(fl.X - 1, fl.Y - 1);
      br = new Point(br.X + 1, br.Y + 1);

      //setup the bounding box
      Face inside = new Face(new Point((fl.X + br.X )/2, (fl.Y + br.Y )/2));
      Face outside = new Face(new Point(double.PositiveInfinity, double.PositiveInfinity));
      var back = new HalfEdge(inside, outside);
      var left = new HalfEdge(inside, outside);
      var front = new HalfEdge(inside, outside);
      var right = new HalfEdge(inside, outside);
      back.Next = left;
      left.Next = front;
      front.Next = right;
      right.Next = back;
      back.Twin.Next = right.Twin;
      right.Twin.Next = front.Twin;
      front.Twin.Next = left.Twin;
      left.Twin.Next = back.Twin;

      SetupBBEdge(br.X, br.Y, back, right);
      SetupBBEdge(fl.X, br.Y, left, back);
      SetupBBEdge(fl.X, fl.Y, front, left);
      SetupBBEdge(br.X, fl.Y, right, front);

      //someplace to store points (indexed by wellknown consts)
      var intersections = new[] { new List<HalfEdge>(), new List<HalfEdge>(), new List<HalfEdge>(), new List<HalfEdge>() };

      //foreach non-origned edge
      foreach (var edge in result.Edges.Where(e=>e.Origin==null))
      {
        //get bisector, find intersection w/ edge
        var v = (edge.Twin.IncidentFace.Site - edge.IncidentFace.Site) / 2;
        var o = (edge.IncidentFace.Site + v);
        double m = v.Y == 0 ? double.NaN : (- v.X / v.Y); //v is perpendicular to the slop of the edge
        double b = double.IsNaN(m) ? double.NaN : o.Y - o.X * m;
        var possibles = new[]
        {
          GetHIntersection(back.Origin.Coordinates.Y, o, m, b), GetVIntersection(left.Origin.Coordinates.X, o, m, b),
          GetHIntersection(front.Origin.Coordinates.Y, o, m, b), GetVIntersection(right.Origin.Coordinates.X, o, m, b)
        };
        var dists = possibles.Select(p => 
          {
            if (p == NAN_POINT) return double.NaN;
            if (CheckPoint(edge.IncidentFace.Site, o, p) != PointIs.Right) return double.NaN;
            return Math.Abs((p - o).Length);
          }).ToArray();

        int index = -1;
        for (int i = 0; i < 4; i++)
        {
          if (double.IsNaN(dists[i])) continue;

          index = (index == -1) || (dists[index] > dists[i]) ? i : index;
        }
        System.Diagnostics.Debug.Assert(index != -1, "Can't find intersection for " + edge.ToString());

        //take the right-hand one, add to list
        edge.Origin = new Vertex { Coordinates = possibles[index], IncidentEdge = edge };
        intersections[index].Add(edge);
        //result.Verticies.Add(edge.Origin);
        
      }

      //now that we have the intersections go through and add each of the new edges that trace the boundry box, in order
      AddBoundingEdge(intersections[BACK].OrderBy(he => he.Origin.Coordinates, PointComparer.Instance).Reverse(), back, result);
      AddBoundingEdge(intersections[LEFT].OrderBy(he => he.Origin.Coordinates, PointComparer.Instance), left, result);
      AddBoundingEdge(intersections[FRONT].OrderBy(he => he.Origin.Coordinates, PointComparer.Instance), front, result);
      AddBoundingEdge(intersections[RIGHT].OrderBy(he => he.Origin.Coordinates, PointComparer.Instance).Reverse(), right, result);

      foreach (var e in result.Edges)
      {
        if (e.IncidentFace == inside)
          e.IncidentFace = (result.Faces.Count == 1) ? result.Faces[0] : GetGoodFace(e);
      }

    }
    const int BACK = 0;
    const int LEFT = 1;
    const int FRONT = 2;
    const int RIGHT = 3;

    private static Face GetGoodFace(HalfEdge e)
    {
      var item = e.Next;
      while (item != e)
      {
        if (item.IncidentFace != e.IncidentFace)
          return item.IncidentFace;
        else
          item = item.Next;
      }

      System.Diagnostics.Debug.Fail("Unable to get a good IncidentFace for " + e);
      return null;
    }

    /// <summary>
    /// boundEdge is the boundry edge (ccw from the inside to the outside). toAdd is the list of halfedges
    /// emanating from this boundry, sorted in the order from boundEdge's source to end. None of these
    /// edges nor verticies are part of result. This adds the edges in toAdd, and chops up boundEdge into
    /// the necessary component edges and adds them.
    /// </summary>
    private static void AddBoundingEdge(IEnumerable<HalfEdge> toAdd, HalfEdge boundEdge, HalfEdgeStructure result)
    {
      //add the start
      result.Verticies.Add(boundEdge.Origin);
      var start = boundEdge.Origin;
      foreach (var item in toAdd)
      {
        //get new edge from start to next vertex, add it
        var newChunk = new HalfEdge(item.IncidentFace, boundEdge.Twin.IncidentFace) {Origin=start};
        newChunk.Twin.Origin = item.Origin;
        result.Verticies.Add(item.Origin);
        result.Edges.Add(newChunk);
        result.Edges.Add(newChunk.Twin);
        boundEdge.Origin = item.Origin;

        //hook up edges' nexts 
        var right = boundEdge.Twin.Next.Twin;
        newChunk.Next = item;
        item.Twin.Next = boundEdge;
        boundEdge.Twin.Next = newChunk.Twin;
        newChunk.Twin.Next = right.Twin;
        right.Next = newChunk;
      }

      result.Edges.Add(boundEdge);
      result.Edges.Add(boundEdge.Twin);
      //what about incident face...? Maybe this needs to wait for completion?
    }

    private static Point GetVIntersection(double x, Point o, double m, double b)
    {
      return double.IsNaN(m) ? NAN_POINT : m == 0 ? new Point(x, o.Y) : new Point(x, m * x + b);
    }

    private static Point GetHIntersection(double y, Point o, double m, double b)
    {
      return double.IsNaN(m) ? new Point(o.X, y) : m == 0 ? NAN_POINT : new Point((y - b) / m, y);
    }
    static readonly Point NAN_POINT = new Point(double.NaN, double.NaN);

    private static void SetupBBEdge(double x, double y, HalfEdge edge, HalfEdge prev)
    {
      edge.Origin = new Vertex { Coordinates = new Point(x, y), IncidentEdge = edge };
      prev.Twin.Origin = edge.Origin;
    }

    private static void DropRelatedCircleEvents(SkipList<IEvent> pQueue, SkipNode<Triple> skipNode)
    {
      if (pQueue == null) throw new ArgumentNullException("PQueue");
      if (skipNode == null) return;
      if (skipNode.Value == null) return;
      if (skipNode.Value.IsIndex) throw new ArgumentException("skipNode.IsIndex is true", "skipNode");
      if (skipNode.Value.VanishEvent == null) return;

      pQueue.Remove(skipNode.Value.VanishEvent);
      System.Diagnostics.Trace.WriteLine("Removed " + skipNode.Value.VanishEvent);
      skipNode.Value.VanishEvent = null;
    }

    private enum PointIs {Left, On, Right};
    private static PointIs CheckPoint(Point segStart, Point segEnd, Point p)
    {
      var seg = segEnd - segStart;
      var pVec = p - segStart;
      var cp = Vector.CrossProduct(seg, pVec);

      if (cp == 0)
        return PointIs.On;
      else if (cp > 0)
        return PointIs.Left;
      else //if (cp < 0)
        return PointIs.Right;
    }

    private static void AddCircleEvent(SkipList<IEvent> pQueue, double cutoffY, SkipNode<Triple> centerNode)
    {
      if (pQueue == null) throw new ArgumentNullException("pQueue");

      if (centerNode == null) return;
      var arc = centerNode.Value;

      if (arc == null) return;//should only happen for the 1st node.

      //assert !null, has value, is not index...
      if (!arc.Left.HasValue || !arc.Right.HasValue) return;
      if (arc.Right.Value == arc.Left.Value) return;
      if (arc.Center == arc.Left.Value) return;
      if (arc.Right.Value == arc.Center) return;

      //CHECK CONVERGENCE L->C->R MUST BE CLOCKWISE. (r right of lc)
      if (CheckPoint(arc.Left.Value, arc.Center, arc.Right.Value) != PointIs.Right) return;

      var c = new Circle(arc.Left.Value, arc.Center, arc.Right.Value);
      if (c.Center.Y - c.Radius < cutoffY) //TODO: worried what happens when  we have 4 co-circular points
      {
        //point ce at node
        var ce = new CircleEvent(centerNode, c);

        //point node.value at ce (I was considering removing the even regardless, but that does not seem to be correct. We'll see in testing.)
        if (arc.VanishEvent != null)
        {
          pQueue.Remove(arc.VanishEvent);
          System.Diagnostics.Trace.WriteLine("Removed " + arc.VanishEvent);
        }
        arc.VanishEvent = ce;

        pQueue.Add(ce);
        System.Diagnostics.Trace.WriteLine("Enqueued " + ce);

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
      public CircleEvent(SkipNode<Triple> t, Circle c)
      {
        TargetTripleNode = t;
        C = c;
        P = new Point(c.Center.X, c.Center.Y - c.Radius);
      }
      public Circle C { get; protected set; }
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
      /// givent the parabolas described by Center and Right (and the directix) 
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
            //This probably has the same problem as LeftBound...
            //if (Left == Right) //special case when we're intersecting one on both sides 
            //  uBound = Math.Max(intersections.Item1.X, intersections.Item2.X);
            //else 
            //  uBound = Math.Min(intersections.Item1.X, intersections.Item2.X);
            uBound = (Center.Y >= Right.Value.Y) ?
              Math.Min(intersections.Item1.X, intersections.Item2.X) :
              Math.Max(intersections.Item1.X, intersections.Item2.X);
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
            //TODO: This is just wrong... What is right?
            //if (Left == Right) //special case when we're intersecting one on both sides
            //  lBound = Math.Min(intersections.Item1.X, intersections.Item2.X);
            //else
            //  lBound = Math.Max(intersections.Item1.X, intersections.Item2.X);
            lBound = (Center.Y <= Left.Value.Y) ?
              Math.Min(intersections.Item1.X, intersections.Item2.X) :
              Math.Max(intersections.Item1.X, intersections.Item2.X);
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

              //TODO: vertex?
              var v = new Vertex{Coordinates = item.VanishEvent.C.Center};
              //v.IncidentEdge = ...
              FinalResult.Verticies.Add(v);
              //item.VanishEvent.C.Center...

              //new edgepair
              var newEdge = new HalfEdge(removed.Previous.Value.MyFace, removed.Next().Value.MyFace);
              removed.Previous.Value.RightEdge = newEdge;
              removed.Next().Value.LeftEdge = newEdge.Twin;
              FinalResult.Edges.Add(newEdge);
              FinalResult.Edges.Add(newEdge.Twin);

              //hook up vertex to edges
              newEdge.Twin.Origin = v;
              v.IncidentEdge = newEdge.Twin;
              //previous edges gets an origins too
              removed.Value.RightEdge.Origin = v;
              removed.Value.LeftEdge.Twin.Origin = v;

              //edge.nexts.
              removed.Value.LeftEdge.Next = removed.Value.RightEdge;
              removed.Value.RightEdge.Twin.Next = removed.Next().Value.LeftEdge;
              removed.Previous.Value.RightEdge.Next = removed.Value.LeftEdge.Twin;

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
    /// Compare 2 point events by their Y value, high to low. if they are equal, compare by X, low to high.
    /// </summary>
    private class PointComparer : IComparer<Point>
    {
      public static readonly PointComparer Instance = new PointComparer();

      public int Compare(Point x, Point y)
      {
        double result = -1 * (x.Y - y.Y);
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

    /// <summary>
    /// Compare 2 point events by their Y value, high to low. if they are equal, compare by X, low to high.
    /// </summary>
    private class PointEventComparer : IComparer<IEvent>
    {
      public static readonly PointEventComparer Instance = new PointEventComparer();

      public int Compare(IEvent x, IEvent y)
      {
        return PointComparer.Instance.Compare(x.P, y.P);
        //double result = -1 * (x.P.Y - y.P.Y);
        //if (result == 0)
        //  result = x.P.X - y.P.X;//note this is reversed.

        //if (result > 0) 
        //  return 1;
        //else if (result < 0) 
        //  return -1;
        //else 
        //  return 0;
      }
    }
  }
}
