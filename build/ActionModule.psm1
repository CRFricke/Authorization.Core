#!/usr/bin/env pwsh

## Portions stollen from:
##    https://github.com/MADE-Apps/MADE.NET/blob/main/build/GetBuildVersion.psm1
##    https://github.com/Amadevus/pwsh-script/blob/master/lib/GitHubActionsCore/GitHubActionsCore.psm1
##
## who adapted from:
##    https://github.com/ebekker/pwsh-github-action-base/blob/b19583aaecd66696896e9b7dbc9f419e2fca458b/lib/ActionsCore.ps1
## 
## which in turn was adapted from:
##    https://github.com/actions/toolkit/blob/c65fe87e339d3dd203274c62d0f36f405d78e8a0/packages/core/src/core.ts

function Get-VersionVariables {
    [CmdletBinding()]
    Param (
      [Parameter(Position = 0, Mandatory)]
      [string]$VersionString
    )

    Write-Host "`$VersionString: '$VersionString'"

    if ($env:GITHUB_REF_TYPE -eq 'tag')
    {
        # Parse via regex
        $null = $VersionString -match "(?<major>\d+)(\.(?<minor>\d+))?(\.(?<patch>\d+))?(\-(?<pre>[0-9A-Za-z\-\.]+))?(\+(?<build>[0-9A-Za-z\-\.]+))?"
    }

    if (!$matches)
    {
        $Major = $env:VER_MAJOR_DEFAULT
        $Minor = $env:VER_MINOR_DEFAULT
        $Patch = $env:VER_PATCH_DEFAULT
        $PreRelease = "build"
    }
    else
    {
        $Major = [uint64]$matches['major']
        $Minor = [uint64]$matches['minor']
        $Patch = [uint64]$matches['patch']
        $PreRelease = [string]$matches['pre']
        $Build = [string]$matches['build']
    }

    if ($PreRelease -and !$Build)
    {
        $Build = [string]$env:GITHUB_RUN_NUMBER
    }

    if ($Major -eq $null)
    {
        throw "Error: `$Major variable could not be set. (Is VER_MAJOR_DEFAULT environment variable specified?)"
    }

    if ($Minor -eq $null)
    {
        throw "Error: `$Minor variable could not be set. (Is VER_MINOR_DEFAULT environment variable specified?)"
    }

    if ($Patch -eq $null)
    {
        throw "Error: `$Patch variable could not be set. (Is VER_PATCH_DEFAULT environment variable specified?)"
    }

    Enter-ActionOutputGroup "Dump Get-VersionVariables Output Variables"

    Write-Host "`$Tag_Major: $Major"
    $env:TAG_MAJOR = $Major
    $global:Tag_Major = $Major

    Write-Host "`$Tag_Minor: $Minor"
    $env:TAG_MINOR = $Minor
    $global:Tag_Minor = $Minor

    Write-Host "`$Tag_Patch: $Patch"
    $env:TAG_PATCH = $Patch
    $global:Tag_Patch = $Patch

    Write-Host "`$Tag_PreRelease: $PreRelease"
    $env:TAG_PRERELEASE = $PreRelease
    $global:Tag_PreRelease = $PreRelease

    Write-Host "`$Tag_Build: $Build"
    $env:TAG_BUILD = $Build
    $global:Tag_Build = $Build

    Exit-ActionOutputGroup
}

<#
.SYNOPSIS
Sets env variable for this action and future actions in the job.
Equivalent of `core.exportVariable(name, value)`.
.PARAMETER Name
The name of the variable to set.
.PARAMETER Value
The value of the variable. Non-string values will be converted to a string via ConvertTo-Json.
.PARAMETER SkipLocal
Do not set variable in current action's/step's environment.
.LINK
https://help.github.com/en/actions/reference/workflow-commands-for-github-actions#setting-an-environment-variable
.LINK
https://github.com/actions/toolkit/tree/master/packages/core#exporting-variables
#>
function Set-ActionVariable {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$Name,

        [Parameter(Position = 1, Mandatory)]
        [AllowNull()]
        [AllowEmptyString()]
        [AllowEmptyCollection()]
        [object]$Value,
        
        [switch]$SkipLocal
    )
    
    $convertedValue = ConvertTo-ActionCommandValue $Value

    ## To take effect in the current action/step
    if (-not $SkipLocal) {
        [System.Environment]::SetEnvironmentVariable($Name, $convertedValue)
    }

    ## To take effect for all subsequent actions/steps
    if ($env:GITHUB_ENV) {
        $delimiter = [guid]::NewGuid().toString()
        $eol = [System.Environment]::NewLine
        $commandValue = "$name<<${delimiter}${eol}${convertedValue}${eol}${delimiter}"
        Send-ActionFileCommand -Command ENV -Message $commandValue
    }
    else {
        Send-ActionCommand set-env @{ name = $Name } -Message $convertedValue
    }
}

<#
.SYNOPSIS
Begin an output group.
Output until the next `groupEnd` will be foldable in this group.
Equivalent of `core.startGroup(name)`.
.DESCRIPTION
Output until the next `groupEnd` will be foldable in this group.
.PARAMETER Name
The name of the output group.
.LINK
https://help.github.com/en/actions/reference/workflow-commands-for-github-actions#masking-a-value-in-log
.LINK
https://github.com/actions/toolkit/tree/master/packages/core#logging
 #>
function Enter-ActionOutputGroup {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$Name
    )

    Send-ActionCommand group $Name
}

<#
.SYNOPSIS
End an output group.
Equivalent of `core.endGroup()`.
.LINK
https://help.github.com/en/actions/reference/workflow-commands-for-github-actions#masking-a-value-in-log
.LINK
https://github.com/actions/toolkit/tree/master/packages/core#logging
 #>
function Exit-ActionOutputGroup {
    [CmdletBinding()]
    param()
    Send-ActionCommand endgroup
}

<#
.SYNOPSIS
Sets the value of an output.
Equivalent of `core.setOutput(name, value)`.
.PARAMETER Name
Name of the output to set.
.PARAMETER Value
Value to store. Non-string values will be converted to a string via ConvertTo-Json.
.LINK
https://help.github.com/en/actions/reference/workflow-commands-for-github-actions#setting-an-output-parameter
.LINK
https://github.com/actions/toolkit/tree/master/packages/core#inputsoutputs
#>
function Set-ActionOutput {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$Name,

        [Parameter(Position = 1, Mandatory)]
        [AllowNull()]
        [AllowEmptyString()]
        [AllowEmptyCollection()]
        [object]$Value
    )

    Send-ActionCommand set-output @{
        name = $Name
    } -Message (ConvertTo-ActionCommandValue $Value)
}

## Used to signal output that is a command to Action/Workflow context
if (-not (Get-Variable -Scope Script -Name CMD_STRING -ErrorAction SilentlyContinue)) {
    Set-Variable -Scope Script -Option Constant -Name CMD_STRING -Value '::'
}

<#
.SYNOPSIS
Sends a command to the hosting Workflow/Action context.
Equivalent to `core.issue(cmd, msg)`/`core.issueCommand(cmd, props, msg)`.
.DESCRIPTION
Command Format:
  ::workflow-command parameter1={data},parameter2={data}::{command value}
.PARAMETER Command
The workflow command name.
.PARAMETER Properties
Properties to add to the command.
.PARAMETER Message
Message to add to the command.
.EXAMPLE
PS> Send-ActionCommand warning 'This is the user warning message'
::warning::This is the user warning message
.EXAMPLE
PS> Send-ActionCommand set-secret @{name='mypassword'} 'definitelyNotAPassword!'
::set-secret name=mypassword::definitelyNotAPassword!
.LINK
https://help.github.com/en/actions/reference/workflow-commands-for-github-actions#about-workflow-commands
#>
function Send-ActionCommand {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$Command,

        [Parameter(ParameterSetName = "WithProps", Position = 1, Mandatory)]
        [System.Collections.IDictionary]$Properties,

        [Parameter(ParameterSetName = "WithProps", Position = 2)]
        [Parameter(ParameterSetName = "SkipProps", Position = 1)]
        [string]$Message = ''
    )

    $cmdStr = ConvertTo-ActionCommandString $Command $Properties $Message
    Write-Host $cmdStr
}

<#
.SYNOPSIS
Sends a command to an Action Environment File.
Equivalent to `core.issueFileCommand(cmd, msg)`.
.DESCRIPTION
Appends given message to an Action Environment File.
.PARAMETER Command
Command (environment file variable suffix) to send message for.
.PARAMETER Message
Message to append.
.EXAMPLE
PS> Send-ActionFileCommand ENV 'myvar=value'
.EXAMPLE
PS> 'myvar=value', 'myvar2=novalue' | Send-ActionFileCommand ENV
.LINK
https://docs.github.com/en/actions/reference/workflow-commands-for-github-actions#environment-files
#>
function Send-ActionFileCommand {
    [CmdletBinding()]
    param (
        [Parameter(Position = 0, Mandatory)]
        [ValidateNotNullOrEmpty()]
        [string]$Command,

        [Parameter(Position = 1, Mandatory, ValueFromPipeline)]
        [psobject]$Message
    )
    begin {
        $filePath = [System.Environment]::GetEnvironmentVariable("GITHUB_$Command")
        if (-not $filePath) {
            throw "Unable to find environment variable for file command $Command"
        }
        if (-not (Test-Path $filePath -PathType Leaf)) {
            throw "Missing file at path: $filePath"
        }
    }
    process {
        ConvertTo-ActionCommandValue $Message | Out-File -FilePath $filePath -Append
    }
}

###########################################################################
## Internal Implementation
###########################################################################

<#
.SYNOPSIS
Convert command, properties and message into a single-line workflow command.
.PARAMETER Command
The workflow command name.
.PARAMETER Properties
Properties to add to the command.
.PARAMETER Message
Message to add to the command.
#>
function ConvertTo-ActionCommandString {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Position = 0, Mandatory)]
        [string]$Command,

        [Parameter(Position = 1)]
        [System.Collections.IDictionary]$Properties,

        [Parameter(Position = 2)]
        [AllowNull()]
        [AllowEmptyString()]
        [AllowEmptyCollection()]
        [object]$Message
    )

    if (-not $Command) {
        $Command = 'missing.command'
    }

    $cmdStr = "$($CMD_STRING)$($Command)"
    if ($Properties.Count -gt 0) {
        $first = $true
        foreach ($key in $Properties.Keys) {
            $val = ConvertTo-ActionEscapedProperty $Properties[$key]
            if ($val) {
                if ($first) {
                    $first = $false
                    $cmdStr += ' '
                }
                else {
                    $cmdStr += ','
                }
                $cmdStr += "$($key)=$($val)"
            }
        }
    }
    $cmdStr += $CMD_STRING
    $cmdStr += ConvertTo-ActionEscapedData $Message

    return $cmdStr
}

<#
.SYNOPSIS
Sanitizes an input into a string so it can be passed into issueCommand safely.
Equivalent of `core.toCommandValue(input)`.
.PARAMETER Value
Input to sanitize into a string.
#>
function ConvertTo-ActionCommandValue {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory, Position = 0)]
        [AllowNull()]
        [AllowEmptyString()]
        [AllowEmptyCollection()]
        [object]$Value
    )
    if ($null -eq $Value) {
        return ''
    }
    if ($Value -is [string]) {
        return $Value
    }
    return ConvertTo-Json $Value -Depth 100 -Compress -EscapeHandling EscapeNonAscii
}

## Escaping based on https://github.com/actions/toolkit/blob/3e40dd39cc56303a2451f5b175068dbefdc11c18/packages/core/src/command.ts#L92-L105
function ConvertTo-ActionEscapedData {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory, Position = 0)]
        [AllowNull()]
        [AllowEmptyString()]
        [AllowEmptyCollection()]
        [object]$Value
    )
    return (ConvertTo-ActionCommandValue $Value).
    Replace("%", '%25').
    Replace("`r", '%0D').
    Replace("`n", '%0A')
}

function ConvertTo-ActionEscapedProperty {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory, Position = 0)]
        [AllowNull()]
        [AllowEmptyString()]
        [AllowEmptyCollection()]
        [object]$Value
    )
    return (ConvertTo-ActionCommandValue $Value).
    Replace("%", '%25').
    Replace("`r", '%0D').
    Replace("`n", '%0A').
    Replace(':', '%3A').
    Replace(',', '%2C')
}

Export-ModuleMember `
    Get-VersionVariables,
    Enter-ActionOutputGroup,
    Exit-ActionOutputGroup,
    Send-ActionCommand,
    Set-ActionOutput,
    Set-ActionVariable