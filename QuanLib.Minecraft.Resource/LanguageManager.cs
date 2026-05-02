using QuanLib.Core;
using QuanLib.Minecraft.Resource.Extensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace QuanLib.Minecraft.Resource
{
    public class LanguageManager : IReadOnlyDictionary<string, LanguageTemplate>, ISingleton<LanguageManager>, ISingletonFactory<LanguageManager, LanguageManager.InstantiateArgs>
    {
        public const string DEFAULT_LANGUAGE = "en_us";

        private LanguageManager(InstantiateArgs args)
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));

            _languageTemplates = args.LanguageTemplates;
            Language = args.Language;
        }

        private static readonly Lock _slock = new();
        private readonly Dictionary<string, Dictionary<string, LanguageTemplate>> _languageTemplates;
        private readonly ConcurrentDictionary<string, LanguageTemplate> _missingLanguageTemplates = [];

        public static bool IsLoaded { get; private set; }

        public static LanguageManager Instance
        {
            get => field ?? throw new InvalidOperationException("Instance not loaded, please call LoadInstance() method to load the instance first");
            private set => field = value;
        }

        public string Language
        {
            get => field;
            set
            {
                if (!_languageTemplates.ContainsKey(value))
                    throw new InvalidOperationException("Not found language: " + value);

                field = value;
            }
        }

        public LanguageTemplate this[string key]
        {
            get
            {
                if (_languageTemplates[Language].TryGetValue(key, out var template))
                    return template;

                if (_languageTemplates.TryGetValue(DEFAULT_LANGUAGE, out var defaultTemplates) &&
                    defaultTemplates.TryGetValue(key, out var defaultTemplate))
                    return defaultTemplate;

                return _missingLanguageTemplates.GetOrAdd(key, LanguageTemplate.Parse);
            }
        }

        public IEnumerable<string> Keys => _languageTemplates[Language].Keys;

        public IEnumerable<LanguageTemplate> Values => _languageTemplates[Language].Values;

        public int Count => _languageTemplates[Language].Count;

        public IEnumerable<string> GetLanguages()
        {
            return _languageTemplates.Keys;
        }

        public bool ContainsKey(string key)
        {
            return _languageTemplates[Language].ContainsKey(key) ||
                   (_languageTemplates.TryGetValue(DEFAULT_LANGUAGE, out var defaultTemplates) &&
                    defaultTemplates.ContainsKey(key));
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out LanguageTemplate value)
        {
            if (_languageTemplates[Language].TryGetValue(key, out value))
                return true;

            if (_languageTemplates.TryGetValue(DEFAULT_LANGUAGE, out var defaultTemplates) &&
                defaultTemplates.TryGetValue(key, out value))
                return true;

            value = null;
            return false;
        }

        public IEnumerator<KeyValuePair<string, LanguageTemplate>> GetEnumerator()
        {
            return _languageTemplates[Language].GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_languageTemplates[Language]).GetEnumerator();
        }

        public static LanguageManager LoadInstance(InstantiateArgs args)
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));

            lock (_slock)
            {
                if (IsLoaded)
                    throw new InvalidOperationException("Instance already loaded, please do not call LoadInstance() method again");

                Instance = new LanguageManager(args);
                IsLoaded = true;

                return Instance;
            }
        }

        public class InstantiateArgs : Core.InstantiateArgs
        {
            public InstantiateArgs(Dictionary<string, Dictionary<string, LanguageTemplate>> languageTemplates, string language)
            {
                ArgumentNullException.ThrowIfNull(languageTemplates, nameof(languageTemplates));
                ArgumentNullException.ThrowIfNull(language, nameof(language));

                if (!languageTemplates.ContainsKey(language))
                    throw new ArgumentException("Language list does not contain the specified language: " + language, nameof(language));

                LanguageTemplates = languageTemplates;
                Language = language;
            }

            public Dictionary<string, Dictionary<string, LanguageTemplate>> LanguageTemplates { get; }

            public string Language { get; }
        }
    }
}
