using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using UnityTexture = UnityEngine.Texture2D;
using UnityAudioClip = UnityEngine.AudioClip;
using UnityResources = UnityEngine.Resources;
using TextAsset = UnityEngine.TextAsset;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Utilities;
using Lz4;
using TextureAtlasContent;
using UnityEngine;

namespace Microsoft.Xna.Framework.Content
{
	public class ContentManager
	{
		const byte ContentCompressedLzx = 0x80;
		const byte ContentCompressedLz4 = 0x40;

		private string _rootDirectory = string.Empty;
		private IServiceProvider serviceProvider;
		//QQQ private IGraphicsDeviceService graphicsDeviceService;
		private Dictionary<string, object> loadedAssets = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		private List<IDisposable> disposableAssets = new List<IDisposable>();
		private bool disposed;

		public ContentManager()
		{ }

		public ContentManager(IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}
			this.serviceProvider = serviceProvider;
		}

		public ContentManager(IServiceProvider serviceProvider, string rootDirectory)
		{
			if (serviceProvider == null)
			{
				throw new ArgumentNullException("serviceProvider");
			}
			if (rootDirectory == null)
			{
				throw new ArgumentNullException("rootDirectory");
			}
			this.RootDirectory = rootDirectory;
			this.serviceProvider = serviceProvider;
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

        public Microsoft.Xna.Framework.Graphics.Texture2D LoadTexture2D(string fileName)
        {
            return new Microsoft.Xna.Framework.Graphics.Texture2D(NativeLoad<UnityTexture>(fileName));
        }

        public SoundEffect LoadSoundEffect(string fileName)
        {
            return new SoundEffect(NativeLoad<UnityAudioClip>(fileName));
        }

        public Song LoadSong(string fileName)
        {
            return new Song(NativeLoad<UnityAudioClip>(fileName));
        }

        public SpriteFont LoadSpriteFont(string fileName)
        {
            return ReadAsset<SpriteFont>(fileName, null);
        }

        public string LoadText(string fileName)
        {
            return (NativeLoad<TextAsset>(fileName)).text;
        }

        public TextureAtlas LoadTextureAtlas(string fileName)
        {
            return ReadAsset<TextureAtlas>(fileName, null);
        }

		public virtual T Load<T>(string fileName) where T : class
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

			object asset = null;
			if (loadedAssets.TryGetValue(fileName, out asset))
			{
				if (asset is T)
				{
					return (T)asset;
				}
			}

			if (typeof(T) == typeof(Microsoft.Xna.Framework.Graphics.Texture2D))
			{
				asset = new Microsoft.Xna.Framework.Graphics.Texture2D(NativeLoad<UnityTexture>(fileName));
			}
			if (typeof(T) == typeof(SoundEffect))
			{
				asset = new SoundEffect(NativeLoad<UnityAudioClip>(fileName));
			}
			if (typeof(T) == typeof(Song))
			{
				asset = new Song(NativeLoad<UnityAudioClip>(fileName));
			}
			if (typeof(T) == typeof(SpriteFont))
			{
				asset = ReadAsset<SpriteFont>(fileName, null);
				//asset = new SpriteFont(NativeLoad<UnityEngine.TextAsset>(fileName));
			}
			if (typeof(T) == typeof(string))
			{
				asset = (NativeLoad<TextAsset>(fileName)).text;
			}
            if (typeof(T) == typeof(TextureAtlas))
            {
                asset = ReadAsset<TextureAtlas>(fileName, null);
            }

			loadedAssets[fileName] = asset;
			return asset as T;
		}

		private T NativeLoad<T>(string fileName) where T : class
		{
            var res = UnityResources.Load(fileName, typeof(T)) as T;
			if (res == null)
			{
                Console.WriteLine("Failed to load " + fileName + " as " + typeof(T));
				throw new ContentLoadException("Failed to load " + fileName + " as " + typeof(T));
			}
			return res;
		}

        public static Stream ReadBytesFileToStream(string assetName)
        {
            TextAsset binData = UnityResources.Load(assetName, typeof(TextAsset)) as TextAsset;
            if (binData == null)
            {
                throw new ContentLoadException("Failed to load " + assetName + " as " + typeof(TextAsset));
            }
            return new MemoryStream(binData.bytes);
        }

        public static string ReadTextFile(string textname)
        {
            TextAsset file = Resources.Load("english") as TextAsset;
            return file.text;
        }

		public T ReadAsset<T>(string assetName, Action<IDisposable> recordDisposableObject)
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
			TextAsset binData = NativeLoad<UnityEngine.TextAsset>(assetName);
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
