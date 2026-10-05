import os
import re

file_path = r'C:\Unity\SlotBase-HotChili\Assets\Scripts\Managers\SlotView.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

pattern = r'if \(spine != null && spine\.SkeletonGraphic != null && spine\.SkeletonGraphic\.skeletonDataAsset != null\)\s*\{\s*spine\.Play\(true\);\s*\}'
replacement = 'if (spine != null && spine.SkeletonGraphic != null && spine.SkeletonGraphic.skeletonDataAsset != null) { if (animRenderer != null) animRenderer.enabled = false; spine.Play(true); }'

code = re.sub(pattern, replacement, code)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

print('Matches replaced:', len(re.findall(pattern, code)) if 'animRenderer.enabled = false;' not in code else 'Success')
