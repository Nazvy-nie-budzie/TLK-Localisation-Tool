# TLK Localisation Tool
Tool that helps with SW KOTOR localisation.

## Features
- View and edit .tlk files.
- Get context for any particular .tlk entry by viewing files, in which it's used.
- Export .tlk entries to JSON.
- Check localised text for spelling errors with spellcheck.

## Prerequisites
1. [Download](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) and install .NET 8 runtime for Windows.
2. If you need 'Context' feture, extract game files from SW KOTOR .bif and .rim files to one folder. You can use [KOTOR Tool](https://deadlystream.com/files/file/280-kotor-tool/) for that.
3. Make a copy of dialog.tlk file, which will contain localised text.

## Spellcheck
There are two ways to use spellchecker in TLK Localisation Tool:
1. If there is a Windows language pack (or language interface pack with spellchecker) for your language, then make sure is's installed on your machine, then open the program, go to 'File' -> 'Settings', and set your language code there (list of codes for Windows language and language interface packs can be found [here](https://learn.microsoft.com/en-us/windows-hardware/manufacture/desktop/available-language-packs-for-windows?view=windows-11#language-packs)). After that spellcheck on 'Entry Editor' form should start working.
2. If there is no Windows language pack (or language interface pack with spellchecker) for your language, or you want spellchecker not to highlight some words, you can create custom dictionary. Information about custom dictionary format can be found [here](https://learn.microsoft.com/en-us/dotnet/api/system.windows.controls.spellcheck.customdictionaries?view=windowsdesktop-8.0#remarks). When dictionary is created, move it to the program's root folder, open the program, and perform the actions described above.

Additional notes regarding spellcheck and .lex files:
- Create .lex dictionaries with UTF-16 LE encoding.
- You can split dictionary into several files - program will load all of them.
- Keep in mind, that default max size of all custom dictionaries is 1mb (2 mbs with additional setup, described [here](https://learn.microsoft.com/en-us/windows/win32/api/spellcheck/nn-spellcheck-iuserdictionariesregistrar#remarks)). If combined size of your dictionaries is bigger, they will be ignored (either fully, if it's one file, or partially, if there are several of them).
- If you want to disable spellcheck, set language code to blank.

## Acknowledgments
Special thanks to:
- Developer of KOTOR Tool Fred Tetra (and [Darth_Sapiens](https://deadlystream.com/profile/9663-darth_sapiens/), who posted it on Deadly Stream).
- People from Deadly Stream forums.
- Developers of [Xoreos](https://github.com/xoreos/xoreos/tree/master).
- Maintainers of [KotOR Modding Wiki](https://kotor-modding.fandom.com/wiki/KotOR_Modding_Wiki).
