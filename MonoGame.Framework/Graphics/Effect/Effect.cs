using System;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.Xna.Framework.Graphics
{
	public class Effect
	{
		protected readonly GraphicsDevice device;
		public Effect(GraphicsDevice device)
		{
			this.device = device;
		}

		public EffectTechnique CurrentTechnique { get; set; }

		public virtual Material Material { get; protected set; }

		// called after this effect is set to be used, but before textures are known
		internal virtual bool OnApply()
		{
			return false;
		}

		// called after a texture is bound to the device, so texture dependent properties can be set. (in other words, just before the draw command itself)
		internal virtual void OnApplyPostTexture() { }
	}
}
