using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReactiveStudy.Clicker.Editor
{
    /// <summary>
    /// 2부(따라하기) 리액티브 클리커 씬을 코드로 자동 생성한다.
    /// 메뉴: Tools > Reactive Study > 2부 클리커 씬 생성
    ///
    /// 수업에서는 교안 06장의 단계를 따라 학생이 직접 만드는 것이 기본이고,
    /// 이 씬은 완성본 확인·시연용 보조 자료다.
    /// </summary>
    public static class ClickerGameSceneCreator
    {
        private const string ScenePath = "Assets/Scenes/ReactiveClicker.unity";
        private const string KoreanFontPath = "Assets/Fonts/Pretendard-Regular SDF.asset";
        private static TMP_FontAsset s_KoreanFont;

        [MenuItem("Tools/Reactive Study/2부 클리커 씬 생성")]
        public static void CreateSceneInteractive()
        {
            if (
                !EditorUtility.DisplayDialog(
                    "2부 클리커 씬 생성",
                    "새 씬을 만들어 리액티브 클리커(완성본)를 구성합니다.\n"
                        + "현재 열린 씬은 닫히며, 저장하지 않은 변경은 먼저 저장 여부를 묻습니다.\n\n계속하시겠습니까?",
                    "생성",
                    "취소"
                )
            )
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            CreateScene();
            EditorUtility.DisplayDialog("완료", "클리커 씬이 생성되었습니다:\n" + ScenePath, "확인");
        }

        /// <summary>대화상자 없이 씬을 생성한다(자동화·배치 실행용).</summary>
        public static void CreateScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Canvas canvas = CreateCanvasAndEventSystem();

            TextMeshProUGUI title = CreateText(canvas.transform, "Title", "리액티브 클리커", 42, FontStyles.Bold);
            SetAnchorTopLeft(title.gameObject, new Vector2(60, -30), new Vector2(800, 60));

            // ---- 왼쪽: 클릭 영역 ----
            TextMeshProUGUI combo = CreateText(canvas.transform, "ComboText", "", 34, FontStyles.Bold);
            SetAnchorTopLeft(combo.gameObject, new Vector2(160, -150), new Vector2(400, 50));
            combo.color = new Color(1f, 0.85f, 0.2f);

            Button clickButton = CreateButton(
                canvas.transform,
                "ClickButton",
                "클릭!",
                new Vector2(120, -220),
                new Vector2(420, 320),
                48
            );

            Button feverButton = CreateButton(
                canvas.transform,
                "FeverButton",
                "피버!",
                new Vector2(120, -570),
                new Vector2(200, 60),
                24
            );
            TextMeshProUGUI feverState = CreateText(
                canvas.transform,
                "FeverStateText",
                "피버 (15초 쿨다운)",
                20,
                FontStyles.Normal
            );
            SetAnchorTopLeft(feverState.gameObject, new Vector2(340, -585), new Vector2(380, 40));
            feverState.color = Color.yellow;

            // ---- 오른쪽: 상태·업그레이드 영역 ----
            GameObject panel = CreatePanel(
                canvas.transform,
                "StatusPanel",
                new Vector2(720, -130),
                new Vector2(640, 560)
            );

            TextMeshProUGUI gold = CreateText(panel.transform, "GoldText", "골드 0", 36, FontStyles.Bold);
            SetAnchorTopLeft(gold.gameObject, new Vector2(30, -25), new Vector2(580, 50));

            TextMeshProUGUI stats = CreateText(
                panel.transform,
                "StatsText",
                "클릭당 +1 · 초당 +0",
                22,
                FontStyles.Normal
            );
            SetAnchorTopLeft(stats.gameObject, new Vector2(30, -85), new Vector2(580, 36));
            stats.color = new Color(0.8f, 0.9f, 1f);

            Button clickUpgrade = CreateButton(
                panel.transform,
                "ClickUpgradeButton",
                "클릭 강화\n비용 10",
                new Vector2(30, -150),
                new Vector2(280, 90),
                22
            );
            Button autoUpgrade = CreateButton(
                panel.transform,
                "AutoUpgradeButton",
                "도우미 고용\n비용 25",
                new Vector2(330, -150),
                new Vector2(280, 90),
                22
            );

            TextMeshProUGUI eventLog = CreateText(
                panel.transform,
                "EventLogText",
                "버튼을 눌러 골드를 모아 보세요.",
                19,
                FontStyles.Normal
            );
            SetAnchorTopLeft(eventLog.gameObject, new Vector2(30, -270), new Vector2(580, 260));
            eventLog.alignment = TextAlignmentOptions.TopLeft;
            eventLog.textWrappingMode = TextWrappingModes.Normal;
            eventLog.color = Color.yellow;

            // ---- Presenter 배치·연결 ----
            var presenter = new GameObject("ClickerPresenter").AddComponent<ClickerPresenter>();
            var so = new SerializedObject(presenter);
            so.FindProperty("m_ClickButton").objectReferenceValue = clickButton;
            so.FindProperty("m_FeverButton").objectReferenceValue = feverButton;
            so.FindProperty("m_ClickUpgradeButton").objectReferenceValue = clickUpgrade;
            so.FindProperty("m_AutoUpgradeButton").objectReferenceValue = autoUpgrade;
            so.FindProperty("m_GoldText").objectReferenceValue = gold;
            so.FindProperty("m_StatsText").objectReferenceValue = stats;
            so.FindProperty("m_ComboText").objectReferenceValue = combo;
            so.FindProperty("m_FeverStateText").objectReferenceValue = feverState;
            so.FindProperty("m_ClickUpgradeLabel").objectReferenceValue =
                clickUpgrade.GetComponentInChildren<TextMeshProUGUI>();
            so.FindProperty("m_AutoUpgradeLabel").objectReferenceValue =
                autoUpgrade.GetComponentInChildren<TextMeshProUGUI>();
            so.FindProperty("m_EventLogText").objectReferenceValue = eventLog;
            so.FindProperty("m_ClickButtonImage").objectReferenceValue = clickButton.GetComponent<Image>();
            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log("[ClickerGameSceneCreator] 씬 생성 완료: " + ScenePath);
        }

        #region UI helpers
        private static Canvas CreateCanvasAndEventSystem()
        {
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<InputSystemUIInputModule>(); // Input System 프로젝트이므로 레거시 StandaloneInputModule 금지
            }

            return canvas;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.18f, 0.9f);
            SetAnchorTopLeft(go, pos, size);
            return go;
        }

        private static TextMeshProUGUI CreateText(
            Transform parent,
            string name,
            string text,
            float fontSize,
            FontStyles style
        )
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            ApplyKoreanFont(tmp);
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
            return tmp;
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            string text,
            Vector2 pos,
            Vector2 size,
            float fontSize
        )
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = Color.white;
            var button = go.AddComponent<Button>();
            SetAnchorTopLeft(go, pos, size);

            var textGO = new GameObject("Text (TMP)");
            textGO.transform.SetParent(go.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            ApplyKoreanFont(tmp);
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            var rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            return button;
        }

        private static void ApplyKoreanFont(TextMeshProUGUI tmp)
        {
            if (s_KoreanFont == null)
                s_KoreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KoreanFontPath);
            if (s_KoreanFont != null)
                tmp.font = s_KoreanFont;
        }

        private static void SetAnchorTopLeft(GameObject go, Vector2 pos, Vector2 size)
        {
            var rect = go.GetComponent<RectTransform>();
            if (rect == null)
                rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
        }
        #endregion
    }
}
