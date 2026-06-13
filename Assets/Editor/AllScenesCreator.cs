using System;
using System.Collections.Generic;
using ReactiveStudy.Basics.Editor;
using ReactiveStudy.Clicker.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ReactiveStudy.Tools.Editor
{
    /// <summary>
    /// 강사용 데모 씬 5개(02~06)를 단원 순서대로 한 번에 생성·갱신한다.
    /// 메뉴: Tools > Reactive Study > Create All Scenes
    ///
    /// 각 씬은 같은 경로에 덮어써 갱신되며, 챕터별 SceneCreator 가 정본이다.
    /// 학생 배포용 씬은 별도다(StudentPackageCreator 참조).
    /// </summary>
    public static class AllScenesCreator
    {
        private static readonly (string Name, Action Build)[] Steps =
        {
            ("02 CreateStreams", () => CreateStreamsSceneCreator.BuildScene()),
            ("03 Operators", () => OperatorsSceneCreator.BuildScene()),
            ("04 Lifetime", () => LifetimeSceneCreator.BuildScene()),
            ("05 BindingAsync", () => BindingAsyncSceneCreator.BuildScene()),
            ("06 ReactiveClicker", () => ClickerGameSceneCreator.BuildScene()),
        };

        [MenuItem("Tools/Reactive Study/Create All Scenes")]
        public static void CreateAllScenes()
        {
            if (
                !EditorUtility.DisplayDialog(
                    "데모 씬 전체 생성",
                    "강사용 데모 씬 5개(02~06)를 단원 순서대로 생성·갱신합니다.\n"
                        + "기존 씬은 같은 경로에 덮어써집니다.\n"
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

            List<string> failures = BuildAllScenes();

            if (failures.Count == 0)
                EditorUtility.DisplayDialog("완료", $"데모 씬 {Steps.Length}개를 모두 생성했습니다.", "확인");
            else
                EditorUtility.DisplayDialog(
                    "일부 실패",
                    "다음 씬 생성에 실패했습니다(Console 확인):\n- " + string.Join("\n- ", failures),
                    "확인"
                );
        }

        /// <summary>비대화형 일괄 생성(자동화·배치용). 실패한 씬 이름 목록을 반환한다.</summary>
        public static List<string> BuildAllScenes()
        {
            var failures = new List<string>();
            try
            {
                for (int i = 0; i < Steps.Length; i++)
                {
                    (string name, Action build) = Steps[i];
                    EditorUtility.DisplayProgressBar("데모 씬 생성", name, (float)i / Steps.Length);
                    try
                    {
                        build();
                    }
                    catch (Exception e)
                    {
                        failures.Add(name);
                        Debug.LogError($"[AllScenesCreator] '{name}' 생성 실패: {e}");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            // 시작 씬을 열어 둔다(첫 차시).
            if (System.IO.File.Exists(CreateStreamsSceneCreator.ScenePath))
                EditorSceneManager.OpenScene(CreateStreamsSceneCreator.ScenePath, OpenSceneMode.Single);

            Debug.Log($"[AllScenesCreator] 완료 — 성공 {Steps.Length - failures.Count}/{Steps.Length}.");
            return failures;
        }
    }
}
