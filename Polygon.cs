using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;

namespace howto_polygon_editor3
{
    public class Polygon : IEnumerable<Point>
    {
        private List<Point> points;

        public Polygon()
        {
            points = new List<Point>();
        }

        public Point this[int index] {
            get { return points[index]; }
            set { points[index] = value; }
        }

        public void Add(Point item)
        {
            points.Add(item);
        }
        
        public void Insert(int index, Point item)
        {
            points.Insert(index, item);
        }

        public int Count { get { return points.Count; } }

        public Point[] ToArray()
        {
            return points.ToArray();
        }

        public IEnumerator<Point> GetEnumerator()
        {
            return points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return points.GetEnumerator();
        }
    }
}
