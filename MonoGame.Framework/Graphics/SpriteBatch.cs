using System;
using System.Text;
using UnityEngine;

namespace Microsoft.Xna.Framework.Graphics
{
	public class SpriteBatch : GraphicsResource
	{
	    readonly SpriteBatcher _batcher;

		SpriteSortMode _sortMode;
		//BlendState _blendState;
		//SamplerState _samplerState;
		//DepthStencilState _depthStencilState; 
		//RasterizerState _rasterizerState;		
		Effect _effect;
        bool _beginCalled;

		BasicEffect basicEffect;

		//Effect _spriteEffect;
	    //readonly EffectParameter _matrixTransform;
        //readonly EffectPass _spritePass;

		Matrix _matrix;
		Rectangle _tempRect = new Rectangle (0,0,0,0);
		Vector2 _texCoordTL = new Vector2 (0,0);
		Vector2 _texCoordBR = new Vector2 (0,0);
        //private int logCount;

        public Matrix tempProjMatrix; //TODO make this a property
        
		public SpriteBatch (GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null) {
				throw new ArgumentException ("graphicsDevice");
			}	

			this.GraphicsDevice = graphicsDevice;

            // Use a custom SpriteEffect so we can control the transformation matrix
            //_spriteEffect = new Effect(graphicsDevice, SpriteEffect.Bytecode);
            //_matrixTransform = _spriteEffect.Parameters["MatrixTransform"];
            //_spritePass = _spriteEffect.CurrentTechnique.Passes[0];

            _batcher = new SpriteBatcher(graphicsDevice);

            _beginCalled = false;
            
			basicEffect = new BasicEffect(graphicsDevice);
		}

		public void Begin ()
		{
            Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);
		}

		public void Begin (SpriteSortMode sortMode, BlendState? blendState, SamplerState? samplerState, DepthStencilState? depthStencilState, RasterizerState? rasterizerState, Effect effect, Matrix transformMatrix)
		{
            if (_beginCalled)
                throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");

			// defaults
			_sortMode = sortMode;
			//_blendState = blendState ?? BlendState.AlphaBlend;
			//_samplerState = samplerState ?? SamplerState.LinearClamp;
			//_depthStencilState = depthStencilState ?? DepthStencilState.None;
			//_rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
			//

			if (effect == null)
			{
				basicEffect.Projection = GraphicsDevice.DefaultProjection;
				basicEffect.View = Matrix.Identity;
				effect = basicEffect;
			}
			else if (effect is BasicEffect)
			{
				BasicEffect uEffect = (BasicEffect)effect;
				if(uEffect.Projection == Matrix.Identity)
				{ 
					uEffect.Projection = GraphicsDevice.DefaultProjection;
					uEffect.View = Matrix.Identity;
				}
				effect = uEffect;
			}

			_effect = effect;
			
			_matrix = transformMatrix;

            // Setup things now so a user can chage them.
            if (sortMode == SpriteSortMode.Immediate)
				Setup();

            _beginCalled = true;
		}

		public void Begin (SpriteSortMode sortMode, BlendState? blendState)
		{
			Begin (sortMode, blendState, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);			
		}

		public void Begin (SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState)
		{
			Begin (sortMode, blendState, samplerState, depthStencilState, rasterizerState, null, Matrix.Identity);
		}

		public void Begin (SpriteSortMode sortMode, BlendState? blendState, SamplerState? samplerState, DepthStencilState? depthStencilState, RasterizerState? rasterizerState, Effect effect)
		{
			Begin (sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, Matrix.Identity);
		}

		public void End ()
		{	
			_beginCalled = false;

			if (_sortMode != SpriteSortMode.Immediate)
				Setup();
#if PSM   
            GraphicsDevice.BlendState = _blendState;
            _blendState.ApplyState(GraphicsDevice);
#endif
			_batcher.DrawBatch(_sortMode, _effect);
			_effect = null;

			GraphicsDevice.activeEffect = null;
        }
		
		void Setup() 
        {
            //var gd = GraphicsDevice;
			//gd.BlendState = _blendState;
			//gd.DepthStencilState = _depthStencilState;
			//gd.RasterizerState = _rasterizerState;
			//gd.SamplerStates[0] = _samplerState;

			// Setup the default sprite effect.
			GraphicsDevice.activeEffect = _effect;
			GraphicsDevice.activeEffect.OnApply();

			//var vp = gd.Viewport;

			//Matrix projection;
#if PSM || DIRECTX
            Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, -1, 0, out projection);
#else
            // GL requires a half pixel offset to match DX.
			//XX: actaully create a projection matrix here
			//if (_effect == null)
			//{
			//	Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, 1, out projection);
			//	if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
			//	{
			//		_matrix.M41 += -0.5f * _matrix.M11;
			//		_matrix.M42 += -0.5f * _matrix.M22;
			//	}
			//
			//	Matrix.Multiply(ref _matrix, ref projection, out projection);
			//
			//	_matrix = projection;
			//}
#endif
            
            //_matrix.SetValue(projection);
            //_spritePass.Apply();

			// XX: Set custom effect on graphics device
			//if (_effect != null)
			//	gd.Material = _effect.Material;
			//gd.Matrix = _matrix;

			// If the user supplied a custom effect then apply
			// it now to override the sprite effect.
			//if (_effect != null)
			//	_effect.OnApply();
			//	_effect.CurrentTechnique.Passes[0].Apply();
        }
		
        void CheckValid(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");
            if (!_beginCalled)
                throw new InvalidOperationException("Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
        }

		void CheckValid(SpriteFont spriteFont, string text)
        {
            if (spriteFont == null)
                throw new ArgumentNullException("spriteFont");
            if (text == null)
                throw new ArgumentNullException("text");
            if (!_beginCalled)
                throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
        }

        void CheckValid(SpriteFont spriteFont, StringBuilder text)
        {
            if (spriteFont == null)
                throw new ArgumentNullException("spriteFont");
            if (text == null)
                throw new ArgumentNullException("text");
            if (!_beginCalled)
                throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
        }

        // Overload for calling Draw() with named parameters
        /// <summary>
        /// This is a MonoGame Extension method for calling Draw() using named parameters.  It is not available in the standard XNA Framework.
        /// </summary>
        /// <param name='texture'>
        /// The Texture2D to draw.  Required.
        /// </param>
        /// <param name='position'>
        /// The position to draw at.  If left empty, the method will draw at drawRectangle instead.
        /// </param>
        /// <param name='drawRectangle'>
        /// The rectangle to draw at.  If left empty, the method will draw at position instead.
        /// </param>
        /// <param name='sourceRectangle'>
        /// The source rectangle of the texture.  Default is null
        /// </param>
        /// <param name='origin'>
        /// Origin of the texture.  Default is Vector2.Zero
        /// </param>
        /// <param name='rotation'>
        /// Rotation of the texture.  Default is 0f
        /// </param>
        /// <param name='scale'>
        /// The scale of the texture as a Vector2.  Default is Vector2.One
        /// </param>
        /// <param name='color'>
        /// Color of the texture.  Default is Color.White
        /// </param>
        /// <param name='effect'>
        /// SpriteEffect to draw with.  Default is SpriteEffects.None
        /// </param>
        /// <param name='depth'>
        /// Draw depth.  Default is 0f.
        /// </param>
        public void Draw (Texture2D texture,
                Vector2? position = null,
                Rectangle? drawRectangle = null,
                Rectangle? sourceRectangle = null,
                Vector2? origin = null,
                float rotation = 0f,
                Vector2? scale = null,
                Color? color = null,
                SpriteEffects effect = SpriteEffects.None,
                float depth = 0f)
        {

            // Assign default values to null parameters here, as they are not compile-time constants
            if(!color.HasValue)
                color = Color.White;
            if(!origin.HasValue)
                origin = Vector2.Zero;
            if(!scale.HasValue)
                scale = Vector2.One;

            // If both drawRectangle and position are null, or if both have been assigned a value, raise an error
            if((drawRectangle.HasValue) == (position.HasValue))
            {
                throw new InvalidOperationException("Expected drawRectangle or position, but received neither or both.");
            }
            else if(position != null)
            {
                // Call Draw() using position
                Draw(texture, (Vector2)position, sourceRectangle, (Color)color, rotation, (Vector2)origin, (Vector2)scale, effect, depth);
            }
            else
            {
                // Call Draw() using drawRectangle
                Draw(texture, (Rectangle)drawRectangle, sourceRectangle, (Color)color, rotation, (Vector2)origin, effect, depth);
            }
        }


		public void Draw (Texture2D texture,
				Vector2 position,
				Rectangle? sourceRectangle,
				Color color,
				float rotation,
				Vector2 origin,
				Vector2 scale,
				SpriteEffects effect,
				float depth)
		{
            CheckValid(texture);

            var w = texture.Width * scale.X;
            var h = texture.Height * scale.Y;
            if (sourceRectangle.HasValue)
            {
                w = sourceRectangle.Value.Width * scale.X;
                h = sourceRectangle.Value.Height * scale.Y;
            }

            DrawInternal(texture,
				new Vector4(position.X, position.Y, w, h),
				sourceRectangle,
				color,
				rotation,
				origin * scale,
				effect,
				depth,
				true);
		}

		public void Draw (Texture2D texture,
				Vector2 position,
				Rectangle? sourceRectangle,
				Color color,
				float rotation,
				Vector2 origin,
				float scale,
				SpriteEffects effect,
				float depth)
		{
            CheckValid(texture);

            var w = texture.Width * scale;
            var h = texture.Height * scale;
            if (sourceRectangle.HasValue)
            {
                w = sourceRectangle.Value.Width * scale;
                h = sourceRectangle.Value.Height * scale;
            }

            DrawInternal(texture,
                new Vector4(position.X, position.Y, w, h),
				sourceRectangle,
				color,
				rotation,
				origin * scale,
				effect,
				depth,
				true);
		}

		public void Draw (Texture2D texture,
			Rectangle destinationRectangle,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			SpriteEffects effect,
			float depth)
		{
            CheckValid(texture);

            DrawInternal(texture,
			      new Vector4(destinationRectangle.X,
			                  destinationRectangle.Y,
			                  destinationRectangle.Width,
			                  destinationRectangle.Height),
			      sourceRectangle,
			      color,
			      rotation,
			      new Vector2(origin.X * ((float)destinationRectangle.Width / (float)( (sourceRectangle.HasValue && sourceRectangle.Value.Width != 0) ? sourceRectangle.Value.Width : texture.Width)),
                        			origin.Y * ((float)destinationRectangle.Height) / (float)( (sourceRectangle.HasValue && sourceRectangle.Value.Height != 0) ? sourceRectangle.Value.Height : texture.Height)),
			      effect,
			      depth,
				  true);
		}

		internal void DrawInternal (Texture2D texture,
			Vector4 destinationRectangle,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			SpriteEffects effect,
			float depth,
			bool autoFlush)
		{
			var item = _batcher.CreateBatchItem();

			item.Depth = depth;
			item.Texture = texture;
			int iTexWidth = texture.width;
			int iTexHeight = texture.height;
			float fTexWidth = iTexWidth;
			float fTexHeight = iTexHeight;

			if (sourceRectangle.HasValue) {
				_tempRect = sourceRectangle.Value;
			} else {
				_tempRect.X = 0;
				_tempRect.Y = 0;
				_tempRect.Width = iTexWidth;
				_tempRect.Height = iTexHeight;				
			}
			
			_texCoordTL.X = _tempRect.X / fTexWidth;
			_texCoordTL.Y = _tempRect.Y / fTexHeight;
			_texCoordBR.X = (_tempRect.X + _tempRect.Width) / fTexWidth;
			_texCoordBR.Y = (_tempRect.Y + _tempRect.Height) / fTexHeight;

            if ((effect & SpriteEffects.FlipVertically) != 0)
            {
                var temp2 = _texCoordBR.Y;
                _texCoordBR.Y = _texCoordTL.Y;
                _texCoordTL.Y = temp2;
            }
            if ((effect & SpriteEffects.FlipHorizontally) != 0)
            {
                var temp = _texCoordBR.X;
                _texCoordBR.X = _texCoordTL.X;
                _texCoordTL.X = temp;
            }

			item.Set (destinationRectangle.X,
					destinationRectangle.Y, 
					-origin.X, 
					-origin.Y,
					destinationRectangle.Z,
					destinationRectangle.W,
					(float)Math.Sin (rotation), 
					(float)Math.Cos (rotation), 
					color, 
					_texCoordTL, 
					_texCoordBR);

            /*logCount++;
            if(logCount > 300)
            {
                logCount = 0;
                Console.WriteLine("Rotation: " + rotation);
                Console.WriteLine("item set input: destinationRectangle: " + destinationRectangle.ToString() + " origin: " + origin.ToString() +
                    " sin: " + (float)Math.Sin(rotation) + " cos: " + (float)Math.Cos(rotation) + " _texCoordTL: " + _texCoordTL.ToString() + " _texCoordBR: " + _texCoordBR);
            }*/

			if (autoFlush)
			{
				FlushIfNeeded();
			}
		}

		// Mark the end of a draw operation for Immediate SpriteSortMode.
		internal void FlushIfNeeded()
		{
			if (_sortMode == SpriteSortMode.Immediate)
			{
				_batcher.DrawBatch(_sortMode, _effect);
			}
		}

		public void Draw (Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
		{
			Draw (texture, position, sourceRectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}

		public void Draw (Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
		{
			Draw (texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0f);
		}

		public void Draw (Texture2D texture, Vector2 position, Color color)
		{
			Draw (texture, position, null, color);
		}

		public void Draw (Texture2D texture, Rectangle rectangle, Color color)
		{
			Draw (texture, rectangle, null, color);
		}
		
		public void DrawString (SpriteFont spriteFont, string text, Vector2 position, Color color)
		{
            CheckValid(spriteFont, text);

            var source = new SpriteFont.CharacterSource(text);
			spriteFont.DrawInto (
                this, ref source, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}

		public void DrawString (
			SpriteFont spriteFont, string text, Vector2 position, Color color,
			float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
		{
            CheckValid(spriteFont, text);

			var scaleVec = new Vector2(scale, scale);
            var source = new SpriteFont.CharacterSource(text);
            spriteFont.DrawInto(this, ref source, position, color, rotation, origin, scaleVec, effects, depth);
		}

		public void DrawString (
			SpriteFont spriteFont, string text, Vector2 position, Color color,
			float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
		{
            CheckValid(spriteFont, text);

            var source = new SpriteFont.CharacterSource(text);
            spriteFont.DrawInto(this, ref source, position, color, rotation, origin, scale, effect, depth);
		}

		public void DrawString (SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
		{
            CheckValid(spriteFont, text);

            var source = new SpriteFont.CharacterSource(text);
			spriteFont.DrawInto(this, ref source, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}

		public void DrawString (
			SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
			float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
		{
            CheckValid(spriteFont, text);

			var scaleVec = new Vector2 (scale, scale);
            var source = new SpriteFont.CharacterSource(text);
            spriteFont.DrawInto(this, ref source, position, color, rotation, origin, scaleVec, effects, depth);
		}

		public void DrawString (
			SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
			float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
		{
            CheckValid(spriteFont, text);

            var source = new SpriteFont.CharacterSource(text);
            spriteFont.DrawInto(this, ref source, position, color, rotation, origin, scale, effect, depth);
		}

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    //if (_spriteEffect != null)
                    //{
                    //    _spriteEffect.Dispose();
                    //    _spriteEffect = null;
                    //}
                }
            }
            base.Dispose(disposing);
        }

		/// <summary>
		/// Writes a string that is preprocessed as a vertex list (including rotation, scaling, flipping, etc). The 
		/// characterVertices array must contain at least numVertices instances (each 4 representing a single sprite, 
		/// which will be copied to the SpriteBatch's internal buffer.
		/// Vertices are expected in the order [TopLeft, TopRight, BottomLeft, BottomRight], repeated.
		/// 
		/// The specified color is applied to the vertices in this function, the Red channel on each vertex is used as
		/// an index to the specified font texture array. 
		/// </summary>
		public void DrawStringUnity(Texture2D[] fontTextures, int numVertices, VertexPositionColorTexture[] characterVertices, Color color)
		{
			// prepare texture instances
			for (int i = 0; i < numVertices; )
			{
				SpriteBatchItem item = _batcher.CreateBatchItem();
				// use the vertex color R channel as index for the texture array (avoids an extra parameter)
				item.Texture = fontTextures[characterVertices[i].Color.R];
				item.Depth = 0;
			
				item.vertexTL = characterVertices[i++];
				item.vertexTR = characterVertices[i++];
				item.vertexBL = characterVertices[i++];
				item.vertexBR = characterVertices[i++];
			
				item.vertexTL.Color = color;
				item.vertexTR.Color = color;
				item.vertexBL.Color = color;
				item.vertexBR.Color = color;
			}
		}

	}
}

