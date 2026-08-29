using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Collections.Specialized;
using System.Data;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess
{
    [Serializable]
    public class EntityCollection : ICollection, IList, IEquatable<EntityCollection>
    {
        private ArrayList _list = null;

        public EntityCollection()
        {
            _list = new ArrayList();
        }

        public EntityCollection(int capacity)
        {
            _list = new ArrayList(capacity);
        }

        public EntityCollection(ICollection c)
        {
            _list = new ArrayList(c);
        }

        public EntityBase this[int index]
        {
            get
            {
                if ( _list.Count >= index )
                    return null;
                else
                    return (EntityBase)_list[index];
            }
        }

        public int Add(EntityBase entity)
        {
            return _list.Add(entity);
        }

        public void AddRange(ICollection collection)
        {
            _list.AddRange(collection);
        }

        public void Remove(int index)
        {
            _list.Remove(index);
        }

        public void Remove(EntityBase entity)
        {
            _list.Remove(entity);
        }

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            _list.CopyTo(array, index);
            //throw new Exception("The method or operation is not implemented.");
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsSynchronized
        {
            get
            {
                return _list.IsSynchronized;
                //throw new Exception("The method or operation is not implemented.");
            }
        }

        public object SyncRoot
        {
            get
            {
                return _list.SyncRoot;
                //throw new Exception("The method or operation is not implemented.");
            }
        }

        #endregion

        #region IEnumerable Members

        public class EntityCollectionEnumerator : IEnumerator
        {
            private EntityCollection _col;
            private int _index = 0;

            internal EntityCollectionEnumerator(EntityCollection c)
            {
                _col = c;
            }

            public object Current
            {
                get { return _col[_index]; }
            }

            public bool MoveNext()
            {
                return (++_index < _col.Count);
            }

            public void Reset()
            {
                _index = 0;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new EntityCollectionEnumerator(this);
        }

        #endregion

        #region IList Members

        public int Add(object value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Clear()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Contains(object value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int IndexOf(object value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Insert(int index, object value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool IsFixedSize
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool IsReadOnly
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public void Remove(object value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RemoveAt(int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        object IList.this[int index]
        {
            get { throw new Exception("The method or operation is not implemented."); }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion

        #region IEquatable<EntityCollection> Members

        public bool Equals(EntityCollection other)
        {
            bool r = this.Count == other.Count;
            if ( r )
                foreach ( EntityBase e in this )
                {
                    r &= other.Contains(e);
                    if ( !r )
                        break;
                }
            return r;
        }

        #endregion
    }

    public class EntityCollectionItemEventArgs : EventArgs
    {
        private object _item;

        public EntityCollectionItemEventArgs(object item)
        {
            _item = item;
        }

        public object Item
        {
            get { return _item; }
        }
    }

    public delegate void ItemAddedHnd(object sender, EntityCollectionItemEventArgs e);

    public delegate void ItemRemovedHnd(object sender, EntityCollectionItemEventArgs e);

    public delegate void CollectionClearedHnd(object sender, EventArgs e);

    [Serializable]
    public class EntityCollection<T> : IList<T>, ICollection<T>, ICollection, IList, IEquatable<EntityCollection<T>> where T : IEntity
    {
        [Serializable]
        public class EntityCollectionEqualityComparer<T> : IEqualityComparer<EntityCollection<T>> where T : IEntity
        {

            #region IEqualityComparer<EntityCollection<T>> Members

            public bool Equals(EntityCollection<T> x, EntityCollection<T> y)
            {
                if ( x.Count == 0 && y.Count == 0 )
                    return true;
                else if ( x.Count != y.Count )
                    return false;
                bool ok = true;
                for ( int i = 0; i < x.Count; i++ )
                {
                    EntityBase e1 = x[i] as EntityBase;
                    ok &= y.Contains(e1);
                }
                return ok;
            }

            public int GetHashCode(EntityCollection<T> obj)
            {
                int num1 = 7;
                int num2 = 13;
                int counter = num1;
                List<int> hashes = new List<int>();
                foreach ( T item in obj )
                {
                    hashes.Add(item.GetHashCode());
                }
                hashes.Sort();
                foreach ( int hash in hashes )
                {
                    counter += counter * num2 + hash;
                }
                return counter;
            }

            #endregion
        }

        [Serializable]
        public class EntityCollectionComparer<T> : IComparer<EntityCollection<T>> where T : IEntity
        {
            #region IComparer<EntityCollection<T>> Members

            public int Compare(EntityCollection<T> x, EntityCollection<T> y)
            {
                if ( x.Count == 0 && y.Count == 0 )
                    return 0;
                else if ( x.Count != y.Count )
                    return x.Count - y.Count;
                bool ok = true;
                for ( int i = 0; i < x.Count; i++ )
                {
                    EntityBase e1 = x[i] as EntityBase;
                    ok &= y.Contains(e1);
                }
                if ( ok )
                    return 0;
                else
                    return 1;
                //throw new Exception("Comparison failed.");
            }

            #endregion
        }

        public event ItemAddedHnd ItemAdded;
        public event ItemRemovedHnd ItemRemoved;
        public event CollectionClearedHnd CollectionCleared;

        private readonly Dictionary<object, T> _pk;
        private readonly List<T> _list;
        //private DataTable _dt = new DataTable();
        private readonly bool _isReadOnly = false;
        private bool _inConstruction = true;
        private readonly EntityBase _owner = null;
        private readonly EntityAttributeDescription _property = null;

        private string[] _indexableFieldsNames;
        private IndexSet<T> _indexes = null;

        public EntityCollection()
        {
            Init();
            _pk = new Dictionary<object, T>();
            _list = new List<T>();
            _inConstruction = false;
        }

        public EntityCollection(EntityBase owner, EntityAttributeDescription property)
        {
            Init();
            _pk = new Dictionary<object, T>();
            _list = new List<T>();
            _inConstruction = false;
            _owner = owner;
            _property = property;
        }

        public EntityCollection(int capacity)
        {
            Init();
            _pk = new Dictionary<object, T>(capacity);
            _list = new List<T>(capacity);
            _inConstruction = false;
        }

        public EntityCollection(IEnumerable enumeration)
        {
            Init();
            _list = new List<T>(); //(enumeration);
            _pk = new Dictionary<object, T>(); //(enumeration.Count);
            foreach ( T o in enumeration )
            {
                Add(o);
            }
            _inConstruction = false;
        }

        public EntityCollection(IEnumerable<T> enumeration)
        {
            Init();
            _list = new List<T>(); //(enumeration);
            _pk = new Dictionary<object, T>(); //(enumeration.Count);
            foreach ( T o in enumeration )
            {
                Add(o);
            }
            _inConstruction = false;
        }

        //public EntityCollection(ICollection c)
        //{
        //    Init();
        //    _list = new List<T>(c.Count);
        //    _pk = new Dictionary<object,T>(c.Count);
        //    foreach ( T o in c )
        //    {
        //        AddProperty(o);
        //    }
        //    _inConstruction = false;
        //}

        //public EntityCollection(Array a)
        //{
        //    Init();
        //    _list = new List<T>(a.Length);
        //    _pk = new Dictionary<object,T>(a.Length);
        //    foreach ( T o in a )
        //    {
        //        AddProperty(o);
        //    }
        //    _inConstruction = false;
        //}

        public EntityCollection(bool isReadOnly)
        {
            Init();
            _isReadOnly = isReadOnly;
            _list = new List<T>();
            _pk = new Dictionary<object, T>();
            _inConstruction = false;
        }

        public EntityCollection(int capacity, bool isReadOnly)
        {
            Init();
            _isReadOnly = isReadOnly;
            _pk = new Dictionary<object, T>(capacity);
            _list = new List<T>(capacity);
            _inConstruction = false;
        }

        public EntityCollection(ICollection c, bool isReadOnly)
        {
            Init();
            _isReadOnly = isReadOnly;
            _list = new List<T>(c.Count);
            _pk = new Dictionary<object, T>(c.Count);
            foreach ( object o in c )
                Add((T)o);
            _inConstruction = false;
        }

        public EntityCollection(Array a, bool isReadOnly)
        {
            Init();
            _isReadOnly = isReadOnly;
            _list = new List<T>(a.Length);
            _pk = new Dictionary<object, T>(a.Length);
            foreach ( object o in a )
                Add((T)o);
            _inConstruction = false;
        }

        private void SetOwnerState(EntityState state)
        {
            if ( _owner != null )
                if ( state == EntityState.Changed )
                {
                    if ( _owner.State == EntityState.Unchanged )
                        _owner.SetState(EntityState.Changed);
                }
        }

        private void Init()
        {
            if ( !CheckParameterType() )
                throw new Exception("This type is not allowed to be contained in this collection." +
                                    " Only IEntity- or/and IEntityBase-derived classes are allowed");
            //Mephi.Cybernetics.Race.DataAccess.EntityFactory.GetObjectDescription(typeof(T), null); 


            //DataColumn colID = new DataColumn("ID", typeof(object));
            //DataColumn colObj = new DataColumn("Obj", typeof(IEntity));
            //DataColumn colState = new DataColumn("State", typeof(EntityState));

            //_dt = new DataTable();
            //DataTable.Columns.AddRange(new DataColumn[] { colID, colState, colObj });
            //DataTable.PrimaryKey = new DataColumn[] {colID};
        }


        //public T this[int index]
        //{
        //    get
        //    {
        //        if ( DataTable.Rows.Count <= index )
        //            return default(T);
        //        else
        //            return (T)DataTable.Rows[index]["Obj"];
        //    }
        //}

        public string[] IndexableFieldsNames
        {
            get { return _indexableFieldsNames; }
            set
            {
                if ( _indexableFieldsNames == value )
                    return;
                _indexableFieldsNames = value;
                if ( _indexes != null )
                    _indexes.Clear();
                if ( _indexableFieldsNames == null || _indexableFieldsNames.Length == 0 )
                {
                    _indexes = null;
                    return;
                }
                _indexes = new IndexSet<T>(_indexableFieldsNames);
                foreach ( T e in _list )
                    _indexes.Add(e);
            }
        }

        public IndexSet<T> Indexes
        {
            get { return _indexes; }
        }

        public T this[object id]
        {
            get
            {
                if ( Pk.ContainsKey(id) )
                    return Pk[id];
                    //return Pk["Obj"];
                else
                    return default(T);
            }
            set
            {
                if ( !Pk.ContainsKey(id) )
                    throw new Exception("There is no such key");
                Pk[id] = value;
            }
        }

        public void RebuildIndexes()
        {
            string[] ind = IndexableFieldsNames;
            IndexableFieldsNames = null;
            IndexableFieldsNames = ind;
        }

        private Dictionary<object, T> Pk
        {
            get { return _pk; }
        }

        private List<T> List
        {
            get { return _list; }
        }

        private bool CheckParameterType()
        {
            return EntityDescription.IsEntity(typeof(T));
        }

        #region ICollection<T> Members

        public void Add(T item)
        {
            if ( _isReadOnly )
                throw new Exception("This collection have been created as read-only");

            if ( List.Contains(item) || Pk.ContainsKey((item as IEntity).ID) )
                throw new Exception("This item is already contained within the collection");
            List.Add(item);
            Pk.Add((item as IEntity).ID, item);
            if ( !_inConstruction )
                if ( ItemAdded != null )
                    ItemAdded(this, new EntityCollectionItemEventArgs(item));
            SetOwnerState(EntityState.Changed);
            if ( _owner != null )
                if ( _property.IsOneToManyRelation )
                {
                    var newEntityDescription = EntityDescription.GetEntityDescription(item.GetType());
                    foreach ( var pd in newEntityDescription.EntityAttributes.Relationships.OneToOne )
                    {
                        if ( !pd.IsMapped )
                            continue;
                        if ( pd.RelationInfo.RelatedColumn == (_property.Domain as EntityDescription).SystemId.Name &&
                             pd.Range.IsSubtypeOf(_owner.GetType()) )
                        {
                            pd.SetValue(item, _owner);
                            break;
                        }
                    }
                }
            if ( _indexes != null )
                _indexes.Add(item);
        }

        public void AddRange(ICollection<T> col)
        {
            foreach ( T item in col )
                Add(item);
        }

        public void AddRange(IList<T> col)
        {
            foreach ( T item in col )
                Add(item);
        }

        public void Clear()
        {
            if ( _isReadOnly )
                throw new Exception("This collection have been created as read-only");
            List.Clear();
            Pk.Clear();
            SetOwnerState(EntityState.Changed);
            if ( CollectionCleared != null )
                CollectionCleared(this, new EventArgs());
        }

        public bool Contains(T item)
        {
            return List.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            List.CopyTo(array, arrayIndex);
        }

        public void CopyTo(T[] array)
        {
            List.CopyTo(array);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            List.CopyTo(index, array, arrayIndex, count);
        }

        public int Count
        {
            get { return List.Count; }
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        /// <summary>
        /// Removes passed item from the collection, if the item is contained within it.
        /// If collection is read-only, an exception is thrown
        /// </summary>
        /// <param name="item">the item to remove</param>
        /// <returns>true, if the item were contained within the collection and deletion
        /// was successfull, otherwise false. If collection is read-only, an exception is thrown</returns>
        public bool Remove(T item)
        {
            if ( _isReadOnly )
                throw new Exception("This collection have been created as read-only");
            if ( !List.Contains(item) )
                return false;
            Pk.Remove((item as IEntity).ID);
            List.Remove(item);
            if ( ItemRemoved != null )
                ItemRemoved(this, new EntityCollectionItemEventArgs(item));
            SetOwnerState(EntityState.Changed);
            if ( _owner != null )
                if ( _property.IsOneToManyRelation )
                {
                    var newEntityDescription = EntityDescription.GetEntityDescription(item.GetType());
                    foreach ( var pd in newEntityDescription.EntityAttributes.Relationships.OneToOne )
                    {
                        if ( !pd.IsMapped )
                            continue;
                        if ( pd.RelationInfo.RelatedColumn == (_property.Domain as EntityDescription).SystemId.Name &&
                             pd.Range.IsSubtypeOf(_owner.GetType()) )
                        {
                            pd.SetValue(item, null);
                            break;
                        }
                    }
                }
            if ( _indexes != null )
                _indexes.Delete(item);
            return true;
        }

        public int RemoveWhere(string fieldName, object value)
        {
            Predicate<T> where = element => element[fieldName] == value;
            var count = 0;
            foreach ( T element in Select(where) )
            {
                Remove(element);
                count++;
            }
            return count;
        }

        public EntityCollection<T> Select(Predicate<T> where)
        {
            var l = new EntityCollection<T>();
            foreach ( T element in this )
                if ( where(element) )
                    l.Add(element);
            return l;
        }

        /// <summary>
        /// Selects a set of element that satisfy the provided condition; elements are converted
        /// to the supplied type by simple type casting
        /// </summary>
        /// <typeparam name="T2">The type that the retrieval elements are to be converted to</typeparam>
        /// <param name="where">Selection condition</param>
        /// <returns>Sub-collection of the original collection with those items that satisfy the condition 
        ///</returns>
        public EntityCollection<T2> Select<T2>(Predicate<T> where) where T2 : EntityBase
        {
            Converter<T, T2> idConverter = delegate(T element)
                                               { return (T2)((object)element); };
            return Select<T2>(where, idConverter);
        }

        /// <summary>
        /// Selects a set of element that satisfy the provided condition; elements are converted
        /// to the supplied type using the converter 'mapper'
        /// </summary>
        /// <typeparam name="T2">The type that the retrieval elements are to be converted to</typeparam>
        /// <param name="where">Selection condition</param>
        /// <param name="mapper">A mapping that does a custom transformation T->T2</param>
        /// <returns>Sub-collection of the original collection with those items that satisfy the condition. Same as 'Map mapper (where this)'</returns>
        public EntityCollection<T2> Select<T2>(Predicate<T> where, Converter<T, T2> mapper) where T2 : EntityBase
        {
            EntityCollection<T2> l = new EntityCollection<T2>();
            foreach ( T element in this )
                if ( where(element) )
                    l.Add(mapper(element));
            return l;
        }

        public T SelectOne(Predicate<T> where)
        {
            foreach ( T element in this )
                if ( where(element) )
                    return element;
            return default(T);
        }

        /// <summary>
        /// Selects the first item that satisfy the provided condition; elements are converted
        /// to the supplied type by simple type casting
        /// </summary>
        /// <typeparam name="T2">The type that the retrieval elements are to be converted to</typeparam>
        /// <param name="where">Selection condition</param>
        /// <returns>The first item of the original collection that satisfies the condition 
        ///</returns>
        public T2 SelectOne<T2>(Predicate<T> where) where T2 : EntityBase
        {
            Converter<T, T2> idConverter = delegate(T element)
                                               { return (T2)((object)element); };
            return SelectOne<T2>(where, idConverter);
        }

        /// <summary>
        /// Selects a set of element that satisfy the provided condition; elements are converted
        /// to the supplied type using the converter 'mapper'
        /// </summary>
        /// <typeparam name="T2">The type that the retrieval elements are to be converted to</typeparam>
        /// <param name="where">Selection condition</param>
        /// <param name="mapper">A mapping that does a custom transformation T->T2</param>
        /// <returns>Sub-collection of the original collection with those items that satisfy the condition. Same as 'mapper (first (where this))'</returns>
        public T2 SelectOne<T2>(Predicate<T> where, Converter<T, T2> mapper) where T2 : EntityBase
        {
            foreach ( T element in this )
                if ( where(element) )
                    return mapper(element);
            return default(T2);
        }

        public int IndexOf(T item)
        {
            if ( !List.Contains(item) )
                return -1;
            else
                return List.IndexOf(item);
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return new EntityCollectionEnumerator<T>(this);
        }

        [Serializable]
        public class EntityCollectionEnumerator<T> : IEnumerator<T> where T : IEntity
        {
            private EntityCollection<T> _collection;
            private int _index;

            internal EntityCollectionEnumerator(EntityCollection<T> c)
            {
                _collection = c;
                if ( _collection.Count != 0 )
                    _index = -1;
                else
                    _index = -2;
            }

            public int Index
            {
                get { return _index; }
                set { _index = value; }
            }

            #region IEnumerator<T> Members

            public T Current
            {
                get { return GetCurrent(); }
            }

            private T GetCurrent()
            {
                if ( Index < -1 )
                    return default(T);
                return _collection[Index];
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                //throw new Exception("The method or operation is not implemented.");
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get { return GetCurrent(); }
            }

            public bool MoveNext()
            {
                //if ( Index < -1 )
                //return false;
                Index++;
                if ( Index >= _collection.Count )
                {
                    Index--;
                    return false;
                }
                else if ( Index >= 0 )
                    return true;
                else
                    return false;
            }

            public void Reset()
            {
                if ( Index < -1 )
                    return;
                else
                    Index = 0;
            }

            #endregion
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new EntityCollectionEnumerator<T>(this);
        }

        #endregion

        #region IList<T> Members


        public void Insert(int index, T item)
        {
            List.Insert(index, item);
            Pk.Add((item as IEntity).ID, item);
            SetOwnerState(EntityState.Changed);
            if ( _indexes != null )
                _indexes.Add(item);
        }

        public void RemoveAt(int index)
        {
            this.Remove(this[index]);
            //Pk.Remove((List[index] as IEntity).ID);
            //List.RemoveAt(index);
            //SetOwnerState(EntityState.Changed);
        }

        public T this[int index]
        {
            get { return List[index]; }
            set
            {
                if ( IsReadOnly )
                    throw new Exception("This collection have been created as read-only");
                Pk.Remove((List[index] as IEntity).ID);
                Pk.Add((value as IEntity).ID, value);
                List[index] = value;
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            if ( List.Count > array.Length - index )
                throw new Exception("Array's is too small");
            for ( int i = index; i < array.Length; i++ )
                array.SetValue(List[i], i);
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return null; }
        }

        #endregion

        #region IList Members

        public int Add(object value)
        {
            this.Add((T)value);
            SetOwnerState(EntityState.Changed);
            return IndexOf((T)value);
        }

        public bool Contains(object value)
        {
            return Contains((T)value);
        }

        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            Remove((T)value);
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        }

        #endregion

        public EntityCollection<TOutput> Map<TOutput>(Converter<T, TOutput> mapper) where TOutput : EntityBase
        {
            var l = new EntityCollection<TOutput>();
            foreach ( T e in this )
                l.Add(mapper(e));
            return l;
        }

        public void FillFrom<T2>(EntityCollection<T2> original, Converter<T2, T> mapper, bool appendItems) where T2 : EntityBase
        {
            if ( !appendItems )
                this.Clear();

        }

        #region IEquatable<EntityCollection<T>> Members

        public bool Equals(EntityCollection<T> other)
        {
            var r = this.Count == other.Count;
            if ( r )
                foreach ( T e in this )
                {
                    r &= other.Contains(e);
                    if ( !r )
                        break;
                }
            return r;
        }

        #endregion

        public static EntityCollectionEqualityComparer<T> GetCollectionEqualityComparer()
        {
            return new EntityCollectionEqualityComparer<T>();
        }

        public static EntityCollectionComparer<T> GetCollectionComparer()
        {
            return new EntityCollectionComparer<T>();
        }
    }
}
