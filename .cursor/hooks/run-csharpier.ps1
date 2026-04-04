$input_json = $input | Out-String
$data = $input_json | ConvertFrom-Json

$file = $data.file_path
if ($file -match '\.cs$') {
    dotnet csharpier $file 2>&1 | Out-Null
}
