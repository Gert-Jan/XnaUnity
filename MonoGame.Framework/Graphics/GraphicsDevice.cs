using System;
using System.Collections.Generic;
using UnityEngine;
using XnaWrapper;
using Debug = System.Diagnostics.Debug;
using UnityGraphics = UnityEngine.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
	public class GraphicsDevice : IDisposable
	{
		public class PParams
		{
			public int BackBufferWidth { get { return Screen.width; } }
			public int BackBufferHeight { get { return Screen.height; } }
		}
		private PParams pparams = new PParams();
		public PParams PresentationParameters { get { return pparams; } }

		//public int drawcount;
		//public UnityEngine.Material Material;
		public Texture2D[] Textures = new Texture2D[1];
		public Effect activeEffect;
		//private Matrix4x4 _baseMatrix;
		private Matrix defaultProjection;
		
		internal GraphicsDevice(Viewport viewport)
		{
			Adapter = new GraphicsAdapter();
			Viewport = viewport;
		}
		
		public GraphicsAdapter Adapter
		{
			get;
			private set;
		}

		private Viewport _viewport;
		public Viewport Viewport
		{
			get { return _viewport; }
			set
			{
				_viewport = value;

				// Set the graphics driver viewport to the right size
				GL.Viewport(new Rect(Viewport.X, Viewport.Y, Viewport.Width, Viewport.Height));

				// Calculate the right projection matrix, used when a draw call doesn't have specified a specific effect
				// defined to calculate the projection matrix
				if (Application.platform == RuntimePlatform.Android)
					Matrix.CreateOrthographicOffCenter(Viewport.X, Viewport.X + Viewport.Width, Viewport.Y + Viewport.Height, Viewport.Y, 0, 1, out defaultProjection);
				else
					Matrix.CreateOrthographicOffCenter(Viewport.X, Viewport.X + Viewport.Width, Viewport.Y, Viewport.Y + Viewport.Height, 0, 1, out defaultProjection);
			}
		}

		public Matrix DefaultProjection
		{
			get { return defaultProjection; }
		}

		public bool IsDisposed
		{
			get { return false; }
		}

		//TODO: This is just a test
		public DisplayMode DisplayMode
		{
			get { return new DisplayMode(1920, 1080, 60, SurfaceFormat.Color); }
		}

		private readonly MaterialPool _materialPool = new MaterialPool();
		private readonly MeshPool _meshPool = new MeshPool();

		public void DrawUserIndexedPrimitives(PrimitiveType primitiveType, VertexPositionColorTexture[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
		{
			Debug.Assert(vertexData != null && vertexData.Length > 0, "The vertexData must not be null or zero length!");
			Debug.Assert(indexData != null && indexData.Length > 0, "The indexData must not be null or zero length!");

			Material mat = activeEffect.Material;
			mat.mainTexture = Textures[0].UnityTexture;
			activeEffect.OnApplyPostTexture();
			mat.SetPass(0);

			var mesh = _meshPool.Get(primitiveCount / 2);
			mesh.Populate(vertexData, numVertices);
			UnityGraphics.DrawMeshNow(mesh.Mesh, UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);
		}

		internal void DrawGroupedPrimitives(GroupedElementVertexArray vertexData, int numVertices)
		{
			Material mat = activeEffect.Material;
			mat.mainTexture = Textures[0].UnityTexture;
			activeEffect.OnApplyPostTexture();
			mat.SetPass(0);

			var mesh = _meshPool.Get(numVertices / 4);
			mesh.Populate(vertexData, numVertices);
			UnityGraphics.DrawMeshNow(mesh.Mesh, UnityEngine.Vector3.zero, UnityEngine.Quaternion.identity);
		}

		public void ResetPools()
		{
			_materialPool.Reset();
			_meshPool.Reset();
		}

		private UnityEngine.Color tmp_uColor = new UnityEngine.Color();
		public void Clear(Color color)
		{
			XnaToUnity.Color(color, ref tmp_uColor);
			GL.Clear(true, true, tmp_uColor);
		}

		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public void SetRenderTarget(RenderTarget2D renderTarget)
		{
			if (renderTarget != null)
			{
				UnityGraphics.SetRenderTarget(renderTarget.UnityRenderTexture);
			}
			else
			{
				UnityGraphics.SetRenderTarget(null);
			}

		}

		private class MaterialPool
		{
			private class MaterialHolder
			{
				public readonly Material Material;
				public readonly Texture2D Texture2D;

				public MaterialHolder(Material material, Texture2D texture2D)
				{
					Material = material;
					Texture2D = texture2D;
				}
			}

			private readonly List<MaterialHolder> _materials = new List<MaterialHolder>();
			private int _index;
			private readonly Shader _shader = Shader.Find("Custom/SpriteShader");
			private Material _defaultmaterial;

			public MaterialPool()
			{
				_defaultmaterial = new Material(_shader);
			}

			private MaterialHolder Create(Texture2D texture)
			{
				var mat = _defaultmaterial;
				mat.mainTexture = texture.UnityTexture;
				mat.renderQueue += _materials.Count;
				return new MaterialHolder(mat, texture);
			}

			public Material Get(Texture2D texture)
			{
				while (_index < _materials.Count)
				{
					if (_materials[_index].Texture2D == texture)
					{
						_index++;
						return _materials[_index - 1].Material;
					}

					_index++;
				}

				var material = Create(texture);
				_materials.Add(material);
				_index++;
				return _materials[_index - 1].Material;
			}

			public void Reset()
			{
				_index = 0;
			}
		}

		private class MeshHolder
		{
			public readonly int SpriteCount;
			public readonly Mesh Mesh;

			public readonly UnityEngine.Vector3[] Vertices;
			public readonly UnityEngine.Vector2[] UVs;
			public readonly Color32[] Colors;

			public MeshHolder(int spriteCount)
			{
				Mesh = new Mesh();
				//Mesh.MarkDynamic(); //Seems to be a win on wp8

				SpriteCount = NextPowerOf2(spriteCount);
				int vCount = SpriteCount * 4;

				Vertices = new UnityEngine.Vector3[vCount];
				UVs = new UnityEngine.Vector2[vCount];
				Colors = new Color32[vCount];

				//Put some random crap in this so we can just set the triangles once
				//if these are not populated then unity totally fucks up our mesh and never draws it
				for (var i = 0; i < vCount; i++)
				{
					Vertices[i] = new UnityEngine.Vector3(1, i);
					UVs[i] = new UnityEngine.Vector2(0, i);
					Colors[i] = new Color32(255, 255, 255, 255);
				}

				var triangles = new int[SpriteCount * 6];
				for (var i = 0; i < SpriteCount; i++)
				{
					/*
					 *  TL    TR
					 *   0----1 0,1,2,3 = index offsets for vertex indices
					 *   |   /| TL,TR,BL,BR are vertex references in SpriteBatchItem.
					 *   |  / |
					 *   | /  |
					 *   |/   |
					 *   2----3
					 *  BL    BR
					 */
					// Triangle 1
					triangles[i * 6 + 0] = i * 4;
					triangles[i * 6 + 1] = i * 4 + 1;
					triangles[i * 6 + 2] = i * 4 + 2;
					// Triangle 2
					triangles[i * 6 + 3] = i * 4 + 1;
					triangles[i * 6 + 4] = i * 4 + 3;
					triangles[i * 6 + 5] = i * 4 + 2;
				}

				Mesh.vertices = Vertices;
				Mesh.uv = UVs;
				Mesh.colors32 = Colors;
				Mesh.triangles = triangles;
			}
			
			public void Populate(GroupedElementVertexArray vertexData, int numVertices)
			{
				Array.Copy(vertexData.positions, Vertices, numVertices);
				Array.Copy(vertexData.texcoords, UVs, numVertices);
				Array.Copy(vertexData.colors, Colors, numVertices);
				Array.Clear(Vertices, numVertices, Vertices.Length - numVertices);

				Mesh.vertices = Vertices;
				Mesh.uv = UVs;
				Mesh.colors32 = Colors;
			}

			//inlined variant
			public void Populate(VertexPositionColorTexture[] vertexData, int numVertices)
			{
				VertexPositionColorTexture vertex;
				for (int i = 0; i < numVertices; i++)
				{
					vertex = vertexData[i];
					Vertices[i].Set(vertex.Position.X, vertex.Position.Y, vertex.Position.Z);
					UVs[i].Set(vertex.TextureCoordinate.X, 1 - vertex.TextureCoordinate.Y);
					uint color = vertex.Color.PackedValue;
					Colors[i].r = (byte)(color);
					Colors[i].g = (byte)(color >> 8);
					Colors[i].b = (byte)(color >> 16);
					Colors[i].a = (byte)(color >> 24);
				}

				//we could clearly less if we remembered how many we used last time
				Array.Clear(Vertices, numVertices, Vertices.Length - numVertices);

				Mesh.vertices = Vertices;
				Mesh.uv = UVs;
				Mesh.colors32 = Colors;
				// possibly optional
				//Mesh.RecalculateBounds();
			}

			// non inlined variant
			public void Populate_Conversion(VertexPositionColorTexture[] vertexData, int numVertices)
			{
				for (int i = 0; i < numVertices; i++)
				{
					XnaToUnity.Vector3(vertexData[i].Position, ref Vertices[i]);
					XnaToUnity.Vector2(vertexData[i].TextureCoordinate, ref UVs[i]);
					UVs[i].y = 1 - UVs[i].y;
					XnaToUnity.Color(vertexData[i].Color, ref Colors[i]);
				}

				//we could clearly less if we remembered how many we used last time
				Array.Clear(Vertices, numVertices, Vertices.Length - numVertices);

				Mesh.vertices = Vertices;
				Mesh.uv = UVs;
				Mesh.colors32 = Colors;
				// possibly optional
				//Mesh.RecalculateBounds();
			}

			public int NextPowerOf2(int minimum)
			{
				int result = 1;

				while (result < minimum)
					result *= 2;

				return result;
			}
		}


		private class MeshPool
		{
			private List<MeshHolder> _unusedMeshes = new List<MeshHolder>();
			private List<MeshHolder> _usedMeshes = new List<MeshHolder>();

			private List<MeshHolder> _otherMeshes = new List<MeshHolder>();
			//private int _index;

			/// <summary>
			/// get a mesh with at least this many triangles
			/// </summary>
			public MeshHolder Get(int spriteCount)
			{
				MeshHolder best = null;
				int bestSpriteCount = 0;
				int bestIndex = -1;
                int unusedMeshesCount = _unusedMeshes.Count;
				for (int i = 0; i < unusedMeshesCount; i++)
				{
					var unusedMesh = _unusedMeshes[i];
					int unusedMeshSpriteCount = unusedMesh.SpriteCount;
                    if ((best == null || bestSpriteCount > unusedMeshSpriteCount) && unusedMeshSpriteCount >= spriteCount)
					{
						best = unusedMesh;
						bestSpriteCount = best.SpriteCount;
						bestIndex = i;
					}
				}
				if (best == null)
				{
					best = new MeshHolder(spriteCount);
				}
				else
				{
					_unusedMeshes.RemoveAt(bestIndex);
				}
				_usedMeshes.Add(best);

				return best;
			}

			public void Reset()
			{
				//Double Buffer our Meshes (Doesnt seem to be a win on wp8)
				//Ref http://forum.unity3d.com/threads/118723-Huge-performance-loss-in-Mesh-CreateVBO-for-dynamic-meshes-IOS

				//meshes from last frame are now unused
				_unusedMeshes.AddRange(_otherMeshes);
				_otherMeshes.Clear();

				//swap our use meshes and the now empty other meshes
				var temp = _otherMeshes;
				_otherMeshes = _usedMeshes;
				_usedMeshes = temp;
			}
		}
	}
}
