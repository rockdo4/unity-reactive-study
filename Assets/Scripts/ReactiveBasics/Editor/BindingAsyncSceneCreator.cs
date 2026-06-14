using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReactiveStudy.Basics.Editor
{
    /// <summary>
    /// 05차시(Unity 통합과 비동기) 데모 씬을 코드로 자동 생성한다.
    /// 메뉴: Tools > Reactive Study > 05. Unity 통합·비동기 씬 생성
    /// 교안 05 · 컴포넌트 <see cref="BindingDemo"/>(바인딩) + <see cref="AsyncDemo"/>(비동기).
    /// 한 씬에 두 패널을 세로로 배치한다.
    /// </summary>
    public static class BindingAsyncSceneCreator
    {
        public const string ScenePath = "Assets/Scenes/05_BindingAsync.unity";

        [MenuItem("Tools/Reactive Study/05. Unity 통합·비동기 씬 생성")]
        public static void CreateSceneInteractive()
        {
            if (
                !EditorUtility.DisplayDialog(
                    "05. Unity 통합·비동기 씬 생성",
                    "새 씬을 만들어 ReactiveProperty 바인딩과 SubscribeAwait(비동기) 데모를 구성합니다.\n"
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
            ReactiveSceneUi.CreateTitle(
                canvas,
                "05. Unity 통합과 비동기 - ReactiveProperty 바인딩 · SubscribeAwait (R3)"
            );
            BuildBindingPanel(canvas, new Vector2(40, -90));
            BuildAsyncPanel(canvas, new Vector2(40, -410));

            EditorSceneManager.MarkSceneDirty(scene);
            if (save)
            {
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.Refresh();
            }
            Debug.Log("[BindingAsyncSceneCreator] 씬 생성 완료: " + ScenePath);
        }

        internal static void BuildBindingPanel(Canvas canvas, Vector2 panelPos)
        {
            GameObject panel = ReactiveSceneUi.CreatePanel(
                canvas.transform,
                "Panel4_Binding",
                panelPos,
                new Vector2(900, 300)
            );
            ReactiveSceneUi.CreateSectionTitle(panel.transform, "4. ReactiveProperty 바인딩 - CombineLatest (교안 05)");

            Button damage = ReactiveSceneUi.CreateButton(
                panel.transform,
                "DamageButton",
                "피해 -10",
                new Vector2(20, -50),
                new Vector2(150, 38)
            );
            Button heal = ReactiveSceneUi.CreateButton(
                panel.transform,
                "HealButton",
                "회복 +10",
                new Vector2(190, -50),
                new Vector2(150, 38)
            );
            Button useMp = ReactiveSceneUi.CreateButton(
                panel.transform,
                "UseMpButton",
                "MP -5",
                new Vector2(360, -50),
                new Vector2(130, 38)
            );
            Button restoreMp = ReactiveSceneUi.CreateButton(
                panel.transform,
                "RestoreMpButton",
                "MP +5",
                new Vector2(510, -50),
                new Vector2(130, 38)
            );
            TextMeshProUGUI hp = ReactiveSceneUi.CreateBodyText(
                panel.transform,
                "HpText",
                "HP : 100",
                new Vector2(20, -100),
                new Vector2(200, 30)
            );
            TextMeshProUGUI mp = ReactiveSceneUi.CreateBodyText(
                panel.transform,
                "MpText",
                "MP : 50",
                new Vector2(240, -100),
                new Vector2(200, 30)
            );
            TextMeshProUGUI hpState = ReactiveSceneUi.CreateBodyText(
                panel.transform,
                "HpStateText",
                "상태: 안전",
                new Vector2(460, -100),
                new Vector2(250, 30)
            );
            Button skill = ReactiveSceneUi.CreateButton(
                panel.transform,
                "SkillButton",
                "스킬 사용 (MP 20)",
                new Vector2(20, -145),
                new Vector2(220, 40)
            );
            TextMeshProUGUI skillLog = ReactiveSceneUi.CreateBodyText(
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

        internal static void BuildAsyncPanel(Canvas canvas, Vector2 panelPos)
        {
            GameObject panel = ReactiveSceneUi.CreatePanel(
                canvas.transform,
                "Panel5_Async",
                panelPos,
                new Vector2(900, 310)
            );
            ReactiveSceneUi.CreateSectionTitle(panel.transform, "5. 비동기 통합 - SubscribeAwait · 취소 (교안 05)");

            Button sequential = ReactiveSceneUi.CreateButton(
                panel.transform,
                "SequentialButton",
                "순차 로드 (Sequential)",
                new Vector2(20, -50),
                new Vector2(250, 40)
            );
            Button drop = ReactiveSceneUi.CreateButton(
                panel.transform,
                "DropButton",
                "드롭 로드 (Drop)",
                new Vector2(290, -50),
                new Vector2(220, 40)
            );
            TextMeshProUGUI log = ReactiveSceneUi.CreateBodyText(
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
    }
}
