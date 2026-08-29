using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;

namespace Mephi.Cybernetics.Race.DataAccess
{
	[Serializable]
	public class EntityList<T> : CollectionBase, IList<T> where T : IEntity
	{
		private EntityBase _owner;
		private EntityAttributeDescription _property;

		public EntityList()
		{
		}

		public EntityList(EntityBase owner, EntityAttributeDescription property)
		{
			_owner = owner;
			_property = property;
		}

		internal EntityList(EntityBase owner, EntityAttributeDescription property, int capacity)
			: base(capacity)
		{
			_owner = owner;
			_property = property;
		}

		public void Add(T e)
		{
			List.Add(e);
			SetOwnerState(EntityState.Changed);
			if ( _property != null && _property.IsOneToManyRelation )
			{
                var newEntityDescription = EntityDescription.GetEntityDescription(e.GetType());
				foreach ( var pd in newEntityDescription.EntityAttributes.Relationships.OneToOne )
				{
					if ( !pd.IsMapped )
						continue;
                    if (pd.RelationInfo.RelatedColumn == (_property.Domain as EntityDescription).SystemId.Name &&
						pd.Range.IsSubtypeOf(_owner.GetType()) )
					{
						pd.SetValue(e, _owner);
						break;
					}
				}
			}
		}

		public bool Remove(T e)
		{
			List.Remove(e);
			SetOwnerState(EntityState.Changed);
			if ( _property != null && _property.IsOneToManyRelation )
			{
                var newEntityDescription = EntityDescription.GetEntityDescription(e.GetType());
				foreach ( var pd in newEntityDescription.EntityAttributes.Relationships )
				{
					if ( !pd.IsOneToOneRelation || !pd.IsMapped )
						continue;
					if ( pd.RelationInfo.RelatedColumn == (_property.Domain as EntityDescription).SystemId.Name &&
						pd.Range.IsSubtypeOf(_owner.GetType()) )
					{
						pd.SetValue(e, null);
						break;
					}
				}
			}
			return true;
		}

		private void SetOwnerState(EntityState state)
		{
			if ( _owner == null )
				return;
			if ( state == EntityState.Changed )
			{
				if ( _owner.State == EntityState.Unchanged )
					_owner.SetState(EntityState.Changed);
			}
		}

		protected override void OnInsert(int index, object value)
		{
			base.OnInsert(index, value);
			SetOwnerState(EntityState.Changed);
		}

		protected override void OnRemove(int index, object value)
		{
			base.OnRemove(index, value);
			SetOwnerState(EntityState.Changed);
		}

		protected override void OnClear()
		{
			base.OnClear();
			SetOwnerState(EntityState.Changed);
		}

		public T this[int index]
		{
			get
			{
				return (T)InnerList[index];
			}
			set
			{
				InnerList[index] = value;
			}
		}

		#region IList<T> Members

		public int IndexOf(T item)
		{
			return base.List.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			base.InnerList.Insert(index, item);
		}

		#endregion

		#region ICollection<T> Members


		public bool Contains(T item)
		{
			return base.List.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			base.List.CopyTo(array, arrayIndex);
		}

		public bool IsReadOnly
		{
			get
			{
				return base.List.IsFixedSize;
			}
		}

		#endregion

		#region IEnumerable<T> Members

		public class EntityListEnumerator<T> : IEnumerator<T> where T : IEntity
		{
			private EntityList<T> _owner;
			private int currentIndex = -1;

			internal EntityListEnumerator(EntityList<T> owner)
			{
				_owner = owner;
			}

			#region IEnumerator<T> Members

			public T Current
			{
				get
				{
					if ( currentIndex >= 0 )
						return _owner[currentIndex];
					else
						throw new Exception("Enumerator not advanced.");
				}
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{

			}

			#endregion

			#region IEnumerator Members

			object IEnumerator.Current
			{
				get
				{
					return Current;
				}
			}

			public bool MoveNext()
			{
				return _owner.Count > ++currentIndex;

			}

			public void Reset()
			{
				currentIndex = -1;
			}

			#endregion
		}

		public new IEnumerator<T> GetEnumerator() 
		{
			return new EntityListEnumerator<T>(this);
		}

		#endregion

		/// <summary>
		/// Generates a detached EntityList with only those elements satisfying the the specified condition. The algorithm performs
		/// linear walkthrought on the elements of the list
		/// </summary>
		/// <param name="fieldName">The name of the attribute to match</param>
		/// <param name="value">The value the specified attribute should be matched with</param>
		/// <returns>Detached EntityList with only those elements satisfying the the specified condition</returns>
		public EntityList<T> Select(string fieldName, object value)
		{
			EntityList<T> list = new EntityList<T>();
			foreach ( T entity in List )
				if ( entity[fieldName] == value )
				{
					list.Add(entity);
				}
			return list;
		}

		/// <summary>
		/// Generates a detached EntityList with only those elements satisfying the the specified condition. The algorithm performs
		/// linear walkthrought on the elements of the list
		/// </summary>
		/// <param name="condition">A predicate that the element of the current list should match in order to be selected</param>
		/// <returns>Detached EntityList with only those elements satisfying the the specified condition</returns>
		public EntityList<T> Select(Predicates.UnaryOperation<T> condition)
		{
			EntityList<T> list = new EntityList<T>();
			foreach ( T entity in List )
				if ( condition(entity) )
				{
					list.Add(entity);
				}
			return list;
		}

		/// <summary>
		/// Searches for the first element satisfying the the specified condition. The algorithm performs
		/// linear walkthrought on the elements of the list
		/// </summary>
		/// <param name="fieldName">The name of the attribute to match</param>
		/// <param name="value">The value the specified attribute should be matched with</param>
		/// <returns>The first element to satisfy the specified condition</returns>
		public T SelectOne(string fieldName, object value)
		{
			foreach ( T entity in List )
				if ( entity[fieldName] == value )
				{
					return entity;
				}
			return default(T);
		}

		/// <summary>
		/// Searches for the first element satisfying the the specified condition. The algorithm performs
		/// linear walkthrought on the elements of the list
		/// </summary>
		/// <param name="condition">A predicate that the element of the current list should match in order to be selected</param>
		/// <returns>The first element to satisfy the specified condition</returns>
		public T SelectOne(Predicates.UnaryOperation<T> condition)
		{
			foreach ( T entity in List )
				if ( condition(entity) )
				{
					return entity;
				}
			return default(T);
		}

		public T[] ToArray()
		{
			T[] array = new T[this.List.Count];
			InnerList.CopyTo(array);
			return array;
		}

		public void Sort()
		{
			Comparison<T> u = delegate(T e1, T e2)
			{
				return string.Compare(e1.ID.ToString(), e2.ID.ToString());
			};
			InnerList.Sort(new EntityComparer<T>(u));
		}

		public void Sort(string fieldName)
		{
			Comparison<T> u = delegate(T e1, T e2)
			{
				return string.Compare(e1[fieldName].ToString(), e2[fieldName].ToString());
			};
			InnerList.Sort(new EntityComparer<T>(u));
		}

		public void Sort(Comparison<T> comp)
		{
			InnerList.Sort(new EntityComparer<T>(comp));
		}
	}

	public class EntityComparer : IComparer
	{
		private Comparison<object> _comparison;

		public EntityComparer(Comparison<object> comparison)
		{
			_comparison = comparison;
		}

		#region IComparer Members

		public int Compare(object x, object y)
		{
			return _comparison(x, y);
		}

		#endregion
	}

	public class EntityComparer<T> : IComparer<T>, IComparer
	{
		private Comparison<T> _comparison;

		public EntityComparer(Comparison<T> comparison)
		{
			_comparison = comparison;
		}

		#region IComparer<T> Members

		public int Compare(T x, T y)
		{
			return _comparison(x, y);
		}

		#endregion

		#region IComparer Members

		public int Compare(object x, object y)
		{
			return _comparison((T)x, (T)y);
		}

		#endregion
	}

    public interface IEntityList : IList
    {

    }
}
