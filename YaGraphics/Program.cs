using System.Diagnostics;
using System.Runtime.InteropServices;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Resources.Extensions;
using System.Text;

namespace YaGraphics
{
    internal class Program
    {
        private static Form? _imgForm;
        private static Bitmap? _bmp;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var dBuilder = new DeserializerBuilder()
                .IncludeNonPublicProperties()
                .WithTagMapping("!Text", typeof(YaText))
                .WithTagMapping("!Rectangle", typeof(YaRect))
                .WithTagMapping("!Templated", typeof(YaTemplateObject));

            var deserializer = dBuilder.Build();
            string ya = File.ReadAllText(@"C:\Users\dnmal\source\repos\YaGraphics\YaGraphics\test.yaml");
            FileSystemWatcher fWatch = new(@"C:\Users\dnmal\source\repos\YaGraphics\YaGraphics")
            {
                NotifyFilter = NotifyFilters.Attributes |
                               NotifyFilters.CreationTime |
                               NotifyFilters.DirectoryName |
                               NotifyFilters.FileName |
                               NotifyFilters.LastAccess |
                               NotifyFilters.LastWrite |
                               NotifyFilters.Security |
                               NotifyFilters.Size,
                Filter = "test.yaml"
            };

            var obj = deserializer.Deserialize<YaObject>(ya);

            YaDrawer yd = new(obj);

            yd.Draw();

            fWatch.Changed += (s, e) =>
            {
                FileStream? yaFile = null;
                while (yaFile == null)
                {
                    try
                    {
                        yaFile = File.Open(@"C:\Users\dnmal\source\repos\YaGraphics\YaGraphics\test.yaml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

                    }
                    catch { }

                    Thread.Sleep(1);
                }

                using var sr = new StreamReader(yaFile, Encoding.UTF8);
                ya = sr.ReadToEnd();

                yaFile.Close();

                try
                {
                    obj = deserializer.Deserialize<YaObject>(ya);

                    yd = new(obj, _bmp!, new());

                    yd.Draw();

                    _imgForm!.Invoke(_imgForm.Invalidate);

                    GC.Collect();
                    _imgForm!.Invoke(() => _imgForm.Text = "Success!");
                }
                catch
                {
                    _imgForm!.Invoke(() => _imgForm.Text = "Fail.");
                }
            };

            _bmp = yd.GetBitmap();

            Thread t = new(FormThread);

            t.Start();

            fWatch.EnableRaisingEvents = true;

            Console.ReadLine();
            try
            {
                _imgForm!.Invoke(_imgForm.Close);
            }
            catch { }
        }


        static void FormPainted(object? sender, PaintEventArgs e)
        {
            if (_bmp != null)
            {
                e.Graphics.DrawImage(_bmp, 0, 0);
            }
        }

        [STAThread]
        static void FormThread()
        {
            _imgForm = new()
            {
                TopMost = true
            };

            if (_bmp != null)
            {
                _imgForm.ClientSize = _bmp.Size;
            }


            Application.EnableVisualStyles();

            _imgForm.Paint += FormPainted;
            Application.Run(_imgForm);
        }

    }

}