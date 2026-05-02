using QuanLib.Minecraft.Resource.Models;
using QuanLib.Minecraft.Resource.Textures;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services
{
    public interface IItemTextureLoader
    {
        public Task<Dictionary<string, IGeneratedItemTexture>> LoadItemTexturesAsync(
            IReadOnlyDictionary<string, AssetFileEntry> entries,
            IReadOnlyDictionary<string, IGeneratedItemModel> models,
            TexturePool texturePool);
    }
}
