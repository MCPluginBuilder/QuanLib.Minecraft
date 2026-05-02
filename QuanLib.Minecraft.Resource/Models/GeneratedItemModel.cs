using QuanLib.Minecraft.Resource.Extensions;
using QuanLib.Minecraft.Resource.Textures;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Models
{
    public class GeneratedItemModel : IGeneratedItemModel
    {
        public GeneratedItemModel(string modelId, IReadOnlyDictionary<string, string> textures, IObjectModel? parent = null)
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

        public string Layer0 => this.GetTexture("layer0") ?? Texture.MISSING_TEXTURE;

        public string? Layer1 => this.GetTexture("layer1");

        public string? Layer2 => Layer1 is null ? null : this.GetTexture("layer2");

        public string? Layer3 => Layer2 is null ? null : this.GetTexture("layer3");

        public string? Layer4 => Layer3 is null ? null : this.GetTexture("layer4");

        public string? Layer5 => Layer4 is null ? null : this.GetTexture("layer5");

        public IEnumerable<string> GetLayers()
        {
            yield return Layer0;

            if (Layer1 is not null)
                yield return Layer1;
            else
                yield break;

            if (Layer2 is not null)
                yield return Layer2;
            else
                yield break;

            if (Layer3 is not null)
                yield return Layer3;
            else
                yield break;

            if (Layer4 is not null)
                yield return Layer4;
            else
                yield break;

            if (Layer5 is not null)
                yield return Layer5;
            else
                yield break;
        }
    }
}
