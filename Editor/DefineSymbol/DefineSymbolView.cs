using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MokomoGamesLib.Editor.DefineSymbol
{
    public class DefineSymbolView
    {
        public DefineSymbolView(DefineSymbolSetting defineSymbolSetting)
        {
            DefineSymbolSetting = defineSymbolSetting;
        }

        public DefineSymbolSetting DefineSymbolSetting { get; }
        public bool IsClickedDeleteButton { get; private set; }
        public List<ToggleResult> ToggledResultList { get; } = new();

        public void OnGUI()
        {
            IsClickedDeleteButton = false;
            ToggledResultList.Clear();

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                if (GUILayout.Button("削除")) IsClickedDeleteButton = true;
                EditorGUILayout.LabelField($"シンボル名: {DefineSymbolSetting.key}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("定義先プラットフォーム一覧:", EditorStyles.boldLabel);

                foreach (var platform in DefineSymbolConfig.SupportPlatformList)
                {
                    var defined = EditorGUILayout.Toggle(
                        platform.TargetName,
                        DefineSymbolSetting.GetPlatformSetting(platform.TargetName).defined
                    );
                    ToggledResultList.Add(new ToggleResult
                    {
                        Platform = platform.TargetName,
                        Define = defined
                    });
                }
            }
        }

        public class ToggleResult
        {
            public bool Define;
            public string Platform;
        }
    }
}