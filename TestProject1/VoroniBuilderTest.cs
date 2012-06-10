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
    public void TestCircleRemovals()
    {
      var myListener = new TestTraceListener();
      try
      {
        Trace.Listeners.Add(myListener);

        var points = new[] { 
        new Point(2, 6), new Point(5, 7), 
        new Point(6, 2), new Point(9, 1) };


        Trace.WriteLine("Wibble");
        Trace.WriteLine("Wobble");
        Trace.WriteLine("Bop!");
        Trace.TraceInformation("Information!");
        //TODO use Trace events to emit this info. Otherwise I have to wait until the end to debug. Ew.
        //as we go through, the arc structure should be:
        //0, 010, 01020, 01 20, 01 2420, 01 24 0
        //System.Diagnostics.Trace.Listeners.Add
        Assert.IsTrue(myListener.GetTrace().Contains("Wibble"));

      }
      finally
      {
        Trace.Listeners.Remove(myListener);
      }
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
