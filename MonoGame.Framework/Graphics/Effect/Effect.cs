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

namespace DDG.XnaWrapper
{
	public abstract class MaterialProperty
	{
		internal string propName;
		internal int propID = -1;

		protected MaterialProperty(string propName)
		{
			this.propName = propName;
		}

		public void ApplyToMaterial(Material mat)
		{
			if (propID == -1)
				propID = Shader.PropertyToID(propName);

			SetToMaterial(mat);
		}

		protected abstract void SetToMaterial(Material mat);
	}

	public class MatrixProperty : MaterialProperty
	{
		public MatrixProperty(string propName) : base(propName) {}

		public Matrix4x4 Value = new Matrix4x4();

		protected override void SetToMaterial(Material mat)
		{
			mat.SetMatrix(propID, Value);
		}
	}

	public class IntegerProperty : MaterialProperty
	{
		public IntegerProperty(string propName) : base(propName) { }

		public int Value = 0;

		protected override void SetToMaterial(Material mat)
		{
			mat.SetInt(propID, Value);
		}
	}

}
