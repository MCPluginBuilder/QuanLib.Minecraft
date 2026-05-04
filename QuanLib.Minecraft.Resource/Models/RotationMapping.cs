using QuanLib.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Models
{
    public struct RotationMapping(Facing sourceFacing, Facing targetFacing, int textureRotation)
    {
        public readonly Facing SourceFacing = sourceFacing;

        public Facing TargetFacing = targetFacing;

        public int TextureRotation = textureRotation;

        public readonly bool IsZero => SourceFacing == TargetFacing && TextureRotation == 0;

        public readonly RotationMapping Clone(Facing sourceFacing)
        {
            return new RotationMapping(sourceFacing, TargetFacing, TextureRotation);
        }

        public override readonly string ToString()
        {
            return $"[SourceFacing={SourceFacing}, TargetFacing={TargetFacing}, TextureRotation={TextureRotation}]";
        }
    }
}
