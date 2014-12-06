﻿//2014 BSD, WinterDev
//ArthurHub

// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;
using System.Text;
using LayoutFarm.Drawing;


namespace LayoutFarm
{

    partial class MyCanvas
    {

        float strokeWidth = 1f;
        Color fillSolidColor = Color.Transparent;
        public override float StrokeWidth
        {
            get
            {
                return this.strokeWidth;
            }
            set
            {
                this.internalPen.Width = strokeWidth;
                this.strokeWidth = value;
            }
        }
        public override Color FillSolidColor
        {
            get
            {
                return fillSolidColor;
            }
            set
            {
                this.fillSolidColor = value;
                this.internalBrush.Color = ConvColor(value);
            }
        }
        public override IGraphics GetIGraphics()
        {
            return this;
        }

        public bool IsPageNumber(int hPageNum, int vPageNum)
        {
            return pageNumFlags == ((hPageNum << 8) | vPageNum);
        }
        public bool IsUnused
        {
            get
            {
                return (pageFlags & CANVAS_UNUSED) != 0;
            }
            set
            {
                if (value)
                {
                    pageFlags |= CANVAS_UNUSED;
                }
                else
                {
                    pageFlags &= ~CANVAS_UNUSED;
                }
            }
        }

    }
}