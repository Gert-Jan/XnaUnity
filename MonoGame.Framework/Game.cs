using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using UnityEngine;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework
{
	public abstract class Game : IDisposable
	{
		// 
		public static bool IsDummy = false;

		public bool IsMouseVisible { get; set; }
		public GraphicsDevice GraphicsDevice { get; private set; }

		internal static Game Instance { get; private set; }
		public GameWindow Window { get { return _window; } }
		public ContentManager Content { get; private set; }

		private readonly GameTime _gameTime = new GameTime(new TimeSpan(), TimeSpan.FromSeconds(Time.fixedDeltaTime));
		private readonly TimeSpan _fixedDeltaTime = TimeSpan.FromSeconds(Time.fixedDeltaTime);

		public readonly string UnityStoragePath;

		public Game()
		{
			GraphicsDevice = new GraphicsDevice(new Viewport(0, 0, Screen.width, Screen.height));
			Content = new ContentManager();

			_window = new UnityGameWindow(GraphicsDevice);

			UnityStoragePath = Application.persistentDataPath;

			UnityEngine.Input.simulateMouseWithTouches = false;
			UnityEngine.Input.multiTouchEnabled = true;
		}

		public abstract void DummyInitialize(System.Text.StringBuilder bundleMappings);

		public void UnityInitialize()
		{
			LoadContent();
			Initialize();
			Instance = this;
		}

		/// <summary>
		/// Must be called by Unity in a Update call
		/// </summary>
		public void UnityUpdate()
		{
			foreach (AudioSource obj in SoundEffectInstance.disposed)
				UnityEngine.Object.Destroy(obj);
			SoundEffectInstance.disposed.Clear();

			PreUpdate();
			RunUpdates();
		}

		public void UnityDraw()
		{
			GraphicsDevice.ResetPools();
			Draw(_gameTime);
		}

		private readonly TimeSpan _targetElapsedTime = TimeSpan.FromTicks((long)10000000 / (long)60);
		private readonly TimeSpan _maxElapsedTime = TimeSpan.FromTicks((long)30000000 / (long)60);

		private readonly Stopwatch _gameTimer = Stopwatch.StartNew();
		private TimeSpan _accumulatedElapsedTime;

		public bool IsActive
		{
			get { return true; }
		}

		//Taken from MonoGame.Game.Tick
		private void RunUpdates()
		{
			_gameTime.ElapsedGameTime = _targetElapsedTime;
			//var stepCount = 0;

			// Advance the accumulated elapsed time.
			var elapsed = _gameTimer.Elapsed;
			if (elapsed > _maxElapsedTime)
				elapsed = _maxElapsedTime;
			_accumulatedElapsedTime += elapsed;
			_gameTimer.Reset();
			_gameTimer.Start();
			// Perform as many full fixed length time steps as we can.
			//bool wasInWhile = false;
			//while (_accumulatedElapsedTime >= _targetElapsedTime)
			//{
			//   wasInWhile = true;
			_gameTime.TotalGameTime += _targetElapsedTime;
			_accumulatedElapsedTime -= _targetElapsedTime;
			//	++stepCount;

			//MediaPlayer.Update((float)_targetElapsedTime.TotalSeconds);
			Update(_gameTime);

			//}

			// Draw needs to know the total elapsed time
			// that occured for the fixed length updates.
			_gameTime.ElapsedGameTime = TimeSpan.FromTicks(_targetElapsedTime.Ticks /* * stepCount*/);
		}

		private void PreUpdate()
		{
			_window.Update();
			UpdateInput();
		}



		//private bool _mouseIsDown = false;
		private UnityGameWindow _window;

		private const int MouseId = int.MinValue;

		private void UpdateInput()
		{
			//if (UnityEngine.Input.mousePresent)
			//{
			//	bool mouseIsDown = UnityEngine.Input.GetMouseButton(0);
			//	if (!_mouseIsDown && mouseIsDown)
			//		TouchPanel.AddEvent(MouseId, TouchLocationState.Pressed, ToMonoGame(UnityEngine.Input.mousePosition));
			//	else if (_mouseIsDown && !mouseIsDown)
			//		TouchPanel.AddEvent(MouseId, TouchLocationState.Released, ToMonoGame(UnityEngine.Input.mousePosition));
			//	else if (_mouseIsDown)
			//		TouchPanel.AddEvent(MouseId, TouchLocationState.Moved, ToMonoGame(UnityEngine.Input.mousePosition));
			//	_mouseIsDown = mouseIsDown;
			//}
			//
			//for (var i = 0; i < UnityEngine.Input.touchCount; i++)
			//{
			//	var touch = UnityEngine.Input.touches[i];
			//	switch (touch.phase)
			//	{
			//		case TouchPhase.Began:
			//			TouchPanel.AddEvent(touch.fingerId, TouchLocationState.Pressed, ToMonoGame(touch.position));
			//			break;
			//		case TouchPhase.Moved:
			//			TouchPanel.AddEvent(touch.fingerId, TouchLocationState.Moved, ToMonoGame(touch.position));
			//			break;
			//		case TouchPhase.Canceled:
			//		case TouchPhase.Ended:
			//			TouchPanel.AddEvent(touch.fingerId, TouchLocationState.Released, ToMonoGame(touch.position));
			//			break;
			//		case TouchPhase.Stationary:
			//			//do nothing
			//			break;
			//	}
			//}
		}

		private Vector2 ToMonoGame(UnityEngine.Vector3 vec)
		{
			return new Vector2(vec.x, Screen.height - vec.y);
		}

		public void Exit()
		{
			Application.Quit();
		}

		public virtual void Dispose()
		{
			UnloadContent();
			Instance = null;
		}

		protected virtual void Initialize()
		{
		}

		protected virtual void LoadContent()
		{
		}

		protected virtual void UnloadContent()
		{
		}

		protected virtual void Update(GameTime gameTime)
		{
		}

		protected virtual void Draw(GameTime gameTime)
		{
		}

		//TODO: work out when to call these from unity
		protected virtual void OnDeactivated(object sender, EventArgs args)
		{
		}

		protected virtual void OnActivated(object sender, EventArgs args)
		{
		}
	}
}
