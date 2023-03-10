using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace YaGraphics.Core
{
    public class YaDrawer
    {
        private YaObject _base;
        private Graphics _graphics;
        private Bitmap _bitmap;

        private Dictionary<string, YaObject> _templates = new();
        private Dictionary<string, YaObject> _siblings = new();

        public YaDrawer(YaObject baseObj, int width, int height)
        {
            _base = baseObj;
            _bitmap = new(width, height);
            _graphics = Graphics.FromImage(_bitmap);
            _graphics.SmoothingMode = SmoothingMode.HighQuality;

            foreach (var item in _base.Templates)
            {
                _templates.Add(item.Id, item);
            }
        }

        public YaDrawer(YaObject baseObj, Bitmap b, Dictionary<string, YaObject> templates)
        {
            _base = baseObj;
            _bitmap = b;
            _graphics = Graphics.FromImage(_bitmap);
            _graphics.SmoothingMode = SmoothingMode.HighQuality;

            foreach (var item in _base.Templates)
            {
                _templates.Add(item.Id, item);
            }

            foreach (var item in templates)
            {
                _templates.Add(item.Key, item.Value);
            }
        }

        public void Draw()
        {
            var parentRect = new RectangleF(0, 0, _bitmap.Width, _bitmap.Height);

            if (_base is YaTemplateObject tO)
            {
                tO.Template = _templates[tO.TemplateName];
            }

            _base.Draw(_graphics, parentRect);

            if (_base is YaText yt)
            {
                parentRect = yt.GetBounds(parentRect);
            }

            foreach (var obj in _base.Children)
            {
                if (!string.IsNullOrEmpty(obj.Id))
                {
                    _siblings.Add(obj.Id, obj);
                }

                var pos = obj.GetPosition(parentRect, _siblings);

                var size = obj.GetSize(parentRect);

                var b = new Bitmap((int)size.Width, (int)size.Height);

                YaDrawer drawer = new(obj, b, _templates);
                drawer.Draw();

                _graphics.DrawImage(b, new Point((int)(pos.X + parentRect.X), (int)(pos.Y + parentRect.Y)));
            }
        }

        public Bitmap GetBitmap() { return _bitmap; }
    }
}
