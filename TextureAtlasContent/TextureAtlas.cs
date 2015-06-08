using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
#if !UNITY
using Microsoft.Xna.Framework.GamerServices;
#endif
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TextureAtlasContent
{
	public class TextureAtlas
	{
		private TextureRegion[] regions;
		private Dictionary<string, int> regionsDict = new Dictionary<string, int>();
		private int maxWidth;
		private int maxHeight;

		internal TextureAtlas(TextureRegion[] regions, int maxWidth, int maxHeight)
		{
			this.regions = regions;
			this.maxWidth = maxWidth;
			this.maxHeight = maxHeight;
			for (int i = 0; i < regions.Length; i++)
				regionsDict.Add(regions[i].Key, i);
		}

		public TextureRegion GetRegion(int index)
		{
			return regions[index];
		}

		public TextureRegion GetRegion(string key)
		{
			if (regionsDict.ContainsKey(key))
				return regions[regionsDict[key]];
			else
				return null;
		}

		public int GetRegionIndex(string key)
		{
			if (regionsDict.ContainsKey(key))
				return regionsDict[key];
			else
				return -1;
		}

		public int RegionCount
		{
			get { return regions.Length; }
		}

		public int MaxWidth
		{
			get { return maxWidth; }
			set { maxWidth = value; }
		}

		public int MaxHeight
		{
			get { return maxHeight; }
			set { maxHeight = value; }
		}
	}
}
