using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace XnaWrapper
{
	public class XnaBundleManager
	{
		internal static string validPathFormat = null;

		private LinkedList<XnaBundle> bundlesLoading = new LinkedList<XnaBundle>();

		private Dictionary<string, XnaBundle> bundleMap = new Dictionary<string, XnaBundle>();
		private Dictionary<string, XnaBundleItem> bundleItemMap = new Dictionary<string, XnaBundleItem>();

		public ICollection<XnaBundle> Bundles { get { return bundleMap.Values; } }

		public bool TryGetItem(string fileName, out XnaBundleItem item)
		{
			return bundleItemMap.TryGetValue(fileName, out item);
		}

		#region Init

		public XnaBundleManager(TextReader bundleMappingsReader, bool readUnityPaths)
        {
			InitPathFormat();

			InitBundlesFromMappings(bundleMappingsReader, readUnityPaths);
        }

		void InitBundlesFromMappings(TextReader bundleMappingsReader, bool readUnityPaths)
		{
			int totalBundles = int.Parse(bundleMappingsReader.ReadLine());
			XnaBundle[] bundles = new XnaBundle[totalBundles];

			for (int i = 0; i < totalBundles; ++i)
				bundles[i] = new XnaBundle(bundleMappingsReader, bundleItemMap, readUnityPaths);

			foreach (XnaBundle bundle in bundles)
			{
				bundleMap.Add(bundle.bundleName, bundle);
			}
		}

		void InitPathFormat()
		{
			if (validPathFormat != null)
				return;

			switch (PlatformData.ActivePlatform)
			{
				case PlatformID.Windows:
					validPathFormat = string.Format("file://{0}/", Application.streamingAssetsPath);
					break;
				default:
					validPathFormat = string.Format("{0}/", Application.streamingAssetsPath);
					break;
			}
		}

		#endregion

		#region Management

		public void LoadBundle(string bundleName, bool async)
		{
			if (!async && PlatformData.AssetProvider == null)
				Debug.Log("(LoadBundle) Synchronized loading of bundles not supported: " + bundleName);

			XnaBundle bundle;
			if (bundleMap.TryGetValue(bundleName, out bundle))
			{
				if (bundle.Status != XnaBundleStatus.Initialized)
					return;

				bundle.LoadBundle();
				bundlesLoading.AddLast(bundle);
			}
			else
				Debug.Log("(LoadBundle) Bundle not present: " + bundleName);
		}

		public void ReleaseBundle(string bundleName)
		{
			XnaBundle bundle;
			if (bundleMap.TryGetValue(bundleName, out bundle))
			{
				XnaBundleStatus status = bundle.Status;
				if (status == XnaBundleStatus.Ready)
					bundle.ReleaseBundle();
				else if (status == XnaBundleStatus.LoadingQueued)
					bundlesLoading.Remove(bundle);
				else if (status == XnaBundleStatus.Loading)
					bundle.AbortLoad();
			}
			else
				Debug.Log("(ReleaseBundle) Bundle not present: " + bundleName);
		}

		public void UpdateBundleLoading()
		{
			while (bundlesLoading.Count > 0)
			{
				if (bundlesLoading.First.Value.Update() == XnaBundleStatus.Ready)
					bundlesLoading.RemoveFirst();
				else
					break;
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

	public class XnaBundle
	{
		private class Loader
		{
			private WWW data;

			private XnaBundle xnaBundle;
			private Stack<ContentRequest> busyRequests;

			private int index = 0;
			public bool shouldAbort = false;

			public Loader(XnaBundle xnaBundle)
			{
				this.xnaBundle = xnaBundle;

				foreach (XnaBundleItem item in xnaBundle.items)
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

			// Returns true once all requests are done
			public bool TryFinishLoading()
			{
				if (PlatformData.AssetProvider != null)
					return LoadFromProvider();


				if (data == null)
				{
					//Debug.LogT(xnaBundle.bundleName);
					LoadFromWWW();
				}
				if (!data.isDone)
					return false;

				if (busyRequests == null)
					InitRequests();

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
				int max = PlatformData.AssetProvider.MaxAssetsPerUpdate();

				for (int i = 0; i < max; ++i)
				{
					ContentRequest request = xnaBundle.items[index].Request;
					request.Asset = PlatformData.AssetProvider.LoadAsset(xnaBundle.itemUnityPaths[index]);

					++index;
					if (index == xnaBundle.items.Length)
					{
						index = 0;
						return true;
					}
				}

				return false;
			}

			private void LoadFromWWW()
			{
				string path = XnaBundleManager.validPathFormat + xnaBundle.bundleName.ToLower();
				data = WWW.LoadFromCacheOrDownload(path, 1);
			}

			private void InitRequests()
			{
				XnaBundleItem[] items = xnaBundle.items;
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

		public readonly string bundleName;

		private XnaBundleItem[] items;
		private string[] itemUnityPaths;

		private Loader loader = null;
		private bool isActive = false;

		public ICollection<XnaBundleItem> Items { get { return items; } }
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

        internal XnaBundle(TextReader reader, Dictionary<string, XnaBundleItem> bundleItemMap, bool readUnityPaths)
        {
            this.bundleName = reader.ReadLine();
            int bundleSize = int.Parse(reader.ReadLine());
            items = new XnaBundleItem[bundleSize];
            itemUnityPaths = new string[bundleSize];
            for (int i = 0; i < bundleSize; ++i)
            {
                string id = reader.ReadLine();
                int semicolonIndex = id.IndexOf(';');
                string name;


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

				XnaBundleItem existingItem;
				if (bundleItemMap.TryGetValue(name, out existingItem))
					items[i] = existingItem;
				else
				{
					items[i] = new XnaBundleItem(name);
					bundleItemMap[name] = items[i];
				}
			}
		}

		internal void LoadBundle()
		{
			isActive = true;

			loader = new Loader(this);
		}
		
		internal void ReleaseBundle()
		{
			isActive = false;

			foreach (XnaBundleItem item in items)
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

	public class XnaBundleItem
	{
		public readonly string name;

		private int objectReferences = 0;
		private ContentRequest request;

		public bool IsActive { get { return request != null; } }

		public ContentRequest Request
		{
			get { return request; }
		}

		internal XnaBundleItem(string name)
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
				if (PlatformData.AssetProvider == null)
					UObject.DestroyImmediate(Request.Asset, true);

				request = null;
			}
			else if (objectReferences < 0)
				throw new Exception("Failed to properly manage asset item references.");
		}
	}

}
