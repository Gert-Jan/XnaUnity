using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XnaWrapper
{
	public abstract class Pool<T> where T : class
	{
		public interface InstanceResetter
		{
			T Create();
			void Reset(T poolable);
		}

		private InstanceResetter instanceResetter;

		private T[] freeItems;
		private int freeIndex;

		private T CreateReset()
		{
			T instance = instanceResetter.Create();
			instanceResetter.Reset(instance);
			return instance;
		}

		protected Pool(int initialCapacity, InstanceResetter resetter)
		{

			instanceResetter = resetter;

			freeItems = new T[initialCapacity];
			for (int i = 0; i < initialCapacity; ++i)
				freeItems[i] = CreateReset();
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
					freeItems[i] = CreateReset();
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
