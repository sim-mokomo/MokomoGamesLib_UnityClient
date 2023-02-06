using UnityEngine;

namespace MokomoGamesLib.Runtime.Audio.Speakers
{
    [RequireComponent(typeof(AudioSource))]
    public class SpeakerPresenter : MonoBehaviour
    {
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void Initialize(Speaker speaker)
        {
            name = $"[Speaker]_{speaker.SoundName}";
            SetVolume(speaker.Volume);
        }

        public void SetVolume(float volume)
        {
            _audioSource.volume = volume;
        }

        public void PlayOneShot(AudioClip clip)
        {
            _audioSource.PlayOneShot(clip);
        }

        public void Stop()
        {
            _audioSource.Stop();
        }
    }
}