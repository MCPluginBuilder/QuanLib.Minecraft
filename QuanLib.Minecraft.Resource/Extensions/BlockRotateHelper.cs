using QuanLib.Game;
using QuanLib.Minecraft.Resource.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Extensions
{
    public static class BlockRotateHelper
    {
        public static Facing RotateFacing(Facing facing, BlockRotation blockRotation)
        {
            if (blockRotation.X is not (0 or 90 or 180 or 270) || blockRotation.Y is not (0 or 90 or 180 or 270))
                throw new ArgumentException("Rotation must be a multiple of 90 degrees and between 0 and 270 degrees.", nameof(blockRotation));

            if (facing is Facing.Zp or Facing.Zm or Facing.Xp or Facing.Xm)
            {
                for (int y = blockRotation.Y; y > 0; y -= 90)
                    facing = facing.RightRotate(Facing.Yp);
            }

            if (facing is Facing.Zp or Facing.Zm or Facing.Yp or Facing.Ym)
            {
                for (int x = blockRotation.X; x > 0; x -= 90)
                    facing = facing.RightRotate(Facing.Xp);
            }

            return facing;
        }

        public static string GetTextureAtFacing(this ICubeBlockModel model, Facing facing, BlockRotation blockRotation)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));

            facing = RotateFacing(facing, blockRotation);
            return model.GetTextureAtFacing(facing);
        }
    }
}
