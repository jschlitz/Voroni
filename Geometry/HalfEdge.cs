using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geometry
{
  public class HalfEdge
  {
    /// <summary>
    /// Where the edge begins
    /// </summary>
    public Vertex Origin { get; set; }

    /// <summary>
    /// Edge that starts at this one's end, and ends at this one's origin
    /// </summary>
    public HalfEdge Twin { get; set; }

    /// <summary>
    /// Which face is this touching?
    /// </summary>
    public Face IncidentFace { get; set; }

    public HalfEdge Next { get; set; }

    public HalfEdge Previous { get; set; }
  }
}
