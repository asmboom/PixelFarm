﻿//2014 BSD,WinterDev
//MatterHackers

using System;

using MatterHackers.Agg.UI;
using MatterHackers.Agg.Image;
using MatterHackers.Agg.VertexSource;

using MatterHackers.VectorMath;

using Mini;
namespace MatterHackers.Agg.Sample_AADemoTest1
{
    public class square
    {
        double m_size;

        public square(double size)
        {
            m_size = size;
        }

        public void draw(ScanlineRasterizer ras, IScanline sl, IImage destImage, ColorRGBA color,
                  double x, double y)
        {
            ras.reset();
            ras.move_to_d(x * m_size, y * m_size);
            ras.line_to_d(x * m_size + m_size, y * m_size);
            ras.line_to_d(x * m_size + m_size, y * m_size + m_size);
            ras.line_to_d(x * m_size, y * m_size + m_size);
            ScanlineRenderer scanlineRenderer = new ScanlineRenderer();
            scanlineRenderer.render_scanlines_aa_solid(destImage, ras, sl, color);
        }
    }

    class renderer_enlarged_test1 : ScanlineRenderer
    {
        double m_size;
        square m_square;
        ScanlineUnpacked8 m_sl = new ScanlineUnpacked8();
        public renderer_enlarged_test1(double size)
        {
            m_size = size;
            m_square = new square(size);
        }


        protected override void RenderSolidSingleScanLine(IImage destImage, IScanline scanline, ColorRGBA color)
        {
            int y = scanline.Y;
            int num_spans = scanline.SpanCount;

            byte[] covers = scanline.GetCovers();
            var gfx = Graphics2D.CreateFromImage(destImage);

            for (int i = 1; i <= num_spans; ++i)
            {
                ScanlineSpan span = scanline.GetSpan(i);

                int x = span.x;
                int num_pix = span.len;
                int coverIndex = span.cover_index;
                do
                {
                    int a = (covers[coverIndex++] * color.Alpha0To255) >> 8;
                    m_square.draw(
                           gfx.Rasterizer, m_sl, destImage,
                            ColorRGBA.Make(color.Red0To255, color.Green0To255, color.Blue0To255, a),
                            x, y);
                    ++x;
                }
                while (--num_pix > 0);
            }

        }
    }

    [Info(OrderCode = "02")]
    [Info("Demonstration of the Anti-Aliasing principle with Subpixel Accuracy. The triangle "
                    + "is rendered two times, with its “natural” size (at the bottom-left) and enlarged. "
                    + "To draw the enlarged version there is a special scanline renderer written (see "
                    + "class renderer_enlarged in the source code). You can drag the whole triangle as well "
                    + "as each vertex of it. Also change “Gamma” to see how it affects the quality of Anti-Aliasing.")]
    public class aa_demo_test1 : DemoBase
    {
        double[] m_x = new double[3];
        double[] m_y = new double[3];
        double m_dx;
        double m_dy;
        int m_idx;


        public aa_demo_test1()
        {
            m_idx = -1;
            m_x[0] = 57; m_y[0] = 100;
            m_x[1] = 369; m_y[1] = 170;
            m_x[2] = 143; m_y[2] = 310;

            //init value
            this.PixelSize = 32;
            this.GammaValue = 1;
        }

        [DemoConfig(MinValue = 8, MaxValue = 100)]
        public double PixelSize
        {
            get;
            set;
        }
        [DemoConfig(MinValue = 0, MaxValue = 3)]
        public double GammaValue
        {
            get;
            set;
        }
        public override void Draw(Graphics2D g)
        {
            OnDraw(g);
        }

        public void OnDraw(Graphics2D graphics2D)
        {
            var widgetsSubImage = ImageHelper.NewSubImageReference(graphics2D.DestImage, graphics2D.GetClippingRect());

            GammaLookUpTable gamma = new GammaLookUpTable(this.GammaValue);
            IRecieveBlenderByte NormalBlender = new BlenderBGRA();
            IRecieveBlenderByte GammaBlender = new BlenderGammaBGRA(gamma);
            var rasterGamma = new ChildImage(widgetsSubImage, GammaBlender);


            ClipProxyImage clippingProxyNormal = new ClipProxyImage(widgetsSubImage);
            ClipProxyImage clippingProxyGamma = new ClipProxyImage(rasterGamma);

            clippingProxyNormal.clear(ColorRGBA.White);
            ScanlineRasterizer rasterizer = new ScanlineRasterizer();
            ScanlineUnpacked8 sl = new ScanlineUnpacked8();

            int size_mul = (int)this.PixelSize;

            renderer_enlarged_test1 ren_en = new renderer_enlarged_test1(size_mul);

            rasterizer.reset();
            rasterizer.move_to_d(m_x[0] / size_mul, m_y[0] / size_mul);
            rasterizer.line_to_d(m_x[1] / size_mul, m_y[1] / size_mul);
            rasterizer.line_to_d(m_x[2] / size_mul, m_y[2] / size_mul);
            ren_en.render_scanlines_aa_solid(clippingProxyGamma, rasterizer, sl, ColorRGBA.Black);

            //----------------------------------------
            ScanlineRenderer scanlineRenderer = new ScanlineRenderer();
            scanlineRenderer.render_scanlines_aa_solid(clippingProxyGamma, rasterizer, sl, ColorRGBA.Black);
            rasterizer.ResetGamma(new gamma_none());
            //----------------------------------------
            PathStorage ps = new PathStorage();
            Stroke pg = new Stroke(ps);
            pg.width(2);
            ps.remove_all();
            ps.MoveTo(m_x[0], m_y[0]);
            ps.LineTo(m_x[1], m_y[1]);
            ps.LineTo(m_x[2], m_y[2]);
            ps.LineTo(m_x[0], m_y[0]);

            rasterizer.add_path(pg);

            scanlineRenderer.render_scanlines_aa_solid(clippingProxyNormal, rasterizer, sl, new ColorRGBA(0, 150, 160, 200));

        }
        public override void MouseDown(int mx, int my, bool isRightButton)
        {
            double x = mx;
            double y = my;
            int i;
            for (i = 0; i < 3; i++)
            {
                if (Math.Sqrt((x - m_x[i]) * (x - m_x[i]) + (y - m_y[i]) * (y - m_y[i])) < 5.0)
                {
                    m_dx = x - m_x[i];
                    m_dy = y - m_y[i];
                    m_idx = i;
                    break;
                }
            }
            if (i == 3)
            {
                if (agg_math.point_in_triangle(m_x[0], m_y[0],
                                      m_x[1], m_y[1],
                                      m_x[2], m_y[2],
                                      x, y))
                {
                    m_dx = x - m_x[0];
                    m_dy = y - m_y[0];
                    m_idx = 3;
                }
            }
        }
        public override void MouseDrag(int mx, int my)
        {
            double x = mx;
            double y = my;
            if (m_idx == 3)
            {
                double dx = x - m_dx;
                double dy = y - m_dy;
                m_x[1] -= m_x[0] - dx;
                m_y[1] -= m_y[0] - dy;
                m_x[2] -= m_x[0] - dx;
                m_y[2] -= m_y[0] - dy;
                m_x[0] = dx;
                m_y[0] = dy;

                return;
            }

            if (m_idx >= 0)
            {
                m_x[m_idx] = x - m_dx;
                m_y[m_idx] = y - m_dy;

            }
        }

        public override void MouseUp(int x, int y)
        {
            m_idx = -1;
            base.MouseUp(x, y);
        }
    }


}