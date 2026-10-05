import os

file_path = r'C:\Unity\SlotBase-HotChili\Assets\Scripts\Managers\SlotView.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

# Fix the foreach loop error
bad_code = '''        foreach (var imageAnim in activeAnims)
        {
            
            SpineAnimController spine = animGO.GetComponentInChildren<SpineAnimController>();
            if (spine != null && spine.SkeletonGraphic != null && spine.SkeletonGraphic.skeletonDataAsset != null)
            {
                spine.Play(true);
            }
            else
            {
                ImageAnimation img = animGO.GetComponentInChildren<ImageAnimation>();
                if (img != null) img.StartAnimation();
            }

        }'''

good_code = '''        foreach (var imageAnim in activeAnims)
        {
            if (imageAnim != null) imageAnim.StartAnimation();
        }'''

code = code.replace(bad_code, good_code)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

print('animGO bug fixed!')
