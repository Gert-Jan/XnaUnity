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

		public bool TryGetItem(string fileName, out XnaBundleItem item)
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
				bundles[i] = new XnaBundle(reader, bundleItemMap);
			
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
			if (bundleMap.TryGetValue(bundleName, out bundle))
			{

				if (bundle.IsActive)
					return;

				if (assetProvider != null)
					bundle.LoadBundleFromProvider(assetProvider);
				else if (async)
					bundle.LoadBundleAsync();
				else
					bundle.LoadBundle();
			}
			else
				Debug.Log("(LoadBundle) Bundle not present: " + bundleName);
		}

		public void ReleaseBundle(string bundleName)
		{
			XnaBundle bundle;
			if (bundleMap.TryGetValue(bundleName, out bundle))
			{
				if (!bundle.IsDone)
					throw new Exception("Attempt to release a bundle that is not yet loaded.");

				if (bundle.IsActive)
					bundle.ReleaseBundle();
			}
			else
				Debug.Log("(ReleaseBundle) Bundle not present: " + bundleName);
		}

		public bool IsBundleLoaded(string bundleName)
		{
			XnaBundle bundle;
			if (bundleMap.TryGetValue(bundleName, out bundle))
				return bundle.IsDone;
			else
				return true;
		}

		#endregion

	}

	public class XnaBundle
	{
		private class AsyncLoader
		{
			private WWW data;
			private AssetBundle assetBundle;

			private XnaBundle xnaBundle;
			private Stack<XnaBundleItem> busyRequests;

			public AsyncLoader(XnaBundle xnaBundle)
			{
				this.xnaBundle = xnaBundle;

				data = WWW.LoadFromCacheOrDownload(xnaBundle.fullPath, 1);
				assetBundle = data.assetBundle;

				XnaBundleItem[] items = xnaBundle.items;
				busyRequests = new Stack<XnaBundleItem>(items.Length);
				for (int i = 0; i < items.Length; ++i)
				{
					XnaBundleItem item = items[i];
					item.AddUsageReference();
					if (!item.IsActive)
					{
						item.Request = assetBundle.LoadAssetAsync(xnaBundle.unityPaths[i]);
						busyRequests.Push(item);
					}
				}
			}

			private bool Update()
			{
				while (busyRequests.Count > 0)
				{
					XnaBundleItem item = busyRequests.Peek();
					if (item.Request.isDone)
					{
						item.Asset = item.Request.asset;
						busyRequests.Pop();
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

		public readonly string bundleName;
		private string fullPath;

		internal XnaBundleItem[] items;
		internal string[] unityPaths;

		private AsyncLoader loader;

		private bool isActive = false;
		public bool IsActive { get { return isActive; } }

		public ICollection<XnaBundleItem> Items { get { return items; } }

		internal XnaBundle(TextReader reader, Dictionary<string, XnaBundleItem> bundleItemMap)
		{
			this.bundleName = reader.ReadLine();
			int bundleSize = int.Parse(reader.ReadLine());
			items = new XnaBundleItem[bundleSize];
			for (int i = 0; i < bundleSize; ++i)
			{
				string itemName = reader.ReadLine();
				XnaBundleItem existingItem;
				if (bundleItemMap.TryGetValue(itemName, out existingItem))
					items[i] = existingItem;
				else
				{
					items[i] = new XnaBundleItem(itemName);
					bundleItemMap[itemName] = items[i];
				}
			}
		}

		static string[] pathPrefixAttempts = new string[] { "file://", "jar:File://" };

		internal void PrepareLoad()
		{
			isActive = true;

			// Ensure Proper Asset Paths
			if (unityPaths != null)
				return;

			// Find proper bundle path
			WWW data;
			AssetBundle assetBundle;
			string basePath = Application.streamingAssetsPath + '/' + bundleName.ToLower();
			string attempt = basePath;
			int attemptCount = 0;
			while(true)
			{
				data = WWW.LoadFromCacheOrDownload(attempt, 1);
				assetBundle = data.assetBundle;

				if (assetBundle != null)
					break;

				data.Dispose();
				if (attemptCount == pathPrefixAttempts.Length)
					throw new Exception("AssetBundle file could not be loaded. (" + basePath + ")");
				attempt = basePath + pathPrefixAttempts[attemptCount++];
			}
			fullPath = attempt;
			
			// Reconstruct content bundle item IDs from names present in the asset bundle
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
			unityPaths = new string[items.Length];
			for (int i = 0; i < items.Length; ++i)
				unityPaths[i] = mappings[items[i].name.ToLower()];

			assetBundle.Unload(false);
			data.Dispose();
		}

		internal void LoadBundle()
		{
			PrepareLoad();

			WWW data = WWW.LoadFromCacheOrDownload(fullPath, 1);
			AssetBundle assetBundle = data.assetBundle;

			for (int i = 0; i < items.Length; ++i)
			{
				XnaBundleItem item = items[i];
				item.AddUsageReference();
				if (item.Asset == null)
					item.Asset = assetBundle.LoadAsset(unityPaths[i]);
			}

			data.Dispose();
			assetBundle.Unload(false);
		}
		
		internal void LoadBundleAsync()
		{
			PrepareLoad();

			if (loader == null)
				loader = new AsyncLoader(this);
		}

		internal void LoadBundleFromProvider(XnaAssetProvider assetProvider)
		{
			PrepareLoad();

			for (int i = 0; i < items.Length; ++i)
			{
				XnaBundleItem item = items[i];
				item.AddUsageReference();
				if (item.Asset == null)
					item.Asset = assetProvider.LoadAsset(unityPaths[i]);
			}
		}

		internal void ReleaseBundle()
		{
			isActive = false;

			foreach (XnaBundleItem item in items)
			{
				item.RemoveUsageReference();
			}
		}

		public bool IsDone
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
		public readonly string name;

		private int objectReferences = 0;
		private UObject loadedObject;
		private AssetBundleRequest request;

		public AssetBundleRequest Request
		{
			get { return request; }
			internal set { request = value; }
		}

		public UObject Asset
		{
			get { return loadedObject; }
			internal set { loadedObject = value; }
		}

		public bool IsActive { get { return loadedObject != null || request != null; } }

		internal XnaBundleItem(string name)
		{
			this.name = name;
		}

		internal void AddUsageReference()
		{
			++objectReferences;
		}

		internal void RemoveUsageReference()
		{
			--objectReferences;
			if (objectReferences == 0)
			{
				// Don't delete files in the editor, in case the asset was retrieved from AssetDatabase
				if (XnaBundleManager.assetProvider == null) 
					UObject.DestroyImmediate(loadedObject, true);
				loadedObject = null;
				request = null;
			}
			else if (objectReferences < 0)
				throw new Exception("Failed to properly manage asset item references.");
		}
	}

}
