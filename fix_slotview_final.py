import os
import re

file_path = r'C:\Unity\SlotBase-HotChili\Assets\Scripts\Managers\SlotView.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

# 1. Replace StartWinAnimation exactly
# Find from "private IEnumerator StartWinAnimation" to "private void StartContinuousWinAnimation"
start_win_pattern = r'private IEnumerator StartWinAnimation\(.*?(?=private void StartContinuousWinAnimation)'
new_start_win = '''private IEnumerator StartWinAnimation(List<int> flatPositions, Action onComplete)
    {
        if (flatPositions == null || flatPositions.Count == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        List<GameObject> activeSpines = new List<GameObject>();
        List<Image> hiddenImages = new List<Image>();

        foreach (int flatIndex in flatPositions)
        {
            if (flatIndex < 0 || flatIndex >= 9) continue;
            int col = flatIndex % 3;
            int row = flatIndex / 3;

            Image symbolImage = null;
            if (reelImagesList != null && col < reelImagesList.Length)
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

        onComplete?.Invoke();
    }

    '''
code = re.sub(start_win_pattern, new_start_win, code, flags=re.DOTALL)

# 2. Fix the extra } before HideAllWinLineTexts
# Replace double }} before HideAllWinLineTexts with a single }
code = re.sub(r'}\s*}\s*private void HideAllWinLineTexts', r'}\n\n    private void HideAllWinLineTexts', code)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

print('SlotView.cs completely fixed!')
