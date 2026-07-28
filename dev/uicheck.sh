#!/bin/sh
# Compile the UI (and editor) scripts against a real Unity install without
# opening the editor. `dotnet test` only covers Core and AI, which are plain
# C#, so nothing else catches a typo'd UnityEngine call until a full build.
#
#   UNITY=/opt/unity-6000.0.80f1 dev/uicheck.sh
set -e

UNITY=${UNITY:-/opt/unity-6000.0.80f1}
MANAGED="$UNITY/Editor/Data/Managed"
ENGINE="$MANAGED/UnityEngine"
ROOT=$(cd "$(dirname "$0")/.." && pwd)

if [ ! -d "$ENGINE" ]; then
  echo "no Unity managed assemblies under $ENGINE - set UNITY to your install" >&2
  exit 1
fi

# uGUI ships as source in the built-in package, but the editor keeps a compiled
# copy in its project-template cache. Close enough, and it saves building it.
UGUI=$(find "$UNITY/Editor/Data/Resources/PackageManager/ProjectTemplates/libcache" \
  -name UnityEngine.UI.dll 2>/dev/null | head -1)
if [ -z "$UGUI" ]; then
  echo "couldn't find UnityEngine.UI.dll in the template cache" >&2
  exit 1
fi

WORK=$(mktemp -d)
trap 'rm -rf "$WORK"' EXIT

emit_proj() {
  name=$1; sources=$2; refs=$3
  mkdir -p "$WORK/$name"
  {
    echo '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>'
    echo '<TargetFramework>netstandard2.1</TargetFramework>'
    echo '<EnableDefaultCompileItems>false</EnableDefaultCompileItems>'
    echo '<LangVersion>9</LangVersion><ProduceReferenceAssembly>false</ProduceReferenceAssembly>'
    echo "<AssemblyName>$name</AssemblyName>"
    echo '</PropertyGroup><ItemGroup>'
    for s in $sources; do echo "<Compile Include=\"$ROOT/$s/**/*.cs\" />"; done
    echo '</ItemGroup><ItemGroup>'
    for r in $refs; do echo "<Reference Include=\"$r\" />"; done
    echo '</ItemGroup></Project>'
  } > "$WORK/$name/$name.csproj"
}

ENGINE_REFS="$ENGINE/UnityEngine.dll $ENGINE/UnityEngine.CoreModule.dll \
$ENGINE/UnityEngine.UIModule.dll $ENGINE/UnityEngine.IMGUIModule.dll \
$ENGINE/UnityEngine.TextRenderingModule.dll $ENGINE/UnityEngine.InputLegacyModule.dll \
$ENGINE/UnityEngine.InputModule.dll $ENGINE/UnityEngine.UIElementsModule.dll $UGUI"

emit_proj runtime "Assets/Scripts/Core Assets/Scripts/AI Assets/Scripts/UI" "$ENGINE_REFS"
emit_proj editor "Assets/Editor" "$ENGINE/UnityEngine.dll $ENGINE/UnityEngine.CoreModule.dll $MANAGED/UnityEditor.dll"

for p in runtime editor; do
  printf '%s: ' "$p"
  dotnet build "$WORK/$p/$p.csproj" -v q -nologo | grep -E "error|Build succeeded" || true
done
