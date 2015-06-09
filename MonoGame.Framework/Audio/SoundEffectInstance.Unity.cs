using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Microsoft.Xna.Framework.Audio
{
	public partial class SoundEffectInstance
	{
		private AudioSource _source;
		private bool isPaused;

		public void UnitySetup(SoundEffect soundEffect)
		{
			if (_source == null)
				_source = SoundEffect.GameObject.AddComponent<AudioSource>();
			_source.clip = soundEffect.UnityAudioClip;
			_source.volume = _volume;
			_source.panStereo= _pan;
			//ref http://answers.unity3d.com/questions/55023/how-does-audiosourcepitch-changes-pitch.html
			_source.pitch = Mathf.Pow(2, _pitch);
		}

		private void PlatformInitialize(byte[] buffer, int sampleRate, int channels)
		{
			throw new NotImplementedException();
		}

		private void PlatformDispose(bool disposing)
		{
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
