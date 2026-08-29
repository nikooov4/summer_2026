using System;
using System.Collections;
using System.Collections.Generic;

namespace Mephi.Cybernetics.Race.Core
{
    public class SwitchableEnumerator<T> : IEnumerator<T>
    {
        private List<IEnumerable<T>> _collections;
        private int _currentEnumIndex;
        private List<IEnumerator<T>> _enums;

        public SwitchableEnumerator(IEnumerable<IEnumerable<T>> collections)
        {
            Init(collections);
        }


        public SwitchableEnumerator(params IEnumerable<T>[] collections) 
        {
            Init(collections);
        }

        private void Init(IEnumerable<IEnumerable<T>> collections)
        {
            _collections = new List<IEnumerable<T>>(collections);
            if (_collections.FindAll(element => element == null).Count > 0)
                throw new ArgumentException("Collection cannot be null");
            _enums = _collections.ConvertAll(collection => collection.GetEnumerator());
            _currentEnumIndex = 0;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _collections.Clear();
        }

        #endregion

        #region Implementation of IEnumerator

        public bool MoveNext()
        {
            var moved = false;
            while (_currentEnumIndex < _enums.Count && !(moved = _enums[_currentEnumIndex].MoveNext()))
                _currentEnumIndex++;
            return moved;
        }

        public void Reset()
        {
            _currentEnumIndex = 0;
        }

        public T Current
        {
            get { return _enums[_currentEnumIndex].Current; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion
    }
}