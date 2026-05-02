using QuanLib.Core;
using QuanLib.IO.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Security.AccessControl;
using System.Text;

namespace QuanLib.Minecraft.Resource
{
    public class ResourceEntryManager : UnmanagedBase, IReadOnlyDictionary<string, ResourceEntry>
    {
        public ResourceEntryManager(
            ZipPack[] zipPacks,
            Dictionary<string, ResourceEntry> resourceEntries,
            Dictionary<string, Dictionary<string, string>> languageEntries)
        {
            ArgumentNullException.ThrowIfNull(zipPacks, nameof(zipPacks));
            ArgumentNullException.ThrowIfNull(resourceEntries, nameof(resourceEntries));
            ArgumentNullException.ThrowIfNull(languageEntries, nameof(languageEntries));

            _zipPacks = zipPacks;
            _entries = resourceEntries;
            LanguageEntries = languageEntries.ToDictionary(item => item.Key, item => item.Value.AsReadOnly()).AsReadOnly();
        }

        private readonly ZipPack[] _zipPacks;
        private readonly Dictionary<string, ResourceEntry> _entries;

        public ResourceEntry this[string modId] => _entries[modId];

        public int Count => _entries.Count;

        public IEnumerable<string> Keys => _entries.Keys;

        public IEnumerable<ResourceEntry> Values => _entries.Values;

        public ReadOnlyDictionary<string, ReadOnlyDictionary<string, string>> LanguageEntries { get; }

        public bool ContainsKey(string modId)
        {
            return _entries.ContainsKey(modId);
        }

        public bool TryGetValue(string modId, [MaybeNullWhen(false)] out ResourceEntry entry)
        {
            return _entries.TryGetValue(modId, out entry);
        }

        public IEnumerator<KeyValuePair<string, ResourceEntry>> GetEnumerator()
        {
            return _entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_entries).GetEnumerator();
        }

        protected override void DisposeUnmanaged()
        {
            foreach (ZipPack zipPack in _zipPacks)
                zipPack.Dispose();
        }
    }
}
