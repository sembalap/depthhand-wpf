using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace depthhand_wpf
{
    class ConvexHull
    {
        public ConvexHull(IList<Point> points)
        {
            this.Points = points;
        }

        public ConvexHull()
        {
            // TODO: Complete member initialization
        }

        public IList<Point> Points { get; protected set; }

        public int Count
        {
            get { return Points.Count; }
        }    
    }
}
