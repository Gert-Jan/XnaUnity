using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace XnaWrapper
{
	public class StreamedContentManager
	{
		internal static string validPath;
		private Dictionary<string, ContentItem> itemMap = new Dictionary<string, ContentItem>();
		private Dictionary<string, string> unityPaths = new Dictionary<string, string>();
		private Queue<Loader> loading = new Queue<Loader>();

		public StreamedContentManager(string streamingAssetsPath, TextReader mappingsReader)
		{
			validPath = streamingAssetsPath;

			int numItems = int.Parse(mappingsReader.ReadLine());

			mappingsReader.ReadLine(); // type (streaming)

			for (int i = 0; i < numItems; ++i)
			{
				string line = mappingsReader.ReadLine();
				string[] parts = line.Split(';');
				itemMap[parts[0]] = new ContentItem(parts[0]);
				unityPaths[parts[0]] = parts[1];
            }
        }

		public bool TryGetItem(string fileName, out ContentItem item)
		{
			return itemMap.TryGetValue(fileName, out item);
		}

		public void LoadItem(string fileName, Type unityType)
		{
			loading.Enqueue(new Loader(itemMap[fileName], unityType, unityPaths[fileName]));
		}

		public void UpdateLoading()
		{
			if (loading.Count == 0)
				return;

			while (loading.Peek().Update())
			{
				loading.Dequeue();
				if (loading.Count == 0)
					return;
			}
		}
		
		class Loader
		{
			readonly ContentItem item;
			readonly string url;

			Func<bool> TypeUpdate;

			UnityEngine.Object asset;
			WWW www;

			public Loader(ContentItem item, Type unityType, string unityPath)
			{
				this.item = item;

				item.AddUsageReference();
				url = "file://" + validPath + unityPath;

				if (unityType == typeof(AudioClip))
					TypeUpdate = UpdateAudioClip;
				else
					throw new Exception("Unknown stream type.");
			}

			public bool Update()
			{
				if (www == null)
				{
					www = new WWW(url);
				}

				if (!string.IsNullOrEmpty(www.error))
					throw new Exception("stream error: " + www.error);

				if (www.isDone)
				{
					return TypeUpdate();
				}

				return false;
			}
			
			bool UpdateAudioClip()
			{
				if (asset == null)
					asset = www.GetAudioClip(false, true);
				AudioClip clip = asset as AudioClip;

				if (clip.loadState == AudioDataLoadState.Failed)
					throw new Exception("Failed to load AudioClip music");

				if (clip.loadState == AudioDataLoadState.Loaded)
				{
					item.Request.Asset = clip;
					return true;
				}

				return false;
			}
			
		}
	}
}
