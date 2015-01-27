#region File Description
//-----------------------------------------------------------------------------
// BasicEffect.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// Built-in effect that supports optional texturing, vertex coloring, fog, and lighting.
	/// </summary>
	public class BasicEffect : Effect, IEffectMatrices
	{
		#region Effect Parameters

		EffectParameter diffuseColorParam;
		EffectParameter worldViewProjParam;

		#endregion

		#region Fields

		bool textureEnabled;
		bool vertexColorEnabled;

		Matrix world = Matrix.Identity;
		Matrix view = Matrix.Identity;
		Matrix projection = Matrix.Identity;

		Matrix worldView = new Matrix();
		Matrix worldViewProj = new Matrix();

		Vector3 diffuseColor = Vector3.One;

		EffectDirtyFlags dirtyFlags = EffectDirtyFlags.All;

		private static readonly UnityEngine.Shader shader = UnityEngine.Shader.Find("Custom/SpriteShader");
		private UnityEngine.Material material = new UnityEngine.Material(shader);

		#endregion

		#region Public Properties

		public override UnityEngine.Material Material
		{
			get { return material; }
			protected set { this.material = value; }
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
		public BasicEffect(GraphicsDevice device)
		{
			base.Material = new UnityEngine.Material(shader);
		}

		/// <summary>
		/// Creates a new BasicEffect by cloning parameter settings from an existing instance.
		/// </summary>
		protected BasicEffect(BasicEffect cloneSource)
		{
			textureEnabled = cloneSource.textureEnabled;
			vertexColorEnabled = cloneSource.vertexColorEnabled;

			world = cloneSource.world;
			view = cloneSource.view;
			projection = cloneSource.projection;

			diffuseColor = cloneSource.diffuseColor;

			base.Material = new UnityEngine.Material(shader);
		}

		/// <summary>
		/// Creates a clone of the current BasicEffect instance.
		/// </summary>
		public Effect Clone()
		{
			return new BasicEffect(this);
		}

		/// <summary>
		/// Lazily computes derived parameter values immediately before applying the effect.
		/// </summary>
		internal override bool OnApply()
		{
			// Recompute the world+view+projection matrix or fog vector?
			dirtyFlags = EffectHelpers.SetWorldViewProjAndFog(dirtyFlags, ref world, ref view, ref projection, ref worldView, ref worldViewProj);
			UnityEngine.Matrix4x4 uWorldViewProj = XnaToUnity.Matrix(worldViewProj);
			uWorldViewProj.m23 = 0;

			material.SetMatrix("_WorldViewProj", uWorldViewProj);
			//XX: set transform on material

			// Recompute the diffuse/emissive/alpha material color parameters?
			/*
			if ((dirtyFlags & EffectDirtyFlags.MaterialColor) != 0)
			{
				EffectHelpers.SetMaterialColor(ref diffuseColor, diffuseColorParam);

				dirtyFlags &= ~EffectDirtyFlags.MaterialColor;
			}

			// Recompute the shader index?
			if ((dirtyFlags & EffectDirtyFlags.ShaderIndex) != 0)
			{
				int shaderIndex = 0;

				if (true)
					shaderIndex += 1;

				if (vertexColorEnabled)
					shaderIndex += 2;

				if (textureEnabled)
					shaderIndex += 4;
			}
			*/
			return false;
		}


		#endregion
	}
}
