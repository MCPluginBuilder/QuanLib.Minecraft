using QuanLib.Game;
using QuanLib.Minecraft.Resource.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Extensions
{
    public static class BlockRotateHelper
    {
        public static Facing RotateFacing(Facing facing, BlockRotate blockRotate)
        {
            if (blockRotate.X is not (0 or 90 or 180 or 270) || blockRotate.Y is not (0 or 90 or 180 or 270))
                throw new ArgumentException("Rotation must be a multiple of 90 degrees and between 0 and 270 degrees.", nameof(blockRotate));

            if (facing is Facing.Zp or Facing.Zm or Facing.Xp or Facing.Xm)
            {
                for (int y = blockRotate.Y; y > 0; y -= 90)
                    facing = facing.RightRotate(Facing.Yp);
            }

            if (facing is Facing.Zp or Facing.Zm or Facing.Yp or Facing.Ym)
            {
                for (int x = blockRotate.X; x > 0; x -= 90)
                    facing = facing.RightRotate(Facing.Xp);
            }

            return facing;
        }

        public static string GetTextureAtFacing(this ICubeBlockModel model, Facing facing, BlockRotate blockRotate)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));

            facing = RotateFacing(facing, blockRotate);
            return model.GetTextureAtFacing(facing);
        }
    }
}
