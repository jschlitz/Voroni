using Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using System.Collections.Generic;

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
  }
}
