using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Geometry
{
  public class Vertex
  {
    /// <summary>
    /// The point
    /// </summary>
    public Point Coordinates { get; set; }
    public HalfEdge IncidentEdge { get; set; }
  }
}
