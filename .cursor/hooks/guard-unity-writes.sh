#!/usr/bin/env bash
# Cursor preToolUse 가드: Unity가 관리하는 파일(.meta / .unity / packages-lock.json)에 대한
# Write 도구 호출을 차단한다. Claude Code의 .claude/settings.json deny와 동일한 보호를 Cursor에 제공.
#
# Cursor preToolUse 입력(stdin JSON): { "tool_name": "Write", "tool_input": { ...경로/내용... }, ... }
# 차단: stdout 으로 {"permission":"deny", ...} 를 출력 (exit 0).
# 공식 문서가 Write 의 tool_input 키명을 명시하지 않으므로, tool_input 의 문자열 값들 중
# 줄바꿈 없는(=경로로 보이는) 값이 차단 패턴에 걸리면 거부한다.
set +e

payload="$(cat)"
[[ -z "$payload" ]] && exit 0
command -v node >/dev/null 2>&1 || exit 0

printf '%s' "$payload" | node -e '
const fs = require("fs");
let d;
try {
  d = JSON.parse(fs.readFileSync(0, "utf8"));
} catch {
  process.exit(0);
}
const inp = d.tool_input || {};
const BLOCK = /(^|[\/\\])packages-lock\.json$|\.meta$|\.unity$/i;
// 알려진 경로 키 우선(공백 포함 경로 정확 매칭, 파일 내용 오탐 방지), 없으면 줄바꿈 없는 값으로 폴백
const pathKeys = ["file_path", "path", "target_file", "relative_path", "filePath", "targetFile", "uri"];
let cand = pathKeys.map((k) => inp[k]).filter((v) => typeof v === "string");
if (cand.length === 0) {
  cand = Object.values(inp).filter((v) => typeof v === "string" && !v.includes("\n"));
}
const hit = cand.find((v) => BLOCK.test(v.trim()));
if (hit) {
  const f = hit.trim();
  process.stdout.write(
    JSON.stringify({
      permission: "deny",
      user_message: `Unity 관리 파일(${f})은 직접 편집 금지 — Unity 에디터로 변경하세요.`,
      agent_message: `Blocked: "${f}" is a Unity-managed file (.meta / .unity / packages-lock.json). Do not write it directly; change it via the Unity Editor.`,
    }),
  );
}
'
exit 0
