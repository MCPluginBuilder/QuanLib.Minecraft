using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource
{
    public record struct BlockRotate(int X, int Y)
    {
        public static readonly BlockRotate Zero = new(0, 0);

        public readonly bool IsZero => X == 0 && Y == 0;
    }
}
