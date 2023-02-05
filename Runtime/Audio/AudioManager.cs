using System.Collections.Generic;
using System.Linq;
using MokomoGamesLib.Runtime.Audio.MasterTable;
using MokomoGamesLib.Runtime.Audio.Speaker;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Audio
{
    [RequireComponent(typeof(AudioResourceMasterTable))]
    [RequireComponent(typeof(ObjectPool.ObjectPool))]
    public class AudioManager : MonoBehaviour
    {
        private const int DefaultPoolSpeakerNum = 5;
        private AudioResourceMasterTable _audioResourceMasterTable;
        private Dictionary<SpeakerEntity, SpeakerPresenter> _speakerCashes;
        private ObjectPool.ObjectPool _speakerPool;

        private void Awake()
        {
            _audioResourceMasterTable = GetComponent<AudioResourceMasterTable>();
            _speakerPool = GetComponent<ObjectPool.ObjectPool>();
            _speakerCashes = new Dictionary<SpeakerEntity, SpeakerPresenter>();
            _speakerPool.Pool<SpeakerPresenter>(DefaultPoolSpeakerNum);
        }

        public SpeakerPresenter GetSpeaker(SoundName soundName)
        {
            var cacheSpeaker =
                _speakerCashes
                    .FirstOrDefault(
                        x =>
                            x.Key.SoundName == soundName).Value;
            if (cacheSpeaker != null) return cacheSpeaker;

            var speaker = _speakerPool.Get<SpeakerPresenter>();
            var speakerEntity = _audioResourceMasterTable.Find(soundName);
            speaker.name = $"[Speaker]_{speakerEntity.SoundName}";
            speaker.Initialize(speakerEntity);
            _speakerCashes.Add(speakerEntity, speaker);

            return speaker;
        }
    }
}