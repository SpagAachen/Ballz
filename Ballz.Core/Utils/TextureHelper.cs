using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ballz.Utils
{
    using System.IO;

    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    internal class TextureHelper
    {
        //TODO: monogame does not support SaveAsXXX and mono does not implement gzipstream properly
        //TODO: find a fix for linux
        public static string SaveTextureData(Texture2D texture)
        {
            var streamOut = new MemoryStream();
            var width = texture.Width;
            var height = texture.Height;
            texture.SaveAsPng(streamOut, width, height);
            return Convert.ToBase64String(streamOut.ToArray());
        }

        public static Texture2D LoadTextureData(string pngdata)
        {
            var png = Convert.FromBase64String(pngdata);

            using (var ms = new MemoryStream(png))
            {
                var texture = Texture2D.FromStream(Ballz.The().GraphicsDevice, ms);
                return texture;
            }
        }
    }
}
