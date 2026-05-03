using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource
{
    public record struct BlockRotation(int X, int Y)
    {
        public static readonly BlockRotation Zero = new(0, 0);

        public readonly bool IsZero => X == 0 && Y == 0;
    }
}
