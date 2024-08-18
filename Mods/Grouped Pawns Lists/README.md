# PawnTableGrouped

You need to modify search assembly paths to make this project to compile (check dependency-*.csproj files)
I probably need to create a configuration tool to generate those dependency files, but this yet to happen.

if you don't have local copy of RWLayout, you need to add its id into dependencies list:
open `./Dependencies/PrepareDependendencies.ps1` and follow instructions


To prepare dependencies you need to start 
`./Dependencies/PrepareDependendencies.bat`
it will download mods from steamworkshop

To compile and deploy the mod start `./Deploy/Deploy.bat`.
Deployment script requites:
- PowerShell 5.0 or above
- properly intalled 7zip
