using System;

namespace XnaWrapper.Collections
{
	public class BulkPool<T> where T : class
	{
		public Func<T> createFunc;

		private T[] freeItems;
		private int freeIndex;

		public BulkPool(int initialCapacity, Func<T> createFunc)
		{
			this.createFunc = createFunc;

			freeItems = new T[initialCapacity];
			for (int i = 0; i < initialCapacity; ++i)
				freeItems[i] = createFunc();
			freeIndex = freeItems.Length - 1;
		}

		public T Obtain()
		{
			if (freeIndex < 0)
			{
				// expand
				int oldCapacity = freeItems.Length;
				int newCapacity = oldCapacity * 2;
				T[] newArray = new T[newCapacity];
				int i = 0;
				for (; i < oldCapacity; ++i)
					newArray[i] = createFunc();
				for (int j = 0; i < newCapacity; ++i, ++j)
					newArray[i] = freeItems[j];
				freeItems = newArray;
				freeIndex = oldCapacity - 1;
			}
			return freeItems[freeIndex--];
		}

		public void RestoreAll()
		{
			freeIndex = freeItems.Length - 1;
		}
	}
}
