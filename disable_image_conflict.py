import os
import re

file_path = r'C:\Unity\SlotBase-HotChili\Assets\Scripts\Managers\SlotView.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

# For StartContinuousWinAnimation
old_continuous = 'SpineAnimController spine = (imageAnim != null) ? imageAnim.GetComponentInParent<SpineAnimController>() : null;\\n            if (spine != null && spine.SkeletonGraphic != null && spine.SkeletonGraphic.skeletonDataAsset != null) { spine.Play(true); }'
new_continuous = 'SpineAnimController spine = (imageAnim != null) ? imageAnim.GetComponentInParent<SpineAnimController>() : null;\\n            if (spine != null && spine.SkeletonGraphic != null && spine.SkeletonGraphic.skeletonDataAsset != null) { if (animRenderer != null) animRenderer.enabled = false; spine.Play(true); }'
code = code.replace(old_continuous, new_continuous)


with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

print('Disabled Image conflict!')
