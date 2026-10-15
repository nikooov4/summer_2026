$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$output = Join-Path $root "Готовые_материалы\Летняя_практика"
$renderRoot = Join-Path $PSScriptRoot "rendered_presentations\Презентация_ЕфименкоНП_v2"
New-Item -ItemType Directory -Path $renderRoot -Force | Out-Null

$wordFiles = @(
    (Join-Path $output "Отчет_по_летней_практике_ЕфименкоНП_v2.docx"),
    (Join-Path $output "Дневник_летней_практики_ЕфименкоНП_v2.docx")
)

$word = New-Object -ComObject Word.Application
$word.Visible = $false
$word.DisplayAlerts = 0
try {
    foreach ($path in $wordFiles) {
        $document = $word.Documents.Open($path, $false, $false)
        try {
            $document.Fields.Update() | Out-Null
            foreach ($contents in $document.TablesOfContents) {
                $contents.Update() | Out-Null
            }
            $document.Save()
            $pdfPath = [System.IO.Path]::ChangeExtension($path, ".pdf")
            $document.ExportAsFixedFormat($pdfPath, 17)
            Write-Output "$(Split-Path $path -Leaf): $($document.ComputeStatistics(2)) pages"
        }
        finally {
            $document.Close(0)
        }
    }
}
finally {
    $word.Quit()
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($word) | Out-Null
}

$pptxPath = Join-Path $output "Презентация_ЕфименкоНП_v2.pptx"
$powerPoint = New-Object -ComObject PowerPoint.Application
try {
    $presentation = $powerPoint.Presentations.Open($pptxPath, $true, $false, $false)
    try {
        $pdfPath = [System.IO.Path]::ChangeExtension($pptxPath, ".pdf")
        $presentation.SaveAs($pdfPath, 32)
        foreach ($slide in $presentation.Slides) {
            $slidePath = Join-Path $renderRoot ("slide-{0:D2}.png" -f $slide.SlideIndex)
            $slide.Export($slidePath, "PNG", 1440, 810)
        }
        Write-Output "$(Split-Path $pptxPath -Leaf): $($presentation.Slides.Count) slides"
    }
    finally {
        $presentation.Close()
    }
}
finally {
    $powerPoint.Quit()
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($powerPoint) | Out-Null
}
