using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using YamlDotNet.Serialization;

namespace YaGraphics.Core
{
    public class YaObject
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

        ///// <summary>
        ///// Rotation anchor X position
        ///// </summary>
        //public float AnchorX { get; set; }

        ///// <summary>
        ///// Rotation anchor Y position
        ///// </summary>
        //public float AnchorY { get; set; }

        /// <summary>
        /// X scaling of object. 0.5 means 50% as wide as the parent
        /// </summary>
        public decimal ScaleX { get; set; }

        /// <summary>
        /// Y scaling of object. 0.5 means 50% as tall as the parent
        /// </summary>
        public decimal ScaleY { get; set; }

        /// <summary>
        /// Fill color/format of the object
        /// </summary>
        public string Fill { get; set; } = string.Empty;

        public string Outline { get; set; } = string.Empty;
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
                    x += 0.5f * (toLeft ? size.Width : -size.Width);
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
                    y += 0.5f * (toAbove ? sibSize.Height : -sibSize.Height);
                }

                if (centered)
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
            var size = new SizeF((float)ScaleX * parentRect.Width, (float)ScaleY * parentRect.Height);
            return size;
        }

        public virtual Brush GetFillBrush()
        {
            if (Fill.StartsWith("rgb("))
            {
                var rgb = Fill.Trim()[4..^1].Split(',').Select(c => int.Parse(c.Trim())).ToList();

                return new SolidBrush(Color.FromArgb(rgb[0], rgb[1], rgb[2]));
            }

            if (Fill.StartsWith("rgba("))
            {
                var rgb = Fill.Trim()[5..^1].Split(',').Select(c => decimal.Parse(c.Trim())).ToList();

                return new SolidBrush(Color.FromArgb((int)(255 * rgb[3]), (int)rgb[0], (int)rgb[1], (int)rgb[2]));
            }

            Color fillColor = Color.Transparent;

            try
            {
                fillColor = ColorTranslator.FromHtml(Fill);
            }
            catch { }


            return new SolidBrush(fillColor);
        }

        public virtual Pen GetOutlinePen()
        {
            string origFill = Fill;

            var penSizeStr = Outline.Split(' ').FirstOrDefault(s => s.EndsWith("px") && float.TryParse(s[..^2], out _));
            if (penSizeStr != null)
            {
                Fill = Outline.Replace(penSizeStr, "");
            }
            else
            {
                penSizeStr = "1px";
            }

            var brush = GetFillBrush();

            var size = float.Parse(penSizeStr[..^2]);

            Fill = origFill;
            var p = new Pen(brush, size);
            p.Alignment = PenAlignment.Inset;
            return p;
        }
    }

    public class YaDocument : YaObject
    {
        /// <summary>
        /// Width of object in pixels. Used for the main object and template objects
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Height of object in pixels. Used for the main object and template objects
        /// </summary>
        public int Height { get; set; }

        public string OutputFile { get; set; } = string.Empty;
    }

    public class YaImg : YaObject
    {
        public override void Draw(Graphics g, RectangleF parentRect)
        {
            using var img = Image.FromFile(Fill);
            g.DrawImage(img, parentRect);

            if (!string.IsNullOrEmpty(Outline))
            {
                g.DrawRectangle(GetOutlinePen(), parentRect);
            }
        }
    }

    public class YaRect : YaObject
    {
        public override void Draw(Graphics g, RectangleF parentRect)
        {
            var rec = new Rectangle(0, 0, (int)parentRect.Width, (int)parentRect.Height);

            using Brush b = GetFillBrush();

            g.FillRectangle(b, rec);

            if (!string.IsNullOrEmpty(Outline))
            {
                g.DrawRectangle(GetOutlinePen(), parentRect);
            }
        }
    }

    public class YaTemplateObject : YaObject
    {
        public string TemplateName { get; set; } = string.Empty;

        public YaObject? Template { get; set; }

        public override void Draw(Graphics g, RectangleF parentRect)
        {
            if (Template == null)
            {
                throw new("Provide a template before calling draw");
            }

            var yd = new YaDrawer(Template, (int)parentRect.Width, (int)parentRect.Height);
            yd.Draw();

            g.DrawImage(yd.GetBitmap(), 0, 0);
        }
    }

    public class YaLine : YaObject
    {
        public float X0 { get; set; }
        public float Y0 { get; set; }
        public float X1 { get; set; }
        public float Y1 { get; set; }

        public override SizeF GetSize(RectangleF parentRect)
        {
            return new SizeF(parentRect.Width, parentRect.Height);
        }

        public override void Draw(Graphics g, RectangleF parentRect)
        {
            var size = GetSize(parentRect);

            if (string.IsNullOrEmpty(Outline))
            {
                Outline = Fill;
            }

            g.DrawLine(GetOutlinePen(), new PointF(X0 * size.Width, Y0 * size.Height), new PointF(X1 * size.Width, Y1 * size.Height));
        }
    }

    public class YaCircle : YaObject
    {
        public override void Draw(Graphics g, RectangleF parentRect)
        {
            using var b = GetFillBrush();
            g.FillEllipse(b, parentRect);

            if (!string.IsNullOrEmpty(Outline))
            {
                g.DrawEllipse(GetOutlinePen(), parentRect);
            }
        }
    }

    public class YaText : YaObject
    {
        public string Text { get; set; } = string.Empty;
        public string? Alignment { get; set; }
        public string? LineAlignment { get; set; }

        public static StringAlignment GetStringAlignment(string? alignment)
        {
            if (alignment != null)
            {
                switch (alignment.ToLower())
                {
                    case "left":
                    case "top":
                        return StringAlignment.Near;
                    case "right":
                    case "bottom":
                        return StringAlignment.Far;
                    case "center":
                    case "middle":
                        return StringAlignment.Center;
                }
            }

            return StringAlignment.Near;
        }

        public Font GetFont()
        {
            return new Font(SystemFonts.DefaultFont.FontFamily, (float)ScaleY);
        }

        public StringFormat GetFormat()
        {
            var format = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.NoClip);
            format.Alignment = GetStringAlignment(Alignment);
            format.LineAlignment = GetStringAlignment(LineAlignment);

            return format;
        }

        public override SizeF GetSize(RectangleF parentRect)
        {
            return new SizeF(parentRect.Width, parentRect.Height);
        }

        public RectangleF GetBounds(RectangleF parentRect)
        {
            GraphicsPath textPath = new();
            var ff = GetFont();

            using var format = GetFormat();

            textPath.AddString(Text, ff.FontFamily, (int)ff.Style, ff.Size, parentRect, format);

            return textPath.GetBounds(null, Pens.Black);
        }

        public override void Draw(Graphics g, RectangleF parentRect)
        {
            GraphicsPath textPath = new();
            var ff = GetFont();

            using var format = GetFormat();

            textPath.AddString(Text, ff.FontFamily, (int)ff.Style, ff.Size, parentRect, format);
            using var b = GetFillBrush();

            if (b is SolidBrush sb && sb.Color.ToArgb() == 0)
            {
                sb.Color = Color.Black;
            }

            g.FillPath(b, textPath);

            if (!string.IsNullOrEmpty(Outline))
            {
                g.DrawPath(GetOutlinePen(), textPath);
            }
        }
    }
}
