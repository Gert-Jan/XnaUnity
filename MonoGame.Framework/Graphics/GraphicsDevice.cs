#define MESH_USE_MEASURING

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

		private readonly SpriteMeshPool spriteMeshPool = new SpriteMeshPool();
		//private readonly UserMeshPool triangleMeshPool;

		public void DrawUserIndexedPrimitives(PrimitiveType primitiveType, VertexPositionColorTexture[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
		{
			Debug.Assert(vertexData != null && vertexData.Length > 0, "The vertexData must not be null or zero length!");
			Debug.Assert(indexData != null && indexData.Length > 0, "The indexData must not be null or zero length!");

			Material mat = activeEffect.Material;
			mat.mainTexture = Textures[0].UnityTexture;
			activeEffect.OnApplyPostTexture();
			mat.SetPass(0);

			SpriteMeshHolder mesh = spriteMeshPool.Get(primitiveCount / 2);
			mesh.Populate(vertexData, numVertices);
			UnityGraphics.DrawMeshNow(mesh.Mesh, Matrix4x4.identity);
		}

		public void DrawUserPrimitives(PrimitiveType primitiveType, VertexPositionColorTexture[] vertexData, int vertexOffset, int numVertices)
		{
			switch (primitiveType)
			{
				case PrimitiveType.TriangleStrip:
					break;

				default:
					throw new Exception("Primitive type unsupported: " + primitiveType);
			}
			//Material mat = activeEffect.Material;
			//mat.mainTexture = Textures[0].UnityTexture;
			//activeEffect.OnApplyPostTexture();
			//mat.SetPass(0);
			//
			//SpriteMeshHolder mesh = spriteMeshPool.Get(numVertices / 4);
			//mesh.Populate(vertexData, numVertices);
			//UnityGraphics.DrawMeshNow(mesh.Mesh, Matrix4x4.identity);
		}

		internal void DrawGroupedPrimitives(GroupedElementVertexArray vertexData, int numVertices)
		{
			Material mat = activeEffect.Material;
			mat.mainTexture = Textures[0].UnityTexture;
			activeEffect.OnApplyPostTexture();
			mat.SetPass(0);
			
			SpriteMeshHolder mesh = spriteMeshPool.Get(numVertices / 4);
			mesh.Populate(vertexData, numVertices);
			UnityGraphics.DrawMeshNow(mesh.Mesh, Matrix4x4.identity);
		}

		public void ResetPools()
		{
			spriteMeshPool.Reset();
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
			spriteMeshPool.InitBufferPair(index, initialInstances);
		}

		private class UserMeshHolder
		{

		}

		private class UserMeshPool
		{

		}

		private abstract class MeshHolder
		{
			public readonly Mesh Mesh;
			public readonly int vertexCount;

			public readonly UnityEngine.Vector3[] Vertices;
			public readonly UnityEngine.Vector2[] UVs;
			public readonly Color32[] Colors;

			public MeshHolder(int vertexCount)
			{
				this.vertexCount = vertexCount;

				Mesh = new Mesh();

				Vertices = new UnityEngine.Vector3[vertexCount];
				UVs = new UnityEngine.Vector2[vertexCount];
				Colors = new Color32[vertexCount];

				//Put some random crap in this so we can just set the triangles once
				//if these are not populated then unity totally fucks up our mesh and never draws it
				for (int i = 0; i < vertexCount; ++i)
				{
					Vertices[i] = new UnityEngine.Vector3(1, i);
					UVs[i] = new UnityEngine.Vector2(0, i);
					Colors[i] = new Color32(255, 255, 255, 255);
				}

				Mesh.vertices = Vertices;
				Mesh.uv = UVs;
				Mesh.colors32 = Colors;
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

				Array.Clear(Vertices, numVertices, Vertices.Length - numVertices);

				Mesh.vertices = Vertices;
				Mesh.uv = UVs;
				Mesh.colors32 = Colors;
			}

			// non inlined variant
			//public void Populate_Conversion(VertexPositionColorTexture[] vertexData, int numVertices)
			//{
			//	for (int i = 0; i < numVertices; i++)
			//	{
			//		XnaToUnity.Vector3(vertexData[i].Position, ref Vertices[i]);
			//		XnaToUnity.Vector2(vertexData[i].TextureCoordinate, ref UVs[i]);
			//		UVs[i].y = 1 - UVs[i].y;
			//		XnaToUnity.Color(vertexData[i].Color, ref Colors[i]);
			//	}
			//
			//	Array.Clear(Vertices, numVertices, Vertices.Length - numVertices);
			//
			//	Mesh.vertices = Vertices;
			//	Mesh.uv = UVs;
			//	Mesh.colors32 = Colors;
			//}
		}

		/*
		private class TriangleStripMeshHolder : MeshHolder
		{
			public TriangleStripMeshHolder(int stripVertices)
				: base((stripVertices - 2) * 3)
			{

				var triangleIndices = new int[vertexCount * 3 - 6];
				int totalIndices = triangleIndices.Length;

				//
				//   0----2----4----6----8
				//   |   /|   /|   /|   / 
				//   |  / |  / |  / |  / |
				//   | /  | /  | /  | /   
				//   |/   |/   |/   |/   |
				//   1----3----5----7 - -9
				//

				int index = 0;
				int a = 0;
				int b = 1;
				int c = 2;
				int d = 3;
				triangleIndices[index++] = a;
				triangleIndices[index++] = b;
				triangleIndices[index++] = c;
				for (; index < totalIndices;)
				{
					triangleIndices[index++] = b;
					triangleIndices[index++] = d;
					triangleIndices[index++] = c;

					if (index >= totalIndices)
						break;
					a += 2;
					b += 2;
					c += 2;
					d += 2;

					triangleIndices[index++] = a;
					triangleIndices[index++] = b;
					triangleIndices[index++] = c;
				}

				Mesh.triangles = triangleIndices;
			}

		}
		*/

		private class SpriteMeshHolder : MeshHolder
		{
			public SpriteMeshHolder(int spriteCount) 
				: base(spriteCount * 4)
			{
				var triangles = new int[spriteCount * 6];
				for (var i = 0; i < spriteCount; i++)
				{
					//
					//  TL    TR
					//   0----1 0,1,2,3 = index offsets for vertex indices
					//   |   /| TL,TR,BL,BR are vertex references in SpriteBatchItem.
					//   |  / |
					//   | /  |
					//   |/   |
					//   2----3
					//  BL    BR
					//
					// Triangle 1
					triangles[i * 6 + 0] = i * 4;
					triangles[i * 6 + 1] = i * 4 + 1;
					triangles[i * 6 + 2] = i * 4 + 2;
					// Triangle 2
					triangles[i * 6 + 3] = i * 4 + 1;
					triangles[i * 6 + 4] = i * 4 + 3;
					triangles[i * 6 + 5] = i * 4 + 2;
				}

				Mesh.triangles = triangles;
			}
			
		}
		
		private class SpriteMeshPool
		{
			private class MeshBufferPair
			{
				readonly int spriteCount;

				// meshes for the next frame
				SpriteMeshHolder[] frontMeshes;
				// meshes in use from the previous frame
				SpriteMeshHolder[] backMeshes;

				// counter in the current frontMeshes list, elements at a lower index are in use, those at an equal or higher index are free to be used next
				int nextFrontMesh = 0;

				public MeshBufferPair(int spriteCount, int initialInstances)
				{
                    this.spriteCount = spriteCount;

					if (initialInstances < 1)
						initialInstances = 1;

					int powerOfTwo = MathUtils.NextPowerOf2(Math.Max(16, initialInstances));
					frontMeshes = new SpriteMeshHolder[powerOfTwo];
					backMeshes = new SpriteMeshHolder[powerOfTwo];

					for (int i = 0; i < initialInstances; ++i)
					{
						frontMeshes[i] = new SpriteMeshHolder(spriteCount);
						backMeshes[i] = new SpriteMeshHolder(spriteCount);
					}
				}
				
				void GrowArray()
				{
					int oldCapacity = frontMeshes.Length;
                    int newCapacity = oldCapacity * 2;

					SpriteMeshHolder[] tmpFront = frontMeshes;
					SpriteMeshHolder[] tmpBack = backMeshes;

                    frontMeshes = new SpriteMeshHolder[newCapacity];
					backMeshes = new SpriteMeshHolder[newCapacity];
					for (int i = 0; i < oldCapacity; ++i)
					{
						frontMeshes[i] = tmpFront[i];
						backMeshes[i] = tmpBack[i];
					}
				}
				
				public SpriteMeshHolder GetNextFree()
				{
					if (nextFrontMesh >= frontMeshes.Length)
						GrowArray();

					SpriteMeshHolder mesh = frontMeshes[nextFrontMesh];
                    if (mesh == null)
					{
						mesh = new SpriteMeshHolder(spriteCount);
						frontMeshes[nextFrontMesh] = mesh;
                    }
					++nextFrontMesh;

                    return mesh;
				}

				public void Swap()
				{
					nextFrontMesh = 0;

					SpriteMeshHolder[] tmp = frontMeshes;
					frontMeshes = backMeshes;
					backMeshes = tmp;
				}
			}

			private const int maxBufferPairs = 16;

			private int highestBufferPairIndex = 0;
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

			public SpriteMeshHolder Get(int spriteCount)
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

				for (int i = 0; i < highestBufferPairIndex; ++i)
					bufferPairs[i].Swap();
			}
		}

		#endregion
	}
}
