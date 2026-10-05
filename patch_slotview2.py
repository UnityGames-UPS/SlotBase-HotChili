import os

file_path = r'C:\Unity\SlotBase-HotChili\Assets\Scripts\Managers\SlotView.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

import re

# Find the BuildSymbolSpriteArray function and replace everything inside it down to defaultSprite
code = re.sub(
    r'private void BuildSymbolSpriteArray\(\)\s*\{.*?Sprite defaultSprite = null;',
    '''private void BuildSymbolSpriteArray()
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
        symbolSprites[8] = spriteBlueSeven;

        Sprite defaultSprite = null;''',
    code,
    flags=re.DOTALL
)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

print('BuildSymbolSpriteArray patched successfully!')
