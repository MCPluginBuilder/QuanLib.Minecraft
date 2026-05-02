using QuanLib.Core;
using QuanLib.Minecraft.Resource.Textures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace QuanLib.Minecraft.Resource
{
    public class ItemTextureManager : IReadOnlyDictionary<string, IGeneratedItemTexture>, ISingleton<ItemTextureManager>, ISingletonFactory<ItemTextureManager, ItemTextureManager.InstantiateArgs>
    {
        private ItemTextureManager(IReadOnlyDictionary<string, IGeneratedItemTexture> itemTextures, TexturePool texturePool)
        {
            ArgumentNullException.ThrowIfNull(itemTextures, nameof(itemTextures));
            ArgumentNullException.ThrowIfNull(texturePool, nameof(texturePool));

            _itemTextures = itemTextures;
            TexturePool = texturePool;
        }

        private static readonly Lock _slock = new();
        private readonly IReadOnlyDictionary<string, IGeneratedItemTexture> _itemTextures;

        public static bool IsLoaded { get; private set; }

        public static ItemTextureManager Instance
        {
            get => field ?? throw new InvalidOperationException("Instance not loaded, please call LoadInstance() method to load the instance first");
            private set => field = value;
        }

        public TexturePool TexturePool { get; }

        public IGeneratedItemTexture this[string itemId] => _itemTextures[itemId];

        public IEnumerable<string> Keys => _itemTextures.Keys;

        public IEnumerable<IGeneratedItemTexture> Values => _itemTextures.Values;

        public int Count => _itemTextures.Count;

        public bool ContainsKey(string itemId)
        {
            return _itemTextures.ContainsKey(itemId);
        }

        public bool TryGetValue(string itemId, [MaybeNullWhen(false)] out IGeneratedItemTexture texture)
        {
            return _itemTextures.TryGetValue(itemId, out texture);
        }

        public IEnumerator<KeyValuePair<string, IGeneratedItemTexture>> GetEnumerator()
        {
            return _itemTextures.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_itemTextures).GetEnumerator();
        }

        public static ItemTextureManager LoadInstance(InstantiateArgs args)
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));

            lock (_slock)
            {
                if (IsLoaded)
                    throw new InvalidOperationException("Instance already loaded, please do not call LoadInstance() method again");

                Instance = new ItemTextureManager(args.ItemTextures, args.TexturePool);
                IsLoaded = true;

                return Instance;
            }
        }

        public class InstantiateArgs : Core.InstantiateArgs
        {
            public InstantiateArgs(IReadOnlyDictionary<string, IGeneratedItemTexture> itemTextures, TexturePool texturePool)
            {
                ArgumentNullException.ThrowIfNull(itemTextures, nameof(itemTextures));
                ArgumentNullException.ThrowIfNull(texturePool, nameof(texturePool));

                ItemTextures = itemTextures;
                TexturePool = texturePool;
            }

            public IReadOnlyDictionary<string, IGeneratedItemTexture> ItemTextures { get; }

            public TexturePool TexturePool { get; }
        }
    }
}
