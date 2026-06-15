using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AYellowpaper.SerializedCollections
{
    internal class DictionaryLookupTable<TKey, TValue> : IKeyable
    {
        private static readonly List<int> EmptyList = new();
        private readonly SerializedDictionary<TKey, TValue> _dictionary;
        private readonly Dictionary<TKey, List<int>> _occurences = new();

        public DictionaryLookupTable(SerializedDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        public IEnumerable Keys => _dictionary.Keys;

        public IReadOnlyList<int> GetOccurences(object key)
        {
            if (key is TKey castKey && _occurences.TryGetValue(castKey, out var list))
                return list;

            return EmptyList;
        }

        public IReadOnlyList<int> GetValueOccurences(object value)
        {
            var indices = new List<int>();
            for (var i = 0; i < _dictionary._serializedList.Count; i++)
            {
                var kvp = _dictionary._serializedList[i];
                if (EqualityComparer<TValue>.Default.Equals(kvp.Value, (TValue)value)) indices.Add(i);
            }

            return indices;
        }

        public void RecalculateOccurences()
        {
            _occurences.Clear();

            var count = _dictionary._serializedList.Count;
            for (var i = 0; i < count; i++)
            {
                var kvp = _dictionary._serializedList[i];
                if (!SerializedCollectionsUtility.IsValidKey(kvp.Key))
                    continue;

                if (!_occurences.ContainsKey(kvp.Key))
                    _occurences.Add(kvp.Key, new List<int> { i });
                else
                    _occurences[kvp.Key].Add(i);
            }
        }

        public void RemoveKey(object key)
        {
            for (var i = _dictionary._serializedList.Count - 1; i >= 0; i--)
            {
                var dictKey = _dictionary._serializedList[i].Key;
                if (SerializedCollectionsUtility.KeysAreEqual(dictKey, key))
                    _dictionary._serializedList.RemoveAt(i);
            }
        }

        public void RemoveAt(int index)
        {
            _dictionary._serializedList.RemoveAt(index);
        }

        public object GetKeyAt(int index)
        {
            return _dictionary._serializedList[index];
        }

        public int GetCount()
        {
            return _dictionary._serializedList.Count;
        }

        public void RemoveDuplicates()
        {
            _dictionary._serializedList = _dictionary._serializedList
                .GroupBy(x => x.Key)
                .Where(x => SerializedCollectionsUtility.IsValidKey(x.Key))
                .Select(x => x.First()).ToList();
        }

        public void AddKey(object key)
        {
            var entry = new SerializedKeyValuePair<TKey, TValue>();
            entry.Key = (TKey)key;
            _dictionary._serializedList.Add(entry);
        }
    }
}