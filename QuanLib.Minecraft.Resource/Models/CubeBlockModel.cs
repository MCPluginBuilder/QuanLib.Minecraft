using QuanLib.Minecraft.Resource.Extensions;
using QuanLib.Minecraft.Resource.Textures;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Models
{
    public class CubeBlockModel : ICubeBlockModel
    {
        public CubeBlockModel(string modelId, IReadOnlyDictionary<string, string> textures, IObjectModel? parent = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(modelId, nameof(modelId));
            ArgumentNullException.ThrowIfNull(textures, nameof(textures));

            ModelId = modelId;
            Textures = textures;
            Parent = parent;
        }

        public IObjectModel? Parent { get; }

        public string ModelId { get; }

        public IReadOnlyDictionary<string, string> Textures { get; }

        public bool IsCubeAll => Textures.ContainsKey("all");

        public string Down => this.GetTexture("down") ?? Texture.MISSING_TEXTURE;

        public string Up => this.GetTexture("up") ?? Texture.MISSING_TEXTURE;

        public string North => this.GetTexture("north") ?? Texture.MISSING_TEXTURE;

        public string East => this.GetTexture("east") ?? Texture.MISSING_TEXTURE;

        public string South => this.GetTexture("south") ?? Texture.MISSING_TEXTURE;

        public string West => this.GetTexture("west") ?? Texture.MISSING_TEXTURE;
    }
}
