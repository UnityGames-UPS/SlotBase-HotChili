import os
import re

file_path = r'C:\Unity\SlotBase-HotChili\Assets\Scripts\Managers\SlotView.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

# Fix StartAnimation override
old_start = 'SpineAnimController spine = (imageAnim != null) ? imageAnim.GetComponent<SpineAnimController>() : null;'
new_start = 'SpineAnimController spine = (imageAnim != null) ? imageAnim.GetComponentInParent<SpineAnimController>() : null;'
code = code.replace(old_start, new_start)

# Fix StopAnimation override
old_stop = 'SpineAnimController spine = imageAnim.GetComponent<SpineAnimController>();'
new_stop = 'SpineAnimController spine = imageAnim.GetComponentInParent<SpineAnimController>();'
code = code.replace(old_stop, new_stop)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

print('Fixed Spine trigger in SlotView!')
