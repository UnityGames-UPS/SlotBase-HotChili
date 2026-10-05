import os
import re

file_path = r'C:\Unity\SlotBase-HotChili\Assets\Scripts\Managers\SlotView.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

# 1. Clean StartWinAnimation
start_win_old = re.search(r'private IEnumerator StartWinAnimation\(.*?yield return new WaitForSeconds\(winSymbolLoopDuration\);\s*}', code, flags=re.DOTALL)
if start_win_old:
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
    }'''
    code = code.replace(start_win_old.group(0), new_start_win)

# 2. Clean StartContinuousWinAnimation
start_cont_old = re.search(r'private void StartContinuousWinAnimation\(.*?}\s*}', code, flags=re.DOTALL)
if start_cont_old:
    new_start_cont = '''private void StartContinuousWinAnimation(IEnumerable<int> flatPositions)
    {
        if (flatPositions == null) return;
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
            int symId = currentDisplayMatrix[col][row];
            
            SkeletonDataAsset dataAsset = GetSpineData(symId);
            if (dataAsset != null)
            {
                spineAnim.SetSkeletonData(dataAsset);
                animGO.SetActive(true);
                spineAnim.Play(true);

                if (symbolImage != null)
                {
                    symbolImage.color = new Color(1f, 1f, 1f, 0f);
                }
            }
        }
    }'''
    code = code.replace(start_cont_old.group(0), new_start_cont)

# 3. Clean StartWheelWinAnimation
start_wheel_old = re.search(r'private void StartWheelWinAnimation\(.*?onComplete\?\.Invoke\(\);\s*}\s*}', code, flags=re.DOTALL)
if start_wheel_old:
    new_start_wheel = '''private void StartWheelWinAnimation(int wheelRow, Action onComplete)
    {
        onComplete?.Invoke();
    }'''
    code = code.replace(start_wheel_old.group(0), new_start_wheel)

# 4. Clean DisableAllOverlays
disable_all_old = re.search(r'private void DisableAllOverlays\(\).*?}\s*}', code, flags=re.DOTALL)
if disable_all_old:
    new_disable_all = '''private void DisableAllOverlays()
    {
        if (winAnimationColumns != null)
        {
            foreach (var colGroup in winAnimationColumns)
            {
                if (colGroup != null && colGroup.winAnimParents != null)
                {
                    foreach (var parent in colGroup.winAnimParents)
                    {
                        if (parent != null)
                        {
                            SpineAnimController spine = parent.GetComponentInChildren<SpineAnimController>();
                            if (spine != null) spine.Stop();
                            parent.SetActive(false);
                        }
                    }
                }
            }
        }
    }'''
    code = code.replace(disable_all_old.group(0), new_disable_all)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

print('SlotView.cs patched with clean Spine-only coroutines!')
