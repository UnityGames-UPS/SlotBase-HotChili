import os
import re

file_path = r'C:\Unity\SlotBase-HotChili\Assets\Scripts\Managers\SlotView.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

# 1. Replace AnimateWinPositionsSingleLoop entirely
pattern = r'private IEnumerator AnimateWinPositionsSingleLoop\(.*?private void StartContinuousWinAnimation'
new_code = '''private IEnumerator AnimateWinPositionsSingleLoop(IEnumerable<int> flatPositions)
    {
        if (flatPositions == null) yield break;

        List<GameObject> activeSpines = new List<GameObject>();
        List<Image> hiddenImages = new List<Image>();

        int reelCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.reelCount : 3;

        foreach (int flatIndex in flatPositions)
        {
            int row = flatIndex / reelCount;
            int col = flatIndex % reelCount;

            if (col < 0 || col >= 5) continue;

            Image symbolImage = null;
            if (reelImagesList != null && col < reelImagesList.Count)
            {
                var reelImages = reelImagesList[col].images;
                int imageIndex = 6 + row;
                if (imageIndex >= 0 && imageIndex < reelImages.Count)
                {
                    symbolImage = reelImages[imageIndex];
                }
            }

            var animGO = WinBox(winAnimationColumns, col, row);
            if (animGO == null) continue;

            SpineAnimController spineAnim = animGO.GetComponentInChildren<SpineAnimController>();
            if (spineAnim == null) continue;

            if (currentDisplayMatrix == null || col >= currentDisplayMatrix.Count || row >= currentDisplayMatrix[col].Count) continue;
            int symbolId = currentDisplayMatrix[col][row];

            SkeletonDataAsset dataAsset = GetSpineData(symbolId);
            if (dataAsset != null)
            {
                spineAnim.SetSkeletonData(dataAsset);
                animGO.SetActive(true);
                spineAnim.Play(true);
                activeSpines.Add(animGO);

                if (symbolImage != null)
                {
                    symbolImage.color = new Color(1f, 1f, 1f, 0f);
                    hiddenImages.Add(symbolImage);
                }
            }
        }

        yield return new WaitForSeconds(winSymbolLoopDuration);

        foreach (var animGO in activeSpines)
        {
            if (animGO != null)
            {
                SpineAnimController spine = animGO.GetComponentInChildren<SpineAnimController>();
                if (spine != null) spine.Stop();
                animGO.SetActive(false);
            }
        }

        foreach (var img in hiddenImages)
        {
            if (img != null)
            {
                img.color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }

    private void StartContinuousWinAnimation'''

code = re.sub(pattern, new_code, code, flags=re.DOTALL)

# 2. Fix reelImagesList.Length to .Count in StartContinuousWinAnimation
code = code.replace('reelImagesList.Length', 'reelImagesList.Count')

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

print('Fully patched AnimateWinPositionsSingleLoop and fixed Length vs Count!')
