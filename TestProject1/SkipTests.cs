using System;
using System.Windows;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataStructures;
using Geometry;

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


  }
}
