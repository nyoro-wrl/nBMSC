Option Strict On

Namespace Editor
    Public NotInheritable Class BmsDefinitionLabels
        Private Sub New()
        End Sub

        Public Shared Function DisplayMax(ByVal useBase62 As Boolean) As Integer
            If useBase62 Then Return Functions.MaxDefinition

            Return Functions.MaxBase36Definition
        End Function

        Public Shared Function LastListIndex(ByVal useBase62 As Boolean) As Integer
            Return DisplayMax(useBase62) - 1
        End Function

        Public Shared Function Label(ByVal value As Long, ByVal useBase62 As Boolean) As String
            If useBase62 Then Return Functions.C10to36(value)

            Return Functions.C10toBase36(value)
        End Function

        Public Shared Function Index(ByVal labelText As String, ByVal useBase62 As Boolean) As Integer
            If useBase62 Then Return Functions.C36to10(labelText)

            Return Functions.CBase36to10(labelText)
        End Function

        Public Shared Function IsLabel(ByVal labelText As String, ByVal useBase62 As Boolean) As Boolean
            If useBase62 Then Return Functions.IsBase62(labelText)

            Return Functions.IsBase36(labelText)
        End Function

        Public Shared Function ContainsBase62Definition(ByVal labelText As String) As Boolean
            For Each value As Char In labelText
                If value >= "a"c AndAlso value <= "z"c Then Return True
            Next

            Return False
        End Function

        Public Shared Function IsBase62Header(ByVal line As String) As Boolean
            Dim trimmedLine As String = line.Trim()
            Return trimmedLine.StartsWith("#BASE", StringComparison.OrdinalIgnoreCase) AndAlso
                   trimmedLine.Length > 5 AndAlso Char.IsWhiteSpace(trimmedLine(5)) AndAlso
                   trimmedLine.Substring(6).Trim() = "62"
        End Function

        Public Shared Function ContainsBase62Definitions(ByVal lines() As String) As Boolean
            For Each line As String In lines
                Dim trimmedLine As String = line.Trim()
                If IsBase62Header(trimmedLine) Then Return True

                If trimmedLine.StartsWith("#WAV", StringComparison.CurrentCultureIgnoreCase) OrElse
                   trimmedLine.StartsWith("#BMP", StringComparison.CurrentCultureIgnoreCase) OrElse
                   trimmedLine.StartsWith("#BPM", StringComparison.CurrentCultureIgnoreCase) OrElse
                   trimmedLine.StartsWith("#STOP", StringComparison.CurrentCultureIgnoreCase) OrElse
                   trimmedLine.StartsWith("#SCROLL", StringComparison.CurrentCultureIgnoreCase) Then
                    Dim labelStart As Integer = 5
                    If trimmedLine.StartsWith("#STOP", StringComparison.CurrentCultureIgnoreCase) Then labelStart = Len("#STOP") + 1
                    If trimmedLine.StartsWith("#SCROLL", StringComparison.CurrentCultureIgnoreCase) Then labelStart = Len("#SCROLL") + 1
                    If ContainsBase62Definition(Mid(trimmedLine, labelStart, 2)) Then Return True

                ElseIf trimmedLine.StartsWith("#LNOBJ", StringComparison.CurrentCultureIgnoreCase) Then
                    If ContainsBase62Definition(Mid(trimmedLine, Len("#LNOBJ") + 1).Trim()) Then Return True

                ElseIf trimmedLine.StartsWith("#") AndAlso Mid(trimmedLine, 7, 1) = ":" Then
                    If Mid(trimmedLine, 5, 2) = "03" Then Continue For

                    For i As Integer = 8 To Len(trimmedLine) - 1 Step 2
                        Dim labelText As String = Mid(trimmedLine, i, 2)
                        If labelText <> "00" AndAlso ContainsBase62Definition(labelText) Then Return True
                    Next
                End If
            Next

            Return False
        End Function
    End Class
End Namespace
