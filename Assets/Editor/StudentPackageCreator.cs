using System;
using System.Collections.Generic;
using ReactiveStudy.Basics;
using ReactiveStudy.Basics.Editor;
using ReactiveStudy.Clicker;
using ReactiveStudy.Clicker.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ReactiveStudy.Tools.Editor
{
    /// <summary>
    /// 학생 배포용 씬·스크립트(<c>Assets/StudentPackage/</c>)를 생성하고 .unitypackage 로 익스포트한다.
    /// 강사용 데모 컴포넌트를 <c>My*</c> 타입으로 스왑해 학생 씬을 만든다.
    /// 배포 단위는 두 패키지: 1) 1부 통합 기초(완성본·공통 폰트 포함) 2) 2부 클리커(스켈레톤).
    /// 메뉴: Tools > Reactive Study > Student Package > ...
    ///
    /// 절차·운영 규칙은 <c>Docs/강사용/00. 학생 배포 가이드.md</c> 참조.
    /// </summary>
    public static class StudentPackageCreator
    {
        private const string StudentRoot = "Assets/StudentPackage";
        private const string ScenesFolder = "Assets/StudentPackage/Scenes";
        private const string ScriptsFolder = "Assets/StudentPackage/Scripts";

        private const string BasicsScenePath = ScenesFolder + "/ReactiveBasics.unity";
        private const string ClickerScenePath = ScenesFolder + "/ReactiveClicker.unity";

        // 배포 패키지 2종.
        // 1) 1부 통합 기초: 폰트·TMP 설정(공통) + 통합 5섹션 씬 + 기초 My* 5종. 학생이 가장 먼저 임포트한다.
        // 2) 2부 클리커: 클리커 씬(스켈레톤) + 클리커 My* 2종. 폰트는 1)에서 이미 들어왔다고 가정.
        private static readonly (string Title, string FileName, string[] AssetPaths)[] Packages =
        {
            (
                "1. R3 기초 통합 (완성본 · 공통 폰트 포함)",
                "R3Lecture-Student-1-Basics",
                new[]
                {
                    "Assets/Fonts",
                    "Assets/TextMesh Pro",
                    BasicsScenePath,
                    $"{ScriptsFolder}/MyCreateStreamsDemo.cs",
                    $"{ScriptsFolder}/MyOperatorsDemo.cs",
                    $"{ScriptsFolder}/MyLifetimeDemo.cs",
                    $"{ScriptsFolder}/MyBindingDemo.cs",
                    $"{ScriptsFolder}/MyAsyncDemo.cs",
                }
            ),
            (
                "2. 리액티브 클리커 (스켈레톤)",
                "R3Lecture-Student-2-Clicker",
                new[]
                {
                    ClickerScenePath,
                    $"{ScriptsFolder}/MyClickerModel.cs",
                    $"{ScriptsFolder}/MyClickerPresenter.cs",
                }
            ),
        };

        // ─────────────────────────────────────────── 씬 생성 메뉴 ───────────────────────────────────────────

        [MenuItem("Tools/Reactive Study/Student Package/Create All Student Scenes", priority = 2000)]
        public static void CreateAllStudentScenesMenu()
        {
            if (
                !EditorUtility.DisplayDialog(
                    "학생용 씬 생성",
                    "강사용 씬을 빌드한 뒤 컴포넌트를 My* 타입으로 교체해 학생용 씬 2개(통합 기초·클리커)를\n"
                        + ScenesFolder
                        + " 에 생성·갱신합니다.\n현재 열린 씬은 닫히며, 저장하지 않은 변경은 먼저 저장 여부를 묻습니다.\n\n계속하시겠습니까?",
                    "생성",
                    "취소"
                )
            )
            {
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            List<string> failures = BuildAllStudentScenes();
            if (failures.Count == 0)
                EditorUtility.DisplayDialog("완료", "학생용 씬 2개를 생성했습니다:\n" + ScenesFolder, "확인");
            else
                EditorUtility.DisplayDialog(
                    "일부 실패",
                    "다음 씬 생성에 실패했습니다(Console 확인):\n- " + string.Join("\n- ", failures),
                    "확인"
                );
        }

        /// <summary>학생용 씬 2개를 생성·저장한다(비대화형). 실패한 씬 이름 목록을 반환한다.</summary>
        public static List<string> BuildAllStudentScenes()
        {
            EnsureFolder("Assets", "StudentPackage");
            EnsureFolder(StudentRoot, "Scenes");

            var steps = new (string Name, Action Build)[]
            {
                ("ReactiveBasics", BuildBasicsStudentScene),
                ("ReactiveClicker", BuildClickerStudentScene),
            };

            var failures = new List<string>();
            try
            {
                for (int i = 0; i < steps.Length; i++)
                {
                    (string name, Action build) = steps[i];
                    EditorUtility.DisplayProgressBar("학생용 씬 생성", name, (float)i / steps.Length);
                    try
                    {
                        build();
                    }
                    catch (Exception e)
                    {
                        failures.Add(name);
                        Debug.LogError($"[StudentPackageCreator] '{name}' 생성 실패: {e}");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.Refresh();
            Debug.Log(
                $"[StudentPackageCreator] 학생 씬 생성 완료 - 성공 {steps.Length - failures.Count}/{steps.Length}."
            );
            return failures;
        }

        /// <summary>1부 통합 기초 씬: 강사용 통합 씬을 빌드한 뒤 데모 5종을 My* 로 스왑(완성본).</summary>
        private static void BuildBasicsStudentScene()
        {
            ReactiveBasicsSceneCreator.BuildScene(save: false);
            SwapComponent<CreateStreamsDemo>(typeof(MyCreateStreamsDemo));
            SwapComponent<OperatorsDemo>(typeof(MyOperatorsDemo));
            SwapComponent<LifetimeDemo>(typeof(MyLifetimeDemo));
            SwapComponent<LifetimeProbe>(typeof(MyLifetimeProbe));
            SwapComponent<BindingDemo>(typeof(MyBindingDemo));
            SwapComponent<AsyncDemo>(typeof(MyAsyncDemo));
            SaveActiveSceneAs(BasicsScenePath);
        }

        /// <summary>2부 클리커 씬: 강사용 클리커 씬을 빌드한 뒤 Presenter 를 My* 로 스왑(스켈레톤).</summary>
        private static void BuildClickerStudentScene()
        {
            ClickerGameSceneCreator.BuildScene(save: false);
            SwapComponent<ClickerPresenter>(typeof(MyClickerPresenter));
            SaveActiveSceneAs(ClickerScenePath);
        }

        // ─────────────────────────────────────────── 익스포트 메뉴 ───────────────────────────────────────────

        [MenuItem("Tools/Reactive Study/Student Package/Export All Student Packages", priority = 2100)]
        public static void ExportAllMenu()
        {
            string dir = EditorUtility.SaveFolderPanel("학생 패키지 저장 폴더 선택", "", "");
            if (string.IsNullOrEmpty(dir))
                return;
            ExportAllTo(dir);
            EditorUtility.DisplayDialog("완료", $"패키지 {Packages.Length}종을 익스포트했습니다:\n{dir}", "확인");
        }

        private static void ExportAllTo(string outputDir)
        {
            foreach ((string title, string fileName, string[] assetPaths) in Packages)
            {
                string path = System.IO.Path.Combine(outputDir, fileName + ".unitypackage");
                ExportPackage(title, assetPaths, path);
            }
        }

        [MenuItem("Tools/Reactive Study/Student Package/Export 1. Basics", priority = 2200)]
        public static void Export1() => ExportSingleViaPanel(0);

        [MenuItem("Tools/Reactive Study/Student Package/Export 2. Clicker", priority = 2201)]
        public static void Export2() => ExportSingleViaPanel(1);

        private static void ExportSingleViaPanel(int index)
        {
            (string title, string fileName, string[] assetPaths) = Packages[index];
            string path = EditorUtility.SaveFilePanel("패키지 저장", "", fileName + ".unitypackage", "unitypackage");
            if (string.IsNullOrEmpty(path))
                return;
            ExportPackage(title, assetPaths, path);
            EditorUtility.DisplayDialog("완료", $"'{title}' 패키지를 익스포트했습니다:\n{path}", "확인");
        }

        private static void ExportPackage(string title, string[] assetPaths, string path)
        {
            foreach (string assetPath in assetPaths)
            {
                string guid = AssetDatabase.AssetPathToGUID(assetPath, AssetPathToGUIDOptions.OnlyExistingAssets);
                if (string.IsNullOrEmpty(guid))
                {
                    throw new Exception(
                        $"'{title}' 패키지에 넣을 에셋이 없습니다: {assetPath} - "
                            + "학생용 씬을 먼저 생성했는지(Create All Student Scenes), "
                            + "파일이 이동·개명되지 않았는지 확인하세요."
                    );
                }
            }

            AssetDatabase.ExportPackage(assetPaths, path, ExportPackageOptions.Recurse);
            Debug.Log($"[StudentPackageCreator] '{title}' 패키지 익스포트 완료: {path}");
        }

        /// <summary>CLI 배치 진입점: 씬 생성 후 환경변수 STUDENT_PACKAGE_OUTPUT 폴더로 전체 익스포트.</summary>
        public static void BuildAndExportBatch()
        {
            List<string> failures = BuildAllStudentScenes();
            if (failures.Count > 0)
            {
                EditorApplication.Exit(1);
                return;
            }

            string outputDir = Environment.GetEnvironmentVariable("STUDENT_PACKAGE_OUTPUT");
            if (!string.IsNullOrEmpty(outputDir))
                ExportAllTo(outputDir);

            EditorApplication.Exit(0);
        }

        // ─────────────────────────────────────────── 헬퍼 ───────────────────────────────────────────

        /// <summary>씬에서 TSrc 컴포넌트를 찾아 dstType(My*)으로 교체하고 직렬화 필드를 복사한다.</summary>
        private static void SwapComponent<TSrc>(Type dstType)
            where TSrc : MonoBehaviour
        {
            TSrc src = UnityEngine.Object.FindFirstObjectByType<TSrc>(FindObjectsInactive.Include);
            if (src == null)
            {
                Debug.LogWarning($"[StudentPackageCreator] 스왑 대상을 찾지 못함: {typeof(TSrc).Name} (건너뜀)");
                return;
            }

            Component dst = src.gameObject.AddComponent(dstType);

            var srcSerialized = new SerializedObject(src);
            var dstSerialized = new SerializedObject(dst);

            SerializedProperty property = srcSerialized.GetIterator();
            if (property.NextVisible(true))
            {
                do
                {
                    if (property.propertyPath == "m_Script")
                        continue;

                    if (dstSerialized.FindProperty(property.propertyPath) == null)
                    {
                        throw new Exception(
                            $"{dstType.Name}에 '{property.propertyPath}' 필드가 없습니다 - "
                                + $"원본 {typeof(TSrc).Name}과 필드명을 동기화하세요."
                        );
                    }

                    dstSerialized.CopyFromSerializedProperty(property);
                } while (property.NextVisible(false));
            }

            dstSerialized.ApplyModifiedPropertiesWithoutUndo();
            UnityEngine.Object.DestroyImmediate(src);
            Debug.Log($"[StudentPackageCreator] 컴포넌트 스왑 완료: {typeof(TSrc).Name} → {dstType.Name}");
        }

        private static void SaveActiveSceneAs(string path)
        {
            Scene active = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(active);
            EditorSceneManager.SaveScene(active, path);
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
