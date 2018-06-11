using System;
using System.Collections.Generic;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;

namespace howto_polygon_editor3
{
    public class Polygon : IEnumerable<Point>
    {
        // The "size" of an object for mouse over purposes.
        public const int object_radius = 3;

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

        public void Move(Point offset)
        {
            for (int i = 0; i < points.Count; i++) {
                points[i] = new Point(points[i].X + offset.X, points[i].Y + offset.Y);
            }
        }

        public void Draw(PaintEventArgs e)
        {
            // Draw the polygon.
            e.Graphics.FillPolygon(Brushes.White, points.ToArray());
            e.Graphics.DrawPolygon(Pens.Blue, points.ToArray());

            // Draw the corners.
            foreach (Point corner in points) {
                Rectangle rect = new Rectangle(
                    corner.X - object_radius,
                    corner.Y - object_radius,
                    2 * object_radius + 1,
                    2 * object_radius + 1
                );
                e.Graphics.FillEllipse(Brushes.White, rect);
                e.Graphics.DrawEllipse(Pens.Black, rect);
            }
        }
        
        // Draw a polygon that is currently being built
        public void DrawNew(PaintEventArgs e, Point newPoint)
        {
            // Draw the new polygon.
            if (points.Count > 1) {
                e.Graphics.DrawLines(Pens.Green, points.ToArray());
            }

            // Draw the newest edge.
            if (points.Count > 0) {
                using (Pen dashed_pen = new Pen(Color.Green)) {
                    dashed_pen.DashPattern = new float[] { 3, 3 };
                    e.Graphics.DrawLine(dashed_pen,
                        points[points.Count - 1],
                        newPoint);
                }
            }
        }
    }
}
