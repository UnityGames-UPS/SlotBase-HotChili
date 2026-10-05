import os
import re

file_path = r'C:\Unity\SlotBase-HotChili\Assets\Scripts\Managers\SlotView.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

# 1. Add using Spine.Unity; at the top
if 'using Spine.Unity;' not in code:
    code = code.replace('using UnityEngine.UI;', 'using UnityEngine.UI;\nusing Spine.Unity;')

# 2. Add SkeletonDataAsset fields below the Sprite fields
old_sprite_fields = '''    [SerializeField] private Sprite spriteBlueSeven; // ID 8'''
new_spine_fields = '''    [SerializeField] private Sprite spriteBlueSeven; // ID 8

    [Header("Spine Win Animations - Matches Backend JSON")]
    [SerializeField] private SkeletonDataAsset spineGreenChilli; // ID 0
    [SerializeField] private SkeletonDataAsset spineYellowChilli; // ID 1
    [SerializeField] private SkeletonDataAsset spineOrangeChilli; // ID 2
    [SerializeField] private SkeletonDataAsset spineRedChilli; // ID 3
    [SerializeField] private SkeletonDataAsset spineTripleBar; // ID 4
    [SerializeField] private SkeletonDataAsset spineDoubleBar; // ID 5
    [SerializeField] private SkeletonDataAsset spineSingleBar; // ID 6
    [SerializeField] private SkeletonDataAsset spineRedSeven; // ID 7
    [SerializeField] private SkeletonDataAsset spineBlueSeven; // ID 8

    private SkeletonDataAsset[] spineDataArray;'''

if 'SkeletonDataAsset spineGreenChilli' not in code:
    code = code.replace(old_sprite_fields, new_spine_fields)

# 3. Add BuildSpineArray to Start()
if 'BuildSpineArray();' not in code:
    code = code.replace('BuildSymbolSpriteArray();', 'BuildSymbolSpriteArray();\n            BuildSpineArray();')

# 4. Add the BuildSpineArray method
if 'private void BuildSpineArray()' not in code:
    build_method = '''
    private void BuildSpineArray()
    {
        spineDataArray = new SkeletonDataAsset[10];
        spineDataArray[0] = spineGreenChilli;
        spineDataArray[1] = spineYellowChilli;
        spineDataArray[2] = spineOrangeChilli;
        spineDataArray[3] = spineRedChilli;
        spineDataArray[4] = spineTripleBar;
        spineDataArray[5] = spineDoubleBar;
        spineDataArray[6] = spineSingleBar;
        spineDataArray[7] = spineRedSeven;
        spineDataArray[8] = spineBlueSeven;
    }

    private SkeletonDataAsset GetSpineData(int symbolId)
    {
        if (spineDataArray == null || symbolId < 0 || symbolId >= spineDataArray.Length) return null;
        return spineDataArray[symbolId];
    }
'''
    code = code.replace('private void BuildSymbolSpriteArray()', build_method + '\n    private void BuildSymbolSpriteArray()')

# 5. Modify the WinAnimation logic to use SpineAnimController if it exists
# We previously injected some Spine stuff but reverted it. Now we will do it properly for Option 2!
# We will find ImageAnimation imageAnim = animGO.GetComponentInChildren<ImageAnimation>();
# and modify the logic to call SpineAnimController.

code = re.sub(
    r'ImageAnimation imageAnim = animGO\.GetComponentInChildren<ImageAnimation>\(\);\s*if \(imageAnim == null\) continue;\s*if \(currentDisplayMatrix.*?symId\];\s*if \(animSprites == null \|\| animSprites\.Count == 0\) continue;\s*imageAnim\.textureArray = animSprites;\s*imageAnim\.animationMode = ImageAnimation\.AnimationMode\.SINGLE_PHASE;\s*imageAnim\.useDynamicFramerate = true;\s*imageAnim\.dynamicLoopDuration = winSymbolLoopDuration;\s*imageAnim\.doLoopAnimation = true;',
    '''
            SpineAnimController spineAnim = animGO.GetComponentInChildren<SpineAnimController>();
            if (currentDisplayMatrix == null || col >= currentDisplayMatrix.Count || row >= currentDisplayMatrix[col].Count) continue;
            int symId = currentDisplayMatrix[col][row];
            
            if (spineAnim != null)
            {
                SkeletonDataAsset dataAsset = GetSpineData(symId);
                if (dataAsset != null)
                {
                    spineAnim.SetSkeletonData(dataAsset);
                }
            }
            else
            {
                ImageAnimation imageAnim = animGO.GetComponentInChildren<ImageAnimation>();
                if (imageAnim != null)
                {
                    var animSprites = GetAnimSprites(symId);
                    if (animSprites != null && animSprites.Count > 0)
                    {
                        imageAnim.textureArray = animSprites;
                        imageAnim.animationMode = ImageAnimation.AnimationMode.SINGLE_PHASE;
                        imageAnim.useDynamicFramerate = true;
                        imageAnim.dynamicLoopDuration = winSymbolLoopDuration;
                        imageAnim.doLoopAnimation = true;
                    }
                }
            }
''',
    code,
    flags=re.DOTALL
)

# Replace imageAnim.StartAnimation() with the dual approach
code = re.sub(
    r'imageAnim\.StartAnimation\(\);',
    '''
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
''',
    code
)

# Replace imageAnim.StopAnimation() with dual approach
code = re.sub(
    r'if \(imageAnim != null\) imageAnim\.StopAnimation\(\);',
    '''
                                if (imageAnim != null) imageAnim.StopAnimation();
                                SpineAnimController spine = animGO.GetComponentInChildren<SpineAnimController>();
                                if (spine != null) spine.Stop();
''',
    code
)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

print('SlotView.cs patched for Option 2 Spine Animations!')
