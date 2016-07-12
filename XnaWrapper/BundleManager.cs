using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using UnityEngine;

namespace XnaWrapper
{
	public class BundleManager
	{
		public const string DirSeparator = "$";
		public const string DotSeparator = "_";
		internal static string validPathFormat = null;
		public readonly bool oneAssetPerBundle;
		private readonly int totalBundles;
		private readonly int maxParallel;

		private LinkedList<Bundle> bundlesQueuedLoading = new LinkedList<Bundle>();
		private LinkedList<Bundle> bundlesLoading = new LinkedList<Bundle>();

		private Dictionary<string, Bundle> bundleMap = new Dictionary<string, Bundle>();
		private Dictionary<string, ContentItem> bundleItemMap = new Dictionary<string, ContentItem>();

		public ICollection<Bundle> Bundles { get { return bundleMap.Values; } }

		public bool TryGetItem(string fileName, out ContentItem item)
		{
			return bundleItemMap.TryGetValue(fileName, out item);
		}

		#region Init

		public BundleManager(string streamingAssetsPath, TextReader bundleMappingsReader, bool readUnityPaths)
		{
			validPathFormat = streamingAssetsPath;

			totalBundles = int.Parse(bundleMappingsReader.ReadLine());
			if (PlatformInstances.AssetLoadingInfo != null)
				maxParallel = PlatformInstances.AssetLoadingInfo.MaxAssetsLoadingParallel;

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
				if (bundleMap.ContainsKey(bundle.bundleName))
				{ 
					throw new ContentLoadException(string.Format("bundleMap already contains bundle named: {0}"
						+ "\nThis problem might be caused by duplicate include entries in CSpeedRunnerContent.contentproj. To resolve this problem, remove any duplicates.", bundle.bundleName));
				}
				else
				{
					bundleMap.Add(bundle.bundleName, bundle);
				}
			}
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
				float startTime = Time.realtimeSinceStartup;

				LinkedListNode<Bundle> node = bundlesLoading.First;
				while (node != null)
				{
					LinkedListNode<Bundle> nextNode = node.Next;

					if (node.Value.Update() == XnaBundleStatus.Ready)
						bundlesLoading.Remove(node);

					if (Time.realtimeSinceStartup - startTime > PlatformInstances.AssetLoadingInfo.MaxSecondsSpentLoadingPerUpdate)
						node = null;
					else
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

				foreach (ContentItem item in xnaBundle.items)
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

			private float startLoadTime;
			private float finishDecompressTime;
			private int loadedItems = 0;

			private void LoadWWW()
			{
				//Log.Write("Loading: " + xnaBundle.bundleFilePath);
				startLoadTime = Time.realtimeSinceStartup;
				data = new WWW(xnaBundle.bundleFilePath);
				//#if U_FUZE
				//				if (PlatformInstances.IsEditor)
				//					data = WWW.LoadFromCacheOrDownload(xnaBundle.bundleFilePath, 1);
				//				else
				//					data = new WWW(xnaBundle.bundleFilePath);
				//#else
				//				data = WWW.LoadFromCacheOrDownload(xnaBundle.bundleFilePath, 1);
				//#endif
			}

			// Returns true once all requests are done
			public bool TryFinishLoading()
			{
				try
				{
					if (PlatformInstances.AssetLoadingInfo.CanLoadFromAssetDatabase)
						return LoadFromProvider();
					
					if (busyRequests == null)
					{
						if (data == null)
						{
							LoadWWW();

							if (!string.IsNullOrEmpty(data.error))
								throw new Exception(string.Format("Error while trying to load url '{0}':\n{1}", data.url, data.error));

							return false;
						}

						if (!data.isDone)
							return false;

						finishDecompressTime = Time.realtimeSinceStartup;
						InitRequests();
						return false;
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

					float totalTime = Time.realtimeSinceStartup - startLoadTime;
					if (totalTime > 0.5f)
					{
						float decompressTime = finishDecompressTime - startLoadTime;
						float loadTime = Time.realtimeSinceStartup - finishDecompressTime;
						//Log.WriteT("Bundle {0} took long to load (decompressing: {1}, integration: {2})", xnaBundle.bundleName, decompressTime, loadTime);
					}
				}
				catch (Exception e)
				{
					Log.Write("Exception during TryFinishLoading ({0}): {1} , {2}", e.Message, xnaBundle.bundleFilePath, xnaBundle.bundleName);
					throw;
				}
				return true;
			}

			private bool LoadFromProvider()
			{
				for (int i = loadedItems; i < xnaBundle.items.Length; ++i)
				{
					ContentRequest request = xnaBundle.items[i].Request;
					request.Asset = PlatformInstances.AssetLoadingInfo.LoadFromAssetDatabase(xnaBundle.itemUnityPaths[i]);
					++loadedItems;
					return false;
                }

				return true;
			}

			private void InitRequests()
			{
				ContentItem[] items = xnaBundle.items;
				int numItems = items.Length;
				busyRequests = new Stack<ContentRequest>(numItems);

				for (int i = 0; i < numItems; ++i)
				{
					ContentRequest request = items[i].Request;
					if (!request.isDone && request.Operation == null)
					{
						if (!string.IsNullOrEmpty(data.error))
							throw new Exception(string.Format("Error while trying to load url '{0}':\n{1}",  data.url, data.error));
						request.Operation = data.assetBundle.LoadAssetAsync(xnaBundle.itemUnityPaths[i]);
						busyRequests.Push(request);
					}
				}
			}

		}

		private readonly string bundleFilePath;
		public readonly string bundleName;

		private ContentItem[] items;
		private string[] itemUnityPaths;

		private Loader loader = null;
		private bool isActive = false;

		public ICollection<ContentItem> Items { get { return items; } }
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

		internal Bundle(TextReader reader, Dictionary<string, ContentItem> bundleItemMap, bool readUnityPaths, bool oneAssetPerBundle)
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

			items = new ContentItem[bundleSize];
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

				ContentItem existingItem;
				if (bundleItemMap.TryGetValue(name, out existingItem))
					items[i] = existingItem;
				else
				{
					items[i] = new ContentItem(name);
					bundleItemMap[name] = items[i];
				}
			}

			if (oneAssetPerBundle)
			{
				bundleName = name;
				bundleFilePath = BundleManager.validPathFormat + name.Replace("\\", BundleManager.DirSeparator).Replace(".", BundleManager.DotSeparator);
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

			foreach (ContentItem item in items)
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
				bool shouldAbort = loader.shouldAbort;
				loader = null;
				if (shouldAbort)
				{
				    ReleaseBundle();
				    return XnaBundleStatus.Initialized;
				}
				else
					return XnaBundleStatus.Ready;
			}
			else
				return Status;
		}

	}
}
