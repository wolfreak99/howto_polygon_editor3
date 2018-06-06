using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Drawing.Drawing2D;

namespace howto_polygon_editor3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Each polygon is represented by a List<Point>.
        private List<Polygon> Polygons = new List<Polygon>();

        // Points for the new polygon.
        private Polygon NewPolygon = null;

        // The current mouse position while drawing a new polygon.
        private Point NewPoint;

        // The polygon and index of the corner we are moving.
        private Polygon MovingPolygon = null;
        private int MovingPoint = -1;
        private int OffsetX, OffsetY;

        // The add point cursor.
        private Cursor AddPointCursor;

        // Create the add point cursor.
        private void Form1_Load(object sender, EventArgs e)
        {
            AddPointCursor = new Cursor(Properties.Resources.add_point.GetHicon());
            MakeBackgroundGrid();
        }

        // Start or continue drawing a new polygon,
        // or start moving a corner or polygon.
        private void picCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            // See what we're over.
            Point mouse_pt = SnapToGrid(e.Location);
            Polygon hit_polygon;
            int hit_point, hit_point2;
            Point closest_point;

            if (NewPolygon != null)
            {
                // We are already drawing a polygon.
                // If it's the right mouse button, finish this polygon.
                if (e.Button == MouseButtons.Right)
                {
                    // Finish this polygon.
                    if (NewPolygon.Count > 2) Polygons.Add(NewPolygon);
                    NewPolygon = null;

                    // We no longer are drawing.
                    picCanvas.MouseMove += picCanvas_MouseMove_NotDrawing;
                    picCanvas.MouseMove -= picCanvas_MouseMove_Drawing;
                }
                else
                {
                    // Add a point to this polygon.
                    if (NewPolygon[NewPolygon.Count - 1] != mouse_pt)
                    {
                        NewPolygon.Add(mouse_pt);
                    }
                }
            }
            else if (MouseUtils.MouseIsOverCornerPoint(mouse_pt, Polygons, out hit_polygon, out hit_point))
            {
                // Start dragging this corner.
                picCanvas.MouseMove -= picCanvas_MouseMove_NotDrawing;
                picCanvas.MouseMove += picCanvas_MouseMove_MovingCorner;
                picCanvas.MouseUp += picCanvas_MouseUp_MovingCorner;

                // Remember the polygon and point number.
                MovingPolygon = hit_polygon;
                MovingPoint = hit_point;

                // Remember the offset from the mouse to the point.
                OffsetX = hit_polygon[hit_point].X - e.X;
                OffsetY = hit_polygon[hit_point].Y - e.Y;
            }
            else if (MouseUtils.MouseIsOverEdge(mouse_pt, Polygons, out hit_polygon,
                out hit_point, out hit_point2, out closest_point))
            {
                // Add a point.
                hit_polygon.Insert(hit_point + 1, closest_point);

                // Start dragging the new corner.
                picCanvas.MouseMove -= picCanvas_MouseMove_NotDrawing;
                picCanvas.MouseMove += picCanvas_MouseMove_MovingCorner;
                picCanvas.MouseUp += picCanvas_MouseUp_MovingCorner;

                // Remember the polygon and new point number.
                MovingPolygon = hit_polygon;
                MovingPoint = hit_point + 1;

                // Remember the offset from the mouse to the point.
                OffsetX = hit_polygon[hit_point + 1].X - e.X;
                OffsetY = hit_polygon[hit_point + 1].Y - e.Y;
            }
            else if (MouseUtils.MouseIsOverPolygon(mouse_pt, Polygons, out hit_polygon))
            {
                // Start moving this polygon.
                picCanvas.MouseMove -= picCanvas_MouseMove_NotDrawing;
                picCanvas.MouseMove += picCanvas_MouseMove_MovingPolygon;
                picCanvas.MouseUp += picCanvas_MouseUp_MovingPolygon;

                // Remember the polygon.
                MovingPolygon = hit_polygon;

                // Remember the offset from the mouse to the segment's first point.
                OffsetX = hit_polygon[0].X - e.X;
                OffsetY = hit_polygon[0].Y - e.Y;
            }
            else
            {
                // Start a new polygon.
                NewPolygon = new Polygon();
                NewPoint = mouse_pt;
                NewPolygon.Add(mouse_pt);

                // Get ready to work on the new polygon.
                picCanvas.MouseMove -= picCanvas_MouseMove_NotDrawing;
                picCanvas.MouseMove += picCanvas_MouseMove_Drawing;
            }

            // Redraw.
            picCanvas.Invalidate();
        }

        // Move the next point in the new polygon.
        private void picCanvas_MouseMove_Drawing(object sender, MouseEventArgs e)
        {
            NewPoint = SnapToGrid(e.Location);
            picCanvas.Invalidate();
        }

        // Move the selected corner.
        private void picCanvas_MouseMove_MovingCorner(object sender, MouseEventArgs e)
        {
            // Move the point.
            MovingPolygon[MovingPoint] =
                SnapToGrid(new Point(e.X + OffsetX, e.Y + OffsetY));

            // Redraw.
            picCanvas.Invalidate();
        }

        // Move the selected polygon.
        private void picCanvas_MouseMove_MovingPolygon(object sender, MouseEventArgs e)
        {
            // See how far the first point will move.
            int new_x1 = e.X + OffsetX;
            int new_y1 = e.Y + OffsetY;

            int dx = new_x1 - MovingPolygon[0].X;
            int dy = new_y1 - MovingPolygon[0].Y;

            // Snap the movement to a multiple of the grid distance.
            dx = GridGap * (int)(Math.Round((float)dx / GridGap));
            dy = GridGap * (int)(Math.Round((float)dy / GridGap));

            if (dx == 0 && dy == 0) return;

            // Move the polygon.
            MovingPolygon.Move(new Point(dx, dy));

            // Redraw.
            picCanvas.Invalidate();
        }

        // See if we're over a polygon or corner point.
        private void picCanvas_MouseMove_NotDrawing(object sender, MouseEventArgs e)
        {
            Cursor new_cursor = Cursors.Cross;

            // See what we're over.
            Point mouse_pt = SnapToGrid(e.Location);

            if (MouseUtils.MouseIsOverCornerPoint(mouse_pt, Polygons))
            {
                new_cursor = Cursors.Arrow;
            }
            else if (MouseUtils.MouseIsOverEdge(mouse_pt, Polygons))
            {
                new_cursor = AddPointCursor;
            }
            else if (MouseUtils.MouseIsOverPolygon(mouse_pt, Polygons))
            {
                new_cursor = Cursors.Hand;
            }

            // Set the new cursor.
            if (picCanvas.Cursor != new_cursor)
            {
                picCanvas.Cursor = new_cursor;
            }
        }

        // Finish moving the selected corner.
        private void picCanvas_MouseUp_MovingCorner(object sender, MouseEventArgs e)
        {
            picCanvas.MouseMove += picCanvas_MouseMove_NotDrawing;
            picCanvas.MouseMove -= picCanvas_MouseMove_MovingCorner;
            picCanvas.MouseUp -= picCanvas_MouseUp_MovingCorner;
        }

        // Finish moving the selected polygon.
        private void picCanvas_MouseUp_MovingPolygon(object sender, MouseEventArgs e)
        {
            picCanvas.MouseMove += picCanvas_MouseMove_NotDrawing;
            picCanvas.MouseMove -= picCanvas_MouseMove_MovingPolygon;
            picCanvas.MouseUp -= picCanvas_MouseUp_MovingPolygon;
        }

        // Redraw old polygons in blue. Draw the new polygon in green.
        // Draw the final segment dashed.
        private void picCanvas_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Draw the old polygons.
            foreach (Polygon polygon in Polygons)
            {
                polygon.Draw(e);
            }

            // Draw the new polygon.
            if (NewPolygon != null)
            {
                NewPolygon.DrawNew(e, NewPoint);
            }
        }
        
        // The grid spacing.
        private const int GridGap = 8;

        // Snap to the nearest grid point.
        private Point SnapToGrid(Point point)
        {
            int x = GridGap * (int)Math.Round((float)point.X / GridGap);
            int y = GridGap * (int)Math.Round((float)point.Y / GridGap);
            return new Point(x, y);
        }

        // Give the PictureBox a grid background.
        private void picCanvas_Resize(object sender, EventArgs e)
        {
            MakeBackgroundGrid();
        }
        private void MakeBackgroundGrid()
        {
            Bitmap bm = new Bitmap(
                picCanvas.ClientSize.Width,
                picCanvas.ClientSize.Height);
            for (int x = 0; x < picCanvas.ClientSize.Width; x += GridGap)
            {
                for (int y = 0; y < picCanvas.ClientSize.Height; y += GridGap)
                {
                    bm.SetPixel(x, y, Color.Black);
                }
            }

            picCanvas.BackgroundImage = bm;
        }
    }
}
