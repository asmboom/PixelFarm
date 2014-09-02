﻿//----------------------------------------------------------------------------
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
using MatterHackers.Agg.Image;
using MatterHackers.Agg.Transform;
using MatterHackers.Agg.VertexSource;
using MatterHackers.VectorMath;

namespace MatterHackers.Agg.UI
{
    abstract public class SimpleVertexSourceWidget : GuiWidget, IVertexSource
    {

        bool localBoundsComeFromPoints = true;

        public SimpleVertexSourceWidget()
        {
            throw new Exception("this is depricated");
        }

        public SimpleVertexSourceWidget(Vector2 originRelativeParent, bool localBoundsComeFromPoints = true)
        {
            this.localBoundsComeFromPoints = localBoundsComeFromPoints;
            OriginRelativeParent = originRelativeParent;
        }

        public override RectangleDouble LocalBounds
        {
            get
            {
                if (localBoundsComeFromPoints)
                {
                    RectangleDouble localBounds = new RectangleDouble(double.PositiveInfinity, double.PositiveInfinity, double.NegativeInfinity, double.NegativeInfinity);

                    this.RewindZero();
                    double x;
                    double y;
                    ShapePath.FlagsAndCommand cmd;
                    int numPoint = 0;
                    while (!ShapePath.IsStop(cmd = GetNextVertex(out x, out y)))
                    {
                        numPoint++;
                        localBounds.ExpandToInclude(x, y);
                    }

                    if (numPoint == 0)
                    {
                        localBounds = new RectangleDouble();
                    }

                    return localBounds;
                }
                else
                {
                    return base.LocalBounds;
                }
            }

            set
            {
                if (localBoundsComeFromPoints)
                {
                    //throw new NotImplementedException();
                    base.LocalBounds = value;
                }
                else
                {
                    base.LocalBounds = value;
                }
            }
        }

        public abstract int num_paths();
        public abstract IEnumerable<VertexData> GetVertexIter();
       
        public abstract void RewindZero();
        public abstract ShapePath.FlagsAndCommand GetNextVertex(out double x, out double y);

        public virtual IColor color(int i) { return (IColor)new ColorRGBAf().GetAsRGBA_Bytes(); }

        public override void OnDraw(Graphics2D graphics2D)
        {
            var list = new System.Collections.Generic.List<VertexData>();
            this.RewindZero();

            ShapePath.FlagsAndCommand cmd;
            double x, y;
            while ((cmd = this.GetNextVertex(out x, out y)) != ShapePath.FlagsAndCommand.CommandStop)
            {
                list.Add(new VertexData(cmd, new Vector2(x, y)));
            }
            //foreach (var v in this.GetVertexIter())
            //{

            //}
            graphics2D.Render(new SinglePath(new VertexStorage(list), 0),
                color(0).GetAsRGBA_Bytes());


            //for (int i = 0; i < num_paths(); i++)
            //{
            //    graphics2D.Render(this, i, color(i).GetAsRGBA_Bytes());
            //}
            base.OnDraw(graphics2D);
        }

        public abstract bool IsDynamicVertexGen
        {
            get;
        }
    }
}
