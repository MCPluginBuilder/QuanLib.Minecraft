using QuanLib.Minecraft.Resource.Models;
using QuanLib.Minecraft.Resource.Textures;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services
{
    public interface IBlockTextureLoader
    {
        public Task<Dictionary<string, ICubeBlockTexture>> LoadBlockTexturesAsync(
            IReadOnlyDictionary<string, AssetFileEntry> entries,
            IReadOnlyDictionary<string, ICubeBlockModel> models,
            TexturePool texturePool);
    }
}
