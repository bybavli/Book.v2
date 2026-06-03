Get-ChildItem -Path "c:\Users\Bavli\source\repos\Book.v2\Book.v2" -Include *.cs,*.js,*.css,*.html -Recurse | ForEach-Object {
    $content = [System.IO.File]::ReadAllText($_.FullName)
    
    if ($_.Extension -eq ".html") {
        $content = [System.Text.RegularExpressions.Regex]::Replace($content, '(?s)<!--.*?-->', '')
    }
    elseif ($_.Extension -eq ".css") {
        $content = [System.Text.RegularExpressions.Regex]::Replace($content, '(?s)/\*.*?\*/', '')
    }
    elseif ($_.Extension -eq ".cs" -or $_.Extension -eq ".js") {
        $content = [System.Text.RegularExpressions.Regex]::Replace($content, '(?s)/\*.*?\*/', '')
        $content = [System.Text.RegularExpressions.Regex]::Replace($content, '(?m)^\s*///.*$', '')
        $content = [System.Text.RegularExpressions.Regex]::Replace($content, '(?m)^\s*//.*$', '')
    }
    
    [System.IO.File]::WriteAllText($_.FullName, $content)
}
