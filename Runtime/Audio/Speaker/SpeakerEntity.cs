using MokomoGamesLib.Runtime.Audio.MasterTable;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Audio.Speaker
{
    public class SpeakerEntity
    {
        public SpeakerEntity(SoundName soundName, AudioClip audioClip, float volume)
        {
            SoundName = soundName;
            AudioClip = audioClip;
            Volume = volume;
        }

        public float Volume { get; set; }
        public SoundName SoundName { get; }
        public AudioClip AudioClip { get; }
    }
}