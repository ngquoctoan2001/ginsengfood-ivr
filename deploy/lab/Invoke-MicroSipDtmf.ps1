[CmdletBinding()]
param(
    [ValidateSet('0', '1')]
    [string]$Digit = '1',
    [ValidateRange(5, 120)]
    [int]$TimeoutSeconds = 45
)

$ErrorActionPreference = 'Stop'

Add-Type @'
using System;
using System.Text;
using System.Runtime.InteropServices;

public static class MicroSipLabUi
{
    public delegate bool EnumProc(IntPtr handle, IntPtr state);

    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    public static extern bool EnumChildWindows(IntPtr parent, EnumProc callback, IntPtr state);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(IntPtr handle, StringBuilder text, int capacity);

    [DllImport("user32.dll")]
    public static extern bool IsWindowVisible(IntPtr handle);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr handle, out Rect rect);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr handle);

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    public static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);
}
'@

$process = Get-Process -Name 'MicroSIP' -ErrorAction Stop | Select-Object -First 1
$deadline = [DateTimeOffset]::UtcNow.AddSeconds($TimeoutSeconds)
$connected = $false

while ([DateTimeOffset]::UtcNow -lt $deadline) {
    $script:endButtonVisible = $false
    $callback = [MicroSipLabUi+EnumProc]{
        param($handle, $state)

        $text = New-Object System.Text.StringBuilder 16
        [void][MicroSipLabUi]::GetWindowText($handle, $text, $text.Capacity)
        if ($text.ToString() -eq 'End' -and [MicroSipLabUi]::IsWindowVisible($handle)) {
            $script:endButtonVisible = $true
            return $false
        }

        return $true
    }
    [void][MicroSipLabUi]::EnumChildWindows(
        $process.MainWindowHandle,
        $callback,
        [IntPtr]::Zero)

    if ($script:endButtonVisible) {
        $connected = $true
        break
    }

    Start-Sleep -Milliseconds 100
}

if (-not $connected) {
    throw 'MicroSIP did not expose its connected-call End button before the timeout.'
}

$script:digitButton = [IntPtr]::Zero
$digitCallback = [MicroSipLabUi+EnumProc]{
    param($handle, $state)

    $text = New-Object System.Text.StringBuilder 8
    [void][MicroSipLabUi]::GetWindowText($handle, $text, $text.Capacity)
    if ($text.ToString() -eq $Digit -and [MicroSipLabUi]::IsWindowVisible($handle)) {
        $script:digitButton = $handle
        return $false
    }

    return $true
}
[void][MicroSipLabUi]::EnumChildWindows(
    $process.MainWindowHandle,
    $digitCallback,
    [IntPtr]::Zero)

if ($script:digitButton -eq [IntPtr]::Zero) {
    throw "The visible MicroSIP DTMF $Digit button was not found."
}

$rect = New-Object MicroSipLabUi+Rect
[void][MicroSipLabUi]::GetWindowRect($script:digitButton, [ref]$rect)
$x = [int](($rect.Left + $rect.Right) / 2)
$y = [int](($rect.Top + $rect.Bottom) / 2)

[void][MicroSipLabUi]::SetForegroundWindow($process.MainWindowHandle)
Start-Sleep -Milliseconds 200
[void][MicroSipLabUi]::SendMessage(
    $script:digitButton,
    0x00F5,
    [IntPtr]::Zero,
    [IntPtr]::Zero)
# Keep the physical click as a fallback for MicroSIP builds whose owner-drawn
# keypad does not act on BM_CLICK. Both target the exact visible digit control.
[void][MicroSipLabUi]::SetCursorPos($x, $y)
[MicroSipLabUi]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero)
Start-Sleep -Milliseconds 100
[MicroSipLabUi]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero)

Write-Host "MicroSIP DTMF $Digit clicked after the call reached connected state."
