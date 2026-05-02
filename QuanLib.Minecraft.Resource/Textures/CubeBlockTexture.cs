using QuanLib.Game;
using QuanLib.Minecraft.Resource.Extensions;
using QuanLib.Minecraft.Resource.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Textures
{
    public class CubeBlockTexture : ICubeBlockTexture
    {
        public CubeBlockTexture(string blockId, ICubeBlockModel model, TexturePool texturePool)
        {
            ArgumentException.ThrowIfNullOrEmpty(blockId, nameof(blockId));
            ArgumentNullException.ThrowIfNull(model, nameof(model));
            ArgumentNullException.ThrowIfNull(texturePool, nameof(texturePool));

            BlockId = blockId;

            _model = model;
            _texturePool = texturePool;
        }

        private readonly ICubeBlockModel _model;
        private readonly TexturePool _texturePool;

        public string BlockId { get; }

        public bool IsCubeAll => _model.IsCubeAll;

        public Texture GetTexture(Facing facing)
        {
            string textureId = AssetHelper.GetFullName(_model.GetTextureAtFacing(facing));
            if (_texturePool.TryGetValue(textureId, out var texture))
                return texture;
            else
                return _texturePool[Texture.MISSING_TEXTURE];
        }
    }
}
