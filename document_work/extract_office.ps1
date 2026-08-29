param(
    [Parameter(Mandatory = $true)]
    [string]$InputPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$extension = [System.IO.Path]::GetExtension($InputPath).ToLowerInvariant()
$lines = [System.Collections.Generic.List[string]]::new()

if ($extension -eq '.pptx') {
    $powerPoint = New-Object -ComObject PowerPoint.Application
    try {
        $presentation = $powerPoint.Presentations.Open($InputPath, $true, $true, $false)
        $lines.Add("SLIDES=$($presentation.Slides.Count)")
        foreach ($slide in $presentation.Slides) {
            $lines.Add("--- SLIDE $($slide.SlideIndex) ---")
            foreach ($shape in $slide.Shapes) {
                if ($shape.HasTextFrame -eq -1 -and $shape.TextFrame.HasText -eq -1) {
                    $value = ($shape.TextFrame.TextRange.Text -replace "`r", ' | ' -replace "`n", ' | ').Trim()
                    if ($value) {
                        $lines.Add("$($shape.Name): $value")
                    }
                }
            }
        }
        $presentation.Close()
    }
    finally {
        $powerPoint.Quit()
    }
}
else {
    $word = New-Object -ComObject Word.Application
    $word.Visible = $false
    $word.DisplayAlerts = 0
    try {
        $document = $word.Documents.Open($InputPath, $false, $true)
        $lines.Add("PAGES=$($document.ComputeStatistics(2)) PARAGRAPHS=$($document.Paragraphs.Count) TABLES=$($document.Tables.Count)")
        $index = 0
        foreach ($paragraph in $document.Paragraphs) {
            $index++
            $value = $paragraph.Range.Text.Replace([char]13, ' ').Replace([char]7, ' ').Trim()
            if ($value) {
                try {
                    $style = $paragraph.Range.Style.NameLocal
                }
                catch {
                    $style = ''
                }
                $lines.Add("[$index][$style] $value")
            }
        }
        $document.Close(0)
    }
    finally {
        $word.Quit()
    }
}

[System.IO.File]::WriteAllLines($OutputPath, $lines, [System.Text.UTF8Encoding]::new($false))
