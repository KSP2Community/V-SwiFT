using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace VSwift.Utilities
{
    /// <summary>
    /// <see cref="Dictionary{TKey,TValue}" /> subclass that round-trips through both Unity's native serializer (via <see cref="ISerializationCallbackReceiver" /> and parallel backing lists) and Newtonsoft.Json (via the inherited <see cref="IDictionary{TKey,TValue}" /> interface).
    /// </summary>
    /// <remarks>
    /// Newtonsoft's default contract resolver detects <c>IDictionary</c> on the base type and emits standard <c>{"key": "value"}</c> JSON shape; the private <c>[SerializeField]</c> backing lists are skipped because they're private. Unity, conversely, ignores the Dictionary base and only persists the backing lists. <see cref="OnBeforeSerialize" /> flattens the dictionary into the lists before Unity reads them; <see cref="OnAfterDeserialize" /> rebuilds the dictionary after Unity hydrates them. The backing lists carry <c>[JsonIgnore]</c> as an explicit guard against future contract-resolver changes.
    /// </remarks>
    [Serializable]
    public class SerializedDictionary<TKey, TValue>
        : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField, JsonIgnore] private List<TKey> _keys = new();
        [SerializeField, JsonIgnore] private List<TValue> _values = new();

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();
            foreach (var kvp in this)
            {
                _keys.Add(kvp.Key);
                _values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            int n = Math.Min(_keys.Count, _values.Count);
            for (int i = 0; i < n; i++)
            {
                this[_keys[i]] = _values[i];
            }
        }
    }
}
