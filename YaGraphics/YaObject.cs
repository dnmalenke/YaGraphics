using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace YaGraphics
{
    internal class YaObject
    {
        public string Id { get; set; } = string.Empty;
        protected string X { get; set; } = string.Empty;
        protected string Y { get; set; } = string.Empty;
        public string ReferenceX { get; set; } = string.Empty;
        public string ReferenceY { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public decimal ScaleX { get; set; }
        public decimal ScaleY { get; set; }
        public List<YaObject> Templates { get; set; } = new();
        public List<YaObject> Children { get; set; } = new();

        public virtual PointF GetPosition(RectangleF parentRect)
        {
            var size = GetSize(parentRect);
            float x = float.Parse(X) * parentRect.Width - (string.IsNullOrEmpty(ReferenceX) ? 0 : float.Parse(ReferenceX) * size.Width);
            float y = float.Parse(Y) * parentRect.Height - (string.IsNullOrEmpty(ReferenceX) ? 0 : float.Parse(ReferenceX) * size.Height);

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
