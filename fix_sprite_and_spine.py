import os
import re

file_path = r'C:\Unity\SlotBase-HotChili\Assets\Scripts\Managers\SlotView.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

# 1. Fix nonBlankIds in SetReelSymbols
code = code.replace('List<int> nonBlankIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };', 'List<int> nonBlankIds = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };')

# 2. Add randomized reel initialization to InitializeReels
init_pattern = r'private void InitializeReels\(\)\s*\{.*?cycleDistance = symbolHeight;\s*middlePosition = 0f;'
new_init = '''private void InitializeReels()
    {
        cycleDistance = symbolHeight;
        middlePosition = 0f;

        // Initialize with random sprites so they aren't default Blue 7s
        if (reelImagesList != null)
        {
            for (int col = 0; col < reelImagesList.Count; col++)
            {
                SetReelSymbols(col, null, true);
            }
        }'''
code = re.sub(init_pattern, new_init, code, flags=re.DOTALL)

# 3. Fix the textureArray NullReferenceException
# In StartWinAnimation, StartContinuousWinAnimation, and StartWheelWinAnimation
# the textureArray is being set to null.
# Let's just find imageAnim.textureArray = animSprites; and wrap it.
code = code.replace('imageAnim.textureArray = animSprites;', '''if (animSprites != null)
            {
                imageAnim.textureArray = animSprites;
            }''')

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

print('Patched successfully!')
