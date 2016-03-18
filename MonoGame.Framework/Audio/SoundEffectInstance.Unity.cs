using System;
using XnaWrapper.Collections;
using UnityEngine;

namespace Microsoft.Xna.Framework.Audio
{
	public partial class SoundEffectInstance
	{
		private AudioSource _source;
		private SoundEffect _soundEffect;
		private bool isPaused;
		private bool isLooped = false;

		private static AudioSource CreateNewSource() { return SoundEffect.GameObject.AddComponent<AudioSource>(); }
		private static TrackedPool<AudioSource> pool = new TrackedPool<AudioSource>(16, CreateNewSource);

		public void UnitySetup(SoundEffect soundEffect)
		{
			_soundEffect = soundEffect;
			SetupAudioSource();
		}

		private void SetupAudioSource()
		{
			if (_source == null)
			{
				_source = pool.Obtain();
			}

			_source.clip = _soundEffect.UnityAudioClip;
			_source.volume = _volume;
			_source.panStereo = _pan;
			_source.loop = isLooped;
			//ref http://answers.unity3d.com/questions/55023/how-does-audiosourcepitch-changes-pitch.html
			_source.pitch = Mathf.Pow(2, _pitch);
		}
		private void PlatformInitialize(byte[] buffer, int sampleRate, int channels)
		{
			throw new NotImplementedException();
		}

		private void PlatformDispose(bool disposing)
		{	
			if (System.Threading.Thread.CurrentThread == XnaWrapper.PlatformInstances.unityMainThread)
				PlatformStop(true);
            pool.Restore(_source);
			_source = null;
		}

		private void PlatformPlay()
		{
			_source.Play();
			isPaused = false;
		}

		private void PlatformStop(bool immediate)
		{
			if (_source)
			{
				_source.Stop();
				pool.Restore(_source);
				_source = null;
			}
		}

		private void PlatformPause()
		{
			if (_source)
			{
				_source.Pause();
				isPaused = true;
			}
		}

		private void PlatformResume()
		{
			if (_source)
			{
				_source.Play();
				isPaused = false;
			}
		}

		private bool PlatformGetIsLooped()
		{
			return isLooped;
		}

		private void PlatformSetIsLooped(bool value)
		{
			isLooped = value;
			if (_source)
			{
				_source.loop = value;
			}
		}

		private void PlatformSetVolume(float value)
		{
			_volume = value;
			if (_source)
			{
				_source.volume = value;
			}
		}

		private void PlatformSetPan(float value)
		{
			_pan = value;
			if (_source)
			{
				_source.panStereo = value;
			}
		}

		private void PlatformSetPitch(float value)
		{
			_pitch = value;
			if (_source)
			{
				//ref http://answers.unity3d.com/questions/55023/how-does-audiosourcepitch-changes-pitch.html
				_source.pitch = Mathf.Pow(2, _pitch);
			}
		}

		private SoundState PlatformGetState()
		{
			if (isPaused)
				return SoundState.Paused;
			else if (_source != null && _source.isPlaying)
				return SoundState.Playing;
			else
				return SoundState.Stopped;
		}

		private void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
		{
			throw new NotImplementedException();
		}
	}
}
