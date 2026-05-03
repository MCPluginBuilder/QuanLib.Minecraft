using QuanLib.Game;
using QuanLib.Minecraft.Resource.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Models
{
    public class RotatedCubeBlockModel : ICubeBlockModel
    {
        public RotatedCubeBlockModel(ICubeBlockModel owner, BlockRotation blockRotation)
        {
            ArgumentNullException.ThrowIfNull(owner, nameof(owner));

            _owner = owner;
            BlockRotation = blockRotation;
        }

        private readonly ICubeBlockModel _owner;

        public BlockRotation BlockRotation { get; }

        public IObjectModel? Parent => _owner.Parent;

        public string ModelId => _owner.ModelId;

        public IReadOnlyDictionary<string, string> Textures => _owner.Textures;

        public bool IsCubeAll => _owner.IsCubeAll;

        public string Down => _owner.GetTextureAtFacing(Facing.Ym, BlockRotation);

        public string Up => _owner.GetTextureAtFacing(Facing.Yp, BlockRotation);

        public string North => _owner.GetTextureAtFacing(Facing.Zm, BlockRotation);

        public string East => _owner.GetTextureAtFacing(Facing.Xp, BlockRotation);

        public string South => _owner.GetTextureAtFacing(Facing.Zp, BlockRotation);

        public string West => _owner.GetTextureAtFacing(Facing.Xm, BlockRotation);
    }
}
