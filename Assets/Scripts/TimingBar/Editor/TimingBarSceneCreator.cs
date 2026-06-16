using ReactiveStudy.Basics.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReactiveStudy.TimingBar.Editor
{
    /// <summary>
    /// "타이밍 바" capstone 과제의 강사용 정본(모범답안) 씬을 코드로 생성한다.
    /// 메뉴: Tools > Reactive Study > 03. 타이밍 바(과제 정본) 씬 생성
    ///
    /// 학생은 빈 씬에서 직접 만든다(스켈레톤 배포 없음). 이 씬은 강사 시연·윈도우 빌드의 소스이며,
    /// git 에 커밋하지 않는다(Assets/Scenes/*.unity 는 .gitignore). UI 헬퍼는 <see cref="ReactiveSceneUi"/> 공유.
    /// </summary>
    public static class TimingBarSceneCreator
    {
        public const string ScenePath = "Assets/Scenes/TimingBar.unity";

        private const float TrackWidth = 1200f;
        private const float TrackHeight = 60f;
        private const float MarkerWidth = 24f;
        private const float MarkerHeight = 96f;

        [MenuItem("Tools/Reactive Study/03. 타이밍 바(과제 정본) 씬 생성")]
        public static void CreateSceneInteractive()
        {
            if (
                !EditorUtility.DisplayDialog(
                    "03. 타이밍 바(과제 정본) 씬 생성",
                    "새 씬을 만들어 타이밍 바(모범답안)를 구성합니다.\n"
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

            BuildScene();
            EditorUtility.DisplayDialog("완료", "타이밍 바 씬이 생성되었습니다:\n" + ScenePath, "확인");
        }

        /// <summary>씬을 생성한다. save=false 면 메모리에만 만들고 저장하지 않는다.</summary>
        public static void BuildScene(bool save = true)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Canvas canvas = ReactiveSceneUi.CreateCanvasAndEventSystem();

            TextMeshProUGUI title = ReactiveSceneUi.CreateText(
                canvas.transform,
                "Title",
                "타이밍 바",
                42,
                FontStyles.Bold
            );
            ReactiveSceneUi.SetAnchorTopLeft(title.gameObject, new Vector2(60, -40), new Vector2(800, 60));

            // 상태(카운트다운/안내/결과)·판정 — 트랙 위 중앙
            TextMeshProUGUI stateText = CreateCenterText(
                canvas.transform,
                "StateText",
                "START 를 누르세요",
                34,
                new Vector2(0, 250),
                new Vector2(1400, 60)
            );
            TextMeshProUGUI judgementText = CreateCenterText(
                canvas.transform,
                "JudgementText",
                string.Empty,
                56,
                new Vector2(0, 160),
                new Vector2(1400, 90)
            );

            // 점수 / 콤보 / 진행도
            TextMeshProUGUI scoreText = ReactiveSceneUi.CreateText(
                canvas.transform,
                "ScoreText",
                "점수 0",
                32,
                FontStyles.Bold
            );
            ReactiveSceneUi.SetAnchorTopLeft(scoreText.gameObject, new Vector2(60, -150), new Vector2(500, 48));

            TextMeshProUGUI comboText = ReactiveSceneUi.CreateText(
                canvas.transform,
                "ComboText",
                string.Empty,
                30,
                FontStyles.Bold
            );
            ReactiveSceneUi.SetAnchorTopLeft(comboText.gameObject, new Vector2(60, -210), new Vector2(500, 48));
            comboText.color = new Color(1f, 0.85f, 0.2f);

            TextMeshProUGUI attemptText = ReactiveSceneUi.CreateText(
                canvas.transform,
                "AttemptText",
                "0 / 12",
                30,
                FontStyles.Bold
            );
            ReactiveSceneUi.SetAnchorTopLeft(attemptText.gameObject, new Vector2(1360, -150), new Vector2(500, 48));

            // 트랙(중앙) + 존(고정 시각 기준선) + 마커. 존 폭은 Model 상수로 계산해 판정과 100% 일치시킨다.
            Image track = CreateCenteredImage(
                canvas.transform,
                "Track",
                new Color(0.2f, 0.2f, 0.25f, 1f),
                new Vector2(0, 40),
                new Vector2(TrackWidth, TrackHeight)
            );
            float usableWidth = TrackWidth - MarkerWidth;
            float goodWidth = 2f * TimingBarModel.GoodHalfWidth * usableWidth;
            float perfectWidth = 2f * TimingBarModel.PerfectHalfWidth * usableWidth;
            CreateCenteredImage(
                track.transform,
                "GoodZone",
                new Color(0.2f, 0.5f, 0.9f, 0.5f),
                Vector2.zero,
                new Vector2(goodWidth, TrackHeight)
            );
            CreateCenteredImage(
                track.transform,
                "PerfectZone",
                new Color(0.95f, 0.75f, 0.15f, 0.6f),
                Vector2.zero,
                new Vector2(perfectWidth, TrackHeight)
            );
            Image marker = CreateCenteredImage(
                track.transform,
                "Marker",
                Color.white,
                Vector2.zero,
                new Vector2(MarkerWidth, MarkerHeight)
            );

            Button hitButton = ReactiveSceneUi.CreateButton(
                canvas.transform,
                "HitButton",
                "HIT  (Space)",
                new Vector2(760, -640),
                new Vector2(400, 120),
                40
            );
            Button startButton = ReactiveSceneUi.CreateButton(
                canvas.transform,
                "StartButton",
                "START",
                new Vector2(760, -800),
                new Vector2(400, 90),
                30
            );

            // Presenter 배치·연결
            var presenter = new GameObject("TimingBarPresenter").AddComponent<TimingBarPresenter>();
            var so = new SerializedObject(presenter);
            so.FindProperty("m_TrackImage").objectReferenceValue = track;
            so.FindProperty("m_Marker").objectReferenceValue = marker.rectTransform;
            so.FindProperty("m_HitButton").objectReferenceValue = hitButton;
            so.FindProperty("m_StartButton").objectReferenceValue = startButton;
            so.FindProperty("m_ScoreText").objectReferenceValue = scoreText;
            so.FindProperty("m_ComboText").objectReferenceValue = comboText;
            so.FindProperty("m_AttemptText").objectReferenceValue = attemptText;
            so.FindProperty("m_JudgementText").objectReferenceValue = judgementText;
            so.FindProperty("m_StateText").objectReferenceValue = stateText;
            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            if (save)
            {
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.Refresh();
            }
            Debug.Log("[TimingBarSceneCreator] 씬 생성 완료: " + ScenePath);
        }

        /// <summary>가운데 정렬·중앙 앵커의 텍스트(카운트다운·판정처럼 화면 중앙에 띄울 때).</summary>
        private static TextMeshProUGUI CreateCenterText(
            Transform parent,
            string name,
            string text,
            float fontSize,
            Vector2 anchoredPos,
            Vector2 size
        )
        {
            TextMeshProUGUI t = ReactiveSceneUi.CreateText(parent, name, text, fontSize, FontStyles.Bold);
            RectTransform rect = t.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
            t.alignment = TextAlignmentOptions.Center;
            return t;
        }

        /// <summary>중앙 앵커·중앙 피벗의 단색 Image(트랙·존·마커처럼 좌우 대칭으로 움직이거나 겹칠 때).</summary>
        private static Image CreateCenteredImage(
            Transform parent,
            string name,
            Color color,
            Vector2 anchoredPos,
            Vector2 size
        )
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = color;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
            return image;
        }
    }
}
