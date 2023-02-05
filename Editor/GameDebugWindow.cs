using MokomoGamesLib.Runtime.Debugger;
using MokomoGamesLib.Runtime.Localization;
using UnityEditor;
using UnityEngine;

namespace MokomoGamesLib.Editor
{
    public class GameDebugWindow : EditorWindow
    {
        private bool _isFoldoutEpicDevAccount;
        private GameDebugSaveData _saveData;

        private void OnGUI()
        {
            if (GUILayout.Button("読み込み")) _saveData = GameDebugRepository.LoadIfNotExistCreate();

            if (_saveData)
            {
                var preLanguage = _saveData.GameLanguage;
                var curLanguage = (AppLanguage)EditorGUILayout.EnumPopup("ゲーム内言語", _saveData.GameLanguage);
                if (preLanguage != curLanguage) _saveData.GameLanguage = curLanguage;

                _isFoldoutEpicDevAccount = EditorGUILayout.Foldout(_isFoldoutEpicDevAccount, "エピック開発アカウント用設定");
                if (_isFoldoutEpicDevAccount)
                {
                    _saveData.CurrentEpicLoginInfo.UserName =
                        EditorGUILayout.TextField("ユーザ名", _saveData.CurrentEpicLoginInfo.UserName);
                    _saveData.CurrentEpicLoginInfo.Port =
                        EditorGUILayout.TextField("ポート番号", _saveData.CurrentEpicLoginInfo.Port);
                }
            }

            if (GUILayout.Button("保存"))
            {
                GameDebugRepository.Save(_saveData);

                var localizedManager = FindObjectOfType<LocalizeManager>();
                localizedManager.LoadAsync(_saveData.GameLanguage);
            }
        }

        [MenuItem("MokomoGames/ゲームデバッグウィンドウを開く")]
        public static void Open()
        {
            GetWindow<GameDebugWindow>();
        }
    }
}