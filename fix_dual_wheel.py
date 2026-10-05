import os
import re

file_path = r'C:\Unity\SlotBase-HotChili\Assets\Scripts\Managers\SlotView.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

pattern = r'internal void AnimateDualWheelWin\(System\.Action onComplete = null\)\s*\{.*?}\s*}'
new_code = '''internal void AnimateDualWheelWin(System.Action onComplete = null)
    {
        onComplete?.Invoke();
    }'''

code = re.sub(pattern, new_code, code, flags=re.DOTALL)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

print('AnimateDualWheelWin neutralized!')
