using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace VSwift.Utilities
{
    /// <summary>
    /// One key+value record inside a <see cref="SerializedDictionary{TKey,TValue}" />'s Unity-serialized backing list.
    /// </summary>
    /// <remarks>
    /// Public fields named <c>Key</c> and <c>Value</c> match SerializedArrayTable's column-by-PropertyName lookup convention, so a dictionary can be rendered as a two-column grid without any bespoke scaffold.
    /// </remarks>
    [Serializable]
    public struct SerializedDictionaryEntry<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
    }

    /// <summary>
    /// <see cref="Dictionary{TKey,TValue}" /> subclass that round-trips through both Unity's native serializer (via <see cref="ISerializationCallbackReceiver" /> and a backing list of <see cref="SerializedDictionaryEntry{TKey,TValue}" />) and Newtonsoft.Json (via the inherited <see cref="IDictionary{TKey,TValue}" /> interface).
    /// </summary>
    /// <remarks>
    /// Newtonsoft's default contract resolver detects <c>IDictionary</c> on the base type and emits standard <c>{"key": "value"}</c> JSON shape; the private <c>[SerializeField]</c> backing list is skipped because it's private. Unity, conversely, ignores the Dictionary base and only persists the backing list. <see cref="OnBeforeSerialize" /> flushes the dictionary into the list before Unity reads it; <see cref="OnAfterDeserialize" /> rebuilds the dictionary after Unity hydrates it. The backing list carries <c>[JsonIgnore]</c> as an explicit guard against future contract-resolver changes.
    /// </remarks>
    [Serializable]
    public class SerializedDictionary<TKey, TValue>
        : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField, JsonIgnore] private List<SerializedDictionaryEntry<TKey, TValue>> _entries = new();

        public void OnBeforeSerialize()
        {
            _entries.Clear();
            foreach (var kvp in this)
            {
                _entries.Add(new SerializedDictionaryEntry<TKey, TValue>
                {
                    Key = kvp.Key,
                    Value = kvp.Value,
                });
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            for (int i = 0; i < _entries.Count; i++)
            {
                this[_entries[i].Key] = _entries[i].Value;
            }
        }
    }
}
