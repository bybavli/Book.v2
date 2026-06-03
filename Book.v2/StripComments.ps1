Get-ChildItem -Path 'c:\Users\Bavli\source\repos\Book.v2\Book.v2' -Include *.cs,*.js,*.css,*.html -Recurse | ForEach-Object {
     = [System.IO.File]::ReadAllText($_.FullName)
    
    if ($_.Extension -eq '.html') {
        # HTML Comments
         = $content -replace '(?s)<!--.*?-->', ''
    }
    elseif ($_.Extension -eq '.css') {
        # CSS Block Comments
         = $content -replace '(?s)/\*.*?\*/', ''
    }
    elseif ($_.Extension -eq '.cs' -or $_.Extension -eq '.js') {
        # JS and C# Comments
        # Block comments
         = $content -replace '(?s)/\*.*?\*/', ''
        # XML Comments
         = $content -replace '(?m)^\s*///.*$', ''
        # Single line comments (only those at the start of a line to avoid URLs in strings)
         = $content -replace '(?m)^\s*//.*$', ''
    }
    
    # Remove empty lines that might have been left behind
     = $content -replace '(?m)^\s*?
', ''
    
    [System.IO.File]::WriteAllText($_.FullName, $content)
}
