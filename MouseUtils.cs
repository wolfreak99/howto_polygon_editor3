using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace howto_polygon_editor3
{
    public static class MouseUtils
    {
        // We're over an object if the distance squared
        // between the mouse and the object is less than this.
        private const int over_dist_squared = Polygon.object_radius * Polygon.object_radius;
        
        // See if the mouse is over a corner point.
        public static bool MouseIsOverCornerPoint(Point mouse_pt, List<Polygon> polygons, out Polygon hit_polygon, out int hit_pt)
        {
            // See if we're over a corner point.
            foreach (Polygon polygon in polygons)
            {
                // See if we're over one of the polygon's corner points.
                for (int i = 0; i < polygon.Count; i++)
                {
                    // See if we're over this point.
                    if (FindDistanceToPointSquared(polygon[i], mouse_pt) < over_dist_squared)
                    {
                        // We're over this point.
                        hit_polygon = polygon;
                        hit_pt = i;
                        return true;
                    }
                }
            }

            hit_polygon = null;
            hit_pt = -1;
            return false;
        }

        public static bool MouseIsOverCornerPoint(Point mouse_pt, List<Polygon> polygons)
        {
            Polygon hit_polygon;
            int hit_pt;
            return MouseIsOverCornerPoint(mouse_pt, polygons, out hit_polygon, out hit_pt);
        }

        // See if the mouse is over a polygon's edge.
        public static bool MouseIsOverEdge(Point mouse_pt, List<Polygon> polygons, out Polygon hit_polygon, out int hit_pt1, out int hit_pt2, out Point closest_point)
        {
            // Examine each polygon.
            // Examine them in reverse order to check the ones on top first.
            for (int pgon = polygons.Count - 1; pgon >= 0; pgon--)
            {
                Polygon polygon = polygons[pgon];

                // See if we're over one of the polygon's segments.
                for (int p1 = 0; p1 < polygon.Count; p1++)
                {
                    // Get the index of the polygon's next point.
                    int p2 = (p1 + 1) % polygon.Count;

                    // See if we're over the segment between these points.
                    PointF closest;
                    if (FindDistanceToSegmentSquared(mouse_pt,
                        polygon[p1], polygon[p2], out closest) < over_dist_squared)
                    {
                        // We're over this segment.
                        hit_polygon = polygon;
                        hit_pt1 = p1;
                        hit_pt2 = p2;
                        closest_point = Point.Round(closest);
                        return true;
                    }
                }
            }

            hit_polygon = null;
            hit_pt1 = -1;
            hit_pt2 = -1;
            closest_point = new Point(0, 0);
            return false;
        }

        public static bool MouseIsOverEdge(Point mouse_pt, List<Polygon> polygons)
        {
            Polygon hit_polygon;
            int hit_pt1, hit_pt2;
            Point closest_point;
            return MouseIsOverEdge(mouse_pt, polygons, out hit_polygon, out hit_pt1, out hit_pt2, out closest_point);
        }

        // See if the mouse is over a polygon's body.
        public static bool MouseIsOverPolygon(Point mouse_pt, List<Polygon> polygons, out Polygon hit_polygon)
        {
            // Examine each polygon.
            // Examine them in reverse order to check the ones on top first.
            for (int i = polygons.Count - 1; i >= 0; i--)
            {
                // Make a GraphicsPath representing the polygon.
                GraphicsPath path = new GraphicsPath();
                path.AddPolygon(polygons[i].ToArray());

                // See if the point is inside the GraphicsPath.
                if (path.IsVisible(mouse_pt))
                {
                    hit_polygon = polygons[i];
                    return true;
                }
            }

            hit_polygon = null;
            return false;
        }

        public static bool MouseIsOverPolygon(Point mouse_pt, List<Polygon> polygons)
        {
            Polygon hit_polygon;
            return MouseIsOverPolygon(mouse_pt, polygons, out hit_polygon);
        }

        #region DistanceFunctions

        // Calculate the distance squared between two points.
        private static int FindDistanceToPointSquared(Point pt1, Point pt2)
        {
            int dx = pt1.X - pt2.X;
            int dy = pt1.Y - pt2.Y;
            return dx * dx + dy * dy;
        }

        // Calculate the distance squared between
        // point pt and the segment p1 --> p2.
        private static double FindDistanceToSegmentSquared(PointF pt, PointF p1, PointF p2, out PointF closest)
        {
            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            float t = ((pt.X - p1.X) * dx + (pt.Y - p1.Y) * dy) / (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new PointF(p1.X, p1.Y);
                dx = pt.X - p1.X;
                dy = pt.Y - p1.Y;
            }
            else if (t > 1)
            {
                closest = new PointF(p2.X, p2.Y);
                dx = pt.X - p2.X;
                dy = pt.Y - p2.Y;
            }
            else
            {
                closest = new PointF(p1.X + t * dx, p1.Y + t * dy);
                dx = pt.X - closest.X;
                dy = pt.Y - closest.Y;
            }

            // return Math.Sqrt(dx * dx + dy * dy);
            return dx * dx + dy * dy;
        }

        #endregion DistanceFunctions

    }
}
