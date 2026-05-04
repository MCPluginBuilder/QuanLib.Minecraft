using QuanLib.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace QuanLib.Minecraft.Resource.Models
{
    public struct BlockRotationMapping
    {
        public BlockRotationMapping()
        {
            YpFacing = new(Facing.Yp, Facing.Yp, 0);
            YmFacing = new(Facing.Ym, Facing.Ym, 0);
            XpFacing = new(Facing.Xp, Facing.Xp, 0);
            XmFacing = new(Facing.Xm, Facing.Xm, 0);
            ZpFacing = new(Facing.Zp, Facing.Zp, 0);
            ZmFacing = new(Facing.Zm, Facing.Zm, 0);
        }

        public static readonly BlockRotationMapping Zero = new();

        public readonly bool IsZero => YpFacing.IsZero && YmFacing.IsZero && XpFacing.IsZero && XmFacing.IsZero && ZpFacing.IsZero && ZmFacing.IsZero;

        public RotationMapping YpFacing;

        public RotationMapping YmFacing;

        public RotationMapping XpFacing;

        public RotationMapping XmFacing;

        public RotationMapping ZpFacing;

        public RotationMapping ZmFacing;

        public readonly BlockRotationMapping RotateFromXaxis()
        {
            BlockRotationMapping result = this;

            result.XpFacing.TextureRotation += 90;
            result.XpFacing.TextureRotation %= 360;

            result.XmFacing.TextureRotation += 270;
            result.XmFacing.TextureRotation %= 360;

            result.YpFacing = ZpFacing.Clone(Facing.Yp);
            result.ZmFacing = YpFacing.Clone(Facing.Zm);
            result.YmFacing = ZmFacing.Clone(Facing.Ym);
            result.ZpFacing = YmFacing.Clone(Facing.Zp);

            result.ZmFacing.TextureRotation += 180;
            result.ZmFacing.TextureRotation %= 360;

            result.YmFacing.TextureRotation += 180;
            result.YmFacing.TextureRotation %= 360;

            return result;
        }

        public readonly BlockRotationMapping RotateFromYaxis()
        {
            BlockRotationMapping result = this;

            result.YpFacing.TextureRotation += 90;
            result.YpFacing.TextureRotation %= 360;

            result.YmFacing.TextureRotation += 270;
            result.YmFacing.TextureRotation %= 360;

            result.ZpFacing = XpFacing.Clone(Facing.Zp);
            result.XmFacing = ZpFacing.Clone(Facing.Xm);
            result.ZmFacing = XmFacing.Clone(Facing.Zm);
            result.XpFacing = ZmFacing.Clone(Facing.Xp);

            return result;
        }

        public readonly RotationMapping GetRotationMapping(Facing facing)
        {
            return facing switch
            {
                Facing.Yp => YpFacing,
                Facing.Ym => YmFacing,
                Facing.Xp => XpFacing,
                Facing.Xm => XmFacing,
                Facing.Zp => ZpFacing,
                Facing.Zm => ZmFacing,
                _ => throw new InvalidEnumArgumentException(nameof(facing), (int)facing, typeof(Facing))
            };
        }

        public static BlockRotationMapping CreateMapping(BlockRotation blockRotation)
        {
            if (blockRotation.X is not (0 or 90 or 180 or 270) || blockRotation.Y is not (0 or 90 or 180 or 270))
                throw new ArgumentException("Rotation must be a multiple of 90 degrees and between 0 and 270 degrees.", nameof(blockRotation));

            BlockRotationMapping result = Zero;
            if (blockRotation.IsZero)
                return result;

            for (int x = blockRotation.X; x > 0; x -= 90)
                result = result.RotateFromXaxis();

            for (int y = blockRotation.Y; y > 0; y -= 90)
                result = result.RotateFromYaxis();

            return result;
        }
    }
}
