using Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;

namespace TestProject1
{
    
    
    /// <summary>
    ///This is a test class for CircleTest and is intended
    ///to contain all CircleTest Unit Tests
    ///</summary>
  [TestClass()]
  public class CircleTest
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
    ///A test for Circle Constructor
    ///</summary>
    [TestMethod()]
    public void CircleConstructorTest()
    {
      CheckCircle(new Point(3, 4), new Point(5, 0), new Point(-4, 3), new Point(0, 0), 5);
      CheckCircle(new Point(13, 104), new Point(15, 100), new Point(6, 103), new Point(10, 100), 5);
      CheckCircle(new Point(-5, 5), new Point(Math.Sqrt(21) - 2, 3), new Point(Math.Sqrt(12.5) - 2, 1 - Math.Sqrt(12.5)), new Point(-2, 1), 5);

      //CheckCircle(new Point(3, -7), new Point(3, -7), new Point(3, -7), new Point(3, -7), 9);
      CheckCircle(new Point(3 - 3, -7 - Math.Sqrt(72)), new Point(3 - Math.Sqrt(45), -7 - 6), new Point(3 - 1, -7 - Math.Sqrt(80)), new Point(3, -7), 9);

    
    }

    private static void CheckCircle(Point p1, Point p2, Point p3, Point expectedC, double expectedR)
    {
      Circle target = new Circle(p1, p2, p3);
      Assert.IsTrue(CloseEnough(expectedR, target.Radius));
      Assert.IsTrue(CloseEnough(expectedC.X, target.Center.X));
      Assert.IsTrue(CloseEnough(expectedC.Y, target.Center.Y));
    }

    private static bool CloseEnough(double d1, double d2)
    {
      return Math.Round(d1, 3) == Math.Round(d2, 3);
    }


  }
}
