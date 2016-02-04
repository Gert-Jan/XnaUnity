using System;
using System.IO;
using UnityEngine;

namespace Microsoft.Xna.Framework.Audio
{
	public sealed partial class SoundEffect
	{
		private readonly AudioClip _audioClip;
		//private AudioSource _audioSource;

		internal static readonly GameObject GameObject = new GameObject();

		internal SoundEffect(AudioClip audioClip)
		{
			if (audioClip == null)
				throw new Exception("AudioClip is null");

			_audioClip = audioClip;
			//_audioSource = GameObject.AddComponent<AudioSource>();
			//_audioSource.clip = _audioClip;
		}

		internal AudioClip UnityAudioClip
		{
			get { return _audioClip; }
		}

		//internal void UnityPlay()
		//{
		//	UnityPlay(1, 0, 0);
		//}
		//
		//internal void UnityPlay(float volume, float pitch, float pan)
		//{
		//	//ref http://answers.unity3d.com/questions/55023/how-does-audiosourcepitch-changes-pitch.html
		//	_audioSource.volume = volume;
        //    _audioSource.panStereo = pan;
		//	_audioSource.pitch = Mathf.Pow(2, pitch);
		//	_audioSource.Play();
		//}

		private void PlatformInitialize(byte[] buffer, int sampleRate, AudioChannels channels)
		{
			throw new NotImplementedException();
		}

		private void PlatformInitialize(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
		{
			throw new NotImplementedException();
		}

		private void PlatformLoadAudioStream(Stream s)
		{
			throw new NotImplementedException();
		}

		private void PlatformDispose(bool disposing)
		{
			throw new NotImplementedException();
		}

		private void PlatformSetupInstance(SoundEffectInstance instance)
		{
			instance.UnitySetup(this);
		}
	}
}
