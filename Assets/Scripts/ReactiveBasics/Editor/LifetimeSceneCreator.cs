using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReactiveStudy.Basics.Editor
{
    /// <summary>
    /// 04차시(구독 수명 관리) 데모 씬을 코드로 자동 생성한다.
    /// 메뉴: Tools > Reactive Study > 04. 구독 수명 씬 생성
    /// 교안 04 · 컴포넌트 <see cref="LifetimeDemo"/> + <see cref="LifetimeProbe"/>.
    /// </summary>
    public static class LifetimeSceneCreator
    {
        public const string ScenePath = "Assets/Scenes/04_Lifetime.unity";

        [MenuItem("Tools/Reactive Study/04. 구독 수명 씬 생성")]
        public static void CreateSceneInteractive()
        {
            if (
                !EditorUtility.DisplayDialog(
                    "04. 구독 수명 씬 생성",
                    "새 씬을 만들어 구독 수명(CompositeDisposable·AddTo·ObservableTracker) 데모를 구성합니다.\n"
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
            ReactiveSceneUi.CreateTitle(canvas, "04. 구독 수명 - CompositeDisposable · ObservableTracker (R3)");
            BuildLifetimePanel(canvas, new Vector2(40, -90));

            EditorSceneManager.MarkSceneDirty(scene);
            if (save)
            {
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.Refresh();
            }
            Debug.Log("[LifetimeSceneCreator] 씬 생성 완료: " + ScenePath);
        }

        internal static void BuildLifetimePanel(Canvas canvas, Vector2 panelPos)
        {
            GameObject panel = ReactiveSceneUi.CreatePanel(
                canvas.transform,
                "Panel3_Lifetime",
                panelPos,
                new Vector2(900, 320)
            );
            ReactiveSceneUi.CreateSectionTitle(
                panel.transform,
                "3. 구독 수명 - CompositeDisposable · ObservableTracker (교안 04)"
            );

            Button add = ReactiveSceneUi.CreateButton(
                panel.transform,
                "AddButton",
                "구독 추가",
                new Vector2(20, -50),
                new Vector2(160, 38)
            );
            Button disposeAll = ReactiveSceneUi.CreateButton(
                panel.transform,
                "DisposeAllButton",
                "모두 해제",
                new Vector2(200, -50),
                new Vector2(160, 38)
            );
            Button leak = ReactiveSceneUi.CreateButton(
                panel.transform,
                "LeakButton",
                "누수 만들기",
                new Vector2(380, -50),
                new Vector2(160, 38)
            );
            Button tracker = ReactiveSceneUi.CreateButton(
                panel.transform,
                "TrackerButton",
                "활성 구독 조회",
                new Vector2(560, -50),
                new Vector2(180, 38)
            );
            Button destroyProbe = ReactiveSceneUi.CreateButton(
                panel.transform,
                "DestroyProbeButton",
                "프로브 파괴",
                new Vector2(20, -95),
                new Vector2(160, 38)
            );
            TextMeshProUGUI status = ReactiveSceneUi.CreateBodyText(
                panel.transform,
                "StatusText",
                "대기 중",
                new Vector2(20, -140),
                new Vector2(860, 30)
            );
            TextMeshProUGUI trackerText = ReactiveSceneUi.CreateBodyText(
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
    }
}
