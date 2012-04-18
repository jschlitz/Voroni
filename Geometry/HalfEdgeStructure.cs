using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Geometry
{
  /// <summary>
  /// A structure that describes a set of faces, the half-edges that seperate 
  /// them, and the verticies they are composed of
  /// </summary>
  public class HalfEdgeStructure
  {
    public HalfEdgeStructure()
    {
      Faces = new List<Face>();
      Edges = new List<HalfEdge>();
      Verticies = new List<Vertex>();
    }

    /// <summary>
    /// The faces
    /// </summary>
    public List<Face> Faces { get; protected set; }

    /// <summary>
    /// The edges that go ccw around the faces
    /// </summary>
    public List<HalfEdge> Edges { get; protected set; }

    /// <summary>
    /// The verticies that the edges connect to.
    /// </summary>
    public List<Vertex> Verticies { get; protected set; }

    /// <summary>
    /// Given a vertex, give all the edges radiate from here.
    /// </summary>
    public IEnumerable<HalfEdge> GetAllEdgesFromVertex(Vertex v)
    {
      throw new NotImplementedException();
    }
  }
}
