@echo off
pushd %~dp0

set xslt=..\..\OrangeShare\Windows\tools\xslt\bin\release\xslt.exe
if not exist %xslt% call ..\..\OrangeShare\Windows\tools\xslt\build.cmd

for %%a in (*.xml.in) do (
  %xslt% parse_plugins.xsl "%%a" "%%~dpna"
)

popd
