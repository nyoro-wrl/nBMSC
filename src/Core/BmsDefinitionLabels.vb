Namespace Editor
    Public NotInheritable Class BmsDefinitionLabels
        Public Const ModeLegacy As Integer = 0
        Public Const ModeBase36 As Integer = 1
        Public Const ModeBase62 As Integer = 2

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

        Public Shared Function ModeMax(ByVal mode As Integer) As Integer
            Select Case mode
                Case ModeBase62
                    Return Functions.MaxDefinition
                Case ModeBase36
                    Return Functions.MaxBase36Definition
            End Select

            Return Functions.MaxLegacyDefinition
        End Function

        Public Shared Function ModeLabel(ByVal value As Long, ByVal mode As Integer) As String
            Select Case mode
                Case ModeBase62
                    Return Functions.C10to36(value)
                Case ModeBase36
                    Return Functions.C10toBase36(value)
            End Select

            Return Mid("0" & Hex(value), Len(Hex(value)))
        End Function

        Public Shared Function ModeIndex(ByVal labelText As String, ByVal mode As Integer) As Integer
            Select Case mode
                Case ModeBase62
                    Return Functions.C36to10(labelText)
                Case ModeBase36
                    Return Functions.CBase36to10(labelText)
            End Select

            Return Convert.ToInt32(labelText, 16)
        End Function

        Public Shared Function ContainsBase62Definition(ByVal labelText As String) As Boolean
            For Each value As Char In labelText
                If value >= "a"c AndAlso value <= "z"c Then Return True
            Next

            Return False
        End Function

        Public Shared Function ContainsBase62Definitions(ByVal lines() As String) As Boolean
            For Each line As String In lines
                Dim trimmedLine As String = line.Trim()

                If trimmedLine.StartsWith("#WAV", StringComparison.CurrentCultureIgnoreCase) OrElse
                   trimmedLine.StartsWith("#BMP", StringComparison.CurrentCultureIgnoreCase) OrElse
                   trimmedLine.StartsWith("#BPM", StringComparison.CurrentCultureIgnoreCase) OrElse
                   trimmedLine.StartsWith("#STOP", StringComparison.CurrentCultureIgnoreCase) OrElse
                   trimmedLine.StartsWith("#SCROLL", StringComparison.CurrentCultureIgnoreCase) Then
                    If ContainsBase62Definition(Mid(trimmedLine, 5, 2)) Then Return True

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
