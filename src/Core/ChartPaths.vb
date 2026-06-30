Imports System.IO

Namespace Editor
    Public NotInheritable Class ChartPaths
        Private Sub New()
        End Sub

        Public Shared Function GetFileName(ByVal pathText As String) As String
            Dim fslash As Integer = InStrRev(pathText, "/")
            Dim bslash As Integer = InStrRev(pathText, "\")
            Return Mid(pathText, If(fslash > bslash, fslash, bslash) + 1)
        End Function

        Public Shared Function ExcludeFileName(ByVal pathText As String) As String
            Dim fslash As Integer = InStrRev(pathText, "/")
            Dim bslash As Integer = InStrRev(pathText, "\")
            If (bslash Or fslash) = 0 Then Return ""
            Return Mid(pathText, 1, If(fslash > bslash, fslash, bslash) - 1)
        End Function

        Public Shared Function ResolveBmsFilePath(ByVal baseDirectory As String, ByVal refPath As String) As String
            If refPath = "" Then Return ""
            If Path.IsPathRooted(refPath) Then Return refPath
            If baseDirectory = "" Then Return refPath

            Try
                Return Path.GetFullPath(Path.Combine(baseDirectory, refPath))
            Catch
                Return refPath
            End Try
        End Function

        Public Shared Function MakeBmsReferencePath(ByVal baseDirectory As String, ByVal targetPath As String) As String
            If targetPath = "" Then Return ""
            If Not Path.IsPathRooted(targetPath) Then Return targetPath
            If baseDirectory = "" Then Return GetFileName(targetPath)

            Try
                Dim baseFullPath As String = Path.GetFullPath(baseDirectory)
                Dim targetFullPath As String = Path.GetFullPath(targetPath)

                If Not String.Equals(Path.GetPathRoot(baseFullPath), Path.GetPathRoot(targetFullPath), StringComparison.OrdinalIgnoreCase) Then
                    Return targetFullPath
                End If

                Return MakeRelativePath(baseFullPath, targetFullPath)
            Catch
                Return GetFileName(targetPath)
            End Try
        End Function

        Public Shared Function MakeRelativePath(ByVal basePath As String, ByVal fullPath As String) As String
            Dim separators As Char() = {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}
            Dim baseParts As String() = Path.GetFullPath(basePath).TrimEnd(separators).Split(separators, StringSplitOptions.RemoveEmptyEntries)
            Dim fullParts As String() = Path.GetFullPath(fullPath).Split(separators, StringSplitOptions.RemoveEmptyEntries)
            Dim same As Integer = 0

            Do While same < baseParts.Length AndAlso
                     same < fullParts.Length AndAlso
                     String.Equals(baseParts(same), fullParts(same), StringComparison.OrdinalIgnoreCase)
                same += 1
            Loop

            Dim relativeParts As New List(Of String)
            For i As Integer = same To baseParts.Length - 1
                relativeParts.Add("..")
            Next
            For i As Integer = same To fullParts.Length - 1
                relativeParts.Add(fullParts(i))
            Next

            If relativeParts.Count = 0 Then Return GetFileName(fullPath)

            Return String.Join("\", relativeParts.ToArray())
        End Function
    End Class
End Namespace
