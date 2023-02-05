using MokomoGamesLib.Runtime.Audio;
using MokomoGamesLib.Runtime.Audio.MasterTable;
using MokomoGamesLib.Runtime.GameConfig;
using UnityEngine;
using UnityEngine.UI;

namespace MokomoGamesLib.Runtime.UI
{
    public class PlayButtonSoundProcess : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private SoundName soundName;

        private void Start()
        {
            button.onClick.AddListener(() =>
            {
                new AudioService().PlayOneShot(
                    FindObjectOfType<AudioManager>(),
                    soundName,
                    FindObjectOfType<GameConfigManager>()
                );
            });
        }
    }
}