using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Geometry
{
  public class Vertex
  {
    public override string ToString()
    {
      return "(v:" + Coordinates.X.ToString("f2") + "," + Coordinates.Y.ToString("f2") + ")";
    }
    /// <summary>
    /// The point
    /// </summary>
    public Point Coordinates { get; set; }
    public HalfEdge IncidentEdge { get; set; }
  }
}
