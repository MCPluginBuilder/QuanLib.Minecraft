using Microsoft.Extensions.Logging;
using QuanLib.Core;
using QuanLib.IO.Zip;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services.Implementations
{
    public class BlockStateLoader : IBlockStateLoader
    {
        public BlockStateLoader(IBlockStateParser parser, ILogger<BlockStateLoader>? logger = null)
        {
            ArgumentNullException.ThrowIfNull(parser, nameof(parser));

            _parser = parser;
            _logger = logger;
        }

        private readonly IBlockStateParser _parser;
        private readonly ILogger<BlockStateLoader>? _logger;

        public async Task<Dictionary<string, BlockStateModel>> LoadBlockStatesAsync(IEnumerable<AssetFileEntry> entries)
        {
            ArgumentNullException.ThrowIfNull(entries, nameof(entries));

            Dictionary<string, BlockStateModel> result = [];
            foreach (AssetFileEntry entry in entries)
            {
                BlockStateModel[] blockStates;
                try
                {
                    string json = await entry.Entry.ReadAllTextAsync(Encoding.UTF8);
                    blockStates = _parser.ParseBlockState(json);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning("Loading block state from '{FilePath}' failed: {Message}", entry.FilePath, ObjectFormatter.Format(ex));
                    continue;
                }

                if (blockStates.Length == 0)
                    continue;

                string[] assetIdParts = entry.AssetId.Split(':', 2);
                if (assetIdParts.Length != 2)
                    throw new FormatException($"Invalid asset ID format: {entry.AssetId}");

                foreach (BlockStateModel blockStateModel in blockStates)
                {
                    StringBuilder blockIdBuilder = new();
                    blockIdBuilder.Append(assetIdParts[0]);
                    blockIdBuilder.Append(':');
                    blockIdBuilder.Append(Path.GetFileNameWithoutExtension(entry.FilePath));

                    if (!string.IsNullOrEmpty(blockStateModel.BlockState))
                        blockIdBuilder.AppendFormat("[{0}]", blockStateModel.BlockState);

                    result[blockIdBuilder.ToString()] = blockStateModel;
                }
            }

            return result;
        }
    }
}
