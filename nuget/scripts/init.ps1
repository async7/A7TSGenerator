param($installPath, $toolsPath, $package)
Import-Module (Join-Path $toolsPath A7TS-GenTs.psd1) -ArgumentList $installPath