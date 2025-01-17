﻿using MsmhToolsClass;
using MsmhToolsWinFormsClass;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Windows.Forms.Design;
/*
* Copyright MSasanMH, June 20, 2022.
*/

namespace CustomControls
{
    public class CustomContextMenuStrip : ContextMenuStrip
    {
        private readonly CustomToolStripRenderer MyRenderer = new();

        private Color mBorderColor = Color.Blue;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Border Color")]
        public Color BorderColor
        {
            get { return mBorderColor; }
            set
            {
                if (mBorderColor != value)
                {
                    mBorderColor = value;
                    BorderColorChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        private int mRoundedCorners = 0;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Rounded Corners")]
        public int RoundedCorners
        {
            get { return mRoundedCorners; }
            set
            {
                if (mRoundedCorners != value)
                {
                    mRoundedCorners = value;
                    Invalidate();
                }
            }
        }

        private Color mSelectionColor = Color.LightBlue;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Editor(typeof(WindowsFormsComponentEditor), typeof(Color))]
        [Category("Appearance"), Description("Selection Color")]
        public Color SelectionColor
        {
            get { return mSelectionColor; }
            set
            {
                if (mSelectionColor != value)
                {
                    mSelectionColor = value;
                    SelectionColorChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        private bool mSameColorForSubItems = true;
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Appearance"), Description("Same Color For Sub Items")]
        public bool SameColorForSubItems
        {
            get { return mSameColorForSubItems; }
            set
            {
                if (mSameColorForSubItems != value)
                {
                    mSameColorForSubItems = value;
                    SameColorForSubItemsChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Action"), Description("Same Color For Sub Items Changed Event")]
        public event EventHandler? SameColorForSubItemsChanged;

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Border Color Changed Event")]
        public event EventHandler? BorderColorChanged;

        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true)]
        [Category("Property Changed"), Description("Selection Color Changed Event")]
        public event EventHandler? SelectionColorChanged;

        public CustomContextMenuStrip() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            
            // Default
            BackColor = Color.DimGray;
            ForeColor = Color.White;
            BorderColor = Color.Blue;
            SelectionColor = Color.LightBlue;

            MyRenderer.BackColor = GetBackColor();
            MyRenderer.ForeColor = GetForeColor();
            MyRenderer.BorderColor = GetBorderColor();
            MyRenderer.RoundedCorners = RoundedCorners;
            MyRenderer.SelectionColor = SelectionColor;
            Renderer = MyRenderer;

            BackColorChanged += CustomContextMenuStrip_BackColorChanged;
            ForeColorChanged += CustomContextMenuStrip_ForeColorChanged;
            BorderColorChanged += CustomContextMenuStrip_BorderColorChanged;
            SelectionColorChanged += CustomContextMenuStrip_SelectionColorChanged;
            SameColorForSubItemsChanged += CustomContextMenuStrip_SameColorForSubItemsChanged;
            ItemAdded += CustomContextMenuStrip_ItemAdded;
            Paint += CustomContextMenuStrip_Paint;
            Application.Idle += Application_Idle;

            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 100;
            timer.Tick += (s, e) =>
            {
                if (SameColorForSubItems) ColorForSubItems();
                if (RoundedCorners > 0 && Visible) Invalidate();
            };
            timer.Start();
        }

        private void Application_Idle(object? sender, EventArgs e)
        {
            MyRenderer.RoundedCorners = RoundedCorners;
        }

        private void ColorForSubItems()
        {
            for (int a = 0; a < Items.Count; a++)
            {
                ToolStripItem toolStripItem = Items[a];
                var toolStripItems = Controllers.GetAllToolStripItems(toolStripItem);
                for (int b = 0; b < toolStripItems.Count(); b++)
                {
                    ToolStripItem? tsi = toolStripItems.ToList()[b];
                    if (tsi is ToolStripMenuItem)
                    {
                        if (tsi is ToolStripMenuItem tsmi)
                        {
                            tsmi.BackColor = GetBackColor();
                            tsmi.ForeColor = GetForeColor();
                        }
                    }
                    else if (tsi is ToolStripSeparator)
                    {
                        if (tsi is ToolStripSeparator tss)
                        {
                            tss.BackColor = GetBackColor();
                            tss.ForeColor = BorderColor;
                        }
                    }
                }
            }
        }

        private void CustomContextMenuStrip_BackColorChanged(object? sender, EventArgs e)
        {
            MyRenderer.BackColor = GetBackColor();
            Invalidate();
        }

        private void CustomContextMenuStrip_ForeColorChanged(object? sender, EventArgs e)
        {
            MyRenderer.ForeColor = GetForeColor();
            Invalidate();
        }

        private void CustomContextMenuStrip_BorderColorChanged(object? sender, EventArgs e)
        {
            MyRenderer.BorderColor = GetBorderColor();
            MyRenderer.RoundedCorners = RoundedCorners;
            Invalidate();
        }

        private void CustomContextMenuStrip_SelectionColorChanged(object? sender, EventArgs e)
        {
            MyRenderer.SelectionColor = SelectionColor;
            Invalidate();
        }

        private void CustomContextMenuStrip_SameColorForSubItemsChanged(object? sender, EventArgs e)
        {
            if (SameColorForSubItems)
                ColorForSubItems();
        }

        private void CustomContextMenuStrip_ItemAdded(object? sender, ToolStripItemEventArgs e)
        {
            if (SameColorForSubItems)
                ColorForSubItems();
            Invalidate();
        }

        private void CustomContextMenuStrip_Paint(object? sender, PaintEventArgs e)
        {
            // Main Menu Border
            Color borderColor = GetBorderColor();
            Rectangle rect = new(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
            int r = RoundedCorners;
            using Pen pen = new(borderColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawRoundedRectangle(pen, rect, r, r, r, r);
            e.Graphics.SmoothingMode = SmoothingMode.Default;
        }

        private Color GetBackColor()
        {
            if (Enabled)
                return BackColor;
            else
            {
                if (BackColor.DarkOrLight() == "Dark")
                    return BackColor.ChangeBrightness(0.2f);
                else
                    return BackColor.ChangeBrightness(-0.2f);
            }
        }

        private Color GetForeColor()
        {
            if (Enabled)
                return ForeColor;
            else
            {
                if (ForeColor.DarkOrLight() == "Dark")
                    return ForeColor.ChangeBrightness(0.2f);
                else
                    return ForeColor.ChangeBrightness(-0.2f);
            }
        }

        private Color GetBorderColor()
        {
            if (Enabled)
                return BorderColor;
            else
            {
                if (BorderColor.DarkOrLight() == "Dark")
                    return BorderColor.ChangeBrightness(0.3f);
                else
                    return BorderColor.ChangeBrightness(-0.3f);
            }
        }
    }
}
