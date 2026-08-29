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

        Public Sub SetMeasureLengthOverrides(ByVal value As Integer, ByVal measureLengths As Dictionary(Of Integer, Double))
            If measureLengths Is Nothing OrElse measureLengths.Count = 0 Then
                MeasureLengthByValue.Remove(value)
                Return
            End If

            Dim xOverrides As New Dictionary(Of Integer, Double)()
            For Each pair As KeyValuePair(Of Integer, Double) In measureLengths
                If pair.Key < 0 OrElse pair.Key > 999 Then Continue For
                If pair.Value <= 0.0R Then Continue For
                xOverrides(pair.Key) = pair.Value
            Next

            If xOverrides.Count = 0 Then
                MeasureLengthByValue.Remove(value)
                Return
            End If

            MeasureLengthByValue(value) = xOverrides
        End Sub
    End Class

    Public NotInheritable Class BmsMeasureStructure
        Friend Structure MeasureRange
            Public ReadOnly StartIndex As Integer
            Public ReadOnly Count As Integer

            Public Sub New(ByVal startIndex As Integer, ByVal count As Integer)
                Me.StartIndex = startIndex
                Me.Count = count
            End Sub
        End Structure

        Private Sub New()
        End Sub

        Friend Shared Function GetSelectedMeasureRanges(ByVal selectedIndices As IEnumerable(Of Integer),
                                                        ByVal lastMeasureIndex As Integer) As List(Of MeasureRange)
            Dim ranges As New List(Of MeasureRange)()
            If selectedIndices Is Nothing OrElse lastMeasureIndex < 0 Then Return ranges

            Dim sortedIndices As New List(Of Integer)()
            For Each index As Integer In selectedIndices
                If index < 0 OrElse index > lastMeasureIndex OrElse sortedIndices.Contains(index) Then Continue For
                sortedIndices.Add(index)
            Next
            sortedIndices.Sort()
            If sortedIndices.Count = 0 Then Return ranges

            Dim rangeStart As Integer = sortedIndices(0)
            Dim previousIndex As Integer = rangeStart
            For i As Integer = 1 To sortedIndices.Count - 1
                Dim currentIndex As Integer = sortedIndices(i)
                If currentIndex = previousIndex + 1 Then
                    previousIndex = currentIndex
                    Continue For
                End If

                ranges.Add(New MeasureRange(rangeStart, previousIndex - rangeStart + 1))
                rangeStart = currentIndex
                previousIndex = currentIndex
            Next
            ranges.Add(New MeasureRange(rangeStart, previousIndex - rangeStart + 1))

            Return ranges
        End Function

        Public Shared Function CanJoinSelectedMeasures(ByVal selectedIndices As IEnumerable(Of Integer),
                                                       ByVal lastMeasureIndex As Integer) As Boolean
            Return GetJoinableMeasureRanges(selectedIndices, lastMeasureIndex).Count > 0
        End Function

        Public Shared Sub JoinSelectedMeasures(ByVal baseMeasureLengths() As Double,
                                               ByVal randomBlocks As IList(Of BmsRandomBlock),
                                               ByVal selectedIndices As IEnumerable(Of Integer))
            If baseMeasureLengths Is Nothing Then Return

            Dim ranges As List(Of MeasureRange) = GetJoinableMeasureRanges(selectedIndices, UBound(baseMeasureLengths))
            If ranges.Count = 0 Then Return

            Dim sourceBaseMeasureLengths() As Double = CopyMeasureLengths(baseMeasureLengths)
            Dim joinedBaseMeasureLengths() As Double = CopyMeasureLengths(baseMeasureLengths)
            JoinMeasureRanges(joinedBaseMeasureLengths, ranges)

            If randomBlocks IsNot Nothing Then
                For Each block As BmsRandomBlock In randomBlocks
                    If block Is Nothing Then Continue For

                    Dim values As New List(Of Integer)(block.MeasureLengthByValue.Keys)
                    For Each value As Integer In values
                        Dim effectiveMeasureLengths() As Double = BuildEffectiveMeasureLengths(sourceBaseMeasureLengths,
                                                                                               block.GetMeasureLengthOverrides(value))
                        Dim overrideFlags(UBound(sourceBaseMeasureLengths)) As Boolean
                        For Each pair As KeyValuePair(Of Integer, Double) In block.GetMeasureLengthOverrides(value)
                            If pair.Key >= 0 AndAlso pair.Key <= UBound(overrideFlags) AndAlso pair.Value > 0.0R Then
                                overrideFlags(pair.Key) = True
                            End If
                        Next

                        JoinMeasureRanges(effectiveMeasureLengths, ranges)
                        JoinOverrideRanges(overrideFlags, ranges)

                        Dim joinedOverrides As New Dictionary(Of Integer, Double)()
                        For measureIndex As Integer = 0 To UBound(overrideFlags)
                            If overrideFlags(measureIndex) Then joinedOverrides(measureIndex) = effectiveMeasureLengths(measureIndex)
                        Next
                        block.SetMeasureLengthOverrides(value, joinedOverrides)
                    Next
                Next
            End If

            Array.Copy(joinedBaseMeasureLengths, baseMeasureLengths, baseMeasureLengths.Length)
        End Sub

        Private Shared Function GetJoinableMeasureRanges(ByVal selectedIndices As IEnumerable(Of Integer),
                                                         ByVal lastMeasureIndex As Integer) As List(Of MeasureRange)
            Dim joinableRanges As New List(Of MeasureRange)()
            For Each range As MeasureRange In GetSelectedMeasureRanges(selectedIndices, lastMeasureIndex)
                If range.Count > 1 Then joinableRanges.Add(range)
            Next
            Return joinableRanges
        End Function

        Private Shared Function CopyMeasureLengths(ByVal source() As Double) As Double()
            Dim result(UBound(source)) As Double
            Array.Copy(source, result, source.Length)
            Return result
        End Function

        Private Shared Function BuildEffectiveMeasureLengths(ByVal baseMeasureLengths() As Double,
                                                             ByVal xOverrides As Dictionary(Of Integer, Double)) As Double()
            Dim result(UBound(baseMeasureLengths)) As Double
            For i As Integer = 0 To UBound(baseMeasureLengths)
                result(i) = If(baseMeasureLengths(i) > 0.0R, baseMeasureLengths(i), 192.0R)
            Next

            If xOverrides IsNot Nothing Then
                For Each pair As KeyValuePair(Of Integer, Double) In xOverrides
                    If pair.Key >= 0 AndAlso pair.Key <= UBound(result) AndAlso pair.Value > 0.0R Then result(pair.Key) = pair.Value
                Next
            End If

            Return result
        End Function

        Private Shared Sub JoinMeasureRanges(ByVal measureLengths() As Double,
                                             ByVal ranges As IList(Of MeasureRange))
            For rangeIndex As Integer = ranges.Count - 1 To 0 Step -1
                Dim range As MeasureRange = ranges(rangeIndex)
                Dim joinedLength As Double = 0.0R
                For measureIndex As Integer = range.StartIndex To range.StartIndex + range.Count - 1
                    joinedLength += If(measureLengths(measureIndex) > 0.0R, measureLengths(measureIndex), 192.0R)
                Next
                measureLengths(range.StartIndex) = joinedLength

                For i As Integer = 2 To range.Count
                    RemoveMeasure(measureLengths, Nothing, range.StartIndex + 1)
                Next
            Next
        End Sub

        Private Shared Sub JoinOverrideRanges(ByVal overrideFlags() As Boolean,
                                              ByVal ranges As IList(Of MeasureRange))
            For rangeIndex As Integer = ranges.Count - 1 To 0 Step -1
                Dim range As MeasureRange = ranges(rangeIndex)
                Dim hasOverride As Boolean = False
                For measureIndex As Integer = range.StartIndex To range.StartIndex + range.Count - 1
                    hasOverride = hasOverride OrElse overrideFlags(measureIndex)
                Next
                overrideFlags(range.StartIndex) = hasOverride

                For i As Integer = 2 To range.Count
                    RemoveOverrideFlag(overrideFlags, range.StartIndex + 1)
                Next
            Next
        End Sub

        Private Shared Sub RemoveOverrideFlag(ByVal overrideFlags() As Boolean, ByVal measureIndex As Integer)
            For i As Integer = measureIndex To UBound(overrideFlags) - 1
                overrideFlags(i) = overrideFlags(i + 1)
            Next
        End Sub

        Public Shared Sub InsertMeasuresBeforeSelection(ByVal baseMeasureLengths() As Double,
                                                        ByVal randomBlocks As IList(Of BmsRandomBlock),
                                                        ByVal selectedIndices As IEnumerable(Of Integer))
            If baseMeasureLengths Is Nothing Then Return

            Dim ranges As List(Of MeasureRange) = GetSelectedMeasureRanges(selectedIndices, UBound(baseMeasureLengths))
            For rangeIndex As Integer = ranges.Count - 1 To 0 Step -1
                Dim range As MeasureRange = ranges(rangeIndex)
                For i As Integer = 1 To range.Count
                    InsertMeasure(baseMeasureLengths, randomBlocks, range.StartIndex)
                Next
            Next
        End Sub

        Friend Shared Sub InsertMeasure(ByVal baseMeasureLengths() As Double,
                                        ByVal randomBlocks As IList(Of BmsRandomBlock),
                                        ByVal measureIndex As Integer)
            If baseMeasureLengths Is Nothing Then Return
            If measureIndex < 0 OrElse measureIndex > UBound(baseMeasureLengths) Then Return

            Dim lastMeasureIndex As Integer = UBound(baseMeasureLengths)
            Dim insertedMeasureLength As Double = If(baseMeasureLengths(measureIndex) > 0.0R,
                                                     baseMeasureLengths(measureIndex),
                                                     192.0R)
            For i As Integer = lastMeasureIndex To measureIndex + 1 Step -1
                baseMeasureLengths(i) = baseMeasureLengths(i - 1)
            Next
            baseMeasureLengths(measureIndex) = insertedMeasureLength

            If randomBlocks Is Nothing Then Return
            For Each block As BmsRandomBlock In randomBlocks
                InsertMeasure(block, measureIndex, lastMeasureIndex)
            Next
        End Sub

        Private Shared Sub InsertMeasure(ByVal block As BmsRandomBlock,
                                         ByVal measureIndex As Integer,
                                         ByVal lastMeasureIndex As Integer)
            If block Is Nothing Then Return

            Dim values As New List(Of Integer)(block.MeasureLengthByValue.Keys)
            For Each value As Integer In values
                Dim source As Dictionary(Of Integer, Double) = block.GetMeasureLengthOverrides(value)
                Dim shifted As New Dictionary(Of Integer, Double)()
                Dim insertedMeasureLength As Double = 0.0R
                Dim hasInsertedMeasureLength As Boolean = source.TryGetValue(measureIndex, insertedMeasureLength)

                For Each pair As KeyValuePair(Of Integer, Double) In source
                    If pair.Key < measureIndex Then
                        shifted(pair.Key) = pair.Value
                    ElseIf pair.Key < lastMeasureIndex Then
                        shifted(pair.Key + 1) = pair.Value
                    End If
                Next

                If hasInsertedMeasureLength Then shifted(measureIndex) = insertedMeasureLength
                block.SetMeasureLengthOverrides(value, shifted)
            Next
        End Sub

        Public Shared Sub RemoveSelectedMeasures(ByVal baseMeasureLengths() As Double,
                                                 ByVal randomBlocks As IList(Of BmsRandomBlock),
                                                 ByVal selectedIndices As IEnumerable(Of Integer))
            If baseMeasureLengths Is Nothing Then Return

            Dim ranges As List(Of MeasureRange) = GetSelectedMeasureRanges(selectedIndices, UBound(baseMeasureLengths))
            For rangeIndex As Integer = ranges.Count - 1 To 0 Step -1
                Dim range As MeasureRange = ranges(rangeIndex)
                For measureIndex As Integer = range.StartIndex + range.Count - 1 To range.StartIndex Step -1
                    RemoveMeasure(baseMeasureLengths, randomBlocks, measureIndex)
                Next
            Next
        End Sub

        Public Shared Sub RemoveMeasure(ByVal baseMeasureLengths() As Double,
                                        ByVal randomBlocks As IList(Of BmsRandomBlock),
                                        ByVal measureIndex As Integer)
            If baseMeasureLengths Is Nothing Then Return
            If measureIndex < 0 OrElse measureIndex > UBound(baseMeasureLengths) Then Return

            Dim lastMeasureIndex As Integer = UBound(baseMeasureLengths)
            For i As Integer = measureIndex To lastMeasureIndex - 1
                baseMeasureLengths(i) = baseMeasureLengths(i + 1)
            Next

            If randomBlocks Is Nothing Then Return
            For Each block As BmsRandomBlock In randomBlocks
                RemoveMeasure(block, measureIndex, lastMeasureIndex)
            Next
        End Sub

        Private Shared Sub RemoveMeasure(ByVal block As BmsRandomBlock,
                                         ByVal measureIndex As Integer,
                                         ByVal lastMeasureIndex As Integer)
            If block Is Nothing Then Return

            Dim values As New List(Of Integer)(block.MeasureLengthByValue.Keys)
            For Each value As Integer In values
                Dim source As Dictionary(Of Integer, Double) = block.GetMeasureLengthOverrides(value)
                Dim shifted As New Dictionary(Of Integer, Double)()
                Dim lastMeasureLength As Double = 0.0R
                Dim hasLastMeasureLength As Boolean = source.TryGetValue(lastMeasureIndex, lastMeasureLength)

                For Each pair As KeyValuePair(Of Integer, Double) In source
                    If pair.Key < measureIndex Then
                        shifted(pair.Key) = pair.Value
                    ElseIf pair.Key > measureIndex Then
                        shifted(pair.Key - 1) = pair.Value
                    End If
                Next

                If hasLastMeasureLength Then shifted(lastMeasureIndex) = lastMeasureLength
                block.SetMeasureLengthOverrides(value, shifted)
            Next
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
