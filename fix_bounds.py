import os
import re

file_path = r'C:\Unity\SlotBase-HotChili\Assets\Scripts\Managers\SlotView.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

code = code.replace('reel.images.Count < 14', 'reel.images.Count < 9')

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

print('Bounds fixed!')
