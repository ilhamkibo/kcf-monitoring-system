@echo off
setlocal EnableDelayedExpansion

title KCF Monitoring Deployment

echo =====================================================
echo           KCF Monitoring Deployment
echo =====================================================
echo.

cd /d C:\toho\project\kcf\KcfMonitoringSystem

echo [%date% %time%] Checking repository...

git pull > git_pull.log

findstr /C:"Already up to date." git_pull.log >nul

if %errorlevel%==0 (
    echo.
    echo ============================================
    echo Tidak ada update dari Git.
    echo Deployment dibatalkan.
    echo ============================================
    del git_pull.log
    timeout /t 5 /nobreak >nul
    exit
)

type git_pull.log
del git_pull.log

echo.
echo ============================================
echo Update ditemukan.
echo Memulai deployment...
echo ============================================

echo.
echo [1/7] Stop WebApi...
sc stop KcfMonitoringWebApi >nul

echo [2/7] Stop Worker...
sc stop KcfMonitoringWorker >nul

echo Menunggu service berhenti...

:WAITWEB
sc query KcfMonitoringWebApi | find "STOPPED" >nul
if errorlevel 1 (
    timeout /t 1 /nobreak >nul
    goto WAITWEB
)

:WAITWORKER
sc query KcfMonitoringWorker | find "STOPPED" >nul
if errorlevel 1 (
    timeout /t 1 /nobreak >nul
    goto WAITWORKER
)

echo.
echo [3/7] Publish WebApi...

dotnet publish .\KcfMonitoringSystem.WebApi\ -c Release -o C:\toho\project\kcf\Release\webapi

if errorlevel 1 (
    echo.
    echo ============================================
    echo Publish WebApi GAGAL
    echo ============================================

    sc start KcfMonitoringWebApi
    sc start KcfMonitoringWorker

    timeout /t 5 /nobreak >nul
    exit
)

echo.
echo [4/7] Publish Worker...

dotnet publish .\KcfMonitoringSystem.Worker\ -c Release -o C:\toho\project\kcf\Release\worker

if errorlevel 1 (
    echo.
    echo ============================================
    echo Publish Worker GAGAL
    echo ============================================

    sc start KcfMonitoringWebApi
    sc start KcfMonitoringWorker

    timeout /t 5 /nobreak >nul
    exit
)

echo.
echo [5/7] Start WebApi...
sc start KcfMonitoringWebApi >nul

echo [6/7] Start Worker...
sc start KcfMonitoringWorker >nul

echo.
echo Menunggu service running...
timeout /t 3 /nobreak >nul

echo.
echo ============================================
echo DEPLOYMENT BERHASIL
echo Waktu : %date% %time%
echo ============================================

timeout /t 5 /nobreak >nul
exit