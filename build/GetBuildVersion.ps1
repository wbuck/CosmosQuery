function Get-BuildVersion {
    param (
        [string]$VersionString
    )

    $VersionString -match "(?<major>\d+)(\.(?<minor>\d+))?(\.(?<patch>\d+))?(\-(?<pre>[0-9A-Za-z\-\.]+))?(\+(?<build>\d+))?" | Out-Null

    if ($matches -eq $null) {
        return "1.0.0-build"
    }

    $buildRevision = [uint64]$matches['build']
    $preReleaseTag = [string]$matches['pre']
    $patch = [uint64]$matches['patch']
    $minor = [uint64]$matches['minor']
    $major = [uint64]$matches['major']

    $version = [string]$major + '.' + [string]$minor + '.' + [string]$patch

    if ($preReleaseTag -ne [string]::Empty) {
        $version += '-' + $preReleaseTag
    }

    if ($buildRevision -ne 0) {
        $version += '.' + [string]$buildRevision
    }

    return $version
}