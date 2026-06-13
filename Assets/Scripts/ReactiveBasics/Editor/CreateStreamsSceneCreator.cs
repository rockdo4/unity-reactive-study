using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReactiveStudy.Basics.Editor
{
    /// <summary>
    /// 02차시(Observable 만들기) 데모 씬을 코드로 자동 생성한다.
    /// 메뉴: Tools > Reactive Study > 02. Observable 만들기 씬 생성
    /// 교안 02 · 컴포넌트 <see cref="CreateStreamsDemo"/>.
    /// </summary>
    public static class CreateStreamsSceneCreator
    {
        public const string ScenePath = "Assets/Scenes/02_CreateStreams.unity";

        [MenuItem("Tools/Reactive Study/02. Observable 만들기 씬 생성")]
        public static void CreateSceneInteractive()
        {
            if (
                !EditorUtility.DisplayDialog(
                    "02. Observable 만들기 씬 생성",
                    "새 씬을 만들어 R3 생성(팩토리·Subject·FromEvent) 데모를 구성합니다.\n"
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
            EditorUtility.DisplayDialog("완료", "데모 씬이 생성되었습니다:\n" + ScenePath, "확인");
        }

        /// <summary>씬을 생성한다. save=false 면 메모리에만 만들고 저장하지 않는다(StudentPackage 스왑용).</summary>
        public static void BuildScene(bool save = true)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Canvas canvas = ReactiveSceneUi.CreateCanvasAndEventSystem();
            ReactiveSceneUi.CreateTitle(canvas, "02. Observable 만들기 — 팩토리 · Subject · FromEvent (R3)");
            BuildCreatePanel(canvas);

            EditorSceneManager.MarkSceneDirty(scene);
            if (save)
            {
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.Refresh();
            }
            Debug.Log("[CreateStreamsSceneCreator] 씬 생성 완료: " + ScenePath);
        }

        private static void BuildCreatePanel(Canvas canvas)
        {
            GameObject panel = ReactiveSceneUi.CreatePanel(
                canvas.transform,
                "Panel1_Create",
                new Vector2(40, -90),
                new Vector2(900, 300)
            );
            ReactiveSceneUi.CreateSectionTitle(
                panel.transform,
                "1. Observable 만들기 — 팩토리 · Subject · FromEvent (교안 02)"
            );

            Button range = ReactiveSceneUi.CreateButton(
                panel.transform,
                "RangeButton",
                "Range(1, 4)",
                new Vector2(20, -50),
                new Vector2(170, 38)
            );
            Button timer = ReactiveSceneUi.CreateButton(
                panel.transform,
                "TimerButton",
                "Timer 2초",
                new Vector2(210, -50),
                new Vector2(170, 38)
            );
            Button interval = ReactiveSceneUi.CreateButton(
                panel.transform,
                "IntervalButton",
                "Interval 시작",
                new Vector2(400, -50),
                new Vector2(190, 38)
            );
            Button subject = ReactiveSceneUi.CreateButton(
                panel.transform,
                "SubjectButton",
                "Subject.OnNext",
                new Vector2(20, -95),
                new Vector2(190, 38)
            );
            Button legacyEvent = ReactiveSceneUi.CreateButton(
                panel.transform,
                "EventButton",
                "event 발생 (FromEvent)",
                new Vector2(230, -95),
                new Vector2(230, 38)
            );
            TextMeshProUGUI log = ReactiveSceneUi.CreateBodyText(
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
    }
}
