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
		{ }

		public Texture(UnityEngine.Texture texture)
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
	}
}
