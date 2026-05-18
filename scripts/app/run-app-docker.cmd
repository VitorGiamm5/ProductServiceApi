@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run-app-docker.ps1" %*
