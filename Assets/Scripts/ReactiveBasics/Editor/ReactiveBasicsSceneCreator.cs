using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReactiveStudy.Basics.Editor
{
    /// <summary>
    /// 1부(기본 개념·사용법) 데모 씬을 코드로 자동 생성한다.
    /// 메뉴: Tools > Reactive Study > 1부 기초 데모 씬 생성
    ///
    /// 다섯 패널(생성/연산자/구독 수명/바인딩/비동기)이 교안 02~05장과 1:1로 대응한다.
    /// </summary>
    public static class ReactiveBasicsSceneCreator
    {
        private const string ScenePath = "Assets/Scenes/ReactiveBasics.unity";
        private const string KoreanFontPath = "Assets/Fonts/Pretendard-Regular SDF.asset";
        private static TMP_FontAsset s_KoreanFont;

        [MenuItem("Tools/Reactive Study/1부 기초 데모 씬 생성")]
        public static void CreateSceneInteractive()
        {
            if (
                !EditorUtility.DisplayDialog(
                    "1부 기초 데모 씬 생성",
                    "새 씬을 만들어 R3 기초 데모(생성/연산자/수명/바인딩/비동기)를 구성합니다.\n"
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
            EditorUtility.DisplayDialog("완료", "데모 씬이 생성되었습니다:\n" + ScenePath, "확인");
        }

        /// <summary>대화상자 없이 씬을 생성한다(자동화·배치 실행용).</summary>
        public static void CreateScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Canvas canvas = CreateCanvasAndEventSystem();
            CreateTitle(canvas, "리액티브 프로그래밍 기초 데모 (R3)");
            BuildCreatePanel(canvas);
            BuildOperatorsPanel(canvas);
            BuildLifetimePanel(canvas);
            BuildBindingPanel(canvas);
            BuildAsyncPanel(canvas);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log("[ReactiveBasicsSceneCreator] 씬 생성 완료: " + ScenePath);
        }

        #region Panels
        private static void BuildCreatePanel(Canvas canvas)
        {
            GameObject panel = CreatePanel(
                canvas.transform,
                "Panel1_Create",
                new Vector2(40, -90),
                new Vector2(900, 300)
            );
            CreateSectionTitle(panel.transform, "1. Observable 만들기 — 팩토리 · Subject · FromEvent (교안 02)");

            Button range = CreateButton(
                panel.transform,
                "RangeButton",
                "Range(1, 4)",
                new Vector2(20, -50),
                new Vector2(170, 38)
            );
            Button timer = CreateButton(
                panel.transform,
                "TimerButton",
                "Timer 2초",
                new Vector2(210, -50),
                new Vector2(170, 38)
            );
            Button interval = CreateButton(
                panel.transform,
                "IntervalButton",
                "Interval 시작",
                new Vector2(400, -50),
                new Vector2(190, 38)
            );
            Button subject = CreateButton(
                panel.transform,
                "SubjectButton",
                "Subject.OnNext",
                new Vector2(20, -95),
                new Vector2(190, 38)
            );
            Button legacyEvent = CreateButton(
                panel.transform,
                "EventButton",
                "event 발생 (FromEvent)",
                new Vector2(230, -95),
                new Vector2(230, 38)
            );
            TextMeshProUGUI log = CreateBodyText(
                panel.transform,
                "LogText",
                "버튼을 눌러 스트림을 만들어 보세요.",
                new Vector2(20, -145),
                new Vector2(860, 145)
            );

            var demo = new GameObject("CreateStreamsDemo").AddComponent<CreateStreamsDemo>();
            var so = new SerializedObject(demo);
            so.FindProperty("m_RangeButton").objectReferenceValue = range;
            so.FindProperty("m_TimerButton").objectReferenceValue = timer;
            so.FindProperty("m_IntervalToggleButton").objectReferenceValue = interval;
            so.FindProperty("m_IntervalToggleLabel").objectReferenceValue =
                interval.GetComponentInChildren<TextMeshProUGUI>();
            so.FindProperty("m_SubjectButton").objectReferenceValue = subject;
            so.FindProperty("m_EventButton").objectReferenceValue = legacyEvent;
            so.FindProperty("m_LogText").objectReferenceValue = log;
            so.ApplyModifiedProperties();
        }

        private static void BuildOperatorsPanel(Canvas canvas)
        {
            GameObject panel = CreatePanel(
                canvas.transform,
                "Panel2_Operators",
                new Vector2(40, -410),
                new Vector2(900, 310)
            );
            CreateSectionTitle(
                panel.transform,
                "2. 연산자 — Debounce · Chunk · ThrottleFirst · DistinctUntilChanged (교안 03)"
            );

            TextMeshProUGUI doubleClick = CreateBodyText(
                panel.transform,
                "DoubleClickText",
                "화면 아무 곳이나 빠르게 두 번 클릭 →",
                new Vector2(20, -45),
                new Vector2(860, 30)
            );
            TMP_InputField search = CreateInputField(
                panel.transform,
                "SearchInput",
                "검색어 입력...",
                new Vector2(20, -85),
                new Vector2(380, 40)
            );
            TextMeshProUGUI searchResult = CreateBodyText(
                panel.transform,
                "SearchResultText",
                "검색 대기 중... (입력 멈춘 뒤 0.5초)",
                new Vector2(420, -90),
                new Vector2(460, 34)
            );
            Button tap = CreateButton(panel.transform, "TapButton", "탭", new Vector2(20, -145), new Vector2(170, 38));
            TextMeshProUGUI tapText = CreateBodyText(
                panel.transform,
                "TapText",
                "500ms 안에 3번 탭 →",
                new Vector2(210, -150),
                new Vector2(650, 30)
            );
            TextMeshProUGUI cooldownText = CreateBodyText(
                panel.transform,
                "CooldownText",
                "탭은 2초 쿨다운으로도 발동 →",
                new Vector2(210, -185),
                new Vector2(650, 30)
            );

            var demo = new GameObject("OperatorsDemo").AddComponent<OperatorsDemo>();
            var so = new SerializedObject(demo);
            so.FindProperty("m_DoubleClickText").objectReferenceValue = doubleClick;
            so.FindProperty("m_SearchInput").objectReferenceValue = search;
            so.FindProperty("m_SearchResultText").objectReferenceValue = searchResult;
            so.FindProperty("m_TapButton").objectReferenceValue = tap;
            so.FindProperty("m_TapText").objectReferenceValue = tapText;
            so.FindProperty("m_CooldownText").objectReferenceValue = cooldownText;
            so.ApplyModifiedProperties();
        }

        private static void BuildLifetimePanel(Canvas canvas)
        {
            GameObject panel = CreatePanel(
                canvas.transform,
                "Panel3_Lifetime",
                new Vector2(40, -740),
                new Vector2(900, 320)
            );
            CreateSectionTitle(panel.transform, "3. 구독 수명 — CompositeDisposable · ObservableTracker (교안 04)");

            Button add = CreateButton(
                panel.transform,
                "AddButton",
                "구독 추가",
                new Vector2(20, -50),
                new Vector2(160, 38)
            );
            Button disposeAll = CreateButton(
                panel.transform,
                "DisposeAllButton",
                "모두 해제",
                new Vector2(200, -50),
                new Vector2(160, 38)
            );
            Button leak = CreateButton(
                panel.transform,
                "LeakButton",
                "누수 만들기",
                new Vector2(380, -50),
                new Vector2(160, 38)
            );
            Button tracker = CreateButton(
                panel.transform,
                "TrackerButton",
                "활성 구독 조회",
                new Vector2(560, -50),
                new Vector2(180, 38)
            );
            Button destroyProbe = CreateButton(
                panel.transform,
                "DestroyProbeButton",
                "프로브 파괴",
                new Vector2(20, -95),
                new Vector2(160, 38)
            );
            TextMeshProUGUI status = CreateBodyText(
                panel.transform,
                "StatusText",
                "대기 중",
                new Vector2(20, -140),
                new Vector2(860, 30)
            );
            TextMeshProUGUI trackerText = CreateBodyText(
                panel.transform,
                "TrackerText",
                "[활성 구독 조회]를 눌러 현재 구독을 확인하세요.",
                new Vector2(20, -175),
                new Vector2(860, 135)
            );
            trackerText.fontSize = 15;

            var probe = new GameObject("LifetimeProbe");
            probe.AddComponent<LifetimeProbe>();

            var demo = new GameObject("LifetimeDemo").AddComponent<LifetimeDemo>();
            var so = new SerializedObject(demo);
            so.FindProperty("m_AddSubscriptionButton").objectReferenceValue = add;
            so.FindProperty("m_DisposeAllButton").objectReferenceValue = disposeAll;
            so.FindProperty("m_LeakButton").objectReferenceValue = leak;
            so.FindProperty("m_TrackerButton").objectReferenceValue = tracker;
            so.FindProperty("m_DestroyProbeButton").objectReferenceValue = destroyProbe;
            so.FindProperty("m_ProbeObject").objectReferenceValue = probe;
            so.FindProperty("m_StatusText").objectReferenceValue = status;
            so.FindProperty("m_TrackerText").objectReferenceValue = trackerText;
            so.ApplyModifiedProperties();
        }

        private static void BuildBindingPanel(Canvas canvas)
        {
            GameObject panel = CreatePanel(
                canvas.transform,
                "Panel4_Binding",
                new Vector2(980, -90),
                new Vector2(900, 300)
            );
            CreateSectionTitle(panel.transform, "4. ReactiveProperty 바인딩 — CombineLatest (교안 05)");

            Button damage = CreateButton(
                panel.transform,
                "DamageButton",
                "피해 -10",
                new Vector2(20, -50),
                new Vector2(150, 38)
            );
            Button heal = CreateButton(
                panel.transform,
                "HealButton",
                "회복 +10",
                new Vector2(190, -50),
                new Vector2(150, 38)
            );
            Button useMp = CreateButton(
                panel.transform,
                "UseMpButton",
                "MP -5",
                new Vector2(360, -50),
                new Vector2(130, 38)
            );
            Button restoreMp = CreateButton(
                panel.transform,
                "RestoreMpButton",
                "MP +5",
                new Vector2(510, -50),
                new Vector2(130, 38)
            );
            TextMeshProUGUI hp = CreateBodyText(
                panel.transform,
                "HpText",
                "HP : 100",
                new Vector2(20, -100),
                new Vector2(200, 30)
            );
            TextMeshProUGUI mp = CreateBodyText(
                panel.transform,
                "MpText",
                "MP : 50",
                new Vector2(240, -100),
                new Vector2(200, 30)
            );
            TextMeshProUGUI hpState = CreateBodyText(
                panel.transform,
                "HpStateText",
                "상태: 안전",
                new Vector2(460, -100),
                new Vector2(250, 30)
            );
            Button skill = CreateButton(
                panel.transform,
                "SkillButton",
                "스킬 사용 (MP 20)",
                new Vector2(20, -145),
                new Vector2(220, 40)
            );
            TextMeshProUGUI skillLog = CreateBodyText(
                panel.transform,
                "SkillLogText",
                "HP가 0이거나 MP가 부족하면 버튼이 꺼집니다.",
                new Vector2(260, -150),
                new Vector2(600, 60)
            );

            var demo = new GameObject("BindingDemo").AddComponent<BindingDemo>();
            var so = new SerializedObject(demo);
            so.FindProperty("m_DamageButton").objectReferenceValue = damage;
            so.FindProperty("m_HealButton").objectReferenceValue = heal;
            so.FindProperty("m_UseMpButton").objectReferenceValue = useMp;
            so.FindProperty("m_RestoreMpButton").objectReferenceValue = restoreMp;
            so.FindProperty("m_SkillButton").objectReferenceValue = skill;
            so.FindProperty("m_HpText").objectReferenceValue = hp;
            so.FindProperty("m_MpText").objectReferenceValue = mp;
            so.FindProperty("m_HpStateText").objectReferenceValue = hpState;
            so.FindProperty("m_SkillLogText").objectReferenceValue = skillLog;
            so.ApplyModifiedProperties();
        }

        private static void BuildAsyncPanel(Canvas canvas)
        {
            GameObject panel = CreatePanel(
                canvas.transform,
                "Panel5_Async",
                new Vector2(980, -410),
                new Vector2(900, 310)
            );
            CreateSectionTitle(panel.transform, "5. 비동기 통합 — SubscribeAwait · 취소 (교안 05)");

            Button sequential = CreateButton(
                panel.transform,
                "SequentialButton",
                "순차 로드 (Sequential)",
                new Vector2(20, -50),
                new Vector2(250, 40)
            );
            Button drop = CreateButton(
                panel.transform,
                "DropButton",
                "드롭 로드 (Drop)",
                new Vector2(290, -50),
                new Vector2(220, 40)
            );
            TextMeshProUGUI log = CreateBodyText(
                panel.transform,
                "AsyncLogText",
                "버튼을 빠르게 연타해 두 방식의 차이를 비교해 보세요.\n(로드 1건당 1.5초)",
                new Vector2(20, -105),
                new Vector2(860, 185)
            );

            var demo = new GameObject("AsyncDemo").AddComponent<AsyncDemo>();
            var so = new SerializedObject(demo);
            so.FindProperty("m_SequentialButton").objectReferenceValue = sequential;
            so.FindProperty("m_DropButton").objectReferenceValue = drop;
            so.FindProperty("m_LogText").objectReferenceValue = log;
            so.ApplyModifiedProperties();
        }
        #endregion

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

        private static void CreateTitle(Canvas canvas, string text)
        {
            TextMeshProUGUI title = CreateText(canvas.transform, "Title", text, 36, FontStyles.Bold);
            SetAnchorTopLeft(title.gameObject, new Vector2(40, -20), new Vector2(1200, 50));
            title.alignment = TextAlignmentOptions.Left;
        }

        private static void CreateSectionTitle(Transform parent, string text)
        {
            TextMeshProUGUI t = CreateText(parent, "SectionTitle", text, 21, FontStyles.Bold);
            SetAnchorTopLeft(t.gameObject, new Vector2(15, -10), new Vector2(870, 32));
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

        private static TextMeshProUGUI CreateBodyText(
            Transform parent,
            string name,
            string text,
            Vector2 pos,
            Vector2 size
        )
        {
            TextMeshProUGUI t = CreateText(parent, name, text, 18, FontStyles.Normal);
            SetAnchorTopLeft(t.gameObject, pos, size);
            t.alignment = TextAlignmentOptions.TopLeft;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.color = Color.yellow;
            return t;
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

        private static Button CreateButton(Transform parent, string name, string text, Vector2 pos, Vector2 size)
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
            tmp.fontSize = 17;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            var rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            return button;
        }

        private static TMP_InputField CreateInputField(
            Transform parent,
            string name,
            string placeholder,
            Vector2 pos,
            Vector2 size
        )
        {
            GameObject go = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources());
            go.name = name;
            go.transform.SetParent(parent, false);
            SetAnchorTopLeft(go, pos, size);

            var field = go.GetComponent<TMP_InputField>();
            if (field.textComponent is TextMeshProUGUI textComponent)
            {
                ApplyKoreanFont(textComponent);
                textComponent.fontSize = 18;
                textComponent.color = Color.black;
            }
            if (field.placeholder is TextMeshProUGUI placeholderText)
            {
                ApplyKoreanFont(placeholderText);
                placeholderText.fontSize = 18;
                placeholderText.text = placeholder;
            }
            return field;
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
