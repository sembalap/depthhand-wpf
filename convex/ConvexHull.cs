﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace depthhand_wpf.convex
{
    public class ConvexHull
    {
        public ConvexHull(IList<Point> points)
        {
            this.Points = points;
        }

        public IList<Point> Points { get; protected set; }

        public int Count
        {
            get { return Points.Count; }
        }
    }
}
