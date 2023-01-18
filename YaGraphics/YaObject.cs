﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace YaGraphics
{
    internal class YaObject
    {
        /// <summary>
        /// Used for template names and position references
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// X position of object. 0.5 means 50% from the left edge of the parent
        /// Can also reference other objects for positioning. Ex: 0.1 below obj1
        /// </summary>
        protected string X { get; set; } = string.Empty;

        /// <summary>
        /// Y position of object. 0.5 means 50% from the top edge of the parent
        /// Can also reference other objects for positioning. Ex: 0.1 below obj1
        /// </summary>
        protected string Y { get; set; } = string.Empty;

        /// <summary>
        /// Rotation anchor X position
        /// </summary>
        public float AnchorX { get; set; }

        /// <summary>
        /// Rotation anchor Y position
        /// </summary>
        public float AnchorY { get; set; }

        /// <summary>
        /// Width of object in pixels. Used for the main object and template objects
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Height of object in pixels. Used for the main object and template objects
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// X scaling of object. 0.5 means 50% as wide as the parent
        /// </summary>
        public decimal ScaleX { get; set; }

        /// <summary>
        /// Y scaling of object. 0.5 means 50% as tall as the parent
        /// </summary>
        public decimal ScaleY { get; set; }
        public List<YaObject> Templates { get; set; } = new();
        public List<YaObject> Children { get; set; } = new();

        public virtual PointF GetPosition(RectangleF parentRect, Dictionary<string, YaObject> siblings)
        {
            var size = GetSize(parentRect);

            var splX = X.Split(' ');
            var splY = Y.Split(' ');

            bool xPx = false;
            bool yPx = false;

            if (splX[0].EndsWith("px"))
            {
                splX[0] = splX[0][..^2];
                xPx = true;
            }

            if (splY[0].EndsWith("px"))
            {
                splY[0] = splY[0][..^2];
                yPx = true;
            }

            YaObject? foundSibling = null;
            bool toLeft = false;
            bool middle = false;
            bool centered = false;
            int i = 0;

            if (float.TryParse(splX[0], out float x))
            {
                i++;
            }

            // Process X position
            for (; i < splX.Length; i++)
            {
                switch (splX[i].ToLower())
                {
                    case "center":
                    case "centered":
                        centered = true;
                        break;
                    case "middle":
                        middle = true;
                        break;
                    case "left":
                        toLeft = true;
                        break;
                    case "right":
                        toLeft = false;
                        break;
                    default:
                        break;
                }

                if (siblings.ContainsKey(splX[i]))
                {
                    foundSibling = siblings[splX[i]];
                }
            }

            if (!xPx)
            {
                x *= parentRect.Width;
            }

            if (foundSibling != null)
            {
                var sibPos = foundSibling.GetPosition(parentRect, siblings);
                var sibSize = foundSibling.GetSize(parentRect);

                if (toLeft)
                {
                    x *= -1;
                    x -= size.Width;
                }
                else
                {
                    x += sibSize.Width;
                }

                x += sibPos.X;

                if (middle)
                {
                    x += 0.5f * (toLeft ? sibSize.Width : -sibSize.Width);
                }

                if (centered)
                {
                    x += 0.5f* (toLeft ? size.Width : -size.Width);
                }
            }

            foundSibling = null;
            bool toAbove = false;
            middle = false;
            centered = false;
            i = 0;

            if (float.TryParse(splY[0], out float y))
            {
                i++;
            }

            // Process Y position
            for (; i < splY.Length; i++)
            {
                switch (splY[i].ToLower())
                {
                    case "center":
                    case "centered":
                        centered = true;
                        break;
                    case "middle":
                        middle = true;
                        break;
                    case "above":
                        toAbove = true;
                        break;
                    case "below":
                        toAbove = false;
                        break;
                    default:
                        break;
                }

                if (siblings.ContainsKey(splY[i]))
                {
                    foundSibling = siblings[splY[i]];
                }
            }

            if (!yPx)
            {
                y *= parentRect.Height;
            }

            if (foundSibling != null)
            {
                var sibPos = foundSibling.GetPosition(parentRect, siblings);
                var sibSize = foundSibling.GetSize(parentRect);

                if (toAbove)
                {
                    y *= -1;
                    y -= size.Height;
                }
                else
                {
                    y += sibSize.Height;
                }

                y += sibPos.Y;

                if (middle)
                {
                    y+= 0.5f * (toAbove ? sibSize.Height : -sibSize.Height);
                }

                if(centered)
                {
                    y += 0.5f * (toAbove ? size.Height : -size.Height);
                }
            }




            return new PointF(x, y);
        }

        public virtual void Draw(Graphics g, RectangleF parentRect)
        {

        }

        public virtual SizeF GetSize(RectangleF parentRect)
        {
            return new SizeF((float)ScaleX * parentRect.Width, (float)ScaleY * parentRect.Height);
        }
    }

    internal class YaRect : YaObject
    {
        public string Fill { get; set; } = string.Empty;

        public override void Draw(Graphics g, RectangleF parentRect)
        {
            var rec = new Rectangle(0, 0, (int)parentRect.Width, (int)parentRect.Height);

            SolidBrush b = new(ColorTranslator.FromHtml(Fill));

            g.FillRectangle(b, rec);
        }
    }

    internal class YaTemplateObject : YaObject
    {
        public string TemplateName { get; set; } = string.Empty;

        public YaObject? Template { get; set; }

        public override void Draw(Graphics g, RectangleF parentRect)
        {
            if (Template == null)
            {
                throw new("Provide a template before calling draw");
            }

            Template.Width = (int)parentRect.Width;
            Template.Height = (int)parentRect.Height;
            var yd = new YaDrawer(Template);
            yd.Draw();

            g.DrawImage(yd.GetBitmap(), 0, 0);
        }
    }

    internal class YaText : YaObject
    {
        public string Text { get; set; } = string.Empty;

        public override SizeF GetSize(RectangleF parentRect)
        {
            var ff = new Font(SystemFonts.DefaultFont.FontFamily, (float)ScaleY);
            return TextRenderer.MeasureText(Text, ff);
        }

        public override void Draw(Graphics g, RectangleF parentRect)
        {
            GraphicsPath textPath = new();
            var ff = new Font(SystemFonts.DefaultFont.FontFamily, (float)ScaleY);
            textPath.AddString(Text, ff.FontFamily, (int)ff.Style, ff.Size, new Point(0, 0), StringFormat.GenericDefault);

            g.FillPath(Brushes.Black, textPath);
        }
    }
}
