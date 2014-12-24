﻿
#region Using Directives

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using Examples.Tutorial;
using Mini;

#endregion

using LayoutFarm.DrawingGL;
namespace OpenTkEssTest
{
    [Info(OrderCode = "105")]
    [Info("T105_Stencil")]
    public class T105_Stencil : PrebuiltGLControlDemoBase
    {

        CanvasGL2d canvas2d;
        protected override void OnInitGLProgram(object sender, EventArgs args)
        {

            //--------------------------------------------------------------------------------
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.ClearColor(1, 1, 1, 1);
            //setup viewport size
            int max = Math.Max(this.Width, this.Height);

            canvas2d = new CanvasGL2d(max, max);

            //square viewport
            GL.Viewport(0, 0, max, max);
        }
        protected override void DemoClosing()
        {
            canvas2d.Dispose();
        }
        protected override void OnGLRender(object sender, EventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            canvas2d.SmoothMode = CanvasSmoothMode.AggSmooth; 
            canvas2d.FillCircle(LayoutFarm.Drawing.Color.OrangeRed, 100, 400, 25); 
            var color = LayoutFarm.Drawing.Color.OrangeRed;
            canvas2d.FillEllipse(
                new LayoutFarm.Drawing.Color(
                    100,
                    color.R,
                    color.G,
                    color.B), 200, 400, 25, 50);
            
            ////--------------------------------------------
            miniGLControl.SwapBuffers();
        }

    }


}
