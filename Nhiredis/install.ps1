param($installPath, $toolsPath, $package, $project)

$platformNames = "x86", "x64"
$propertyName = "CopyToOutputDirectory"

foreach($platformName in $platformNames) {
  $folder = $project.ProjectItems.Item($platformName)

  if ($folder -eq $null) {
    continue
  }

  $fileName = "hiredisx.dll"
  $item = $folder.ProjectItems.Item($fileName)
  if ($item -eq $null) {
    continue
  }
  $property = $item.Properties.Item($propertyName)
  if ($property -eq $null) {
    continue
  }
  $property.Value = 1
  
  $fileName = "libhiredisx.so"
  $item = $folder.ProjectItems.Item($fileName)
  if ($item -eq $null) {
    continue
  }
  $property = $item.Properties.Item($propertyName)
  if ($property -eq $null) {
    continue
  }
  $property.Value = 1
}
