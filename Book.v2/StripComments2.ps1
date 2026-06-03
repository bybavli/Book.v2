Get-ChildItem -Path "c:\Users\Bavli\source\repos\Book.v2\Book.v2" -Include *.cs,*.js,*.css,*.html -Recurse | ForEach-Object {
    $content = [System.IO.File]::ReadAllText($_.FullName)
    
    if ($_.Extension -eq ".html") {
        # Keep Mermaid and other critical things, ONLY remove standard comments
        # Warning: blindly removing HTML comments might remove the mermaid blocks or scripts.
        # Actually, let's just do C# and JS comments to be safe since they are 99% of comments.
    }
    elseif ($_.Extension -eq ".css") {
        $content = $content -replace '(?s)/\*.*?\*/', ''
    }
    elseif ($_.Extension -eq ".cs" -or $_.Extension -eq ".js") {
        $content = $content -replace '(?s)/\*.*?\*/', ''
        $content = $content -replace '(?m)^\s*///.*$', ''
        $content = $content -replace '(?m)^\s*//.*$', ''
    }
    
    # Remove empty lines with just whitespace left behind
    $content = $content -replace '(?m)^\s*`r?`n', ''
    
    [System.IO.File]::WriteAllText($_.FullName, $content)
}
