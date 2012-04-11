using System;
using System.Windows;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataStructures;

namespace TestProject1
{
  [TestClass]
  public class SkipTests
  {
    /// <summary>
    /// Thin class to expose some internals for testing.
    /// </summary>
    class TestSkipList<T>:SkipList<T>
    {

       //but will get see changes in subfolders?
    #region constructors
    public TestSkipList(int seed, IComparer<T> c):base(seed, c)
    {}

    public TestSkipList():base()
    {}

    public TestSkipList(int seed): base(seed)
    {}

    public TestSkipList(IComparer<T> c):base(c)
    {}
    #endregion

      protected int GetHeightAccess()
      {
        return GetHeight();
      }

      public bool CheckIntegrityAccess()
      {
        return CheckIntegrity();
      }
    }

    [TestMethod]
    public void TestMethod1()
    {
      //seeds
      BigMessOfTests(-1000, 1000);
      BigMessOfTests(int.MinValue, int.MaxValue);
    }

    private void BigMessOfTests(int minRange, int maxRange)
    {
      var listSeeds = new int[] { 33, 44, 03, 41, 57, 44 };
      var valueSeeds = new int[] { 78, 41, 12, 59, 97, 49 };

      for (int i = 0; i < listSeeds.Length; i++)
      {
        var target = new TestSkipList<int>(listSeeds[i]);
        var verf = new List<int>();

        //add some things that we'll remove later
        for (int j = -1000; j < 1000; j+=10)
        {
          verf.Add(j);
          target.Add(j);
          Assert.IsTrue(target.Contains(j));
        }

        verf.Sort();
        CheckIt(verf, target);
        
        //Some random values
        var r = new Random(valueSeeds[i]);
        for (int j = 0; j < 10000; j++)
        {
          int temp = r.Next(minRange, maxRange);
          verf.Add(temp);
          target.Add(temp);
        }
        verf.Sort();
        CheckIt(verf, target);

        //now remove items that we know to be in the list
        for (int j = -1000; j < 1000; j += 10)
        {
          Assert.IsTrue(verf.Remove(j));
          Assert.IsTrue(target.Remove(j));
        }
        Assert.AreEqual(verf.Count, target.Count);
        CheckIt(verf, target);

        //And just to be sure, add some new items.
        for (int j = 0; j < 1000; j++)
        {
          int temp = r.Next(int.MinValue, int.MaxValue);
          verf.Add(temp);
          target.Add(temp);
        }
        verf.Sort();
        CheckIt(verf, target);
      }
    }

    private void CheckIt(List<int> verf, TestSkipList<int> target)
    {
      Assert.AreEqual(verf.Count, target.Count);
      Assert.AreEqual(target.Count(), target.Count);
      Assert.AreEqual(target.Sum(x=>1), target.Count);
      
      int index = 0;
      foreach (int targetVal in target)
        Assert.AreEqual(targetVal, verf[index++]);

      target.CheckIntegrityAccess();
    }

    [TestMethod]
    public void ParabolaIntersections()
    {
      Tuple<Point, Point> points;
      var p110 = new Parabola(new Point(1, 1), 0);
      var p120 = new Parabola(new Point(1, 2), 0);
      var p230 = new Parabola(new Point(2, 3), 0);
      var p340 = new Parabola(new Point(3, 4), 0);

      CheckIntersection(p110, p120);
      CheckIntersection(p120, p230);
      CheckIntersection(p230, p340);
      CheckIntersection(p340, p110);

      //same y, 1 intersection
      var p410 = new Parabola(new Point(4, 1), 0);
      points = CheckIntersection(p110, p410);
      Assert.IsTrue(Parabola.IsNoIntersection(points.Item2));

      //one that goes the other way
      var p357 = new Parabola(new Point(3, 5), 7);
      points = CheckIntersection(p110, p357);

      //no intersection
      var p131 = new Parabola(new Point(1, 3), 1);
      points = CheckIntersection(p120, p131);
      Assert.IsTrue(Parabola.IsNoIntersection(points.Item1));
      Assert.IsTrue(Parabola.IsNoIntersection(points.Item2));

      //0 the other way
      var p001 = new Parabola(new Point(0, 0), 1);
      points = CheckIntersection(p001, p340);
      Assert.IsTrue(Parabola.IsNoIntersection(points.Item1));
      Assert.IsTrue(Parabola.IsNoIntersection(points.Item2));

      //ray straight up
      var p000 = new Parabola(new Point(0, 0), 0);
      points = p110.Intersect(p000);
      Assert.IsTrue(CloseEnough(p110.GetY(0), points.Item1.Y));
      Assert.IsTrue(CloseEnough(0, points.Item1.X));

      var p411 = new Parabola(new Point(4, 1), 1);
      points = p110.Intersect(p411);
      Assert.IsTrue(CloseEnough(p110.GetY(4), points.Item1.Y));
      Assert.IsTrue(CloseEnough(4, points.Item1.X));

    }

    private static Tuple<Point, Point> CheckIntersection(Parabola first, Parabola second)
    {
      Tuple<Point, Point> intersections = first.Intersect(second);
      string err = ErrStr(first, second, intersections);
      if (!Parabola.IsNoIntersection(intersections.Item1))
        Assert.IsTrue(CloseEnough(first.GetY(intersections.Item1.X), second.GetY(intersections.Item1.X)), err);
      if (!Parabola.IsNoIntersection(intersections.Item2))
        Assert.IsTrue(CloseEnough(first.GetY(intersections.Item2.X), second.GetY(intersections.Item2.X)), err);

      return intersections;
    }

    private static bool CloseEnough(double d1, double d2)
    {
      return Math.Round(d1, 3) == Math.Round(d2, 3);
    }

    private static string ErrStr(Parabola p1, Parabola p2, Tuple<Point, Point> intersections)
    {
      var sb = new StringBuilder();
      AppendPoints(sb, intersections);
      sb.Append("Error at:");
      sb.Append(p1.ToString());
      sb.Append(" / ");
      sb.Append(p2.ToString());
      return sb.ToString();
    }


    private static void AppendPoints(StringBuilder sb, Tuple<Point, Point> intersections)
    {
      sb.Append(intersections.Item1);
      sb.Append(Environment.NewLine);
      sb.Append(intersections.Item2);
      sb.Append(Environment.NewLine);
    }

    public class Parabola
    {
      public Parabola(Point p, double yLine)
      {
        Focus = p;
        Directix = yLine;
      }

      public override string ToString()
      {
        return Focus.X.ToString("f2") + "," + Focus.Y.ToString("f2") + "|" + Directix.ToString("f2");
      }

      public Point Focus { get; set; }
      public double Directix { get; set; }

      private bool Cannonicalize(out double a, out double b, out double c)
      {
        if (Directix == Focus.Y)
        {
          a = double.NaN;
          b = double.NaN;
          c = double.NaN;
          return false;
        }
        else
        {
          a = 1 / (2 * (Focus.Y - Directix));
          b = (-1 * Focus.X) / (Focus.Y - Directix);
          c = (Sq(Focus.X) + Sq(Focus.Y) - Sq(Directix)) / (2 * (Focus.Y - Directix));
          return true;
        }
      }

      /// <summary>
      /// For given x, get y.
      /// </summary>
      public double GetY(double x)
      {
        double a, b, c;
        if (!Cannonicalize(out a, out b, out c))
          return double.NaN;
        return a*x*x + b*x + c;
      }

      /// <summary>
      /// 0, 1 or 2 intersections between the 2 paraboloas
      /// </summary>
      public Tuple<Point, Point> Intersect(Parabola other)
      {

        //put in cannonical form
        double a1, a2, b1, b2, c1, c2;
        bool ok1 = Cannonicalize(out a1, out b1, out c1);
        bool ok2 = other.Cannonicalize(out a2, out b2, out c2);

        //some degenerate cases
        if (!ok1 && !ok2) return new Tuple<Point, Point>(NO_INTERSECTION, NO_INTERSECTION);
        if (ok1 && !ok2)
          return new Tuple<Point, Point>(CalcPoint(other.Focus.X, a1, b1, c1), NO_INTERSECTION);
        if (!ok1 && ok2)
          return new Tuple<Point, Point>(CalcPoint(Focus.X, a2, b2, c2), NO_INTERSECTION);

        //now subtract the cannonicals and put through quadratic
        Tuple<double, double> xes = Quadratic(a1 - a2, b1 - b2, c1 - c2);

        return new Tuple<Point, Point>(CalcPoint(xes.Item1, a1, b1, c1), CalcPoint(xes.Item2, a1, b1, c1));
      }

      private static Point CalcPoint(double x, double a1, double b1, double c1)
      {
        if (double.IsNaN(x))
          return NO_INTERSECTION;
        else
          return new Point(x, a1 * Sq(x) + b1 * x + c1);
      }

      public static readonly Point NO_INTERSECTION = new Point(double.NaN, double.NaN);

      /// <summary>
      /// Is this a pair of NaN?
      /// </summary>
      public static bool IsNoIntersection(Point p)
      {
        return double.IsNaN(p.X) && double.IsNaN(p.Y);
      }

      /// <summary>
      /// Perform quadratic on a,b,c. if a==0, solve for xb+c=0, and return NaN 
      /// for the 2nd. if a==b==0, NaN for both. 
      /// Also, don't consider imaginary solutions. if b*b &lt; 4*a*c, you get NaNs
      /// </summary>
      private Tuple<double, double> Quadratic(double a, double b, double c)
      {
        if (a == 0)
        {
          if (b == 0)
            return new Tuple<double, double>(double.NaN, double.NaN);
          else
            return new Tuple<double, double>(-1 * c / b, double.NaN);
        }
        else
        {
          var disc = b * b - 4 * a * c;
          if (disc < 0) return new Tuple<double, double>(double.NaN, double.NaN);
          disc = Math.Sqrt(disc);

          return new Tuple<double, double>((-1 * b + disc) / (2 * a), (-1 * b - disc) / (2 * a));
        }
      }

      private static double Sq(double lf) { return lf * lf; }
    }

  }
}
