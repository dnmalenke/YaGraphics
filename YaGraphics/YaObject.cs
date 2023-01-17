using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Http.Headers;

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

        private static readonly string[] SKIP_WORDS =
        {
            "to",
            "the",
            "of"
        };

        public virtual PointF GetPosition(RectangleF parentRect, Dictionary<string, YaObject> siblings)
        {
            var size = GetSize(parentRect);

            var splX = X.Split(' ');
            var splY = Y.Split(' ');

            _ = float.TryParse(splX[0], out float x);
            _ = float.TryParse(splY[0], out float y);

            if (siblings.Keys.Intersect(SKIP_WORDS).Any())
            {
                Console.WriteLine("Warning: object name is a skip word.");
            }


            for (int i = 1; i < splX.Length; i++)
            {
                if (SKIP_WORDS.Contains(splX[i]))
                {
                    continue;
                }

                switch (splX[i].ToLower())
                {
                    case "left":
                        break;
                    case "right":
                        break;
                    default:
                        break;
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
