@echo off
:: Variables
set directory="C:\Program Files (x86)\SteamNew\steamapps\common\RimWorld\Mods\LetsGoExplore\Languages\
set source=English"
set catalan=Catalan"
set chineseSimplified=ChineseSimplified"
set chineseTraditional=ChineseTraditional"
set czech=Czech"
set danish=Danish"
set estonian=Estonian"
set finnish=Finnish"
set french=French"
set german=German"
set hungarian=Hungarian"
set italian=Italian"
set japanese=Japanese"
set korean=Korean"
set norwegian=Norwegian"
set polish=Polish"
set portuguese=Portuguese"
set portugueseBrazilian=PortugueseBrazilian"
set romanian=Romanian"
set russian=Russian"
set slovak=Slovak"
set spanish=Spanish"
set spanishLatin=SpanishLatin"
set swedish=Swedish"
set turkish=Turkish"
set ukranian=Ukranian"
set xcopy=xcopy /S/Q/Y/I
%xcopy% %directory%%source% %directory%%catalan%
echo English files copied to Catalan
%xcopy% %directory%%source% %directory%%chineseSimplified%
echo English files copied to ChineseSimplified
%xcopy% %directory%%source% %directory%%chineseTraditional%
echo English files copied to ChineseTraditional
%xcopy% %directory%%source% %directory%%czech%
echo English files copied to Czech
%xcopy% %directory%%source% %directory%%danish%
echo English files copied to Danish
%xcopy% %directory%%source% %directory%%estonian%
echo English files copied to Estonian
%xcopy% %directory%%source% %directory%%finnish%
echo English files copied to Finnish
%xcopy% %directory%%source% %directory%%french%
echo English files copied to French
%xcopy% %directory%%source% %directory%%german%
echo English files copied to German
%xcopy% %directory%%source% %directory%%hungarian%
echo English files copied to Hungarian
%xcopy% %directory%%source% %directory%%italian%
echo English files copied to Italian
%xcopy% %directory%%source% %directory%%japanese%
echo English files copied to Japanese
%xcopy% %directory%%source% %directory%%korean%
echo English files copied to Korean
%xcopy% %directory%%source% %directory%%norwegian%
echo English files copied to Norwegian
%xcopy% %directory%%source% %directory%%polish%
echo English files copied to Polish
%xcopy% %directory%%source% %directory%%portuguese%
echo English files copied to Portuguese
%xcopy% %directory%%source% %directory%%portugueseBrazilian%
echo English files copied to PortugueseBrazilian
%xcopy% %directory%%source% %directory%%romanian%
echo English files copied to Romanian
%xcopy% %directory%%source% %directory%%russian%
echo English files copied to Russian
%xcopy% %directory%%source% %directory%%slovak%
echo English files copied to Slovak
%xcopy% %directory%%source% %directory%%spanish%
echo English files copied to Spanish
%xcopy% %directory%%source% %directory%%spanishLatin%
echo English files copied to SpanishLatin
%xcopy% %directory%%source% %directory%%swedish%
echo English files copied to Swedish
%xcopy% %directory%%source% %directory%%turkish%
echo English files copied to Turkish
%xcopy% %directory%%source% %directory%%ukranian%
echo English files copied to Ukranian
echo Translations Complete
pause