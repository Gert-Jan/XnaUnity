using System;
using XnaWrapper.Collections;
using UnityEngine;

namespace Microsoft.Xna.Framework.Audio
{
	public partial class SoundEffectInstance
	{
		private AudioSource _source;
		private bool isPaused;

		private static AudioSource CreateNewSource() { return SoundEffect.GameObject.AddComponent<AudioSource>(); }
		private static TrackedPool<AudioSource> pool = new TrackedPool<AudioSource>(16, CreateNewSource);

		public void UnitySetup(SoundEffect soundEffect)
		{
			if (_source == null)
			{
				_source = pool.Obtain();
			}
            _source.clip = soundEffect.UnityAudioClip;
			_source.volume = _volume;
			_source.panStereo= _pan;
			_source.loop = false;
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
		}

		private void PlatformPlay()
		{
			_source.Play();
			isPaused = false;
		}

		private void PlatformStop(bool immediate)
		{
			_source.Stop();
		}

		private void PlatformPause()
		{
			_source.Pause();
			isPaused = true;
		}

		private void PlatformResume()
		{
			_source.Play();
			isPaused = false;
		}

		private bool PlatformGetIsLooped()
		{
			return _source.loop;
		}

		private void PlatformSetIsLooped(bool value)
		{
			_source.loop = value;
		}

		private void PlatformSetVolume(float value)
		{
			_source.volume = value;
		}

		private void PlatformSetPan(float value)
		{
			_source.panStereo = value;
		}

		private void PlatformSetPitch(float value)
		{
			//ref http://answers.unity3d.com/questions/55023/how-does-audiosourcepitch-changes-pitch.html
			_source.pitch = Mathf.Pow(2, _pitch);
		}

		private SoundState PlatformGetState()
		{
			if (isPaused)
				return SoundState.Paused;
			else if (_source.isPlaying)
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
