param(
  [string]$DocsPath = "docs"
)

# Lints markdown links in the docs folder to ensure they don't escape the repo root
# and they don't use absolute local paths. Exits with non-zero code on violations.

# Determine repo root relative to the script location; fall back to current location
try {
  $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
} catch {
  $repoRoot = (Get-Location).Path
}
# Normalize for comparisons
$repoRoot = [System.IO.Path]::GetFullPath($repoRoot)
$errors = @()

# Gather markdown files
$mdFiles = Get-ChildItem -Path $DocsPath -Filter *.md -Recurse -ErrorAction Stop

foreach ($file in $mdFiles) {
  $lines = Get-Content -LiteralPath $file.FullName
  for ($i = 0; $i -lt $lines.Count; $i++) {
    $line = $lines[$i]
    $match = [regex]::Match($line, "\[.*?\]\((.*?)\)")
    while ($match.Success) {
      $target = $match.Groups[1].Value

      # Allow external links
      if ([regex]::IsMatch($target, "^(https?://|mailto:)")) { $match = $match.NextMatch(); continue }

      # Disallow absolute local paths and root-anchored paths
      if ([regex]::IsMatch($target, "^[A-Za-z]:\\")) {
        $errors += @{ File = $file.FullName; Line = $i + 1; Issue = "Absolute local path"; Target = $target }
        $match = $match.NextMatch(); continue
      }
      if ([regex]::IsMatch($target, "^/")) {
        $errors += @{ File = $file.FullName; Line = $i + 1; Issue = "Root-anchored path"; Target = $target }
        $match = $match.NextMatch(); continue
      }
      if ([regex]::IsMatch($target, "^(file|vscode)://")) {
        $errors += @{ File = $file.FullName; Line = $i + 1; Issue = "Unsupported URI scheme"; Target = $target }
        $match = $match.NextMatch(); continue
      }

      # Root-escape detection handled via absolute path resolution below

      # Strip fragment and query for existence check
      $pathOnly = $target.Split('#')[0].Split('?')[0]
      # Normalize slashes for Windows filesystem check
      $pathFs = $pathOnly -replace "/", "\\"

      try {
        $abs = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($file.DirectoryName, $pathFs))
      } catch {
        $abs = $null
      }

      if ($abs -and $abs.ToLower().StartsWith($repoRoot.ToLower())) {
        # Check existence for local relative links only
        if (-not (Test-Path -LiteralPath $abs)) {
          $errors += @{ File = $file.FullName; Line = $i + 1; Issue = "Missing target"; Target = $target }
        }
      } elseif ($abs) {
        $errors += @{ File = $file.FullName; Line = $i + 1; Issue = "Resolved outside repo"; Target = $target }
      }
      $match = $match.NextMatch()
    }
  }
}

if ($errors.Count -gt 0) {
  Write-Host "Docs link check found $($errors.Count) issue(s):" -ForegroundColor Red
  foreach ($e in $errors) {
    Write-Host " - $($e.File):$($e.Line) -> $($e.Issue): $($e.Target)"
  }
  exit 1
} else {
  Write-Host "Docs link check passed: no invalid links." -ForegroundColor Green
}
