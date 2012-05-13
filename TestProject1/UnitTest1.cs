using System;
using System.Windows;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Voroni;

namespace TestProject1
{
  /// <summary>
  /// Summary description for UnitTest1
  /// </summary>
  [TestClass]
  public class UnitTest1
  {
    public UnitTest1()
    {
    }

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
    // You can use the following additional attributes as you write your tests:
    //
    // Use ClassInitialize to run code before running the first test in the class
    // [ClassInitialize()]
    // public static void MyClassInitialize(TestContext testContext) { }
    //
    // Use ClassCleanup to run code after all tests in a class have run
    // [ClassCleanup()]
    // public static void MyClassCleanup() { }
    //
    // Use TestInitialize to run code before running each test 
    // [TestInitialize()]
    // public void MyTestInitialize() { }
    //
    // Use TestCleanup to run code after each test has run
    // [TestCleanup()]
    // public void MyTestCleanup() { }
    //
    #endregion

    [TestMethod]
    public void TestComparer()
    {
      Point p1 = new Point(3, 5);
      Point p2 = new Point(4, 2);
      Assert.IsTrue(PointComparer.Comparer.Compare(p1, p2) < 0);
      Assert.IsTrue(PointComparer.Comparer.Compare(p2, p1) > 0);
      Assert.IsTrue(PointComparer.Comparer.Compare(p1, p1) == 0);
      Point p3 = new Point(3, 2);
      Assert.IsTrue(PointComparer.Comparer.Compare(p1, p3) > 0);
      Assert.IsTrue(PointComparer.Comparer.Compare(p3, p1) < 0);
      Assert.IsTrue(PointComparer.Comparer.Compare(p3, p2) < 0);
      Assert.IsTrue(PointComparer.Comparer.Compare(p2, p3) > 0);
    }

    [TestMethod]
    public void TestSlope()
    {
      Point p1 = new Point(3, 5);
      Point p2 = new Point(4, 2);
      var ls = new LineSegment(p1, p2);
      Assert.AreEqual(ls.Slope, -3.0);
      ls = new LineSegment(p2, p1);
      Assert.AreEqual(ls.Slope, -3.0);

      p1 = new Point(4, 1);
      p2 = new Point(7, -1);
      ls = new LineSegment(p1, p2);
      Assert.AreEqual(ls.Slope, -(2.0/3.0));
    }

    [TestMethod]
    public void TestIntersect()
    {
      var p1 = new Point(3, 5);
      var p2 = new Point(4, 2);
      var ls = new LineSegment(p1, p2);
      Assert.IsTrue(ls.Intersects(p1));
      Assert.IsTrue(ls.Intersects(p2));
      Assert.IsTrue(ls.Intersects(new Point(3.0 + 1.0 / 3.0, 4.0)));
      Assert.IsFalse(ls.Intersects(new Point(3.0, 4.0)));
      Assert.IsFalse(ls.Intersects(new Point(3.33, 4.0)));
      Assert.IsFalse(ls.Intersects(new Point(5.0, -1.0)));
    }

    [TestMethod]
    public void TestLineIntersect()
    {
      //Assert.IsTrue(LineSegment.NO_INTERSECTION == new Point(double.NaN, double.NaN), "Oh, snap.");

      var a = new Point(1, 2);
      var b = new Point(3, 3);
      var c = new Point(5, 4);
      var d = new Point(7, 5);
      var e = new Point(5, 3);
      var f = new Point(5, 7);
      var g = new Point(-1, 3);

      ShouldCross(a, d, e, f, c);
      ShouldCross(a, c, e, f, c);
      ShouldCross(a, d, c, f, c);
      ShouldCross(a, d, e, c, c);

      ShouldCross(a, d, e, g, b);
      ShouldCross(b, d, e, g, b);
      ShouldCross(a, b, e, g, b);
      ShouldCross(a, d, b, g, b);
      ShouldCross(a, d, e, b, b);

      Point tmp;
      var ad = new LineSegment(a, d);
      Assert.IsTrue(ad.GetIntersection(new LineSegment(b, c), out tmp), "Colinear fail");
      var cd = new LineSegment(c, d);
      Assert.IsFalse(cd.GetIntersection(new LineSegment(a, b), out tmp), "Parallel fail");
      Assert.IsFalse(cd.GetIntersection(new LineSegment(a, e), out tmp), "what? " + tmp.ToString());
    }

    private static void ShouldCross(Point a, Point d, Point e, Point f, Point crossAt)
    {
      Point tmp;
      (new LineSegment(a, d)).GetIntersection(new LineSegment(f, e), out tmp);
      Assert.IsTrue(crossAt == tmp, tmp.ToString());
      (new LineSegment(d, a)).GetIntersection(new LineSegment(f, e), out tmp);
      Assert.IsTrue(crossAt == tmp, tmp.ToString());
      (new LineSegment(d, a)).GetIntersection(new LineSegment(e, f), out tmp);
      Assert.IsTrue(crossAt == tmp, tmp.ToString());
      (new LineSegment(a, d)).GetIntersection(new LineSegment(e, f), out tmp);
      Assert.IsTrue(crossAt == tmp, tmp.ToString());
    }

    [TestMethod]
    public void TestInsidePoly()
    {
      Polygon poly1 = new Polygon(new Point[] {
        new Point(0,7), 
        new Point(3,4), 
        new Point(4,7), 
        new Point(5,4), 
        new Point(5,2), 
        new Point(3,0), 
        new Point(0,4)});
      
      //Inside!
      Polygon truePoly = new Polygon(new Point[] { 
        new Point(3, 2),
        new Point(3, 3),
        new Point(2, 4),
        new Point(1, 5),
        new Point(4, 5)});

      CheckInside(poly1, truePoly.Verticies, true);

      //Outside!
      Polygon falsePoly = new Polygon(new Point[] { 
        new Point(0, 0),
        new Point(4.5, 0.5),
        new Point(-3, 7),
        new Point(2, 7),
        new Point(5, 7),
        new Point(3, 8),
        new Point(6, 4),
        new Point(0, 0),
        new Point(-1, 4),
        new Point(-1, 5)});

      CheckInside(poly1, falsePoly.Verticies, false);

      //should work if they're negative.
      var shift = new Vector(-8, -10);
      poly1.Translate(shift);
      truePoly.Translate(shift);
      falsePoly.Translate(shift);
      CheckInside(poly1, truePoly.Verticies, true);
      CheckInside(poly1, falsePoly.Verticies, false);

      //checks colinear
      poly1 = new Polygon(new Point[] {
        new Point(1,5), 
        new Point(6,5), 
        new Point(6,0), 
        new Point(5,2), 
        new Point(2,2), 
        new Point(0,0)});

      truePoly = new Polygon(new Point[] { 
        new Point(1, 2),
        new Point(5.5, 2),
        new Point(3, 3)});
      falsePoly = new Polygon(new Point[] { 
        new Point(-1, 0),
        new Point(0, 5),
        new Point(3, 0)});
      CheckInside(poly1, truePoly.Verticies, true);
      CheckInside(poly1, falsePoly.Verticies, false);


      shift = new Vector(-2, -2);
      poly1.Translate(shift);
      truePoly.Translate(shift);
      falsePoly.Translate(shift);
      CheckInside(poly1, truePoly.Verticies, true);
      CheckInside(poly1, falsePoly.Verticies, false);

    }

    private void CheckInside(Polygon poly, IEnumerable<Point> points, bool shouldBeInside)
    {
      foreach (var p in points)
        Assert.IsTrue(poly.Contains(p) == shouldBeInside);
    }
  }
}
