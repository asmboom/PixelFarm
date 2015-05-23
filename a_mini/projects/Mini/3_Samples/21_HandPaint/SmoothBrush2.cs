﻿//BSD 2014,2015 WinterDev 
//adapt from Paper.js

using System;
using System.Collections.Generic;
using PixelFarm.Agg.Transform;
using PixelFarm.Agg.Image;
using PixelFarm.Agg.VertexSource;
using PixelFarm.VectorMath;

using Mini;
using burningmime.curves; //for curve fit
using ClipperLib;

namespace PixelFarm.Agg.Samples
{

    public enum SmoothBrushMode
    {
        SolidBrush,
        EraseBrush,
        CutBrush
    }

    [Info(OrderCode = "22")]
    [Info("SmoothBrush2")]
    public class SmoothBrush2 : DemoBase
    {

        Point latestMousePoint;
        List<MyBrushPath> myBrushPathList = new List<MyBrushPath>();
        CanvasPainter p;
        MyBrushPath currentBrushPath;

        public override void Init()
        {

        }
        [DemoConfig]
        public SmoothBrushMode BrushMode
        {
            get;
            set;
        }

        public override void Draw(Graphics2D g)
        {
            if (p == null)
            {
                p = new CanvasPainter(g);
                p.StrokeColor = ColorRGBA.Black;
                p.StrokeWidth = 1;
            }

            p.Clear(ColorRGBA.White);
            p.FillColor = ColorRGBA.Black;

            int j = myBrushPathList.Count;
            for (int n = 0; n < j; ++n)
            {
                var brushPath = myBrushPathList[n];

                if (brushPath.vxs != null)
                {
                    switch (brushPath.BrushMode)
                    {
                        case SmoothBrushMode.CutBrush:
                            {
                            } break;
                        default:
                            {
                                p.FillColor = brushPath.FillColor;
                                p.Fill(brushPath.vxs);

                                if (brushPath.StrokeColor.alpha > 0)
                                {
                                    p.StrokeColor = ColorRGBA.Red;
                                    p.Draw(brushPath.vxs);
                                }
                            } break;
                    }
                }
                else
                {
                    var contPoints = brushPath.contPoints;
                    int pcount = contPoints.Count;
                    for (int i = 1; i < pcount; ++i)
                    {
                        var p0 = contPoints[i - 1];
                        var p1 = contPoints[i];
                        p.Line(p0.x, p0.y, p1.x, p1.y);
                    }
                }
            }

        }
        public override void MouseUp(int x, int y)
        {
            if (currentBrushPath != null)
            {
                currentBrushPath.GetSmooth();
                switch (currentBrushPath.BrushMode)
                {
                    case SmoothBrushMode.CutBrush:
                        {
                            if (myBrushPathList.Count > 0)
                            {
                                //1. remove 
                                myBrushPathList.RemoveAt(myBrushPathList.Count - 1);
                                //

                                if (myBrushPathList.Count > 0)
                                {
                                    var lastPath = myBrushPathList[myBrushPathList.Count - 1];

                                    //do path clip***

                                    PathWriter result = CombinePaths(
                                          new VertexStoreSnap(lastPath.vxs), 
                                          new VertexStoreSnap(currentBrushPath.vxs),
                                          ClipType.ctDifference);

                                    myBrushPathList.RemoveAt(myBrushPathList.Count - 1);

                                    MyBrushPath newBrushPath = new MyBrushPath();
                                    newBrushPath.BrushMode = lastPath.BrushMode;
                                    newBrushPath.StrokeColor = lastPath.StrokeColor;
                                    newBrushPath.FillColor = lastPath.FillColor;
                                    newBrushPath.vxs = result.Vxs;
                                    myBrushPathList.Add(newBrushPath);

                                }
                            }

                        } break;
                }
                currentBrushPath = null;
            }

            base.MouseUp(x, y);
        }
        public override void MouseDrag(int x, int y)
        {

            //find diff 
            Vector newPoint = new Vector(x, y);
            //find distance
            Vector oldPoint = new Vector(latestMousePoint.x, latestMousePoint.y);
            var delta = (newPoint - oldPoint) / 2; // 2,4 etc 
            //midpoint
            var midPoint = (newPoint + oldPoint) / 2;
            delta = delta.NewLength(5);
            delta.Rotate(90);

            var newTopPoint = midPoint + delta;
            var newBottomPoint = midPoint - delta;


            //bottom point
            currentBrushPath.AddPointFirst((int)newBottomPoint.X, (int)newBottomPoint.Y);
            currentBrushPath.AddPointLast((int)newTopPoint.X, (int)newTopPoint.Y);

            latestMousePoint = new Point(x, y);

        }
        public override void MouseDown(int x, int y, bool isRightButton)
        {
            latestMousePoint = new Point(x, y);
            currentBrushPath = new MyBrushPath();
            switch (BrushMode)
            {
                case SmoothBrushMode.SolidBrush:
                    currentBrushPath.FillColor = ColorRGBA.Black;
                    currentBrushPath.StrokeColor = ColorRGBA.Red;
                    break;

                case SmoothBrushMode.EraseBrush:
                    currentBrushPath.FillColor = ColorRGBA.White;
                    currentBrushPath.StrokeColor = ColorRGBA.Transparent;
                    break;
                case SmoothBrushMode.CutBrush:

                    break;
            }
            currentBrushPath.BrushMode = this.BrushMode;
            this.myBrushPathList.Add(currentBrushPath);
            currentBrushPath.AddPointFirst(x, y);
            base.MouseDown(x, y, isRightButton);
        }


        static PathWriter CombinePaths(VertexStoreSnap a, VertexStoreSnap b, ClipType clipType)
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

    }

    //--------------------------------------------------



}

