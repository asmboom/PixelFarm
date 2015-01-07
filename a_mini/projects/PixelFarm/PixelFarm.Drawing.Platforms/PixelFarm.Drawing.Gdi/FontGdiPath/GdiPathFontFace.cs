﻿//MIT 2014, WinterDev   
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using PixelFarm;

namespace PixelFarm.Agg.Fonts
{
    class GdiPathFontFace : FontFace
    {
        Dictionary<int, GdiPathFont> stockFonts = new Dictionary<int, GdiPathFont>();
        public GdiPathFontFace(string facename)
        {
            this.FaceName = facename;
        }
        protected override void OnDispose()
        {
        }
        public string FaceName
        {
            get;
            private set;
        }
        public GdiPathFont GetFontAtSpecificSize(int emsize)
        {
            GdiPathFont found;
            if (!stockFonts.TryGetValue(emsize, out found))
            {   
                found = new GdiPathFont(this, emsize);
                stockFonts.Add(emsize, found);
            }
            return found;
        }
    }
}