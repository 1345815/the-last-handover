Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class W {
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int cmd);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern void SwitchToThisWindow(IntPtr h, bool altTab);
}
"@
$p = Get-Process TheLastHandover -ErrorAction Stop
$h = $p.MainWindowHandle
[W]::ShowWindow($h, 3) | Out-Null
[W]::SwitchToThisWindow($h, $true)
Start-Sleep -Milliseconds 600
[W]::SetForegroundWindow($h) | Out-Null
Start-Sleep -Milliseconds 400
$fg = [System.Runtime.InteropServices.Marshal]::ReadIntPtr([IntPtr]::Zero) # placeholder
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class W2 { [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow(); }
"@
Write-Output ("foreground=" + ([W2]::GetForegroundWindow() -eq $h) + " handle=$h")
