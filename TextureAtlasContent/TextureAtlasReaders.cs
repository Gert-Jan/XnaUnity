using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace TextureAtlasContent
{
	public class TextureAtlasReader : ContentTypeReader<TextureAtlas>
	{
        protected internal override TextureAtlas Read(ContentReader input, TextureAtlas existingInstance)
		{
			int regionCount = input.ReadInt32();
			TextureRegion[] regions = new TextureRegion[regionCount];
			for (int i = 0; i < regionCount; i++)
			{
				try
				{
					regions[i] = input.ReadObject<TextureRegion>();
				}
				catch (ContentLoadException e)
				{
					Console.WriteLine(e);
				}
			}
			int maxWidth = input.ReadInt32();
			int maxHeight = input.ReadInt32();
			return new TextureAtlas(regions, maxWidth, maxHeight);
		}
	}

	public class TextureRegionReader : ContentTypeReader<TextureRegion>
	{
        protected internal override TextureRegion Read(ContentReader input, TextureRegion existingInstance)
		{
			TextureRegion region = new TextureRegion();
			region.Key = input.ReadString();
			region.Bounds = input.ReadObject<Rectangle>();
			region.Origin = input.ReadVector2();
			region.OriginalSize = input.ReadVector2();
			return region;
		}
	}
}
