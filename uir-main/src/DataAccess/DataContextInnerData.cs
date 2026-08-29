using System;
using System.Collections;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.Core;

namespace Mephi.Cybernetics.Race.DataAccess
{
    public class DataContextInnerData : IDictionary<string, object>
    {
        private Dictionary<string,object> _data = new Dictionary<string, object>();
        private readonly IDictionary<string, object> _parentContentData;

        internal DataContextInnerData(IDictionary<string, object> parentContentData)
        {
            if (parentContentData != null)
                _parentContentData = parentContentData;
            else
                _parentContentData = new Dictionary<string, object>();
        }

        #region Implementation of IEnumerable

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new SwitchableEnumerator<KeyValuePair<string, object>>(_parentContentData, _data);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<KeyValuePair<string,object>>

        public void Add(KeyValuePair<string, object> item)
        {
            (_data as ICollection<KeyValuePair<string,object>>).Add(item);
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return (_data as ICollection<KeyValuePair<string, object>>).Contains(item) || _parentContentData.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _parentContentData.CopyTo(array, arrayIndex);
            (_data as ICollection<KeyValuePair<string, object>>).CopyTo(array, arrayIndex + _parentContentData.Count);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            if (_parentContentData.Contains(item))
                throw new Exception("Cannot delete an item from parent context");
            return (_data as ICollection<KeyValuePair<string, object>>).Remove(item);
        }

        public int Count
        {
            get { return _parentContentData.Count + _data.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region Implementation of IDictionary<string,object>

        public bool ContainsKey(string key)
        {
            return _data.ContainsKey(key) || _parentContentData.ContainsKey(key);
        }

        public void Add(string key, object value)
        {
            _data.Add(key, value);
        }

        public bool Remove(string key)
        {
            return _data.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _data.TryGetValue(key, out value) || _parentContentData.TryGetValue(key, out value);
        }

        public object this[string key]
        {
            get
            {
                if (_data.ContainsKey(key))
                    return _data[key];
                else
                    return _parentContentData[key];
            }
            set
            {
                _data[key] = value;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                var parentKeys =
                    _parentContentData is Dictionary<string, object> ?
                        new List<string>(_parentContentData.Keys) :
                        _parentContentData.Keys;
                foreach ( var key in _data.Keys )
                    parentKeys.Add(key);
                return parentKeys;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                var parentValues =
                    _parentContentData is Dictionary<string, object> ?
                        new List<object>(_parentContentData.Values) :
                        _parentContentData.Values;
                foreach (var value in _data.Values)
                    parentValues.Add(value);
                return parentValues;
            }
        }

        #endregion
    }
}