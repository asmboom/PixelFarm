﻿/*
Copyright (c) 2014, Lars Brubaker
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies, 
either expressed or implied, of the FreeBSD Project.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MatterHackers.Agg;
using MatterHackers.Agg.VertexSource;
using MatterHackers.VectorMath;
using MatterHackers.Agg.Transform;

namespace MatterHackers.Agg.UI
{
    public class DefaultThumbView : GuiWidget
    {
        public RGBA_Bytes ThumbColor = RGBA_Bytes.DarkGray;
        public override void OnDraw(Graphics2D graphics2D)
        {
            RoundedRect rect = new RoundedRect(LocalBounds, 0);
            //RoundedRect rect = new RoundedRect(LocalBounds, 0);
            graphics2D.Render(rect, ThumbColor);
            base.OnDraw(graphics2D);
        }
    }

    public class ThumDragWidget : GuiWidget
    {
        bool mouseDownOnThumb = false;
        Vector2 MouseDownPosition;
        Orientation orientation;

        public ThumDragWidget(Orientation orientation)
        {
            this.orientation = orientation;
        }

        public override void OnBoundsChanged(EventArgs e)
        {
            if (Children.Count != 1)
            {
                throw new Exception("We should have one child that is the thum view.");
            }
            Children[0].LocalBounds = LocalBounds;
            base.OnBoundsChanged(e);
        }

        protected bool MouseDownOnThumb
        {
            get { return mouseDownOnThumb; }
            set { mouseDownOnThumb = value; }
        }

        override public void OnMouseDown(MouseEventArgs mouseEvent)
        {
            MouseDownOnThumb = true;
            Vector2 mousePosition = new Vector2(mouseEvent.X, mouseEvent.Y);
            MouseDownPosition = mousePosition;

            base.OnMouseDown(mouseEvent);
        }

        override public void OnMouseUp(MouseEventArgs mouseEvent)
        {
            MouseDownOnThumb = false;
            base.OnMouseUp(mouseEvent);
        }

        override public void OnMouseMove(MouseEventArgs mouseEvent)
        {
            if (MouseDownOnThumb)
            {
                Vector2 mousePosition = new Vector2(mouseEvent.X, mouseEvent.Y);

                Vector2 deltaFromDownPosition = mousePosition - MouseDownPosition;

                if (orientation == Orientation.Vertical)
                {
                    deltaFromDownPosition.x = 0;
                }
                else
                {
                    deltaFromDownPosition.y = 0;
                }

                ScrollBar parentScrollBar = (ScrollBar)Parent;
                parentScrollBar.MoveThumb(deltaFromDownPosition);
            }
            base.OnMouseMove(mouseEvent);
        }
    }

    public class ScrollBar : GuiWidget
    {
        ScrollableWidget ParentScrollWidget;
        GuiWidget background;
        ThumDragWidget thumb;

		public static double ScrollBarWidth = 15;

        public enum ShowState { Never, WhenRequired, Always };

        ShowState showState = ShowState.WhenRequired;
        public ShowState Show
        {
            get
            {
                return showState;
            }

            set
            {
                if (value != showState)
                {
                    showState = value;
                    switch (showState)
                    {
                        case ShowState.Never:
                            Visible = false;
                            break;

                        case ShowState.WhenRequired:
                            break;

                        case ShowState.Always:
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }

        internal ScrollBar(ScrollableWidget parent, Orientation orientation = Orientation.Vertical)
            : this(parent, new GuiWidget(), new DefaultThumbView(), orientation)
        {
        }

        internal ScrollBar(ScrollableWidget parent, GuiWidget background, GuiWidget thumbView, Orientation orientation = Orientation.Vertical)
        {
            ParentScrollWidget = parent;

            this.background = background;
            thumb = new ThumDragWidget(orientation);
            thumb.AddChild(thumbView);

            background.BackgroundColor = RGBA_Bytes.LightGray;

            AddChild(background);
            AddChild(thumb);

            BackgroundColor = RGBA_Bytes.Blue;

            ParentScrollWidget.BoundsChanged += new EventHandler(Parent_BoundsChanged);
            ParentScrollWidget.ScrollArea.BoundsChanged += new EventHandler(ScrollArea_BoundsChanged);
            ParentScrollWidget.ScrollPositionChanged += new EventHandler(scrollWidgeContainingThis_ScrollPositionChanged);
            ParentScrollWidget.ScrollArea.MarginChanged += new EventHandler(ScrollArea_MarginChanged);
            UpdateScrollBar();
        }

        void ScrollArea_MarginChanged(object sender, EventArgs e)
        {
            UpdateScrollBar();
        }

        void scrollWidgeContainingThis_ScrollPositionChanged(object sender, EventArgs e)
        {
            UpdateScrollBar();
        }

        override public void OnMouseDown(MouseEventArgs mouseEvent)
        {
            if (!thumb.BoundsRelativeToParent.Contains(mouseEvent.X, mouseEvent.Y))
            {
                // we did not click on the thumb so we want to move the scroll bar towards the click
                if (mouseEvent.Y < thumb.OriginRelativeParent.y)
                {
                    MoveThumb(new Vector2(0, -thumb.Height));
                }
                else
                {
                    MoveThumb(new Vector2(0, thumb.Height));
                }
            }

            base.OnMouseDown(mouseEvent);
        }

        void UpdateScrollBar()
        {
            switch (Show)
            {
                case ShowState.WhenRequired:
                    if (ParentScrollWidget.ScrollArea.Height > ParentScrollWidget.Height)
                    {
                        goto case ShowState.Always;
                    }
                    else
                    {
                        goto case ShowState.Never;
                    }

                case ShowState.Always:
                    // make sure we can see it
                    Visible = true;
                    // fix the bounds of the scroll bar background
                    LocalBounds = new RectangleDouble(0, 0, ScrollBarWidth, ParentScrollWidget.Height);
                    background.LocalBounds = LocalBounds;

                    // this is for vertical scroll bar
                    thumb.LocalBounds = new RectangleDouble(0, 0, ScrollBarWidth, ThumbHeight);

                    Vector2 scrollRatioFromTop0To1 = ParentScrollWidget.ScrollRatioFromTop0To1;
                    double notThumbHeight = ParentScrollWidget.Height - ThumbHeight;
                    thumb.OriginRelativeParent = new Vector2(0, notThumbHeight * scrollRatioFromTop0To1.y);
                    break;

                case ShowState.Never:
                    Visible = false;
                    break;
            }
        }

        internal void MoveThumb(Vector2 deltaToMove)
        {
            double notThumbHeight = ParentScrollWidget.Height - ThumbHeight;
            double changeRatio = deltaToMove.y / notThumbHeight;
            ParentScrollWidget.ScrollRatioFromTop0To1 = ParentScrollWidget.ScrollRatioFromTop0To1 + new Vector2(0, changeRatio);
        }

        internal double ThumbHeight
        {
            get
            {
                Vector2 ratioOfViewToContents0To1 = ParentScrollWidget.RatioOfViewToContents0To1();
                return ratioOfViewToContents0To1.y * ParentScrollWidget.Height;
            }
        }

        void ScrollArea_BoundsChanged(object sender, EventArgs e)
        {
            UpdateScrollBar();
        }

        void Parent_BoundsChanged(object sender, EventArgs e)
        {
            UpdateScrollBar();
        }
    }
}
