using Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;

namespace TestProject1
{
    
    
    /// <summary>
    ///This is a test class for VoroniBuilder_TripleTest and is intended
    ///to contain all VoroniBuilder_TripleTest Unit Tests
    ///</summary>
  [TestClass()]
  public class VoroniBuilder_TripleTest
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

    static Point P1 = new Point(-5, 1.1);
    static Point P2 = new Point(-3, 11);
    static Point P3 = new Point(-1, 1.1);
    static Point P4 = new Point(1, 1.5);
    static Point P6 = new Point(5, 1.1);

    /// <summary>
    ///A test for FindAnX
    ///</summary>
    [TestMethod()]
    public void FindAnXTest()
    {
      VoroniBuilder.Triple target = new VoroniBuilder.Triple(P3, P4, P6); 
      double directix = 1F; 
      double actual = target.FindAnX(directix);
      Assert.IsTrue(CloseEnough(actual, 1.69648));

      target = new VoroniBuilder.Triple(null, P4, P6);
      actual = target.FindAnX(directix);
      Assert.IsTrue(CloseEnough(actual, 2.75278));
      Assert.IsFalse(double.IsInfinity(actual));

      target = new VoroniBuilder.Triple(P3, P4, null);
      actual = target.FindAnX(directix);
      Assert.IsTrue(CloseEnough(actual, 0.64018));
      Assert.IsFalse(double.IsInfinity(actual));

      target = new VoroniBuilder.Triple(P1, P3, P6);
      actual = target.FindAnX(directix);
      Assert.IsTrue(CloseEnough(actual, -0.5));
    }

    private static bool CloseEnough(double d1, double d2)
    {
      return Math.Round(d1, 3) == Math.Round(d2, 3);
    }
  }
}
