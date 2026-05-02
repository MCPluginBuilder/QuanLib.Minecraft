using QuanLib.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Textures
{
    public class Texture : UnmanagedBase
    {
        public const string MISSING_TEXTURE = "minecraft:builtin/missing";

        public Texture(string textureId, Image<Rgba32> image)
        {
            ArgumentException.ThrowIfNullOrEmpty(textureId, nameof(textureId));
            ArgumentNullException.ThrowIfNull(image, nameof(image));

            _image = image;
            Image = new ReadOnlyImage<Rgba32>(image);

            TextureId = textureId;
        }

        private readonly Image<Rgba32> _image;
        private Rgba32? _averageColor;

        public string TextureId { get; }

        public ReadOnlyImage<Rgba32> Image { get; }

        public Rgba32 AverageColor
        {
            get
            {
                if (_averageColor.HasValue)
                    return _averageColor.Value;

                _averageColor = GetAverageColor(_image);
                return _averageColor.Value;
            }
        }

        public static Texture CreateMissingTexture()
        {
            int imageSize = 16;
            int blockSize = imageSize / 2;
            Rgba32 black = Rgba32.ParseHex("#000000");
            Rgba32 magenta = Rgba32.ParseHex("#F800F8");
            Image<Rgba32> image = new(imageSize, imageSize, magenta);

            for (int y = 0; y < blockSize; y++)
            {
                for (int x = 0; x < blockSize; x++)
                    image[x, y] = black;
            }

            for (int y = blockSize; y < imageSize; y++)
            {
                for (int x = blockSize; x < imageSize; x++)
                    image[x, y] = black;
            }

            return new Texture(MISSING_TEXTURE, image);
        }

        private static Rgba32 GetAverageColor(Image<Rgba32> image)
        {
            int r = 0, g = 0, b = 0, a = 0;

            if (image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> memory))
            {
                ReadOnlySpan<Rgba32> span = memory.Span;
                foreach (ref readonly Rgba32 pixel in span)
                {
                    r += pixel.R;
                    g += pixel.G;
                    b += pixel.B;
                    a += pixel.A;
                }
            }
            else
            {
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        r += image[x, y].R;
                        g += image[x, y].G;
                        b += image[x, y].B;
                        a += image[x, y].A;
                    }
                }
            }
 
            int size = image.Width * image.Height;
            r /= size;
            g /= size;
            b /= size;
            a /= size;

            return new Rgba32((byte)r, (byte)g, (byte)b, (byte)a);
        }

        protected override void DisposeUnmanaged()
        {
            _image.Dispose();
        }
    }
}
