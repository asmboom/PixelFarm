﻿//MIT 2014, WinterDev
using System;
using LayoutFarm.Drawing;

namespace OpenTK.Graphics.OpenGL
{
    public static class GLHelper
    {
        public static void ClearColor(Color c)
        {
            GL.ClearColor(
                (float)c.R / 255f,
                (float)c.G / 255f,
                (float)c.B / 255f,
                (float)c.A / 255f);
        }
        public static void Color3(Color c)
        {
            GL.Color3(
                 (float)c.R / 255f,
                 (float)c.G / 255f,
                 (float)c.B / 255f);
        }
        public static LayoutFarm.Drawing.Rectangle ConvToRect(Rectangle openTkRect)
        {
            return new LayoutFarm.Drawing.Rectangle(
                openTkRect.X,
                openTkRect.Y,
                openTkRect.Width,
                openTkRect.Height);
        }
    }
}
