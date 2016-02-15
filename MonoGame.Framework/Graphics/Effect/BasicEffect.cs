#region File Description
//-----------------------------------------------------------------------------
// BasicEffect.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using XnaWrapper;

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// Built-in effect that supports optional texturing, vertex coloring, fog, and lighting.
	/// </summary>
	public class BasicEffect : Effect, IEffectMatrices
	{
		bool textureEnabled;
		bool vertexColorEnabled;

		Texture2D texture = null;

		Matrix world = Matrix.Identity;
		Matrix view = Matrix.Identity;
		Matrix projection = Matrix.Identity;

		Matrix worldView = new Matrix();
		Matrix worldViewProj = new Matrix();

		Vector3 diffuseColor = Vector3.One;

		EffectDirtyFlags dirtyFlags = EffectDirtyFlags.All;

		private static readonly UnityEngine.Shader shader = UnityEngine.Shader.Find("Custom/SpriteShader");

		private UnityEngine.Material material;
		
		#region Public Properties

		internal override UnityEngine.Material Material
		{
			get { return material; }
			set { this.material = value; }
		}

		/// <summary>
		/// Gets or sets the world matrix.
		/// </summary>
		public Matrix World
		{
			get { return world; }

			set
			{
				world = value;
				dirtyFlags |= EffectDirtyFlags.World | EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.Fog;
			}
		}


		/// <summary>
		/// Gets or sets the view matrix.
		/// </summary>
		public Matrix View
		{
			get { return view; }

			set
			{
				view = value;
				dirtyFlags |= EffectDirtyFlags.WorldViewProj | EffectDirtyFlags.EyePosition | EffectDirtyFlags.Fog;
			}
		}


		/// <summary>
		/// Gets or sets the projection matrix.
		/// </summary>
		public Matrix Projection
		{
			get { return projection; }

			set
			{
				projection = value;
				dirtyFlags |= EffectDirtyFlags.WorldViewProj;
			}
		}


		/// <summary>
		/// Gets or sets the material diffuse color (range 0 to 1).
		/// </summary>
		public Vector3 DiffuseColor
		{
			get { return diffuseColor; }

			set
			{
				diffuseColor = value;
				dirtyFlags |= EffectDirtyFlags.MaterialColor;
			}
		}

		/// <summary>
		/// Gets or sets whether texturing is enabled.
		/// </summary>
		public bool TextureEnabled
		{
			get { return textureEnabled; }

			set
			{
				if (textureEnabled != value)
				{
					textureEnabled = value;
					dirtyFlags |= EffectDirtyFlags.ShaderIndex;
				}
			}
		}

		public Texture2D Texture
		{
			set
			{
				texture = value;
            }
		}

		/// <summary>
		/// Gets or sets whether vertex color is enabled.
		/// </summary>
		public bool VertexColorEnabled
		{
			get { return vertexColorEnabled; }

			set
			{
				if (vertexColorEnabled != value)
				{
					vertexColorEnabled = value;
					dirtyFlags |= EffectDirtyFlags.ShaderIndex;
				}
			}
		}

		public float Alpha
		{
			get;
			set;
		}
		#endregion

		#region Methods

		/// <summary>
		/// Creates a new BasicEffect with default parameter settings.
		/// </summary>
		public BasicEffect(GraphicsDevice device) : base(device)
		{
			material = new UnityEngine.Material(shader);
		}

		/// <summary>
		/// Creates a new BasicEffect by cloning parameter settings from an existing instance.
		/// </summary>
		//protected BasicEffect(BasicEffect cloneSource)
		//{
		//	textureEnabled = cloneSource.textureEnabled;
		//	vertexColorEnabled = cloneSource.vertexColorEnabled;
		//
		//	world = cloneSource.world;
		//	view = cloneSource.view;
		//	projection = cloneSource.projection;
		//
		//	diffuseColor = cloneSource.diffuseColor;
		//
		//	base.Material = new UnityEngine.Material(shader);
		//}

		/// <summary>
		/// Creates a clone of the current BasicEffect instance.
		/// </summary>
		//public Effect Clone()
		//{
		//	return new BasicEffect(this);
		//}

		private MatrixProperty worldViewProj_Property = new MatrixProperty("_WorldViewProj");
		private IntegerProperty Font_Property = new IntegerProperty("_IsFont");

		internal override void OnApplyPostTexture()
		{
			if (device.Textures[0].IsFontTexture)
				Font_Property.Value = 1;
			else
				Font_Property.Value = 0;
			Font_Property.ApplyToMaterial(material);
		}

		/// <summary>
		/// Lazily computes derived parameter values immediately before applying the effect.
		/// </summary>
		internal override bool OnApply()
		{
			// Recompute the world+view+projection matrix or fog vector?
			dirtyFlags = EffectHelpers.SetWorldViewProjAndFog(dirtyFlags, ref world, ref view, ref projection, ref worldView, ref worldViewProj);

			if (texture != null)
				device.Textures[0] = texture;

			if (dirtyFlags != 0)
			{
				XnaToUnity.Matrix(worldViewProj, out worldViewProj_Property.Value);
				worldViewProj_Property.Value.m23 = 0;

				worldViewProj_Property.ApplyToMaterial(material);
			}

			return false;
		}


		#endregion
	}
}
