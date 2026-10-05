import os

file_path = r'C:\Unity\SlotBase-HotChili\Assets\Scripts\Managers\SlotView.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    code = f.read()

old_fields = '''    [Header("Symbol Sprites - Assign by Name")]
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

new_fields = '''    [Header("Symbol Sprites - Matches Backend JSON")]
    [SerializeField] private Sprite spriteGreenChilli; // ID 0
    [SerializeField] private Sprite spriteYellowChilli; // ID 1
    [SerializeField] private Sprite spriteOrangeChilli; // ID 2
    [SerializeField] private Sprite spriteRedChilli; // ID 3
    [SerializeField] private Sprite spriteTripleBar; // ID 4
    [SerializeField] private Sprite spriteDoubleBar; // ID 5
    [SerializeField] private Sprite spriteSingleBar; // ID 6
    [SerializeField] private Sprite spriteRedSeven; // ID 7
    [SerializeField] private Sprite spriteBlueSeven; // ID 8'''

code = code.replace(old_fields, new_fields)

old_build = '''    private void BuildSymbolSpriteArray()
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

new_build = '''    private void BuildSymbolSpriteArray()
    {
        symbolSprites = new Sprite[10]; // IDs 0 through 8
        symbolSprites[0] = spriteGreenChilli;
        symbolSprites[1] = spriteYellowChilli;
        symbolSprites[2] = spriteOrangeChilli;
        symbolSprites[3] = spriteRedChilli;
        symbolSprites[4] = spriteTripleBar;
        symbolSprites[5] = spriteDoubleBar;
        symbolSprites[6] = spriteSingleBar;
        symbolSprites[7] = spriteRedSeven;
        symbolSprites[8] = spriteBlueSeven;'''

code = code.replace(old_build, new_build)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(code)

print('SlotView.cs patched successfully!')
