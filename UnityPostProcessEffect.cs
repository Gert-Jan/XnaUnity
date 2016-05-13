using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XnaWrapper
{
	/// <summary>
	/// UnityPostProcessingEffect. Based on BasicEffect.
	/// </summary>
	public class UnityPostProcessEffect : Effect, IEffectMatrices
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
		public UnityPostProcessEffect(GraphicsDevice device, string shaderName) : base(device)
		{
            UnityEngine.Shader shader = UnityEngine.Shader.Find(shaderName);
			material = new UnityEngine.Material(shader);
		}

		private MatrixProperty worldViewProj_Property = new MatrixProperty("_WorldViewProj");

		internal override void OnApplyPostTexture()
		{
		}

		/// <summary>
		/// Lazily computes derived parameter values immediately before applying the effect.
		/// </summary>
		internal override bool OnApply()
		{
			UnityEngine.Debug.Log("UnityPostProcessEffect.OnApply");
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
