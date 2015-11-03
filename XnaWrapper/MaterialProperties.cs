using System;
using UnityEngine;

namespace XnaWrapper
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
		public MatrixProperty(string propName) : base(propName) { }

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
