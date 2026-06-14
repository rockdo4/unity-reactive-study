using ReactiveStudy.Basics.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ReactiveStudy.Clicker.Editor
{
    /// <summary>
    /// 06차시(2부 따라하기) 리액티브 클리커 씬을 코드로 자동 생성한다.
    /// 메뉴: Tools > Reactive Study > 06. 클리커 씬 생성
    ///
    /// 수업에서는 교안 06장의 단계를 따라 학생이 직접 만드는 것이 기본이고,
    /// 이 씬은 완성본 확인·시연용 보조 자료다. UI 헬퍼는 <see cref="ReactiveSceneUi"/> 공유.
    /// </summary>
    public static class ClickerGameSceneCreator
    {
        public const string ScenePath = "Assets/Scenes/ReactiveClicker.unity";

        [MenuItem("Tools/Reactive Study/02. 클리커(2부) 씬 생성")]
        public static void CreateSceneInteractive()
        {
            if (
                !EditorUtility.DisplayDialog(
                    "02. 클리커(2부) 씬 생성",
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

            BuildScene();
            EditorUtility.DisplayDialog("완료", "클리커 씬이 생성되었습니다:\n" + ScenePath, "확인");
        }

        /// <summary>씬을 생성한다. save=false 면 메모리에만 만들고 저장하지 않는다(StudentPackage 스왑용).</summary>
        public static void BuildScene(bool save = true)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Canvas canvas = ReactiveSceneUi.CreateCanvasAndEventSystem();

            TextMeshProUGUI title = ReactiveSceneUi.CreateText(
                canvas.transform,
                "Title",
                "리액티브 클리커",
                42,
                FontStyles.Bold
            );
            ReactiveSceneUi.SetAnchorTopLeft(title.gameObject, new Vector2(60, -30), new Vector2(800, 60));

            // ---- 왼쪽: 클릭 영역 ----
            TextMeshProUGUI combo = ReactiveSceneUi.CreateText(canvas.transform, "ComboText", "", 34, FontStyles.Bold);
            ReactiveSceneUi.SetAnchorTopLeft(combo.gameObject, new Vector2(160, -150), new Vector2(400, 50));
            combo.color = new Color(1f, 0.85f, 0.2f);

            Button clickButton = ReactiveSceneUi.CreateButton(
                canvas.transform,
                "ClickButton",
                "클릭!",
                new Vector2(120, -220),
                new Vector2(420, 320),
                48
            );

            Button feverButton = ReactiveSceneUi.CreateButton(
                canvas.transform,
                "FeverButton",
                "피버!",
                new Vector2(120, -570),
                new Vector2(200, 60),
                24
            );
            TextMeshProUGUI feverState = ReactiveSceneUi.CreateText(
                canvas.transform,
                "FeverStateText",
                "피버 (15초 쿨다운)",
                20,
                FontStyles.Normal
            );
            ReactiveSceneUi.SetAnchorTopLeft(feverState.gameObject, new Vector2(340, -585), new Vector2(380, 40));
            feverState.color = Color.yellow;

            // ---- 오른쪽: 상태·업그레이드 영역 ----
            GameObject panel = ReactiveSceneUi.CreatePanel(
                canvas.transform,
                "StatusPanel",
                new Vector2(720, -130),
                new Vector2(640, 560)
            );

            TextMeshProUGUI gold = ReactiveSceneUi.CreateText(
                panel.transform,
                "GoldText",
                "골드 0",
                36,
                FontStyles.Bold
            );
            ReactiveSceneUi.SetAnchorTopLeft(gold.gameObject, new Vector2(30, -25), new Vector2(580, 50));

            TextMeshProUGUI stats = ReactiveSceneUi.CreateText(
                panel.transform,
                "StatsText",
                "클릭당 +1 · 초당 +0",
                22,
                FontStyles.Normal
            );
            ReactiveSceneUi.SetAnchorTopLeft(stats.gameObject, new Vector2(30, -85), new Vector2(580, 36));
            stats.color = new Color(0.8f, 0.9f, 1f);

            Button clickUpgrade = ReactiveSceneUi.CreateButton(
                panel.transform,
                "ClickUpgradeButton",
                "클릭 강화\n비용 10",
                new Vector2(30, -150),
                new Vector2(280, 90),
                22
            );
            Button autoUpgrade = ReactiveSceneUi.CreateButton(
                panel.transform,
                "AutoUpgradeButton",
                "도우미 고용\n비용 25",
                new Vector2(330, -150),
                new Vector2(280, 90),
                22
            );

            TextMeshProUGUI eventLog = ReactiveSceneUi.CreateText(
                panel.transform,
                "EventLogText",
                "버튼을 눌러 골드를 모아 보세요.",
                19,
                FontStyles.Normal
            );
            ReactiveSceneUi.SetAnchorTopLeft(eventLog.gameObject, new Vector2(30, -270), new Vector2(580, 260));
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
            if (save)
            {
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.Refresh();
            }
            Debug.Log("[ClickerGameSceneCreator] 씬 생성 완료: " + ScenePath);
        }
    }
}
