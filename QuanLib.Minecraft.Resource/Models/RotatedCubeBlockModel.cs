using QuanLib.Game;
using QuanLib.Minecraft.Resource.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Models
{
    public class RotatedCubeBlockModel : ICubeBlockModel
    {
        public RotatedCubeBlockModel(ICubeBlockModel owner, BlockRotate blockRotate)
        {
            ArgumentNullException.ThrowIfNull(owner, nameof(owner));

            _owner = owner;
            BlockRotate = blockRotate;
        }

        private readonly ICubeBlockModel _owner;

        public BlockRotate BlockRotate { get; }

        public IObjectModel? Parent => _owner.Parent;

        public string ModelId => _owner.ModelId;

        public IReadOnlyDictionary<string, string> Textures => _owner.Textures;

        public bool IsCubeAll => _owner.IsCubeAll;

        public string Down => _owner.GetTextureAtFacing(Facing.Ym, BlockRotate);

        public string Up => _owner.GetTextureAtFacing(Facing.Yp, BlockRotate);

        public string North => _owner.GetTextureAtFacing(Facing.Zm, BlockRotate);

        public string East => _owner.GetTextureAtFacing(Facing.Xp, BlockRotate);

        public string South => _owner.GetTextureAtFacing(Facing.Zp, BlockRotate);

        public string West => _owner.GetTextureAtFacing(Facing.Xm, BlockRotate);
    }
}
