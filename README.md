# Unity Template

Unity 6.3 / URP 17 기반 프로젝트 템플릿. GitHub **템플릿 저장소(Template repository)** 로 새 프로젝트를 만들 수 있습니다.

## 새 프로젝트 시작

1. GitHub에서 **Use this template** → 새 저장소 생성 (또는 `git clone` 후 `origin` 변경).
2. Unity Hub로 프로젝트 열기 → **Edit > Project Settings > Player**에서 `companyName`, `productName` 변경.
3. 이 README를 프로젝트 소개로 교체.
4. 최초 1회 로컬 개발 환경 초기화:

```bash
bash .claude/skills/setup/setup.sh   # Claude Code: `/setup` 스킬로도 실행 가능
```

대용량 바이너리가 많아지면, 새 저장소에서 [Git LFS](https://git-lfs.github.com/)를 별도로 도입하면 됩니다.
