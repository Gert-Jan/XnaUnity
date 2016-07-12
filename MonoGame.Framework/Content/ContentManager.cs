using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using UTexture = UnityEngine.Texture2D;
using UAudioClip = UnityEngine.AudioClip;
using UObject = UnityEngine.Object;
using UResources = UnityEngine.Resources;
using TextAsset = UnityEngine.TextAsset;
using XTexture = Microsoft.Xna.Framework.Graphics.Texture2D;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Utilities;
using TextureAtlasContent;
using UnityEngine;
using XnaWrapper;

namespace Microsoft.Xna.Framework.Content
{
	public class ContentRequest
	{
		private AsyncOperation operation;
		private UObject asset;

		internal ContentRequest() { }

		internal AsyncOperation Operation
		{
			set { operation = value; }
			get { return operation; }
		}

		public bool isDone
		{
			get
			{
				if (operation != null && operation.isDone)
				{
					if (operation is ResourceRequest)
						asset = (operation as ResourceRequest).asset;
					else if (operation is AssetBundleRequest)
						asset = (operation as AssetBundleRequest).asset;
					operation = null;
				}
				return asset != null;
			}
		}

		public UObject Asset
		{
			internal set { asset = value; }
			get { return asset; }
		}

	}

	public class ContentManager
	{
		const byte ContentCompressedLzx = 0x80;
		const byte ContentCompressedLz4 = 0x40;
        
		private static BundleManager xnaBundles;
		private static StreamedContentManager streams;

		private string _rootDirectory = string.Empty;
		private IServiceProvider serviceProvider;
		
		private Dictionary<string, object> loadedAssets = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		private List<IDisposable> disposableAssets = new List<IDisposable>();
		private bool disposed;

		public ContentManager()
		{
		}

		public ContentManager(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}
			this.serviceProvider = serviceProvider;
		}

		public ContentManager(IServiceProvider serviceProvider, string RootDirectory)
		{
			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}
			if (RootDirectory == null)
			{
				throw new ArgumentNullException("rootDirectory");
			}
			this.RootDirectory = RootDirectory;
			this.serviceProvider = serviceProvider;
		}

		private void EnsureMappingsExist()
        {
			if (xnaBundles == null && !Game.IsDummy)
			{
				string streamingAssetsPath;
				if (PlatformInstances.IsEditor)
					streamingAssetsPath = string.Format("file://{0}/", Application.streamingAssetsPath);
				else
#if U_WINDOWS || U_PS4
					streamingAssetsPath = string.Format("file://{0}/", Application.streamingAssetsPath);
#else
					streamingAssetsPath = string.Format("{0}/", Application.streamingAssetsPath);
#endif

				Log.WriteT("Initializing Mapped Content...");
				
                xnaBundles = new BundleManager(streamingAssetsPath, 
					new StringReader(UResources.Load<TextAsset>("AssetBundleMappings").text), 
					true);
				
				streams = new StreamedContentManager(streamingAssetsPath, 
					new StringReader(UResources.Load<TextAsset>("StreamedMappings").text));

				Log.WriteT("Mapped Content Initialized");
			}
		}

		public void UpdateBundleLoading()
		{
			EnsureMappingsExist();

            xnaBundles.UpdateBundleLoading();
			streams.UpdateLoading();
		}

		public void LoadBundle(string bundleName)
        {
			EnsureMappingsExist();

            //XnaWrapper.Debug.Log("loading: " + bundleName);
            xnaBundles.LoadBundle(bundleName);
		}

		public void ReleaseBundle(string bundleName)
        {
			EnsureMappingsExist();

            //XnaWrapper.Debug.Log("unloading: " + bundleName);
            xnaBundles.ReleaseBundle(bundleName);
			// We need to call this because otherwise WWW loaded textures won't be removed from the Unity Scene.
			UnityEngine.Resources.UnloadUnusedAssets();
		}

		public void Dispose()
		{
			Dispose(true);
		}

		// If disposing is true, it was called explicitly and we should dispose managed objects.
		// If disposing is false, it was called by the finalizer and managed objects should not be disposed.
		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					Unload();
				}
				disposed = true;
			}
		}
		
        // Show AOT which types will be used in combination with this function; this function can't ever be called
        public void _dummy_Load()
        {
            this.Load<XTexture>("");
            this.Load<SoundEffect>("");
            this.Load<Song>("");
            this.Load<SpriteFont>("");
            this.Load<string>("");
            this.Load<TextureAtlas>("");
            this.Load<Effect>("");
        }
		
		public T Load<T>(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				throw new ArgumentNullException("assetName");
			}
			if (disposed)
			{
                Console.WriteLine("ContentManager.Load: manager disposed");
				throw new ObjectDisposedException("ContentManager");
			}
			fileName = fileName.ToLower();

			object asset = null;
			if (loadedAssets.TryGetValue(fileName, out asset))
			{
				if (asset is T)
				{
					return (T)asset;
				}
			}

			// Make Unity load the different XNA asset types in the right Unity way
			Type xnaType = typeof(T);
			if (xnaType == typeof(string))
			{
				asset = ((TextAsset)NativeLoad(fileName, typeof(TextAsset))).text;
			}
			else
			{
				Type unityType = GetUnityType(xnaType);
				asset = NativeLoad(fileName, unityType);
			}

            // Convert the Unity asset type to the right XNA asset type
			asset = ConvertAsset(fileName, asset, xnaType);

			loadedAssets[fileName] = asset;
			return (T)asset;
		}

		private Type GetUnityType(Type xnaType)
		{
			Type unityType = null;
			if (xnaType == typeof(XTexture))
				unityType = typeof(UTexture);
			else if (xnaType == typeof(SoundEffect) || xnaType == typeof(Song))
				unityType = typeof(UAudioClip);
			else if (xnaType == typeof(SpriteFont) || xnaType == typeof(string) || xnaType == typeof(TextureAtlas))
				unityType = typeof(TextAsset);
			return unityType;
		}
		
		private ContentItem GetStreamedItem(string fileName)
		{
			EnsureMappingsExist();
			ContentItem item;
			streams.TryGetItem(fileName, out item);
			return item;
		}

		private ContentItem GetBundleItem(string fileName)
        {
			EnsureMappingsExist();
			ContentItem item;
            xnaBundles.TryGetItem(fileName, out item);
            return item;
        }

		public ContentRequest LoadAsync(string fileName, Type type)
		{
			if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("assetName");
            }
            if (disposed)
            {
                Console.WriteLine("ContentManager.Load: manager disposed");
                throw new ObjectDisposedException("ContentManager");
			}
			fileName = fileName.ToLower();

            ContentItem item = GetStreamedItem(fileName);
			if (item != null)
			{
				if (!item.IsActive)
				{
					streams.LoadItem(fileName, GetUnityType(type));
				}

				if (item.Request == null)
					throw new Exception("Unable to find streaming asset file " + fileName);

				return item.Request;
			}

			item = GetBundleItem(fileName);
			if (item != null)
			{
				if (!item.IsActive)
				{
					if (xnaBundles.oneAssetPerBundle)
						LoadBundle(fileName);
					else
						throw new Exception("Asset present in bundles, but has never started loading: " + fileName);
				}

				if (item.Request == null)
					throw new Exception("Unable to find asset bundle file " + fileName);

				return item.Request;
			}

			ContentRequest request = new ContentRequest();

			Type unityType = GetUnityType(type);
			fileName = UnityResourcePath(fileName);
			if (unityType == null)
			{
				Log.Write("ContentManager: LoadAsync: type {0} not defined.", type);
				request.Operation = UResources.LoadAsync(fileName);
			}
			else
			{
				request.Operation = UResources.LoadAsync(fileName, unityType);
			}
			return request;
        }

		public void Unload(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
			{
				throw new ArgumentNullException("assetName");
			}
			if (disposed)
			{
				Console.WriteLine("ContentManager.Unload: manager disposed");
				throw new ObjectDisposedException("ContentManager");
			}
			fileName = fileName.ToLower();

			ContentItem item = GetStreamedItem(fileName);
			if (item != null)
			{
				return;
			}

			item = GetBundleItem(fileName);
			if (item != null)
			{
				if (item.IsActive)
				{
					if (xnaBundles.oneAssetPerBundle)
						ReleaseBundle(fileName);
				}
				return;
			}
		}

		private static string UnityResourcePath(string xnaPath)
		{
			return "Content/" + xnaPath.Replace('\\', '/');
		}

		private UObject NativeLoad(string fileName, Type type)
		{
			UObject res = UResources.Load(UnityResourcePath(fileName), type);

			if (res == null)
                throw new ContentLoadException("Failed to load " + fileName + " as " + type);
			return res;
		}

        /**
         * Convert Unity assets loaded from disk to corresponding XNA asset types
         */
        public object ConvertAsset(string name, object asset, Type type)
        {
            if (type == typeof(Song))
            {
                return new Song((UAudioClip)asset);
            }
            else if (type == typeof(XTexture))
            {
                return new XTexture((UTexture)asset);
            }
            else if (type == typeof(SoundEffect))
            {
                return new SoundEffect((UAudioClip)asset);
            }
            else if (type == typeof(SpriteFont))
            {
                return ReadAsset<SpriteFont>(asset, name, null);
            }
            else if (type == typeof(string))
            {
                return ((TextAsset)asset).text;
            }
            else if (type == typeof(TextureAtlas))
            {
                return ReadAsset<TextureAtlas>(asset, name, null);
            }
            else
			{
				Log.Write("ContentManager: LoadAsync: type {0} not defined.", type);
                return asset;
            }
        }

        public static Stream ReadBytesFileToStream(string fileName)
		{
			fileName = fileName.ToLower().Replace("\\", "/");
            TextAsset binData = UResources.Load(fileName, typeof(TextAsset)) as TextAsset;
            if (binData == null)
            {
                throw new ContentLoadException("Failed to load " + fileName + " as " + typeof(TextAsset));
            }
            return new MemoryStream(binData.bytes);
        }

		public static string ReadTextFile(string assetName)
        {
			TextAsset textasset = UResources.Load(assetName, typeof(TextAsset)) as TextAsset;
			if (textasset == null)
			{
				throw new ContentLoadException("Failed to load " + assetName + " as " + typeof(TextAsset));
			}
			return textasset.text;
        }

		public T ReadAsset<T>(object asset, string assetName, Action<IDisposable> recordDisposableObject)
		{
			if (string.IsNullOrEmpty(assetName))
			{
				throw new ArgumentNullException("assetName");
			}
			if (disposed)
			{
				throw new ObjectDisposedException("ContentManager");
			}

			string originalAssetName = assetName;
			object result = null;

			/* QQQ
			if (this.graphicsDeviceService == null)
			{
				this.graphicsDeviceService = serviceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
				if (this.graphicsDeviceService == null)
				{
					throw new InvalidOperationException("No Graphics Device Service");
				}
			}
			*/
			Stream stream = null;
            TextAsset binData;
            if (asset == null)
                binData = (TextAsset) NativeLoad(assetName, typeof(TextAsset));
            else
                binData = (TextAsset) asset;
                
			stream = new MemoryStream(binData.bytes);

			// Try to load as XNB file
			try
			{
				using (BinaryReader xnbReader = new BinaryReader(stream))
				{
					using (ContentReader reader = GetContentReaderFromXnb(assetName, ref stream, xnbReader, recordDisposableObject))
					{
						result = reader.ReadAsset<T>();
						if (result is GraphicsResource)
							((GraphicsResource)result).Name = originalAssetName;
					}
				}
			}
			finally
			{
				if (stream != null)
				{
					stream.Dispose();
				}
			}
			
			if (result == null)
				throw new ContentLoadException("Could not load " + originalAssetName + " asset!");

			return (T)result;
		}

		/* QQQ
		protected virtual string Normalize<T>(string assetName)
		{
			if (typeof(T) == typeof(Texture2D))
			{
				return Texture2DReader.Normalize(assetName);
			}
			else if ((typeof(T) == typeof(SpriteFont)))
			{
				return SpriteFontReader.Normalize(assetName);
			}
#if !WINRT
			else if ((typeof(T) == typeof(Song)))
			{
				return SongReader.Normalize(assetName);
			}
			else if ((typeof(T) == typeof(SoundEffect)))
			{
				return SoundEffectReader.Normalize(assetName);
			}
#endif
			else if ((typeof(T) == typeof(Effect)))
			{
				return EffectReader.Normalize(assetName);
			}
			
			return null;
		}
		*/
		/* QQQ
		protected virtual object ReadRawAsset<T>(string assetName, string originalAssetName)
		{
			if (typeof(T) == typeof(Texture2D) || typeof(T) == typeof(Texture))
			{
				using (Stream assetStream = TitleContainer.OpenStream(assetName))
				{
					Texture2D texture = Texture2D.FromStream(
						graphicsDeviceService.GraphicsDevice, assetStream);
					texture.Name = originalAssetName;
					return texture;
				}
			}
			else if ((typeof(T) == typeof(SpriteFont)))
			{
				//result = new SpriteFont(Texture2D.FromFile(graphicsDeviceService.GraphicsDevice,assetName), null, null, null, 0, 0.0f, null, null);
				throw new NotImplementedException();
			}
#if !DIRECTX
			else if ((typeof(T) == typeof(Song)))
			{
				return new Song(assetName);
			}
			else if ((typeof(T) == typeof(SoundEffect)))
			{
				using (Stream s = TitleContainer.OpenStream(assetName))
					return SoundEffect.FromStream(s);
			}
#endif
			else if ((typeof(T) == typeof(Effect)))
			{
				using (Stream assetStream = TitleContainer.OpenStream(assetName))
				{
					var data = new byte[assetStream.Length];
					assetStream.Read(data, 0, (int)assetStream.Length);
					return new Effect(this.graphicsDeviceService.GraphicsDevice, data);
				}
			}
			return null;
		}
		*/
		private ContentReader GetContentReaderFromXnb(string originalAssetName, ref Stream stream, BinaryReader xnbReader, Action<IDisposable> recordDisposableObject)
		{
			// The first 4 bytes should be the "XNB" header. i use that to detect an invalid file
			byte x = xnbReader.ReadByte();
			byte n = xnbReader.ReadByte();
			byte b = xnbReader.ReadByte();
			byte platform = xnbReader.ReadByte();

			if (x != 'X' || n != 'N' || b != 'B')
			{
				throw new ContentLoadException("Asset does not appear to be a valid XNB file. Did you process your content for Windows?");
			}

			byte version = xnbReader.ReadByte();
			byte flags = xnbReader.ReadByte();

			bool compressedLzx = (flags & ContentCompressedLzx) != 0;
			bool compressedLz4 = (flags & ContentCompressedLz4) != 0;
			if (version != 5 && version != 4)
			{
				throw new ContentLoadException("Invalid XNB version");
			}

			// The next int32 is the length of the XNB file
			int xnbLength = xnbReader.ReadInt32();

			ContentReader reader;
			if (compressedLzx || compressedLz4)
			{
				// Decompress the xnb
				int decompressedSize = xnbReader.ReadInt32();
				MemoryStream decompressedStream = null;

				if (compressedLzx)
				{
					//thanks to ShinAli (https://bitbucket.org/alisci01/xnbdecompressor)
					// default window size for XNB encoded files is 64Kb (need 16 bits to represent it)
					LzxDecoder dec = new LzxDecoder(16);
					decompressedStream = new MemoryStream(decompressedSize);
					int compressedSize = xnbLength - 14;
					long startPos = stream.Position;
					long pos = startPos;

					while (pos - startPos < compressedSize)
					{
						// the compressed stream is seperated into blocks that will decompress
						// into 32Kb or some other size if specified.
						// normal, 32Kb output blocks will have a short indicating the size
						// of the block before the block starts
						// blocks that have a defined output will be preceded by a byte of value
						// 0xFF (255), then a short indicating the output size and another
						// for the block size
						// all shorts for these cases are encoded in big endian order
						int hi = stream.ReadByte();
						int lo = stream.ReadByte();
						int block_size = (hi << 8) | lo;
						int frame_size = 0x8000; // frame size is 32Kb by default
						// does this block define a frame size?
						if (hi == 0xFF)
						{
							hi = lo;
							lo = (byte)stream.ReadByte();
							frame_size = (hi << 8) | lo;
							hi = (byte)stream.ReadByte();
							lo = (byte)stream.ReadByte();
							block_size = (hi << 8) | lo;
							pos += 5;
						}
						else
							pos += 2;

						// either says there is nothing to decode
						if (block_size == 0 || frame_size == 0)
							break;

						dec.Decompress(stream, block_size, decompressedStream, frame_size);
						pos += block_size;

						// reset the position of the input just incase the bit buffer
						// read in some unused bytes
						stream.Seek(pos, SeekOrigin.Begin);
					}

					if (decompressedStream.Position != decompressedSize)
					{
						throw new ContentLoadException("Decompression of " + originalAssetName + " failed. ");
					}

					decompressedStream.Seek(0, SeekOrigin.Begin);
				}
				else if (compressedLz4)
				{
					// Decompress to a byte[] because Windows 8 doesn't support MemoryStream.GetBuffer()
					var buffer = new byte[decompressedSize];
					using (var decoderStream = new Lz4DecoderStream(stream))
					{
						if (decoderStream.Read(buffer, 0, buffer.Length) != decompressedSize)
						{
							throw new ContentLoadException("Decompression of " + originalAssetName + " failed. ");
						}
					}
					// Creating the MemoryStream with a byte[] shares the buffer so it doesn't allocate any more memory
					decompressedStream = new MemoryStream(buffer);
				}

				reader = new ContentReader(this, decompressedStream, originalAssetName, version, recordDisposableObject);
			}
			else
			{
				reader = new ContentReader(this, stream, originalAssetName, version, recordDisposableObject);
			}
			return reader;
		}

		internal void RecordDisposable(IDisposable disposable)
		{
			//Debug.Assert(disposable != null, "The disposable is null!");

			// Avoid recording disposable objects twice. ReloadAsset will try to record the disposables again.
			// We don't know which asset recorded which disposable so just guard against storing multiple of the same instance.
			if (!disposableAssets.Contains(disposable))
				disposableAssets.Add(disposable);
		}

		/// <summary>
		/// Virtual property to allow a derived ContentManager to have it's assets reloaded
		/// </summary>
		protected virtual Dictionary<string, object> LoadedAssets
		{
			get { return loadedAssets; }
		}

		/* QQQ
		protected virtual void ReloadGraphicsAssets()
		{
			foreach (var asset in LoadedAssets)
			{
				// This never executes as asset.Key is never null.  This just forces the 
				// linker to include the ReloadAsset function when AOT compiled.
				if (asset.Key == null)
					ReloadAsset(asset.Key, Convert.ChangeType(asset.Value, asset.Value.GetType()));

#if WINDOWS_STOREAPP
				var methodInfo = typeof(ContentManager).GetType().GetTypeInfo().GetDeclaredMethod("ReloadAsset");
#else
				var methodInfo = typeof(ContentManager).GetMethod("ReloadAsset", BindingFlags.NonPublic | BindingFlags.Instance);
#endif
				var genericMethod = methodInfo.MakeGenericMethod(asset.Value.GetType());
				genericMethod.Invoke(this, new object[] { asset.Key, Convert.ChangeType(asset.Value, asset.Value.GetType()) });
			}
		}

		protected virtual void ReloadAsset<T>(string originalAssetName, T currentAsset)
		{
			string assetName = originalAssetName;
			if (string.IsNullOrEmpty(assetName))
			{
				throw new ArgumentNullException("assetName");
			}
			if (disposed)
			{
				throw new ObjectDisposedException("ContentManager");
			}

			if (this.graphicsDeviceService == null)
			{
				this.graphicsDeviceService = serviceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
				if (this.graphicsDeviceService == null)
				{
					throw new InvalidOperationException("No Graphics Device Service");
				}
			}

			Stream stream = null;
			try
			{
				//try load it traditionally
				stream = OpenStream(assetName);

				// Try to load as XNB file
				try
				{
					using (BinaryReader xnbReader = new BinaryReader(stream))
					{
						using (ContentReader reader = GetContentReaderFromXnb(assetName, ref stream, xnbReader, null))
						{
							reader.InitializeTypeReaders();
							reader.ReadObject<T>(currentAsset);
							reader.ReadSharedResources();
						}
					}
				}
				finally
				{
					if (stream != null)
					{
						stream.Dispose();
					}
				}
			}
			catch (ContentLoadException)
			{
				// Try to reload as a non-xnb file.
				// Just textures supported for now.

				assetName = TitleContainer.GetFilename(Path.Combine(RootDirectory, assetName));

				assetName = Normalize<T>(assetName);

				ReloadRawAsset(currentAsset, assetName, originalAssetName);
			}
		}

		protected virtual void ReloadRawAsset<T>(T asset, string assetName, string originalAssetName)
		{
			if (asset is Texture2D)
			{
				using (Stream assetStream = TitleContainer.OpenStream(assetName))
				{
					var textureAsset = asset as Texture2D;
					textureAsset.Reload(assetStream);
				}
			}
		}
		*/
		public virtual void Unload()
		{
			// Look for disposable assets.
			foreach (var disposable in disposableAssets)
			{
				if (disposable != null)
					disposable.Dispose();
			}
			disposableAssets.Clear();
			loadedAssets.Clear();
		}

		public string RootDirectory
		{
			get
			{
				return _rootDirectory;
			}
			set
			{
				_rootDirectory = value;
			}
		}
		/* QQQ
		internal string RootDirectoryFullPath
		{
			get
			{
				return Path.Combine(TitleContainer.Location, RootDirectory);
			}
		}
		*/
		public IServiceProvider ServiceProvider
		{
			get
			{
				return this.serviceProvider;
			}
		}

	}
}
