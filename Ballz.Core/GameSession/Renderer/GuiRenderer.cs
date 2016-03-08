using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Ballz.GameSession.Renderer
{
    public class GuiRenderer
    {
        WebBrowser Browser;
        Thread BrowserThread;

        Bitmap LatestBitmap = null;

        public void Start()
        {
            BrowserThread = new Thread(() =>
            {
                Browser = new WebBrowser();
                Browser.Width = Ballz.The().GraphicsDevice.Viewport.Width;
                Browser.Height = Ballz.The().GraphicsDevice.Viewport.Height;
                Browser.ScrollBarsEnabled = false;
                //Browser.IsWebBrowserContextMenuEnabled = false;
                LatestBitmap = new Bitmap(Browser.Width, Browser.Height);
                Browser.Validated += (s, e) =>
                {
                    lock (this)
                    {
                        Browser.DrawToBitmap(LatestBitmap, new Rectangle(0, 0, Browser.Width, Browser.Height));
                    }
                };

                Browser.DocumentCompleted += (s, e) =>
                {
                    lock (this)
                    {
                        Browser.DrawToBitmap(LatestBitmap, new Rectangle(0, 0, Browser.Width, Browser.Height));
                    }
                };

                Browser.Navigate("file://C:/Users/Lukas/Documents/gui.html");

                var context = new ApplicationContext();
                Application.Run();
            });

            BrowserThread.SetApartmentState(ApartmentState.STA);
            BrowserThread.Start();
        }

        public Bitmap ToBitmap()
        {
            lock(this)
            {
                return LatestBitmap;
            }
        }

        Texture2D Texture;
        public Texture2D ToTexture()
        {
            if (Texture == null)
                Texture = new Texture2D(Ballz.The().GraphicsDevice, Browser.Width, Browser.Height, false, SurfaceFormat.Bgra32);

            var bitmap = ToBitmap();
            if (bitmap != null)
            {
                var bitmapdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                byte[] data = new byte[bitmap.Width * bitmap.Height * 4];
                Marshal.Copy(bitmapdata.Scan0, data, 0, data.Length);
                bitmap.UnlockBits(bitmapdata);
                
                byte[] fixeddata = new byte[bitmap.Width * bitmap.Height * 4];
                
                for(int i = 0; i < data.Length; i += 4)
                {
                    bool transparent = data[i + 0] == 255 && data[i + 1] == 0 && data[i + 2] == 255;
                    fixeddata[i + 0] = data[i + 0];
                    fixeddata[i + 1] = data[i + 1];
                    fixeddata[i + 2] = data[i + 2];
                    fixeddata[i + 3] = transparent ? (byte)0 : (byte)255;
                }
                
                Texture.SetData(fixeddata);

                return Texture;
            }
            else
                return null;
        }
    }
}
