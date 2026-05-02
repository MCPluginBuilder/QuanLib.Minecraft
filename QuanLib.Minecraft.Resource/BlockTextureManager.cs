using QuanLib.Core;
using QuanLib.Minecraft.Resource.Textures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace QuanLib.Minecraft.Resource
{
    public class BlockTextureManager : IReadOnlyDictionary<string, ICubeBlockTexture>, ISingleton<BlockTextureManager>, ISingletonFactory<BlockTextureManager, BlockTextureManager.InstantiateArgs>
    {
        private BlockTextureManager(IReadOnlyDictionary<string, ICubeBlockTexture> blockTextures, TexturePool texturePool)
        {
            ArgumentNullException.ThrowIfNull(blockTextures, nameof(blockTextures));
            ArgumentNullException.ThrowIfNull(texturePool, nameof(texturePool));

            _blockTextures = blockTextures;
            TexturePool = texturePool;
        }

        private static readonly Lock _slock = new();
        private readonly IReadOnlyDictionary<string, ICubeBlockTexture> _blockTextures;

        public static bool IsLoaded { get; private set; }

        public static BlockTextureManager Instance
        {
            get => field ?? throw new InvalidOperationException("Instance not loaded, please call LoadInstance() method to load the instance first");
            private set => field = value;
        }

        public TexturePool TexturePool { get; }

        public ICubeBlockTexture this[string blockId] => _blockTextures[blockId];

        public IEnumerable<string> Keys => _blockTextures.Keys;

        public IEnumerable<ICubeBlockTexture> Values => _blockTextures.Values;

        public int Count => _blockTextures.Count;

        public bool ContainsKey(string blockId)
        {
            return _blockTextures.ContainsKey(blockId);
        }

        public bool TryGetValue(string blockId, [MaybeNullWhen(false)] out ICubeBlockTexture texture)
        {
            return _blockTextures.TryGetValue(blockId, out texture);
        }

        public IEnumerator<KeyValuePair<string, ICubeBlockTexture>> GetEnumerator()
        {
            return _blockTextures.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return((IEnumerable)_blockTextures).GetEnumerator();
        }

        public static BlockTextureManager LoadInstance(InstantiateArgs args)
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));

            lock (_slock)
            {
                if (IsLoaded)
                    throw new InvalidOperationException("Instance already loaded, please do not call LoadInstance() method again");

                Instance = new BlockTextureManager(args.BlockTextures, args.TexturePool);
                IsLoaded = true;

                return Instance;
            }
        }

        public class InstantiateArgs : Core.InstantiateArgs
        {
            public InstantiateArgs(IReadOnlyDictionary<string, ICubeBlockTexture> blockTextures, TexturePool texturePool)
            {
                ArgumentNullException.ThrowIfNull(blockTextures, nameof(blockTextures));
                ArgumentNullException.ThrowIfNull(texturePool, nameof(texturePool));

                BlockTextures = blockTextures;
                TexturePool = texturePool;
            }

            public IReadOnlyDictionary<string, ICubeBlockTexture> BlockTextures { get; }

            public TexturePool TexturePool { get; }
        }
    }
}
