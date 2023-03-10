using System.Collections.Generic;
using System.Linq;
using MokomoGamesLib.Runtime.Audio.ResourceMasterTable;
using MokomoGamesLib.Runtime.Audio.ResourceMasterTable.AutoGenerated;
using MokomoGamesLib.Runtime.Audio.Speakers;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Audio
{
    [RequireComponent(typeof(MasterTable))]
    [RequireComponent(typeof(ObjectPool.ObjectPool))]
    public class AudioManager : MonoBehaviour
    {
        private const int DefaultPoolSpeakerNum = 5;
        private MasterTable _masterTable;
        private Dictionary<Speaker, SpeakerPresenter> _speakerCashes;
        private ObjectPool.ObjectPool _speakerPool;
        
        private void Awake()
        {
            _masterTable = GetComponent<MasterTable>();
            _speakerCashes = new Dictionary<Speaker, SpeakerPresenter>();
            _speakerPool = GetComponent<ObjectPool.ObjectPool>();
            _speakerPool.Pool<SpeakerPresenter>(DefaultPoolSpeakerNum);
        }
        
        public SpeakerPresenter GetOrCreateSpeakerPresenter(Speaker speaker)
        {
            SpeakerPresenter GetCache(Speaker inSpeaker)
            {
                return _speakerCashes.FirstOrDefault(x => x.Key.SoundName == inSpeaker.SoundName).Value;
            }

            var cachePresenter = GetCache(speaker);
            if (cachePresenter != null) return cachePresenter;

            speaker.OnChangedVolume += self => { GetCache(self).SetVolume(self.Volume); };
            var presenter = _speakerPool.Get<SpeakerPresenter>();
            presenter.Initialize(speaker);
            _speakerCashes.Add(speaker, presenter);
            return presenter;
        }

        public Speaker GetOrCreateSpeaker(SoundName soundName)
        {
            Speaker GetCache(SoundName inSoundName)
            {
                return _speakerCashes.FirstOrDefault(x => x.Key.SoundName == inSoundName).Key;
            }

            var cache = GetCache(soundName);
            if (cache != null) return cache;
            
            // TODO: 同じサウンドでも場面に応じたボリューム設定ができない問題はどうにかする
            var resourceRecord = _masterTable.Find(soundName);
            if (resourceRecord == null) return null;
            var speaker = new Speaker(resourceRecord.SoundName, resourceRecord.AudioClip, resourceRecord.SourceVolume);
            return speaker;
        }
    }
}