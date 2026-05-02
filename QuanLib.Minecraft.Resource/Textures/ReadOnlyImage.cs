using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Textures
{
    public class ReadOnlyImage<TPixel> : IReadOnlyImage<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        public ReadOnlyImage(Image<TPixel> image)
        {
            ArgumentNullException.ThrowIfNull(image, nameof(image));

            _image = image;
        }

        private readonly Image<TPixel> _image;

        public int Width => _image.Width;

        public int Height => _image.Height;

        public Size Size => _image.Size;

        public Rectangle Bounds => _image.Bounds;

        public PixelTypeInfo PixelType => _image.PixelType;

        public ImageMetadata Metadata => _image.Metadata;

        public TPixel this[int x, int y] => _image[x, y];

        public void ProcessPixelRows(PixelAccessorAction<TPixel> processPixels)
        {
            _image.ProcessPixelRows(processPixels);
        }

        public void CopyPixelDataTo(Span<TPixel> destination)
        {
            _image.CopyPixelDataTo(destination);
        }

        public void CopyPixelDataTo(Span<byte> destination)
        {
            _image.CopyPixelDataTo(destination);
        }

        public bool DangerousTryGetSinglePixelMemory(out Memory<TPixel> memory)
        {
            return _image.DangerousTryGetSinglePixelMemory(out memory);
        }

        public Image<TPixel> Clone()
        {
            return _image.Clone();
        }

        public Image<TPixel> Clone(Configuration configuration)
        {
            return _image.Clone(configuration);
        }

        public Image<TPixel2> CloneAs<TPixel2>() where TPixel2 : unmanaged, IPixel<TPixel2>
        {
            return _image.CloneAs<TPixel2>();
        }

        public Image<TPixel2> CloneAs<TPixel2>(Configuration configuration) where TPixel2 : unmanaged, IPixel<TPixel2>
        {
            return _image.CloneAs<TPixel2>(configuration);
        }

        public void Save(Stream stream, IImageEncoder encoder)
        {
            _image.Save(stream, encoder);
        }

        public Task SaveAsync(Stream stream, IImageEncoder encoder, CancellationToken cancellationToken = default)
        {
            return _image.SaveAsync(stream, encoder, cancellationToken);
        }
    }
}
