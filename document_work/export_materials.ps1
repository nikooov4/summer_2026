param(
    [string]$OutputDirectory = "$PSScriptRoot\..\Готовые_материалы"
)

$output = [System.IO.Path]::GetFullPath($OutputDirectory)
$renderRoot = Join-Path $PSScriptRoot 'rendered_presentations'
New-Item -ItemType Directory -Path $renderRoot -Force | Out-Null

$word = New-Object -ComObject Word.Application
$word.Visible = $false
$word.DisplayAlerts = 0
try {
    foreach ($path in Get-ChildItem -LiteralPath $output -Filter '*.docx') {
        $document = $word.Documents.Open($path.FullName, $false, $false)
        try {
            foreach ($field in $document.Fields) {
                $field.Update() | Out-Null
            }
            foreach ($contents in $document.TablesOfContents) {
                $contents.Update() | Out-Null
            }
            $document.Save()
            $pdfPath = [System.IO.Path]::ChangeExtension($path.FullName, '.pdf')
            $document.ExportAsFixedFormat($pdfPath, 17)
            "$($path.Name): $($document.ComputeStatistics(2)) pages"
        }
        finally {
            $document.Close(0)
        }
    }
}
finally {
    $word.Quit()
}

$powerPoint = New-Object -ComObject PowerPoint.Application
try {
    foreach ($path in Get-ChildItem -LiteralPath $output -Filter '*.pptx') {
        $presentation = $powerPoint.Presentations.Open($path.FullName, $true, $false, $false)
        try {
            $pdfPath = [System.IO.Path]::ChangeExtension($path.FullName, '.pdf')
            $presentation.SaveAs($pdfPath, 32)

            $renderDirectory = Join-Path $renderRoot $path.BaseName
            New-Item -ItemType Directory -Path $renderDirectory -Force | Out-Null
            foreach ($slide in $presentation.Slides) {
                $slidePath = Join-Path $renderDirectory ("slide-{0:D2}.png" -f $slide.SlideIndex)
                $slide.Export($slidePath, 'PNG', 1440, 810)
            }
            "$($path.Name): $($presentation.Slides.Count) slides"
        }
        finally {
            $presentation.Close()
        }
    }
}
finally {
    $powerPoint.Quit()
}
