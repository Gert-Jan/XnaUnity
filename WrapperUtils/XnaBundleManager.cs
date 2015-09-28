using System;
using System.IO;
using System.Collections.Generic;

namespace XnaWrapper
{
	public class XnaBundleManager
	{
		/// <summary>
		/// Generate a mapping from the content bundles in the game code. Implicitly makes the run a dummy run: limited to initializing and ignores content loading and the game loop.
		/// </summary>
		public static bool GENERATE_ASSET_MAPPINGS = true;
		public static string MAPPINGS_TEXT = null;


		private Dictionary<string, XnaBundleItem> activeMappings = new Dictionary<string, XnaBundleItem>();

		private Dictionary<string, XnaBundle> bundleMap = new Dictionary<string, XnaBundle>();

		public ICollection<XnaBundle> Bundles { get { return bundleMap.Values; } }

		#region Init

		public XnaBundleManager(TextReader bundleMappingsText)
		{
			try
			{
				InitBundles(bundleMappingsText);
			}
			catch (Exception e)
			{
				if (!GENERATE_ASSET_MAPPINGS)
					Debug.Log(e);
			}
		}

		void InitBundles(TextReader reader)
		{
			reader.ReadLine(); // Total (unnecessary line)
			int totalBundles = int.Parse(reader.ReadLine());
			XnaBundle[] bundles = new XnaBundle[totalBundles];

			for (int i = 0; i < totalBundles; ++i)
			{
				string name = reader.ReadLine();
				bundles[i] = new XnaBundle(name, reader);
			}

			foreach (XnaBundle bundle in bundles)
				bundleMap.Add(bundle.bundleName, bundle);
		}

		#endregion

		public void LoadBundle(string bundleName)
		{
			XnaBundle bundle;
			if (bundleMap.TryGetValue(bundleName, out bundle))
			{
				bundle.LoadBundleFile();
				MapBundleContents(bundle);
			}
			else
				throw new Exception("(LoadBundle) Unknown bundle: " + bundleName);
		}

		public bool IsBundleLoaded(string bundleName)
		{
			XnaBundle bundle;
			if (bundleMap.TryGetValue(bundleName, out bundle))
				return bundle.IsLoaded;
			else
				throw new Exception("(IsBundleLoaded) Unknown bundle: " + bundleName);
		}

		public void ReleaseBundle(string bundleName)
		{
			XnaBundle bundle;
			if (bundleMap.TryGetValue(bundleName, out bundle))
			{
				UnmapBundleContents(bundle);
				bundle.UnloadBundleFile();
			}
			else
				throw new Exception("(ReleaseBundle) Unknown bundle: " + bundleName);
		}

		public bool GetItem(string fileName, out XnaBundleItem item)
		{
			return activeMappings.TryGetValue(fileName, out item);
		}

		void MapBundleContents(XnaBundle bundle)
		{
			if (bundle.IsLoaded)
				return;
			bundle.IsLoaded = true;

			XnaBundleItem existingItem;
			foreach (XnaBundleItem item in bundle.Items)
			{
				if (activeMappings.TryGetValue(item.path, out existingItem))
				{
					// duplicate handling
					while (existingItem.nextDuplicate != null)
						existingItem = existingItem.nextDuplicate;
					existingItem.nextDuplicate = item;
					item.prevDuplicate = existingItem;
				}
				else
					activeMappings[item.path] = item;
			}
		}

		void UnmapBundleContents(XnaBundle bundle)
		{
			if (!bundle.IsLoaded)
				return;
			bundle.IsLoaded = false;

			foreach (XnaBundleItem item in bundle.Items)
			{
				if (item.nextDuplicate != null || item.prevDuplicate != null)
				{
					// duplicate handling
					if (item.nextDuplicate != null)
						item.nextDuplicate.prevDuplicate = item.prevDuplicate;
					if (item.prevDuplicate != null)
						item.prevDuplicate.nextDuplicate = item.nextDuplicate;
					item.nextDuplicate = null;
					item.prevDuplicate = null;
				}
				else
					activeMappings.Remove(item.path);
			}
		}


	}

	public class XnaBundle
	{
		public readonly string bundleName;

		internal bool IsLoaded = false;

		internal UnityEngine.AssetBundle unityBundle;
		private XnaBundleItem[] items;

		public ICollection<XnaBundleItem> Items { get { return items; } }

		internal XnaBundle(string name, TextReader reader)
		{
			bundleName = name;
			int bundleSize = int.Parse(reader.ReadLine());
			items = new XnaBundleItem[bundleSize];
			for (int i = 0; i < bundleSize; ++i)
				items[i] = new XnaBundleItem(this, reader);
		}

		internal void LoadBundleFile()
		{
			unityBundle = UnityEngine.WWW.LoadFromCacheOrDownload(bundleName, 1).assetBundle;
			if (unityBundle == null)
				throw new Exception("AssetBundle file could not be loaded. (" + bundleName + ")");
		}

		internal void UnloadBundleFile()
		{
			unityBundle.Unload(true);
		}
	}

	public class XnaBundleItem
	{
		public readonly XnaBundle owner;
		public readonly string path;

		internal XnaBundleItem nextDuplicate = null;
		internal XnaBundleItem prevDuplicate = null;

		internal XnaBundleItem(XnaBundle owner, TextReader reader)
		{
			this.owner = owner;
			path = reader.ReadLine();
		}

		public UnityEngine.AssetBundleRequest LoadAsync(string fileName, Type type)
		{
			if (type == null)
			{
				XnaWrapper.Debug.Log("XnaBundleItem: LoadAsync: type {0} not defined.", type);
				return owner.unityBundle.LoadAssetAsync(fileName);
			}
			else
				return owner.unityBundle.LoadAssetAsync(fileName, type);
		}

		public UnityEngine.Object Load(string fileName, Type type)
		{
			return owner.unityBundle.LoadAsset(fileName, type);
		}

	}

}
