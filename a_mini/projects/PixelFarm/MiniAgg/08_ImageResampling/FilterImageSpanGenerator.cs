//2014,2015 BSD,WinterDev   
//----------------------------------------------------------------------------
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
//
// Image transformations with filtering. Span generator base class
//
//----------------------------------------------------------------------------
using System;
using img_subpix_scale = PixelFarm.Agg.ImageFilterLookUpTable.ImgSubPixConst;

namespace PixelFarm.Agg.Image
{
    //=====================================================span_image_resample
    public abstract partial class FilterImageSpanGenerator : ImgSpanGen
    {
        ImageFilterLookUpTable filterLookup;
        int m_scale_limit;
        const int m_blur_x = img_subpix_scale.SCALE;
        const int m_blur_y = img_subpix_scale.SCALE;
        ImageBufferAccessor imageBufferAccessor;
        public FilterImageSpanGenerator(IImageReaderWriter src,
                            ISpanInterpolator inter,
                            ImageFilterLookUpTable filterLookup)
            : base(inter)
        {
            this.imageBufferAccessor = new ImageBufferAccessor(src);
            m_scale_limit = 20;
            //m_blur_x = ((int)img_subpix_scale.SCALE);
            //m_blur_y = ((int)img_subpix_scale.SCALE);
            this.filterLookup = filterLookup;
        }

        protected byte[] BaseGetSpan(int x, int y, int len, out int bufferOffset)
        {
            return this.imageBufferAccessor.GetSpan(x, y, len, out bufferOffset);
        }
        protected byte[] BaseNextX(out int bufferOffset)
        {
            return this.imageBufferAccessor.NextX(out bufferOffset);
        }
        protected byte[] BaseNextY(out int bufferOffset)
        {
            return this.imageBufferAccessor.NextY(out bufferOffset);
        }



        protected ImageFilterLookUpTable FilterLookup
        {
            get { return filterLookup; }
        }

        protected void AdjustScale(ref int rx, ref int ry)
        {
            if (rx < img_subpix_scale.SCALE)
            {
                rx = img_subpix_scale.SCALE;
            }
            else if (rx > img_subpix_scale.SCALE * m_scale_limit)
            {
                rx = img_subpix_scale.SCALE * m_scale_limit;
            }
            //-----------------------------------------------------------
            if (ry < img_subpix_scale.SCALE)
            {
                ry = img_subpix_scale.SCALE;
            }
            else if (ry > img_subpix_scale.SCALE * m_scale_limit)
            {
                ry = img_subpix_scale.SCALE * m_scale_limit;
            }
            //-----------------------------------------------------------

            rx = (rx * m_blur_x) >> img_subpix_scale.SHIFT;
            ry = (ry * m_blur_y) >> img_subpix_scale.SHIFT;

            if (rx < img_subpix_scale.SCALE) rx = img_subpix_scale.SCALE;
            if (ry < img_subpix_scale.SCALE) ry = img_subpix_scale.SCALE;
        }


    }
}
