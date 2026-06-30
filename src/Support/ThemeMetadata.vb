Imports System.IO
Imports System.Xml

Public NotInheritable Class ThemeMetadata
    Private Sub New()
    End Sub

    Public Shared Function TryReadThemeName(ByVal filePath As String, ByRef themeName As String) As Boolean
        Try
            Dim document As New XmlDocument()
            Using stream As New FileStream(filePath, FileMode.Open, FileAccess.Read)
                document.Load(stream)
            End Using

            Dim root As XmlElement = document.Item("nBMSCTheme")
            If root Is Nothing OrElse root.GetAttribute("version") <> "1" Then Return False

            themeName = root.GetAttribute("name")
            If themeName = "" Then themeName = Path.GetFileNameWithoutExtension(filePath)
            Return True

        Catch ex As Exception
            Return False
        End Try
    End Function
End Class
