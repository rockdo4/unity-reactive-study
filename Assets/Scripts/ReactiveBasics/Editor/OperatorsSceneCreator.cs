using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReactiveStudy.Basics.Editor
{
    /// <summary>
    /// 03차시(연산자) 데모 씬을 코드로 자동 생성한다.
    /// 메뉴: Tools > Reactive Study > 03. 연산자 씬 생성
    /// 교안 03 · 컴포넌트 <see cref="OperatorsDemo"/>.
    /// </summary>
    public static class OperatorsSceneCreator
    {
        public const string ScenePath = "Assets/Scenes/03_Operators.unity";

        [MenuItem("Tools/Reactive Study/03. 연산자 씬 생성")]
        public static void CreateSceneInteractive()
        {
            if (
                !EditorUtility.DisplayDialog(
                    "03. 연산자 씬 생성",
                    "새 씬을 만들어 연산자(Debounce·Chunk·ThrottleFirst 등) 데모를 구성합니다.\n"
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
                "03. 연산자 - Debounce · Chunk · ThrottleFirst · DistinctUntilChanged (R3)"
            );
            BuildOperatorsPanel(canvas, new Vector2(40, -90));

            EditorSceneManager.MarkSceneDirty(scene);
            if (save)
            {
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.Refresh();
            }
            Debug.Log("[OperatorsSceneCreator] 씬 생성 완료: " + ScenePath);
        }

        internal static void BuildOperatorsPanel(Canvas canvas, Vector2 panelPos)
        {
            GameObject panel = ReactiveSceneUi.CreatePanel(
                canvas.transform,
                "Panel2_Operators",
                panelPos,
                new Vector2(900, 310)
            );
            ReactiveSceneUi.CreateSectionTitle(
                panel.transform,
                "2. 연산자 - Debounce · Chunk · ThrottleFirst · DistinctUntilChanged (교안 03)"
            );

            TextMeshProUGUI doubleClick = ReactiveSceneUi.CreateBodyText(
                panel.transform,
                "DoubleClickText",
                "화면 아무 곳이나 빠르게 두 번 클릭 →",
                new Vector2(20, -45),
                new Vector2(860, 30)
            );
            TMP_InputField search = ReactiveSceneUi.CreateInputField(
                panel.transform,
                "SearchInput",
                "검색어 입력...",
                new Vector2(20, -85),
                new Vector2(380, 40)
            );
            TextMeshProUGUI searchResult = ReactiveSceneUi.CreateBodyText(
                panel.transform,
                "SearchResultText",
                "검색 대기 중... (입력 멈춘 뒤 0.5초)",
                new Vector2(420, -90),
                new Vector2(460, 34)
            );
            Button tap = ReactiveSceneUi.CreateButton(
                panel.transform,
                "TapButton",
                "탭",
                new Vector2(20, -145),
                new Vector2(170, 38)
            );
            TextMeshProUGUI tapText = ReactiveSceneUi.CreateBodyText(
                panel.transform,
                "TapText",
                "500ms 안에 3번 탭 →",
                new Vector2(210, -150),
                new Vector2(650, 30)
            );
            TextMeshProUGUI cooldownText = ReactiveSceneUi.CreateBodyText(
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
    }
}
