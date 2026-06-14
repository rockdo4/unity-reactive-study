using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReactiveStudy.Basics.Editor
{
    /// <summary>
    /// 1부(R3 기초) 데모를 한 씬에 모은 통합 씬을 생성한다.
    /// 02~05차시 패널 5개(생성·연산자·수명·바인딩·비동기)를 2열로 배치한다.
    /// 메뉴: Tools > Reactive Study > 01. R3 기초 통합 씬 생성
    ///
    /// 각 패널의 정본은 챕터별 SceneCreator 의 Build*Panel 이며, 여기서는 위치만 지정해 재사용한다.
    /// 학생 배포용 통합 씬은 StudentPackageCreator 가 이 BuildScene 으로 만든 뒤 컴포넌트를 My* 로 스왑한다.
    /// </summary>
    public static class ReactiveBasicsSceneCreator
    {
        public const string ScenePath = "Assets/Scenes/ReactiveBasics.unity";

        // 2열 레이아웃(1920x1080 기준): 왼쪽 열 = 생성·연산자·수명, 오른쪽 열 = 바인딩·비동기.
        private static readonly Vector2 PosCreate = new(40, -90);
        private static readonly Vector2 PosOperators = new(40, -410);
        private static readonly Vector2 PosLifetime = new(40, -740);
        private static readonly Vector2 PosBinding = new(980, -90);
        private static readonly Vector2 PosAsync = new(980, -410);

        [MenuItem("Tools/Reactive Study/01. R3 기초 통합 씬 생성")]
        public static void CreateSceneInteractive()
        {
            if (
                !EditorUtility.DisplayDialog(
                    "01. R3 기초 통합 씬 생성",
                    "새 씬을 만들어 1부 데모 5섹션(생성·연산자·수명·바인딩·비동기)을 2열로 구성합니다.\n"
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
            EditorUtility.DisplayDialog("완료", "통합 기초 데모 씬이 생성되었습니다:\n" + ScenePath, "확인");
        }

        /// <summary>씬을 생성한다. save=false 면 메모리에만 만들고 저장하지 않는다(StudentPackage 스왑용).</summary>
        public static void BuildScene(bool save = true)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Canvas canvas = ReactiveSceneUi.CreateCanvasAndEventSystem();
            ReactiveSceneUi.CreateTitle(canvas, "R3 기초 데모 - 생성 · 연산자 · 수명 · 바인딩 · 비동기 (R3)");

            CreateStreamsSceneCreator.BuildCreatePanel(canvas, PosCreate);
            OperatorsSceneCreator.BuildOperatorsPanel(canvas, PosOperators);
            LifetimeSceneCreator.BuildLifetimePanel(canvas, PosLifetime);
            BindingAsyncSceneCreator.BuildBindingPanel(canvas, PosBinding);
            BindingAsyncSceneCreator.BuildAsyncPanel(canvas, PosAsync);

            EditorSceneManager.MarkSceneDirty(scene);
            if (save)
            {
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.Refresh();
            }
            Debug.Log("[ReactiveBasicsSceneCreator] 통합 기초 씬 생성 완료: " + ScenePath);
        }
    }
}
