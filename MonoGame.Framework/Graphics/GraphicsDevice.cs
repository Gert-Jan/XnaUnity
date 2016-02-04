using System;
using UnityEngine;
using XnaWrapper;
using UnityGraphics = UnityEngine.Graphics;
using UVec2 = UnityEngine.Vector2;
using UVec3 = UnityEngine.Vector3;

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

		private readonly MeshBufferPool<SpriteMeshHolder> spriteMeshPool = new MeshBufferPool<SpriteMeshHolder>();
		private readonly MeshBufferPool<TriangleStripMeshHolder> triangleStripMeshPool = new MeshBufferPool<TriangleStripMeshHolder>();

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

		//public void DrawUserIndexedPrimitives(PrimitiveType primitiveType, VertexPositionColorTexture[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
		//{
		//	Material mat = activeEffect.Material;
		//	mat.mainTexture = Textures[0].UnityTexture;
		//	activeEffect.OnApplyPostTexture();
		//	mat.SetPass(0);
		//
		//	MeshHolder mesh = spriteMeshPool.Get(primitiveCount / 2);
		//	mesh.Draw(vertexData, numVertices);
		//}

		public void DrawUserPrimitives(PrimitiveType primitiveType, VertexPositionColorTexture[] vertexData, int vertexOffset, int primitiveCount)
		{
			activeEffect.OnApply();
			Material mat = activeEffect.Material;
			mat.mainTexture = Textures[0].UnityTexture;
			activeEffect.OnApplyPostTexture();
			mat.SetPass(0);
			
			switch (primitiveType)
			{
				case PrimitiveType.TriangleStrip:
					// primitiveCount is the number of triangles in strip
					TriangleStripMeshHolder stripMesh = triangleStripMeshPool.Get(primitiveCount);
					stripMesh.DrawStrip(vertexData, vertexOffset, primitiveCount);
                    break;
				default:
					throw new Exception("Primitive type unsupported: " + primitiveType);
			}
		}

		internal void DrawSpritePrimitives(GroupedElementVertexArray vertexData, int numVertices)
		{
			Material mat = activeEffect.Material;
			mat.mainTexture = Textures[0].UnityTexture;
			activeEffect.OnApplyPostTexture();
			mat.SetPass(0);

			SpriteMeshHolder mesh = spriteMeshPool.Get(numVertices / 4);
			mesh.DrawSprites(vertexData, numVertices);
		}

		public void SwapMeshes()
		{
			spriteMeshPool.RestartBuffers();
			triangleStripMeshPool.RestartBuffers();
            if (Time.frameCount % 2 == 0)
				MeshHolder.DrawFunction = MeshHolder.DrawA;
			else
				MeshHolder.DrawFunction = MeshHolder.DrawB;
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
		
		private abstract class MeshHolder
		{
			private Mesh meshA;
			private Mesh meshB;

			protected UVec3[] Vertices { get; private set; }
			protected UVec2[] UVs { get; private set; }
			protected Color32[] Colors { get; private set; }
			
			public static Action<MeshHolder> DrawFunction = DrawA;
			
			public void InitMesh(int primitiveCount)
			{
				int vertexCount = VerticesNeeded(primitiveCount);

				Vertices = new UVec3[vertexCount];
				UVs = new UVec2[vertexCount];
				Colors = new Color32[vertexCount];

				//Put some random crap in this so we can just set the triangles once
				//if these are not populated then unity totally fucks up our mesh and never draws it
				for (int i = 0; i < vertexCount; ++i)
				{
					Vertices[i] = new UVec3(1, i);
					UVs[i] = new UVec2(0, i);
					Colors[i] = new Color32(255, 255, 255, 255);
				}

				int[] triangles = MakeTriangles(primitiveCount);

				meshA = new Mesh();
				meshA.vertices = Vertices;
				meshA.uv = UVs;
				meshA.colors32 = Colors;
				meshA.triangles = triangles;

				meshB = new Mesh();
				meshB.vertices = Vertices;
				meshB.uv = UVs;
				meshB.colors32 = Colors;
				meshB.triangles = triangles;
			}

			protected abstract int VerticesNeeded(int primitiveCount);
			protected abstract int[] MakeTriangles(int primitiveCount);

			public static void DrawA(MeshHolder holder)
			{
				holder.meshA.vertices = holder.Vertices;
				holder.meshA.uv = holder.UVs;
				holder.meshA.colors32 = holder.Colors;
				UnityGraphics.DrawMeshNow(holder.meshA, Matrix4x4.identity);
			}

			public static void DrawB(MeshHolder holder)
			{
				holder.meshB.vertices = holder.Vertices;
				holder.meshB.uv = holder.UVs;
				holder.meshB.colors32 = holder.Colors;
				UnityGraphics.DrawMeshNow(holder.meshB, Matrix4x4.identity);
			}
			
			// unused
			public void DrawOld_A(VertexPositionColorTexture[] vertexData, int vertexOffset, int numVertices)
			{
				VertexPositionColorTexture vertex;
				for (int i = 0, v = vertexOffset; i < numVertices; ++i, ++v)
				{
					vertex = vertexData[v];
					Vertices[i].Set(vertex.Position.X, vertex.Position.Y, vertex.Position.Z);
					UVs[i].Set(vertex.TextureCoordinate.X, 1 - vertex.TextureCoordinate.Y);
					uint color = vertex.Color.PackedValue;
					Colors[i].r = (byte)(color);
					Colors[i].g = (byte)(color >> 8);
					Colors[i].b = (byte)(color >> 16);
					Colors[i].a = (byte)(color >> 24);
				}

				Array.Clear(Vertices, numVertices, Vertices.Length - numVertices);

				DrawFunction(this);
			}

			// unused
			public void DrawOld_B(VertexPositionColorTexture[] vertexData, int numVertices)
			{
				for (int i = 0; i < numVertices; i++)
				{
					XnaToUnity.Vector3(vertexData[i].Position, ref Vertices[i]);
					XnaToUnity.Vector2(vertexData[i].TextureCoordinate, ref UVs[i]);
					UVs[i].y = 1 - UVs[i].y;
					XnaToUnity.Color(vertexData[i].Color, ref Colors[i]);
				}
			
				Array.Clear(Vertices, numVertices, Vertices.Length - numVertices);

				DrawFunction(this);
			}
		}
		
		private class TriangleStripMeshHolder : MeshHolder
		{
			protected override int VerticesNeeded(int trianglesInStrip)
			{
				return trianglesInStrip * 3;
			}

			protected override int[] MakeTriangles(int trianglesInStrip)
			{
				int[] indices = new int[trianglesInStrip * 3];
				int totalIndices = indices.Length;

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
				indices[index++] = a;
				indices[index++] = c;
				indices[index++] = b;
				for (; index < totalIndices;)
				{
					indices[index++] = b;
					indices[index++] = c;
					indices[index++] = d;

					if (index >= totalIndices)
						break;
					a += 2;
					b += 2;
					c += 2;
					d += 2;

					indices[index++] = a;
					indices[index++] = c;
					indices[index++] = b;
				}

				return indices;
			}

			public void DrawStrip(VertexPositionColorTexture[] vertexData, int vertexOffset, int primitiveCount)
			{
				VertexPositionColorTexture vertex;
				int numVertices = primitiveCount + 2;
				int i, v;
                for (i = 0, v = vertexOffset; i < numVertices; ++i, ++v)
				{
					vertex = vertexData[v];
					Vertices[i].Set(vertex.Position.X, vertex.Position.Y, vertex.Position.Z);
					UVs[i].Set(vertex.TextureCoordinate.X, 1 - vertex.TextureCoordinate.Y);
					uint color = vertex.Color.PackedValue;
					Colors[i].r = (byte)(color);
					Colors[i].g = (byte)(color >> 8);
					Colors[i].b = (byte)(color >> 16);
					Colors[i].a = (byte)(color >> 24);
				}

				// set all next vertices to the last one, so their triangles are zero-surface (no pixels)
				UVec3 lastVertex = Vertices[i - 1];
                for (; i < Vertices.Length; ++i)
				{
					Vertices[i] = lastVertex;
				}
					
				DrawFunction(this);
			}

		}

		private class SpriteMeshHolder : MeshHolder
		{
			protected override int VerticesNeeded(int spritesInBatch)
			{
				return spritesInBatch * 4;
            }

			protected override int[] MakeTriangles(int spritesInBatch)
			{
				int[] triangles = new int[spritesInBatch * 6];
				for (var i = 0; i < spritesInBatch; i++)
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
				return triangles;
			}

			public void DrawSprites(GroupedElementVertexArray vertexData, int numVertices)
			{
				Array.Copy(vertexData.positions, Vertices, numVertices);
				Array.Copy(vertexData.texcoords, UVs, numVertices);
				Array.Copy(vertexData.colors, Colors, numVertices);

				Array.Clear(Vertices, numVertices, Vertices.Length - numVertices);

				DrawFunction(this);
			}
		}

		private class SizedMeshBuffer<T> where T : MeshHolder, new()
		{
			private readonly int primitiveCount;

			private T[] meshes;

			// counter in the current frontMeshes list, elements at a lower index are in use, those at an equal or higher index are free to be used next
			private int nextFrontMesh = 0;

			internal SizedMeshBuffer(int primitiveCount, int initialInstances)
			{
				this.primitiveCount = primitiveCount;

				if (initialInstances < 1)
					initialInstances = 1;

				int powerOfTwo = MathUtils.NextPowerOf2(Math.Max(16, initialInstances));
				meshes = new T[powerOfTwo];

				for (int i = 0; i < initialInstances; ++i)
				{
					T mesh = new T();
					mesh.InitMesh(primitiveCount);
					meshes[i] = mesh;
                }
			}

			private void GrowArray(int newCapacity)
			{
				int oldCapacity = meshes.Length;

				T[] tmp = meshes;

				meshes = new T[newCapacity];
				for (int i = 0; i < oldCapacity; ++i)
				{
					meshes[i] = tmp[i];
				}
			}

			internal T GetNextFree()
			{
				if (nextFrontMesh >= meshes.Length)
					GrowArray(meshes.Length * 2);

				T mesh = meshes[nextFrontMesh];
				if (mesh == null)
				{
					mesh = new T();
					mesh.InitMesh(primitiveCount);
					meshes[nextFrontMesh] = mesh;
				}
				++nextFrontMesh;

				return mesh;
			}

			internal void Restart()
			{
				nextFrontMesh = 0;
			}

			internal void EnsureCapacity(int newCapacity)
			{
				newCapacity = MathUtils.NextPowerOf2(newCapacity);
				if (newCapacity > meshes.Length)
					GrowArray(newCapacity);
			}
		}

		private class MeshBufferPool<T> where T : MeshHolder, new()
		{
			private const int maxBuffers = 16;
			private SizedMeshBuffer<T>[] buffers = new SizedMeshBuffer<T>[maxBuffers];
			private int numBuffers = 0;
			
			public void InitBufferPair(int index, int initialCapacity)
			{
				int powerOfTwo = 1 << index;
				if (buffers[index] != null)
					buffers[index].EnsureCapacity(initialCapacity);
				else
					buffers[index] = new SizedMeshBuffer<T>(powerOfTwo, initialCapacity);
				// make sure instances exist for all lower indices as well
				for (int i = index - 1; i >= 0; --i)
					if (buffers[i] == null)
					{
						powerOfTwo = 1 << i;
						buffers[i] = new SizedMeshBuffer<T>(powerOfTwo, 1);
					}
				numBuffers = index - 1;
            }

			public T Get(int primitiveCount)
			{
				int index = MathUtils.HighestBitIndex(MathUtils.NextPowerOf2(primitiveCount));
				SizedMeshBuffer<T> buffer = buffers[index];
                if (buffer == null)
				{
					// if we happen to need buffers with higher counts of primitives, make a new one
					InitBufferPair(index, 1);
					buffer = buffers[index];
				}
				return buffer.GetNextFree();
			}

			public void RestartBuffers()
			{
				for (int i = 0; i < numBuffers; ++i)
					buffers[i].Restart();
            }
		}

		#endregion
	}
}
