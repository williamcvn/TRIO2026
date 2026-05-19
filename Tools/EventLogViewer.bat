@echo off
title TRIO2026 EventLog Viewer
dotnet run --project "%~dp0QueryEvents" -- %*
pause
