import os
import re

file_path = r'C:\Unity\SlotBase-HotChili\Assets\Scripts\Managers\SlotView.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

pattern = r'if \(animRenderer != null\) animRenderer\.enabled = false;\s*spine\.SkeletonGraphic\.MatchRectTransformWithBounds\(\);\s*spine\.Play\(true\);\s*\}'
replacement = 'var ar = imageAnim.rendererDelegate != null ? imageAnim.rendererDelegate : imageAnim.GetComponent<UnityEngine.UI.Image>(); if (ar != null) ar.enabled = false; spine.SkeletonGraphic.MatchRectTransformWithBounds(); spine.Play(true); }'

code = re.sub(pattern, replacement, code)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

print('Success')
