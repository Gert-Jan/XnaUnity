using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XnaWrapper
{
	public abstract class Pool<T>
	{
		public abstract class InstanceResetter<T>
		{
			public abstract T Create();
			public abstract void Reset(T poolable);

			internal T CreateReset()
			{
				T instance = Create();
				Reset(instance);
				return instance;
			}
		}

		private InstanceResetter<T> instanceResetter;

		private T[] freeItems;
		private int freeIndex;

		protected Pool(int initialCapacity, InstanceResetter<T> resetter)
		{
			instanceResetter = resetter;

			freeItems = new T[initialCapacity];
			for (int i = 0; i < initialCapacity; ++i)
				freeItems[i] = instanceResetter.CreateReset();
			freeIndex = initialCapacity - 1;
		}

		public T Obtain()
		{
			if (freeIndex < 0)
			{
				// expand
				int oldCapacity = freeItems.Length;
				freeItems = new T[oldCapacity * 2];
				// leave half free, to leave room for items being restored
				for (int i = 0; i < oldCapacity; ++i)
					freeItems[i] = instanceResetter.CreateReset();
				freeIndex = oldCapacity - 1;
			}
			return freeItems[freeIndex--];
		}

		public void Restore(T item)
		{
			instanceResetter.Reset(item);
			freeItems[++freeIndex] = item;
		}
	}
}
