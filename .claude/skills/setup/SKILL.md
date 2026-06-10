---
name: setup
description: 템플릿을 클론한 직후 1회 실행하는 로컬 개발 환경 초기화. CSharpier·Prettier 설치, UnityYAMLMerge 머지 드라이버 안내. "초기 설정", "setup", "환경 셋업", "클론 후 설정" 등에 사용.
---

# 프로젝트 초기 설정

`setup.sh`를 실행해 로컬 개발 환경을 초기화한다.

```bash
bash .claude/skills/setup/setup.sh
```

## 동작

- **자동**: CSharpier(`dotnet tool restore`), Prettier(`npm ci`).
- **수동 안내**: 스크립트 마지막에 UnityYAMLMerge 머지 드라이버 등록 명령을 출력한다. 글로벌 git 설정을 바꾸므로 자동 실행하지 않는다.

## 수행 방법

1. `bash .claude/skills/setup/setup.sh`를 실행한다.
2. 자동 단계의 성공/실패를 요약해 보고한다.
3. 출력된 UnityYAMLMerge 명령을 사용자에게 보여주고, **글로벌 git 설정을 변경해도 되는지 확인한 뒤에만** 실행한다.
