using Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
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
        Assert.AreEqual<HalfEdge>(item, item.Twin.Twin);
        Assert.AreEqual<HalfEdge>(item.Twin, item.Twin.Twin.Twin);
      }
    }

    [TestMethod]
    public void RemoveTest()
    {
      Point a = new Point(2, 3);//red
      Point b = new Point(9, 10);//blue
      Point c = new Point(11, 6);//purple
      var dummy = new HalfEdgeStructure();
      VoroniBuilder.StatusStructure target = new VoroniBuilder.StatusStructure(5261975, dummy);
      SkipNode<VoroniBuilder.Triple> itemNode = null;

      target.Directix = b.Y;
      target.Add(new VoroniBuilder.Triple(b), out itemNode); // xbx
      Assert.AreEqual(b, itemNode.Value.Center);
      Assert.AreEqual(1, target.Count);
      target.Directix = c.Y;
      target.Add(new VoroniBuilder.Triple(c), out itemNode); // xbc bcb cbx
      Assert.AreEqual(b, itemNode.Value.Left);
      Assert.AreEqual(c, itemNode.Value.Center);
      Assert.AreEqual(b, itemNode.Value.Right);
      Assert.AreEqual(3, target.Count);
      target.Directix = a.Y;
      target.Add(new VoroniBuilder.Triple(a), out itemNode); // xba bab abc bcb cbx
      Assert.AreEqual(b, itemNode.Value.Left);
      Assert.AreEqual(a, itemNode.Value.Center);
      Assert.AreEqual(b, itemNode.Value.Right);
      Assert.AreEqual(5, target.Count);

      //now the hard part.
      target.Directix = 1;
      VoroniBuilder.Triple nodeToDelete = itemNode.Next().Value; //happens to be the next one in line.
      nodeToDelete.VanishEvent = new VoroniBuilder.CircleEvent(itemNode.Next(), new Circle(a, b, c));
      target.Remove(nodeToDelete, out itemNode); // xba bac  acb cbx
      Assert.AreEqual(4, target.Count);
      Assert.AreEqual(a, itemNode.Value.Left);
      Assert.AreEqual(b, itemNode.Value.Center);
      Assert.AreEqual(c, itemNode.Value.Right);

      //hot damn, now check the rest of the list is kosher.
      var asArray =  target.ToArray();
      Assert.AreEqual(null, asArray[0].Left);
      Assert.AreEqual(b, asArray[0].Center);
      Assert.AreEqual(a, asArray[0].Right);
      Assert.AreEqual(b, asArray[1].Left);
      Assert.AreEqual(a, asArray[1].Center);
      Assert.AreEqual(c, asArray[1].Right);
      Assert.AreEqual(a, asArray[2].Left);
      Assert.AreEqual(c, asArray[2].Center);
      Assert.AreEqual(b, asArray[2].Right);
      Assert.AreEqual(c, asArray[3].Left);
      Assert.AreEqual(b, asArray[3].Center);
      Assert.AreEqual(null, asArray[3].Right);

        
    }

    /// <summary>
    ///A test for Add
    ///</summary>
    [TestMethod]
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
    }
  }
}
