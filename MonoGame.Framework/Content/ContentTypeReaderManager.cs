#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2014 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */

/* Derived from code by the Mono.Xna Team (Copyright 2006).
 * Released under the MIT License. See monoxna.LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
#endregion

namespace Microsoft.Xna.Framework.Content
{
	public sealed class ContentTypeReaderManager
	{
		private static Dictionary<String, ContentTypeReader> contentReaders = new Dictionary<String, ContentTypeReader>();
		private static readonly object locker;
		private static readonly string assemblyName;

		static ContentTypeReaderManager()
		{
			locker = new object();
			assemblyName = Assembly.GetExecutingAssembly().FullName;
		}

		public ContentTypeReader GetTypeReader(Type targetType)
		{
			bool needsInit;
			return GetReaderByTypeName(targetType.FullName, out needsInit);
		}

		private ContentTypeReader GetReaderByTypeName(string originalReaderTypeString, out bool needsInitialize)
		{
			string readerTypeString = PrepareType(originalReaderTypeString);
			ContentTypeReader typeReader;
			if (contentReaders.TryGetValue(readerTypeString, out typeReader))
			{
				needsInitialize = true;
				return typeReader;
			}
			else
			{
				int length = originalReaderTypeString.IndexOf(',');
				if (length == -1)
					length = originalReaderTypeString.Length;
				string readerTypeName = originalReaderTypeString.Substring(0, length);
				// we need to hardcode typereaders because reflection doesn't work on consoles
				switch (readerTypeName)
				{
					case "System.Char":
					case "Microsoft.Xna.Framework.Content.CharReader":
						typeReader = new CharReader();
						break;
					case "Microsoft.Xna.Framework.Vector3":
					case "Microsoft.Xna.Framework.Content.Vector3Reader":
						typeReader = new Vector3Reader();
						break;
					case "Microsoft.Xna.Framework.Rectangle":
					case "Microsoft.Xna.Framework.Content.RectangleReader":
						typeReader = new RectangleReader();
						break;
					case "Microsoft.Xna.Framework.Content.Texture2DReader":
						typeReader = new Texture2DReader();
						break;
					case "Microsoft.Xna.Framework.Content.SpriteFontReader":
						typeReader = new SpriteFontReader();
						break;
					case "Microsoft.Xna.Framework.Content.ListReader`1[[Microsoft.Xna.Framework.Rectangle":
						typeReader = new ListReader<Rectangle>();
						break;
					case "Microsoft.Xna.Framework.Content.ListReader`1[[System.Char":
						typeReader = new ListReader<Char>();
						break;
					case "Microsoft.Xna.Framework.Content.ListReader`1[[Microsoft.Xna.Framework.Vector3":
						typeReader = new ListReader<Vector3>();
						break;
					default:
						throw new ContentLoadException(
							"Could not find ContentTypeReader Type. " +
							"Please ensure the name of the Assembly that " +
							"contains the Type matches the assembly in the full type name: " +
										originalReaderTypeString + " (" + readerTypeString + ")");
				}

				typeReader.Initialize(this);
				contentReaders.Add(readerTypeString, typeReader);
				needsInitialize = false;
				return typeReader;
			}
		}

		#region Internal Death Defying Method

		internal ContentTypeReader[] LoadAssetReaders(ContentReader reader)
		{
			/* The first content byte i read tells me the number of
			 * content readers in this XNB file.
			 */
			int numberOfReaders = reader.Read7BitEncodedInt();
			ContentTypeReader[] newReaders = new ContentTypeReader[numberOfReaders];
			BitArray needsInitialize = new BitArray(numberOfReaders);

			/* Lock until we're done allocating and initializing any new
			 * content type readers... this ensures we can load content
			 * from multiple threads and still cache the readers.
			 */
			lock (locker)
			{
				/* For each reader in the file, we read out the
				 * length of the string which contains the type
				 * of the reader, then we read out the string.
				 * Finally we instantiate an instance of that
				 * reader using reflection.
				 */
				for (int i = 0; i < numberOfReaders; i += 1)
				{
					/* This string tells us what reader we
					 * need to decode the following data.
					 */
					string originalReaderTypeString = reader.ReadString();
					bool needsInit = false;
					ContentTypeReader typeReader = GetReaderByTypeName(originalReaderTypeString, out needsInit);
					newReaders[i] = typeReader;
					needsInitialize[i] = needsInit;

					/* I think the next 4 bytes refer to the "Version" of the type reader,
					 * although it always seems to be zero.
					 */
					reader.ReadInt32();
				}

				// Initialize any new readers.
				for (int i = 0; i < newReaders.Length; i += 1)
				{
					if (needsInitialize.Get(i))
					{
						newReaders[i].Initialize(this);
					}
				}
			} // lock (locker)

			return newReaders;
		}

		#endregion

		#region Internal Static Methods
		/// <summary>
		/// Removes Version, Culture and PublicKeyToken from a type string.
		/// </summary>
		/// <remarks>
		/// Supports multiple generic types (e.g. Dictionary&lt;TKey,TValue&gt;)
		/// and nested generic types (e.g. List&lt;List&lt;int&gt;&gt;).
		/// </remarks>
		/// <param name="type">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		internal static string PrepareType(string type)
		{
			// Needed to support nested types
			int count = type.Split(
				new[] { "[[" },
				StringSplitOptions.None
			).Length - 1;
			string preparedType = type;
			for (int i = 0; i < count; i += 1)
			{
				preparedType = Regex.Replace(
					preparedType,
					@"\[(.+?), Version=.+?\]",
					"[$1]"
				);
			}
			// Handle non generic types
			if (preparedType.Contains("PublicKeyToken"))
			{
				preparedType = Regex.Replace(
					preparedType,
					@"(.+?), Version=.+?$",
					"$1"
				);
			}
			preparedType = preparedType.Replace(
				", Microsoft.Xna.Framework.Graphics",
				string.Format(
					", {0}",
					assemblyName
				)
			);
			preparedType = preparedType.Replace(
				", Microsoft.Xna.Framework.Video",
				string.Format(
					", {0}",
					assemblyName
				)
			);
			preparedType = preparedType.Replace(
				", Microsoft.Xna.Framework",
				string.Format(
					", {0}",
					assemblyName
				)
			);
			return preparedType;
		}

		#endregion
	}
}