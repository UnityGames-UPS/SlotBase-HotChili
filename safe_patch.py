import os

file_path = r'C:\Unity\SlotBase-HotChili\Assets\Scripts\Managers\SlotView.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

# 1. Add using Spine.Unity;
if 'using Spine.Unity;' not in code:
    code = code.replace('using UnityEngine.UI;', 'using UnityEngine.UI;\nusing Spine.Unity;')

# 2. Fix Sprite variables
old_sprites = '''    [Header("Symbol Sprites - Assign by Name")]
    [SerializeField] private Sprite spriteRed3X;
    [SerializeField] private Sprite spriteBlue2X;
    [SerializeField] private Sprite spriteBlue7;
    [SerializeField] private Sprite spriteWhite7;
    [SerializeField] private Sprite spriteWhite7Bar;
    [SerializeField] private Sprite spriteRed7;
    [SerializeField] private Sprite spriteTripleBar;
    [SerializeField] private Sprite spriteDoubleBar;
    [SerializeField] private Sprite spriteSingleBar;
    [SerializeField] private Sprite spriteSpin;
    [SerializeField] private Sprite spriteGreenWheel;
    [SerializeField] private Sprite spriteDoubleWheel;
    [SerializeField] private Sprite spriteRedWheel;'''

new_sprites = '''    [Header("Symbol Sprites - Matches Backend JSON")]
    [SerializeField] private Sprite spriteGreenChilli; // ID 0
    [SerializeField] private Sprite spriteYellowChilli; // ID 1
    [SerializeField] private Sprite spriteOrangeChilli; // ID 2
    [SerializeField] private Sprite spriteRedChilli; // ID 3
    [SerializeField] private Sprite spriteTripleBar; // ID 4
    [SerializeField] private Sprite spriteDoubleBar; // ID 5
    [SerializeField] private Sprite spriteSingleBar; // ID 6
    [SerializeField] private Sprite spriteRedSeven; // ID 7
    [SerializeField] private Sprite spriteBlueSeven; // ID 8

    [Header("Spine Win Animations - Matches Backend JSON")]
    [SerializeField] private SkeletonDataAsset spineGreenChilli;
    [SerializeField] private SkeletonDataAsset spineYellowChilli;
    [SerializeField] private SkeletonDataAsset spineOrangeChilli;
    [SerializeField] private SkeletonDataAsset spineRedChilli;
    [SerializeField] private SkeletonDataAsset spineTripleBar;
    [SerializeField] private SkeletonDataAsset spineDoubleBar;
    [SerializeField] private SkeletonDataAsset spineSingleBar;
    [SerializeField] private SkeletonDataAsset spineRedSeven;
    [SerializeField] private SkeletonDataAsset spineBlueSeven;
    private SkeletonDataAsset[] spineDataArray;'''
code = code.replace(old_sprites, new_sprites)

# 3. Remove [SerializeField] from animSprites
code = code.replace('[SerializeField] private List<Sprite> animSprites', 'private List<Sprite> animSprites')

# 4. Replace BuildSymbolSpriteArray
old_build_sprite = '''    private void BuildSymbolSpriteArray()
    {
       /* symbolSprites = new Sprite[15];
        symbolSprites[1] = spriteRed3X;
        symbolSprites[2] = spriteBlue2X;
        symbolSprites[3] = spriteBlue7;
        symbolSprites[4] = spriteWhite7;
        symbolSprites[5] = spriteWhite7Bar;
        symbolSprites[6] = spriteRed7;
        symbolSprites[7] = spriteTripleBar;
        symbolSprites[8] = spriteDoubleBar;
        symbolSprites[9] = spriteSingleBar;
        symbolSprites[10] = spriteSpin;
        symbolSprites[11] = spriteGreenWheel;
        symbolSprites[12] = spriteDoubleWheel;
        symbolSprites[13] = spriteRedWheel;
        symbolSprites[14] = spriteRedWheel;*/'''

new_build_sprite = '''    private void BuildSymbolSpriteArray()
    {
        symbolSprites = new Sprite[10];
        symbolSprites[0] = spriteGreenChilli;
        symbolSprites[1] = spriteYellowChilli;
        symbolSprites[2] = spriteOrangeChilli;
        symbolSprites[3] = spriteRedChilli;
        symbolSprites[4] = spriteTripleBar;
        symbolSprites[5] = spriteDoubleBar;
        symbolSprites[6] = spriteSingleBar;
        symbolSprites[7] = spriteRedSeven;
        symbolSprites[8] = spriteBlueSeven;'''
code = code.replace(old_build_sprite, new_build_sprite)

# 5. Add BuildSpineArray to Start
code = code.replace('BuildSymbolSpriteArray();', 'BuildSymbolSpriteArray();\n            BuildSpineArray();')

# 6. Add BuildSpineArray method and GetSpineData
spine_methods = '''
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
code = code.replace('private void BuildSymbolSpriteArray()', spine_methods + '\n    private void BuildSymbolSpriteArray()')

# 7. Hijack WinBox loop safely inside SlotView
old_hijack_target = '''            ImageAnimation imageAnim = animGO.GetComponentInChildren<ImageAnimation>();
            if (imageAnim == null) continue;

            if (currentDisplayMatrix == null || col >= currentDisplayMatrix.Count || row >= currentDisplayMatrix[col].Count) continue;'''

new_hijack_target = '''            SpineAnimController spineAnim = animGO.GetComponentInChildren<SpineAnimController>();
            if (currentDisplayMatrix != null && col < currentDisplayMatrix.Count && row < currentDisplayMatrix[col].Count)
            {
                int sId = currentDisplayMatrix[col][row];
                if (spineAnim != null) spineAnim.SetSkeletonData(GetSpineData(sId));
            }

            ImageAnimation imageAnim = animGO.GetComponentInChildren<ImageAnimation>();
            if (imageAnim == null) continue;

            if (currentDisplayMatrix == null || col >= currentDisplayMatrix.Count || row >= currentDisplayMatrix[col].Count) continue;'''

code = code.replace(old_hijack_target, new_hijack_target)

# 8. StartAnimation override
old_start = 'imageAnim.StartAnimation();'
new_start = '''
            SpineAnimController spine = (imageAnim != null) ? imageAnim.GetComponent<SpineAnimController>() : null;
            if (spine != null && spine.SkeletonGraphic != null && spine.SkeletonGraphic.skeletonDataAsset != null) { spine.Play(true); }
            else { imageAnim.StartAnimation(); }'''
code = code.replace(old_start, new_start)

# 9. StopAnimation override
old_stop = 'if (imageAnim != null) imageAnim.StopAnimation();'
new_stop = '''if (imageAnim != null) { imageAnim.StopAnimation(); SpineAnimController spine = imageAnim.GetComponent<SpineAnimController>(); if (spine != null) spine.Stop(); }'''
code = code.replace(old_stop, new_stop)

# 10. DisableAllOverlays Spine fix
old_disable = 'if (parent != null)'
new_disable = 'if (parent != null) { SpineAnimController spine = parent.GetComponentInChildren<SpineAnimController>(); if (spine != null) spine.Stop(); } \n                        if (parent != null)'
code = code.replace(old_disable, new_disable)

# 11. Remove the empty sprites restriction
code = code.replace('if (animSprites == null || animSprites.Count == 0) continue;', '')


with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

print('Safe patch applied!')
