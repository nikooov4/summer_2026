$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$output = Join-Path $root "Готовые_материалы"
$word = New-Object -ComObject Word.Application
$word.Visible = $false
$word.DisplayAlerts = 0

try {
    $files = @()
    $files += Get-ChildItem -LiteralPath $output -Filter "Отчет_по_летней_практике_*.docx"
    $files += Get-ChildItem -LiteralPath $output -Filter "Дневник_летней_практики_*.docx"

    foreach ($file in $files) {
        $document = $word.Documents.Open($file.FullName)
        try {
            foreach ($toc in $document.TablesOfContents) {
                $toc.Update()
            }
            $document.Fields.Update() | Out-Null
            $document.Save()
            $pdf = [System.IO.Path]::ChangeExtension($file.FullName, ".pdf")
            $document.ExportAsFixedFormat($pdf, 17)
            $pages = $document.ComputeStatistics(2)
            Write-Output "$($file.Name): $pages pages"
        }
        finally {
            $document.Close($false)
        }
    }
}
finally {
    $word.Quit()
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($word) | Out-Null
}
