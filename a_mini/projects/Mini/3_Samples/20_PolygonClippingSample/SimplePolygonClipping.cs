//2014,2015 BSD, WinterDev
//MatterHackers

using System;
using System.Collections.Generic;

using PixelFarm.Agg.Image;
using PixelFarm.Agg.VertexSource;

using PixelFarm.VectorMath;
using PixelFarm.Agg.Transform;

using Mini;
using ClipperLib;

namespace PixelFarm.Agg.Sample_PolygonClipping
{


    [Info(OrderCode = "20")]
    public class SimplePolygonClipping : DemoBase
    {
        PathWriter CombinePaths(VertexStoreSnap a, VertexStoreSnap b, ClipType clipType)
        {
            List<List<IntPoint>> aPolys = CreatePolygons(a);
            List<List<IntPoint>> bPolys = CreatePolygons(b);

            Clipper clipper = new Clipper();

            clipper.AddPaths(aPolys, PolyType.ptSubject, true);
            clipper.AddPaths(bPolys, PolyType.ptClip, true);

            List<List<IntPoint>> intersectedPolys = new List<List<IntPoint>>();
            clipper.Execute(clipType, intersectedPolys);

            PathWriter output = new PathWriter();

            foreach (List<IntPoint> polygon in intersectedPolys)
            {
                bool first = true;
                int j = polygon.Count;

                if (j > 0)
                {
                    //first one
                    IntPoint point = polygon[0];

                    output.MoveTo(point.X / 1000.0, point.Y / 1000.0);

                    //next ...
                    if (j > 1)
                    {
                        for (int i = 1; i < j; ++i)
                        {
                            point = polygon[i];
                            output.LineTo(point.X / 1000.0, point.Y / 1000.0);
                        }
                    }
                }
                //foreach (IntPoint point in polygon)
                //{
                //    if (first)
                //    {
                //        output.AddVertex(point.X / 1000.0, point.Y / 1000.0, ShapePath.FlagsAndCommand.CommandMoveTo);
                //        first = false;
                //    }
                //    else
                //    {
                //        output.AddVertex(point.X / 1000.0, point.Y / 1000.0, ShapePath.FlagsAndCommand.CommandLineTo);
                //    }
                //}

                output.CloseFigure();
            }


            output.Stop();
            return output;
        }

        static List<List<IntPoint>> CreatePolygons(VertexStoreSnap a)
        {
     
            List<List<IntPoint>> allPolys = new List<List<IntPoint>>();
            List<IntPoint> currentPoly = null;
            VertexData last = new VertexData();
            VertexData first = new VertexData();
            bool addedFirst = false;

            var snapIter = a.GetVertexSnapIter();
            VertexCmd cmd;
            double x, y;
            cmd = snapIter.GetNextVertex(out x, out y);
            do
            {
                if (cmd == VertexCmd.LineTo)
                {
                    if (!addedFirst)
                    {
                        currentPoly.Add(new IntPoint((long)(last.x * 1000), (long)(last.y * 1000)));
                        addedFirst = true;
                        first = last;
                    }
                    currentPoly.Add(new IntPoint((long)(x * 1000), (long)(y * 1000)));
                    last = new VertexData(cmd, x, y);
                }
                else
                {
                    addedFirst = false;
                    currentPoly = new List<IntPoint>();
                    allPolys.Add(currentPoly);
                    if (cmd == VertexCmd.MoveTo)
                    {
                        last = new VertexData(cmd, x, y);
                    }
                    else
                    {
                        last = first;
                    }
                }
                cmd = snapIter.GetNextVertex(out x, out y);

            } while (cmd != VertexCmd.Stop);

            return allPolys;
        }

        double m_x;
        double m_y;
        ColorRGBA BackgroundColor;


        public SimplePolygonClipping()
        {
            BackgroundColor = ColorRGBA.White;
            this.Width = 800;
            this.Height = 600;
        }
        [DemoConfig]
        public OperationOption OpOption
        {
            get;
            set;
        }

        public override void Draw(Graphics2D g)
        {
            if (BackgroundColor.Alpha0To255 > 0)
            {
                g.FillRectangle(new RectD(0, 0, this.Width, Height), BackgroundColor);
            }
            PathWriter ps1 = new PathWriter();
            PathWriter ps2 = new PathWriter();

            double x = m_x - Width / 2 + 100;
            double y = m_y - Height / 2 + 100;
            ps1.MoveTo(x + 140, y + 145);
            ps1.LineTo(x + 225, y + 44);
            ps1.LineTo(x + 296, y + 219);
            ps1.CloseFigure();

            ps1.LineTo(x + 226, y + 289);
            ps1.LineTo(x + 82, y + 292);

            ps1.MoveTo(x + 220, y + 222);
            ps1.LineTo(x + 363, y + 249);
            ps1.LineTo(x + 265, y + 331);

            ps1.MoveTo(x + 242, y + 243);
            ps1.LineTo(x + 268, y + 309);
            ps1.LineTo(x + 325, y + 261);

            ps1.MoveTo(x + 259, y + 259);
            ps1.LineTo(x + 273, y + 288);
            ps1.LineTo(x + 298, y + 266);

            ps2.MoveTo(100 + 32, 100 + 77);
            ps2.LineTo(100 + 473, 100 + 263);
            ps2.LineTo(100 + 351, 100 + 290);
            ps2.LineTo(100 + 354, 100 + 374);

            g.Render(ps1.MakeVertexSnap(), ColorRGBAf.MakeColorRGBA(0f, 0f, 0f, 0.1f));
            g.Render(ps2.MakeVertexSnap(), ColorRGBAf.MakeColorRGBA(0f, 0.6f, 0f, 0.1f));

            CreateAndRenderCombined(g, ps1.MakeVertexSnap(), ps2.MakeVertexSnap());
        }
        void CreateAndRenderCombined(Graphics2D graphics2D, VertexStoreSnap ps1, VertexStoreSnap ps2)
        {
            PathWriter combined = null;

            switch (this.OpOption)
            {
                case OperationOption.OR:
                    combined = CombinePaths(ps1, ps2, ClipType.ctUnion);
                    break;
                case OperationOption.AND:
                    combined = CombinePaths(ps1, ps2, ClipType.ctIntersection);
                    break;
                case OperationOption.XOR:
                    combined = CombinePaths(ps1, ps2, ClipType.ctXor);
                    break;
                case OperationOption.A_B:
                    combined = CombinePaths(ps1, ps2, ClipType.ctDifference);
                    break;
                case OperationOption.B_A:
                    combined = CombinePaths(ps2, ps1, ClipType.ctDifference);
                    break;
            }

            if (combined != null)
            {
                graphics2D.Render(combined.MakeVertexSnap(), ColorRGBAf.MakeColorRGBA(0.5f, 0.0f, 0f, 0.5f));
            }
        }
        public override void MouseDrag(int x, int y)
        {
            m_x = x;
            m_y = y;
        }
        public override void MouseDown(int x, int y, bool isRightoy)
        {
            m_x = x;
            m_y = y;
        }
        public override void MouseUp(int x, int y)
        {
            m_x = x;
            m_y = y;
        }
    }

}