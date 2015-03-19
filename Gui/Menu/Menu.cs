﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using MatterHackers.VectorMath;

namespace MatterHackers.Agg.UI
{
    public enum Direction { Up, Down };

    public class Menu : Button
    {
        public bool AlignToRightEdge { get; set; }
        public Vector2 OpenOffset { get; set; }
        bool menuIsOpen = false;
        public bool IsOpen { get { return menuIsOpen; } }
        Direction menuDirection;
        double maxHeight;
        public ObservableCollection<MenuItem> MenuItems = new ObservableCollection<MenuItem>();

        public BorderDouble MenuItemsPadding { get; set; }

        RGBA_Bytes menuItemsBackgroundColor = RGBA_Bytes.White;
        public RGBA_Bytes MenuItemsBackgroundColor
        {
            get { return menuItemsBackgroundColor; }
            set { menuItemsBackgroundColor = value; }
        }

        public Direction MenuDirection { get {return menuDirection;}}

        RGBA_Bytes menuItemsHoverColor = RGBA_Bytes.Gray;
        public RGBA_Bytes MenuItemsBackgroundHoverColor
        {
            get { return menuItemsHoverColor; }
            set { menuItemsHoverColor = value; }
        }

        RGBA_Bytes menuItemsTextColor = RGBA_Bytes.Black;
        public RGBA_Bytes MenuItemsTextColor
        {
            get { return menuItemsTextColor; }
            set { menuItemsTextColor = value; }
        }

        RGBA_Bytes menuItemsTextHoverColor = RGBA_Bytes.Black;
        public RGBA_Bytes MenuItemsTextHoverColor
        {
            get { return menuItemsTextHoverColor; }
            set { menuItemsTextHoverColor = value; }
        }

        RGBA_Bytes menuItemsBorderColor;
        public RGBA_Bytes MenuItemsBorderColor
        {
            get { return menuItemsBorderColor; }
            set { menuItemsBorderColor = value; }
        }

        int borderWidth = 1;
        public int MenuItemsBorderWidth { get { return borderWidth; } set { borderWidth = value; } }

        // If max height is > 0 it will limit the height of the menu
        public Menu(Direction direction = Direction.Down, double maxHeight = 0)
        {
            this.maxHeight = maxHeight;
            this.menuDirection = direction;
            Click += new EventHandler(ListMenu_Click);
            VAnchor = UI.VAnchor.FitToChildren;
            HAnchor = UI.HAnchor.FitToChildren;
        }

        // If max height is > 0 it will limit the height of the menu
        public Menu(GuiWidget view, Direction direction = Direction.Down, double maxHeight = 0)
            : this(direction, maxHeight)
        {
            AddChild(view);
        }

        bool mouseDownWhileOpen = false;
        public override void OnMouseDown(MouseEventArgs mouseEvent)
        {
            if (IsOpen)
            {
                mouseDownWhileOpen = true;
            }
            else
            {
                mouseDownWhileOpen = false;
            }
            base.OnMouseDown(mouseEvent);
        }

        void ListMenu_Click(object sender, EventArgs mouseEvent)
        {
            if (mouseDownWhileOpen)
            {
            }
            else
            {
                UiThread.RunOnIdle(ShowMenu);
                menuIsOpen = true;
            }
        }

        void ShowMenu(object state)
        {
            if (this.Parent == null)
            {
                throw new Exception("You cannot show the menu on a Menu unless it has a parent (has been added to a GuiWidegt).");
            }

            OpenMenuContents dropListItems = new OpenMenuContents(MenuItems, this, OpenOffset, menuDirection, MenuItemsBackgroundColor, MenuItemsBorderColor, MenuItemsBorderWidth, maxHeight, AlignToRightEdge);
            dropListItems.Closed += new EventHandler(DropListItems_Closed);

            dropListItems.Focus();
        }

        virtual protected void DropListItems_Closed(object sender, EventArgs e)
        {
            OpenMenuContents dropListItems = (OpenMenuContents)sender;
            dropListItems.Closed -= new EventHandler(DropListItems_Closed);
            menuIsOpen = false;
        }
    }
}
