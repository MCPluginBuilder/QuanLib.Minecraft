using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

namespace QuanLib.Minecraft.Resource
{
    public record class AssetFileEntry(string AssetId, string FilePath, ZipArchiveEntry Entry);
}
