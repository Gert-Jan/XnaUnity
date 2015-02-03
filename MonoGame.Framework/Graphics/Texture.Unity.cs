using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class Texture
	{
		protected UnityEngine.Texture texture;

		public UnityEngine.Texture UnityTexture
		{
			get { return (UnityEngine.Texture)texture; }
			private set { texture = value; }
		}

		public Texture()
		{ }

		public Texture(UnityEngine.Texture texture)
		{
			if (texture == null)
				throw new ArgumentNullException("texture");
			this.UnityTexture = texture;
			this._format = SurfaceFormat.Color;
		}

		private void PlatformGraphicsDeviceResetting()
		{
			throw new NotImplementedException();
		}
	}
}
