$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.Windows.Forms

$signature = @"
using System;
using System.Runtime.InteropServices;
using System.Text;

public static class NativeWin32
{
    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
"@
Add-Type $signature

function Capture-Window {
    param(
        [IntPtr]$Handle,
        [string]$Path
    )

    $rect = New-Object NativeWin32+RECT
    [NativeWin32]::GetWindowRect($Handle, [ref]$rect) | Out-Null
    $width = $rect.Right - $rect.Left
    $height = $rect.Bottom - $rect.Top

    if ($width -le 0 -or $height -le 0) {
        throw "Invalid window size for screenshot."
    }

    $bitmap = New-Object System.Drawing.Bitmap($width, $height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.CopyFromScreen($rect.Left, $rect.Top, 0, 0, (New-Object System.Drawing.Size($width, $height)))
    $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    $graphics.Dispose()
    $bitmap.Dispose()
}

function Get-ChildByText {
    param(
        [IntPtr]$Parent,
        [string]$Text
    )

    $script:FoundChild = [IntPtr]::Zero
    $callback = [NativeWin32+EnumWindowsProc]{
        param([IntPtr]$hWnd, [IntPtr]$lParam)
        $builder = New-Object System.Text.StringBuilder 256
        [NativeWin32]::GetWindowText($hWnd, $builder, $builder.Capacity) | Out-Null
        if ($builder.ToString() -eq $Text) {
            $script:FoundChild = $hWnd
            return $false
        }

        return $true
    }

    [NativeWin32]::EnumChildWindows($Parent, $callback, [IntPtr]::Zero) | Out-Null
    return $script:FoundChild
}

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$screenshots = Join-Path $root "Screenshots"
$appPath = Join-Path $root "RestaurantOrderKitchenTrackingSystem\bin\Debug\RestaurantOrderKitchenTrackingSystem.exe"

New-Item -ItemType Directory -Force -Path $screenshots | Out-Null

Get-Process | Where-Object { $_.ProcessName -eq "RestaurantOrderKitchenTrackingSystem" } | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Process -FilePath $appPath
Start-Sleep -Seconds 2

$process = Get-Process | Where-Object { $_.ProcessName -eq "RestaurantOrderKitchenTrackingSystem" } | Select-Object -First 1
if ($null -eq $process) {
    throw "Application process was not found."
}

Capture-Window -Handle $process.MainWindowHandle -Path (Join-Path $screenshots "login-screen.png")

$loginButton = Get-ChildByText -Parent $process.MainWindowHandle -Text "Log In"
if ($loginButton -eq [IntPtr]::Zero) {
    throw "Log In button was not found."
}

[NativeWin32]::SendMessage($loginButton, 0x00F5, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
Start-Sleep -Seconds 4

$process.Refresh()
Capture-Window -Handle $process.MainWindowHandle -Path (Join-Path $screenshots "main-form.png")

Write-Host "Screenshots captured in $screenshots"
