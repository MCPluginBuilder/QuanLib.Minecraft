using QuanLib.Minecraft.Resource.Extensions;
using QuanLib.Minecraft.Resource.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Textures
{
    public class GeneratedItemTexture : IGeneratedItemTexture
    {
        public GeneratedItemTexture(string itemId, IGeneratedItemModel model, TexturePool texturePool)
        {
            ArgumentException.ThrowIfNullOrEmpty(itemId, nameof(itemId));
            ArgumentNullException.ThrowIfNull(model, nameof(model));
            ArgumentNullException.ThrowIfNull(texturePool, nameof(texturePool));

            _model = model;
            _texturePool = texturePool;

            ItemId = itemId;
        }

        private readonly IGeneratedItemModel _model;
        private readonly TexturePool _texturePool;

        public string ItemId { get; }

        public Texture GetTexture()
        {
            string textureId = _model.Layer1 is null ? AssetHelper.GetFullName(_model.Layer0) : string.Join('+', _model.GetLayers().Select(AssetHelper.GetFullName));
            if (_texturePool.TryGetValue(textureId, out var texture))
                return texture;
            else
                return _texturePool[Texture.MISSING_TEXTURE];
        }
    }
}
