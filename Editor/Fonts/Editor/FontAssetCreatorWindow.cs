using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace MokomoGamesLib.Runtime.Font.Editor
{
    public class FontAssetCreatorWindow : EditorWindow
    {
        private const string k_FontAssetCreationSettingsContainerKey = "TextMeshPro.FontAssetCreator.RecentFontAssetCreationSettings.Container";
        private const string k_FontAssetCreationSettingsCurrentIndexKey = "TextMeshPro.FontAssetCreator.RecentFontAssetCreationSettings.CurrentIndex";
        private const float k_TwoColumnControlsWidth = 335f;
        private const string CustomGenerateReportsDir = "Assets/MokomoGames/Localization/Fonts/GenerateReports";

        private static FontAssetCreationSettingsContainer m_FontAssetCreationSettingsContainer;

        //static readonly string[] m_FontCreationPresets = new string[] { "Recent 1", "Recent 2", "Recent 3", "Recent 4" };
        private static int m_FontAssetCreationSettingsCurrentIndex;

        // Diagnostics
        private static Stopwatch m_StopWatch;
        private static double m_GlyphPackingGenerationTime;
        private static double m_GlyphRenderingGenerationTime;
        private static int m_PointSizeSamplingMode;
        private static FontPackingModes m_PackingMode = FontPackingModes.Fast;

        private static int m_CharacterSetSelectionMode;

        private static string m_CharacterSequence = "";
        private static string m_OutputFeedback = "";
        private static int m_CharacterCount;

        private static float m_AtlasGenerationProgress;
        private static string m_AtlasGenerationProgressLabel = string.Empty;
        private static bool m_IsGlyphPackingDone;
        private static bool m_IsGlyphRenderingDone;
        private static bool m_IsRenderingDone;
        private static bool m_IsProcessing;
        private static bool m_IsGenerationCancelled;
        private static Object m_SourceFontFile;
        private static TMP_FontAsset m_SelectedFontAsset;
        private static TMP_FontAsset m_LegacyFontAsset;
        private static TMP_FontAsset m_ReferencedFontAsset;

        private static TextAsset m_CharactersFromFile;
        private static int m_PointSize;

        private static int m_Padding = 5;
        //FaceStyles m_FontStyle = FaceStyles.Normal;
        //float m_FontStyleValue = 2;

        private static GlyphRenderMode m_GlyphRenderMode = GlyphRenderMode.SDFAA;
        private static int m_AtlasWidth = 512;
        private static int m_AtlasHeight = 512;
        private static byte[] m_AtlasTextureBuffer;
        private static Texture2D m_FontAtlasTexture;
        private static Texture2D m_GlyphRectPreviewTexture;
        private static Texture2D m_SavedFontAtlas;

        //
        private static readonly List<Glyph> m_FontGlyphTable = new();
        private static readonly List<TMP_Character> m_FontCharacterTable = new();

        private static readonly Dictionary<uint, uint> m_CharacterLookupMap = new();
        private static readonly Dictionary<uint, List<uint>> m_GlyphLookupMap = new();

        private static readonly List<Glyph> m_GlyphsToPack = new();
        private static readonly List<Glyph> m_GlyphsPacked = new();
        private static readonly List<GlyphRect> m_FreeGlyphRects = new();
        private static readonly List<GlyphRect> m_UsedGlyphRects = new();
        private static readonly List<Glyph> m_GlyphsToRender = new();
        private static readonly List<uint> m_AvailableGlyphsToAdd = new();
        private static readonly List<uint> m_MissingCharacters = new();
        private static readonly List<uint> m_ExcludedCharacters = new();

        private static FaceInfo m_FaceInfo;

        private static bool m_IncludeFontFeatures;

        private static readonly List<ConverterConfig> ConvertConfigTable = new()
        {
            new ConverterConfig
            {
                FontDataName = "Dubai-Regular.ttf",
                CharacterListFileName = "Characterlist_AR.txt"
            },
            new ConverterConfig
            {
                FontDataName = "migu-1c-regular.ttf",
                CharacterListFileName = "CharcterList_OTHER.txt"
            },
            new ConverterConfig
            {
                FontDataName = "NotoSansKR-Black.otf",
                CharacterListFileName = "Characterlist_KR.txt"
            },
            new ConverterConfig
            {
                FontDataName = "NotoSansSC-Black.otf",
                CharacterListFileName = "CharacterList_SC.txt"
            },
            new ConverterConfig
            {
                FontDataName = "NotoSansTC-Black.otf",
                CharacterListFileName = "CharacterList_TC.txt"
            }
        };

        private static bool processFromEditor;
        private readonly int[] m_FontAtlasResolutions = { 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

        private readonly string[] m_FontCharacterSets =
        {
            "ASCII", "Extended ASCII", "ASCII Lowercase", "ASCII Uppercase", "Numbers + Symbols", "Custom Range", "Unicode Range (Hex)",
            "Custom Characters", "Characters from File"
        };

        private readonly string[] m_FontResolutionLabels = { "8", "16", "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" };

        private readonly string[] m_FontSizingOptions = { "Auto Sizing", "Custom Size" };

        private bool m_IsFontAtlasInvalid;
        private bool m_IsGenerationDisabled;

        private bool m_IsRepaintNeeded;
        private Vector2 m_OutputScrollPosition;
        private float m_RenderingProgress;
        private Vector2 m_ScrollPosition;
        private string m_WarningMessage;

        public void Update()
        {
            if (m_IsRepaintNeeded)
            {
                //Debug.Log("Repainting...");
                m_IsRepaintNeeded = false;
                Repaint();
            }

            // Update Progress bar is we are Rendering a Font.
            if (m_IsProcessing)
            {
                m_AtlasGenerationProgress = FontEngineBridge.GetGenerationProgress();
                m_IsRepaintNeeded = true;
            }

            if (m_IsGlyphPackingDone) ApplyGlyphPackingResult();

            if (m_IsGlyphRenderingDone)
            {
                Debug.Log("Font Atlas generation completed in: " + m_GlyphRenderingGenerationTime.ToString("0.000 ms."));
                m_IsGlyphRenderingDone = false;
            }

            // Update Feedback Window & Create Font Texture once Rendering is done.
            if (m_IsRenderingDone)
            {
                CleanupAfterRenderTextureProcess();
                Repaint();
            }
        }


        public void OnEnable()
        {
            // Used for Diagnostics
            m_StopWatch = new Stopwatch();

            // Set Editor window size.
            minSize = new Vector2(315, minSize.y);

            // Initialize & Get shader property IDs.
            ShaderUtilities.GetShaderPropertyIDs();

            // Load last selected preset if we are not already in the process of regenerating an existing font asset (via the Context menu)
            if (EditorPrefs.HasKey(k_FontAssetCreationSettingsContainerKey))
            {
                if (m_FontAssetCreationSettingsContainer == null)
                    m_FontAssetCreationSettingsContainer =
                        JsonUtility.FromJson<FontAssetCreationSettingsContainer>(EditorPrefs.GetString(k_FontAssetCreationSettingsContainerKey));

                if (m_FontAssetCreationSettingsContainer.fontAssetCreationSettings != null &&
                    m_FontAssetCreationSettingsContainer.fontAssetCreationSettings.Count > 0)
                {
                    // Load Font Asset Creation Settings preset.
                    if (EditorPrefs.HasKey(k_FontAssetCreationSettingsCurrentIndexKey))
                        m_FontAssetCreationSettingsCurrentIndex = EditorPrefs.GetInt(k_FontAssetCreationSettingsCurrentIndexKey);

                    LoadFontCreationSettings(m_FontAssetCreationSettingsContainer.fontAssetCreationSettings[m_FontAssetCreationSettingsCurrentIndex]);
                }
            }

            ClearGeneratedData();
        }


        public void OnDisable()
        {
            //Debug.Log("TextMeshPro Editor Window has been disabled.");

            // Destroy Engine only if it has been initialized already
            FontEngine.DestroyFontEngine();

            ClearGeneratedData();

            // Remove Glyph Report if one was created.
            if (File.Exists("Assets/TextMesh Pro/Glyph Report.txt"))
            {
                File.Delete("Assets/TextMesh Pro/Glyph Report.txt");
                File.Delete("Assets/TextMesh Pro/Glyph Report.txt.meta");

                AssetDatabase.Refresh();
            }

            // Save Font Asset Creation Settings Index
            ProcessSaveDataOnExitApplication();

            // Unregister to event
            TMPro_EventManager.RESOURCE_LOAD_EVENT.Remove(ON_RESOURCES_LOADED);

            Resources.UnloadUnusedAssets();
        }


        public void OnGUI()
        {
            GUILayout.BeginHorizontal();

            DrawControls();

            if (position.width > position.height && position.width > k_TwoColumnControlsWidth)
                DrawPreview();

            GUILayout.EndHorizontal();
        }

        [MenuItem("MokomoGames/Font/Font Asset Creator")]
        public static void ShowFontAtlasCreatorWindow()
        {
            var window = GetWindow<FontAssetCreatorWindow>();
            window.titleContent = new GUIContent("Font Asset Creator");
            window.Focus();

            // Make sure TMP Essential Resources have been imported.
            window.CheckEssentialResources();
        }


        public static void ShowFontAtlasCreatorWindow(UnityEngine.Font sourceFontFile)
        {
            var window = GetWindow<FontAssetCreatorWindow>();

            window.titleContent = new GUIContent("Font Asset Creator");
            window.Focus();

            window.ClearGeneratedData();
            m_LegacyFontAsset = null;
            m_SelectedFontAsset = null;

            // Override selected font asset
            m_SourceFontFile = sourceFontFile;

            // Make sure TMP Essential Resources have been imported.
            window.CheckEssentialResources();
        }


        public static void ShowFontAtlasCreatorWindow(TMP_FontAsset fontAsset)
        {
            var window = GetWindow<FontAssetCreatorWindow>();

            window.titleContent = new GUIContent("Font Asset Creator");
            window.Focus();

            // Clear any previously generated data
            window.ClearGeneratedData();
            m_LegacyFontAsset = null;

            // Load font asset creation settings if we have valid settings
            if (string.IsNullOrEmpty(fontAsset.creationSettings.sourceFontFileGUID) == false)
            {
                window.LoadFontCreationSettings(fontAsset.creationSettings);

                // Override settings to inject character list from font asset
                m_CharacterSetSelectionMode = 6;
                m_CharacterSequence = TMP_EditorUtility.GetUnicodeCharacterSequence(TMP_FontAsset.GetCharactersArray(fontAsset));


                m_ReferencedFontAsset = fontAsset;
                m_SavedFontAtlas = fontAsset.atlasTexture;
            }
            else
            {
                window.m_WarningMessage = "Font Asset [" + fontAsset.name +
                                          "] does not contain any previous \"Font Asset Creation Settings\". This usually means [" + fontAsset.name +
                                          "] was created before this new functionality was added.";
                m_SourceFontFile = null;
                m_LegacyFontAsset = fontAsset;
            }

            // Even if we don't have any saved generation settings, we still want to pre-select the source font file.
            m_SelectedFontAsset = fontAsset;

            // Make sure TMP Essential Resources have been imported.
            window.CheckEssentialResources();
        }

        private static void ProcessSaveDataOnExitApplication()
        {
            SaveCreationSettingsToEditorPrefs(SaveFontCreationSettings());
            EditorPrefs.SetInt(k_FontAssetCreationSettingsCurrentIndexKey, m_FontAssetCreationSettingsCurrentIndex);
        }


        // Event received when TMP resources have been loaded.
        private void ON_RESOURCES_LOADED()
        {
            TMPro_EventManager.RESOURCE_LOAD_EVENT.Remove(ON_RESOURCES_LOADED);

            m_IsGenerationDisabled = false;
        }

        // Make sure TMP Essential Resources have been imported.
        private void CheckEssentialResources()
        {
            if (TMP_Settings.instance == null)
            {
                if (m_IsGenerationDisabled == false)
                    TMPro_EventManager.RESOURCE_LOAD_EVENT.Add(ON_RESOURCES_LOADED);

                m_IsGenerationDisabled = true;
            }
        }

        private static void ApplyGlyphPackingResult()
        {
            UpdateRenderFeedbackWindow();

            if (m_IsGenerationCancelled == false)
            {
                DrawGlyphRectPreviewTexture();
                Debug.Log("Glyph packing completed in: " + m_GlyphPackingGenerationTime.ToString("0.000 ms."));
            }

            m_IsGlyphPackingDone = false;
        }

        private static void CleanupAfterRenderTextureProcess()
        {
            m_IsProcessing = false;
            m_IsRenderingDone = false;
            if (processFromEditor) return;

            if (m_IsGenerationCancelled == false) StoreAtlasTexture();
        }

        private static void StoreAtlasTexture()
        {
            m_AtlasGenerationProgress = FontEngineBridge.GetGenerationProgress();
            m_AtlasGenerationProgressLabel = "Generation completed in: " +
                                             (m_GlyphPackingGenerationTime + m_GlyphRenderingGenerationTime).ToString(
                                                 "0.00 ms.");

            UpdateRenderFeedbackWindow();
            CreateFontAtlasTexture();

            // If dynamic make readable ...
            m_FontAtlasTexture.Apply(false, false);
        }


        /// <summary>
        ///     Method which returns the character corresponding to a decimal value.
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        private static uint[] ParseNumberSequence(string sequence)
        {
            var unicodeList = new List<uint>();
            var sequences = sequence.Split(',');

            foreach (var seq in sequences)
            {
                var s1 = seq.Split('-');

                if (s1.Length == 1)
                    try
                    {
                        unicodeList.Add(uint.Parse(s1[0]));
                    }
                    catch
                    {
                        Debug.Log("No characters selected or invalid format.");
                    }
                else
                    for (var j = uint.Parse(s1[0]); j < uint.Parse(s1[1]) + 1; j++)
                        unicodeList.Add(j);
            }

            return unicodeList.ToArray();
        }


        /// <summary>
        ///     Method which returns the character (decimal value) from a hex sequence.
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        private static uint[] ParseHexNumberSequence(string sequence)
        {
            var unicodeList = new List<uint>();
            var sequences = sequence.Split(',');

            foreach (var seq in sequences)
            {
                var s1 = seq.Split('-');

                if (s1.Length == 1)
                    try
                    {
                        unicodeList.Add(uint.Parse(s1[0], NumberStyles.AllowHexSpecifier));
                    }
                    catch
                    {
                        Debug.Log("No characters selected or invalid format.");
                    }
                else
                    for (var j = uint.Parse(s1[0], NumberStyles.AllowHexSpecifier); j < uint.Parse(s1[1], NumberStyles.AllowHexSpecifier) + 1; j++)
                        unicodeList.Add(j);
            }

            return unicodeList.ToArray();
        }


        private void DrawControls()
        {
            GUILayout.Space(5f);

            if (position.width > position.height && position.width > k_TwoColumnControlsWidth)
                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, GUILayout.Width(315));
            else
                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            GUILayout.Space(5f);

            GUILayout.Label(m_SelectedFontAsset != null ? string.Format("Font Settings [{0}]", m_SelectedFontAsset.name) : "Font Settings",
                EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUIUtility.labelWidth = 125f;
            EditorGUIUtility.fieldWidth = 5f;

            // Disable Options if already generating a font atlas texture.
            EditorGUI.BeginDisabledGroup(m_IsProcessing);
            {
                // FONT TTF SELECTION
                EditorGUI.BeginChangeCheck();
                m_SourceFontFile =
                    EditorGUILayout.ObjectField("Source Font File", m_SourceFontFile, typeof(UnityEngine.Font), false) as UnityEngine.Font;
                if (EditorGUI.EndChangeCheck())
                {
                    m_SelectedFontAsset = null;
                    m_IsFontAtlasInvalid = true;
                }

                // FONT SIZING
                EditorGUI.BeginChangeCheck();
                if (m_PointSizeSamplingMode == 0)
                {
                    m_PointSizeSamplingMode = EditorGUILayout.Popup("Sampling Point Size", m_PointSizeSamplingMode, m_FontSizingOptions);
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    m_PointSizeSamplingMode = EditorGUILayout.Popup("Sampling Point Size", m_PointSizeSamplingMode, m_FontSizingOptions,
                        GUILayout.Width(225));
                    m_PointSize = EditorGUILayout.IntField(m_PointSize);
                    GUILayout.EndHorizontal();
                }

                if (EditorGUI.EndChangeCheck()) m_IsFontAtlasInvalid = true;

                // FONT PADDING
                EditorGUI.BeginChangeCheck();
                m_Padding = EditorGUILayout.IntField("Padding", m_Padding);
                m_Padding = (int)Mathf.Clamp(m_Padding, 0f, 64f);
                if (EditorGUI.EndChangeCheck()) m_IsFontAtlasInvalid = true;

                // FONT PACKING METHOD SELECTION
                EditorGUI.BeginChangeCheck();
                m_PackingMode = (FontPackingModes)EditorGUILayout.EnumPopup("Packing Method", m_PackingMode);
                if (EditorGUI.EndChangeCheck()) m_IsFontAtlasInvalid = true;

                // FONT ATLAS RESOLUTION SELECTION
                GUILayout.BeginHorizontal();
                GUI.changed = false;

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PrefixLabel("Atlas Resolution");
                m_AtlasWidth = EditorGUILayout.IntPopup(m_AtlasWidth, m_FontResolutionLabels, m_FontAtlasResolutions);
                m_AtlasHeight = EditorGUILayout.IntPopup(m_AtlasHeight, m_FontResolutionLabels, m_FontAtlasResolutions);
                if (EditorGUI.EndChangeCheck()) m_IsFontAtlasInvalid = true;

                GUILayout.EndHorizontal();


                // FONT CHARACTER SET SELECTION
                EditorGUI.BeginChangeCheck();
                var hasSelectionChanged = false;
                m_CharacterSetSelectionMode = EditorGUILayout.Popup("Character Set", m_CharacterSetSelectionMode, m_FontCharacterSets);
                if (EditorGUI.EndChangeCheck())
                {
                    m_CharacterSequence = "";
                    hasSelectionChanged = true;
                    m_IsFontAtlasInvalid = true;
                }

                switch (m_CharacterSetSelectionMode)
                {
                    case 0: // ASCII
                        //characterSequence = "32 - 126, 130, 132 - 135, 139, 145 - 151, 153, 155, 161, 166 - 167, 169 - 174, 176, 181 - 183, 186 - 187, 191, 8210 - 8226, 8230, 8240, 8242 - 8244, 8249 - 8250, 8252 - 8254, 8260, 8286";
                        m_CharacterSequence = "32 - 126, 160, 8203, 8230, 9633";
                        break;

                    case 1: // EXTENDED ASCII
                        m_CharacterSequence = "32 - 126, 160 - 255, 8192 - 8303, 8364, 8482, 9633";
                        // Could add 9632 for missing glyph
                        break;

                    case 2: // Lowercase
                        m_CharacterSequence = "32 - 64, 91 - 126, 160";
                        break;

                    case 3: // Uppercase
                        m_CharacterSequence = "32 - 96, 123 - 126, 160";
                        break;

                    case 4: // Numbers & Symbols
                        m_CharacterSequence = "32 - 64, 91 - 96, 123 - 126, 160";
                        break;

                    case 5: // Custom Range
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.Label(
                            "Enter a sequence of decimal values to define the characters to be included in the font asset or retrieve one from another font asset.",
                            TMP_UIStyleManager.label);
                        GUILayout.Space(10f);

                        EditorGUI.BeginChangeCheck();
                        m_ReferencedFontAsset =
                            EditorGUILayout.ObjectField("Select Font Asset", m_ReferencedFontAsset, typeof(TMP_FontAsset), false) as TMP_FontAsset;
                        if (EditorGUI.EndChangeCheck() || hasSelectionChanged)
                        {
                            if (m_ReferencedFontAsset != null)
                                m_CharacterSequence =
                                    TMP_EditorUtility.GetDecimalCharacterSequence(TMP_FontAsset.GetCharactersArray(m_ReferencedFontAsset));

                            m_IsFontAtlasInvalid = true;
                        }

                        // Filter out unwanted characters.
                        var chr = Event.current.character;
                        if ((chr < '0' || chr > '9') && (chr < ',' || chr > '-')) Event.current.character = '\0';
                        GUILayout.Label("Character Sequence (Decimal)", EditorStyles.boldLabel);
                        EditorGUI.BeginChangeCheck();
                        m_CharacterSequence = EditorGUILayout.TextArea(m_CharacterSequence, TMP_UIStyleManager.textAreaBoxWindow,
                            GUILayout.Height(120), GUILayout.ExpandWidth(true));
                        if (EditorGUI.EndChangeCheck()) m_IsFontAtlasInvalid = true;

                        EditorGUILayout.EndVertical();
                        break;

                    case 6: // Unicode HEX Range
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.Label(
                            "Enter a sequence of Unicode (hex) values to define the characters to be included in the font asset or retrieve one from another font asset.",
                            TMP_UIStyleManager.label);
                        GUILayout.Space(10f);

                        EditorGUI.BeginChangeCheck();
                        m_ReferencedFontAsset =
                            EditorGUILayout.ObjectField("Select Font Asset", m_ReferencedFontAsset, typeof(TMP_FontAsset), false) as TMP_FontAsset;
                        if (EditorGUI.EndChangeCheck() || hasSelectionChanged)
                        {
                            if (m_ReferencedFontAsset != null)
                                m_CharacterSequence =
                                    TMP_EditorUtility.GetUnicodeCharacterSequence(TMP_FontAsset.GetCharactersArray(m_ReferencedFontAsset));

                            m_IsFontAtlasInvalid = true;
                        }

                        // Filter out unwanted characters.
                        chr = Event.current.character;
                        if ((chr < '0' || chr > '9') && (chr < 'a' || chr > 'f') && (chr < 'A' || chr > 'F') && (chr < ',' || chr > '-'))
                            Event.current.character = '\0';
                        GUILayout.Label("Character Sequence (Hex)", EditorStyles.boldLabel);
                        EditorGUI.BeginChangeCheck();
                        m_CharacterSequence = EditorGUILayout.TextArea(m_CharacterSequence, TMP_UIStyleManager.textAreaBoxWindow,
                            GUILayout.Height(120), GUILayout.ExpandWidth(true));
                        if (EditorGUI.EndChangeCheck()) m_IsFontAtlasInvalid = true;

                        EditorGUILayout.EndVertical();
                        break;

                    case 7: // Characters from Font Asset
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        GUILayout.Label("Type the characters to be included in the font asset or retrieve them from another font asset.",
                            TMP_UIStyleManager.label);
                        GUILayout.Space(10f);

                        EditorGUI.BeginChangeCheck();
                        m_ReferencedFontAsset =
                            EditorGUILayout.ObjectField("Select Font Asset", m_ReferencedFontAsset, typeof(TMP_FontAsset), false) as TMP_FontAsset;
                        if (EditorGUI.EndChangeCheck() || hasSelectionChanged)
                        {
                            if (m_ReferencedFontAsset != null)
                                m_CharacterSequence = TMP_FontAsset.GetCharacters(m_ReferencedFontAsset);

                            m_IsFontAtlasInvalid = true;
                        }

                        EditorGUI.indentLevel = 0;

                        GUILayout.Label("Custom Character List", EditorStyles.boldLabel);
                        EditorGUI.BeginChangeCheck();
                        m_CharacterSequence = EditorGUILayout.TextArea(m_CharacterSequence, TMP_UIStyleManager.textAreaBoxWindow,
                            GUILayout.Height(120), GUILayout.ExpandWidth(true));
                        if (EditorGUI.EndChangeCheck()) m_IsFontAtlasInvalid = true;
                        EditorGUILayout.EndVertical();
                        break;

                    case 8: // Character List from File
                        EditorGUI.BeginChangeCheck();
                        m_CharactersFromFile =
                            EditorGUILayout.ObjectField("Character File", m_CharactersFromFile, typeof(TextAsset), false) as TextAsset;
                        if (EditorGUI.EndChangeCheck()) m_IsFontAtlasInvalid = true;

                        if (m_CharactersFromFile != null)
                        {
                            var rx = new Regex(@"(?<!\\)(?:\\u[0-9a-fA-F]{4}|\\U[0-9a-fA-F]{8})");

                            m_CharacterSequence = rx.Replace(m_CharactersFromFile.text,
                                match =>
                                {
                                    if (match.Value.StartsWith("\\U"))
                                        return char.ConvertFromUtf32(int.Parse(match.Value.Replace("\\U", ""), NumberStyles.HexNumber));

                                    return char.ConvertFromUtf32(int.Parse(match.Value.Replace("\\u", ""), NumberStyles.HexNumber));
                                });
                        }

                        break;
                }

                // FONT STYLE SELECTION
                //GUILayout.BeginHorizontal();
                //EditorGUI.BeginChangeCheck();
                ////m_FontStyle = (FaceStyles)EditorGUILayout.EnumPopup("Font Style", m_FontStyle, GUILayout.Width(225));
                ////m_FontStyleValue = EditorGUILayout.IntField((int)m_FontStyleValue);
                //if (EditorGUI.EndChangeCheck())
                //{
                //    m_IsFontAtlasInvalid = true;
                //}
                //GUILayout.EndHorizontal();

                // Render Mode Selection
                CheckForLegacyGlyphRenderMode();

                EditorGUI.BeginChangeCheck();
                m_GlyphRenderMode = (GlyphRenderMode)EditorGUILayout.EnumPopup("Render Mode", m_GlyphRenderMode);
                if (EditorGUI.EndChangeCheck()) m_IsFontAtlasInvalid = true;

                m_IncludeFontFeatures = EditorGUILayout.Toggle("Get Kerning Pairs", m_IncludeFontFeatures);

                EditorGUILayout.Space();
            }

            EditorGUI.EndDisabledGroup();

            if (!string.IsNullOrEmpty(m_WarningMessage)) EditorGUILayout.HelpBox(m_WarningMessage, MessageType.Warning);

            GUI.enabled = m_SourceFontFile != null && !m_IsProcessing &&
                          !m_IsGenerationDisabled; // Enable Preview if we are not already rendering a font.
            if (GUILayout.Button("Generate Font Atlas") && GUI.enabled) GenerateFontAtlas();

            // FONT RENDERING PROGRESS BAR
            GUILayout.Space(1);
            var progressRect = EditorGUILayout.GetControlRect(false, 20);

            GUI.enabled = true;
            progressRect.width -= 22;
            EditorGUI.ProgressBar(progressRect, Mathf.Max(0.01f, m_AtlasGenerationProgress), m_AtlasGenerationProgressLabel);
            progressRect.x = progressRect.x + progressRect.width + 2;
            progressRect.y -= 1;
            progressRect.width = 20;
            progressRect.height = 20;

            GUI.enabled = m_IsProcessing;
            if (GUI.Button(progressRect, "X"))
            {
                FontEngineBridge.SendCancellationRequest();
                m_AtlasGenerationProgress = 0;
                m_IsProcessing = false;
                m_IsGenerationCancelled = true;
            }

            GUILayout.Space(5);

            // FONT STATUS & INFORMATION
            GUI.enabled = true;

            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Height(200));
            m_OutputScrollPosition = EditorGUILayout.BeginScrollView(m_OutputScrollPosition);
            EditorGUILayout.LabelField(m_OutputFeedback, TMP_UIStyleManager.label);
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();

            // SAVE TEXTURE & CREATE and SAVE FONT XML FILE
            GUI.enabled = m_FontAtlasTexture != null && !m_IsProcessing; // Enable Save Button if font_Atlas is not Null.

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Save") && GUI.enabled)
            {
                if (m_SelectedFontAsset == null)
                {
                    if (m_LegacyFontAsset != null)
                        SaveNewFontAssetWithSameName(m_LegacyFontAsset);
                    else
                        SaveNewFontAsset(m_SourceFontFile);
                }
                else
                {
                    // Save over exiting Font Asset
                    var filePath = Path.GetFullPath(AssetDatabase.GetAssetPath(m_SelectedFontAsset)).Replace('\\', '/');

                    if (((int)m_GlyphRenderMode & GlyphRasterModesBridge.GetRasterModeBitMap()) == GlyphRasterModesBridge.GetRasterModeBitMap())
                        Save_Bitmap_FontAsset(filePath);
                    else
                        Save_SDF_FontAsset(filePath);
                }
            }

            if (GUILayout.Button("Save as...") && GUI.enabled)
            {
                if (m_SelectedFontAsset == null)
                    SaveNewFontAsset(m_SourceFontFile);
                else
                    SaveNewFontAssetWithSameName(m_SelectedFontAsset);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();

            GUI.enabled = true; // Re-enable GUI

            if (position.height > position.width || position.width < k_TwoColumnControlsWidth)
                DrawPreview();

            EditorGUILayout.EndScrollView();

            if (m_IsFontAtlasInvalid)
                ClearGeneratedData();
        }


        /// <summary>
        ///     Clear the previously generated data.
        /// </summary>
        private void ClearGeneratedData()
        {
            m_IsFontAtlasInvalid = false;

            if (m_FontAtlasTexture != null && !EditorUtility.IsPersistent(m_FontAtlasTexture))
            {
                DestroyImmediate(m_FontAtlasTexture);
                m_FontAtlasTexture = null;
            }

            if (m_GlyphRectPreviewTexture != null)
            {
                DestroyImmediate(m_GlyphRectPreviewTexture);
                m_GlyphRectPreviewTexture = null;
            }

            m_AtlasGenerationProgressLabel = string.Empty;
            m_AtlasGenerationProgress = 0;
            m_SavedFontAtlas = null;

            m_OutputFeedback = string.Empty;
            m_WarningMessage = string.Empty;
        }


        /// <summary>
        ///     Function to update the feedback window showing the results of the latest generation.
        /// </summary>
        private static void UpdateRenderFeedbackWindow()
        {
            m_PointSize = m_FaceInfo.pointSize;

            var missingGlyphReport = string.Empty;

            //string colorTag = m_FontCharacterTable.Count == m_CharacterCount ? "<color=#C0ffff>" : "<color=#ffff00>";
            var colorTag2 = "<color=#C0ffff>";

            missingGlyphReport = "Font: <b>" + colorTag2 + m_FaceInfo.familyName + "</color></b>  Style: <b>" + colorTag2 + m_FaceInfo.styleName +
                                 "</color></b>";

            missingGlyphReport += "\nPoint Size: <b>" + colorTag2 + m_FaceInfo.pointSize + "</color></b>   SP/PD Ratio: <b>" + colorTag2 +
                                  ((float)m_Padding / m_FaceInfo.pointSize).ToString("0.0%" + "</color></b>");

            missingGlyphReport += "\n\nCharacters included: <color=#ffff00><b>" + m_FontCharacterTable.Count + "/" + m_CharacterCount +
                                  "</b></color>";
            missingGlyphReport += "\nMissing characters: <color=#ffff00><b>" + m_MissingCharacters.Count + "</b></color>";
            missingGlyphReport += "\nExcluded characters: <color=#ffff00><b>" + m_ExcludedCharacters.Count + "</b></color>";

            // Report characters missing from font file
            missingGlyphReport += "\n\n<b><color=#ffff00>Characters missing from font file:</color></b>";
            missingGlyphReport += "\n----------------------------------------";

            m_OutputFeedback = missingGlyphReport;

            for (var i = 0; i < m_MissingCharacters.Count; i++)
            {
                missingGlyphReport += "\nID: <color=#C0ffff>" + m_MissingCharacters[i] + "\t</color>Hex: <color=#C0ffff>" +
                                      m_MissingCharacters[i].ToString("X") + "\t</color>Char [<color=#C0ffff>" + (char)m_MissingCharacters[i] +
                                      "</color>]";

                if (missingGlyphReport.Length < 16300)
                    m_OutputFeedback = missingGlyphReport;
            }

            // Report characters that did not fit in the atlas texture
            missingGlyphReport += "\n\n<b><color=#ffff00>Characters excluded from packing:</color></b>";
            missingGlyphReport += "\n----------------------------------------";

            for (var i = 0; i < m_ExcludedCharacters.Count; i++)
            {
                missingGlyphReport += "\nID: <color=#C0ffff>" + m_ExcludedCharacters[i] + "\t</color>Hex: <color=#C0ffff>" +
                                      m_ExcludedCharacters[i].ToString("X") + "\t</color>Char [<color=#C0ffff>" + (char)m_ExcludedCharacters[i] +
                                      "</color>]";

                if (missingGlyphReport.Length < 16300)
                    m_OutputFeedback = missingGlyphReport;
            }

            if (missingGlyphReport.Length > 16300)
                m_OutputFeedback += "\n\n<color=#ffff00>Report truncated.</color>\n<color=#c0ffff>See</color> \"TextMesh Pro\\Glyph Report.txt\"";

            // Save Missing Glyph Report file
            if (Directory.Exists("Assets/TextMesh Pro"))
            {
                missingGlyphReport = Regex.Replace(missingGlyphReport, @"<[^>]*>", string.Empty);
                File.WriteAllText("Assets/TextMesh Pro/Glyph Report.txt", missingGlyphReport);
                AssetDatabase.Refresh();
            }

            var reportContent = new ReportContent
            {
                FontName = m_FaceInfo.familyName,
                PointSize = m_PointSize,
                Padding = m_Padding,
                IncludeCharacterList = m_FontCharacterTable.Select(x => x.unicode).ToList(),
                MissingCharacterList = m_MissingCharacters.ToList(),
                ExcludedCharacterList = m_ExcludedCharacters.ToList()
            };
            if (GlyphGeneratedReportDirExists()) SaveGlyphGeneratedReport(reportContent);
        }

        private static bool GlyphGeneratedReportDirExists()
        {
            return Directory.Exists(CustomGenerateReportsDir);
        }

        private static void ClearGeneratedReports()
        {
            Directory.Delete(CustomGenerateReportsDir, true);
            Directory.CreateDirectory(CustomGenerateReportsDir);
        }

        private static void SaveGlyphGeneratedReport(ReportContent reportContent)
        {
            File.WriteAllText(
                Path.Join(CustomGenerateReportsDir, $"{m_FaceInfo.familyName}_GlyphReport.txt"),
                JsonUtility.ToJson(reportContent));
            AssetDatabase.Refresh();
        }

        private static void DrawGlyphRectPreviewTexture()
        {
            if (m_GlyphRectPreviewTexture != null)
                DestroyImmediate(m_GlyphRectPreviewTexture);

            m_GlyphRectPreviewTexture = new Texture2D(m_AtlasWidth, m_AtlasHeight, TextureFormat.RGBA32, false, true);

            FontEngineBridge.ResetAtlasTexture(m_GlyphRectPreviewTexture);

            foreach (var glyph in m_GlyphsPacked)
            {
                var glyphRect = glyph.glyphRect;

                var c = Random.ColorHSV(0f, 1f, 0.5f, 0.5f, 1.0f, 1.0f);

                var x0 = glyphRect.x;
                var x1 = x0 + glyphRect.width;

                var y0 = glyphRect.y;
                var y1 = y0 + glyphRect.height;

                // Draw glyph rectangle.
                for (var x = x0; x < x1; x++)
                for (var y = y0; y < y1; y++)
                    m_GlyphRectPreviewTexture.SetPixel(x, y, c);
            }

            m_GlyphRectPreviewTexture.Apply(false);
        }

        private static void CreateFontAtlasTexture()
        {
            if (m_FontAtlasTexture != null)
                DestroyImmediate(m_FontAtlasTexture);

            m_FontAtlasTexture = new Texture2D(m_AtlasWidth, m_AtlasHeight, TextureFormat.Alpha8, false, true);

            var colors = new Color32[m_AtlasWidth * m_AtlasHeight];

            Debug.Log($"byte {m_AtlasTextureBuffer.Length}");
            for (var i = 0; i < colors.Length; i++)
            {
                var c = m_AtlasTextureBuffer[i];
                colors[i] = new Color32(c, c, c, c);
            }

            // Clear allocation of
            m_AtlasTextureBuffer = null;

            if ((m_GlyphRenderMode & GlyphRenderMode.RASTER) == GlyphRenderMode.RASTER ||
                (m_GlyphRenderMode & GlyphRenderMode.RASTER_HINTED) == GlyphRenderMode.RASTER_HINTED)
                m_FontAtlasTexture.filterMode = FilterMode.Point;

            m_FontAtlasTexture.SetPixels32(colors, 0);
            m_FontAtlasTexture.Apply(false, false);

            // Saving File for Debug
            //var pngData = m_FontAtlasTexture.EncodeToPNG();
            //File.WriteAllBytes("Assets/Textures/Debug Font Texture.png", pngData);
        }


        /// <summary>
        ///     Open Save Dialog to provide the option save the font asset using the name of the source font file. This also appends SDF to the name if using any
        ///     of the SDF Font Asset creation modes.
        /// </summary>
        /// <param name="sourceObject"></param>
        private static void SaveNewFontAsset(Object sourceObject)
        {
            string filePath;

            // Save new Font Asset and open save file requester at Source Font File location.
            var saveDirectory = new FileInfo(AssetDatabase.GetAssetPath(sourceObject)).DirectoryName;

            if (((int)m_GlyphRenderMode & GlyphRasterModesBridge.GetRasterModeBitMap()) == GlyphRasterModesBridge.GetRasterModeBitMap())
            {
                filePath = EditorUtility.SaveFilePanel("Save TextMesh Pro! Font Asset File", saveDirectory, sourceObject.name, "asset");

                if (filePath.Length == 0)
                    return;

                Save_Bitmap_FontAsset(filePath);
            }
            else
            {
                filePath = EditorUtility.SaveFilePanel("Save TextMesh Pro! Font Asset File", saveDirectory, sourceObject.name + " SDF", "asset");

                if (filePath.Length == 0)
                    return;

                Save_SDF_FontAsset(filePath);
            }
        }


        /// <summary>
        ///     Open Save Dialog to provide the option to save the font asset under the same name.
        /// </summary>
        /// <param name="sourceObject"></param>
        private void SaveNewFontAssetWithSameName(Object sourceObject)
        {
            string filePath;

            // Save new Font Asset and open save file requester at Source Font File location.
            var saveDirectory = new FileInfo(AssetDatabase.GetAssetPath(sourceObject)).DirectoryName;

            filePath = EditorUtility.SaveFilePanel("Save TextMesh Pro! Font Asset File", saveDirectory, sourceObject.name, "asset");

            if (filePath.Length == 0)
                return;

            if (((int)m_GlyphRenderMode & GlyphRasterModesBridge.GetRasterModeBitMap()) == GlyphRasterModesBridge.GetRasterModeBitMap())
                Save_Bitmap_FontAsset(filePath);
            else
                Save_SDF_FontAsset(filePath);
        }


        private static void Save_Bitmap_FontAsset(string filePath)
        {
            filePath = filePath.Substring(0, filePath.Length - 6); // Trim file extension from filePath.

            var dataPath = Application.dataPath;

            if (filePath.IndexOf(dataPath, StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                Debug.LogError(
                    "You're saving the font asset in a directory outside of this project folder. This is not supported. Please select a directory under \"" +
                    dataPath + "\"");
                return;
            }

            var relativeAssetPath = filePath.Substring(dataPath.Length - 6);
            var tex_DirName = Path.GetDirectoryName(relativeAssetPath);
            var tex_FileName = Path.GetFileNameWithoutExtension(relativeAssetPath);
            var tex_Path_NoExt = tex_DirName + "/" + tex_FileName;

            // Check if TextMeshPro font asset already exists. If not, create a new one. Otherwise update the existing one.
            var fontAsset = AssetDatabase.LoadAssetAtPath(tex_Path_NoExt + ".asset", typeof(TMP_FontAsset)) as TMP_FontAsset;
            var fontAssetBridge = new FontAssetBridge(fontAsset);
            if (fontAsset == null)
            {
                //Debug.Log("Creating TextMeshPro font asset!");
                fontAsset = CreateInstance<TMP_FontAsset>(); // Create new TextMeshPro Font Asset.
                AssetDatabase.CreateAsset(fontAsset, tex_Path_NoExt + ".asset");

                fontAssetBridge = new FontAssetBridge(fontAsset);
                // Set version number of font asset
                fontAssetBridge.SetVersion("1.1.0");

                //Set Font Asset Type
                fontAssetBridge.SetAtlasRenderMode(m_GlyphRenderMode);

                // Reference to the source font file GUID.
                fontAssetBridge.SetSourceFontFile_EditorRef((UnityEngine.Font)m_SourceFontFile);
                fontAssetBridge.SetSourceFontFileGuid(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_SourceFontFile)));

                // Add FaceInfo to Font Asset
                fontAsset.faceInfo = m_FaceInfo;

                // Add GlyphInfo[] to Font Asset
                fontAssetBridge.SetGlyphTable(m_FontGlyphTable);

                // Add CharacterTable[] to font asset.
                fontAssetBridge.SetCharacterTable(m_FontCharacterTable);

                // Sort glyph and character tables.
                fontAssetBridge.SortAllTables();

                // Get and Add Kerning Pairs to Font Asset
                if (m_IncludeFontFeatures)
                    fontAssetBridge.SetFontFeatureTable(GetKerningTable());

                // Add Font Atlas as Sub-Asset
                fontAsset.atlasTextures = new[] { m_FontAtlasTexture };
                m_FontAtlasTexture.name = tex_FileName + " Atlas";
                fontAssetBridge.SetAtlasWidth(m_AtlasWidth);
                fontAssetBridge.SetAtlasHeight(m_AtlasHeight);
                fontAssetBridge.SetAtlasPadding(m_Padding);

                AssetDatabase.AddObjectToAsset(m_FontAtlasTexture, fontAsset);

                // Create new Material and Add it as Sub-Asset
                var default_Shader = Shader.Find("TextMeshPro/Bitmap"); // m_shaderSelection;
                var tmp_material = new Material(default_Shader);
                tmp_material.name = tex_FileName + " Material";
                tmp_material.SetTexture(ShaderUtilities.ID_MainTex, m_FontAtlasTexture);
                fontAsset.material = tmp_material;

                AssetDatabase.AddObjectToAsset(tmp_material, fontAsset);
            }
            else
            {
                // Find all Materials referencing this font atlas.
                var material_references = TMP_EditorUtility.FindMaterialReferences(fontAsset);

                // Set version number of font asset
                fontAssetBridge.SetVersion("1.1.0");

                // Special handling to remove legacy font asset data
                var infoList = fontAssetBridge.GetGlyphInfoList();
                if (infoList != null && infoList.Count > 0)
                    fontAssetBridge.SetGlyphInfoList(null);

                //Set Font Asset Type
                fontAssetBridge.SetAtlasRenderMode(m_GlyphRenderMode);

                // Add FaceInfo to Font Asset
                fontAsset.faceInfo = m_FaceInfo;

                // Add GlyphInfo[] to Font Asset
                fontAssetBridge.SetGlyphTable(m_FontGlyphTable);

                // Add CharacterTable[] to font asset.
                fontAssetBridge.SetCharacterTable(m_FontCharacterTable);

                // Sort glyph and character tables.
                fontAssetBridge.SortAllTables();

                // Get and Add Kerning Pairs to Font Asset
                if (m_IncludeFontFeatures)
                    fontAssetBridge.SetFontFeatureTable(GetKerningTable());

                // Destroy Assets that will be replaced.
                if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0)
                    for (var i = 1; i < fontAsset.atlasTextures.Length; i++)
                        DestroyImmediate(fontAsset.atlasTextures[i], true);

                fontAssetBridge.SetAtlasTextureIndex(0);
                fontAssetBridge.SetAtlasWidth(m_AtlasWidth);
                fontAssetBridge.SetAtlasHeight(m_AtlasHeight);
                fontAssetBridge.SetAtlasPadding(m_Padding);

                // Make sure remaining atlas texture is of the correct size
                var tex = fontAsset.atlasTextures[0];
                tex.name = tex_FileName + " Atlas";

                // Make texture readable to allow resizing
                var isReadableState = tex.isReadable;
                if (isReadableState == false)
                    FontEngineEditorUtilitiesBridge.SetAtlasTextureIsReadable(tex, true);

                if (tex.width != m_AtlasWidth || tex.height != m_AtlasHeight)
                {
                    tex.Reinitialize(m_AtlasWidth, m_AtlasHeight);
                    tex.Apply(false);
                }

                // Copy new texture data to existing texture
                Graphics.CopyTexture(m_FontAtlasTexture, tex);

                // Apply changes to the texture.
                tex.Apply(false);

                // Special handling due to a bug in earlier versions of Unity.
                m_FontAtlasTexture.hideFlags = HideFlags.None;
                fontAsset.material.hideFlags = HideFlags.None;

                // Update the Texture reference on the Material
                //for (int i = 0; i < material_references.Length; i++)
                //{
                //    material_references[i].SetFloat(ShaderUtilities.ID_TextureWidth, tex.width);
                //    material_references[i].SetFloat(ShaderUtilities.ID_TextureHeight, tex.height);

                //    int spread = m_Padding;
                //    material_references[i].SetFloat(ShaderUtilities.ID_GradientScale, spread);

                //    material_references[i].SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
                //    material_references[i].SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);
                //}
            }

            // Set texture to non readable
            FontEngineEditorUtilitiesBridge.SetAtlasTextureIsReadable(fontAsset.atlasTexture, false);

            // Add list of GlyphRects to font asset.
            fontAssetBridge.SetFreeGlyphRects(m_FreeGlyphRects);
            fontAssetBridge.SetUsedGlyphRects(m_UsedGlyphRects);

            // Save Font Asset creation settings
            m_SelectedFontAsset = fontAsset;
            m_LegacyFontAsset = null;
            fontAsset.creationSettings = SaveFontCreationSettings();

            AssetDatabase.SaveAssets();

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(fontAsset)); // Re-import font asset to get the new updated version.

            //EditorUtility.SetDirty(font_asset);
            fontAsset.ReadFontAssetDefinition();

            AssetDatabase.Refresh();

            m_FontAtlasTexture = null;

            // NEED TO GENERATE AN EVENT TO FORCE A REDRAW OF ANY TEXTMESHPRO INSTANCES THAT MIGHT BE USING THIS FONT ASSET
            TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, fontAsset);
        }


        private static void Save_SDF_FontAsset(string filePath)
        {
            filePath = filePath.Substring(0, filePath.Length - 6); // Trim file extension from filePath.

            var dataPath = Application.dataPath;

            if (filePath.IndexOf(dataPath, StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                Debug.LogError(
                    "You're saving the font asset in a directory outside of this project folder. This is not supported. Please select a directory under \"" +
                    dataPath + "\"");
                return;
            }

            var relativeAssetPath = filePath.Substring(dataPath.Length - 6);
            var tex_DirName = Path.GetDirectoryName(relativeAssetPath);
            var tex_FileName = Path.GetFileNameWithoutExtension(relativeAssetPath);
            var tex_Path_NoExt = tex_DirName + "/" + tex_FileName;


            // Check if TextMeshPro font asset already exists. If not, create a new one. Otherwise update the existing one.
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(tex_Path_NoExt + ".asset");
            var fontAssetBridge = new FontAssetBridge(fontAsset);
            if (fontAsset == null)
            {
                //Debug.Log("Creating TextMeshPro font asset!");
                fontAsset = CreateInstance<TMP_FontAsset>(); // Create new TextMeshPro Font Asset.
                AssetDatabase.CreateAsset(fontAsset, tex_Path_NoExt + ".asset");

                fontAssetBridge = new FontAssetBridge(fontAsset);
                // Set version number of font asset
                fontAssetBridge.SetVersion("1.1.0");

                // Reference to source font file GUID.
                fontAssetBridge.SetSourceFontFile_EditorRef((UnityEngine.Font)m_SourceFontFile);
                fontAssetBridge.SetSourceFontFileGuid(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_SourceFontFile)));

                //Set Font Asset Type
                fontAssetBridge.SetAtlasRenderMode(m_GlyphRenderMode);

                // Add FaceInfo to Font Asset
                fontAsset.faceInfo = m_FaceInfo;

                // Add GlyphInfo[] to Font Asset
                fontAssetBridge.SetGlyphTable(m_FontGlyphTable);

                // Add CharacterTable[] to font asset.
                fontAssetBridge.SetCharacterTable(m_FontCharacterTable);

                // Sort glyph and character tables.
                fontAssetBridge.SortAllTables();

                // Get and Add Kerning Pairs to Font Asset
                if (m_IncludeFontFeatures)
                    fontAssetBridge.SetFontFeatureTable(GetKerningTable());

                // Add Font Atlas as Sub-Asset
                fontAsset.atlasTextures = new[] { m_FontAtlasTexture };
                m_FontAtlasTexture.name = tex_FileName + " Atlas";
                fontAssetBridge.SetAtlasWidth(m_AtlasWidth);
                fontAssetBridge.SetAtlasHeight(m_AtlasHeight);
                fontAssetBridge.SetAtlasPadding(m_Padding);

                AssetDatabase.AddObjectToAsset(m_FontAtlasTexture, fontAsset);

                // Create new Material and Add it as Sub-Asset
                var default_Shader = Shader.Find("TextMeshPro/Distance Field");
                var tmp_material = new Material(default_Shader);

                tmp_material.name = tex_FileName + " Material";
                tmp_material.SetTexture(ShaderUtilities.ID_MainTex, m_FontAtlasTexture);
                tmp_material.SetFloat(ShaderUtilities.ID_TextureWidth, m_FontAtlasTexture.width);
                tmp_material.SetFloat(ShaderUtilities.ID_TextureHeight, m_FontAtlasTexture.height);

                var spread = m_Padding + 1;
                tmp_material.SetFloat(ShaderUtilities.ID_GradientScale, spread); // Spread = Padding for Brute Force SDF.

                tmp_material.SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
                tmp_material.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);

                fontAsset.material = tmp_material;

                AssetDatabase.AddObjectToAsset(tmp_material, fontAsset);
            }
            else
            {
                // Find all Materials referencing this font atlas.
                var material_references = TMP_EditorUtility.FindMaterialReferences(fontAsset);

                // Set version number of font asset
                fontAssetBridge.SetVersion("1.1.0");

                // Special handling to remove legacy font asset data
                var infoList = fontAssetBridge.GetGlyphInfoList();
                if (infoList != null && infoList.Count > 0)
                    fontAssetBridge.SetGlyphInfoList(null);

                //Set Font Asset Type
                fontAssetBridge.SetAtlasRenderMode(m_GlyphRenderMode);

                // Add FaceInfo to Font Asset
                fontAsset.faceInfo = m_FaceInfo;

                // Add GlyphInfo[] to Font Asset
                fontAssetBridge.SetGlyphTable(m_FontGlyphTable);

                // Add CharacterTable[] to font asset.
                fontAssetBridge.SetCharacterTable(m_FontCharacterTable);

                // Sort glyph and character tables.
                fontAssetBridge.SortAllTables();

                // Get and Add Kerning Pairs to Font Asset
                // TODO: Check and preserve existing adjustment pairs.
                if (m_IncludeFontFeatures)
                    fontAssetBridge.SetFontFeatureTable(GetKerningTable());

                // Destroy Assets that will be replaced.
                if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0)
                    for (var i = 1; i < fontAsset.atlasTextures.Length; i++)
                        DestroyImmediate(fontAsset.atlasTextures[i], true);

                fontAssetBridge.SetAtlasTextureIndex(0);
                fontAssetBridge.SetAtlasWidth(m_AtlasWidth);
                fontAssetBridge.SetAtlasHeight(m_AtlasHeight);
                fontAssetBridge.SetAtlasPadding(m_Padding);

                // Make sure remaining atlas texture is of the correct size
                var tex = fontAsset.atlasTextures[0];
                tex.name = tex_FileName + " Atlas";

                // Make texture readable to allow resizing
                var isReadableState = tex.isReadable;
                if (isReadableState == false)
                    FontEngineEditorUtilitiesBridge.SetAtlasTextureIsReadable(tex, true);

                if (tex.width != m_AtlasWidth || tex.height != m_AtlasHeight)
                {
                    tex.Reinitialize(m_AtlasWidth, m_AtlasHeight);
                    tex.Apply(false);
                }

                // Copy new texture data to existing texture
                Graphics.CopyTexture(m_FontAtlasTexture, tex);

                // Apply changes to the texture.
                tex.Apply(false);

                // Special handling due to a bug in earlier versions of Unity.
                m_FontAtlasTexture.hideFlags = HideFlags.None;
                fontAsset.material.hideFlags = HideFlags.None;

                // Update the Texture reference on the Material
                for (var i = 0; i < material_references.Length; i++)
                {
                    material_references[i].SetFloat(ShaderUtilities.ID_TextureWidth, tex.width);
                    material_references[i].SetFloat(ShaderUtilities.ID_TextureHeight, tex.height);

                    var spread = m_Padding + 1;
                    material_references[i].SetFloat(ShaderUtilities.ID_GradientScale, spread);

                    material_references[i].SetFloat(ShaderUtilities.ID_WeightNormal, fontAsset.normalStyle);
                    material_references[i].SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);
                }
            }

            // Saving File for Debug
            //var pngData = destination_Atlas.EncodeToPNG();
            //File.WriteAllBytes("Assets/Textures/Debug Distance Field.png", pngData);

            // Set texture to non readable
            FontEngineEditorUtilitiesBridge.SetAtlasTextureIsReadable(fontAsset.atlasTexture, false);

            // Add list of GlyphRects to font asset.
            fontAssetBridge.SetFreeGlyphRects(m_FreeGlyphRects);
            fontAssetBridge.SetUsedGlyphRects(m_UsedGlyphRects);

            // Save Font Asset creation settings
            m_SelectedFontAsset = fontAsset;
            m_LegacyFontAsset = null;
            fontAsset.creationSettings = SaveFontCreationSettings();

            AssetDatabase.SaveAssets();

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(fontAsset)); // Re-import font asset to get the new updated version.

            fontAsset.ReadFontAssetDefinition();

            AssetDatabase.Refresh();

            m_FontAtlasTexture = null;

            // NEED TO GENERATE AN EVENT TO FORCE A REDRAW OF ANY TEXTMESHPRO INSTANCES THAT MIGHT BE USING THIS FONT ASSET
            TMPro_EventManager.ON_FONT_PROPERTY_CHANGED(true, fontAsset);
        }


        /// <summary>
        ///     Internal method to save the Font Asset Creation Settings
        /// </summary>
        /// <returns></returns>
        private static FontAssetCreationSettings SaveFontCreationSettings()
        {
            var settings = new FontAssetCreationSettings();

            //settings.sourceFontFileName = m_SourceFontFile.name;
            settings.sourceFontFileGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_SourceFontFile));
            settings.pointSizeSamplingMode = m_PointSizeSamplingMode;
            settings.pointSize = m_PointSize;
            settings.padding = m_Padding;
            settings.packingMode = (int)m_PackingMode;
            settings.atlasWidth = m_AtlasWidth;
            settings.atlasHeight = m_AtlasHeight;
            settings.characterSetSelectionMode = m_CharacterSetSelectionMode;
            settings.characterSequence = m_CharacterSequence;
            settings.referencedFontAssetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_ReferencedFontAsset));
            settings.referencedTextAssetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m_CharactersFromFile));
            //settings.fontStyle = (int)m_FontStyle;
            //settings.fontStyleModifier = m_FontStyleValue;
            settings.renderMode = (int)m_GlyphRenderMode;
            settings.includeFontFeatures = m_IncludeFontFeatures;

            return settings;
        }


        /// <summary>
        ///     Internal method to load the Font Asset Creation Settings
        /// </summary>
        /// <param name="settings"></param>
        private void LoadFontCreationSettings(FontAssetCreationSettings settings)
        {
            m_SourceFontFile = AssetDatabase.LoadAssetAtPath<UnityEngine.Font>(AssetDatabase.GUIDToAssetPath(settings.sourceFontFileGUID));
            m_PointSizeSamplingMode = settings.pointSizeSamplingMode;
            m_PointSize = settings.pointSize;
            m_Padding = settings.padding;
            m_PackingMode = (FontPackingModes)settings.packingMode;
            m_AtlasWidth = settings.atlasWidth;
            m_AtlasHeight = settings.atlasHeight;
            m_CharacterSetSelectionMode = settings.characterSetSelectionMode;
            m_CharacterSequence = settings.characterSequence;
            m_ReferencedFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(settings.referencedFontAssetGUID));
            m_CharactersFromFile = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(settings.referencedTextAssetGUID));
            //m_FontStyle = (FaceStyles)settings.fontStyle;
            //m_FontStyleValue = settings.fontStyleModifier;
            m_GlyphRenderMode = (GlyphRenderMode)settings.renderMode;
            m_IncludeFontFeatures = settings.includeFontFeatures;
        }


        /// <summary>
        ///     Save the latest font asset creation settings to EditorPrefs.
        /// </summary>
        /// <param name="settings"></param>
        private static void SaveCreationSettingsToEditorPrefs(FontAssetCreationSettings settings)
        {
            // Create new list if one does not already exist
            if (m_FontAssetCreationSettingsContainer == null)
            {
                m_FontAssetCreationSettingsContainer = new FontAssetCreationSettingsContainer();
                m_FontAssetCreationSettingsContainer.fontAssetCreationSettings = new List<FontAssetCreationSettings>();
            }

            // Add new creation settings to the list
            m_FontAssetCreationSettingsContainer.fontAssetCreationSettings.Add(settings);

            // Since list should only contain the most 4 recent settings, we remove the first element if list exceeds 4 elements.
            if (m_FontAssetCreationSettingsContainer.fontAssetCreationSettings.Count > 4)
                m_FontAssetCreationSettingsContainer.fontAssetCreationSettings.RemoveAt(0);

            m_FontAssetCreationSettingsCurrentIndex = m_FontAssetCreationSettingsContainer.fontAssetCreationSettings.Count - 1;

            // Serialize list to JSON
            var serializedSettings = JsonUtility.ToJson(m_FontAssetCreationSettingsContainer, true);

            EditorPrefs.SetString(k_FontAssetCreationSettingsContainerKey, serializedSettings);
        }

        private void DrawPreview()
        {
            Rect pixelRect;

            var ratioX = (position.width - k_TwoColumnControlsWidth) / m_AtlasWidth;
            var ratioY = (position.height - 15) / m_AtlasHeight;

            if (position.width < position.height)
            {
                ratioX = (position.width - 15) / m_AtlasWidth;
                ratioY = (position.height - 485) / m_AtlasHeight;
            }

            if (ratioX < ratioY)
            {
                var width = m_AtlasWidth * ratioX;
                var height = m_AtlasHeight * ratioX;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(width), GUILayout.MaxHeight(height));

                pixelRect = GUILayoutUtility.GetRect(width - 5, height, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
            }
            else
            {
                var width = m_AtlasWidth * ratioY;
                var height = m_AtlasHeight * ratioY;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(width), GUILayout.MaxHeight(height));

                pixelRect = GUILayoutUtility.GetRect(width - 5, height, GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));
            }

            if (m_FontAtlasTexture != null)
            {
                EditorGUI.DrawTextureAlpha(pixelRect, m_FontAtlasTexture, ScaleMode.StretchToFill);

                // Destroy GlyphRect preview texture
                if (m_GlyphRectPreviewTexture != null)
                {
                    DestroyImmediate(m_GlyphRectPreviewTexture);
                    m_GlyphRectPreviewTexture = null;
                }
            }
            else if (m_GlyphRectPreviewTexture != null)
            {
                EditorGUI.DrawPreviewTexture(pixelRect, m_GlyphRectPreviewTexture, null, ScaleMode.StretchToFill);
            }
            else if (m_SavedFontAtlas != null)
            {
                EditorGUI.DrawTextureAlpha(pixelRect, m_SavedFontAtlas, ScaleMode.StretchToFill);
            }

            EditorGUILayout.EndVertical();
        }


        private void CheckForLegacyGlyphRenderMode()
        {
            // Special handling for legacy glyph render mode
            if ((int)m_GlyphRenderMode < 0x100)
                switch ((int)m_GlyphRenderMode)
                {
                    case 0:
                        m_GlyphRenderMode = GlyphRenderMode.SMOOTH_HINTED;
                        break;
                    case 1:
                        m_GlyphRenderMode = GlyphRenderMode.SMOOTH;
                        break;
                    case 2:
                        m_GlyphRenderMode = GlyphRenderMode.RASTER_HINTED;
                        break;
                    case 3:
                        m_GlyphRenderMode = GlyphRenderMode.RASTER;
                        break;
                    case 6:
                    case 7:
                        m_GlyphRenderMode = GlyphRenderMode.SDFAA;
                        break;
                }
        }


        // Get Kerning Pairs
        public static TMP_FontFeatureTable GetKerningTable()
        {
            var adjustmentRecords = FontEngineBridge.GetGlyphPairAdjustmentTable(m_AvailableGlyphsToAdd.ToArray());

            if (adjustmentRecords == null)
                return null;

            var fontFeatureTable = new TMP_FontFeatureTable();

            for (var i = 0; i < adjustmentRecords.Length && adjustmentRecords[i].firstAdjustmentRecord.glyphIndex != 0; i++)
                fontFeatureTable.glyphPairAdjustmentRecords.Add(TMPGlyphPairAdjustmentRecordBridge.CreateAdjustmentRecord(adjustmentRecords[i]));

            fontFeatureTable.SortGlyphPairAdjustmentRecords();

            return fontFeatureTable;
        }

        [MenuItem("MokomoGames/Font/全言語のフォントアトラス生成")]
        public static void GenerateFontAtlasAllFromEditor()
        {
            processFromEditor = true;
            ClearGeneratedReports();
            foreach (var convertConfig in ConvertConfigTable) GenerateFontAtlasFromEditor(convertConfig);

            processFromEditor = false;
            ProcessSaveDataOnExitApplication();
        }

        private static void GenerateFontAtlasFromEditor(ConverterConfig config)
        {
            var fontsDir = Path.Combine("Assets", "MokomoGames", "Localization", "Fonts");
            var fontDataDir = Path.Combine(fontsDir, "FontDataList");
            var fontDataName = config.FontDataName;
            m_SourceFontFile = AssetDatabase.LoadAssetAtPath<UnityEngine.Font>(Path.Combine(fontDataDir, fontDataName));
            // NOTE: カスタムサイズを使用。
            m_PointSizeSamplingMode = 1;
            m_PointSize = 80;
            m_Padding = 5;
            m_PackingMode = FontPackingModes.Optimum;
            // TODO: レポートを見つつ、手動で調整
            m_AtlasWidth = 1024;
            m_AtlasHeight = 1024;
            // NOTE: カスタムキャラクタを使用
            m_CharacterSetSelectionMode = 7;
            var characterListFileName = config.CharacterListFileName;
            var characterListDir = Path.Combine(fontsDir, "CharacterList");
            var characterListFilePath = Path.Combine(characterListDir, characterListFileName);
            var fontData = AssetDatabase.LoadAssetAtPath<TextAsset>(characterListFilePath);
            m_CharacterSequence = fontData.text;
            m_ReferencedFontAsset = null;
            m_GlyphRenderMode = GlyphRenderMode.SDF16;
            m_IncludeFontFeatures = false;

            GenerateFontAtlas();
            StoreAtlasTexture();
            SaveAtlas(config.FontDataName);
            ApplyGlyphPackingResult();
            CleanupAfterRenderTextureProcess();
        }

        private static void SaveAtlas(string fontDataName)
        {
            var bitmapFontDir = Path.Combine(Application.dataPath, "MokomoGames", "Localization", "Fonts", "Resources", "BitMap");
            var saveFileName = $"{Path.GetFileNameWithoutExtension(fontDataName)}.asset";
            var saveFilePath = Path.Combine(bitmapFontDir, saveFileName);
            Save_SDF_FontAsset(saveFilePath);
        }

        private static void GenerateFontAtlas()
        {
            if (!m_IsProcessing && m_SourceFontFile != null)
            {
                DestroyImmediate(m_FontAtlasTexture);
                DestroyImmediate(m_GlyphRectPreviewTexture);
                m_FontAtlasTexture = null;
                m_SavedFontAtlas = null;
                m_OutputFeedback = string.Empty;

                // Initialize font engine
                var errorCode = FontEngine.InitializeFontEngine();
                if (errorCode != FontEngineError.Success)
                    Debug.Log("Font Asset Creator - Error [" + errorCode +
                              "] has occurred while Initializing the FreeType Library.");

                // Get file path of the source font file.
                var fontPath = AssetDatabase.GetAssetPath(m_SourceFontFile);

                if (errorCode == FontEngineError.Success)
                {
                    errorCode = FontEngine.LoadFontFace(fontPath);

                    if (errorCode != FontEngineError.Success)
                        Debug.Log(
                            "Font Asset Creator - Error Code [" + errorCode + "] has occurred trying to load the [" +
                            m_SourceFontFile.name +
                            "] font file. This typically results from the use of an incompatible or corrupted font file.",
                            m_SourceFontFile);
                }


                // Define an array containing the characters we will render.
                if (errorCode == FontEngineError.Success)
                {
                    uint[] characterSet = null;

                    // Get list of characters that need to be packed and rendered to the atlas texture.
                    if (m_CharacterSetSelectionMode == 7 || m_CharacterSetSelectionMode == 8)
                    {
                        var char_List = new List<uint>();

                        for (var i = 0; i < m_CharacterSequence.Length; i++)
                        {
                            uint unicode = m_CharacterSequence[i];

                            // Handle surrogate pairs
                            if (i < m_CharacterSequence.Length - 1 && char.IsHighSurrogate((char)unicode) &&
                                char.IsLowSurrogate(m_CharacterSequence[i + 1]))
                            {
                                unicode = (uint)char.ConvertToUtf32(m_CharacterSequence[i], m_CharacterSequence[i + 1]);
                                i += 1;
                            }

                            // Check to make sure we don't include duplicates
                            if (char_List.FindIndex(item => item == unicode) == -1)
                                char_List.Add(unicode);
                        }

                        characterSet = char_List.ToArray();
                    }
                    else if (m_CharacterSetSelectionMode == 6)
                    {
                        characterSet = ParseHexNumberSequence(m_CharacterSequence);
                    }
                    else
                    {
                        characterSet = ParseNumberSequence(m_CharacterSequence);
                    }

                    m_CharacterCount = characterSet.Length;

                    m_AtlasGenerationProgress = 0;
                    m_IsProcessing = true;
                    m_IsGenerationCancelled = false;

                    var glyphLoadFlags =
                        ((int)m_GlyphRenderMode & GlyphRasterModesBridge.GetRasterModeHinted()) ==
                        GlyphRasterModesBridge.GetRasterModeHinted()
                            ? GlyphLoadFlags.LOAD_RENDER
                            : GlyphLoadFlags.LOAD_RENDER | GlyphLoadFlags.LOAD_NO_HINTING;

                    glyphLoadFlags = ((int)m_GlyphRenderMode & GlyphRasterModesBridge.GetRasterModeMono()) ==
                                     GlyphRasterModesBridge.GetRasterModeMono()
                        ? glyphLoadFlags | GlyphLoadFlags.LOAD_MONOCHROME
                        : glyphLoadFlags;

                    // Start Stop Watch
                    m_StopWatch = Stopwatch.StartNew();

                    // Clear the various lists used in the generation process.
                    m_AvailableGlyphsToAdd.Clear();
                    m_MissingCharacters.Clear();
                    m_ExcludedCharacters.Clear();
                    m_CharacterLookupMap.Clear();
                    m_GlyphLookupMap.Clear();
                    m_GlyphsToPack.Clear();
                    m_GlyphsPacked.Clear();

                    // Check if requested characters are available in the source font file.
                    for (var i = 0; i < characterSet.Length; i++)
                    {
                        var unicode = characterSet[i];
                        uint glyphIndex;

                        if (FontEngine.TryGetGlyphIndex(unicode, out glyphIndex))
                        {
                            // Skip over potential duplicate characters.
                            if (m_CharacterLookupMap.ContainsKey(unicode))
                                continue;

                            // Add character to character lookup map.
                            m_CharacterLookupMap.Add(unicode, glyphIndex);

                            // Skip over potential duplicate glyph references.
                            if (m_GlyphLookupMap.ContainsKey(glyphIndex))
                            {
                                // Add additional glyph reference for this character.
                                m_GlyphLookupMap[glyphIndex].Add(unicode);
                                continue;
                            }

                            // Add glyph reference to glyph lookup map.
                            m_GlyphLookupMap.Add(glyphIndex, new List<uint> { unicode });

                            // Add glyph index to list of glyphs to add to texture.
                            m_AvailableGlyphsToAdd.Add(glyphIndex);
                        }
                        else
                        {
                            // Add Unicode to list of missing characters.
                            m_MissingCharacters.Add(unicode);
                        }
                    }

                    // Pack available glyphs in the provided texture space.
                    if (m_AvailableGlyphsToAdd.Count > 0)
                    {
                        var packingModifier =
                            ((int)m_GlyphRenderMode & GlyphRasterModesBridge.GetRasterModeBitMap()) ==
                            GlyphRasterModesBridge.GetRasterModeBitMap()
                                ? 0
                                : 1;

                        if (m_PointSizeSamplingMode == 0) // Auto-Sizing Point Size Mode
                        {
                            // Estimate min / max range for auto sizing of point size.
                            var minPointSize = 0;
                            var maxPointSize =
                                (int)Mathf.Sqrt(m_AtlasWidth * m_AtlasHeight / m_AvailableGlyphsToAdd.Count) * 3;

                            m_PointSize = (maxPointSize + minPointSize) / 2;

                            var optimumPointSizeFound = false;
                            for (var iteration = 0; iteration < 15 && optimumPointSizeFound == false; iteration++)
                            {
                                m_AtlasGenerationProgressLabel = "Packing glyphs - Pass (" + iteration + ")";

                                FontEngine.SetFaceSize(m_PointSize);

                                m_GlyphsToPack.Clear();
                                m_GlyphsPacked.Clear();

                                m_FreeGlyphRects.Clear();
                                m_FreeGlyphRects.Add(new GlyphRect(0, 0, m_AtlasWidth - packingModifier,
                                    m_AtlasHeight - packingModifier));
                                m_UsedGlyphRects.Clear();

                                for (var i = 0; i < m_AvailableGlyphsToAdd.Count; i++)
                                {
                                    var glyphIndex = m_AvailableGlyphsToAdd[i];
                                    Glyph glyph;

                                    if (FontEngine.TryGetGlyphWithIndexValue(glyphIndex, glyphLoadFlags, out glyph))
                                    {
                                        if (glyph.glyphRect.width > 0 && glyph.glyphRect.height > 0)
                                            m_GlyphsToPack.Add(glyph);
                                        else
                                            m_GlyphsPacked.Add(glyph);
                                    }
                                }

                                FontEngineBridge.TryPackGlyphsInAtlas(
                                    m_GlyphsToPack,
                                    m_GlyphsPacked,
                                    m_Padding,
                                    (GlyphPackingMode)m_PackingMode,
                                    m_GlyphRenderMode,
                                    m_AtlasWidth,
                                    m_AtlasHeight,
                                    m_FreeGlyphRects,
                                    m_UsedGlyphRects
                                );

                                if (m_IsGenerationCancelled)
                                {
                                    DestroyImmediate(m_FontAtlasTexture);
                                    m_FontAtlasTexture = null;
                                    return;
                                }

                                //Debug.Log("Glyphs remaining to add [" + m_GlyphsToAdd.Count + "]. Glyphs added [" + m_GlyphsAdded.Count + "].");

                                if (m_GlyphsToPack.Count > 0)
                                {
                                    if (m_PointSize > minPointSize)
                                    {
                                        maxPointSize = m_PointSize;
                                        m_PointSize = (m_PointSize + minPointSize) / 2;

                                        //Debug.Log("Decreasing point size from [" + maxPointSize + "] to [" + m_PointSize + "].");
                                    }
                                }
                                else
                                {
                                    if (maxPointSize - minPointSize > 1 && m_PointSize < maxPointSize)
                                    {
                                        minPointSize = m_PointSize;
                                        m_PointSize = (m_PointSize + maxPointSize) / 2;

                                        //Debug.Log("Increasing point size from [" + minPointSize + "] to [" + m_PointSize + "].");
                                    }
                                    else
                                    {
                                        //Debug.Log("[" + iteration + "] iterations to find the optimum point size of : [" + m_PointSize + "].");
                                        optimumPointSizeFound = true;
                                    }
                                }
                            }
                        }
                        else // Custom Point Size Mode
                        {
                            m_AtlasGenerationProgressLabel = "Packing glyphs...";

                            // Set point size
                            FontEngine.SetFaceSize(m_PointSize);

                            m_GlyphsToPack.Clear();
                            m_GlyphsPacked.Clear();

                            m_FreeGlyphRects.Clear();
                            m_FreeGlyphRects.Add(new GlyphRect(0, 0, m_AtlasWidth - packingModifier,
                                m_AtlasHeight - packingModifier));
                            m_UsedGlyphRects.Clear();

                            for (var i = 0; i < m_AvailableGlyphsToAdd.Count; i++)
                            {
                                var glyphIndex = m_AvailableGlyphsToAdd[i];
                                Glyph glyph;

                                if (FontEngine.TryGetGlyphWithIndexValue(glyphIndex, glyphLoadFlags, out glyph))
                                {
                                    if (glyph.glyphRect.width > 0 && glyph.glyphRect.height > 0)
                                        m_GlyphsToPack.Add(glyph);
                                    else
                                        m_GlyphsPacked.Add(glyph);
                                }
                            }

                            FontEngineBridge.TryPackGlyphsInAtlas(
                                m_GlyphsToPack,
                                m_GlyphsPacked,
                                m_Padding,
                                (GlyphPackingMode)m_PackingMode,
                                m_GlyphRenderMode,
                                m_AtlasWidth,
                                m_AtlasHeight,
                                m_FreeGlyphRects,
                                m_UsedGlyphRects
                            );

                            if (m_IsGenerationCancelled)
                            {
                                DestroyImmediate(m_FontAtlasTexture);
                                m_FontAtlasTexture = null;
                                return;
                            }
                            //Debug.Log("Glyphs remaining to add [" + m_GlyphsToAdd.Count + "]. Glyphs added [" + m_GlyphsAdded.Count + "].");
                        }
                    }
                    else
                    {
                        var packingModifier =
                            ((int)m_GlyphRenderMode & GlyphRasterModesBridge.GetRasterModeBitMap()) ==
                            GlyphRasterModesBridge.GetRasterModeBitMap()
                                ? 0
                                : 1;

                        FontEngine.SetFaceSize(m_PointSize);

                        m_GlyphsToPack.Clear();
                        m_GlyphsPacked.Clear();

                        m_FreeGlyphRects.Clear();
                        m_FreeGlyphRects.Add(new GlyphRect(0, 0, m_AtlasWidth - packingModifier,
                            m_AtlasHeight - packingModifier));
                        m_UsedGlyphRects.Clear();
                    }

                    //Stop StopWatch
                    m_StopWatch.Stop();
                    m_GlyphPackingGenerationTime = m_StopWatch.Elapsed.TotalMilliseconds;
                    m_IsGlyphPackingDone = true;
                    m_StopWatch.Reset();

                    m_FontCharacterTable.Clear();
                    m_FontGlyphTable.Clear();
                    m_GlyphsToRender.Clear();

                    // Handle Results and potential cancellation of glyph rendering
                    if (m_GlyphRenderMode == GlyphRenderMode.SDF32 && m_PointSize > 512 ||
                        m_GlyphRenderMode == GlyphRenderMode.SDF16 && m_PointSize > 1024 ||
                        m_GlyphRenderMode == GlyphRenderMode.SDF8 && m_PointSize > 2048)
                    {
                        var upSampling = 1;
                        switch (m_GlyphRenderMode)
                        {
                            case GlyphRenderMode.SDF8:
                                upSampling = 8;
                                break;
                            case GlyphRenderMode.SDF16:
                                upSampling = 16;
                                break;
                            case GlyphRenderMode.SDF32:
                                upSampling = 32;
                                break;
                        }

                        Debug.Log("Glyph rendering has been aborted due to sampling point size of [" + m_PointSize +
                                  "] x SDF [" + upSampling +
                                  "] up sampling exceeds 16,384 point size. Please revise your generation settings to make sure the sampling point size x SDF up sampling mode does not exceed 16,384.");

                        m_IsRenderingDone = true;
                        m_AtlasGenerationProgress = 0;
                        m_IsGenerationCancelled = true;
                    }

                    // Add glyphs and characters successfully added to texture to their respective font tables.
                    foreach (var glyph in m_GlyphsPacked)
                    {
                        var glyphIndex = glyph.index;

                        m_FontGlyphTable.Add(glyph);

                        // Add glyphs to list of glyphs that need to be rendered.
                        if (glyph.glyphRect.width > 0 && glyph.glyphRect.height > 0)
                            m_GlyphsToRender.Add(glyph);

                        foreach (var unicode in m_GlyphLookupMap[glyphIndex])
                            // Create new Character
                            m_FontCharacterTable.Add(new TMP_Character(unicode, glyph));
                    }

                    //
                    foreach (var glyph in m_GlyphsToPack)
                    foreach (var unicode in m_GlyphLookupMap[glyph.index])
                        m_ExcludedCharacters.Add(unicode);

                    // Get the face info for the current sampling point size.
                    m_FaceInfo = FontEngine.GetFaceInfo();

                    if (m_IsGenerationCancelled == false)
                    {
                        // Start Stop Watch
                        m_StopWatch = Stopwatch.StartNew();

                        m_IsRenderingDone = false;

                        // Allocate texture data
                        Debug.Log("allocate byte");
                        m_AtlasTextureBuffer = new byte[m_AtlasWidth * m_AtlasHeight];

                        m_AtlasGenerationProgressLabel = "Rendering glyphs...";

                        // Render and add glyphs to the given atlas texture.
                        if (m_GlyphsToRender.Count > 0)
                            FontEngineBridge.RenderGlyphsToTexture(
                                m_GlyphsToRender,
                                m_Padding,
                                m_GlyphRenderMode,
                                m_AtlasTextureBuffer,
                                m_AtlasWidth,
                                m_AtlasHeight
                            );

                        m_IsRenderingDone = true;

                        // Stop StopWatch
                        m_StopWatch.Stop();
                        m_GlyphRenderingGenerationTime = m_StopWatch.Elapsed.TotalMilliseconds;
                        m_IsGlyphRenderingDone = true;
                        m_StopWatch.Reset();
                    }
                }

                SaveCreationSettingsToEditorPrefs(SaveFontCreationSettings());
            }
        }

        [Serializable]
        private class FontAssetCreationSettingsContainer
        {
            public List<FontAssetCreationSettings> fontAssetCreationSettings;
        }

        private enum FontPackingModes
        {
            Fast = 0,
            Optimum = 4
        }


        internal enum TestEnum
        {
            A,
            B,
            C
        }

        private class ReportContent
        {
            public List<uint> ExcludedCharacterList = new();
            public string FontName;
            public List<uint> IncludeCharacterList = new();
            public List<uint> MissingCharacterList = new();
            public int Padding;
            public int PointSize;
        }

        public class ConverterConfig
        {
            public string CharacterListFileName;
            public string FontDataName;
        }
    }
}