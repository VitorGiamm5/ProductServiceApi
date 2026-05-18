@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0edge-prod.ps1" %*
