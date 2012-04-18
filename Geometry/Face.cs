using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Geometry
{
  public class Face
  {
    public Face(Point site)
    {
      Site = site;
    }

    /// <summary>
    /// One of the edges in the outer edge of the face
    /// </summary>
    public HalfEdge OuterEdge { get; set; }

    public Point Site { get; set; }
  }
}
