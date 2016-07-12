using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class Texture
	{
		protected UnityEngine.Texture texture;
		protected bool isFontTexture;

		public UnityEngine.Texture UnityTexture { get { return texture; } }
		public bool IsFontTexture { get { return isFontTexture; } }

		public Texture()
		{
			Disposing += OnDispose;
		}

		public Texture(UnityEngine.Texture texture) :
			this()
		{
			if (texture == null)
				throw new ArgumentNullException("texture");
			this.texture = texture;
			isFontTexture = texture.name == "Font Texture";
			_format = SurfaceFormat.Color;
		}

		private void PlatformGraphicsDeviceResetting()
		{
			throw new NotImplementedException();
		}

		private void OnDispose(object sender, EventArgs e)
		{
			UnityEngine.Object.Destroy(texture);
			Disposing -= OnDispose;
		}
	}
}
