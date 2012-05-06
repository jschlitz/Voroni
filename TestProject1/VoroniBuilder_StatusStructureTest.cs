using Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using DataStructures;

namespace TestProject1
{
    
    
    /// <summary>
    ///This is a test class for VoroniBuilder_StatusStructureTest and is intended
    ///to contain all VoroniBuilder_StatusStructureTest Unit Tests
    ///</summary>
  [TestClass()]
  public class VoroniBuilder_StatusStructureTest
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

    private void AssertEdgeTwins(HalfEdgeStructure hes)
    {
      foreach (var item in hes.Edges)
      {
        Assert.IsNotNull(item.Twin);
        Assert.IsTrue(hes.Edges.Contains(item.Twin));
      }
    }

    /// <summary>
    ///A test for Add
    ///</summary>
    [TestMethod()]
    public void AddTest()
    {
      var dummy = new HalfEdgeStructure() ;
      VoroniBuilder.StatusStructure target = new VoroniBuilder.StatusStructure(5261975, dummy);
      SkipNode<VoroniBuilder.Triple> itemNode = null;

      target.Directix = P2.Y;
      target.Add(new VoroniBuilder.Triple(P2), out itemNode);//x2x
      Assert.AreEqual(null, itemNode.Value.Left);
      Assert.AreEqual(P2, itemNode.Value.Center);
      Assert.AreEqual(null, itemNode.Value.Right);
      Assert.AreEqual(1, target.Count);
      AssertEdgeTwins(dummy);

      target.Directix = P4.Y;
      target.Add(new VoroniBuilder.Triple(P4), out itemNode);//x24, 242, 42x
      Assert.AreEqual(P2, itemNode.Value.Left);
      Assert.AreEqual(P4, itemNode.Value.Center);
      Assert.AreEqual(P2, itemNode.Value.Right);
      Assert.AreEqual(null, itemNode.Previous.Value.Left);
      Assert.AreEqual(P2, itemNode.Previous.Value.Center);
      Assert.AreEqual(P4, itemNode.Previous.Value.Right);
      Assert.AreEqual(P4, itemNode.Next().Value.Left);
      Assert.AreEqual(P2, itemNode.Next().Value.Center);
      Assert.AreEqual(null, itemNode.Next().Value.Right);
      Assert.AreEqual(3, target.Count);
      AssertEdgeTwins(dummy);

      target.Directix = P1.Y;
      target.Add(new VoroniBuilder.Triple(P1), out itemNode);//x21, 212, 124, 242, 42x
      Assert.AreEqual(P2, itemNode.Value.Left);
      Assert.AreEqual(P1, itemNode.Value.Center);
      Assert.AreEqual(P2, itemNode.Value.Right);
      Assert.AreEqual(null, itemNode.Previous.Value.Left);
      Assert.AreEqual(P2, itemNode.Previous.Value.Center);
      Assert.AreEqual(P1, itemNode.Previous.Value.Right);
      Assert.AreEqual(P1, itemNode.Next().Value.Left);
      Assert.AreEqual(P2, itemNode.Next().Value.Center);
      Assert.AreEqual(P4, itemNode.Next().Value.Right);
      Assert.AreEqual(5, target.Count);
      AssertEdgeTwins(dummy);

      target.Directix = P3.Y;
      target.Add(new VoroniBuilder.Triple(P3), out itemNode);//x21, 212, 123, 232, 324, 242, 42x
      Assert.AreEqual(P2, itemNode.Value.Left);
      Assert.AreEqual(P3, itemNode.Value.Center);
      Assert.AreEqual(P2, itemNode.Value.Right);
      Assert.AreEqual(P1, itemNode.Previous.Value.Left);
      Assert.AreEqual(P2, itemNode.Previous.Value.Center);
      Assert.AreEqual(P3, itemNode.Previous.Value.Right);
      Assert.AreEqual(P3, itemNode.Next().Value.Left);
      Assert.AreEqual(P2, itemNode.Next().Value.Center);
      Assert.AreEqual(P4, itemNode.Next().Value.Right);
      Assert.AreEqual(7, target.Count);
      AssertEdgeTwins(dummy);

      target.Directix = P6.Y;
      target.Add(new VoroniBuilder.Triple(P6), out itemNode);//x21, 212, 123, 232, 324, 242, 426, 262, 62x
      Assert.AreEqual(P2, itemNode.Value.Left);
      Assert.AreEqual(P6, itemNode.Value.Center);
      Assert.AreEqual(P2, itemNode.Value.Right);
      Assert.AreEqual(P4, itemNode.Previous.Value.Left);
      Assert.AreEqual(P2, itemNode.Previous.Value.Center);
      Assert.AreEqual(P6, itemNode.Previous.Value.Right);
      Assert.AreEqual(P6, itemNode.Next().Value.Left);
      Assert.AreEqual(P2, itemNode.Next().Value.Center);
      Assert.AreEqual(null, itemNode.Next().Value.Right);
      Assert.AreEqual(9, target.Count);
      AssertEdgeTwins(dummy);

      //in the real app, we'll be removing by direct reference. Still. this ought to work.
      double removeAtX = (new VoroniBuilder.Triple(P3, P2, P4)).FindAnX(1.1);
      target.Remove(new VoroniBuilder.Triple(new Point(removeAtX, 0)), out itemNode); //x21, 212, 123, 234, 342, 426, 262, 62x
      Assert.AreEqual(P3, itemNode.Value.Left);
      Assert.AreEqual(P2, itemNode.Value.Center);
      Assert.AreEqual(P4, itemNode.Value.Right);
      Assert.AreEqual(P2, itemNode.Previous.Value.Left);
      Assert.AreEqual(P3, itemNode.Previous.Value.Center);
      Assert.AreEqual(P4, itemNode.Previous.Value.Right);
      Assert.AreEqual(P3, itemNode.Next().Value.Left);
      Assert.AreEqual(P4, itemNode.Next().Value.Center);
      Assert.AreEqual(P2, itemNode.Next().Value.Right);
      Assert.AreEqual(8, target.Count);
      AssertEdgeTwins(dummy);

    }
  }
}
