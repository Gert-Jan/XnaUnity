using UnityEngine;

namespace XnaWrapper.UnityUtils
{
	public static class Texture2DExtensions
	{
		/// <summary>
		/// Resize a unity texture, either upscaling or downscaling the texture data. Warning. This is a lossy operation and expensive in performance.
		/// </summary>
		public static void ScaleResize(this Texture2D source, int newWidth, int newHeight, FilterMode filterMode)
		{
			// save the old filtervalue on the source texture before changing it, so we can restore it later.
			FilterMode sourceFilterMode = source.filterMode;
			source.filterMode = filterMode;

			// use a temporary render texture to for the image data conversion
			RenderTexture rt = UnityEngine.RenderTexture.GetTemporary(newWidth, newHeight);
			rt.filterMode = UnityEngine.FilterMode.Point;
			RenderTexture.active = rt;
			Graphics.Blit(source, rt);

			// we then need to resize the original texture and copy the data back into it.
			source.Resize(newWidth, newHeight);
			source.ReadPixels(new UnityEngine.Rect(0, 0, newWidth, newWidth), 0, 0);
			RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(rt);
			source.filterMode = sourceFilterMode;
		}
	}
}
