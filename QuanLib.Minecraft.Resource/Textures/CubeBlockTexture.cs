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
            int textureRotation = _model.GetTextureRotation(facing);
            string rotatedTextureId = textureRotation == 0 ? textureId : $"{textureId}[rotation={textureRotation}]";

            if (_texturePool.TryGetValue(rotatedTextureId, out var texture))
                return texture;

            return _texturePool[Texture.MISSING_TEXTURE];
        }
    }
}
