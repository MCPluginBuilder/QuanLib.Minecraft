using QuanLib.Minecraft.Resource.Extensions;
using QuanLib.Minecraft.Resource.Textures;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Models
{
    public class CubeBlockModel : ICubeBlockModel
    {
        public CubeBlockModel(string modelId, IReadOnlyDictionary<string, string> textures, BlockModelElement? element, IObjectModel? parent = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(modelId, nameof(modelId));
            ArgumentNullException.ThrowIfNull(textures, nameof(textures));

            ModelId = modelId;
            Textures = textures;
            Parent = parent;

            _element = element;
        }

        private readonly BlockModelElement? _element;

        public IObjectModel? Parent { get; }

        public string ModelId { get; }

        public IReadOnlyDictionary<string, string> Textures { get; }

        public BlockModelElement? Element => _element ?? (Parent as ICubeBlockModel)?.Element;

        public bool IsCubeAll => Textures.ContainsKey("all");

        public string Down => FindTexture("down");

        public string Up => FindTexture("up");

        public string North => FindTexture("north");

        public string East => FindTexture("east");

        public string South => FindTexture("south");

        public string West => FindTexture("west");

        private string FindTexture(string face)
        {
            BlockModelElement? element = Element;
            if (element is not null)
                face = element.Faces.AllFaces.GetValueOrDefault(face)?.Texture ?? face;

            if (face == BlockFace.MISSINGNO)
                return Texture.MISSING_TEXTURE;

            face = face.TrimStart('#');
            return this.GetTexture(face) ?? Texture.MISSING_TEXTURE;
        }
    }
}
