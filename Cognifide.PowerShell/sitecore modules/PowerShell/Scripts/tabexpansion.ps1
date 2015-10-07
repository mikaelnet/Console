if(!$options) {
    $options = @{
        CustomArgumentCompleters = @{}
        NativeArgumentCompleters = @{}
        ResultProcessors = @()
        InputProcessors = @()   
    }
} else {
    if(!$options["CustomArgumentCompleters"]) {
        $options["CustomArgumentCompleters"] = @{}
    }
    if(!$options["NativeArgumentCompleters"]) {
        $options["NativeArgumentCompleters"] = @{}
    }
    if(!$options["ResultProcessors"]) {
        $options["ResultProcessors"] = @()
    }
    if(!$options["InputProcessors"]) {
        $options["InputProcessors"] = @()
    }
}

function global:New-CompletionResult {
    param (
	    [Parameter(Mandatory)]
        [string]$CompletionText,
	    [string]$ListItemText = $CompletionText,
	    [System.Management.Automation.CompletionResultType]$ResultType = 'ParameterValue',
	    [string]$ToolTip = $CompletionText
    )
	New-Object System.Management.Automation.CompletionResult $CompletionText, $ListItemText, $ResultType, $ToolTip
}

### Add input processors
$options["InputProcessors"] += {
	### Complete [Type/Namespace[Tab]
	# Expands one piece at a time, e.g. [System. | [System.Data. | [System.Data.CommandType]
	# If pattern in "[pattern" contains wildcard characters all types are searched for the match.
	param($ast, $tokens, $positionOfCursor, $options)

	$token = foreach($_ in $tokens) {if ($_.Extent.EndOffset -eq $positionOfCursor.Offset) {$_; break}}
	if (!$token -or ($token.TokenFlags -ne 'TypeName' -and $token.TokenFlags -ne 'CommandName')) {return}

	$line = $positionOfCursor.Line.Substring(0, $positionOfCursor.ColumnNumber - 1)
	if ($line -notmatch '\[([\w.*?]+)$') {return}
	$pattern = $matches[1]

	# fake
	function TabExpansion($line, $lastWord) { GetTabExpansionType $pattern $lastWord.Substring(0, $lastWord.Length - $pattern.Length) }
	$result = [System.Management.Automation.CommandCompletion]::CompleteInput($ast, $tokens, $positionOfCursor, $null)

	# amend
	for($i = $result.CompletionMatches.Count; --$i -ge 0) {

		$text = $result.CompletionMatches[$i].CompletionText
        if($text -match '[\w.]+') {
            $match = $matches[0]
            $type = 'Type'
            
            if($match.EndsWith(".")) { 
                $type = 'Namespace'
                $match = $match.trimend('.')
            }
            $itemName = $match.trimend('.').split('.') | Select-Object -Last 1
            $result.CompletionMatches[$i] = New-CompletionResult -CompletionText $itemName -ListItemText $itemName -ResultType $type -ToolTip $match
		}
	}

	$result
}

<#
.Synopsis
	Gets types and namespaces for completers.
#>
function global:GetTabExpansionType($pattern, $prefix) {
	$suffix = if ($prefix) {']'} else {''}

	# wildcard type
	if ([System.Management.Automation.WildcardPattern]::ContainsWildcardCharacters($pattern)) {
		.{ foreach($assembly in [System.AppDomain]::CurrentDomain.GetAssemblies()) {
			try {
				foreach($_ in $assembly.GetExportedTypes()) {
					Write-Verbose "Checking $($_.FullName)" -Verbose
					if ($_.FullName -like $pattern) {
					    Write-Verbose "Picked $($_.FullName)" -Verbose
						"$prefix$($_.FullName)$suffix"
					}
				}
			}
			catch { $Error.RemoveAt(0) }
		}} | Sort-Object
		return
	}

	# patterns
	$escaped = [regex]::Escape($pattern)
	$re1 = [regex]"(?i)^($escaped[^.]*)"
	$re2 = [regex]"(?i)^($escaped[^.``]*)(?:``(\d+))?$"
	if (!$pattern.StartsWith('System.', 'OrdinalIgnoreCase')) {
		$re1 = $re1, [regex]"(?i)^System\.($escaped[^.]*)"
		$re2 = $re2, [regex]"(?i)^System\.($escaped[^.``]*)(?:``(\d+))?$"
	}

	# namespaces and types
	$1 = @{}
	$2 = [System.Collections.ArrayList]@()
	foreach($assembly in [System.AppDomain]::CurrentDomain.GetAssemblies()) {
	    
		try { $types = $assembly.GetExportedTypes() }
		catch { $Error.RemoveAt(0); continue }
		$n = [System.Collections.Generic.HashSet[object]]@(foreach($_ in $types) {$_.Namespace})
		foreach($r in $re1) {
			foreach($_ in $n) {
				if ($_ -match $r) {
					$1["$prefix$($matches[1])."] = $null
				}
			}
		}
		foreach($r in $re2) {
			foreach($_ in $types) {
				if ($_.FullName -match $r) {
					if ($matches[2]) {
						$null = $2.Add("$prefix$($matches[1])[$(''.PadRight(([int]$matches[2] - 1), ','))]$suffix")
					}
					else {
						$null = $2.Add("$prefix$($matches[1])$suffix")
					}
				}
			}
		}
	}
	$1.Keys | Sort-Object
	$2 | Sort-Object
}

function global:TabExpansion2
{
	[CmdletBinding(DefaultParameterSetName = 'ScriptInputSet')]
	param(
		[Parameter(ParameterSetName = 'ScriptInputSet', Mandatory = $true, Position = 0)]
		[string]$inputScript,
		[Parameter(ParameterSetName = 'ScriptInputSet', Mandatory = $true, Position = 1)]
		[int]$cursorColumn,
		[Parameter(ParameterSetName = 'AstInputSet', Mandatory = $true, Position = 0)]
		[System.Management.Automation.Language.Ast]$ast,
		[Parameter(ParameterSetName = 'AstInputSet', Mandatory = $true, Position = 1)]
		[System.Management.Automation.Language.Token[]]$tokens,
		[Parameter(ParameterSetName = 'AstInputSet', Mandatory = $true, Position = 2)]
		[System.Management.Automation.Language.IScriptPosition]$positionOfCursor,
		[Parameter(ParameterSetName = 'ScriptInputSet', Position = 2)]
		[Parameter(ParameterSetName = 'AstInputSet', Position = 3)]
		[Hashtable]$options
	)

	# parse input
	if ($psCmdlet.ParameterSetName -eq 'ScriptInputSet') {
		$_ = [System.Management.Automation.CommandCompletion]::MapStringInputToParsedInput($inputScript, $cursorColumn)
		$ast = $_.Item1; $tokens = $_.Item2; $positionOfCursor = $_.Item3
	}

	# input processors
	foreach($_ in $script:options['InputProcessors']) {
		if ($private:result = & $_ $ast $tokens $positionOfCursor $script:options) {
			if ($result -is [System.Management.Automation.CommandCompletion]) {
				return $result
			}
			Write-Error -ErrorAction 0 "TabExpansion2: Invalid result. Input processor: $_"
		}
	}

	# built-in
	$private:result = [System.Management.Automation.CommandCompletion]::CompleteInput($ast, $tokens, $positionOfCursor, $script:options)

	# result processors?
	if (!($private:processors = $script:options['ResultProcessors'])) {
		return $result
	}

	# work around read only
	if ($result.CompletionMatches.IsReadOnly) {
		if ($result.CompletionMatches) {
			return $result
		}
		function TabExpansion {'*'}
		$result = [System.Management.Automation.CommandCompletion]::CompleteInput("$ast", $positionOfCursor.Offset, $null)
		$result.CompletionMatches.Clear()
	}

	# result processors
	foreach($_ in $processors) {
		if (& $_ $result $ast $tokens $positionOfCursor $script:options) {
			Write-Error -ErrorAction 0 "TabExpansion2: Unexpected output. Result processor: $_"
		}
	}

	$result
}