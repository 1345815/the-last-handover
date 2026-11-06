param(
  [string]$Action,          # tap | hold | shot | focus
  [string]$Keys = '',       # for tap: sequence like "w,s,e" ; for hold: e.g. "shift+w"
  [int]$HoldMs = 1500,
  [string]$Out = 'Y:\Codex\GameOff2026\Docs\qa\shot.png'
)

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class K {
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int cmd);
  [DllImport("user32.dll")] public static extern void SwitchToThisWindow(IntPtr h, bool altTab);
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
  [DllImport("user32.dll")] public static extern void mouse_event(uint flags, int dx, int dy, uint data, UIntPtr extra);
  [DllImport("user32.dll")] public static extern short GetAsyncKeyState(int vk);
  public const uint DOWN = 0x0000, UP = 0x0002;
  public static void TapKey(byte vk) { keybd_event(vk, 0, DOWN, UIntPtr.Zero); System.Threading.Thread.Sleep(60); keybd_event(vk, 0, UP, UIntPtr.Zero); }
}
"@

function Focus-Game {
  $p = Get-Process TheLastHandover -ErrorAction SilentlyContinue
  if ($p -and $p.MainWindowHandle -ne 0) {
    for ($i = 0; $i -lt 6; $i++) {
      [K]::ShowWindow($p.MainWindowHandle, 3) | Out-Null
      [K]::SwitchToThisWindow($p.MainWindowHandle, $true)
      Start-Sleep -Milliseconds 300
      [K]::SetForegroundWindow($p.MainWindowHandle) | Out-Null
      Start-Sleep -Milliseconds 200
      if ([K]::GetForegroundWindow() -eq $p.MainWindowHandle) { Write-Output "FOCUS_OK"; return }
    }
    Write-Output "FOCUS_FAIL"
  } else { Write-Output "NO_GAME" }
}

switch ($Action) {
  'focus' { Focus-Game }
  'look'   {
    Focus-Game
    [K]::mouse_event(0x0001, [int]$Keys, 0, 0, [UIntPtr]::Zero)
  }
  'tap'   {
    Focus-Game
    foreach ($k in $Keys.Split(',')) {
      $vk = switch ($k.Trim().ToLower()) {
        'w' {[byte]0x57} 's' {[byte]0x53} 'e' {[byte]0x45}
        'shift' {[byte]0x10} 'up' {[byte]0x26} 'down' {[byte]0x28}
        default {[byte]0}
      }
      if ($vk -ne 0) { [K]::TapKey($vk); Start-Sleep -Milliseconds 250 }
    }
  }
  'hold'  {
    # hold: "shift" alone, or "shift+w" (hold shift, tap w while held)
    Focus-Game
    $parts = $Keys.Split('+')
    $mods = $parts | Where-Object { $_ -ne $parts[-1] }
    $last = $parts[-1]
    foreach ($m in $mods) {
      $vk = switch ($m.Trim().ToLower()) { 'shift' {[byte]0x10} default {[byte]0} }
      [K]::keybd_event($vk, 0, [K]::DOWN, [UIntPtr]::Zero)
    }
    Start-Sleep -Milliseconds $HoldMs
    if ($parts.Count -eq 1 -and $last -eq 'shift') {
      # 单独 hold shift：先按下再等再松开
      [K]::keybd_event(0x10, 0, [K]::DOWN, [UIntPtr]::Zero)
      Start-Sleep -Milliseconds $HoldMs
      [K]::keybd_event(0x10, 0, [K]::UP, [UIntPtr]::Zero)
    } elseif ($last -ne '' -and $last -ne 'shift') {
      $vk = switch ($last.Trim().ToLower()) { 'w' {[byte]0x57} 's' {[byte]0x53} 'e' {[byte]0x45} default {[byte]0} }
      if ($vk -ne 0) { [K]::TapKey($vk) }
    }
    foreach ($m in $mods) {
      $vk = switch ($m.Trim().ToLower()) { 'shift' {[byte]0x10} default {[byte]0} }
      [K]::keybd_event($vk, 0, [K]::UP, [UIntPtr]::Zero)
    }
  }
  'shot'  {
    $b = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bmp = New-Object System.Drawing.Bitmap $b.Width, $b.Height
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
    $bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
    Write-Output "saved $Out"
  }
}
