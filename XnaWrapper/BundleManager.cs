using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace XnaWrapper
{
	public class BundleManager
	{
		public const string DirSeparator = "$";
		internal static string validPathFormat = null;
		public readonly bool oneAssetPerBundle;
		private readonly int totalBundles;
		private readonly int maxParallel;

		private LinkedList<Bundle> bundlesQueuedLoading = new LinkedList<Bundle>();
		private LinkedList<Bundle> bundlesLoading = new LinkedList<Bundle>();

		private Dictionary<string, Bundle> bundleMap = new Dictionary<string, Bundle>();
		private Dictionary<string, BundleItem> bundleItemMap = new Dictionary<string, BundleItem>();

		public ICollection<Bundle> Bundles { get { return bundleMap.Values; } }

		public bool TryGetItem(string fileName, out BundleItem item)
		{
			return bundleItemMap.TryGetValue(fileName, out item);
		}

		#region Init

		public BundleManager(TextReader bundleMappingsReader, bool readUnityPaths)
        {
			InitPathFormat();

			totalBundles = int.Parse(bundleMappingsReader.ReadLine());
			if (PlatformInstances.AssetLoadingInfo != null)
				maxParallel = PlatformInstances.AssetLoadingInfo.MaxAssetsLoadingParallel();

			string mode = bundleMappingsReader.ReadLine().ToLower();
			switch (mode)
			{
				case "single":
					oneAssetPerBundle = true;
                    break;
				case "multiple":
					oneAssetPerBundle = false;
					break;
				default:
					throw new Exception("Unknown AssetBundleMapping source mode: " + mode);
            }
			
			InitBundlesFromMappings(bundleMappingsReader, readUnityPaths);
        }

		void InitBundlesFromMappings(TextReader bundleMappingsReader, bool readUnityPaths)
		{
			Bundle[] bundles = new Bundle[totalBundles];
			for (int i = 0; i < totalBundles; ++i)
				bundles[i] = new Bundle(bundleMappingsReader, bundleItemMap, readUnityPaths, oneAssetPerBundle);

			foreach (Bundle bundle in bundles)
			{
				bundleMap.Add(bundle.bundleName, bundle);
			}
		}

		void InitPathFormat()
		{
			if (validPathFormat != null)
				return;

			if (Application.isEditor)
				validPathFormat = string.Format("file://{0}/", Application.streamingAssetsPath);
			else
#if U_WINDOWS
				validPathFormat = string.Format("file://{0}/", Application.streamingAssetsPath);
#else
				validPathFormat = string.Format("{0}/", Application.streamingAssetsPath);
#endif
		}

#endregion

#region Management

		public void LoadBundle(string bundleName)
		{
			Bundle bundle;
			if (bundleMap.TryGetValue(bundleName, out bundle))
			{
				if (bundle.Status != XnaBundleStatus.Initialized)
					return;

				bundle.LoadBundle();
				bundlesQueuedLoading.AddLast(bundle);
			}
			else if (!oneAssetPerBundle)
				Log.Write("(LoadBundle) Bundle not present: " + bundleName);
		}

		public void ReleaseBundle(string bundleName)
		{
			Bundle bundle;
			if (bundleMap.TryGetValue(bundleName, out bundle))
			{
				XnaBundleStatus status = bundle.Status;
				if (status == XnaBundleStatus.Ready)
					bundle.ReleaseBundle();
				else if (status == XnaBundleStatus.LoadingQueued)
					bundlesQueuedLoading.Remove(bundle);
				else if (status == XnaBundleStatus.Loading)
					bundle.AbortLoad();
			}
			else if (!oneAssetPerBundle)
				Log.Write("(ReleaseBundle) Bundle not present: " + bundleName);
		}

		public void UpdateBundleLoading()
		{
			while (bundlesQueuedLoading.Count > 0 && bundlesLoading.Count < maxParallel)
			{
				bundlesLoading.AddLast(bundlesQueuedLoading.First.Value);
				bundlesQueuedLoading.RemoveFirst();
			}
			
			if (bundlesLoading.Count > 0)
			{
				LinkedListNode<Bundle> node = bundlesLoading.First;
				while (node != null)
				{
					LinkedListNode<Bundle> nextNode = node.Next;

					if (node.Value.Update() == XnaBundleStatus.Ready)
						bundlesLoading.Remove(node);

					node = nextNode;
				}
			}
		}

#endregion
	}

	public enum XnaBundleStatus
	{
		Initialized,
		LoadingQueued,
		Loading,
		Ready
	}

	public class Bundle
	{
		private class Loader
		{
			private WWW data;

			private Bundle xnaBundle;
			private Stack<ContentRequest> busyRequests;
			
			public bool shouldAbort = false;

			public Loader(Bundle xnaBundle)
			{
				this.xnaBundle = xnaBundle;

				foreach (BundleItem item in xnaBundle.items)
					item.AddUsageReference();
			}

			public XnaBundleStatus Status
			{
				get
				{
					if (data == null)
						return XnaBundleStatus.LoadingQueued;
					else
						return XnaBundleStatus.Loading;
				}
			}

			private void LoadWWW()
			{
#if U_FUZE
				if (PlatformInstances.IsEditor)
					data = WWW.LoadFromCacheOrDownload(xnaBundle.bundleFilePath, 1);
				else
					data = new WWW(xnaBundle.bundleFilePath);
#else
				data = WWW.LoadFromCacheOrDownload(xnaBundle.bundleFilePath, 1);
#endif
			}

			// Returns true once all requests are done
			public bool TryFinishLoading()
			{
				if (PlatformInstances.AssetLoadingInfo.LoadFromAssetDatabase())
					return LoadFromProvider();

				if (busyRequests == null)
				{
					if (data == null)
						LoadWWW();

					if (!string.IsNullOrEmpty(data.error))
						throw new Exception(data.error);

					if (!data.isDone)
						return false;

					InitRequests();
				}

				while (busyRequests.Count > 0)
				{
					if (busyRequests.Peek().isDone)
						busyRequests.Pop();
					else
						return false;
				}

				data.assetBundle.Unload(false);
				data.Dispose();

				return true;
			}

			private bool LoadFromProvider()
			{
				for (int i = 0; i < xnaBundle.items.Length; ++i)
				{
					ContentRequest request = xnaBundle.items[i].Request;
					request.Asset = PlatformInstances.AssetLoadingInfo.LoadFromAssetDatabase(xnaBundle.itemUnityPaths[i]);
				}

				return true;
			}
			
			private void InitRequests()
			{
				BundleItem[] items = xnaBundle.items;
				int numItems = items.Length;
				busyRequests = new Stack<ContentRequest>(numItems);

				for (int i = 0; i < numItems; ++i)
				{
					ContentRequest request = items[i].Request;
					if (!request.isDone && request.Operation == null)
					{
						request.Operation = data.assetBundle.LoadAssetAsync(xnaBundle.itemUnityPaths[i]);
						busyRequests.Push(request);
					}
				}
			}

		}

		private readonly string bundleFilePath;
		public readonly string bundleName;

		private BundleItem[] items;
		private string[] itemUnityPaths;

		private Loader loader = null;
		private bool isActive = false;

		public ICollection<BundleItem> Items { get { return items; } }
		public ICollection<string> ItemUnityPaths { get { return itemUnityPaths; } }

		public XnaBundleStatus Status
		{
			get
			{
				if (isActive)
				{
					if (loader == null)
						return XnaBundleStatus.Ready;
					else
						return loader.Status;
				}
				return XnaBundleStatus.Initialized;
			}
		}

        internal Bundle(TextReader reader, Dictionary<string, BundleItem> bundleItemMap, bool readUnityPaths, bool oneAssetPerBundle)
        {
			int bundleSize;
            if (oneAssetPerBundle)
				bundleSize = 1;
			else
			{
				bundleName = reader.ReadLine();
				bundleFilePath = BundleManager.validPathFormat + bundleName;
                bundleSize = int.Parse(reader.ReadLine());
			}
			
			items = new BundleItem[bundleSize];
			itemUnityPaths = new string[bundleSize];

			string name = null;
			for (int i = 0; i < bundleSize; ++i)
			{
				string id = reader.ReadLine();
				int semicolonIndex = id.IndexOf(';');

				if (semicolonIndex == -1)
				{
					if (readUnityPaths)
						throw new Exception("Unable to find unity paths in input mappings.");
					name = id;
				}
				else
				{
					name = id.Substring(0, semicolonIndex);
					itemUnityPaths[i] = id.Substring(semicolonIndex + 1, id.Length - semicolonIndex - 1);
				}

				BundleItem existingItem;
				if (bundleItemMap.TryGetValue(name, out existingItem))
					items[i] = existingItem;
				else
				{
					items[i] = new BundleItem(name);
					bundleItemMap[name] = items[i];
				}
			}

			if (oneAssetPerBundle)
			{
				bundleName = name;
				bundleFilePath = BundleManager.validPathFormat + name.Replace("\\", BundleManager.DirSeparator);
			}
			bundleFilePath = bundleFilePath.ToLower();
        }
		
		internal void LoadBundle()
		{
			isActive = true;

			loader = new Loader(this);
		}
		
		internal void ReleaseBundle()
		{
			isActive = false;

			foreach (BundleItem item in items)
				item.RemoveUsageReference();
		}

		internal void AbortLoad()
		{
			loader.shouldAbort = true;
		}

		internal XnaBundleStatus Update()
		{
            if (loader != null && loader.TryFinishLoading())
            {
               //bool shouldAbort = loader.shouldAbort;
               //loader = null;
               //if (shouldAbort)
               //{
               //    ReleaseBundle();
               //    return XnaBundleStatus.Initialized;
               //}
               //else
                    return XnaBundleStatus.Ready;
            }
            else
                return Status;
		}

	}

	public class BundleItem
	{
		public readonly string name;

		private int objectReferences = 0;
		private ContentRequest request;

		public bool IsActive { get { return request != null; } }

		public ContentRequest Request
		{
			get { return request; }
		}

		internal BundleItem(string name)
		{
			this.name = name;
		}

		internal void AddUsageReference()
		{
			++objectReferences;

			if (request == null)
				request = new ContentRequest();
		}

		internal void RemoveUsageReference()
		{
			--objectReferences;
			if (objectReferences == 0)
			{
				// Don't delete files in the editor, in case the asset was retrieved from AssetDatabase
				if (!PlatformInstances.AssetLoadingInfo.LoadFromAssetDatabase())
					UObject.DestroyImmediate(Request.Asset, true);

				request = null;
			}
			else if (objectReferences < 0)
				throw new Exception("Failed to properly manage asset item references.");
		}
	}

}
