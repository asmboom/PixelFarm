﻿//2014 Apache2, WinterDev
using System;
using System.Collections.Generic;
using System.Text;
using LayoutFarm.Drawing;
using LayoutFarm.Drawing.DrawingGL;

namespace LayoutFarm.Drawing.DrawingGL
{
    public static class CanvasGLPortal
    {
        static bool isInit;
        static CanvasGLPlatform platform;
        public static void Start()
        {

            if (isInit)
            {
                return;
            }
            isInit = true;
            CurrentGraphicsPlatform.SetCurrentPlatform(CanvasGLPortal.platform = new CanvasGLPlatform());
            CurrentGraphicsPlatform.GenericSerifFontName = System.Drawing.FontFamily.GenericSerif.Name;

        }
        public static void End()
        {

        }
        public static GraphicsPlatform P
        {
            get { return platform; }
        }
    }
}