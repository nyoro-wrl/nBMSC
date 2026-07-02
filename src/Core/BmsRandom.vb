Option Strict On

Imports System.Collections.Generic

Namespace Editor
    Public Enum BmsRandomViewMode
        AllBranches = 0
        CurrentValue = 1
        Hidden = 2
    End Enum

    Public Class BmsRandomBlock
        Public Property DefinitionValue As Integer = 2
        Public Property CurrentValue As Integer = 1
        Public Property ViewMode As BmsRandomViewMode = BmsRandomViewMode.CurrentValue
        Public ReadOnly Property ExtraTextByValue As Dictionary(Of Integer, String)
        Public ReadOnly Property MeasureLengthByValue As Dictionary(Of Integer, Dictionary(Of Integer, Double))

        Public Sub New()
            ExtraTextByValue = New Dictionary(Of Integer, String)()
            MeasureLengthByValue = New Dictionary(Of Integer, Dictionary(Of Integer, Double))()
        End Sub

        Public Sub New(ByVal definitionValue As Integer)
            Me.New()
            Me.DefinitionValue = Math.Max(1, definitionValue)
            Me.CurrentValue = 1
        End Sub

        Public Sub Normalize()
            DefinitionValue = Math.Max(1, DefinitionValue)
            CurrentValue = Math.Min(DefinitionValue, Math.Max(1, CurrentValue))
        End Sub

        Public Function GetExtraText(ByVal value As Integer) As String
            Dim result As String = ""
            If ExtraTextByValue.TryGetValue(value, result) Then Return If(result, "")

            Return ""
        End Function

        Public Sub SetExtraText(ByVal value As Integer, ByVal text As String)
            If String.IsNullOrEmpty(text) Then
                ExtraTextByValue.Remove(value)
                Return
            End If

            ExtraTextByValue(value) = text
        End Sub

        Public Function GetMeasureLengthOverrides(ByVal value As Integer) As Dictionary(Of Integer, Double)
            Dim result As Dictionary(Of Integer, Double) = Nothing
            If MeasureLengthByValue.TryGetValue(value, result) Then Return result

            Return New Dictionary(Of Integer, Double)()
        End Function

        Public Sub SetMeasureLength(ByVal value As Integer, ByVal measureIndex As Integer, ByVal length As Double)
            If measureIndex < 0 OrElse measureIndex > 999 Then Return
            If length <= 0.0R Then Return

            Dim xOverrides As Dictionary(Of Integer, Double) = Nothing
            If Not MeasureLengthByValue.TryGetValue(value, xOverrides) Then
                xOverrides = New Dictionary(Of Integer, Double)()
                MeasureLengthByValue(value) = xOverrides
            End If

            xOverrides(measureIndex) = length
        End Sub
    End Class

    Public Class BmsRandomParsedBranch
        Public Property Value As Integer
        Public ReadOnly Property Lines As List(Of String)

        Public Sub New(ByVal value As Integer)
            Me.Value = value
            Lines = New List(Of String)()
        End Sub
    End Class

    Public Class BmsRandomParsedBlock
        Public Property DefinitionValue As Integer
        Public Property IsRawText As Boolean
        Public ReadOnly Property Branches As List(Of BmsRandomParsedBranch)
        Public ReadOnly Property RawLines As List(Of String)

        Public Sub New(ByVal definitionValue As Integer)
            Me.DefinitionValue = Math.Max(1, definitionValue)
            Branches = New List(Of BmsRandomParsedBranch)()
            RawLines = New List(Of String)()
        End Sub
    End Class

    Public Class BmsRandomParseResult
        Public ReadOnly Property TopLevelLines As List(Of String)
        Public ReadOnly Property Blocks As List(Of BmsRandomParsedBlock)

        Public Sub New()
            TopLevelLines = New List(Of String)()
            Blocks = New List(Of BmsRandomParsedBlock)()
        End Sub
    End Class

    Public NotInheritable Class BmsRandomParser
        Private Sub New()
        End Sub

        Public Shared Function Parse(ByVal lines() As String) As BmsRandomParseResult
            Dim result As New BmsRandomParseResult()
            If lines Is Nothing Then Return result

            Dim i As Integer = 0
            While i < lines.Length
                Dim line As String = lines(i)
                Dim trimmed As String = line.Trim()

                If IsCommand(trimmed, "RANDOM") Then
                    Dim nextIndex As Integer = i + 1
                    result.Blocks.Add(ParseRandomBlock(lines, i, nextIndex))
                    i = nextIndex
                Else
                    result.TopLevelLines.Add(line)
                    i += 1
                End If
            End While

            Return result
        End Function

        Private Shared Function ParseRandomBlock(ByVal lines() As String, ByVal startIndex As Integer, ByRef nextIndex As Integer) As BmsRandomParsedBlock
            Dim startLine As String = lines(startIndex)
            Dim block As New BmsRandomParsedBlock(CommandArgumentInt(startLine.Trim(), 1))
            block.RawLines.Add(startLine)

            Dim i As Integer = startIndex + 1

            While i < lines.Length
                Dim line As String = lines(i)
                Dim trimmed As String = line.Trim()

                If IsIgnorable(trimmed) Then
                    block.RawLines.Add(line)
                    i += 1
                    Continue While
                End If

                If IsCommand(trimmed, "ENDRANDOM") Then
                    block.RawLines.Add(line)
                    i += 1
                    Exit While
                End If

                If Not IsCommand(trimmed, "IF") Then
                    If block.Branches.Count > 0 AndAlso Not HasEndRandomAhead(lines, i) Then Exit While

                    block.IsRawText = True
                    block.RawLines.Add(line)
                    i += 1
                    Continue While
                End If

                Dim branch As New BmsRandomParsedBranch(CommandArgumentInt(trimmed, 0))
                block.RawLines.Add(line)
                i += 1

                Dim endedIf As Boolean = False
                Dim ifDepth As Integer = 0
                While i < lines.Length
                    line = lines(i)
                    trimmed = line.Trim()

                    If IsCommand(trimmed, "IF") Then
                        ifDepth += 1
                        branch.Lines.Add(line)
                        block.RawLines.Add(line)
                        i += 1
                        Continue While
                    End If

                    If IsCommand(trimmed, "ENDIF") Then
                        If ifDepth > 0 Then
                            ifDepth -= 1
                            branch.Lines.Add(line)
                            block.RawLines.Add(line)
                            i += 1
                            Continue While
                        End If

                        block.RawLines.Add(line)
                        i += 1
                        endedIf = True
                        Exit While
                    End If

                    branch.Lines.Add(line)
                    block.RawLines.Add(line)
                    i += 1
                End While

                block.Branches.Add(branch)

                If Not endedIf Then
                    block.IsRawText = True
                    Exit While
                End If
            End While

            nextIndex = i
            If block.IsRawText Then block.Branches.Clear()
            Return block
        End Function

        Private Shared Function HasEndRandomAhead(ByVal lines() As String, ByVal startIndex As Integer) As Boolean
            For i As Integer = startIndex To lines.Length - 1
                Dim trimmed As String = lines(i).Trim()
                If IsCommand(trimmed, "ENDRANDOM") Then Return True
                If i > startIndex AndAlso IsCommand(trimmed, "RANDOM") Then Return False
            Next

            Return False
        End Function

        Public Shared Function IsCommand(ByVal trimmedLine As String, ByVal commandName As String) As Boolean
            If trimmedLine Is Nothing Then Return False
            If Not trimmedLine.StartsWith("#", StringComparison.CurrentCultureIgnoreCase) Then Return False

            Dim commandText As String = "#" & commandName
            If Not trimmedLine.StartsWith(commandText, StringComparison.CurrentCultureIgnoreCase) Then Return False
            If trimmedLine.Length = commandText.Length Then Return True

            Dim nextChar As Char = trimmedLine(commandText.Length)
            Return Char.IsWhiteSpace(nextChar)
        End Function

        Private Shared Function IsIgnorable(ByVal trimmedLine As String) As Boolean
            Return trimmedLine = "" OrElse trimmedLine.StartsWith("*", StringComparison.CurrentCultureIgnoreCase)
        End Function

        Private Shared Function CommandArgumentInt(ByVal trimmedLine As String, ByVal fallback As Integer) As Integer
            Dim parts() As String = trimmedLine.Split(New Char() {" "c, ControlChars.Tab}, StringSplitOptions.RemoveEmptyEntries)
            If parts.Length < 2 Then Return fallback

            Dim value As Integer = fallback
            If Integer.TryParse(parts(1), value) Then Return Math.Max(1, value)

            Return fallback
        End Function
    End Class
End Namespace
