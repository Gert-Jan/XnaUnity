using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class RenderTarget2D
	{
		private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
		{
			throw new NotImplementedException();
		}

		private void PlatformGraphicsDeviceResetting()
		{
			throw new NotImplementedException();
		}
	}
}
