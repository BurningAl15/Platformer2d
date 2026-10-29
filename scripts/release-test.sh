#!/usr/bin/env bash
set -euo pipefail
set +x

# Load only this game's entries from the local secrets file. Do not source or
# evaluate the file: it also contains credentials for unrelated projects.
load_foxstale_config() {
  local config_file="${HOME}/.fatcat-build/secrets.env"
  local line key value
  [[ -f "$config_file" ]] || return 0

  while IFS= read -r line || [[ -n "$line" ]]; do
    [[ "$line" == *=* ]] || continue
    key="${line%%=*}"
    value="${line#*=}"
    case "$key" in
      FOXSTALE_KEYSTORE_PATH|FOXSTALE_KEYSTORE_PASSWORD|FOXSTALE_KEYALIAS_NAME|FOXSTALE_KEYALIAS_PASSWORD|FOXSTALE_PLAY_STORE_JSON_KEY_PATH|FOXSTALE_ASC_KEY_ID|FOXSTALE_ASC_ISSUER_ID|FOXSTALE_ASC_KEY_FILEPATH|FOXSTALE_IOS_DEVELOPMENT_TEAM)
        if [[ -z "${!key:-}" ]]; then
          printf -v "$key" '%s' "$value"
          export "$key"
        fi
        ;;
    esac
  done < "$config_file"

  export PLAY_STORE_JSON_KEY_PATH="${PLAY_STORE_JSON_KEY_PATH:-${FOXSTALE_PLAY_STORE_JSON_KEY_PATH:-}}"
  export ASC_KEY_ID="${ASC_KEY_ID:-${FOXSTALE_ASC_KEY_ID:-}}"
  export ASC_ISSUER_ID="${ASC_ISSUER_ID:-${FOXSTALE_ASC_ISSUER_ID:-}}"
  export ASC_KEY_FILEPATH="${ASC_KEY_FILEPATH:-${FOXSTALE_ASC_KEY_FILEPATH:-}}"
  export IOS_DEVELOPMENT_TEAM="${IOS_DEVELOPMENT_TEAM:-${FOXSTALE_IOS_DEVELOPMENT_TEAM:-}}"
}

load_foxstale_config

usage() {
  cat <<'EOF'
Usage: scripts/release-test.sh android|ios|both --android-code N --ios-build N [--upload-test] [--skip-build]

Builds local Android AAB and/or iOS Xcode project. --upload-test additionally uploads
only to Google Play Internal Testing and/or TestFlight. It never targets production.
--skip-build reuses an existing iOS Xcode export for its numbered build.

Android signing: FOXSTALE_KEYSTORE_PATH, FOXSTALE_KEYSTORE_PASSWORD,
FOXSTALE_KEYALIAS_NAME, FOXSTALE_KEYALIAS_PASSWORD (all required for Android).
Play upload: PLAY_STORE_JSON_KEY_PATH.
iOS upload: ASC_KEY_ID, ASC_ISSUER_ID, ASC_KEY_FILEPATH, IOS_DEVELOPMENT_TEAM.
Optional Unity executable override: FOXSTALE_UNITY_PATH.
The script silently loads this game's whitelisted values from ~/.fatcat-build/secrets.env.

Use fresh positive build numbers verified in the respective store. No credentials are
stored in this repository or printed by this script.
EOF
}

die() { printf 'ERROR: %s\n' "$1" >&2; exit 2; }
[[ $# -ge 1 ]] || { usage >&2; exit 2; }
mode="$1"; shift
case "$mode" in android|ios|both) ;; -h|--help) usage; exit 0 ;; *) usage >&2; exit 2 ;; esac
android_code=""
ios_build=""
upload=0
skip_build=0
while (($#)); do
  case "$1" in
    --android-code) [[ $# -ge 2 ]] || die "Missing Android build number"; android_code="$2"; shift 2 ;;
    --ios-build) [[ $# -ge 2 ]] || die "Missing iOS build number"; ios_build="$2"; shift 2 ;;
    --upload-test) upload=1; shift ;;
    --skip-build) skip_build=1; shift ;;
    -h|--help) usage; exit 0 ;;
    *) die "Unknown option" ;;
  esac
done

[[ "$(uname -s)" == "Darwin" ]] || die "This pipeline requires macOS"
if [[ "$mode" == android || "$mode" == both ]]; then
  [[ "$android_code" =~ ^[1-9][0-9]*$ ]] || die "Provide --android-code with a fresh positive integer"
  for name in FOXSTALE_KEYSTORE_PATH FOXSTALE_KEYSTORE_PASSWORD FOXSTALE_KEYALIAS_NAME FOXSTALE_KEYALIAS_PASSWORD; do
    [[ -n "${!name:-}" ]] || die "Android signing configuration is incomplete"
  done
  [[ "$FOXSTALE_KEYSTORE_PATH" = /* && -f "$FOXSTALE_KEYSTORE_PATH" ]] || die "Android keystore must be an existing absolute file"
fi
if [[ "$mode" == ios || "$mode" == both ]]; then
  [[ "$ios_build" =~ ^[1-9][0-9]*$ ]] || die "Provide --ios-build with a fresh positive integer"
fi
if ((skip_build)) && [[ "$mode" != ios ]]; then
  die "--skip-build currently supports iOS only"
fi

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)"
project_root="$(cd "$script_dir/.." && pwd -P)"
unity_bin="${FOXSTALE_UNITY_PATH:-/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity}"
[[ -x "$unity_bin" ]] || die "Unity 6000.6.0f1 executable not found"
[[ "$(sed -n 's/^m_EditorVersion: //p' "$project_root/ProjectSettings/ProjectVersion.txt" | head -n 1)" == "6000.6.0f1" ]] || die "Project Unity version differs from the configured build version"

# Unity refuses batch mode when this project is already open in the GUI.
# Detect that condition before starting a second editor and give a useful hint.
editor_instance="$project_root/Library/EditorInstance.json"
if [[ -f "$editor_instance" ]]; then
  editor_pid="$(sed -n 's/.*"process_id"[[:space:]]*:[[:space:]]*\([0-9][0-9]*\).*/\1/p' "$editor_instance" | head -n 1)"
  if [[ "$editor_pid" =~ ^[1-9][0-9]*$ ]] && kill -0 "$editor_pid" 2>/dev/null; then
    die "Unity Editor already has this project open; save work, close the Editor, then retry batch mode"
  fi
fi

if ((upload)); then
  command -v fastlane >/dev/null 2>&1 || die "Fastlane is required for --upload-test"
  if [[ "$mode" == android || "$mode" == both ]]; then
    [[ -n "${PLAY_STORE_JSON_KEY_PATH:-}" && -f "$PLAY_STORE_JSON_KEY_PATH" ]] || die "Google Play service account file is missing"
  fi
  if [[ "$mode" == ios || "$mode" == both ]]; then
    [[ -n "${ASC_KEY_ID:-}" && -n "${ASC_ISSUER_ID:-}" && -n "${IOS_DEVELOPMENT_TEAM:-}" ]] || die "App Store Connect or signing configuration is incomplete"
    [[ -n "${ASC_KEY_FILEPATH:-}" && "$ASC_KEY_FILEPATH" = /* && -f "$ASC_KEY_FILEPATH" ]] || die "App Store Connect private key file is missing"
  fi
fi

build_root="$project_root/Builds/CI"
mkdir -p "$build_root/logs"
run_unity() {
  local method="$1" log_file="$2"
  "$unity_bin" -batchmode -quit -nographics -projectPath "$project_root" -executeMethod "$method" -logFile "$log_file" || die "Unity build failed; inspect the local build log"
}

if [[ "$mode" == android || "$mode" == both ]]; then
  android_aab="$build_root/Android/FoxsTale_${android_code}.aab"
  [[ ! -e "$android_aab" ]] || die "Android build output already exists; choose a fresh versionCode"
  mkdir -p "$(dirname "$android_aab")"
  export FOXSTALE_ANDROID_VERSION_CODE="$android_code" FOXSTALE_ANDROID_OUTPUT_PATH="$android_aab"
  run_unity BuildAutomation.BuildAndroidAAB "$build_root/logs/android_${android_code}.log"
  [[ -s "$android_aab" ]] || die "Unity did not create a nonempty Android AAB"
fi

if [[ "$mode" == ios || "$mode" == both ]]; then
  ios_project="$build_root/iOS/XcodeProject_${ios_build}"
  if ((skip_build)); then
    [[ -d "$ios_project" ]] || die "Existing iOS export not found"
  else
    [[ ! -e "$ios_project" ]] || die "iOS export already exists; choose a fresh build number"
    export FOXSTALE_IOS_BUILD_NUMBER="$ios_build" FOXSTALE_IOS_OUTPUT_PATH="$ios_project"
    run_unity BuildAutomation.BuildiOS "$build_root/logs/ios_${ios_build}.log"
  fi
  [[ -d "$ios_project/Unity-iPhone.xcodeproj" ]] || die "Unity did not create an Xcode project"
fi

if ((upload)); then
  if [[ "$mode" == android || "$mode" == both ]]; then
    (cd "$project_root" && ANDROID_AAB_PATH="$android_aab" fastlane android internal)
  fi
  if [[ "$mode" == ios || "$mode" == both ]]; then
    (cd "$project_root" && UNITY_IOS_PROJECT="$ios_project" fastlane ios upload_testflight)
  fi
fi

printf 'Build complete. Store upload: %s\n' "$([[ "$upload" == 1 ]] && echo requested || echo skipped)"
