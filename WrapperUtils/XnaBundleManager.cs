using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace XnaWrapper
{
	public abstract class XnaAssetProvider
	{
		public abstract UObject LoadAsset(string path);
	}

	public class XnaBundleManager
	{
		/// <summary>
		/// Generate a mapping from the content bundles created in the game code. Implicitly makes the game execution a 
		/// dummy run: limited to initializing and ignores content loading and the game loop.
		/// 
		/// Used in CSpeedRunners main.cs
		/// </summary>
		public static bool GENERATE_ASSET_MAPPINGS = true;
		public static string MAPPINGS_TEXT = null;

		public static XnaAssetProvider assetProvider = null;

		private Dictionary<string, XnaBundle> bundleMap = new Dictionary<string, XnaBundle>();
		private Dictionary<string, XnaBundleItem> bundleItemMap = new Dictionary<string, XnaBundleItem>();

		public ICollection<XnaBundle> Bundles { get { return bundleMap.Values; } }

		public bool GetItem(string fileName, out XnaBundleItem item)
		{
			return bundleItemMap.TryGetValue(fileName, out item);
		}

		#region Init

		/// <summary>
		/// Providing a null assetProvider will default to loading from assetbundles. 
		/// </summary>
		/// <param name="bundleMappingsText"></param>
		/// <param name="assetProvider"></param>
		public XnaBundleManager(TextReader bundleMappingsText)
		{
			try
			{	
				InitBundles(bundleMappingsText);
			}
			catch (Exception e)
			{
				if (!GENERATE_ASSET_MAPPINGS)
					throw e;
			}
		}

		void InitBundles(TextReader reader)
		{
			reader.ReadLine(); // Total (unnecessary line)
			int totalBundles = int.Parse(reader.ReadLine());
			XnaBundle[] bundles = new XnaBundle[totalBundles];

			for (int i = 0; i < totalBundles; ++i)
				bundles[i] = new XnaBundle(reader);
			
			foreach (XnaBundle bundle in bundles)
			{
				bundleMap.Add(bundle.bundleName, bundle);
			}
		}

		#endregion

		#region Management

		public void LoadBundle(string bundleName, bool async)
		{
			XnaBundle bundle;
			if (!bundleMap.TryGetValue(bundleName, out bundle))
				throw new Exception("(MapBundle) Bundle not present: " + bundleName);

			XnaBundleItem existingItem;
			List<string> duplicates = new List<string>();
			foreach (XnaBundleItem item in bundle.Items)
			{
				if (bundleItemMap.TryGetValue(item.name, out existingItem))
				{
					if (bundle == existingItem.owner)
						return;
					duplicates.Add(String.Format("item[{0}], bundleA[{1}], bundleB[{2}]", item.name, bundle.bundleName, existingItem.owner.bundleName));
				}
				else
					bundleItemMap.Add(item.name, item);
			}

			if (duplicates.Count > 0)
			{
				string warn = String.Format("Warning: {0} active duplicates detected: ", duplicates.Count);
				foreach (string s in duplicates)
					warn += '\n' + s;
				Debug.Log(warn);
			}

			if (async)
				bundle.LoadBundleAsync();
			else
				bundle.LoadBundle();
		}

		public void ReleaseBundle(string bundleName)
		{
			XnaBundle bundle;
			if (!bundleMap.TryGetValue(bundleName, out bundle))
				throw new Exception("(UnmapBundle) Bundle not present: " + bundleName);

			foreach (XnaBundleItem item in bundle.Items)
				bundleItemMap.Remove(item.name);

			bundle.ReleaseBundle();
		}

		public bool IsBundleLoaded(string bundleName)
		{
			XnaBundle bundle;
			if (!bundleMap.TryGetValue(bundleName, out bundle))
				throw new Exception("(IsBundleLoaded) Bundle not present: " + bundleName);

			return bundle.isDone;
		}

		#endregion

	}

	internal class XnaBundleAsyncLoader
	{
		private WWW data;
		private AssetBundle assetBundle;

		private XnaBundle xnaBundle;
		private List<XnaBundleItem> busyRequests;

		public XnaBundleAsyncLoader(XnaBundle xnaBundle)
		{
			this.xnaBundle = xnaBundle;

			data = WWW.LoadFromCacheOrDownload(xnaBundle.bundlePath, 1);
			assetBundle = data.assetBundle;
			if (assetBundle == null)
				throw new Exception("AssetBundle file could not be loaded. (" + xnaBundle.bundlePath + ")");

			XnaBundleItem[] items = xnaBundle.items;
			busyRequests = new List<XnaBundleItem>(xnaBundle.Items);
			foreach (XnaBundleItem item in busyRequests)
				item.request = assetBundle.LoadAssetAsync(item.unityPath);
		}

		private bool Update()
		{
			while (busyRequests.Count > 0)
			{
				int index = busyRequests.Count - 1;
				XnaBundleItem item = busyRequests[index];
				if (item.request.isDone)
				{
					item.loadedObject = item.request.asset;
					busyRequests.RemoveAt(index);
				}
				else
					return false;
			}
			return true;
		}

		public bool isDone
		{
			get
			{
				bool done = Update();
				if (done)
				{
					data.Dispose();
					assetBundle.Unload(false);
				}
				return done;
			}
		}
	}

	public class XnaBundle
	{
		public readonly string bundleName;
		public readonly string bundlePath;

		internal XnaBundleItem[] items;
		private bool wasPreviouslyLoaded = false;

		private XnaBundleAsyncLoader loader;

		public ICollection<XnaBundleItem> Items { get { return items; } }

		internal XnaBundle(TextReader reader)
		{
			this.bundleName = reader.ReadLine();
			int bundleSize = int.Parse(reader.ReadLine());
			items = new XnaBundleItem[bundleSize];
			for (int i = 0; i < bundleSize; ++i)
				items[i] = new XnaBundleItem(this, reader);

			if (Application.platform == RuntimePlatform.Android)
				bundlePath = "jar:file://" + Application.streamingAssetsPath + '/' + bundleName.ToLower();
			else
				bundlePath = "file://" + Application.streamingAssetsPath + '/' + bundleName.ToLower();
		}

		internal void EnsureProperAssetPaths()
		{
			if (wasPreviouslyLoaded)
				return;

			WWW data = WWW.LoadFromCacheOrDownload(bundlePath, 1);
			AssetBundle assetBundle = data.assetBundle;

			// reconstruct content bundle item IDs from names present in the asset bundle
			string[] validPaths = assetBundle.GetAllAssetNames();
			Dictionary<string, string> mappings = new Dictionary<string, string>();
			string prepath = "assets/content/" + bundleName.ToLower() + '/';
			foreach(string validPath in validPaths)
			{
				int i = validPath.LastIndexOf('.');
				string key = validPath.Substring(0, i).ToLower().Replace(prepath, "").Replace('/', '\\');
				mappings.Add(key, validPath);
			}

			// supply all items with the new path
			foreach (XnaBundleItem item in items)
				item.unityPath = mappings[item.name.ToLower()];

			data.Dispose();
			wasPreviouslyLoaded = true;
		}

		internal void LoadBundle()
		{
			EnsureProperAssetPaths();

			if (XnaBundleManager.assetProvider != null)
				LoadFromProvider();
			else
			{
				WWW data = WWW.LoadFromCacheOrDownload(bundlePath, 1);
				AssetBundle assetBundle = data.assetBundle;
				if (assetBundle == null)
					throw new Exception("AssetBundle file could not be loaded. (" + bundlePath + ")");

				foreach (XnaBundleItem item in items)
					item.loadedObject = assetBundle.LoadAsset(item.unityPath);

				data.Dispose();
				assetBundle.Unload(false);
			}
		}
		
		internal void LoadBundleAsync()
		{
			EnsureProperAssetPaths();

			if (XnaBundleManager.assetProvider != null) 
				LoadFromProvider();
			else
			{
				if (loader == null)
					loader = new XnaBundleAsyncLoader(this);
			}
		}

		private void LoadFromProvider()
		{
			foreach (XnaBundleItem item in items)
				item.loadedObject = XnaBundleManager.assetProvider.LoadAsset(item.unityPath);
		}

		internal void ReleaseBundle()
		{
			if (!isDone)
				throw new Exception("Attempt to release a bundle that is not yet loaded.");

			foreach (XnaBundleItem item in items)
			{
				UObject.DestroyImmediate(item.loadedObject, true);
				item.loadedObject = null;
			}
		}

		public bool isDone
		{
			get
			{
				if (loader == null)
					return true;
				else if (loader.isDone)
				{
					loader = null;
					return true;
				}
				else
					return false;
			}
		}
	}

	public class XnaBundleItem
	{
		public readonly XnaBundle owner;
		public readonly string name;

		internal string unityPath;

		internal UObject loadedObject;
		internal AssetBundleRequest request;

		internal XnaBundleItem(XnaBundle owner, TextReader reader)
		{
			this.owner = owner;
			name = reader.ReadLine();
		}
	}

}
