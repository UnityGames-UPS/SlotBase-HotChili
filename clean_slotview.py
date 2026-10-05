import os
import re

file_path = r'C:\Unity\SlotBase-HotChili\Assets\Scripts\Managers\SlotView.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

# 1. Remove the old Win Animation Sprite Arrays block
code = re.sub(
    r'\s*\[Header\("Win Animation Sprite Arrays for All Icons"\)\].*?\[SerializeField\] private List<Sprite> animSpritesRedWheel;\s*private List<Sprite>\[\] animationSpriteArrays;',
    '',
    code,
    flags=re.DOTALL
)

# 2. Remove GetAnimSprites method
code = re.sub(
    r'\s*private List<Sprite> GetAnimSprites\(int symbolId\)\s*\{.*?\n    \}',
    '',
    code,
    flags=re.DOTALL
)

# 3. Remove initialization in BuildSymbolSpriteArray (if it exists)
code = re.sub(
    r'\s*animationSpriteArrays = new List<Sprite>\[15\];.*?(?=\n\s*})',
    '',
    code,
    flags=re.DOTALL
)

# 4. Clean up the win animation logic (remove ImageAnimation fallback completely)
# Let's replace the whole WinBox block
winbox_logic_old = '''
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
            }'''

winbox_logic_new = '''
            SpineAnimController spineAnim = animGO.GetComponentInChildren<SpineAnimController>();
            if (spineAnim == null) continue;

            if (currentDisplayMatrix == null || col >= currentDisplayMatrix.Count || row >= currentDisplayMatrix[col].Count) continue;
            int symId = currentDisplayMatrix[col][row];
            
            SkeletonDataAsset dataAsset = GetSpineData(symId);
            if (dataAsset != null)
            {
                spineAnim.SetSkeletonData(dataAsset);
            }'''

code = code.replace(winbox_logic_old, winbox_logic_new)

# 5. Clean up StartAnimation() override
start_anim_old = '''
            SpineAnimController spine = animGO.GetComponentInChildren<SpineAnimController>();
            if (spine != null && spine.SkeletonGraphic != null && spine.SkeletonGraphic.skeletonDataAsset != null)
            {
                spine.Play(true);
            }
            else
            {
                ImageAnimation img = animGO.GetComponentInChildren<ImageAnimation>();
                if (img != null) img.StartAnimation();
            }'''

start_anim_new = '''
            SpineAnimController spine = animGO.GetComponentInChildren<SpineAnimController>();
            if (spine != null && spine.SkeletonGraphic != null && spine.SkeletonGraphic.skeletonDataAsset != null)
            {
                spine.Play(true);
            }'''

code = code.replace(start_anim_old, start_anim_new)

# 6. Clean up StopAnimation() override
stop_anim_old = '''
                                if (imageAnim != null) imageAnim.StopAnimation();
                                SpineAnimController spine = animGO.GetComponentInChildren<SpineAnimController>();
                                if (spine != null) spine.Stop();'''

stop_anim_new = '''
                                SpineAnimController spine = animGO.GetComponentInChildren<SpineAnimController>();
                                if (spine != null) spine.Stop();'''

code = code.replace(stop_anim_old, stop_anim_new)

# 7. Clean up the loop complete / wait logic
# Because activeAnims might be tracking ImageAnimation (which we removed)
# We actually just want it to wait for the loop duration.
# Actually, the user doesn't care if we leave activeAnims as an empty list that just bypasses to WaitForSeconds.
# Let's remove activeAnims entirely if possible, but that might be complex regex.
# Let's just remove the ImageAnimation tracking.
code = re.sub(
    r'\s*List<ImageAnimation> activeAnims = new List<ImageAnimation>\(\);.*?(?=\s*foreach)',
    '',
    code,
    flags=re.DOTALL
)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

print('SlotView.cs stripped of legacy ImageAnimation code!')
