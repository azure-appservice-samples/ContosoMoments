function Expand-ZIPFile($file, $destination)
{
    $shell = new-object -com shell.application
    $zip = $shell.NameSpace($file)
    foreach($item in $zip.items())
    {
        $shell.Namespace($destination).copyhere($item)
    }
}

IF(-NOT (Test-Path "C:\Xamarin")){
  New-Item -ItemType Directory -Path C:\ -Name Xamarin
}

Push-Location C:\Xamarin

IF(-NOT (Test-Path "C:\Xamarin\xamarin-component.exe")) {
  Invoke-WebRequest -Uri https://components.xamarin.com/submit/xpkg -OutFile xpkg.zip -UseBasicParsing
  Expand-ZIPFile -file C:\Xamarin\xpkg.zip -destination C:\Xamarin
}

Pop-Location