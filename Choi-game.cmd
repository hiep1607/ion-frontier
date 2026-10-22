@echo off
if not exist "%~dp0Builds\Combat-Clarity\IonFrontier.exe" goto missing
start "ION FRONTIER" /D "%~dp0Builds\Combat-Clarity" "%~dp0Builds\Combat-Clarity\IonFrontier.exe"
exit /b
:missing
if not exist "%~dp0Builds\Windows\IonFrontier.exe" goto nobuild
start "ION FRONTIER" /D "%~dp0Builds\Windows" "%~dp0Builds\Windows\IonFrontier.exe"
exit /b
:nobuild
echo Chua co build local. Mo project bang Unity, chon Ion Frontier / Build Windows.
echo Ban build mac dinh nam tai Builds\Windows\IonFrontier.exe.
pause
