using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Geometry
{
  public class HalfEdge
  {
    /// <summary>
    /// This will actually make the halfedge and its twin.
    /// </summary>
    public HalfEdge(Face incidentFace, Face opposingFace)
    {
      System.Diagnostics.Debug.Assert(incidentFace != null);
      System.Diagnostics.Debug.Assert(opposingFace != null);

      IncidentFace = incidentFace;
      Twin = new HalfEdge(opposingFace);
      Twin.Twin = this;
    }

    /// <summary>
    /// Only used internally.
    /// </summary>
    private HalfEdge(Face incidentFace)
    {
      IncidentFace = incidentFace;
    }

    /// <summary>
    /// Where the edge begins
    /// </summary>
    public Vertex Origin { get; set; }

    /// <summary>
    /// Edge that starts at this one's end, and ends at this one's origin
    /// </summary>
    public HalfEdge Twin { get; protected set; }

    /// <summary>
    /// Which face is this touching?
    /// </summary>
    public Face IncidentFace { get; protected set; }

    public HalfEdge Next { get; set; }

    public HalfEdge Previous { get; set; }

    public override string ToString()
    {
      if (IncidentFace == null)
        return "{eg:noface?!}";
      else if (Twin == null)
        return "{eg:" + IncidentFace.ToString() + ", {notwin?!}}";
      else 
        return "{eg:" + IncidentFace.ToString() + ", " + Twin.IncidentFace.ToString() + "}";
    }
  }
}
