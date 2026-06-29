Imports System.Net
Imports System.Text
Imports System.Text.Json

Public Class UpdateCheckResult
    Public Property IsSuccess As Boolean
    Public Property ErrorMessage As String
    Public Property LatestTag As String
    Public Property LatestVersion As Version
    Public Property ReleaseUrl As String
    Public Property CanCompare As Boolean
    Public Property HasUpdate As Boolean
End Class

Public NotInheritable Class UpdateChecker
    Private Const LatestReleaseApiUrl As String = "https://api.github.com/repos/nyoro-wrl/nBMSC/releases/latest"

    Private Sub New()
    End Sub

    Public Shared Function Check(ByVal currentVersion As Version) As UpdateCheckResult
        Try
            Dim xJson As String = DownloadLatestReleaseJson()
            Dim xTagName As String = ""
            Dim xReleaseUrl As String = ""

            Using xDocument As JsonDocument = JsonDocument.Parse(xJson)
                Dim xRoot As JsonElement = xDocument.RootElement
                Dim xTagElement As JsonElement
                Dim xUrlElement As JsonElement

                If Not xRoot.TryGetProperty("tag_name", xTagElement) Then Return Fail("tag_name was not found.")
                If Not xRoot.TryGetProperty("html_url", xUrlElement) Then Return Fail("html_url was not found.")

                xTagName = xTagElement.GetString()
                xReleaseUrl = xUrlElement.GetString()
            End Using

            If String.IsNullOrWhiteSpace(xTagName) Then Return Fail("tag_name was empty.")
            If String.IsNullOrWhiteSpace(xReleaseUrl) Then Return Fail("html_url was empty.")

            Dim xLatestVersion As Version = Nothing
            Dim xCanCompare As Boolean = TryParseVersionTag(xTagName, xLatestVersion)
            Dim xHasUpdate As Boolean = False
            If xCanCompare Then xLatestVersion = NormalizeVersion(xLatestVersion)
            If xCanCompare AndAlso currentVersion IsNot Nothing Then
                xHasUpdate = xLatestVersion.CompareTo(NormalizeVersion(currentVersion)) > 0
            End If

            Return New UpdateCheckResult With {
                .IsSuccess = True,
                .LatestTag = xTagName,
                .LatestVersion = xLatestVersion,
                .ReleaseUrl = xReleaseUrl,
                .CanCompare = xCanCompare,
                .HasUpdate = xHasUpdate
            }
        Catch ex As Exception
            Return Fail(ex.Message)
        End Try
    End Function

    Public Shared Function TryParseVersionTag(ByVal tagName As String, ByRef version As Version) As Boolean
        If String.IsNullOrWhiteSpace(tagName) Then Return False

        Dim xTagName As String = tagName.Trim()
        If xTagName.StartsWith("v", StringComparison.OrdinalIgnoreCase) Then
            xTagName = xTagName.Substring(1)
        End If

        If xTagName.Split("."c).Length <> 3 Then Return False

        Return Version.TryParse(xTagName, version)
    End Function

    Private Shared Function NormalizeVersion(ByVal version As Version) As Version
        If version Is Nothing Then Return Nothing
        If version.Build < 0 Then Return New Version(version.Major, version.Minor)

        Return New Version(version.Major, version.Minor, version.Build)
    End Function

    Private Shared Function DownloadLatestReleaseJson() As String
        ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol Or SecurityProtocolType.Tls12

        Using xClient As New WebClient()
            xClient.Encoding = Encoding.UTF8
            xClient.Headers(HttpRequestHeader.Accept) = "application/vnd.github+json"
            xClient.Headers(HttpRequestHeader.UserAgent) = "nBMSC"
            xClient.Headers("X-GitHub-Api-Version") = "2022-11-28"
            Return xClient.DownloadString(LatestReleaseApiUrl)
        End Using
    End Function

    Private Shared Function Fail(ByVal message As String) As UpdateCheckResult
        Return New UpdateCheckResult With {
            .IsSuccess = False,
            .ErrorMessage = message
        }
    End Function
End Class
