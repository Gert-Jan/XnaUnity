using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TextureAtlasContent
{
	public class TextureRegion
	{
		public String Key { get; set; }
		public Rectangle Bounds { get; set; }
		public Vector2 Origin { get; set; }
		public Vector2 OriginalSize { get; set; }
	}
}
