using Newtonsoft.Json;
using QuanLib.IO.Zip;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

namespace QuanLib.Minecraft.Resource.Services.Implementations
{
    public class ResourceEntryLoader : IResourceEntryLoader
    {
        public ResourceEntryManager LoadResourceEntry(ZipPack[] zipPacks)
        {
            ArgumentNullException.ThrowIfNull(zipPacks, nameof(zipPacks));

            Dictionary<string, ResourcePath> paths = [];
            Dictionary<string, Dictionary<string, AssetFileEntry>> allAssetEntries = [];
            Dictionary<string, Dictionary<string, string>> languageEntries = [];

            foreach (ZipPack zipPack in zipPacks)
            {
                if (!zipPack.DirectoryExists("assets"))
                    continue;

                string[] folders = zipPack.GetDirectoryPaths("assets");
                foreach (string folder in folders)
                {
                    string modId = Path.GetFileName(folder);

                    if (!paths.TryGetValue(modId, out var path))
                    {
                        path = new ResourcePath(modId);
                        paths.Add(modId, path);
                    }

                    if (!allAssetEntries.TryGetValue(modId, out var assetEntries))
                    {
                        assetEntries = [];
                        allAssetEntries.Add(modId, assetEntries);
                    }

                    LoadAssetEntries(path.BlockStates, modId + ":block/{0}", zipPack, assetEntries);
                    LoadAssetEntries(path.ItemMapping, modId + ":item/{0}", zipPack, assetEntries);
                    LoadAssetEntries(path.BlockModels, modId + ":block/{0}", zipPack, assetEntries);
                    LoadAssetEntries(path.ItemModels, modId + ":item/{0}", zipPack, assetEntries);
                    LoadAssetEntries(path.BlockTextures, modId + ":block/{0}", zipPack, assetEntries);
                    LoadAssetEntries(path.ItemTextures, modId + ":item/{0}", zipPack, assetEntries);

                    LoadLanguageEntries(path.Languages, zipPack, languageEntries);
                }
            }

            Dictionary<string, ResourceEntry> resourceEntries = [];
            foreach (var item in allAssetEntries)
            {
                string modId = item.Key;
                Dictionary<string, AssetFileEntry> assetEntries = item.Value;

                ResourceEntry resourceEntry = new(modId, assetEntries.Values);
                if (resourceEntry.IsEmpty)
                    continue;

                resourceEntries.Add(modId, resourceEntry);
            }

            return new ResourceEntryManager(zipPacks, resourceEntries, languageEntries);
        }

        private static void LoadAssetEntries(string folderPath, string assetIdFormat, ZipPack zipPack, Dictionary<string, AssetFileEntry> assetEntries)
        {
            if (!zipPack.DirectoryExists(folderPath))
                return;

            string[] filePaths = zipPack.GetFilePaths(folderPath);
            foreach (string filePath in filePaths)
            {
                string assetName = Path.GetFileNameWithoutExtension(filePath);
                string assetId = string.Format(assetIdFormat, assetName);

                ZipArchiveEntry zipArchiveEntry = zipPack.ReadZipItem(filePath);
                AssetFileEntry assetFileEntry = new(assetId, filePath, zipArchiveEntry);

                assetEntries[filePath] = assetFileEntry;
            }
        }

        private static void LoadLanguageEntries(string folderPath, ZipPack zipPack, Dictionary<string, Dictionary<string, string>> languageEntries)
        {
            if (!zipPack.DirectoryExists(folderPath))
                return;

            string[] filePaths = zipPack.GetFilePaths(folderPath);
            foreach (string filePath in filePaths)
            {
                string language = Path.GetFileNameWithoutExtension(filePath);
                ZipArchiveEntry zipArchiveEntry = zipPack.ReadZipItem(filePath);
                string json = zipArchiveEntry.ReadAllText(Encoding.UTF8);
                Dictionary<string, string> languages = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? [];

                if (languageEntries.TryGetValue(language, out var entries))
                {
                    foreach (var item in languages)
                        entries[item.Key] = item.Value;
                }
                else
                {
                    languageEntries[language] = languages;
                }
            }
        }
    }
}
