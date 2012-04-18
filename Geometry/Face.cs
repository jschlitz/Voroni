using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Geometry
{
  public class Face
  {
    /// <summary>
    /// One of the edges in the outer edge of the face
    /// </summary>
    HalfEdge OuterComponent { get; set; }

    /// <summary>
    /// One of the edges in any hole in the face
    /// </summary>
    HalfEdge InnerComponent { get; set; }
  }
}
