using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XnaWrapper
{
	public class AssetBundleGeneration
	{
		/// <summary>
		/// Generate a mapping from the content bundles in the game code. Implicitly makes the run a dummy run: limited to initializing and ignores content loading and the game loop.
		/// </summary>
		public static bool GENERATE_ASSET_MAPPINGS = true;
		public static string generatedMappings = null;
	}
}
