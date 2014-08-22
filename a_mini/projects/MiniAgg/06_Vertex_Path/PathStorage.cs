//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using MatterHackers.VectorMath;

namespace MatterHackers.Agg.VertexSource
{
    //---------------------------------------------------------------path_base
    // A container to store vertices with their flags. 
    // A path consists of a number of contours separated with "move_to" 
    // commands. The path storage can keep and maintain more than one
    // path. 
    // To navigate to the beginning of a particular path, use rewind(path_id);
    // Where path_id is what start_new_path() returns. So, when you call
    // start_new_path() you need to store its return value somewhere else
    // to navigate to the path afterwards.
    //
    // See also: vertex_source concept
    //------------------------------------------------------------------------
    public class PathStorage : IVertexSource, IVertexDest
    {

        #region InternalVertexStorage
        private class VertexStorage
        {
            int m_num_vertices;
            int m_allocated_vertices;
            //easy to transfer back to unmanaged part!
            double[] m_coord_xy;
            ShapePath.FlagsAndCommand[] m_CommandAndFlags;

            public void free_all()
            {
                //m_coord_x = null;
                //m_coord_y = null;
                m_coord_xy = null;  
                m_CommandAndFlags = null; 
                m_num_vertices = 0;
            }

            public int size()
            {
                return m_num_vertices;
            }

            public VertexStorage()
            {
            }

            public void remove_all()
            {
                m_num_vertices = 0;
            }

            public void AddVertex(double x, double y, ShapePath.FlagsAndCommand CommandAndFlags)
            {
                allocate_if_required(m_num_vertices);

                m_coord_xy[m_num_vertices << 1] = x;
                m_coord_xy[(m_num_vertices << 1) + 1] = y;
                m_CommandAndFlags[m_num_vertices] = CommandAndFlags;

                m_num_vertices++;
            }

            public void modify_vertex(int index, double x, double y)
            {
                m_coord_xy[index << 1] = x;
                m_coord_xy[(index << 1) + 1] = y;
            }

            public void modify_vertex(int index, double x, double y, ShapePath.FlagsAndCommand CommandAndFlags)
            {
                //m_coord_x[index] = x;
                //m_coord_y[index] = y;
                m_coord_xy[index << 1] = x;
                m_coord_xy[(index << 1) + 1] = y;

                m_CommandAndFlags[index] = CommandAndFlags;
            }

            public void modify_command(int index, ShapePath.FlagsAndCommand CommandAndFlags)
            {
                m_CommandAndFlags[index] = CommandAndFlags;
            }

            public void swap_vertices(int v1, int v2)
            {

                double x_tmp, y_tmp;
                x_tmp = m_coord_xy[v1 << 1];
                y_tmp = m_coord_xy[(v1 << 1) + 1];

                m_coord_xy[v1 << 1] = m_coord_xy[v2 << 1];//x
                m_coord_xy[(v1 << 1) + 1] = m_coord_xy[(v2 << 1) + 1];//y

                m_coord_xy[v2 << 1] = x_tmp;
                m_coord_xy[(v2 << 1) + 1] = y_tmp;


                ShapePath.FlagsAndCommand cmd = m_CommandAndFlags[v1];
                m_CommandAndFlags[v1] = m_CommandAndFlags[v2];
                m_CommandAndFlags[v2] = cmd;
            }

            public ShapePath.FlagsAndCommand last_command()
            {
                if (m_num_vertices != 0)
                {
                    return command(m_num_vertices - 1);
                }

                return ShapePath.FlagsAndCommand.CommandStop;
            }

            public ShapePath.FlagsAndCommand last_vertex(out double x, out double y)
            {
                if (m_num_vertices != 0)
                {
                    return vertex((int)(m_num_vertices - 1), out x, out y);
                }

                x = new double();
                y = new double();
                return ShapePath.FlagsAndCommand.CommandStop;
            }

            public ShapePath.FlagsAndCommand prev_vertex(out double x, out double y)
            {
                if (m_num_vertices > 1)
                {
                    return vertex((int)(m_num_vertices - 2), out x, out y);
                }
                x = new double();
                y = new double();
                return ShapePath.FlagsAndCommand.CommandStop;
            }
            public double last_x()
            {
                if (m_num_vertices > 0)
                {
                    int index = (int)(m_num_vertices - 1);
                    return m_coord_xy[index << 1];
                }
                return new double();
            }
            public double last_y()
            {
                if (m_num_vertices > 0)
                {
                    int index = (int)(m_num_vertices - 1);
                    return m_coord_xy[(index << 1) + 1];
                }
                return new double();
            }

            public int total_vertices()
            {
                return m_num_vertices;
            }

            public ShapePath.FlagsAndCommand vertex(int index, out double x, out double y)
            {
                var i = index << 1;
                x = m_coord_xy[i];
                y = m_coord_xy[i + 1];
                return m_CommandAndFlags[index];
            }

            public ShapePath.FlagsAndCommand command(int index)
            {
                return m_CommandAndFlags[index];
            }

            private void allocate_if_required(int indexToAdd)
            {
                if (indexToAdd < m_allocated_vertices)
                {
                    return;
                }
                
                while (indexToAdd >= m_allocated_vertices)
                {
                    int newSize = m_allocated_vertices + 256;

                    double[] new_xy = new double[newSize << 1];
                    ShapePath.FlagsAndCommand[] newCmd = new ShapePath.FlagsAndCommand[newSize];
                    if (m_coord_xy != null)
                    {
                        //copy old buffer to new buffer 
                        int actualLen = m_num_vertices << 1;
                        for (int i = actualLen - 1; i >= 0; )
                        {
                            new_xy[i] = m_coord_xy[i];
                            i--;
                            new_xy[i] = m_coord_xy[i];
                            i--;
                        }
                        for (int i = m_num_vertices - 1; i >= 0; --i)
                        {
                            newCmd[i] = m_CommandAndFlags[i];
                        }
                    }
                    m_coord_xy = new_xy;
                    m_CommandAndFlags = newCmd;

                    m_allocated_vertices = newSize;
                }
            }


            //----------------------------------------------------------
            internal static void UnsafeDirectSetData(
                VertexStorage vstore,                
                int m_allocated_vertices,
                int m_num_vertices,
                double[] m_coord_xy,
                ShapePath.FlagsAndCommand[] m_CommandAndFlags)
            {
                vstore.m_num_vertices = m_num_vertices;
                vstore.m_allocated_vertices = m_allocated_vertices;
                vstore.m_coord_xy = m_coord_xy;
                vstore.m_CommandAndFlags = m_CommandAndFlags;
            }
            internal static void UnsafeDirectGetData(
                VertexStorage vstore,
                out int m_allocated_vertices,
                out int m_num_vertices,               
                out double[] m_coord_xy,
                out ShapePath.FlagsAndCommand[] m_CommandAndFlags)
            {

                m_num_vertices = vstore.m_num_vertices;
                m_allocated_vertices = vstore.m_allocated_vertices;
                m_coord_xy = vstore.m_coord_xy;
                m_CommandAndFlags = vstore.m_CommandAndFlags;
            }

            //----------------------------------------------------------
        }
        #endregion

        private VertexStorage vertices;
        private int iteratorIndex;

        public PathStorage()
        {
            vertices = new VertexStorage();
        }

        public void add(Vector2 vertex)
        {
            throw new System.NotImplementedException();
        }

        public void Add(double x, double y, ShapePath.FlagsAndCommand flagsAndCommand)
        {
            vertices.AddVertex(x, y, flagsAndCommand);
        }

        public int size()
        {
            return vertices.size();
        }

        public Vector2 this[int i]
        {
            get
            {

                throw new NotImplementedException("make this work");
            }
        }

        public void remove_all() { vertices.remove_all(); iteratorIndex = 0; }
        public void free_all() { vertices.free_all(); iteratorIndex = 0; }

       

         
       
        // Make path functions
        //--------------------------------------------------------------------
        public int start_new_path()
        {
            if (!ShapePath.is_stop(vertices.last_command()))
            {
                vertices.AddVertex(0.0, 0.0, ShapePath.FlagsAndCommand.CommandStop);
            }
            return vertices.total_vertices();
        }


        public void rel_to_abs(ref double x, ref double y)
        {
            if (vertices.total_vertices() != 0)
            {
                double x2;
                double y2;
                if (ShapePath.is_vertex(vertices.last_vertex(out x2, out y2)))
                {
                    x += x2;
                    y += y2;
                }
            }
        }

        public void MoveTo(double x, double y)
        {
            vertices.AddVertex(x, y, ShapePath.FlagsAndCommand.CommandMoveTo);
        }

        public void LineTo(double x, double y)
        {
            vertices.AddVertex(x, y, ShapePath.FlagsAndCommand.CommandLineTo);
        }

        public void HorizontalLineTo(double x)
        {
            vertices.AddVertex(x, GetLastY(), ShapePath.FlagsAndCommand.CommandLineTo);
        }

        public void VerticalLineTo(double y)
        {
            vertices.AddVertex(GetLastX(), y, ShapePath.FlagsAndCommand.CommandLineTo);
        }

        /*
        public void arc_to(double rx, double ry,
                                   double angle,
                                   bool large_arc_flag,
                                   bool sweep_flag,
                                   double x, double y)
        {
            if(m_vertices.total_vertices() && is_vertex(m_vertices.last_command()))
            {
                double epsilon = 1e-30;
                double x0 = 0.0;
                double y0 = 0.0;
                m_vertices.last_vertex(&x0, &y0);

                rx = fabs(rx);
                ry = fabs(ry);

                // Ensure radii are valid
                //-------------------------
                if(rx < epsilon || ry < epsilon) 
                {
                    line_to(x, y);
                    return;
                }

                if(calc_distance(x0, y0, x, y) < epsilon)
                {
                    // If the endpoints (x, y) and (x0, y0) are identical, then this
                    // is equivalent to omitting the elliptical arc segment entirely.
                    return;
                }
                bezier_arc_svg a(x0, y0, rx, ry, angle, large_arc_flag, sweep_flag, x, y);
                if(a.radii_ok())
                {
                    join_path(a);
                }
                else
                {
                    line_to(x, y);
                }
            }
            else
            {
                move_to(x, y);
            }
        }

        public void arc_rel(double rx, double ry,
                                    double angle,
                                    bool large_arc_flag,
                                    bool sweep_flag,
                                    double dx, double dy)
        {
            rel_to_abs(&dx, &dy);
            arc_to(rx, ry, angle, large_arc_flag, sweep_flag, dx, dy);
        }
         */

        /// <summary>
        /// Draws a quadratic B�zier curve from the current point to (x,y) using (xControl,yControl) as the control point.
        /// </summary>
        /// <param name="xControl"></param>
        /// <param name="yControl"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void curve3(double xControl, double yControl,
                                   double x, double y)
        {
            vertices.AddVertex(xControl, yControl, ShapePath.FlagsAndCommand.CommandCurve3);
            vertices.AddVertex(x, y, ShapePath.FlagsAndCommand.CommandCurve3);
        }

        /// <summary>
        /// Draws a quadratic B�zier curve from the current point to (x,y) using (xControl,yControl) as the control point.
        /// </summary>
        /// <param name="xControl"></param>
        /// <param name="yControl"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void curve3_rel(double dx_ctrl, double dy_ctrl, double dx_to, double dy_to)
        {
            rel_to_abs(ref dx_ctrl, ref dy_ctrl);
            rel_to_abs(ref dx_to, ref dy_to);
            vertices.AddVertex(dx_ctrl, dy_ctrl, ShapePath.FlagsAndCommand.CommandCurve3);
            vertices.AddVertex(dx_to, dy_to, ShapePath.FlagsAndCommand.CommandCurve3);
        }

        /// <summary>
        /// <para>Draws a quadratic B�zier curve from the current point to (x,y).</para>
        /// <para>The control point is assumed to be the reflection of the control point on the previous command relative to the current point.</para>
        /// <para>(If there is no previous command or if the previous command was not a curve, assume the control point is coincident with the current point.)</para>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void curve3(double x, double y)
        {
            double x0;
            double y0;
            if (ShapePath.is_vertex(vertices.last_vertex(out x0, out y0)))
            {
                double x_ctrl;
                double y_ctrl;
                ShapePath.FlagsAndCommand cmd = vertices.prev_vertex(out x_ctrl, out y_ctrl);
                if (ShapePath.is_curve(cmd))
                {
                    x_ctrl = x0 + x0 - x_ctrl;
                    y_ctrl = y0 + y0 - y_ctrl;
                }
                else
                {
                    x_ctrl = x0;
                    y_ctrl = y0;
                }
                curve3(x_ctrl, y_ctrl, x, y);
            }
        }

        /// <summary>
        /// <para>Draws a quadratic B�zier curve from the current point to (x,y).</para>
        /// <para>The control point is assumed to be the reflection of the control point on the previous command relative to the current point.</para>
        /// <para>(If there is no previous command or if the previous command was not a curve, assume the control point is coincident with the current point.)</para>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void curve3_rel(double dx_to, double dy_to)
        {
            rel_to_abs(ref dx_to, ref dy_to);
            curve3(dx_to, dy_to);
        }

        public void curve4(double x_ctrl1, double y_ctrl1,
                                   double x_ctrl2, double y_ctrl2,
                                   double x_to, double y_to)
        {
            vertices.AddVertex(x_ctrl1, y_ctrl1, ShapePath.FlagsAndCommand.CommandCurve4);
            vertices.AddVertex(x_ctrl2, y_ctrl2, ShapePath.FlagsAndCommand.CommandCurve4);
            vertices.AddVertex(x_to, y_to, ShapePath.FlagsAndCommand.CommandCurve4);
        }

        public void curve4_rel(double dx_ctrl1, double dy_ctrl1,
                                       double dx_ctrl2, double dy_ctrl2,
                                       double dx_to, double dy_to)
        {
            rel_to_abs(ref dx_ctrl1, ref dy_ctrl1);
            rel_to_abs(ref dx_ctrl2, ref dy_ctrl2);
            rel_to_abs(ref dx_to, ref dy_to);
            vertices.AddVertex(dx_ctrl1, dy_ctrl1, ShapePath.FlagsAndCommand.CommandCurve4);
            vertices.AddVertex(dx_ctrl2, dy_ctrl2, ShapePath.FlagsAndCommand.CommandCurve4);
            vertices.AddVertex(dx_to, dy_to, ShapePath.FlagsAndCommand.CommandCurve4);
        }

        public void curve4(double x_ctrl2, double y_ctrl2,
                                   double x_to, double y_to)
        {
            double x0;
            double y0;
            if (ShapePath.is_vertex(last_vertex(out x0, out y0)))
            {
                double x_ctrl1;
                double y_ctrl1;
                ShapePath.FlagsAndCommand cmd = prev_vertex(out x_ctrl1, out y_ctrl1);
                if (ShapePath.is_curve(cmd))
                {
                    x_ctrl1 = x0 + x0 - x_ctrl1;
                    y_ctrl1 = y0 + y0 - y_ctrl1;
                }
                else
                {
                    x_ctrl1 = x0;
                    y_ctrl1 = y0;
                }
                curve4(x_ctrl1, y_ctrl1, x_ctrl2, y_ctrl2, x_to, y_to);
            }
        }

        public void curve4_rel(double dx_ctrl2, double dy_ctrl2,
                                       double dx_to, double dy_to)
        {
            rel_to_abs(ref dx_ctrl2, ref dy_ctrl2);
            rel_to_abs(ref dx_to, ref dy_to);
            curve4(dx_ctrl2, dy_ctrl2, dx_to, dy_to);
        }

        public int total_vertices()
        {
            return vertices.total_vertices();
        }

        public ShapePath.FlagsAndCommand last_vertex(out double x, out double y)
        {
            return vertices.last_vertex(out x, out y);
        }

        public ShapePath.FlagsAndCommand prev_vertex(out double x, out double y)
        {
            return vertices.prev_vertex(out x, out y);
        }

        public double GetLastX()
        {
            return vertices.last_x();
        }

        public double GetLastY()
        {
            return vertices.last_y();
        }

        public ShapePath.FlagsAndCommand vertex(int index, out double x, out double y)
        {
            return vertices.vertex(index, out x, out y);
        }

        public ShapePath.FlagsAndCommand command(int index)
        {
            return vertices.command(index);
        }

        public void modify_vertex(int index, double x, double y)
        {
            vertices.modify_vertex(index, x, y);
        }

        public void modify_vertex(int index, double x, double y, ShapePath.FlagsAndCommand PathAndFlags)
        {
            vertices.modify_vertex(index, x, y, PathAndFlags);
        }

        public void modify_command(int index, ShapePath.FlagsAndCommand PathAndFlags)
        {
            vertices.modify_command(index, PathAndFlags);
        }

        public IEnumerable<VertexData> Vertices()
        {
            int count = vertices.total_vertices();
            for (int i = 0; i < count; i++)
            {
                double x = 0;
                double y = 0;
                ShapePath.FlagsAndCommand command = vertices.vertex(i, out x, out y);
                yield return new VertexData(command, new Vector2(x, y));
            }

            yield return new VertexData(ShapePath.FlagsAndCommand.CommandStop, new Vector2(0, 0));
        }

        public virtual void rewind(int pathId)
        {
            iteratorIndex = pathId;
        }

        public ShapePath.FlagsAndCommand vertex(out double x, out double y)
        {
            if (iteratorIndex >= vertices.total_vertices())
            {
                x = 0;
                y = 0;
                return ShapePath.FlagsAndCommand.CommandStop;
            }

            return vertices.vertex(iteratorIndex++, out x, out y);
        }

        // Arrange the orientation of a polygon, all polygons in a path, 
        // or in all paths. After calling arrange_orientations() or 
        // arrange_orientations_all_paths(), all the polygons will have 
        // the same orientation, i.e. path_flags_cw or path_flags_ccw
        //--------------------------------------------------------------------
        public int arrange_polygon_orientation(int start, ShapePath.FlagsAndCommand orientation)
        {
            if (orientation == ShapePath.FlagsAndCommand.FlagNone) return start;

            // Skip all non-vertices at the beginning
            while (start < vertices.total_vertices() &&
                  !ShapePath.is_vertex(vertices.command(start))) ++start;

            // Skip all insignificant move_to
            while (start + 1 < vertices.total_vertices() &&
                  ShapePath.is_move_to(vertices.command(start)) &&
                  ShapePath.is_move_to(vertices.command(start + 1))) ++start;

            // Find the last vertex
            int end = start + 1;
            while (end < vertices.total_vertices() &&
                  !ShapePath.is_next_poly(vertices.command(end))) ++end;

            if (end - start > 2)
            {
                if (perceive_polygon_orientation(start, end) != orientation)
                {
                    // Invert polygon, set orientation flag, and skip all end_poly
                    invert_polygon(start, end);
                    ShapePath.FlagsAndCommand PathAndFlags;
                    while (end < vertices.total_vertices() &&
                          ShapePath.is_end_poly(PathAndFlags = vertices.command(end)))
                    {
                        vertices.modify_command(end++, PathAndFlags | orientation);// Path.set_orientation(cmd, orientation));
                    }
                }
            }
            return end;
        }

        public int arrange_orientations(int start, ShapePath.FlagsAndCommand orientation)
        {
            if (orientation != ShapePath.FlagsAndCommand.FlagNone)
            {
                while (start < vertices.total_vertices())
                {
                    start = arrange_polygon_orientation(start, orientation);
                    if (ShapePath.is_stop(vertices.command(start)))
                    {
                        ++start;
                        break;
                    }
                }
            }
            return start;
        }

        public void arrange_orientations_all_paths(ShapePath.FlagsAndCommand orientation)
        {
            if (orientation != ShapePath.FlagsAndCommand.FlagNone)
            {
                int start = 0;
                while (start < vertices.total_vertices())
                {
                    start = arrange_orientations(start, orientation);
                }
            }
        }

        // Flip all vertices horizontally or vertically, 
        // between x1 and x2, or between y1 and y2 respectively
        //--------------------------------------------------------------------
        public void flip_x(double x1, double x2)
        {
            int i;
            double x, y;
            for (i = 0; i < vertices.total_vertices(); i++)
            {
                ShapePath.FlagsAndCommand PathAndFlags = vertices.vertex(i, out x, out y);
                if (ShapePath.is_vertex(PathAndFlags))
                {
                    vertices.modify_vertex(i, x2 - x + x1, y);
                }
            }
        }

        public void flip_y(double y1, double y2)
        {
            int i;
            double x, y;
            for (i = 0; i < vertices.total_vertices(); i++)
            {
                ShapePath.FlagsAndCommand PathAndFlags = vertices.vertex(i, out x, out y);
                if (ShapePath.is_vertex(PathAndFlags))
                {
                    vertices.modify_vertex(i, x, y2 - y + y1);
                }
            }
        }

        public void end_poly()
        {
            close_polygon(ShapePath.FlagsAndCommand.FlagClose);
        }

        public void end_poly(ShapePath.FlagsAndCommand flags)
        {
            if (ShapePath.is_vertex(vertices.last_command()))
            {
                vertices.AddVertex(0.0, 0.0, ShapePath.FlagsAndCommand.CommandEndPoly | flags);
            }
        }


        public void ClosePolygon()
        {
            close_polygon(ShapePath.FlagsAndCommand.FlagNone);
        }

        public void close_polygon(ShapePath.FlagsAndCommand flags)
        {
            end_poly(ShapePath.FlagsAndCommand.FlagClose | flags);
        }

        // Concatenate path. The path is added as is.
        public void concat_path(IVertexSource vs)
        {
            concat_path(vs, 0);
        }

        public void concat_path(IVertexSource vs, int path_id)
        {
            double x, y;
            ShapePath.FlagsAndCommand PathAndFlags;
            vs.rewind(path_id);
            while (!ShapePath.is_stop(PathAndFlags = vs.vertex(out x, out y)))
            {
                vertices.AddVertex(x, y, PathAndFlags);
            }
        }

        //--------------------------------------------------------------------
        // Join path. The path is joined with the existing one, that is, 
        // it behaves as if the pen of a plotter was always down (drawing)
        //template<class VertexSource> 
        public void join_path(PathStorage vs)
        {
            join_path(vs, 0);

        }

        public void join_path(PathStorage vs, int path_id)
        {
            double x, y;
            vs.rewind(path_id);
            ShapePath.FlagsAndCommand PathAndFlags = vs.vertex(out x, out y);
            if (!ShapePath.is_stop(PathAndFlags))
            {
                if (ShapePath.is_vertex(PathAndFlags))
                {
                    double x0, y0;
                    ShapePath.FlagsAndCommand PathAndFlags0 = last_vertex(out x0, out y0);
                    if (ShapePath.is_vertex(PathAndFlags0))
                    {
                        if (agg_math.calc_distance(x, y, x0, y0) > agg_math.vertex_dist_epsilon)
                        {
                            if (ShapePath.is_move_to(PathAndFlags)) PathAndFlags = ShapePath.FlagsAndCommand.CommandLineTo;
                            vertices.AddVertex(x, y, PathAndFlags);
                        }
                    }
                    else
                    {
                        if (ShapePath.is_stop(PathAndFlags0))
                        {
                            PathAndFlags = ShapePath.FlagsAndCommand.CommandMoveTo;
                        }
                        else
                        {
                            if (ShapePath.is_move_to(PathAndFlags)) PathAndFlags = ShapePath.FlagsAndCommand.CommandLineTo;
                        }
                        vertices.AddVertex(x, y, PathAndFlags);
                    }
                }
                while (!ShapePath.is_stop(PathAndFlags = vs.vertex(out x, out y)))
                {
                    vertices.AddVertex(x, y, ShapePath.is_move_to(PathAndFlags) ?
                                                    ShapePath.FlagsAndCommand.CommandLineTo :
                                                    PathAndFlags);
                }
            }
        }

        /*
        // Concatenate polygon/polyline. 
        //--------------------------------------------------------------------
        void concat_poly(T* data, int num_points, bool closed)
        {
            poly_plain_adaptor<T> poly(data, num_points, closed);
            concat_path(poly);
        }

        // Join polygon/polyline continuously.
        //--------------------------------------------------------------------
        void join_poly(T* data, int num_points, bool closed)
        {
            poly_plain_adaptor<T> poly(data, num_points, closed);
            join_path(poly);
        }
         */

        //--------------------------------------------------------------------
        public void translate(double dx, double dy)
        {
            translate(dx, dy, 0);
        }

        public void translate(double dx, double dy, int path_id)
        {
            int num_ver = vertices.total_vertices();
            for (; path_id < num_ver; path_id++)
            {
                double x, y;
                ShapePath.FlagsAndCommand PathAndFlags = vertices.vertex(path_id, out x, out y);
                if (ShapePath.is_stop(PathAndFlags)) break;
                if (ShapePath.is_vertex(PathAndFlags))
                {
                    x += dx;
                    y += dy;
                    vertices.modify_vertex(path_id, x, y);
                }
            }
        }

        public void translate_all_paths(double dx, double dy)
        {
            int index;
            int num_ver = vertices.total_vertices();
            for (index = 0; index < num_ver; index++)
            {
                double x, y;
                if (ShapePath.is_vertex(vertices.vertex(index, out x, out y)))
                {
                    x += dx;
                    y += dy;
                    vertices.modify_vertex(index, x, y);
                }
            }
        }

        //--------------------------------------------------------------------
        public void transform(Transform.Affine trans)
        {
            transform(trans, 0);
        }

        public void transform(Transform.Affine trans, int path_id)
        {
            int num_ver = vertices.total_vertices();
            for (; path_id < num_ver; path_id++)
            {
                double x, y;
                ShapePath.FlagsAndCommand PathAndFlags = vertices.vertex(path_id, out x, out y);
                if (ShapePath.is_stop(PathAndFlags)) break;
                if (ShapePath.is_vertex(PathAndFlags))
                {
                    trans.transform(ref x, ref y);
                    vertices.modify_vertex(path_id, x, y);
                }
            }
        }

        //--------------------------------------------------------------------
        public void transform_all_paths(Transform.Affine trans)
        {
            int index;
            int num_ver = vertices.total_vertices();
            for (index = 0; index < num_ver; index++)
            {
                double x, y;
                if (ShapePath.is_vertex(vertices.vertex(index, out x, out y)))
                {
                    trans.transform(ref x, ref y);
                    vertices.modify_vertex(index, x, y);
                }
            }
        }

        public void invert_polygon(int start)
        {
            // Skip all non-vertices at the beginning
            while (start < vertices.total_vertices() &&
                  !ShapePath.is_vertex(vertices.command(start))) ++start;

            // Skip all insignificant move_to
            while (start + 1 < vertices.total_vertices() &&
                  ShapePath.is_move_to(vertices.command(start)) &&
                  ShapePath.is_move_to(vertices.command(start + 1))) ++start;

            // Find the last vertex
            int end = start + 1;
            while (end < vertices.total_vertices() &&
                  !ShapePath.is_next_poly(vertices.command(end))) ++end;

            invert_polygon(start, end);
        }

        private ShapePath.FlagsAndCommand perceive_polygon_orientation(int start, int end)
        {
            // Calculate signed area (double area to be exact)
            //---------------------
            int np = end - start;
            double area = 0.0;
            int i;
            for (i = 0; i < np; i++)
            {
                double x1, y1, x2, y2;
                vertices.vertex(start + i, out x1, out y1);
                vertices.vertex(start + (i + 1) % np, out x2, out y2);
                area += x1 * y2 - y1 * x2;
            }
            return (area < 0.0) ? ShapePath.FlagsAndCommand.FlagCW : ShapePath.FlagsAndCommand.FlagCCW;
        }

        private void invert_polygon(int start, int end)
        {
            int i;
            ShapePath.FlagsAndCommand tmp_PathAndFlags = vertices.command(start);

            --end; // Make "end" inclusive

            // Shift all commands to one position
            for (i = start; i < end; i++)
            {
                vertices.modify_command(i, vertices.command(i + 1));
            }

            // Assign starting command to the ending command
            vertices.modify_command(end, tmp_PathAndFlags);

            // Reverse the polygon
            while (end > start)
            {
                vertices.swap_vertices(start++, end--);
            }
        }

        public void ShareVertexData(PathStorage pathStorageToShareFrom)
        {
            vertices = pathStorageToShareFrom.vertices;
        }


        //----------------------------------------------------------

        public static void UnsafeDirectSetData(
            PathStorage pathStore,
            int m_allocated_vertices,
            int m_num_vertices,            
            double[] m_coord_xy,
            ShapePath.FlagsAndCommand[] m_CommandAndFlags)
        {

            VertexStorage.UnsafeDirectSetData(
                pathStore.vertices,
                m_allocated_vertices,
                m_num_vertices,                
                m_coord_xy,
                m_CommandAndFlags); 
        }
        public static void UnsafeDirectGetData(
            PathStorage pathStore,
            out int m_allocated_vertices,
            out int m_num_vertices,            
            out double[] m_coord_xy,
            out ShapePath.FlagsAndCommand[] m_CommandAndFlags)
        {
            VertexStorage.UnsafeDirectGetData(
                pathStore.vertices,
                out m_allocated_vertices,
                out m_num_vertices,                
                out m_coord_xy,
                out m_CommandAndFlags);             
        }

    }
}