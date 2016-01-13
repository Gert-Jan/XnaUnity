using System;
using Microsoft.Xna.Framework.Content;
using UObject = UnityEngine.Object;

namespace XnaWrapper
{
	public class ContentItem
	{
		public readonly string name;

		private int objectReferences = 0;
		private ContentRequest request;

		public bool IsActive { get { return request != null; } }

		public ContentRequest Request
		{
			get { return request; }
		}

		internal ContentItem(string name)
		{
			this.name = name;
		}

		internal void AddUsageReference()
		{
			++objectReferences;

			if (request == null)
				request = new ContentRequest();
		}

		internal void RemoveUsageReference()
		{
			--objectReferences;
			if (objectReferences == 0)
			{
				// Don't delete files in the editor, in case the asset was retrieved from AssetDatabase
				if (!PlatformInstances.AssetLoadingInfo.CanLoadFromAssetDatabase)
					UObject.DestroyImmediate(Request.Asset, true);

				request = null;
			}
			else if (objectReferences < 0)
				throw new Exception("Failed to properly manage asset item references.");
		}
	}
}
