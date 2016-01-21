using System;

namespace XnaWrapper.Collections
{
	public class TrackedPool<T> where T : class
	{
		private Func<T> createFunc;

		private T[] freeItems;
		private int freeIndex;

		public TrackedPool(int initialCapacity, Func<T> createFunc)
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
				freeItems = new T[oldCapacity * 2];
				// leave half free, to leave room for items being restored
				for (int i = 0; i < oldCapacity; ++i)
					freeItems[i] = createFunc();
				freeIndex = oldCapacity - 1;
			}
			T item = freeItems[freeIndex];
			freeItems[freeIndex] = null;
			--freeIndex;
			return item;
		}

		public void Restore(T item)
		{
			freeItems[++freeIndex] = item;
		}
	}
}
