function New-ScriptSession {
    <#
        .SYNOPSIS
            Creates a new script session in Sitecore PowerShell Extensions via web service calls.
    
        .EXAMPLE
            The following remotely connects to an instance of Sitecore initializes a session.
            
            New-ScriptSession -Username admin -Password b -ConnectionUri http://remotesitecore
    
            Username      : admin
            Password      : b
            ConnectionUri : http://concentrasitecore/sitecore%20modules/PowerShell/Services/RemoteAutomation.asmx
            SessionId     : 528b9865-a69e-4875-919f-12209646c934
            Credential    : 
            Proxy         : Microsoft.PowerShell.Commands.NewWebserviceProxy.AutogeneratedTypes.WebServiceProxy1Services_RemoteAutomation_asmx.RemoteAutomation
    #>
    param(
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$Username = $null,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$Password = $null,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [Uri[]]$ConnectionUri = $null,

        [Parameter(Mandatory = $false)]
        [System.Management.Automation.PSCredential]
        $Credential
    )
    
    begin {
        $sessionId = [guid]::NewGuid()
        $session = @{
            "Username" = [string]$Username
            "Password" = [string]$Password
            "SessionId" = [string]$sessionId
            "Credential" = [System.Management.Automation.PSCredential]$Credential
            "Connection" = @()
        }
    }

    process {

        foreach($uri in $ConnectionUri) {

            $script = {
                Get-ScriptSession -Current
            }

            $baseUri = $uri

            if(!$uri.AbsoluteUri.EndsWith(".asmx")) {
                $uri = "$($uri.AbsoluteUri.TrimEnd('/'))/sitecore%20modules/PowerShell/Services/RemoteAutomation.asmx"
            }

            $proxyProps = @{
                Uri = $uri
            }

            if($Credential) {
                $proxyProps["Credential"] = $Credential
            }

            $connection = [PSCustomObject]@{
                Uri = [Uri]$uri
                BaseUri = [Uri]$baseUri
            }

            $lazyProxy = {
                $result = & {

                    $proxy = New-WebServiceProxy @proxyProps
                    if($Credential) {
                        $proxy.Credentials = $Credential
                    }

                    $proxy
                }

                Add-Member -InputObject $this -MemberType NoteProperty -Name "Proxy" -Value $result -Force

                $result
            }.GetNewClosure()

            Add-Member -InputObject $connection -MemberType ScriptProperty -Name "Proxy" -Value $lazyProxy

            $session["Connection"] += @($connection)
            #if(-not $proxy) { return $null }

            <#
            $response = $proxy.ExecuteScriptBlock2($Username, $Password, $script.ToString(), $null, $SessionId)
            $serverSession = ConvertFrom-CliXml -InputObject $response

            if([guid]::TryParse($serverSession.ID, [ref] $sessionId)) {
                $session["Connection"] += @(
                    [PSCustomObject]@{
                        Uri = [Uri]$uri
                        Proxy = [PSObject]$proxy
                        BaseUri = [Uri]$baseUri
                    })
            } else {
                Write-Error -Message "There was a problem creating a new script session."
            }
            #>
        }
    }
    
    end {
        [PSCustomObject]$session
    }
}