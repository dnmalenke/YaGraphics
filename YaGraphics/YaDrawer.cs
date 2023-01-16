using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace YaGraphics
{
    internal class YaDrawer
    {
        private YaObject _base;
        private Graphics _graphics;
        private Bitmap _bitmap;

        private Dictionary<string, YaObject> _templates = new();


        public YaDrawer(YaObject baseObj)
        {
            _base = baseObj;
            _bitmap = new(_base.Width, _base.Height);
            _graphics = Graphics.FromImage(_bitmap);
            _graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

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
            _graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

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
            foreach (var obj in _base.Children)
            {
                var pos = obj.GetPosition(parentRect);
                var size = obj.GetSize(parentRect);
                var b = new Bitmap((int)size.Width, (int)size.Height);

                YaDrawer drawer = new(obj, b, _templates);
                drawer.Draw();

                _graphics.DrawImage(b, new Point((int)pos.X, (int)pos.Y));

            }
        }

        public Bitmap GetBitmap() { return _bitmap; }
    }
}
