using Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace TestProject1
{
    
    
    /// <summary>
    ///This is a test class for VoroniBuilderTest and is intended
    ///to contain all VoroniBuilderTest Unit Tests
    ///</summary>
  [TestClass()]
  public class VoroniBuilderTest
  {


    private TestContext testContextInstance;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
    {
      get
      {
        return testContextInstance;
      }
      set
      {
        testContextInstance = value;
      }
    }

    #region Additional test attributes
    // 
    //You can use the following additional attributes as you write your tests:
    //
    //Use ClassInitialize to run code before running the first test in the class
    //[ClassInitialize()]
    //public static void MyClassInitialize(TestContext testContext)
    //{
    //}
    //
    //Use ClassCleanup to run code after all tests in a class have run
    //[ClassCleanup()]
    //public static void MyClassCleanup()
    //{
    //}
    //
    //Use TestInitialize to run code before running each test
    //[TestInitialize()]
    //public void MyTestInitialize()
    //{
    //}
    //
    //Use TestCleanup to run code after each test has run
    //[TestCleanup()]
    //public void MyTestCleanup()
    //{
    //}
    //
    #endregion


    /// <summary>
    ///A test for MakeDiagram
    ///</summary>
    [TestMethod()]
    public void MakeDiagramTest()
    {
      Random r = new Random(1975);
      IList<Point> points = new List<Point>();
      for (int i = 0; i < 20; i++)
        points.Add(new Point(r.NextDouble() * 10, r.NextDouble() * 10));
      
      //Make sure that there's 1 w/ the same y, and an out and out duplicate.
      points.Add(new Point(r.NextDouble() * 10, points[0].Y));
      points.Add(new Point(points[3].X, points[3].Y));

      HalfEdgeStructure actual;
      actual = VoroniBuilder.MakeDiagram(points);

      Point prev = new Point(-1, 300);
      foreach (var f in actual.Faces)
      {
        Assert.IsTrue(
          (f.Site.Y < prev.Y) ||
          ((f.Site.Y == prev.Y) && (f.Site.X >= prev.X)));
        prev = f.Site;
      }
    }

    [TestMethod]
    public void TestMultiY()
    {
      //starting points out on the same Y can be tricky.
      var points = new[] { new Point(3, 4), new Point(1, 4), new Point(6, 2), new Point(10, 4) };

      var expected = new[]{"Dequeued Site:(1,4)",
"Dequeued Site:(3,4)",
"Dequeued Site:(10,4)",
"Dequeued Site:(6,2)",
"Enqueued Circle:(2,-5.6041219597369 - (1,4)[3,4](6,2))",
"Enqueued Circle:(6.5,1.96887112585073 - (6,2)[3,4](10,4))",
"Dequeued Circle:(6.5,1.96887112585073 - (6,2)[3,4](10,4))",
"Dequeued Circle:(2,-5.6041219597369 - (1,4)[3,4](6,2))",
"Final: H:1V:(x)[1,4](6,2), H:1V:(1,4)[6,2](10,4), H:1V:(6,2)[10,4](x), "};

      CheckTraceAndGraph(points, expected);
    }

    [TestMethod]
    public void TestCircleRemovals()
    {
      var points = new[] { 
          new Point(2, 6), new Point(5, 7), new Point(6, 2), new Point(9, 1) };

      // use Trace events to emit this info. Otherwise I have to wait until the end to debug. Ew.
      //as we go through, the arc structure should be:
      //0, 010, 01020, 01 20, 01 2420, 01 24 0
      var expected = new[] {"Dequeued Site:(5,7)",
"Dequeued Site:(2,6)",
"Dequeued Site:(6,2)",
"Enqueued Circle:(4.25,1.39956143725216 - (2,6)[5,7](6,2))",
"Dequeued Circle:(4.25,1.39956143725216 - (2,6)[5,7](6,2))",
"Dequeued Site:(9,1)",
"Enqueued Circle:(8.71428571428571,0.990159470357534 - (6,2)[5,7](9,1))",
"Dequeued Circle:(8.71428571428571,0.990159470357534 - (6,2)[5,7](9,1))",
"Final: H:1V:(x)[5,7](2,6), H:1V:(5,7)[2,6](6,2), H:1V:(2,6)[6,2](9,1), H:1V:(6,2)[9,1](5,7), H:1V:(9,1)[5,7](x),",
};
      CheckTraceAndGraph(points, expected);
    }

    [TestMethod]
    public void TextMultiX()
    {
      //starting points out on the same X should be just dandy. Let's verify can be tricky.
      var points = new[] { new Point(3, 6), new Point(3, 12), new Point(3, 3), new Point(2, 4) };
      var expected = new[]{"Dequeued Site:(3,12)",
"Dequeued Site:(3,6)",
"Dequeued Site:(2,4)",
"Enqueued Circle:(-5.5,-0.013878188659973 - (3,12)[3,6](2,4))",
"Dequeued Site:(3,3)",
"Enqueued Circle:(3.5,2.91886116991581 - (3,3)[2,4](3,6))",
"Dequeued Circle:(3.5,2.91886116991581 - (3,3)[2,4](3,6))",
"Dequeued Circle:(-5.5,-0.013878188659973 - (3,12)[3,6](2,4))",
"Final: H:1V:(x)[3,12](2,4), H:1V:(3,12)[2,4](3,3), H:1V:(2,4)[3,3](3,6), H:1V:(3,3)[3,6](3,12), H:1V:(3,6)[3,12](x), "
};

      CheckTraceAndGraph(points, expected);
    }

    [TestMethod]
    public void TestInfinite()
    {
      var points = new[] { new Point(2, 4), new Point(3, 6), new Point(5, 10), new Point(7, 14) };
      var expected = new[]{"Dequeued Site:(7,14)",
"Dequeued Site:(5,10)",
"Dequeued Site:(3,6)",
"Dequeued Site:(2,4)",
"Final: H:1V:(x)[7,14](5,10), H:1V:(7,14)[5,10](3,6), H:1V:(5,10)[3,6](2,4), H:1V:(3,6)[2,4](3,6), H:1V:(2,4)[3,6](5,10), H:1V:(3,6)[5,10](7,14), H:1V:(5,10)[7,14](x), "
};

      CheckTraceAndGraph(points, expected);
    }

    private HalfEdgeStructure CheckTraceAndGraph(Point[] points, string[] expected)
    {
      var myListener = new TestTraceListener();
      try
      {
        Trace.Listeners.Add(myListener);

        var edgeGraph = VoroniBuilder.MakeDiagram(points);
        var theTrace = myListener.GetTrace();

        //perhaps there are extra things in the trace. But we should have all of expected[] in order
        int i = 0;
        foreach (var actual in theTrace)
        {
          if (i >= expected.Length) break;
          if (actual.StartsWith(expected[i]))
            i++;
        }
        Assert.AreEqual(i, expected.Length);

        CheckEdgeGraph(edgeGraph);
        return edgeGraph;
      }
      finally
      { Trace.Listeners.Remove(myListener); }
    }

    private void CheckEdgeGraph(HalfEdgeStructure edgeGraph)
    {
      var checkoff = new List<HalfEdge>(edgeGraph.Edges);
      //faces should all have closed graphs
      foreach (var f in edgeGraph.Faces)
        CheckOffEdges(checkoff, f.OuterEdge);
      
      //now we just have 1 poly for the outer bounds facing in.
      CheckOffEdges(checkoff, checkoff[0]);

      Assert.AreEqual(0, checkoff.Count, "Dangling edges: " + checkoff.ToString());
    }

    private static void CheckOffEdges(List<HalfEdge> checkoff, HalfEdge start)
    {
      var e = start;
      do
      {
        Assert.IsTrue(checkoff.Remove(e), "tried to pop an edge more than once:" + e);
        e = e.Next;
      } while (e != start);
    }

    private class TestTraceListener : TraceListener
    {
      StringBuilder Buffer = new StringBuilder();
      string[] Cache;

      public override void Write(string message)
      {
        Cache = null;
        Buffer.Append(message);
      }

      public override void WriteLine(string message)
      {
        Cache = null;
        Buffer.AppendLine(message);
      }

      public string[] GetTrace()
      {
        if (Cache == null)
          Cache = Buffer.ToString().Split(new[] { Environment.NewLine },StringSplitOptions.None);

        return Cache;
      }
    }
  }
}
