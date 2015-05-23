//BSD 2015, WinterDev 

using System;
using System.Collections.Generic;
using PixelFarm.Agg.Transform;
using PixelFarm.Agg.Image;
using PixelFarm.Agg.VertexSource;
using PixelFarm.VectorMath;

using Mini;
namespace PixelFarm.Agg.Samples
{
    [Info(OrderCode = "21")]
    [Info("hand paint with line simplification")]
    public class HandPaintWithLineSimplifyExample : DemoBase
    {


        Point latestMousePoint;
        List<List<Point>> pointSets = new List<List<Point>>();
        List<List<Point>> simplifiedPointSets = new List<List<Point>>();

        CanvasPainter p;
        List<Point> currentPointSet;// = new List<Point>();//current point list

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

            var plistCount = pointSets.Count;

            p.StrokeColor = ColorRGBA.Black;
            for (int n = 0; n < plistCount; ++n)
            {
                var contPoints = pointSets[n];
                DrawLineSet(p, contPoints);
            }

            plistCount = simplifiedPointSets.Count;
            p.StrokeColor = ColorRGBA.Red;

            for (int n = 0; n < plistCount; ++n)
            {
                var contPoints = simplifiedPointSets[n];
                DrawLineSet(p, contPoints);
            }



        }
        static void DrawLineSet(CanvasPainter p, List<Point> contPoints)
        {

            int pcount = contPoints.Count;
            for (int i = 1; i < pcount; ++i)
            {
                var p0 = contPoints[i - 1];
                var p1 = contPoints[i];
                p.Line(p0.x, p0.y, p1.x, p1.y);
            }
        }
        public override void MouseDrag(int x, int y)
        {
            //add data to draw             
            currentPointSet.Add(new Point(x, y));
        }
        public override void MouseDown(int x, int y, bool isRightButton)
        {
            currentPointSet = new List<Point>();
            this.pointSets.Add(currentPointSet);

            latestMousePoint = new Point(x, y);
            base.MouseDown(x, y, isRightButton);
        }
        public override void MouseUp(int x, int y)
        {
            //finish the current set
            //create a simplified point set
            var newSimplfiedSet = LineSimplifiedUtility.DouglasPeuckerReduction(currentPointSet, 15);
            this.simplifiedPointSets.Add(newSimplfiedSet);

            base.MouseUp(x, y);
        }


    }


}

