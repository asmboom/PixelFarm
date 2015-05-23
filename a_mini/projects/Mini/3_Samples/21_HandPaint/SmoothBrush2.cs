//BSD 2014,2015 WinterDev 
//adapt from Paper.js

using System;
using System.Collections.Generic;
using PixelFarm.Agg.Transform;
using PixelFarm.Agg.Image;
using PixelFarm.Agg.VertexSource;
using PixelFarm.VectorMath;

using Mini;
using burningmime.curves; //for curve fit

namespace PixelFarm.Agg.Samples
{
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
            foreach (var brushPath in this.myBrushPathList)
            {

                if (brushPath.vxs != null)
                {
                    p.FillColor = ColorRGBA.Black;
                    p.Fill(brushPath.vxs);

                    p.StrokeColor = ColorRGBA.Red;
                    p.Draw(brushPath.vxs);

                }
                else if (brushPath.cubicBzs != null)
                {
                    int ccount = brushPath.cubicBzs.Length;
                    for (int i = 0; i < ccount; ++i)
                    {
                        var cc = brushPath.cubicBzs[i];
                        //FillPoint(cc.p0, p);
                        //FillPoint(cc.p1, p);
                        //FillPoint(cc.p2, p);
                        //FillPoint(cc.p3, p); 
                        p.DrawBezierCurve(
                           (float)cc.p0.x, (float)cc.p0.y,
                           (float)cc.p3.x, (float)cc.p3.y,
                           (float)cc.p1.x, (float)cc.p1.y,
                           (float)cc.p2.x, (float)cc.p2.y);
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
                //currentBrushPath.Close();
                currentBrushPath.GetSmooth();
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
            this.myBrushPathList.Add(currentBrushPath);
            currentBrushPath.AddPointFirst(x, y);
            base.MouseDown(x, y, isRightButton);
        }
    }

    //--------------------------------------------------



}

