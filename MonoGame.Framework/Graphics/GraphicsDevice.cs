//#define MESH_USE_MEASURING

using System;
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
		private DisplayMode theOnlyMode = new DisplayMode(1920, 1080, 60, SurfaceFormat.Color);
		public DisplayMode DisplayMode
		{
			get { return theOnlyMode; }
		}

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

			Stats.Begin(Stats.TRACKER_MONO, "Mesh Get");
			var mesh = _meshPool.Get(numVertices / 4);
			Stats.End(Stats.TRACKER_MONO, "Mesh Get");
			mesh.Populate(vertexData, numVertices);
			UnityGraphics.DrawMeshNow(mesh.Mesh, Matrix4x4.identity);
		}

		public void ResetPools()
		{
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

		#region Mesh Helpers

		/// <summary>
		/// This function can be used to prepare a number of meshes so they don't need to be generated later in the game. Make sure not to init the same index twice.
		/// </summary>
		public void InitMeshPoolInstance(int index, int initialInstances)
		{
			_meshPool.InitBufferPair(index, initialInstances);
		}

		private class MeshHolder
		{
			public readonly Mesh Mesh;

			public readonly UnityEngine.Vector3[] Vertices;
			public readonly UnityEngine.Vector2[] UVs;
			public readonly Color32[] Colors;

			public MeshHolder(int spriteCount)
			{
				Mesh = new Mesh();
				//Mesh.MarkDynamic(); //Seems to be a win on wp8
				
				int vCount = spriteCount * 4;

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

				var triangles = new int[spriteCount * 6];
				for (var i = 0; i < spriteCount; i++)
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

		}
		
		private class MeshPool
		{
			private class MeshBufferPair
			{
				readonly int spriteCount;

				// meshes for the next frame
				MeshHolder[] frontMeshes;
				// meshes in use from the previous frame
				MeshHolder[] backMeshes;

				// counter in the current frontMeshes list, elements at a lower index are in use, those at an equal or higher index are free to be used next
				int nextFrontMesh = 0;

				public MeshBufferPair(int spriteCount, int initialInstances)
				{
                    this.spriteCount = spriteCount;

					if (initialInstances < 1)
						initialInstances = 1;

					int powerOfTwo = MathUtils.NextPowerOf2(Math.Max(16, initialInstances));
					frontMeshes = new MeshHolder[powerOfTwo];
					backMeshes = new MeshHolder[powerOfTwo];

					for (int i = 0; i < initialInstances; ++i)
					{
						frontMeshes[i] = new MeshHolder(spriteCount);
						backMeshes[i] = new MeshHolder(spriteCount);
					}
				}
				
				void GrowArray()
				{
					int oldCapacity = frontMeshes.Length;
                    int newCapacity = oldCapacity * 2;

					MeshHolder[] tmpFront = frontMeshes;
					MeshHolder[] tmpBack = backMeshes;

                    frontMeshes = new MeshHolder[newCapacity];
					backMeshes = new MeshHolder[newCapacity];
					for (int i = 0; i < oldCapacity; ++i)
					{
						frontMeshes[i] = tmpFront[i];
						backMeshes[i] = tmpBack[i];
					}
				}

				//bool tracked = false;

				public MeshHolder GetNextFree()
				{
					if (nextFrontMesh >= frontMeshes.Length)
						GrowArray();

					MeshHolder mesh = frontMeshes[nextFrontMesh];
                    if (mesh == null)
					{
						//if (tracked && nextFrontMesh == 0)
						//	Log.Write(spriteCount + "!!!" + nextFrontMesh);

						//Log.Write(spriteCount + "!!!" + nextFrontMesh);
						mesh = new MeshHolder(spriteCount);
						//tracked = true;
						frontMeshes[nextFrontMesh] = mesh;
                    }
					++nextFrontMesh;

                    return mesh;
				}

				public void Swap()
				{
					nextFrontMesh = 0;
					//tracked = false;

					MeshHolder[] tmp = frontMeshes;
					frontMeshes = backMeshes;
					backMeshes = tmp;
				}
			}

			private const int maxBufferPairs = 16;

			private int highestBufferPairIndex = -1;
			private MeshBufferPair[] bufferPairs = new MeshBufferPair[maxBufferPairs];

#if MESH_USE_MEASURING
			private int[] histogram = new int[maxBufferPairs];
			private int[] globalHistogram = new int[maxBufferPairs];
#endif
			
			public void InitBufferPair(int index, int initialCapacity)
			{
				int powerOfTwo = 1 << index;
				if (bufferPairs[index] != null)
					throw new Exception("(Inverse NullReferenceException): This reference is not null, where it should be (" + index + ")");
				bufferPairs[index] = new MeshBufferPair(powerOfTwo, initialCapacity);
				for (int i = index; i >= 0; --i)
					if (bufferPairs[i] == null)
						throw new NullReferenceException("Manual init should happen in ascending index order. While adding " + index + ", " + i + " was missing");
				highestBufferPairIndex = index;
            }

			public MeshHolder Get(int spriteCount)
			{
				int index = MathUtils.HighestBitIndex(MathUtils.NextPowerOf2(spriteCount));
				if (index > highestBufferPairIndex)
				{
					// if we happen to need buffers with higher spriteCounts, make a new one
					for (int i = highestBufferPairIndex + 1; i <= index; ++i)
						InitBufferPair(i, 1);
				}
#if MESH_USE_MEASURING
				histogram[index] = histogram[index] + 1;
#endif
				return bufferPairs[index].GetNextFree();
			}

			public void Reset()
			{
#if MESH_USE_MEASURING
				for (int i = 0; i < histogram.Length; ++i)
				{
					globalHistogram[i] = Math.Max(histogram[i], globalHistogram[i]);
					histogram[i] = 0;
				}
#endif

				for (int i = 0; i <= highestBufferPairIndex; ++i)
					bufferPairs[i].Swap();
			}
		}

		#endregion
	}
}
