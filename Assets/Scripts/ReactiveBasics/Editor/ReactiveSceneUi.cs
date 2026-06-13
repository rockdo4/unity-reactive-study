using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ReactiveStudy.Basics.Editor
{
    /// <summary>
    /// 데모 씬 생성기들이 공유하는 UI 빌딩 헬퍼.
    /// 챕터별 SceneCreator(1부)와 ClickerGameSceneCreator(2부)가 모두 사용하므로
    /// public 으로 노출한다(ClickerGame.Editor 어셈블리가 ReactiveBasics.Editor 를 참조).
    /// </summary>
    public static class ReactiveSceneUi
    {
        public const string KoreanFontPath = "Assets/Fonts/Pretendard-Regular SDF.asset";
        private static TMP_FontAsset s_KoreanFont;

        /// <summary>ScreenSpaceOverlay Canvas + (없으면) Input System EventSystem 을 만든다.</summary>
        public static Canvas CreateCanvasAndEventSystem()
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

        /// <summary>씬 좌상단의 큰 제목.</summary>
        public static TextMeshProUGUI CreateTitle(Canvas canvas, string text)
        {
            TextMeshProUGUI title = CreateText(canvas.transform, "Title", text, 36, FontStyles.Bold);
            SetAnchorTopLeft(title.gameObject, new Vector2(40, -20), new Vector2(1400, 50));
            title.alignment = TextAlignmentOptions.Left;
            return title;
        }

        /// <summary>패널 안쪽 상단의 섹션 제목.</summary>
        public static TextMeshProUGUI CreateSectionTitle(Transform parent, string text)
        {
            TextMeshProUGUI t = CreateText(parent, "SectionTitle", text, 21, FontStyles.Bold);
            SetAnchorTopLeft(t.gameObject, new Vector2(15, -10), new Vector2(870, 32));
            return t;
        }

        /// <summary>반투명 배경 패널.</summary>
        public static GameObject CreatePanel(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.18f, 0.9f);
            SetAnchorTopLeft(go, pos, size);
            return go;
        }

        /// <summary>로그·안내용 노란 본문 텍스트(좌상단 정렬·줄바꿈).</summary>
        public static TextMeshProUGUI CreateBodyText(
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

        /// <summary>한글 폰트가 적용된 TMP 텍스트.</summary>
        public static TextMeshProUGUI CreateText(
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

        /// <summary>흰 배경 버튼 + 검은 글씨. fontSize 기본값 17(1부 패널 기준), 큰 버튼은 명시 지정.</summary>
        public static Button CreateButton(
            Transform parent,
            string name,
            string text,
            Vector2 pos,
            Vector2 size,
            float fontSize = 17
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

        /// <summary>TMP 입력 필드(플레이스홀더·본문에 한글 폰트 적용).</summary>
        public static TMP_InputField CreateInputField(
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

        /// <summary>Pretendard 한글 폰트 에셋을 로드해 주입(캐시).</summary>
        public static void ApplyKoreanFont(TextMeshProUGUI tmp)
        {
            if (s_KoreanFont == null)
                s_KoreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KoreanFontPath);
            if (s_KoreanFont != null)
                tmp.font = s_KoreanFont;
        }

        /// <summary>좌상단 기준 앵커·피벗으로 RectTransform 배치.</summary>
        public static void SetAnchorTopLeft(GameObject go, Vector2 pos, Vector2 size)
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
    }
}
