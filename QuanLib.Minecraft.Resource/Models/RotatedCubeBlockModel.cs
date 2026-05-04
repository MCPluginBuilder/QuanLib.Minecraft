using QuanLib.Game;
using QuanLib.Minecraft.Resource.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Models
{
    public class RotatedCubeBlockModel : IRotatedCubeBlockModel
    {
        public RotatedCubeBlockModel(ICubeBlockModel owner, BlockRotationMapping blockRotationMapping)
        {
            ArgumentNullException.ThrowIfNull(owner, nameof(owner));

            _owner = owner;
            BlockRotationMapping = blockRotationMapping;
        }

        private readonly ICubeBlockModel _owner;

        public BlockRotationMapping BlockRotationMapping { get; }

        public IObjectModel? Parent => _owner.Parent;

        public string ModelId => _owner.ModelId;

        public IReadOnlyDictionary<string, string> Textures => _owner.Textures;

        public bool IsCubeAll => _owner.IsCubeAll;

        public string Down => _owner.GetTextureAtFacing(BlockRotationMapping.YmFacing.TargetFacing);

        public string Up => _owner.GetTextureAtFacing(BlockRotationMapping.YpFacing.TargetFacing);

        public string North => _owner.GetTextureAtFacing(BlockRotationMapping.ZmFacing.TargetFacing);

        public string East => _owner.GetTextureAtFacing(BlockRotationMapping.XpFacing.TargetFacing);

        public string South => _owner.GetTextureAtFacing(BlockRotationMapping.ZpFacing.TargetFacing);

        public string West => _owner.GetTextureAtFacing(BlockRotationMapping.XmFacing.TargetFacing);
    }
}
