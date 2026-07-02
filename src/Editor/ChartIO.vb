Imports nBMSC.Editor
Imports System.Text.Json

Partial Public Class MainWindow
    Private Const NBMSCFileSignature As Integer = &H534D426E
    Private Const NBMSCFileSuffixLegacy As Byte = &H43
    Private Const NBMSCFileSuffixRandom As Byte = &H44
    Private Const NBMSCFileSuffix As Byte = &H45

    Private Sub ReportLoadProgress(ByVal xProgress As fLoadFileProgress, ByVal xStatus As String, ByVal xPercent As Integer, Optional ByVal xForce As Boolean = False)
        If xProgress Is Nothing Then Return

        xProgress.ReportProgress(xStatus, xPercent, xForce)
    End Sub

    Private Sub CheckLoadCanceled(ByVal xProgress As fLoadFileProgress)
        If xProgress Is Nothing Then Return

        xProgress.CheckCanceled()
    End Sub

    Private Function PlayerIndexFromValue(ByVal value As Integer) As Integer
        Select Case value
            Case 1
                Return 0
            Case 3
                Return 1
            Case 2
                Return 2
        End Select

        Return -1
    End Function

    Private Function PlayerValueFromIndex(ByVal index As Integer) As Integer
        Select Case index
            Case 1
                Return 3
            Case 2
                Return 2
        End Select

        Return 1
    End Function

    Private Function PlayerIndexForSave() As Integer
        If CurrentMode = ChartMode.Key9 Then
            Return 1
        End If

        Return CHPlayer.SelectedIndex
    End Function

    Private Sub OpenBMS(ByVal xStrAll As String, Optional ByVal xPath As String = "", Optional ByVal xProgress As fLoadFileProgress = Nothing)
        KMouseOver = -1

        ReportLoadProgress(xProgress, "Preparing chart", 5, True)
        CheckLoadCanceled(xProgress)

        'Line feed validation: will remove some empty lines
        xStrAll = Replace(Replace(Replace(xStrAll, vbLf, vbCr), vbCr & vbCr, vbCr), vbCr, vbCrLf)

        Dim xStrLine() As String = Split(xStrAll, vbCrLf, , CompareMethod.Text)
        Dim xParsedRandom As BmsRandomParseResult = BmsRandomParser.Parse(xStrLine)
        Dim xTopLines() As String = xParsedRandom.TopLevelLines.ToArray()
        Dim xLineCount As Integer = Math.Max(1, xStrLine.Length)
        Dim xI1 As Integer
        Dim sLine As String
        Dim xExpansion As String = ""
        ReDim Notes(0)
        ReDim mColumn(999)
        ReDim hWAV(MaxDefinition)
        ReDim hBMP(MaxDefinition)
        ReDim hBPM(MaxDefinition)    'x10000
        ReDim hSTOP(MaxDefinition)
        ReDim hBMSCROLL(MaxDefinition)
        Me.InitializeNewBMS()
        Me.InitializeOpenBMS()
        ResetRandomState()
        SetUseBase62Definitions(ContainsBase62Definitions(xStrLine))

        With Notes(0)
            .ColumnIndex = niBPM
            .VPosition = -1
            '.LongNote = False
            '.Selected = False
            .Value = 1200000
            .RandomIndex = -1
            .RandomValue = 0
        End With

        'random, setRandom      0
        'endRandom              0
        'if             +1
        'else           0
        'endif          -1
        'switch, setSwitch      +1
        'case, skip, def        0
        'endSw                  -1
        Dim xStack As Integer = 0

        For xLineIndex As Integer = 0 To xTopLines.Length - 1
            If xLineIndex Mod 256 = 0 Then
                ReportLoadProgress(xProgress, "Reading headers", 10 + CInt((xLineIndex / xLineCount) * 25))
                CheckLoadCanceled(xProgress)
            End If

            sLine = xTopLines(xLineIndex)
            Dim sLineTrim As String = sLine.Trim
            If xStack > 0 Then GoTo Expansion

            If sLineTrim.StartsWith("#") And Mid(sLineTrim, 5, 3) = "02:" Then
                Dim xIndex As Integer = Val(Mid(sLineTrim, 2, 3))
                Dim xRatio As Double = Val(Mid(sLineTrim, 8))
                Dim xxD As Long = GetDenominator(xRatio)
                MeasureLength(xIndex) = xRatio * 192.0R
                LBeat.Items(xIndex) = Add3Zeros(xIndex) & ": " & xRatio & IIf(xxD > 10000, "", " ( " & CLng(xRatio * xxD) & " / " & xxD & " ) ")

            ElseIf sLineTrim.StartsWith("#WAV", StringComparison.CurrentCultureIgnoreCase) Then
                hWAV(DefinitionIndex(Mid(sLineTrim, Len("#WAV") + 1, 2))) = Mid(sLineTrim, Len("#WAV") + 4)

            ElseIf sLineTrim.StartsWith("#BMP", StringComparison.CurrentCultureIgnoreCase) Then
                hBMP(DefinitionIndex(Mid(sLineTrim, Len("#BMP") + 1, 2))) = Mid(sLineTrim, Len("#BMP") + 4)

            ElseIf sLineTrim.StartsWith("#BPM", StringComparison.CurrentCultureIgnoreCase) And Not Mid(sLineTrim, Len("#BPM") + 1, 1).Trim = "" Then  'If BPM##
                ' zdr: No limits on BPM editing.. they don't make much sense.
                hBPM(DefinitionIndex(Mid(sLineTrim, Len("#BPM") + 1, 2))) = Val(Mid(sLineTrim, Len("#BPM") + 4)) * 10000

                'No limits on STOPs either.
            ElseIf sLineTrim.StartsWith("#STOP", StringComparison.CurrentCultureIgnoreCase) Then
                hSTOP(DefinitionIndex(Mid(sLineTrim, Len("#STOP") + 1, 2))) = Val(Mid(sLineTrim, Len("#STOP") + 4)) * 10000

            ElseIf sLineTrim.StartsWith("#SCROLL", StringComparison.CurrentCultureIgnoreCase) Then
                hBMSCROLL(DefinitionIndex(Mid(sLineTrim, Len("#SCROLL") + 1, 2))) = Val(Mid(sLineTrim, Len("#SCROLL") + 4)) * 10000


            ElseIf sLineTrim.StartsWith("#TITLE", StringComparison.CurrentCultureIgnoreCase) Then
                THTitle.Text = Mid(sLineTrim, Len("#TITLE") + 1).Trim

            ElseIf sLineTrim.StartsWith("#ARTIST", StringComparison.CurrentCultureIgnoreCase) Then
                THArtist.Text = Mid(sLineTrim, Len("#ARTIST") + 1).Trim

            ElseIf sLineTrim.StartsWith("#GENRE", StringComparison.CurrentCultureIgnoreCase) Then
                THGenre.Text = Mid(sLineTrim, Len("#GENRE") + 1).Trim

            ElseIf sLineTrim.StartsWith("#BPM", StringComparison.CurrentCultureIgnoreCase) Then  'If BPM ####
                Notes(0).Value = Val(Mid(sLineTrim, Len("#BPM") + 1).Trim) * 10000
                THBPM.Value = Notes(0).Value / 10000

            ElseIf sLineTrim.StartsWith("#PLAYER", StringComparison.CurrentCultureIgnoreCase) Then
                Dim xInt As Integer = Val(Mid(sLineTrim, Len("#PLAYER") + 1).Trim)
                Dim xIndex As Integer = PlayerIndexFromValue(xInt)
                If xIndex >= 0 Then
                    CHPlayer.SelectedIndex = xIndex
                End If

            ElseIf sLineTrim.StartsWith("#RANK", StringComparison.CurrentCultureIgnoreCase) Then
                Dim xInt As Integer = Val(Mid(sLineTrim, Len("#RANK") + 1).Trim)
                If xInt >= 0 And xInt <= 4 Then _
                    CHRank.SelectedIndex = xInt

            ElseIf sLineTrim.StartsWith("#PLAYLEVEL", StringComparison.CurrentCultureIgnoreCase) Then
                THPlayLevel.Text = Mid(sLineTrim, Len("#PLAYLEVEL") + 1).Trim


            ElseIf sLineTrim.StartsWith("#SUBTITLE", StringComparison.CurrentCultureIgnoreCase) Then
                THSubTitle.Text = Mid(sLineTrim, Len("#SUBTITLE") + 1).Trim

            ElseIf sLineTrim.StartsWith("#SUBARTIST", StringComparison.CurrentCultureIgnoreCase) Then
                THSubArtist.Text = Mid(sLineTrim, Len("#SUBARTIST") + 1).Trim

            ElseIf sLineTrim.StartsWith("#STAGEFILE", StringComparison.CurrentCultureIgnoreCase) Then
                THStageFile.Text = Mid(sLineTrim, Len("#STAGEFILE") + 1).Trim

            ElseIf sLineTrim.StartsWith("#BANNER", StringComparison.CurrentCultureIgnoreCase) Then
                THBanner.Text = Mid(sLineTrim, Len("#BANNER") + 1).Trim

            ElseIf sLineTrim.StartsWith("#BACKBMP", StringComparison.CurrentCultureIgnoreCase) Then
                THBackBMP.Text = Mid(sLineTrim, Len("#BACKBMP") + 1).Trim

            ElseIf sLineTrim.StartsWith("#DIFFICULTY", StringComparison.CurrentCultureIgnoreCase) Then
                Try
                    CHDifficulty.SelectedIndex = Integer.Parse(Mid(sLineTrim, Len("#DIFFICULTY") + 1).Trim)
                Catch ex As Exception
                End Try

            ElseIf sLineTrim.StartsWith("#DEFEXRANK", StringComparison.CurrentCultureIgnoreCase) Then
                THExRank.Text = Mid(sLineTrim, Len("#DEFEXRANK") + 1).Trim

            ElseIf sLineTrim.StartsWith("#TOTAL", StringComparison.CurrentCultureIgnoreCase) Then
                Dim xStr As String = Mid(sLineTrim, Len("#TOTAL") + 1).Trim
                'If xStr.EndsWith("%") Then xStr = Mid(xStr, 1, Len(xStr) - 1)
                THTotal.Text = xStr

            ElseIf sLineTrim.StartsWith("#COMMENT", StringComparison.CurrentCultureIgnoreCase) Then
                Dim xStr As String = Mid(sLineTrim, Len("#COMMENT") + 1).Trim
                If xStr.StartsWith("""") Then xStr = Mid(xStr, 2)
                If xStr.EndsWith("""") Then xStr = Mid(xStr, 1, Len(xStr) - 1)
                THComment.Text = xStr

            ElseIf sLineTrim.StartsWith("#LNTYPE", StringComparison.CurrentCultureIgnoreCase) Then
                'THLnType.Text = Mid(sLineTrim, Len("#LNTYPE") + 1).Trim
                If Val(Mid(sLineTrim, Len("#LNTYPE") + 1).Trim) = 1 Then CHLnObj.SelectedIndex = 0

            ElseIf sLineTrim.StartsWith("#LNOBJ", StringComparison.CurrentCultureIgnoreCase) Then
                Dim xValue As Integer = DefinitionIndex(Mid(sLineTrim, Len("#LNOBJ") + 1).Trim)
                CHLnObj.SelectedIndex = xValue

                'TODO: LNOBJ value validation

                'ElseIf sLineTrim.StartsWith("#LNTYPE", StringComparison.CurrentCultureIgnoreCase) Then
                '    CAdLNTYPE.Checked = True
                '    If Mid(sLineTrim, 9) = "" Or Mid(sLineTrim, 9) = "1" Or Mid(sLineTrim, 9) = "01" Then CAdLNTYPEb.Text = "1"
                '    CAdLNTYPEb.Text = Mid(sLineTrim, 9)

            ElseIf sLineTrim.StartsWith("#PREVIEW", StringComparison.CurrentCultureIgnoreCase) Then
                THPreview.Text = Mid(sLineTrim, Len("#PREVIEW") + 1).Trim

            ElseIf sLineTrim.StartsWith("#LNMODE", StringComparison.CurrentCultureIgnoreCase) Then
                Dim xInt As Integer = Val(Mid(sLineTrim, Len("#LNMODE") + 1).Trim)
                If xInt >= 1 And xInt <= 3 Then _
                    CHLnmode.SelectedIndex = xInt

            ElseIf sLineTrim.StartsWith("#") And Mid(sLineTrim, 7, 1) = ":" Then   'If the line contains Ks
                Dim xIdentifier As String = Mid(sLineTrim, 5, 2)
                If BMSChannelToColumn(xIdentifier) = 0 Then GoTo AddExpansion

            Else
Expansion:      If sLineTrim.StartsWith("#IF", StringComparison.CurrentCultureIgnoreCase) Then
                    xStack += 1 : GoTo AddExpansion
                ElseIf sLineTrim.StartsWith("#ENDIF", StringComparison.CurrentCultureIgnoreCase) Then
                    xStack -= 1 : GoTo AddExpansion
                ElseIf sLineTrim.StartsWith("#SWITCH", StringComparison.CurrentCultureIgnoreCase) Then
                    xStack += 1 : GoTo AddExpansion
                ElseIf sLineTrim.StartsWith("#SETSWITCH", StringComparison.CurrentCultureIgnoreCase) Then
                    xStack += 1 : GoTo AddExpansion
                ElseIf sLineTrim.StartsWith("#ENDSW", StringComparison.CurrentCultureIgnoreCase) Then
                    xStack -= 1 : GoTo AddExpansion

                ElseIf sLineTrim.StartsWith("#") Then
AddExpansion:       xExpansion &= sLine & vbCrLf
                End If

            End If
        Next

        ReportLoadProgress(xProgress, "Updating measures", 35, True)
        CheckLoadCanceled(xProgress)
        UpdateMeasureBottom()
        CopyCurrentMeasureLengthToBase()

        xStack = 0
        Dim xNotes As New List(Of Note)(Math.Max(xStrLine.Length, 1))
        xNotes.Add(Notes(0))
        Dim xHasPlayableNotes As Boolean = False
        Dim xHas24KeyNotes As Boolean = False
        Dim xHas7KeyNotes As Boolean = False
        Dim xHas5KeyNotes As Boolean = False

        For xLineIndex As Integer = 0 To xTopLines.Length - 1
            If xLineIndex Mod 256 = 0 Then
                ReportLoadProgress(xProgress, "Reading notes", 35 + CInt((xLineIndex / xLineCount) * 50))
                CheckLoadCanceled(xProgress)
            End If

            sLine = xTopLines(xLineIndex)
            Dim sLineTrim As String = sLine.Trim
            If xStack > 0 Then Continue For

            If Not (sLineTrim.StartsWith("#") And Mid(sLineTrim, 7, 1) = ":") Then Continue For 'If the line contains Ks

            ' >> Measure =           Mid(sLine, 2, 3)
            ' >> Column Identifier = Mid(sLine, 5, 2)
            ' >> K =                 Mid(sLine, xI1, 2)
            Dim xMeasure As Integer = Val(Mid(sLineTrim, 2, 3))
            Dim Channel As String = Mid(sLineTrim, 5, 2)
            If BMSChannelToColumn(Channel) = 0 Then Continue For

            If Channel = "01" Then mColumn(xMeasure) += 1 'If the identifier is 01 then add a B column in that measure
            For xI1 = 8 To Len(sLineTrim) - 1 Step 2   'For all Ks within that line ( - 1 can be ommitted )
                If xI1 Mod 4096 = 0 Then
                    ReportLoadProgress(xProgress, "Reading notes", 35 + CInt((xLineIndex / xLineCount) * 50))
                    CheckLoadCanceled(xProgress)
                End If

                Dim xNoteValueText As String = Mid(sLineTrim, xI1, 2)
                If xNoteValueText = "00" Then Continue For 'If the K is not 00

                ChartModes.ObserveBmsChannel(Channel, xNoteValueText, xHasPlayableNotes, xHas24KeyNotes, xHas7KeyNotes, xHas5KeyNotes)

                Dim xNote As New Note

                With xNote
                    .ColumnIndex = BMSChannelToColumn(Channel) +
                                        IIf(Channel = "01", 1, 0) * (mColumn(xMeasure) - 1)
                    .LongNote = IsChannelLongNote(Channel)
                    .Hidden = IsChannelHidden(Channel)
                    .Landmine = IsChannelLandmine(Channel)
                    .Selected = False
                    .VPosition = MeasureBottom(xMeasure) + MeasureLength(xMeasure) * (xI1 / 2 - 4) / ((Len(sLineTrim) - 7) / 2)
                    .Value = DefinitionIndex(xNoteValueText) * 10000
                    .RandomIndex = -1
                    .RandomValue = 0

                    If Channel = "03" Then .Value = Convert.ToInt32(xNoteValueText, 16) * 10000
                    If Channel = "08" Then .Value = hBPM(DefinitionIndex(xNoteValueText))
                    If Channel = "09" Then .Value = hSTOP(DefinitionIndex(xNoteValueText))
                    If Channel = "SC" Then .Value = hBMSCROLL(DefinitionIndex(xNoteValueText))
                End With

                xNotes.Add(xNote)

            Next
        Next

        ReadRandomBlocks(xParsedRandom, xNotes, xExpansion, xHasPlayableNotes, xHas24KeyNotes, xHas7KeyNotes, xHas5KeyNotes)

        Notes = xNotes.ToArray()

        ReportLoadProgress(xProgress, "Applying chart settings", 86, True)
        CheckLoadCanceled(xProgress)
        If NTInput Then
            ReportLoadProgress(xProgress, "Converting long notes", 86, True)
            CheckLoadCanceled(xProgress)
            ConvertBMSE2NT()
        End If
        ReportLoadProgress(xProgress, "Applying chart mode", 87, True)
        CheckLoadCanceled(xProgress)
        SetChartMode(ChartModes.DetectFromBms(xPath, xHasPlayableNotes, xHas24KeyNotes, xHas7KeyNotes, xHas5KeyNotes), True, False)

        ReportLoadProgress(xProgress, "Refreshing definitions", 88, True)
        CheckLoadCanceled(xProgress)
        RefreshDefinitionLists()
        THLandMine.Text = hWAV(0)
        THMissBMP.Text = hBMP(0)

        TExpansion.Text = xExpansion
        ReportLoadProgress(xProgress, "Refreshing RANDOM panel", 89, True)
        CheckLoadCanceled(xProgress)
        RefreshRandomPanel()

        ReportLoadProgress(xProgress, "Sorting notes", 90, True)
        CheckLoadCanceled(xProgress)
        SortByVPositionQuick(0, UBound(Notes))
        UpdatePairing()

        ReportLoadProgress(xProgress, "Finalizing", 95, True)
        CheckLoadCanceled(xProgress)
        CalculateTotalPlayableNotes()
        CalculateGreatestVPosition()
        RefreshPanelAll()
        POStatusRefresh()
        ReportLoadProgress(xProgress, "Done", 100, True)
    End Sub

    Private Class RandomTimingDefinition
        Public Kind As String
        Public Index As Integer
        Public Value As Long
        Public Line As String
        Public Used As Boolean
    End Class

    Private Sub ResetRandomState()
        RandomBlocks.Clear()
        RandomCommonVisible = True
        SelectedRandomIndex = -1
        ActiveMeasureRandomIndex = -1
        ActiveMeasureRandomValue = 0
    End Sub

    Private Function JoinBmsLines(ByVal lines As IEnumerable(Of String)) As String
        Dim xLines As New List(Of String)(lines)
        If xLines.Count = 0 Then Return ""

        Return String.Join(vbCrLf, xLines.ToArray()) & vbCrLf
    End Function

    Private Sub AppendExpansionText(ByRef expansion As String, ByVal text As String)
        If text = "" Then Return
        expansion &= text
        If Not expansion.EndsWith(vbCrLf, StringComparison.Ordinal) Then expansion &= vbCrLf
    End Sub

    Private Function IsBmsDataLine(ByVal trimmedLine As String) As Boolean
        Return trimmedLine.StartsWith("#", StringComparison.CurrentCultureIgnoreCase) AndAlso Mid(trimmedLine, 7, 1) = ":"
    End Function

    Private Function IsSupportedBmsDataLine(ByVal trimmedLine As String) As Boolean
        If Not IsBmsDataLine(trimmedLine) Then Return False

        Return BMSChannelToColumn(Mid(trimmedLine, 5, 2)) <> 0
    End Function

    Private Function IsMeasureLengthLine(ByVal trimmedLine As String) As Boolean
        Return trimmedLine.StartsWith("#", StringComparison.CurrentCultureIgnoreCase) AndAlso Mid(trimmedLine, 5, 3) = "02:"
    End Function

    Private Function TryReadMeasureLengthLine(ByVal line As String,
                                              ByRef measureIndex As Integer,
                                              ByRef measureLength As Double) As Boolean
        Dim trimmed As String = line.Trim()
        If Not IsMeasureLengthLine(trimmed) Then Return False

        measureIndex = Val(Mid(trimmed, 2, 3))
        measureLength = Val(Mid(trimmed, 8)) * 192.0R
        Return measureIndex >= 0 AndAlso measureIndex <= 999 AndAlso measureLength > 0.0R
    End Function

    Private Function BranchRequiresTextOnly(ByVal lines As List(Of String)) As Boolean
        For Each line As String In lines
            Dim trimmed As String = line.Trim()
            If trimmed = "" Then Continue For

            If BmsRandomParser.IsCommand(trimmed, "RANDOM") OrElse
               BmsRandomParser.IsCommand(trimmed, "IF") OrElse
               BmsRandomParser.IsCommand(trimmed, "ENDIF") OrElse
               BmsRandomParser.IsCommand(trimmed, "ENDRANDOM") OrElse
               BmsRandomParser.IsCommand(trimmed, "ELSEIF") OrElse
               BmsRandomParser.IsCommand(trimmed, "ELSE") OrElse
               BmsRandomParser.IsCommand(trimmed, "SETRANDOM") OrElse
               BmsRandomParser.IsCommand(trimmed, "SWITCH") OrElse
               BmsRandomParser.IsCommand(trimmed, "SETSWITCH") OrElse
               BmsRandomParser.IsCommand(trimmed, "CASE") OrElse
               BmsRandomParser.IsCommand(trimmed, "DEF") OrElse
               BmsRandomParser.IsCommand(trimmed, "SKIP") OrElse
               BmsRandomParser.IsCommand(trimmed, "ENDSW") Then Return True

            If trimmed.StartsWith("#WAV", StringComparison.CurrentCultureIgnoreCase) Then Return True
            If trimmed.StartsWith("#BMP", StringComparison.CurrentCultureIgnoreCase) Then Return True
        Next

        Return False
    End Function

    Private Function TryReadTimingDefinition(ByVal line As String,
                                             ByRef kind As String,
                                             ByRef index As Integer,
                                             ByRef value As Long) As Boolean
        Dim trimmed As String = line.Trim()
        kind = ""
        index = 0
        value = 0

        If trimmed.StartsWith("#BPM", StringComparison.CurrentCultureIgnoreCase) AndAlso Not Mid(trimmed, Len("#BPM") + 1, 1).Trim() = "" Then
            kind = "BPM"
            index = DefinitionIndex(Mid(trimmed, Len("#BPM") + 1, 2))
            value = CLng(Val(Mid(trimmed, Len("#BPM") + 4)) * 10000)
            Return True
        End If

        If trimmed.StartsWith("#STOP", StringComparison.CurrentCultureIgnoreCase) Then
            kind = "STOP"
            index = DefinitionIndex(Mid(trimmed, Len("#STOP") + 1, 2))
            value = CLng(Val(Mid(trimmed, Len("#STOP") + 4)) * 10000)
            Return True
        End If

        If trimmed.StartsWith("#SCROLL", StringComparison.CurrentCultureIgnoreCase) Then
            kind = "SCROLL"
            index = DefinitionIndex(Mid(trimmed, Len("#SCROLL") + 1, 2))
            value = CLng(Val(Mid(trimmed, Len("#SCROLL") + 4)) * 10000)
            Return True
        End If

        Return False
    End Function

    Private Sub MarkTimingDefinitionUsed(ByVal definitions As List(Of RandomTimingDefinition), ByVal kind As String, ByVal index As Integer)
        If definitions Is Nothing Then Return

        For Each definition As RandomTimingDefinition In definitions
            If definition.Kind = kind AndAlso definition.Index = index Then definition.Used = True
        Next
    End Sub

    Private Function ResolveTimingValue(ByVal kind As String,
                                        ByVal labelText As String,
                                        ByVal localValues As Dictionary(Of Integer, Long),
                                        ByVal globalValues() As Long,
                                        ByVal definitions As List(Of RandomTimingDefinition)) As Long
        Dim index As Integer = DefinitionIndex(labelText)

        If localValues IsNot Nothing AndAlso localValues.ContainsKey(index) Then
            MarkTimingDefinitionUsed(definitions, kind, index)
            Return localValues(index)
        End If

        If index >= 0 AndAlso index <= UBound(globalValues) Then Return globalValues(index)

        Return 0
    End Function

    Private Sub ReadRandomBlocks(ByVal parsed As BmsRandomParseResult,
                                 ByVal xNotes As List(Of Note),
                                 ByRef xExpansion As String,
                                 ByRef xHasPlayableNotes As Boolean,
                                 ByRef xHas24KeyNotes As Boolean,
                                 ByRef xHas7KeyNotes As Boolean,
                                 ByRef xHas5KeyNotes As Boolean)
        For Each parsedBlock As BmsRandomParsedBlock In parsed.Blocks
            If parsedBlock.IsRawText Then
                AppendExpansionText(xExpansion, JoinBmsLines(parsedBlock.RawLines))
                Continue For
            End If

            Dim block As New BmsRandomBlock(parsedBlock.DefinitionValue)
            RandomBlocks.Add(block)
            Dim randomIndex As Integer = RandomBlocks.Count - 1

            For Each branch As BmsRandomParsedBranch In parsedBlock.Branches
                If BranchRequiresTextOnly(branch.Lines) Then
                    block.SetExtraText(branch.Value, JoinBmsLines(branch.Lines))
                Else
                    ReadRandomBranch(block, branch, randomIndex, xNotes, xHasPlayableNotes, xHas24KeyNotes, xHas7KeyNotes, xHas5KeyNotes)
                End If
            Next

            block.Normalize()
        Next
    End Sub

    Private Sub ReadRandomBranch(ByVal block As BmsRandomBlock,
                                 ByVal branch As BmsRandomParsedBranch,
                                 ByVal randomIndex As Integer,
                                 ByVal xNotes As List(Of Note),
                                 ByRef xHasPlayableNotes As Boolean,
                                 ByRef xHas24KeyNotes As Boolean,
                                 ByRef xHas7KeyNotes As Boolean,
                                 ByRef xHas5KeyNotes As Boolean)
        Dim localBPM As New Dictionary(Of Integer, Long)()
        Dim localSTOP As New Dictionary(Of Integer, Long)()
        Dim localSCROLL As New Dictionary(Of Integer, Long)()
        Dim timingDefinitions As New List(Of RandomTimingDefinition)()
        Dim noteLines As New List(Of String)()
        Dim extraLines As New List(Of String)()

        For Each line As String In branch.Lines
            Dim trimmed As String = line.Trim()
            Dim kind As String = ""
            Dim index As Integer = 0
            Dim value As Long = 0
            Dim measureIndex As Integer = 0
            Dim measureLength As Double = 0.0R

            If TryReadMeasureLengthLine(line, measureIndex, measureLength) Then
                block.SetMeasureLength(branch.Value, measureIndex, measureLength)
            ElseIf TryReadTimingDefinition(line, kind, index, value) Then
                timingDefinitions.Add(New RandomTimingDefinition With {.Kind = kind, .Index = index, .Value = value, .Line = line})
                If kind = "BPM" Then localBPM(index) = value
                If kind = "STOP" Then localSTOP(index) = value
                If kind = "SCROLL" Then localSCROLL(index) = value
            ElseIf IsSupportedBmsDataLine(trimmed) Then
                noteLines.Add(line)
            ElseIf IsBmsDataLine(trimmed) Then
                extraLines.Add(line)
            ElseIf trimmed.StartsWith("#", StringComparison.CurrentCultureIgnoreCase) Then
                extraLines.Add(line)
            End If
        Next

        Dim baseMeasureLengths() As Double = CopyMeasureLengthArray(MeasureLength)
        Dim branchMeasureLengths() As Double = EffectiveMeasureLengthForRandom(randomIndex, branch.Value)
        Dim branchNotes As New List(Of Note)()

        SetMeasureLengthForSerialization(branchMeasureLengths)
        Try
            ReadNoteLines(noteLines,
                          randomIndex,
                          branch.Value,
                          localBPM,
                          localSTOP,
                          localSCROLL,
                          timingDefinitions,
                          branchNotes,
                          xHasPlayableNotes,
                          xHas24KeyNotes,
                          xHas7KeyNotes,
                          xHas5KeyNotes)
        Finally
            SetMeasureLengthForSerialization(baseMeasureLengths)
        End Try

        xNotes.AddRange(ConvertNotesToMeasureMap(branchNotes.ToArray(), branchMeasureLengths, baseMeasureLengths))

        For Each definition As RandomTimingDefinition In timingDefinitions
            If Not definition.Used Then extraLines.Add(definition.Line)
        Next

        block.SetExtraText(branch.Value, JoinBmsLines(extraLines))
    End Sub

    Private Sub ReadNoteLines(ByVal lines As List(Of String),
                              ByVal randomIndex As Integer,
                              ByVal randomValue As Integer,
                              ByVal localBPM As Dictionary(Of Integer, Long),
                              ByVal localSTOP As Dictionary(Of Integer, Long),
                              ByVal localSCROLL As Dictionary(Of Integer, Long),
                              ByVal timingDefinitions As List(Of RandomTimingDefinition),
                              ByVal xNotes As List(Of Note),
                              ByRef xHasPlayableNotes As Boolean,
                              ByRef xHas24KeyNotes As Boolean,
                              ByRef xHas7KeyNotes As Boolean,
                              ByRef xHas5KeyNotes As Boolean)
        For Each line As String In lines
            Dim sLineTrim As String = line.Trim()
            If Not IsBmsDataLine(sLineTrim) Then Continue For

            Dim xMeasure As Integer = Val(Mid(sLineTrim, 2, 3))
            Dim channel As String = Mid(sLineTrim, 5, 2)
            If BMSChannelToColumn(channel) = 0 Then Continue For

            If channel = "01" Then mColumn(xMeasure) += 1
            For xI1 As Integer = 8 To Len(sLineTrim) - 1 Step 2
                Dim xNoteValueText As String = Mid(sLineTrim, xI1, 2)
                If xNoteValueText = "00" Then Continue For

                ChartModes.ObserveBmsChannel(channel, xNoteValueText, xHasPlayableNotes, xHas24KeyNotes, xHas7KeyNotes, xHas5KeyNotes)

                Dim xNote As New Note
                With xNote
                    .ColumnIndex = BMSChannelToColumn(channel) +
                                        IIf(channel = "01", 1, 0) * (mColumn(xMeasure) - 1)
                    .LongNote = IsChannelLongNote(channel)
                    .Hidden = IsChannelHidden(channel)
                    .Landmine = IsChannelLandmine(channel)
                    .Selected = False
                    .VPosition = MeasureBottom(xMeasure) + MeasureLength(xMeasure) * (xI1 / 2 - 4) / ((Len(sLineTrim) - 7) / 2)
                    .Value = DefinitionIndex(xNoteValueText) * 10000
                    .RandomIndex = randomIndex
                    .RandomValue = If(randomIndex < 0, 0, randomValue)

                    If channel = "03" Then .Value = Convert.ToInt32(xNoteValueText, 16) * 10000
                    If channel = "08" Then .Value = ResolveTimingValue("BPM", xNoteValueText, localBPM, hBPM, timingDefinitions)
                    If channel = "09" Then .Value = ResolveTimingValue("STOP", xNoteValueText, localSTOP, hSTOP, timingDefinitions)
                    If channel = "SC" Then .Value = ResolveTimingValue("SCROLL", xNoteValueText, localSCROLL, hBMSCROLL, timingDefinitions)
                End With

                xNotes.Add(xNote)
            Next
        Next
    End Sub

    ReadOnly BMSChannelList() As String = {"01", "03", "04", "06", "07", "08", "09",
                                       "11", "12", "13", "14", "15", "16", "17", "18", "19", "1A", "1B", "1C", "1D", "1E", "1F", "1G", "1H", "1I", "1J", "1K", "1L", "1M", "1N", "1O", "1P", "1Q",
                                       "21", "22", "23", "24", "25", "26", "27", "28", "29", "2A", "2B", "2C", "2D", "2E", "2F", "2G", "2H", "2I", "2J", "2K", "2L", "2M", "2N", "2O", "2P", "2Q",
                                       "31", "32", "33", "34", "35", "36", "37", "38", "39", "3A", "3B", "3C", "3D", "3E", "3F", "3G", "3H", "3I", "3J", "3K", "3L", "3M", "3N", "3O", "3P", "3Q",
                                       "41", "42", "43", "44", "45", "46", "47", "48", "49", "4A", "4B", "4C", "4D", "4E", "4F", "4G", "4H", "4I", "4J", "4K", "4L", "4M", "4N", "4O", "4P", "4Q",
                                       "51", "52", "53", "54", "55", "56", "57", "58", "59", "5A", "5B", "5C", "5D", "5E", "5F", "5G", "5H", "5I", "5J", "5K", "5L", "5M", "5N", "5O", "5P", "5Q",
                                       "61", "62", "63", "64", "65", "66", "67", "68", "69", "6A", "6B", "6C", "6D", "6E", "6F", "6G", "6H", "6I", "6J", "6K", "6L", "6M", "6N", "6O", "6P", "6Q",
                                       "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "DA", "DB", "DC", "DD", "DE", "DF", "DG", "DH", "DI", "DJ", "DK", "DL", "DM", "DN", "DO", "DP", "DQ",
                                       "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9", "EA", "EB", "EC", "ED", "EE", "EF", "EG", "EH", "EI", "EJ", "EK", "EL", "EM", "EN", "EO", "EP", "EQ",
                                       "SC"}
    ' 71 through 89 are reserved
    '"71", "72", "73", "74", "75", "76", "78", "79",
    '"81", "82", "83", "84", "85", "86", "88", "89",


    Private Function SaveBMS() As String
        StoreRandomExtraText()
        CalculateGreatestVPosition()
        SortByVPositionInsertion()
        UpdatePairing()
        Dim hasOverlapping As Boolean = False
        Dim maxBPMDefinitions As Integer = 0
        Dim maxSTOPDefinitions As Integer = 0
        Dim maxSCROLLDefinitions As Integer = 0

        ' We regenerate these when traversing the bms event list.
        ReDim hBPM(0)
        ReDim hSTOP(0)
        ReDim hBMSCROLL(0)

        Dim xNTInput As Boolean = NTInput
        Dim xKBackUp() As Note = Notes
        If xNTInput Then
            NTInput = False
            ConvertNT2BMSE()
        End If

        Dim xStrCommon As String = GenerateBmsDataForNotesInMeasureMap(NotesForRandomLayer(-1, 0),
                                                                       CopyMeasureLengthArray(BaseMeasureLength),
                                                                       True,
                                                                       hasOverlapping)
        maxBPMDefinitions = Math.Max(maxBPMDefinitions, UBound(hBPM))
        maxSTOPDefinitions = Math.Max(maxSTOPDefinitions, UBound(hSTOP))
        maxSCROLLDefinitions = Math.Max(maxSCROLLDefinitions, UBound(hBMSCROLL))

        Dim commonBPM() As Long = CType(hBPM.Clone(), Long())
        Dim commonSTOP() As Long = CType(hSTOP.Clone(), Long())
        Dim commonSCROLL() As Long = CType(hBMSCROLL.Clone(), Long())

        Dim xStrRandom As String = GenerateRandomBlocksForSave(hasOverlapping, maxBPMDefinitions, maxSTOPDefinitions, maxSCROLLDefinitions)

        ' Warn about 255 limit if neccesary.
        If hasOverlapping Then MsgBox(Strings.Messages.SaveWarning & vbCrLf &
                                                          Strings.Messages.NoteOverlapError & vbCrLf &
                                                Strings.Messages.SavedFileWillContainErrors, MsgBoxStyle.Exclamation)
        If maxBPMDefinitions > DefinitionDisplayMax() Then MsgBox(Strings.Messages.SaveWarning & vbCrLf &
                                                          Strings.Messages.BPMOverflowError & maxBPMDefinitions & " > " & DefinitionDisplayMax() & vbCrLf &
                                                Strings.Messages.SavedFileWillContainErrors, MsgBoxStyle.Exclamation)
        If maxSTOPDefinitions > DefinitionDisplayMax() Then MsgBox(Strings.Messages.SaveWarning & vbCrLf &
                                                           Strings.Messages.STOPOverflowError & maxSTOPDefinitions & " > " & DefinitionDisplayMax() & vbCrLf &
                                                  Strings.Messages.SavedFileWillContainErrors, MsgBoxStyle.Exclamation)
        If maxSCROLLDefinitions > MaxDefinition Then MsgBox(Strings.Messages.SaveWarning & vbCrLf &
                                           Strings.Messages.SCROLLOverflowError & maxSCROLLDefinitions & " > " & MaxDefinition & vbCrLf &
                                         Strings.Messages.SavedFileWillContainErrors, MsgBoxStyle.Exclamation)

        ' Add expansion text
        Dim xStrExp As String = vbCrLf & "*---------------------- EXPANSION FIELD" & vbCrLf & TExpansion.Text & vbCrLf & vbCrLf
        If TExpansion.Text = "" Then xStrExp = ""

        ' Output main data field.
        Dim xStrMain As String = "*---------------------- MAIN DATA FIELD" & vbCrLf & vbCrLf & xStrCommon & xStrRandom & vbCrLf

        If xNTInput Then
            Notes = xKBackUp
            NTInput = True
        End If

        hBPM = commonBPM
        hSTOP = commonSTOP
        hBMSCROLL = commonSCROLL

        ' Generate headers now, since we have the unique BPM/STOP/etc declarations.
        Dim xStrHeader As String = GenerateHeaderMeta()
        xStrHeader &= GenerateHeaderIndexedData()

        Dim xStrAll As String = xStrHeader & vbCrLf & xStrExp & vbCrLf & xStrMain
        Return xStrAll
    End Function

    Private Function GenerateHeaderMeta() As String
        Dim xStrHeader As String = vbCrLf & "*---------------------- HEADER FIELD" & vbCrLf & vbCrLf
        xStrHeader &= "#PLAYER " & PlayerValueFromIndex(PlayerIndexForSave()) & vbCrLf
        xStrHeader &= "#GENRE " & THGenre.Text & vbCrLf
        xStrHeader &= "#TITLE " & THTitle.Text & vbCrLf
        xStrHeader &= "#ARTIST " & THArtist.Text & vbCrLf
        xStrHeader &= "#BPM " & WriteDecimalWithDot(Notes(0).Value / 10000) & vbCrLf
        xStrHeader &= "#PLAYLEVEL " & THPlayLevel.Text & vbCrLf
        xStrHeader &= "#RANK " & CHRank.SelectedIndex & vbCrLf
        xStrHeader &= vbCrLf
        If THSubTitle.Text <> "" Then xStrHeader &= "#SUBTITLE " & THSubTitle.Text & vbCrLf
        If THSubArtist.Text <> "" Then xStrHeader &= "#SUBARTIST " & THSubArtist.Text & vbCrLf
        If THStageFile.Text <> "" Then xStrHeader &= "#STAGEFILE " & THStageFile.Text & vbCrLf
        If THBanner.Text <> "" Then xStrHeader &= "#BANNER " & THBanner.Text & vbCrLf
        If THBackBMP.Text <> "" Then xStrHeader &= "#BACKBMP " & THBackBMP.Text & vbCrLf
        xStrHeader &= vbCrLf
        If CHDifficulty.SelectedIndex Then xStrHeader &= "#DIFFICULTY " & CHDifficulty.SelectedIndex & vbCrLf
        If THExRank.Text <> "" Then xStrHeader &= "#DEFEXRANK " & THExRank.Text & vbCrLf
        If THTotal.Text <> "" Then xStrHeader &= "#TOTAL " & THTotal.Text & vbCrLf
        If THComment.Text <> "" Then xStrHeader &= "#COMMENT """ & THComment.Text & """" & vbCrLf
        'If THLnType.Text <> "" Then xStrHeader &= "#LNTYPE " & THLnType.Text & vbCrLf
        If CHLnObj.SelectedIndex > 0 Then xStrHeader &= "#LNOBJ " & DefinitionLabel(CHLnObj.SelectedIndex) & vbCrLf _
                                     Else xStrHeader &= "#LNTYPE 1" & vbCrLf
        If THPreview.Text <> "" Then xStrHeader &= "#PREVIEW " & THPreview.Text & vbCrLf
        If CHLnmode.SelectedIndex > 0 Then xStrHeader &= "#LNMODE " & CHLnmode.SelectedIndex & vbCrLf
        xStrHeader &= vbCrLf
        Return xStrHeader
    End Function

    Private Function GenerateHeaderIndexedData() As String
        Dim xStrHeader As String = ""

        For i = 0 To UBound(hWAV)
            If Not hWAV(i) = "" Then xStrHeader &= "#WAV" & DefinitionLabel(i) &
                                                    " " & hWAV(i) & vbCrLf
        Next
        For i = 0 To UBound(hBMP)
            If Not hBMP(i) = "" Then xStrHeader &= "#BMP" & DefinitionLabel(i) &
                                                    " " & hBMP(i) & vbCrLf
        Next
        For i = 1 To UBound(hBPM)
            xStrHeader &= "#BPM" &
            DefinitionLabel(i) &
            " " & WriteDecimalWithDot(hBPM(i) / 10000) & vbCrLf
        Next
        For i = 1 To UBound(hSTOP)
            xStrHeader &= "#STOP" &
                DefinitionLabel(i) &
                " " & WriteDecimalWithDot(hSTOP(i) / 10000) & vbCrLf
        Next
        For i = 1 To UBound(hBMSCROLL)
            xStrHeader &= "#SCROLL" &
                DefinitionLabel(i) & " " & WriteDecimalWithDot(hBMSCROLL(i) / 10000) & vbCrLf
        Next

        Return xStrHeader
    End Function

    Private Function GenerateTimingIndexedData(Optional ByVal bpmStartIndex As Integer = 1,
                                               Optional ByVal stopStartIndex As Integer = 1,
                                               Optional ByVal scrollStartIndex As Integer = 1) As String
        Dim xStrHeader As String = ""

        For i = Math.Max(1, bpmStartIndex) To UBound(hBPM)
            If hBPM(i) = Long.MinValue Then Continue For
            xStrHeader &= "#BPM" &
            DefinitionLabel(i) &
            " " & WriteDecimalWithDot(hBPM(i) / 10000) & vbCrLf
        Next
        For i = Math.Max(1, stopStartIndex) To UBound(hSTOP)
            If hSTOP(i) = Long.MinValue Then Continue For
            xStrHeader &= "#STOP" &
                DefinitionLabel(i) &
                " " & WriteDecimalWithDot(hSTOP(i) / 10000) & vbCrLf
        Next
        For i = Math.Max(1, scrollStartIndex) To UBound(hBMSCROLL)
            If hBMSCROLL(i) = Long.MinValue Then Continue For
            xStrHeader &= "#SCROLL" &
                DefinitionLabel(i) & " " & WriteDecimalWithDot(hBMSCROLL(i) / 10000) & vbCrLf
        Next

        Return xStrHeader
    End Function

    Private Sub EnsureTimingDefinitionSlot(ByRef values() As Long, ByVal index As Integer)
        If index <= UBound(values) Then Return

        Dim oldUpper As Integer = UBound(values)
        ReDim Preserve values(index)
        For i As Integer = Math.Max(1, oldUpper + 1) To index
            values(i) = Long.MinValue
        Next
    End Sub

    Private Sub ReserveTimingDefinitionsFromExtra(ByVal extraText As String,
                                                  ByRef bpmReservedMax As Integer,
                                                  ByRef stopReservedMax As Integer,
                                                  ByRef scrollReservedMax As Integer)
        bpmReservedMax = 0
        stopReservedMax = 0
        scrollReservedMax = 0
        If extraText = "" Then Return

        Dim xLines() As String = Split(Replace(extraText, vbLf, vbCr), vbCr)
        For Each line As String In xLines
            Dim kind As String = ""
            Dim index As Integer = 0
            Dim value As Long = 0
            If Not TryReadTimingDefinition(line, kind, index, value) Then Continue For

            If kind = "BPM" Then
                EnsureTimingDefinitionSlot(hBPM, index)
                hBPM(index) = value
                bpmReservedMax = Math.Max(bpmReservedMax, index)
            ElseIf kind = "STOP" Then
                EnsureTimingDefinitionSlot(hSTOP, index)
                hSTOP(index) = value
                stopReservedMax = Math.Max(stopReservedMax, index)
            ElseIf kind = "SCROLL" Then
                EnsureTimingDefinitionSlot(hBMSCROLL, index)
                hBMSCROLL(index) = value
                scrollReservedMax = Math.Max(scrollReservedMax, index)
            End If
        Next
    End Sub

    Private Function NotesForRandomLayer(ByVal randomIndex As Integer, ByVal randomValue As Integer) As Note()
        Dim xNotes As New List(Of Note)()
        xNotes.Add(Notes(0))

        For i As Integer = 1 To UBound(Notes)
            If IsSameRandomOwner(Notes(i), randomIndex, randomValue) Then xNotes.Add(Notes(i))
        Next

        Return xNotes.ToArray()
    End Function

    Private Function NotesForRandomExport() As Note()
        Dim xNotes As New List(Of Note)()
        xNotes.Add(Notes(0))

        For i As Integer = 1 To UBound(Notes)
            If Notes(i).RandomIndex < 0 Then
                xNotes.Add(Notes(i))
            ElseIf IsValidRandomIndex(Notes(i).RandomIndex) Then
                Dim block As BmsRandomBlock = RandomBlocks(Notes(i).RandomIndex)
                block.Normalize()
                If Notes(i).RandomValue = block.CurrentValue Then xNotes.Add(Notes(i))
            End If
        Next

        Return xNotes.ToArray()
    End Function

    Private Function GenerateRandomBlocksForSave(ByRef hasOverlapping As Boolean,
                                                 ByRef maxBPMDefinitions As Integer,
                                                 ByRef maxSTOPDefinitions As Integer,
                                                 ByRef maxSCROLLDefinitions As Integer) As String
        Dim ret As String = ""

        For randomIndex As Integer = 0 To RandomBlocks.Count - 1
            Dim block As BmsRandomBlock = RandomBlocks(randomIndex)
            block.Normalize()

            ret &= vbCrLf & "#RANDOM " & block.DefinitionValue.ToString() & vbCrLf

            For value As Integer = 1 To block.DefinitionValue
                ReDim hBPM(0)
                ReDim hSTOP(0)
                ReDim hBMSCROLL(0)

                Dim extraText As String = block.GetExtraText(value)
                Dim reservedBPM As Integer = 0
                Dim reservedSTOP As Integer = 0
                Dim reservedSCROLL As Integer = 0
                ReserveTimingDefinitionsFromExtra(extraText, reservedBPM, reservedSTOP, reservedSCROLL)

                Dim branchMeasureLengths() As Double = EffectiveMeasureLengthForRandom(randomIndex, value)
                Dim branchData As String = GenerateBmsDataForNotesInMeasureMap(NotesForRandomLayer(randomIndex, value),
                                                                               branchMeasureLengths,
                                                                               False,
                                                                               hasOverlapping)
                Dim branchMeasureLengthData As String = GenerateRandomBranchMeasureLengthData(block, value)
                Dim branchTimingDefinitions As String = GenerateTimingIndexedData(reservedBPM + 1, reservedSTOP + 1, reservedSCROLL + 1)
                maxBPMDefinitions = Math.Max(maxBPMDefinitions, UBound(hBPM))
                maxSTOPDefinitions = Math.Max(maxSTOPDefinitions, UBound(hSTOP))
                maxSCROLLDefinitions = Math.Max(maxSCROLLDefinitions, UBound(hBMSCROLL))

                ret &= "#IF " & value.ToString() & vbCrLf
                ret &= branchMeasureLengthData
                ret &= extraText
                If extraText <> "" AndAlso Not ret.EndsWith(vbCrLf, StringComparison.Ordinal) Then ret &= vbCrLf
                ret &= branchTimingDefinitions
                ret &= branchData
                ret &= "#ENDIF" & vbCrLf
            Next

            ret &= "#ENDRANDOM" & vbCrLf
        Next

        Return ret
    End Function

    Private Function GenerateBmsDataForNotes(ByVal sourceNotes() As Note,
                                             ByVal includeMeasureLength As Boolean,
                                             ByRef hasOverlapping As Boolean) As String
        Dim greatestSourceVPosition As Double = 0.0R
        For i As Integer = 1 To UBound(sourceNotes)
            greatestSourceVPosition = Math.Max(greatestSourceVPosition, sourceNotes(i).VPosition)
        Next

        Dim xStrMeasure(MeasureAtDisplacement(greatestSourceVPosition) + 1) As String
        Dim xprevNotes(-1) As Note

        For measureIndex As Integer = 0 To UBound(xStrMeasure)
            xStrMeasure(measureIndex) = vbCrLf

            If includeMeasureLength AndAlso MeasureLength(measureIndex) <> 192.0R Then
                Dim consistentDecimalStr = WriteDecimalWithDot(MeasureLength(measureIndex) / 192.0R)
                xStrMeasure(measureIndex) &= "#" & Add3Zeros(measureIndex) & "02:" & consistentDecimalStr & vbCrLf
            End If

            Dim lowerLimit As Integer = Nothing
            Dim upperLimit As Integer = Nothing
            GetMeasureLimits(sourceNotes, measureIndex, lowerLimit, upperLimit)

            If upperLimit - lowerLimit = 0 Then Continue For

            Dim xUPrevText As Integer = UBound(xprevNotes)
            Dim NotesInMeasure(upperLimit - lowerLimit + xUPrevText) As Note

            For i = 0 To xUPrevText
                NotesInMeasure(i) = xprevNotes(i)
            Next

            For i = lowerLimit To upperLimit - 1
                NotesInMeasure(i - lowerLimit + xprevNotes.Length) = sourceNotes(i)
            Next

            Dim greatestColumn = 0
            For Each tempNote As Note In NotesInMeasure
                greatestColumn = Math.Max(tempNote.ColumnIndex, greatestColumn)
            Next

            ReDim xprevNotes(-1)
            xStrMeasure(measureIndex) &= GenerateBackgroundTracks(measureIndex, hasOverlapping, NotesInMeasure, greatestColumn, xprevNotes)
            xStrMeasure(measureIndex) &= GenerateKeyTracks(measureIndex, hasOverlapping, NotesInMeasure, xprevNotes)
        Next

        Return Join(xStrMeasure, "")
    End Function

    Private Function GenerateBmsDataForNotesInMeasureMap(ByVal sourceNotes() As Note,
                                                         ByVal targetMeasureLengths() As Double,
                                                         ByVal includeMeasureLength As Boolean,
                                                         ByRef hasOverlapping As Boolean) As String
        Dim xSavedMeasureLengths() As Double = CopyMeasureLengthArray(MeasureLength)
        Dim xConvertedNotes() As Note = ConvertNotesToMeasureMap(sourceNotes, xSavedMeasureLengths, targetMeasureLengths)

        SetMeasureLengthForSerialization(targetMeasureLengths)
        Try
            Return GenerateBmsDataForNotes(xConvertedNotes, includeMeasureLength, hasOverlapping)
        Finally
            SetMeasureLengthForSerialization(xSavedMeasureLengths)
        End Try
    End Function

    Private Sub SetMeasureLengthForSerialization(ByVal lengths() As Double)
        For i As Integer = 0 To 999
            MeasureLength(i) = If(lengths IsNot Nothing AndAlso i <= UBound(lengths) AndAlso lengths(i) > 0.0R, lengths(i), 192.0R)
        Next

        UpdateMeasureBottom()
    End Sub

    Private Function GenerateRandomBranchMeasureLengthData(ByVal block As BmsRandomBlock, ByVal value As Integer) As String
        Dim xOverrides As Dictionary(Of Integer, Double) = block.GetMeasureLengthOverrides(value)
        If xOverrides.Count = 0 Then Return ""

        Dim xKeys As New List(Of Integer)(xOverrides.Keys)
        xKeys.Sort()

        Dim ret As String = ""
        For Each measureIndex As Integer In xKeys
            If measureIndex < 0 OrElse measureIndex > 999 Then Continue For
            If xOverrides(measureIndex) <= 0.0R Then Continue For

            ret &= "#" & Add3Zeros(measureIndex) & "02:" & WriteDecimalWithDot(xOverrides(measureIndex) / 192.0R) & vbCrLf
        Next

        Return ret
    End Function

    Private Sub GetMeasureLimits(MeasureIndex As Integer, ByRef LowerLimit As Integer, ByRef UpperLimit As Integer)
        GetMeasureLimits(Notes, MeasureIndex, LowerLimit, UpperLimit)
    End Sub

    Private Sub GetMeasureLimits(SourceNotes() As Note, MeasureIndex As Integer, ByRef LowerLimit As Integer, ByRef UpperLimit As Integer)
        Dim NoteCount = UBound(SourceNotes)
        LowerLimit = 0

        For i = 1 To NoteCount  'Collect Ks in the same measure
            If MeasureAtDisplacement(SourceNotes(i).VPosition) >= MeasureIndex Then
                LowerLimit = i
                Exit For
            End If 'Lower limit found
        Next

        UpperLimit = 0

        For i = LowerLimit To NoteCount
            If MeasureAtDisplacement(SourceNotes(i).VPosition) > MeasureIndex Then
                UpperLimit = i
                Exit For 'Upper limit found
            End If
        Next

        If UpperLimit < LowerLimit Then UpperLimit = NoteCount + 1
    End Sub

    Private Function GenerateKeyTracks(MeasureIndex As Integer, ByRef hasOverlapping As Boolean, NotesInMeasure() As Note, ByRef xprevNotes() As Note) As String
        Dim CurrentBMSChannel As String
        Dim Ret As String = ""

        For Each CurrentBMSChannel In BMSChannelList 'Start rendering other notes
            Dim relativeMeasurePos(-1) 'Ks in the same column
            Dim NoteStrings(-1)      'Ks in the same column

            ' Background tracks take care of this.
            If CurrentBMSChannel = "01" Then Continue For


            For NoteIndex = 0 To UBound(NotesInMeasure) 'Find Ks in the same column (xI4 is TK index)

                Dim currentNote As Note = NotesInMeasure(NoteIndex)
                If GetBMSChannelBy(currentNote) = CurrentBMSChannel Then

                    ReDim Preserve relativeMeasurePos(UBound(relativeMeasurePos) + 1)
                    ReDim Preserve NoteStrings(UBound(NoteStrings) + 1)
                    relativeMeasurePos(UBound(relativeMeasurePos)) = currentNote.VPosition - MeasureBottom(MeasureAtDisplacement(currentNote.VPosition))
                    If relativeMeasurePos(UBound(relativeMeasurePos)) < 0 Then relativeMeasurePos(UBound(relativeMeasurePos)) = 0

                    If CurrentBMSChannel = "03" Then 'If integer bpm
                        NoteStrings(UBound(NoteStrings)) = Mid("0" & Hex(currentNote.Value \ 10000), Len(Hex(currentNote.Value \ 10000)))
                    ElseIf CurrentBMSChannel = "08" Then 'If bpm requires declaration
                        Dim BpmIndex
                        For BpmIndex = 1 To UBound(hBPM) ' find BPM value in existing array
                            If currentNote.Value = hBPM(BpmIndex) Then Exit For
                        Next
                        If BpmIndex > UBound(hBPM) Then ' Didn't find it, add it
                            ReDim Preserve hBPM(UBound(hBPM) + 1)
                            hBPM(UBound(hBPM)) = currentNote.Value
                        End If
                        NoteStrings(UBound(NoteStrings)) = DefinitionLabel(BpmIndex)
                    ElseIf CurrentBMSChannel = "09" Then 'If STOP
                        Dim StopIndex
                        For StopIndex = 1 To UBound(hSTOP) ' find STOP value in existing array
                            If currentNote.Value = hSTOP(StopIndex) Then Exit For
                        Next

                        If StopIndex > UBound(hSTOP) Then ' Didn't find it, add it
                            ReDim Preserve hSTOP(UBound(hSTOP) + 1)
                            hSTOP(UBound(hSTOP)) = currentNote.Value
                        End If
                        NoteStrings(UBound(NoteStrings)) = DefinitionLabel(StopIndex)
                    ElseIf CurrentBMSChannel = "SC" Then 'If SCROLL
                        Dim ScrollIndex
                        For ScrollIndex = 1 To UBound(hBMSCROLL) ' find SCROLL value in existing array
                            If currentNote.Value = hBMSCROLL(ScrollIndex) Then Exit For
                        Next

                        If ScrollIndex > UBound(hBMSCROLL) Then ' Didn't find it, add it
                            ReDim Preserve hBMSCROLL(UBound(hBMSCROLL) + 1)
                            hBMSCROLL(UBound(hBMSCROLL)) = currentNote.Value
                        End If
                        NoteStrings(UBound(NoteStrings)) = DefinitionLabel(ScrollIndex)
                    Else
                        NoteStrings(UBound(NoteStrings)) = DefinitionLabel(currentNote.Value \ 10000)
                    End If
                End If
            Next

            If relativeMeasurePos.Length = 0 Then Continue For

            Dim xGCD As Double = MeasureLength(MeasureIndex)
            For i = 0 To UBound(relativeMeasurePos)        'find greatest common divisor
                If relativeMeasurePos(i) > 0 Then xGCD = GCD(xGCD, relativeMeasurePos(i))
            Next

            Dim xStrKey() As String
            ReDim xStrKey(CInt(MeasureLength(MeasureIndex) / xGCD) - 1)
            For i = 0 To UBound(xStrKey)           'assign 00 to all keys
                xStrKey(i) = "00"
            Next

            For i = 0 To UBound(relativeMeasurePos)        'assign K texts
                If CInt(relativeMeasurePos(i) / xGCD) > UBound(xStrKey) Then
                    ReDim Preserve xprevNotes(UBound(xprevNotes) + 1)
                    With xprevNotes(UBound(xprevNotes))
                        .ColumnIndex = BMSChannelToColumn(BMSChannelList(CurrentBMSChannel))
                        .LongNote = IsChannelLongNote(BMSChannelList(CurrentBMSChannel))
                        .Hidden = IsChannelHidden(BMSChannelList(CurrentBMSChannel))
                        .VPosition = MeasureBottom(MeasureIndex)
                        .Value = DefinitionIndex(NoteStrings(i))
                    End With
                    If BMSChannelList(CurrentBMSChannel) = "08" Then _
                        xprevNotes(UBound(xprevNotes)).Value = hBPM(DefinitionIndex(NoteStrings(i)))
                    If BMSChannelList(CurrentBMSChannel) = "09" Then _
                        xprevNotes(UBound(xprevNotes)).Value = hSTOP(DefinitionIndex(NoteStrings(i)))
                    If BMSChannelList(CurrentBMSChannel) = "SC" Then _
                        xprevNotes(UBound(xprevNotes)).Value = hBMSCROLL(DefinitionIndex(NoteStrings(i)))
                    Continue For
                End If
                If xStrKey(CInt(relativeMeasurePos(i) / xGCD)) <> "00" Then
                    hasOverlapping = True
                End If

                xStrKey(CInt(relativeMeasurePos(i) / xGCD)) = NoteStrings(i)
            Next

            Ret &= "#" & Add3Zeros(MeasureIndex) & CurrentBMSChannel & ":" & Join(xStrKey, "") & vbCrLf
        Next

        Return Ret
    End Function

    Private Function GenerateBackgroundTracks(MeasureIndex As Integer, ByRef hasOverlapping As Boolean, NotesInMeasure() As Note, GreatestColumn As Integer, ByRef xprevNotes() As Note) As String
        Dim relativeNotePositions() As Double 'Ks in the same column
        Dim noteStrings() As String    'Ks in the same column
        Dim Ret As String = ""

        For ColIndex = niB To GreatestColumn 'Start rendering B notes (xI3 is columnindex)
            ReDim relativeNotePositions(-1) 'Ks in the same column
            ReDim noteStrings(-1)      'Ks in the same column

            For I = 0 To UBound(NotesInMeasure) 'Find Ks in the same column (xI4 is TK index)
                If NotesInMeasure(I).ColumnIndex = ColIndex Then

                    ReDim Preserve relativeNotePositions(UBound(relativeNotePositions) + 1)
                    ReDim Preserve noteStrings(UBound(noteStrings) + 1)

                    relativeNotePositions(UBound(relativeNotePositions)) = NotesInMeasure(I).VPosition - MeasureBottom(MeasureAtDisplacement(NotesInMeasure(I).VPosition))
                    If relativeNotePositions(UBound(relativeNotePositions)) < 0 Then relativeNotePositions(UBound(relativeNotePositions)) = 0

                    noteStrings(UBound(noteStrings)) = DefinitionLabel(NotesInMeasure(I).Value \ 10000)
                End If
            Next

            Dim xGCD As Double = MeasureLength(MeasureIndex)
            For i = 0 To UBound(relativeNotePositions)        'find greatest common divisor
                If relativeNotePositions(i) > 0 Then xGCD = GCD(xGCD, relativeNotePositions(i))
            Next

            Dim xStrKey(CInt(MeasureLength(MeasureIndex) / xGCD) - 1) As String
            For i = 0 To UBound(xStrKey)           'assign 00 to all keys
                xStrKey(i) = "00"
            Next

            For i = 0 To UBound(relativeNotePositions)        'assign K texts
                If CInt(relativeNotePositions(i) / xGCD) > UBound(xStrKey) Then

                    ReDim Preserve xprevNotes(UBound(xprevNotes) + 1)

                    With xprevNotes(UBound(xprevNotes))
                        .ColumnIndex = ColIndex
                        .VPosition = MeasureBottom(MeasureIndex)
                        .Value = DefinitionIndex(noteStrings(i))
                    End With

                    Continue For
                End If
                If xStrKey(CInt(relativeNotePositions(i) / xGCD)) <> "00" Then hasOverlapping = True
                xStrKey(CInt(relativeNotePositions(i) / xGCD)) = noteStrings(i)
            Next

            Ret &= "#" & Add3Zeros(MeasureIndex) & "01:" & Join(xStrKey, "") & vbCrLf
        Next

        Return Ret
    End Function

    ''' <summary>Do not clear Undo.</summary>
    Private Sub OpenNBMSC(ByVal Path As String)
        KMouseOver = -1

        Dim br As New BinaryReader(New FileStream(Path, FileMode.Open, FileAccess.Read), System.Text.Encoding.Unicode)

        If br.ReadInt32 <> NBMSCFileSignature Then GoTo EndOfSub
        Dim xSuffix As Byte = br.ReadByte
        If xSuffix <> NBMSCFileSuffixLegacy AndAlso xSuffix <> NBMSCFileSuffixRandom AndAlso xSuffix <> NBMSCFileSuffix Then GoTo EndOfSub
        Dim xReadRandomFields As Boolean = xSuffix <> NBMSCFileSuffixLegacy
        Dim xReadRandomMeasureLengths As Boolean = xSuffix = NBMSCFileSuffix
        Dim xMajor As Integer = br.ReadByte
        Dim xMinor As Integer = br.ReadByte
        Dim xBuild As Integer = br.ReadByte

        ClearUndo()
        ReDim Notes(0)
        ReDim mColumn(999)
        ReDim hWAV(MaxDefinition)
        ReDim hBMP(MaxDefinition)
        Me.InitializeNewBMS()
        Me.InitializeOpenBMS()

        Notes(0) = New Note
        With Notes(0)
            .ColumnIndex = niBPM
            .VPosition = -1
            '.LongNote = False
            '.Selected = False
            .Value = 1200000
            .RandomIndex = -1
            .RandomValue = 0
        End With

        Do Until br.BaseStream.Position >= br.BaseStream.Length
            Dim BlockID As Integer = br.ReadInt32()

            Select Case BlockID

                Case &H66657250     'Preferences
                    Dim xPref As Integer = br.ReadInt32

                    NTInput = xPref And &H1
                    TBNTInput.Checked = NTInput
                    mnNTInput.Checked = NTInput
                    POBLong.Enabled = Not NTInput
                    POBLongShort.Enabled = Not NTInput

                    ErrorCheck = xPref And &H2
                    TBErrorCheck.Checked = ErrorCheck
                    TBErrorCheck_Click(TBErrorCheck, New System.EventArgs)

                    PreviewOnClick = xPref And &H4
                    TBPreviewOnClick.Checked = PreviewOnClick
                    TBPreviewOnClick_Click(TBPreviewOnClick, New System.EventArgs)

                    ShowFileName = xPref And &H8
                    TBShowFileName.Checked = ShowFileName
                    TBShowFileName_Click(TBShowFileName, New System.EventArgs)

                    Rscratch = xPref And &H10
                    If TBChangePlaySide.Checked <> Rscratch Then
                        TBChangePlaySide.Checked = Rscratch
                        TBChangePlaySide_Click(TBChangePlaySide, New System.EventArgs)
                    Else
                        TBChangePlaySide.Checked = Rscratch
                    End If

                    mnSMenu.Checked = xPref And &H100
                    mnSTB.Checked = xPref And &H200
                    mnSOP.Checked = xPref And &H400
                    mnSStatus.Checked = xPref And &H800
                    CGShow.Checked = xPref And &H4000
                    CGShowS.Checked = xPref And &H8000
                    CGShowBG.Checked = xPref And &H10000
                    CGShowM.Checked = xPref And &H20000
                    CGShowMB.Checked = xPref And &H40000
                    CGShowV.Checked = xPref And &H80000
                    CGShowC.Checked = xPref And &H100000
                    CGBLP.Checked = xPref And &H200000
                    CGSTOP.Checked = xPref And &H400000
                    CGSCROLL.Checked = xPref And &H20000000
                    CGBPM.Checked = xPref And &H800000

                    CGSnap.Checked = xPref And &H1000000
                    CGDisableVertical.Checked = xPref And &H2000000
                    CGDivide.Value = br.ReadInt32
                    CGSub.Value = br.ReadInt32
                    gSlash = br.ReadInt32
                    SetGridScaleValue(CGHeight, CDec(br.ReadSingle))
                    SetGridScaleValue(CGWidth, CDec(br.ReadSingle))
                    Dim xBGMColumns As Integer = br.ReadInt32
                    CGB.Value = Math.Min(CInt(CGB.Maximum), Math.Max(CInt(CGB.Minimum), xBGMColumns))

                Case &H64616548     'Header
                    THTitle.Text = br.ReadString
                    THArtist.Text = br.ReadString
                    THGenre.Text = br.ReadString
                    Notes(0).Value = br.ReadInt64
                    Dim xPlayerRank As Integer = br.ReadByte
                    THPlayLevel.Text = br.ReadString

                    CHPlayer.SelectedIndex = xPlayerRank And &HF
                    CHRank.SelectedIndex = xPlayerRank >> 4

                    THSubTitle.Text = br.ReadString
                    THSubArtist.Text = br.ReadString
                    'THMaker.Text = br.ReadString
                    THStageFile.Text = br.ReadString
                    THBanner.Text = br.ReadString
                    THBackBMP.Text = br.ReadString
                    'THMidiFile.Text = br.ReadString
                    CHDifficulty.SelectedIndex = br.ReadByte
                    THExRank.Text = br.ReadString
                    THTotal.Text = br.ReadString
                    'THVolWAV.Text = br.ReadString
                    THComment.Text = br.ReadString
                    'THLnType.Text = br.ReadString
                    CHLnObj.SelectedIndex = br.ReadInt16
                    THPreview.Text = br.ReadString
                    CHLnmode.SelectedIndex = br.ReadByte

                Case &H564157       'WAV List
                    Dim xWAVOptions As Integer = br.ReadByte
                    WAVMultiSelect = xWAVOptions And &H1
                    CWAVMultiSelect.Checked = WAVMultiSelect
                    CWAVMultiSelect_CheckedChanged(CWAVMultiSelect, New EventArgs)
                    WAVChangeLabel = xWAVOptions And &H2
                    CWAVChangeLabel.Checked = WAVChangeLabel
                    CWAVChangeLabel_CheckedChanged(CWAVChangeLabel, New EventArgs)
                    WAVEmptyfill = xWAVOptions And &H4
                    CWAVEmptyfill.Checked = WAVEmptyfill
                    CWAVEmptyfill_CheckedChanged(CWAVEmptyfill, New EventArgs)
                    SetUseBase62Definitions(xWAVOptions And &H8)

                    Dim xWAVCount As Integer = br.ReadInt32
                    For xxi As Integer = 1 To xWAVCount
                        Dim xI As Integer = br.ReadInt16
                        If xI > MaxBase36Definition Then SetUseBase62Definitions(True)
                        hWAV(xI) = br.ReadString
                    Next

                Case &H504D42       'BMP List

                    Dim xBMPCount As Integer = br.ReadInt32
                    For xxi As Integer = 1 To xBMPCount
                        Dim xI As Integer = br.ReadInt16
                        If xI > MaxBase36Definition Then SetUseBase62Definitions(True)
                        hBMP(xI) = br.ReadString
                    Next

                Case &H74616542     'Beat
                    nBeatN.Value = br.ReadInt16
                    nBeatD.Value = br.ReadInt16
                    'nBeatD.SelectedIndex = br.ReadByte

                    Dim xBeatChangeMode As Integer = br.ReadByte
                    Dim xBeatChangeList As RadioButton() = {CBeatPreserve, CBeatMeasure, CBeatCut, CBeatScale}
                    xBeatChangeList(xBeatChangeMode).Checked = True
                    CBeatPreserve_Click(xBeatChangeList(xBeatChangeMode), New System.EventArgs)

                    Dim xBeatCount As Integer = br.ReadInt32
                    For xxi As Integer = 1 To xBeatCount
                        Dim xIndex As Integer = br.ReadInt16
                        MeasureLength(xIndex) = br.ReadDouble
                        Dim xRatio As Double = MeasureLength(xIndex) / 192.0R
                        Dim xxD As Long = GetDenominator(xRatio)
                        LBeat.Items(xIndex) = Add3Zeros(xIndex) & ": " & xRatio & IIf(xxD > 10000, "", " ( " & CLng(xRatio * xxD) & " / " & xxD & " ) ")
                    Next

                Case &H6E707845     'Expansion Code
                    TExpansion.Text = br.ReadString

                Case &H646E6152     'Random
                    ReadNBMSCRandomBlock(br, xReadRandomMeasureLengths)

                Case &H65746F4E     'Note
                    Dim xNoteUbound As Integer = br.ReadInt32
                    ReDim Preserve Notes(xNoteUbound)
                    For i As Integer = 1 To UBound(Notes)
                        Notes(i) = New Note
                        Notes(i).FromBinReader(br, xReadRandomFields)
                    Next

                Case &H6F646E55     'Undo / Redo Commands
                    Dim URCount As Integer = br.ReadInt32
                    Dim xPointer As Integer = br.ReadInt32

                    If URCount <= 0 OrElse URCount > MaxUndoRedoSteps + 2 Then
                        ClearUndo()
                        GoTo EndOfSub
                    End If

                    Dim xUndoList(URCount - 1) As UndoRedo.LinkedURCmd
                    Dim xRedoList(URCount - 1) As UndoRedo.LinkedURCmd

                    For xI As Integer = 0 To URCount - 1
                        xUndoList(xI) = ReadUndoRedoCommandList(br)
                        xRedoList(xI) = ReadUndoRedoCommandList(br)
                    Next

                    ImportUndoRedoHistory(xUndoList, xRedoList, xPointer)

            End Select
        Loop

EndOfSub:
        br.Close()

        RefreshUndoRedoEnabled()

        RefreshDefinitionLists()

        THLandMine.Text = hWAV(0)
        THMissBMP.Text = hBMP(0)

        THBPM.Value = Notes(0).Value / 10000
        SortByVPositionQuick(0, UBound(Notes))
        UpdatePairing()
        UpdateMeasureBottom()
        CopyCurrentMeasureLengthToBase()
        ActiveMeasureRandomIndex = -1
        ActiveMeasureRandomValue = 0
        ApplySelectedRandomMeasureMap()
        CalculateTotalPlayableNotes()
        CalculateGreatestVPosition()
        RefreshRandomPanel()
        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Private Sub SaveNBMSC(ByVal Path As String)
        StoreRandomExtraText()
        CalculateGreatestVPosition()
        SortByVPositionInsertion()
        UpdatePairing()
        Dim xActiveMeasureLengths() As Double = CopyMeasureLengthArray(MeasureLength)
        Dim xBaseMeasureLengths() As Double = CopyMeasureLengthArray(BaseMeasureLength)
        Dim xNotesForSave() As Note = ConvertNotesToMeasureMap(Notes, xActiveMeasureLengths, xBaseMeasureLengths)
        Dim xUndoHistoryConvertedForSave As Boolean = False

        Try
            If Not MeasureLengthArraysEqual(xActiveMeasureLengths, xBaseMeasureLengths) Then
                ConvertUndoRedoHistoryMeasureMap(xActiveMeasureLengths, xBaseMeasureLengths)
                xUndoHistoryConvertedForSave = True
            End If

            Dim bw As New BinaryWriter(New IO.FileStream(Path, FileMode.Create), System.Text.Encoding.Unicode)

            'bw.Write("nBMSC".ToCharArray)
            bw.Write(NBMSCFileSignature)
            bw.Write(NBMSCFileSuffix)
            bw.Write(CByte(My.Application.Info.Version.Major))
            bw.Write(CByte(My.Application.Info.Version.Minor))
            bw.Write(CByte(My.Application.Info.Version.Build))

            'Preferences
            'bw.Write("Pref".ToCharArray)
            bw.Write(&H66657250)
            Dim xPref As Integer = 0
            If NTInput Then xPref = xPref Or &H1
            If ErrorCheck Then xPref = xPref Or &H2
            If PreviewOnClick Then xPref = xPref Or &H4
            If ShowFileName Then xPref = xPref Or &H8
            If Rscratch Then xPref = xPref Or &H10
            If mnSMenu.Checked Then xPref = xPref Or &H100
            If mnSTB.Checked Then xPref = xPref Or &H200
            If mnSOP.Checked Then xPref = xPref Or &H400
            If mnSStatus.Checked Then xPref = xPref Or &H800
            If gShowGrid Then xPref = xPref Or &H4000
            If gShowSubGrid Then xPref = xPref Or &H8000
            If gShowBG Then xPref = xPref Or &H10000
            If gShowMeasureNumber Then xPref = xPref Or &H20000
            If gShowMeasureBar Then xPref = xPref Or &H40000
            If gShowVerticalLine Then xPref = xPref Or &H80000
            If gShowC Then xPref = xPref Or &H100000
            If gDisplayBGAColumn Then xPref = xPref Or &H200000
            If gSTOP Then xPref = xPref Or &H400000
            If gBPM Then xPref = xPref Or &H800000
            If gSCROLL Then xPref = xPref Or &H20000000
            If gSnap Then xPref = xPref Or &H1000000
            If DisableVerticalMove Then xPref = xPref Or &H2000000
            bw.Write(xPref)
            bw.Write(BitConverter.GetBytes(gDivide))
            bw.Write(BitConverter.GetBytes(gSub))
            bw.Write(BitConverter.GetBytes(gSlash))
            bw.Write(BitConverter.GetBytes(gxHeight))
            bw.Write(BitConverter.GetBytes(gxWidth))
            bw.Write(BitConverter.GetBytes(CInt(CGB.Value)))

            'Header
            'bw.Write("Head".ToCharArray)
            bw.Write(&H64616548)
            bw.Write(THTitle.Text)
            bw.Write(THArtist.Text)
            bw.Write(THGenre.Text)
            bw.Write(Notes(0).Value)
            Dim xPlayer As Integer = PlayerIndexForSave()
            Dim xRank As Integer = CHRank.SelectedIndex << 4
            bw.Write(CByte(xPlayer Or xRank))
            bw.Write(THPlayLevel.Text)

            bw.Write(THSubTitle.Text)
            bw.Write(THSubArtist.Text)
            'bw.Write(THMaker.Text)
            bw.Write(THStageFile.Text)
            bw.Write(THBanner.Text)
            bw.Write(THBackBMP.Text)
            'bw.Write(THMidiFile.Text)
            bw.Write(CByte(CHDifficulty.SelectedIndex))
            bw.Write(THExRank.Text)
            bw.Write(THTotal.Text)
            'bw.Write(THVolWAV.Text)
            bw.Write(THComment.Text)
            'bw.Write(THLnType.Text)
            bw.Write(CShort(CHLnObj.SelectedIndex))
            bw.Write(THPreview.Text)
            bw.Write(CByte(CHLnmode.SelectedIndex))

            'Wav List
            'bw.Write(("WAV" & vbNullChar).ToCharArray)
            bw.Write(&H564157)

            Dim xWAVOptions As Integer = 0
            If WAVMultiSelect Then xWAVOptions = xWAVOptions Or &H1
            If WAVChangeLabel Then xWAVOptions = xWAVOptions Or &H2
            If WAVEmptyfill Then xWAVOptions = xWAVOptions Or &H4
            If UseBase62Definitions Then xWAVOptions = xWAVOptions Or &H8
            bw.Write(CByte(xWAVOptions))

            Dim xWAVCount As Integer = 0
            For i As Integer = 0 To UBound(hWAV)
                If hWAV(i) <> "" Then xWAVCount += 1
            Next
            bw.Write(xWAVCount)

            For i As Integer = 0 To UBound(hWAV)
                If hWAV(i) = "" Then Continue For
                bw.Write(CShort(i))
                bw.Write(hWAV(i))
            Next

            'Bmp List
            'bw.Write(("BMP" & vbNullChar).ToCharArray)
            bw.Write(&H504D42)

            Dim xBMPCount As Integer = 0
            For i As Integer = 0 To UBound(hBMP)
                If hBMP(i) <> "" Then xBMPCount += 1
            Next
            bw.Write(xBMPCount)

            For i As Integer = 0 To UBound(hBMP)
                If hBMP(i) = "" Then Continue For
                bw.Write(CShort(i))
                bw.Write(hBMP(i))
            Next

            'Beat
            'bw.Write("Beat".ToCharArray)
            bw.Write(&H74616542)
            'Dim xNumerator As Short = nBeatN.Value
            'Dim xDenominator As Short = nBeatD.Value
            'Dim xBeatChangeMode As Byte = BeatChangeMode
            bw.Write(CShort(nBeatN.Value))
            bw.Write(CShort(nBeatD.Value))
            bw.Write(CByte(BeatChangeMode))

            Dim xBeatCount As Integer = 0
            For i As Integer = 0 To UBound(xBaseMeasureLengths)
                If xBaseMeasureLengths(i) <> 192.0R Then xBeatCount += 1
            Next
            bw.Write(xBeatCount)

            For i As Integer = 0 To UBound(xBaseMeasureLengths)
                If xBaseMeasureLengths(i) = 192.0R Then Continue For
                bw.Write(CShort(i))
                bw.Write(xBaseMeasureLengths(i))
            Next

            'Expansion Code
            'bw.Write("Expn".ToCharArray)
            bw.Write(&H6E707845)
            bw.Write(TExpansion.Text)

            'Random
            bw.Write(&H646E6152)
            WriteNBMSCRandomBlock(bw)

            'Note
            'bw.Write("Note".ToCharArray)
            bw.Write(&H65746F4E)
            bw.Write(UBound(xNotesForSave))
            For i As Integer = 1 To UBound(xNotesForSave)
                xNotesForSave(i).WriteBinWriter(bw)
            Next

            'Undo / Redo Commands
            'bw.Write("Undo".ToCharArray)
            bw.Write(&H6F646E55)
            Dim xUndoHistory As List(Of UndoRedoHistorySlot) = GetUndoHistory()
            Dim xRedoHistory As List(Of UndoRedoHistorySlot) = GetRedoHistory()
            Dim xHistoryCount As Integer = xUndoHistory.Count + xRedoHistory.Count + 2

            bw.Write(xHistoryCount)
            bw.Write(xUndoHistory.Count)

            WriteUndoRedoCommandList(bw, New UndoRedo.NoOperation)
            WriteUndoRedoCommandList(bw, New UndoRedo.NoOperation)

            For Each xSlot As UndoRedoHistorySlot In xUndoHistory
                WriteUndoRedoCommandList(bw, xSlot.UndoCmd)
                WriteUndoRedoCommandList(bw, xSlot.RedoCmd)
            Next

            For Each xSlot As UndoRedoHistorySlot In xRedoHistory
                WriteUndoRedoCommandList(bw, xSlot.UndoCmd)
                WriteUndoRedoCommandList(bw, xSlot.RedoCmd)
            Next

            WriteUndoRedoCommandList(bw, New UndoRedo.NoOperation)
            WriteUndoRedoCommandList(bw, New UndoRedo.NoOperation)

            bw.Close()

        Catch ex As Exception

            MsgBox(ex.Message)

        Finally
            If xUndoHistoryConvertedForSave Then ConvertUndoRedoHistoryMeasureMap(xBaseMeasureLengths, xActiveMeasureLengths)

        End Try

    End Sub

    Private Sub ReadNBMSCRandomBlock(ByVal br As BinaryReader, ByVal readMeasureLengths As Boolean)
        ResetRandomState()
        RandomCommonVisible = br.ReadBoolean()
        SelectedRandomIndex = br.ReadInt32()

        Dim blockCount As Integer = br.ReadInt32()
        For i As Integer = 0 To blockCount - 1
            Dim block As New BmsRandomBlock()
            block.DefinitionValue = br.ReadInt32()
            block.CurrentValue = br.ReadInt32()
            block.ViewMode = CType(br.ReadInt32(), BmsRandomViewMode)

            Dim extraCount As Integer = br.ReadInt32()
            For j As Integer = 0 To extraCount - 1
                Dim value As Integer = br.ReadInt32()
                block.SetExtraText(value, br.ReadString())
            Next

            If readMeasureLengths Then
                Dim measureBranchCount As Integer = br.ReadInt32()
                For j As Integer = 0 To measureBranchCount - 1
                    Dim value As Integer = br.ReadInt32()
                    Dim measureCount As Integer = br.ReadInt32()
                    For k As Integer = 0 To measureCount - 1
                        block.SetMeasureLength(value, br.ReadInt32(), br.ReadDouble())
                    Next
                Next
            End If

            block.Normalize()
            RandomBlocks.Add(block)
        Next

        If Not IsValidRandomIndex(SelectedRandomIndex) Then SelectedRandomIndex = -1
    End Sub

    Private Sub WriteNBMSCRandomBlock(ByVal bw As BinaryWriter)
        bw.Write(RandomCommonVisible)
        bw.Write(SelectedRandomIndex)
        bw.Write(RandomBlocks.Count)

        For Each block As BmsRandomBlock In RandomBlocks
            block.Normalize()
            bw.Write(block.DefinitionValue)
            bw.Write(block.CurrentValue)
            bw.Write(CInt(block.ViewMode))

            bw.Write(block.ExtraTextByValue.Count)
            For Each pair As KeyValuePair(Of Integer, String) In block.ExtraTextByValue
                bw.Write(pair.Key)
                bw.Write(If(pair.Value, ""))
            Next

            bw.Write(block.MeasureLengthByValue.Count)
            For Each branch As KeyValuePair(Of Integer, Dictionary(Of Integer, Double)) In block.MeasureLengthByValue
                bw.Write(branch.Key)
                bw.Write(branch.Value.Count)
                For Each measure As KeyValuePair(Of Integer, Double) In branch.Value
                    bw.Write(measure.Key)
                    bw.Write(measure.Value)
                Next
            Next
        Next
    End Sub

    Private Sub SaveBMSON(ByVal Path As String)
        StoreRandomExtraText()
        CalculateGreatestVPosition()
        SortByVPositionInsertion()
        UpdatePairing()
        Dim xNotesBackup() As Note = Notes
        If RandomBlocks.Count > 0 Then Notes = NotesForRandomExport()
        Dim i = 0
        Try
            Dim format = New Bmson
            Dim options = New JsonSerializerOptions

            Dim bar_list = New List(Of BarLine)()
            Dim wav_list = New Dictionary(Of Integer, SoundChannel)()
            Dim note_list = New Dictionary(Of Integer, List(Of BmsonNote))()
            Dim mine_list = New List(Of MineNote)()
            Dim hidden_list = New Dictionary(Of Integer, MineChannel)()
            Dim hidden_note_list = New Dictionary(Of Integer, List(Of MineNote))()
            Dim bmp_list = New List(Of BGAHeader)()
            Dim bga_list = New List(Of BGAEvent)()
            Dim layer_list = New List(Of BGAEvent)()
            Dim miss_list = New List(Of BGAEvent)()
            Dim bpm_list = New List(Of BpmEvent)()
            Dim stop_list = New List(Of StopEvent)()
            Dim scroll_list = New List(Of ScrollEvent)()

            ' ヘッダ情報
            format.info.title = THTitle.Text
            format.info.subtitle = THSubTitle.Text
            format.info.artist = THArtist.Text
            format.info.subartists(0) = THSubArtist.Text
            format.info.genre = THGenre.Text
            If CHPlayer.SelectedIndex = 0 Then
                format.info.mode_hint = "beat-5k"
            Else
                format.info.mode_hint = "popn-9k"
            End If
            format.info.chart_name = ""
            If THExRank.Text <> "" Then
                format.info.judge_rank = CDbl(THExRank.Text)
            Else
                format.info.judge_rank = (CHRank.SelectedIndex + 1) * 25
            End If
            If THTotal.Text <> "" Then
                format.info.total = CalcBMSONTotal(CDbl(THTotal.Text))
            Else
                format.info.total = CalcBMSONTotal(CalculateRecommendedTotal())
            End If
            format.info.init_bpm = CDbl(THBPM.Text)
            If THPlayLevel.Text <> "" Then
                format.info.level = CInt(THPlayLevel.Text)
            Else
                format.info.level = 0
            End If
            format.info.back_image = THBackBMP.Text
            format.info.eyecatch_image = THStageFile.Text
            format.info.banner_image = THBanner.Text
            format.info.preview_music = THPreview.Text
            format.info.ln_type = CHLnmode.SelectedIndex

            ' 必要な分解能を計算
            Dim xGCD As Double = 192.0R
            For i = 0 To UBound(Notes)
                xGCD = GCD(xGCD, Notes(i).VPosition, 1920000)
                ' ついでにプレイモードを検出
                If format.info.mode_hint = "beat-5k" AndAlso
                   GetColumn(Notes(i).ColumnIndex).Identifier >= (36 + 8) AndAlso GetColumn(Notes(i).ColumnIndex).Identifier <= (36 + 9) Then
                    format.info.mode_hint = "beat-7k"
                End If
                If format.info.mode_hint = "beat-5k" AndAlso
                   GetColumn(Notes(i).ColumnIndex).Identifier >= 72 AndAlso GetColumn(Notes(i).ColumnIndex).Identifier <= (72 + 5) Then
                    format.info.mode_hint = "beat-10k"
                End If
                If format.info.mode_hint = "popn-9k" AndAlso
                    (GetColumn(Notes(i).ColumnIndex).Identifier Mod 36 = 6 OrElse GetColumn(Notes(i).ColumnIndex).Identifier = (72 + 1)) Then
                    format.info.mode_hint = "beat-10k"
                End If
                If (format.info.mode_hint = "beat-10k" OrElse format.info.mode_hint = "popn-9k") AndAlso
                   GetColumn(Notes(i).ColumnIndex).Identifier >= (36 + 8) AndAlso GetColumn(Notes(i).ColumnIndex).Identifier <= (36 + 8) Then
                    format.info.mode_hint = "beat-14k"
                End If
                If (format.info.mode_hint <> "beat-14k" AndAlso format.info.mode_hint <> "keyboard-24k" AndAlso format.info.mode_hint <> "keyboard-24k-double") AndAlso
                   GetColumn(Notes(i).ColumnIndex).Identifier >= (72 + 8) AndAlso GetColumn(Notes(i).ColumnIndex).Identifier <= (72 + 9) Then
                    format.info.mode_hint = "beat-14k"
                End If
                If (format.info.mode_hint = "beat-5k" OrElse format.info.mode_hint = "beat-7k") AndAlso
                   (GetColumn(Notes(i).ColumnIndex).Identifier = (36 + 7) OrElse GetColumn(Notes(i).ColumnIndex).Identifier > (36 + 9)) AndAlso GetColumn(Notes(i).ColumnIndex).Identifier < 72 Then
                    format.info.mode_hint = "keyboard-24k"
                End If
                If (format.info.mode_hint = "popn-9k" OrElse format.info.mode_hint = "beat-10k" OrElse format.info.mode_hint = "beat-14k") AndAlso
                   (GetColumn(Notes(i).ColumnIndex).Identifier = (36 + 7) OrElse GetColumn(Notes(i).ColumnIndex).Identifier > (36 + 9)) AndAlso GetColumn(Notes(i).ColumnIndex).Identifier < 72 Then
                    format.info.mode_hint = "keyboard-24k-double"
                End If
                If format.info.mode_hint <> "keyboard-24k-double" AndAlso
                   (GetColumn(Notes(i).ColumnIndex).Identifier = (72 + 7) OrElse GetColumn(Notes(i).ColumnIndex).Identifier > (72 + 9)) AndAlso GetColumn(Notes(i).ColumnIndex).Identifier < 108 Then
                    format.info.mode_hint = "keyboard-24k-double"
                End If
            Next
            Dim resolution = CInt(48.0R / xGCD)
            format.info.resolution = resolution
            ' 小節線定義
            Dim len As Double = 0
            For i = 0 To MeasureAtDisplacement(GreatestVPosition) + 1
                len += MeasureLength(i) * resolution / 48.0R
                bar_list.Add(New BarLine(len))
            Next
            ' Notes
            For i = 1 To UBound(Notes)
                Dim position = Notes(i).VPosition * resolution / 48.0R
                If Notes(i).ColumnIndex = niSCROLL Then
                    scroll_list.Add(New ScrollEvent(position, Notes(i).Value / 10000.0R))
                ElseIf Notes(i).ColumnIndex = niBPM Then
                    bpm_list.Add(New BpmEvent(position, Notes(i).Value / 10000.0R))
                ElseIf Notes(i).ColumnIndex = niSTOP Then
                    stop_list.Add(New StopEvent(position, Notes(i).Value * resolution / 480000.0R))
                ElseIf Notes(i).ColumnIndex = niBGA Then
                    bga_list.Add(New BGAEvent(position, Notes(i).Value / 10000))
                ElseIf Notes(i).ColumnIndex = niLAYER Then
                    layer_list.Add(New BGAEvent(position, Notes(i).Value / 10000))
                ElseIf Notes(i).ColumnIndex = niPOOR Then
                    miss_list.Add(New BGAEvent(position, Notes(i).Value / 10000))
                ElseIf Notes(i).ColumnIndex >= niB Then
                    Dim value = Notes(i).Value / 10000
                    If Not note_list.ContainsKey(value) Then
                        note_list(value) = New List(Of BmsonNote)
                    End If
                    note_list(value).Add(New BmsonNote(position, 0))
                Else
                    Dim lane = GetColumn(Notes(i).ColumnIndex).Identifier - 36
                    Dim value = Notes(i).Value / 10000
                    'ノート定義を変換
                    If format.info.mode_hint = "popn-9k" Then
                        If lane >= 36 Then
                            lane -= 32
                        End If
                    ElseIf format.info.mode_hint = "keyboard-24k" OrElse format.info.mode_hint = "keyboard-24k-double" Then
                        If (lane Mod 36) = 6 OrElse (lane Mod 36) = 7 Then
                            lane += 19
                        ElseIf (lane Mod 36) >= 8 Then
                            lane -= 2
                        End If
                        If (lane >= 36) Then
                            lane -= 10
                        End If
                    Else
                        If (lane Mod 36) = 6 Then
                            lane += 2
                        ElseIf (lane Mod 36) >= 8 Then
                            lane -= 2
                        End If
                        If (lane >= 36) Then
                            lane -= 28
                        End If
                    End If

                    If Notes(i).Landmine Then
                        mine_list.Add(New MineNote(position, lane, value))
                    ElseIf Notes(i).Hidden Then
                        If Not hidden_note_list.ContainsKey(value) Then
                            hidden_note_list(value) = New List(Of MineNote)
                        End If
                        hidden_note_list(value).Add(New MineNote(position, lane, 0))
                    ElseIf Notes(i).length > 0 AndAlso NTInput Then
                        If Not note_list.ContainsKey(value) Then
                            note_list(value) = New List(Of BmsonNote)
                        End If
                        Dim length = Notes(i).Length * resolution / 48.0R
                        note_list(value).Add(New BmsonNote(position, lane, length))
                    ElseIf Notes(i).LNPair > 0 Then
                        If i < Notes(i).LNPair Then
                            If Not note_list.ContainsKey(value) Then
                                note_list(value) = New List(Of BmsonNote)
                            End If
                            Dim length = (Notes(Notes(i).LNPair).VPosition - Notes(i).VPosition) * resolution / 48.0R
                            Dim note = New BmsonNote(position, lane, length)
                            note_list(value).Add(note)
                        Else
                            If Notes(i).Value \ 10000 <> LnObj AndAlso Notes(i).Value <> Notes(Notes(i).LNPair).Value Then
                                Dim note = New BmsonNote(position, lane)
                                note.up = True
                                note_list(value).Add(note)
                            End If
                        End If
                    Else
                        If Not note_list.ContainsKey(value) Then
                            note_list(value) = New List(Of BmsonNote)
                        End If
                        note_list(value).Add(New BmsonNote(position, lane))
                    End If
                End If
            Next
            ' 音定義
            For i = 1 To UBound(hWAV)
                If hWAV(i) <> "" Then
                    If note_list.ContainsKey(i) Then
                        wav_list.Add(i, New SoundChannel(hWAV(i)))
                    End If
                End If
                If hidden_note_list.ContainsKey(i) Then
                    If hWAV(i) <> "" Then
                        hidden_list.Add(i, New MineChannel(hWAV(i)))
                    Else
                        hidden_list.Add(i, New MineChannel(""))
                    End If
                End If
            Next
            ReDim format.mine_channels(0)
            If hWAV(0) <> "" Then
                format.mine_channels(0) = New MineChannel(hWAV(0))
            Else
                format.mine_channels(0) = New MineChannel("")
            End If
            For i = 1 To UBound(hBMP)
                If hBMP(i) <> "" Then
                    bmp_list.Add(New BGAHeader(i, hBMP(i)))
                End If
            Next
            ' 適用
            format.lines = bar_list.ToArray()
            format.bpm_events = bpm_list.ToArray()
            format.stop_events = stop_list.ToArray()
            format.scroll_events = scroll_list.ToArray()
            format.bga.bga_header = bmp_list.ToArray()
            format.bga.bga_events = bga_list.ToArray()
            format.bga.layer_events = layer_list.ToArray()
            format.bga.poor_events = miss_list.ToArray()
            format.mine_channels(0).notes = mine_list.ToArray()

            ReDim format.sound_channels(wav_list.Count() - 1)
            ReDim format.key_channels(hidden_list.Count() - 1)

            i = 0
            For Each n In wav_list
                format.sound_channels(i) = New SoundChannel(n.Value().name)
                ReDim format.sound_channels(i).notes(note_list(n.Key()).Count() - 1)
                format.sound_channels(i).notes = note_list(n.Key()).ToArray()
                i += 1
            Next

            i = 0
            For Each n In hidden_list
                format.key_channels(i) = New MineChannel(n.Value().name)
                ReDim format.key_channels(i).notes(hidden_note_list(n.Key()).Count() - 1)
                format.key_channels(i).notes = hidden_note_list(n.Key()).ToArray()
                i += 1
            Next

            options.IncludeFields = True
            options.WriteIndented = True
            Dim bw As New BinaryWriter(New IO.FileStream(Path, FileMode.Create), System.Text.Encoding.UTF8)
            Dim str = JsonSerializer.SerializeToUtf8Bytes(format, options)
            bw.Write(str)
            bw.Close()
        Catch ex As Exception
            MsgBox(ex.Message)
        Finally
            Notes = xNotesBackup
        End Try
    End Sub

    Function CalcBMSONTotal(total As Double) As Double
        Dim notes = CalculateRecommendedTotalNotes()
        Return total / System.Math.Max((800.0 / (700 + notes) * notes), 250.0) * 100
    End Function

End Class
