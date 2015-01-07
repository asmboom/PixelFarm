﻿//2014,2015 MIT, WinterDev
namespace PixelFarm.Drawing
{
    public abstract class GraphicsPlatform
    {
        public abstract FontInfo GetFont(string fontfaceName, float emsize, FontStyle st);
        public abstract GraphicsPath CreateGraphicsPath();
        public abstract Canvas CreateCanvas(
            int left,
            int top,
            int width,
            int height);

        public abstract IFonts SampleIFonts { get; }

        public static string GenericSerifFontName
        {
            get;
            set;
        }
    }


}