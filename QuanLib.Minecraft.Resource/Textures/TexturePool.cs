using QuanLib.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace QuanLib.Minecraft.Resource.Textures
{
    public class TexturePool : UnmanagedBase, IReadOnlyDictionary<string, Texture>
    {
        internal Dictionary<string, Texture> Textures { get; } = [];

        public Texture this[string textureId] => Textures[textureId];

        public int Count => Textures.Count;

        public IEnumerable<string> Keys => Textures.Keys;

        public IEnumerable<Texture> Values => Textures.Values;

        protected override void DisposeUnmanaged()
        {
            foreach (Texture texture in Textures.Values)
                texture.Dispose();
        }

        public bool ContainsKey(string textureId)
        {
            return Textures.ContainsKey(textureId);
        }

        public bool TryGetValue(string textureId, [MaybeNullWhen(false)] out Texture texture)
        {
            return Textures.TryGetValue(textureId, out texture);
        }

        public IEnumerator<KeyValuePair<string, Texture>> GetEnumerator()
        {
            return Textures.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Textures).GetEnumerator();
        }
    }
}
