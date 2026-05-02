using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Textures
{
    public interface IReadOnlyImage<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        public int Width { get; }

        public int Height { get; }

        public Size Size { get; }

        public Rectangle Bounds { get; }

        public PixelTypeInfo PixelType { get; }

        public ImageMetadata Metadata { get; }

        public TPixel this[int x, int y] { get; }

        public void ProcessPixelRows(PixelAccessorAction<TPixel> processPixels);

        public void CopyPixelDataTo(Span<TPixel> destination);

        public void CopyPixelDataTo(Span<byte> destination);

        public bool DangerousTryGetSinglePixelMemory(out Memory<TPixel> memory);

        public Image<TPixel> Clone();

        public Image<TPixel> Clone(Configuration configuration);

        public Image<TPixel2> CloneAs<TPixel2>() where TPixel2 : unmanaged, IPixel<TPixel2>;

        public Image<TPixel2> CloneAs<TPixel2>(Configuration configuration) where TPixel2 : unmanaged, IPixel<TPixel2>;

        public void Save(Stream stream, IImageEncoder encoder);

        public Task SaveAsync(Stream stream, IImageEncoder encoder, CancellationToken cancellationToken = default);
    }
}
