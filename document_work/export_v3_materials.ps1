$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$output = Join-Path $root "Готовые_материалы\Летняя_практика\v3"
$renderRoot = Join-Path $PSScriptRoot "rendered_presentations_v3"
New-Item -ItemType Directory -Path $renderRoot -Force | Out-Null

$wordFiles = @(
    (Join-Path $output "Ефименко\Отчет_по_летней_практике_ЕфименкоНП_v3.docx"),
    (Join-Path $output "Локтев\Отчет_по_летней_практике_ЛоктевМС_v3.docx")
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

$presentations = @(
    @{ Person = "Ефименко"; File = "Презентация_ЕфименкоНП_v3.pptx" },
    @{ Person = "Локтев"; File = "Презентация_ЛоктевМС_v3.pptx" }
)

$powerPoint = New-Object -ComObject PowerPoint.Application
try {
    foreach ($item in $presentations) {
        $pptxPath = Join-Path $output "$($item.Person)\$($item.File)"
        $personRender = Join-Path $renderRoot $item.Person
        New-Item -ItemType Directory -Path $personRender -Force | Out-Null
        $presentation = $powerPoint.Presentations.Open($pptxPath, $true, $false, $false)
        try {
            $pdfPath = [System.IO.Path]::ChangeExtension($pptxPath, ".pdf")
            $presentation.SaveAs($pdfPath, 32)
            foreach ($slide in $presentation.Slides) {
                $slidePath = Join-Path $personRender ("slide-{0:D2}.png" -f $slide.SlideIndex)
                $slide.Export($slidePath, "PNG", 1440, 810)
            }
            Write-Output "$(Split-Path $pptxPath -Leaf): $($presentation.Slides.Count) slides"
        }
        finally {
            $presentation.Close()
        }
    }
}
finally {
    $powerPoint.Quit()
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($powerPoint) | Out-Null
}
