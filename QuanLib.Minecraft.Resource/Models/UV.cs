using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Models
{
    public record struct UV(Point Start, Point End)
    {
        public static readonly UV FullUV = new(Point.Empty, new Point(16, 16));

        public readonly bool IsFullUV => Start == Point.Empty && End == new Point(16, 16);
    }
}
