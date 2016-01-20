// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Graphics
{
	public sealed class GraphicsAdapter : IDisposable
	{
		private static ReadOnlyCollection<GraphicsAdapter> _adapters;
		private DisplayModeCollection _supportedDisplayModes;

		internal GraphicsAdapter()
		{
		}

		public void Dispose()
		{
		}

		public DisplayMode CurrentDisplayMode
		{
			get
			{
				UnityEngine.Resolution r = UnityEngine.Screen.currentResolution;
				return new DisplayMode(r.width, r.height, r.refreshRate, SurfaceFormat.Color);
			}
		}

		public static GraphicsAdapter DefaultAdapter
		{
			get { return Adapters[0]; }
		}

		public static ReadOnlyCollection<GraphicsAdapter> Adapters
		{
			get
			{
				if (_adapters == null)
				{
					_adapters = new ReadOnlyCollection<GraphicsAdapter>(new[] { new GraphicsAdapter() });
				}

				return _adapters;
			}
		}

		/// <summary>
		/// Used to request creation of the reference graphics device.
		/// </summary>
		/// <remarks>
		/// This only works on DirectX platforms where a reference graphics
		/// device is available and must be defined before the graphics device
		/// is created.  It defaults to false.
		/// </remarks>
		public static bool UseReferenceDevice { get; set; }

		public DisplayModeCollection SupportedDisplayModes
		{
			get
			{
				if (_supportedDisplayModes == null)
				{
					//UnityEngine.Resolution[] resolutions = UnityEngine.Screen.resolutions;
					List<DisplayMode> modes = new List<DisplayMode>();
					//foreach (UnityEngine.Resolution r in resolutions)
					//{
					//	modes.Add(new DisplayMode(r.width, r.height, r.refreshRate, SurfaceFormat.Color));
					//}
					modes.Add(new DisplayMode(1920, 1080, 60, SurfaceFormat.Color));
					_supportedDisplayModes = new DisplayModeCollection(modes);
				}

				return _supportedDisplayModes;
			}
		}

		/// <summary>
		/// Gets a <see cref="System.Boolean"/> indicating whether
		/// <see cref="GraphicsAdapter.CurrentDisplayMode"/> has a
		/// Width:Height ratio corresponding to a widescreen <see cref="DisplayMode"/>.
		/// Common widescreen modes include 16:9, 16:10 and 2:1.
		/// </summary>
		public bool IsWideScreen
		{
			get
			{
				// Common non-widescreen modes: 4:3, 5:4, 1:1
				// Common widescreen modes: 16:9, 16:10, 2:1
				// XNA does not appear to account for rotated displays on the desktop
				const float limit = 4.0f / 3.0f;
				var aspect = CurrentDisplayMode.AspectRatio;
				return aspect > limit;
			}
		}
	}
}
