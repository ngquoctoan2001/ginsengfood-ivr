[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

if (-not ('MicroSipLabMainWindow' -as [type])) {
    Add-Type @'
using System;
using System.Text;
using System.Runtime.InteropServices;

public static class MicroSipLabMainWindow
{
    public delegate bool EnumProc(IntPtr handle, IntPtr state);

    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumProc callback, IntPtr state);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr handle, out uint processId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetClassName(IntPtr handle, StringBuilder text, int capacity);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr handle, int command);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr handle);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr handle);
}
'@
}

$process = Get-Process -Name 'MicroSIP' -ErrorAction Stop | Select-Object -First 1
$script:mainWindow = [IntPtr]::Zero
$callback = [MicroSipLabMainWindow+EnumProc]{
    param($handle, $state)

    [uint32]$ownerProcessId = 0
    [void][MicroSipLabMainWindow]::GetWindowThreadProcessId(
        $handle,
        [ref]$ownerProcessId)
    if ($ownerProcessId -ne [uint32]$process.Id) {
        return $true
    }

    $className = New-Object System.Text.StringBuilder 64
    [void][MicroSipLabMainWindow]::GetClassName($handle, $className, $className.Capacity)
    if ($className.ToString() -eq 'MicroSIP') {
        $script:mainWindow = $handle
        return $false
    }

    return $true
}
[void][MicroSipLabMainWindow]::EnumWindows($callback, [IntPtr]::Zero)

if ($script:mainWindow -eq [IntPtr]::Zero) {
    throw 'The MicroSIP main window was not found.'
}

# SW_RESTORE also makes a tray-hidden MicroSIP window visible.
[void][MicroSipLabMainWindow]::ShowWindow($script:mainWindow, 9)
Start-Sleep -Milliseconds 300
[void][MicroSipLabMainWindow]::SetForegroundWindow($script:mainWindow)

if (-not [MicroSipLabMainWindow]::IsWindowVisible($script:mainWindow)) {
    throw 'The MicroSIP main window could not be made visible.'
}

Write-Host 'MicroSIP LAB-A window is visible and ready for Answer.'
