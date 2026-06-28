Imports iBMSC.Editor

Partial Public Class MainWindow

    Private Sub PMainInPreviewKeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.PreviewKeyDownEventArgs) Handles PMainIn.PreviewKeyDown, PMainInL.PreviewKeyDown, PMainInR.PreviewKeyDown
        If e.KeyCode = Keys.ShiftKey Or e.KeyCode = Keys.ControlKey Then
            RefreshPanelAll()
            POStatusRefresh()
            Exit Sub
        End If

        If e.KeyCode = 18 Then Exit Sub

        If HandleGlobalShortcutFromPreview(e.KeyCode,
                                           My.Computer.Keyboard.CtrlKeyDown,
                                           My.Computer.Keyboard.ShiftKeyDown,
                                           My.Computer.Keyboard.AltKeyDown) Then Return

        If IsMoveKey(e.KeyCode) AndAlso Not My.Computer.Keyboard.ShiftKeyDown Then
            BeginKeyMove(e.KeyCode)
        Else
            EndKeyMove()
        End If

        Dim iI As Integer = sender.Tag
        If Not IsValidPanelIndex(iI) Then Return
        Dim xI1 As Integer
        Dim xTargetColumn As Integer = -1
        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo
        ReDim SelectedNotes(-1)

        Select Case e.KeyCode
            Case Keys.Up
                If My.Computer.Keyboard.ShiftKeyDown Then
                    AdjustSelectedLongNoteEnds(e.KeyCode)
                    Exit Select
                End If

                Dim xVPosition As Double = 192 / gDivide
                If My.Computer.Keyboard.CtrlKeyDown Then xVPosition = 1

                'Ks cannot be beyond the upper boundary
                Dim muVPosition As Double = GetMaxVPosition() - 1
                For xI1 = 1 To UBound(Notes)
                    If Notes(xI1).Selected Then
                        'K(xI1).VPosition = Math.Floor(K(xI1).VPosition / (192 / gDivide)) * 192 / gDivide
                        muVPosition = IIf(Notes(xI1).VPosition + IIf(NTInput, Notes(xI1).Length, 0) + xVPosition > muVPosition,
                                                          Notes(xI1).VPosition + IIf(NTInput, Notes(xI1).Length, 0) + xVPosition,
                                                          muVPosition)
                    End If
                Next
                muVPosition -= 191999

                'xRedo = sCmdKMs(0, xVPosition - muVPosition, True)
                Dim xVPos As Double
                For xI1 = 1 To UBound(Notes)
                    If Not Notes(xI1).Selected Then Continue For

                    xVPos = Notes(xI1).VPosition + xVPosition - muVPosition
                    Me.RedoMoveNote(KeyMoveStartNote(Notes(xI1)), Notes(xI1).ColumnIndex, xVPos, xUndo, xRedo)
                    Notes(xI1).VPosition = xVPos
                Next
                'xUndo = sCmdKMs(0, -xVPosition + muVPosition, True)

                If xVPosition - muVPosition <> 0 Then AddKeyMoveUndo(xUndo, xBaseRedo.Next)
                SortByVPositionInsertion()
                UpdatePairing()
                CalculateTotalPlayableNotes()
                CalculateGreatestVPosition()
                FollowSelectedNotes(iI, e.KeyCode)
                RefreshPanelAll()

            Case Keys.Down
                If My.Computer.Keyboard.ShiftKeyDown Then
                    AdjustSelectedLongNoteEnds(e.KeyCode)
                    Exit Select
                End If

                Dim xVPosition As Double = -192 / gDivide
                If My.Computer.Keyboard.CtrlKeyDown Then xVPosition = -1

                'Ks cannot be beyond the lower boundary
                Dim mVPosition As Double = 0
                For xI1 = 1 To UBound(Notes)
                    If Notes(xI1).Selected Then
                        'K(xI1).VPosition = Math.Ceiling(K(xI1).VPosition / (192 / gDivide)) * 192 / gDivide
                        mVPosition = IIf(Notes(xI1).VPosition + xVPosition < mVPosition,
                                                                 Notes(xI1).VPosition + xVPosition,
                                                                 mVPosition)
                    End If
                Next

                'xRedo = sCmdKMs(0, xVPosition - mVPosition, True)
                Dim xVPos As Double
                For xI1 = 1 To UBound(Notes)
                    If Not Notes(xI1).Selected Then Continue For

                    xVPos = Notes(xI1).VPosition + xVPosition - mVPosition
                    Me.RedoMoveNote(KeyMoveStartNote(Notes(xI1)), Notes(xI1).ColumnIndex, xVPos, xUndo, xRedo)
                    Notes(xI1).VPosition = xVPos
                Next
                'xUndo = sCmdKMs(0, -xVPosition + mVPosition, True)

                If xVPosition - mVPosition <> 0 Then AddKeyMoveUndo(xUndo, xBaseRedo.Next)
                SortByVPositionInsertion()
                UpdatePairing()
                CalculateTotalPlayableNotes()
                CalculateGreatestVPosition()
                FollowSelectedNotes(iI, e.KeyCode)
                RefreshPanelAll()

            Case Keys.Left
                'For xI1 = 1 To UBound(K)
                '    If K(xI1).Selected Then K(xI1).ColumnIndex = RealColumnToEnabled(K(xI1).ColumnIndex) - 1
                'Next

                'Ks cannot be beyond the left boundary
                Dim mLeft As Integer = 0
                For xI1 = 1 To UBound(Notes)
                    If Notes(xI1).Selected Then mLeft = IIf(ColumnArrayIndexToEnabledColumnIndex(Notes(xI1).ColumnIndex) - 1 < mLeft,
                                                        ColumnArrayIndexToEnabledColumnIndex(Notes(xI1).ColumnIndex) - 1,
                                                        mLeft)
                Next
                'xRedo = sCmdKMs(-1 - mLeft, 0, True)
                Dim xCol As Integer
                For xI1 = 1 To UBound(Notes)
                    If Not Notes(xI1).Selected Then Continue For

                    xCol = EnabledColumnIndexToColumnArrayIndex(ColumnArrayIndexToEnabledColumnIndex(Notes(xI1).ColumnIndex) - 1 - mLeft)
                    Me.RedoMoveNote(KeyMoveStartNote(Notes(xI1)), xCol, Notes(xI1).VPosition, xUndo, xRedo)
                    Notes(xI1).ColumnIndex = xCol
                Next
                'xUndo = sCmdKMs(1 + mLeft, 0, True)

                If -1 - mLeft <> 0 Then AddKeyMoveUndo(xUndo, xBaseRedo.Next)
                UpdatePairing()
                CalculateTotalPlayableNotes()
                FollowSelectedNotes(iI, e.KeyCode)
                RefreshPanelAll()

            Case Keys.Right
                'xRedo = sCmdKMs(1, 0, True)
                Dim xCol As Integer
                For xI1 = 1 To UBound(Notes)
                    If Not Notes(xI1).Selected Then Continue For

                    xCol = EnabledColumnIndexToColumnArrayIndex(ColumnArrayIndexToEnabledColumnIndex(Notes(xI1).ColumnIndex) + 1)
                    Me.RedoMoveNote(KeyMoveStartNote(Notes(xI1)), xCol, Notes(xI1).VPosition, xUndo, xRedo)
                    Notes(xI1).ColumnIndex = xCol
                Next
                'xUndo = sCmdKMs(-1, 0, True)

                AddKeyMoveUndo(xUndo, xBaseRedo.Next)
                UpdatePairing()
                CalculateTotalPlayableNotes()
                FollowSelectedNotes(iI, e.KeyCode)
                RefreshPanelAll()

            Case Keys.Delete
                mnDelete_Click(mnDelete, New System.EventArgs)

            Case Keys.Home
                Dim xHomeScroll As EditorScrollBar = GetPanelVScrollBar(PanelFocus)
                If xHomeScroll IsNot Nothing Then xHomeScroll.Value = 0

            Case Keys.End
                Dim xEndScroll As EditorScrollBar = GetPanelVScrollBar(PanelFocus)
                If xEndScroll IsNot Nothing Then xEndScroll.Value = xEndScroll.Minimum

            Case Keys.PageUp
                Dim xPageUpScroll As EditorScrollBar = GetPanelVScrollBar(PanelFocus)
                If xPageUpScroll IsNot Nothing Then xPageUpScroll.Value = IIf(xPageUpScroll.Value - gPgUpDn > xPageUpScroll.Minimum, xPageUpScroll.Value - gPgUpDn, xPageUpScroll.Minimum)

            Case Keys.PageDown
                Dim xPageDownScroll As EditorScrollBar = GetPanelVScrollBar(PanelFocus)
                If xPageDownScroll IsNot Nothing Then xPageDownScroll.Value = IIf(xPageDownScroll.Value + gPgUpDn < 0, xPageDownScroll.Value + gPgUpDn, 0)

            Case Keys.Oemcomma
                MoveGridDivide(1)

            Case Keys.OemPeriod
                MoveGridDivide(-1)

            Case Keys.OemQuestion
                'Dim xTempSwap As Integer = gSlash
                'gSlash = CGDivide.Value
                'CGDivide.Value = xTempSwap
                CGDivide.Value = gSlash

            Case Keys.Oemplus
                With IIf(My.Computer.Keyboard.ShiftKeyDown, CGWidth, CGHeight)
                    .Value += IIf(.Value > .Maximum - .Increment, .Maximum - .Value, .Increment)
                End With

            Case Keys.OemMinus
                With IIf(My.Computer.Keyboard.ShiftKeyDown, CGWidth, CGHeight)
                    .Value -= IIf(.Value < .Minimum + .Increment, .Value - .Minimum, .Increment)
                End With

            Case Keys.Add
                IncreaseCurrentWav()
            Case Keys.Subtract
                DecreaseCurrentWav()

            Case Keys.G
                'az: don't trigger when we use Go To Measure
                If Not My.Computer.Keyboard.CtrlKeyDown Then CGSnap.Checked = Not gSnap

            Case Keys.L
                If Not My.Computer.Keyboard.CtrlKeyDown Then POBLong_Click(Nothing, Nothing)

            Case Keys.S
                If Not My.Computer.Keyboard.CtrlKeyDown Then POBNormal_Click(Nothing, Nothing)

            Case Keys.M
                If Not My.Computer.Keyboard.CtrlKeyDown Then POBMirror_Click(Nothing, Nothing)

            Case Keys.D
                CGDisableVertical.Checked = Not CGDisableVertical.Checked

            Case Keys.NumPad0, Keys.D0
                MoveToBGM(xUndo, xRedo)

            Case Keys.Oem1, Keys.NumPad1, Keys.D1 : MoveToColumn(niA2, xUndo, xRedo)
            Case Keys.Oem2, Keys.NumPad2, Keys.D2 : MoveToColumn(niA3, xUndo, xRedo)
            Case Keys.Oem3, Keys.NumPad3, Keys.D3 : MoveToColumn(niA4, xUndo, xRedo)
            Case Keys.Oem4, Keys.NumPad4, Keys.D4 : MoveToColumn(niA5, xUndo, xRedo)
            Case Keys.Oem5, Keys.NumPad5, Keys.D5 : MoveToColumn(niA6, xUndo, xRedo)
            Case Keys.Oem6, Keys.NumPad6, Keys.D6 : MoveToColumn(niA7, xUndo, xRedo)
            Case Keys.Oem7, Keys.NumPad7, Keys.D7 : MoveToColumn(niA8, xUndo, xRedo)
            Case Keys.Oem8, Keys.NumPad8, Keys.D8 : MoveToColumn(niA9, xUndo, xRedo)

        End Select

        If My.Computer.Keyboard.CtrlKeyDown And (Not My.Computer.Keyboard.AltKeyDown) And (Not My.Computer.Keyboard.ShiftKeyDown) Then
            Select Case e.KeyCode
                Case Keys.Z : TBUndo_Click(TBUndo, New EventArgs)
                Case Keys.Y : TBRedo_Click(TBRedo, New EventArgs)
                Case Keys.X : TBCut_Click(TBCut, New EventArgs)
                Case Keys.C : TBCopy_Click(TBCopy, New EventArgs)
                Case Keys.V : TBPaste_Click(TBPaste, New EventArgs)
                Case Keys.A : mnSelectAll_Click(mnSelectAll, New EventArgs)
                Case Keys.F : TBFind_Click(TBFind, New EventArgs)
                Case Keys.T : TBStatistics_Click(TBStatistics, New EventArgs)
            End Select
        End If

        If ModifierMultiselectActive() Then
            If e.KeyCode = Keys.A And KMouseOver <> -1 Then
                SelectAllWithHoveredNoteLabel()
            End If
        End If

        PMainInMouseMove(sender)
        POStatusRefresh()
    End Sub

    Private Sub PMainInKeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles PMainIn.KeyUp, PMainInL.KeyUp, PMainInR.KeyUp
        If e.KeyCode = KeyMoveKey Then EndKeyMove()
    End Sub

    Private Function IsMoveKey(ByVal keyCode As Keys) As Boolean
        Select Case keyCode
            Case Keys.Up, Keys.Down, Keys.Left, Keys.Right
                Return True
        End Select

        Return False
    End Function

    Private Sub BeginKeyMove(ByVal keyCode As Keys)
        If KeyMoveKey = keyCode AndAlso KeyMoveNotes.Length <> 0 Then Return

        EndKeyMove()
        KeyMoveKey = keyCode

        Dim xCount As Integer = 0
        For xI1 As Integer = 1 To UBound(Notes)
            If Notes(xI1).Selected Then xCount += 1
        Next

        If xCount = 0 Then
            KeyMoveKey = Keys.None
            Return
        End If

        ReDim KeyMoveNotes(xCount - 1)
        Dim xIndex As Integer = 0
        For xI1 As Integer = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For

            Notes(xI1).TempIndex = xIndex
            KeyMoveNotes(xIndex) = Notes(xI1)
            xIndex += 1
        Next
    End Sub

    Private Sub EndKeyMove()
        KeyMoveKey = Keys.None
        KeyMoveUndoAdded = False
        ReDim KeyMoveNotes(-1)
    End Sub

    Private Function KeyMoveStartNote(ByVal note As Note) As Note
        If note.TempIndex >= 0 AndAlso note.TempIndex < KeyMoveNotes.Length Then Return KeyMoveNotes(note.TempIndex)

        Return note
    End Function

    Private Sub AddKeyMoveUndo(ByVal undoCmd As UndoRedo.LinkedURCmd, ByVal redoCmd As UndoRedo.LinkedURCmd)
        If undoCmd Is Nothing And redoCmd Is Nothing Then Return

        AddUndo(undoCmd, redoCmd, KeyMoveUndoAdded)
        If Not KeyMoveUndoAdded Then KeyMoveUndoAdded = True
    End Sub

    Private Sub FollowSelectedNotes(ByVal panelIndex As Integer, ByVal keyCode As Keys)
        Dim xMinLeft As Integer = Integer.MaxValue
        Dim xMaxRight As Integer = Integer.MinValue
        Dim xMinV As Double = Double.MaxValue
        Dim xMaxV As Double = Double.MinValue
        Dim xFound As Boolean = False

        For xI1 As Integer = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For
            If Not nEnabled(Notes(xI1).ColumnIndex) Then Continue For

            xFound = True

            Dim xLeft As Integer = nLeft(Notes(xI1).ColumnIndex)
            Dim xRight As Integer = xLeft + GetColumnWidth(Notes(xI1).ColumnIndex)
            xMinLeft = Math.Min(xMinLeft, xLeft)
            xMaxRight = Math.Max(xMaxRight, xRight)

            Dim xLowerV As Double = Notes(xI1).VPosition
            Dim xUpperV As Double = Notes(xI1).VPosition + IIf(NTInput, Notes(xI1).Length, 0)
            xMinV = Math.Min(xMinV, xLowerV)
            xMaxV = Math.Max(xMaxV, xUpperV)
        Next

        If Not xFound Then Return

        Dim xPanel As Panel = spMain(panelIndex)
        If xPanel.Width <= 0 OrElse xPanel.Height <= 0 Then Return

        FollowSelectedNotesHorizontal(GetPanelHScroll(panelIndex), xPanel.Width, xMinLeft, xMaxRight, keyCode)
        FollowSelectedNotesVertical(GetPanelVScrollBar(panelIndex), xPanel.Height, xMinV, xMaxV, keyCode)
    End Sub

    Private Sub FollowSelectedNotesHorizontal(ByVal xScroll As EditorScrollBar, ByVal xPanelWidth As Integer, ByVal xMinLeft As Integer, ByVal xMaxRight As Integer, ByVal keyCode As Keys)
        If xScroll Is Nothing Then Return

        Dim xValue As Integer = xScroll.Value
        Dim xWidth As Double = xPanelWidth / gxWidth

        Select Case keyCode
            Case Keys.Left
                If xMinLeft < xValue Then
                    xValue = xMinLeft
                ElseIf xMaxRight > xValue + xWidth Then
                    xValue = CInt(Math.Ceiling(xMaxRight - xWidth))
                End If

            Case Keys.Right
                If xMaxRight > xValue + xWidth Then
                    xValue = CInt(Math.Ceiling(xMaxRight - xWidth))
                ElseIf xMinLeft < xValue Then
                    xValue = xMinLeft
                End If
        End Select

        SetScrollValue(xScroll, xValue)
    End Sub

    Private Sub FollowSelectedNotesVertical(ByVal xScroll As EditorScrollBar, ByVal xPanelHeight As Integer, ByVal xMinV As Double, ByVal xMaxV As Double, ByVal keyCode As Keys)
        If xScroll Is Nothing Then Return

        Dim xValue As Integer = xScroll.Value

        Select Case keyCode
            Case Keys.Up
                If xMinV > VisibleUpperVPosition(xValue, xPanelHeight) Then
                    xValue = CInt(Math.Floor((xPanelHeight - vo.kHeight - 1) / gxHeight - xMinV))
                ElseIf xMaxV < -xValue Then
                    xValue = CInt(Math.Ceiling(-xMaxV))
                End If

            Case Keys.Down
                If xMaxV < -xValue Then
                    xValue = CInt(Math.Ceiling(-xMaxV))
                ElseIf xMinV > VisibleUpperVPosition(xValue, xPanelHeight) Then
                    xValue = CInt(Math.Floor((xPanelHeight - vo.kHeight - 1) / gxHeight - xMinV))
                End If
        End Select

        SetScrollValue(xScroll, xValue)
    End Sub

    Private Function VisibleUpperVPosition(ByVal xScrollValue As Integer, ByVal xPanelHeight As Integer) As Double
        Return -xScrollValue + (xPanelHeight - vo.kHeight - 1) / gxHeight
    End Function

    Private Sub SetScrollValue(ByVal xScroll As EditorScrollBar, ByVal xValue As Integer)
        xValue = ClampScrollValue(xScroll, xValue)
        If xScroll.Value <> xValue Then xScroll.Value = xValue
    End Sub

    Private Function ClampScrollValue(ByVal xScroll As EditorScrollBar, ByVal xValue As Integer) As Integer
        Dim xMaximum As Integer = xScroll.Maximum - xScroll.LargeChange + 1
        If xMaximum < xScroll.Minimum Then xMaximum = xScroll.Minimum
        If xValue > xMaximum Then xValue = xMaximum
        If xValue < xScroll.Minimum Then xValue = xScroll.Minimum

        Return xValue
    End Function

    Private Function GetPanelHScroll(ByVal panelIndex As Integer) As EditorScrollBar
        If Not IsValidPanelIndex(panelIndex) Then Return Nothing
        Return SplitPanes(panelIndex).HScroll
    End Function

    Private Function GetPanelVScrollBar(ByVal panelIndex As Integer) As EditorScrollBar
        If Not IsValidPanelIndex(panelIndex) Then Return Nothing
        Return SplitPanes(panelIndex).VScroll
    End Function

    Private Sub MoveGridDivide(ByVal direction As Integer)
        If TBGridDivide IsNot Nothing AndAlso MoveGridComboValue(TBGridDivide, direction) Then Return

        Dim xValue As Decimal = CGDivide.Value + direction
        xValue = Math.Min(CGDivide.Maximum, Math.Max(CGDivide.Minimum, xValue))
        CGDivide.Value = xValue
    End Sub

    Private Sub AdjustSelectedLongNoteEnds(ByVal keyCode As Keys)
        If Not NTInput Then Return

        Dim xDelta As Double = If(My.Computer.Keyboard.CtrlKeyDown, 1, 192 / gDivide)
        If keyCode = Keys.Down Then xDelta = -xDelta

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        For xI1 As Integer = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For
            If IsColumnNumeric(Notes(xI1).ColumnIndex) Then Continue For

            Dim xLength As Double = Notes(xI1).Length + xDelta
            Dim xMaxLength As Double = Math.Max(0, GetMaxVPosition() - 1 - Notes(xI1).VPosition)
            xLength = Math.Max(0, xLength)
            xLength = Math.Min(xMaxLength, xLength)
            If xLength = Notes(xI1).Length Then Continue For

            RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition, xLength, xUndo, xRedo)
            Notes(xI1).Length = xLength
            Notes(xI1).LongNote = xLength <> 0
        Next

        If xUndo Is Nothing Then Return

        AddUndo(xUndo, xBaseRedo.Next)
        SortByVPositionInsertion()
        UpdatePairing()
        CalculateTotalPlayableNotes()
        CalculateGreatestVPosition()
        RefreshPanelAll()
    End Sub

    Private Sub HandleGridWidthMouseWheel(ByVal delta As Integer)
        Dim dv = Math.Round(CGWidth2.Value + delta / 120)
        CGWidth2.Value = Math.Min(CGWidth2.Maximum, Math.Max(CGWidth2.Minimum, dv))
        CGWidth.Value = CGWidth2.Value / 4
    End Sub

    Private Sub SelectAllWithHoveredNoteLabel()
        For xI1 = 0 To UBound(Notes)
            Notes(xI1).Selected = IIf(IsLabelMatch(Notes(xI1), KMouseOver), True, Notes(xI1).Selected)
        Next
    End Sub

    Private Function IsLabelMatch(note As Note, index As Integer) As Boolean
        If TBShowFileName.Checked Then
            Dim wavidx = Notes(index).Value / 10000
            Dim wav = hWAV(wavidx)
            If hWAV(note.Value / 10000) = wav Then
                Return True
            End If
        Else
            If note.Value = Notes(index).Value Then
                Return True
            End If
        End If

        Return False
    End Function

    Private Sub DecreaseCurrentWav()
        If LWAV.SelectedIndex = -1 Then
            LWAV.SelectedIndex = 0
        Else
            Dim newIndex As Integer = LWAV.SelectedIndex - 1
            If newIndex < 0 Then newIndex = 0
            LWAV.SelectedIndices.Clear()
            LWAV.SelectedIndex = newIndex
        End If
    End Sub

    Private Sub IncreaseCurrentWav()
        If LWAV.SelectedIndex = -1 Then
            LWAV.SelectedIndex = 0
        Else
            Dim newIndex As Integer = LWAV.SelectedIndex + 1
            If newIndex > LWAV.Items.Count - 1 Then newIndex = LWAV.Items.Count - 1
            LWAV.SelectedIndices.Clear()
            LWAV.SelectedIndex = newIndex
            ValidateWavListView()
        End If
    End Sub

    Private Sub DecreaseCurrentBmp()
        If LBMP.SelectedIndex = -1 Then
            LBMP.SelectedIndex = 0
        Else
            Dim newIndex As Integer = LBMP.SelectedIndex - 1
            If newIndex < 0 Then newIndex = 0
            LBMP.SelectedIndices.Clear()
            LBMP.SelectedIndex = newIndex
        End If
    End Sub

    Private Sub IncreaseCurrentBmp()
        If LBMP.SelectedIndex = -1 Then
            LBMP.SelectedIndex = 0
        Else
            Dim newIndex As Integer = LBMP.SelectedIndex + 1
            If newIndex > LBMP.Items.Count - 1 Then newIndex = LBMP.Items.Count - 1
            LBMP.SelectedIndices.Clear()
            LBMP.SelectedIndex = newIndex
            ValidateWavListView()
        End If
    End Sub

    Private Sub MoveToBGM(xUndo As UndoRedo.LinkedURCmd, xRedo As UndoRedo.LinkedURCmd)
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        For xI2 As Integer = 1 To UBound(Notes)
            If Not Notes(xI2).Selected Then Continue For

            With Notes(xI2)
                Dim currentBGMColumn As Integer = niB

                'TODO: optimize the for loops below
                If NTInput Then
                    For xI0 As Integer = 1 To UBound(Notes)
                        Dim IntersectA = Notes(xI0).VPosition <= Notes(xI2).VPosition + Notes(xI2).Length
                        Dim IntersectB = Notes(xI0).VPosition + Notes(xI0).Length >= Notes(xI2).VPosition
                        If Notes(xI0).ColumnIndex = currentBGMColumn AndAlso IntersectA And IntersectB Then
                            currentBGMColumn += 1 : xI0 = 1
                        End If
                    Next
                Else
                    For xI0 As Integer = 1 To UBound(Notes)
                        If Notes(xI0).ColumnIndex = currentBGMColumn AndAlso Notes(xI0).VPosition = Notes(xI2).VPosition Then
                            currentBGMColumn += 1 : xI0 = 1
                        End If
                    Next
                End If

                Me.RedoMoveNote(Notes(xI2), currentBGMColumn, .VPosition, xUndo, xRedo)
                .ColumnIndex = currentBGMColumn
            End With
        Next
        AddUndo(xUndo, xBaseRedo.Next)
        UpdatePairing()
        CalculateTotalPlayableNotes()
        RefreshPanelAll()
    End Sub

    Private Sub MoveToColumn(xTargetColumn As Integer, xUndo As UndoRedo.LinkedURCmd, xRedo As UndoRedo.LinkedURCmd)
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo
        If xTargetColumn = -1 Then Return

        If Rscratch Then
            Select Case xTargetColumn
                Case niA2 : xTargetColumn = niA9
                Case niA3 : xTargetColumn = niA1
                Case niA4 : xTargetColumn = niA2
                Case niA5 : xTargetColumn = niA3
                Case niA6 : xTargetColumn = niA4
                Case niA7 : xTargetColumn = niA5
                Case niA8 : xTargetColumn = niA6
                Case niA9 : xTargetColumn = niA7
            End Select
        End If

        If Not nEnabled(xTargetColumn) Then Return
        Dim bMoveAndDeselectFirstNote = My.Computer.Keyboard.ShiftKeyDown

        For xI2 As Integer = 1 To UBound(Notes)
            If Not Notes(xI2).Selected Then Continue For

            RedoMoveNote(Notes(xI2), xTargetColumn, Notes(xI2).VPosition, xUndo, xRedo)
            Notes(xI2).ColumnIndex = xTargetColumn

            If bMoveAndDeselectFirstNote Then
                Notes(xI2).Selected = False
                PanelPreviewNoteIndex(xI2)

                ' az: Add selected notes to undo
                ' to preserve selection status
                ' this works because the note find
                ' does not account for selection status
                ' when checking equality! (equalsBMSE, equalsNT)
                For xI3 As Integer = 1 To UBound(Notes)
                    If xI3 = xI2 Then Continue For
                    If Notes(xI3).Selected Then
                        RedoMoveNote(Notes(xI3), Notes(xI3).ColumnIndex, Notes(xI3).VPosition, xUndo, xRedo)
                    End If
                Next

                Exit For
            End If
        Next
        AddUndo(xUndo, xBaseRedo.Next)
        UpdatePairing()
        CalculateTotalPlayableNotes()
        RefreshPanelAll()
    End Sub

    Private Sub PMainInResize(ByVal sender As Object, ByVal e As System.EventArgs) Handles PMainIn.Resize, PMainInL.Resize, PMainInR.Resize
        If Not Me.Created Then Exit Sub

        Dim iI As Integer = sender.Tag
        If Not IsValidPanelIndex(iI) Then Return

        PanelWidth(iI) = SplitPanes(iI).Container.Width
        Dim xVScroll As EditorScrollBar = GetPanelVScrollBar(iI)
        xVScroll.LargeChange = sender.Height * 0.9
        xVScroll.Maximum = xVScroll.LargeChange - 1

        Dim xPreviousColumns As Integer = gColumns
        UpdateHorizontalScrollMetrics()
        If xPreviousColumns <> gColumns Then
            RefreshPanelAll()
        Else
            RefreshPanel(iI, sender.DisplayRectangle)
        End If
    End Sub

    Private Sub PMainInLostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles PMainIn.LostFocus, PMainInL.LostFocus, PMainInR.LostFocus
        RefreshPanelAll()
    End Sub

    Private Sub PMainInMouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PMainIn.MouseDown, PMainInL.MouseDown, PMainInR.MouseDown
        tempFirstMouseDown = FirstClickDisabled And Not sender.Focused

        PanelFocus = sender.Tag
        If Not IsValidPanelIndex(PanelFocus) Then Return
        sender.Focus()
        LastMouseDownLocation = New Point(-1, -1)
        VSValue = PanelVScroll(PanelFocus)

        If NTInput Then bAdjustUpper = False : bAdjustLength = False
        Me.ctrlPressed = False : Me.DuplicatedSelectedNotes = False

        If MiddleButtonClicked Then MiddleButtonClicked = False : Exit Sub

        Dim xHS As Long = PanelhBMSCROLL(PanelFocus)
        Dim xVS As Long = PanelVScroll(PanelFocus)
        Dim xHeight As Integer = spMain(PanelFocus).Height

        Select Case e.Button
            Case Windows.Forms.MouseButtons.Left
                If tempFirstMouseDown And Not TBTimeSelect.Checked Then RefreshPanelAll() : Exit Select

                KMouseOver = -1
                'If K Is Nothing Then pMouseDown = e.Location : Exit Select

                'Find the clicked K
                Dim NoteIndex As Integer = GetClickedNote(e, xHS, xVS, xHeight)

                PanelPreviewNoteIndex(NoteIndex)

                For xI1 = 0 To UBound(Notes)
                    Notes(xI1).TempMouseDown = False
                Next

                HandleCurrentModeOnClick(e, xHS, xVS, xHeight, NoteIndex)
                RefreshPanelAll()
                POStatusRefresh()

            Case Windows.Forms.MouseButtons.Middle
                If MiddleButtonMoveMethod = 1 Then
                    tempX = e.X
                    tempY = e.Y
                    tempV = xVS
                    tempH = xHS
                Else
                    MiddleButtonLocation = Cursor.Position
                    MiddleButtonClicked = True
                    TimerMiddle.Enabled = True
                End If

            Case Windows.Forms.MouseButtons.Right
                HandleRightClick(sender, e, xHS, xVS, xHeight)
        End Select
    End Sub

    Private Sub HandleRightClick(ByVal sender As Object, ByVal e As MouseEventArgs, ByVal xHS As Long, ByVal xVS As Long, ByVal xHeight As Integer)
        KMouseOver = -1
        'KMouseDown = -1
        ReDim SelectedNotes(-1)
        'If K Is Nothing Then pMouseDown = e.Location : Exit Select

        If Not tempFirstMouseDown Then
            Dim xNoteIndex As Integer = GetRightClickedNote(e, xHS, xVS, xHeight)

            If ShouldRemoveOnRightClick(xNoteIndex) Then
                DeselectOrRemoveNote(xNoteIndex)
                CalculateTotalPlayableNotes()
                Return
            End If
        End If

        ShowEditorContextMenu(TryCast(sender, Control), e, xVS, xHeight)
    End Sub

    Private Function GetRightClickedNote(ByVal e As MouseEventArgs, ByVal xHS As Long, ByVal xVS As Long, ByVal xHeight As Integer) As Integer
        For xI1 As Integer = UBound(Notes) To 1 Step -1
            If MouseInNote(e, xHS, xVS, xHeight, Notes(xI1)) Then Return xI1
        Next

        Return -1
    End Function

    Private Function ShouldRemoveOnRightClick(ByVal xNoteIndex As Integer) As Boolean
        If xNoteIndex <= 0 Then Return False
        If TBWrite.Checked Then Return True
        If TBSelect.Checked Then Return Not Notes(xNoteIndex).Selected

        Return False
    End Function

    Private Sub DeselectOrRemoveNote(ByVal xNoteIndex As Integer)
        If My.Computer.Keyboard.ShiftKeyDown Then
            If Not IsColumnNumeric(Notes(xNoteIndex).ColumnIndex) Then
                If IsColumnSound(Notes(xNoteIndex).ColumnIndex) Then
                    LWAV.SelectedIndices.Clear()
                    Dim xIndex As Integer = Notes(xNoteIndex).Value \ 10000 - 1
                    If xIndex >= 0 AndAlso xIndex < LWAV.Items.Count Then
                        LWAV.SelectedIndex = xIndex
                        ValidateWavListView()
                    End If
                Else
                    LBMP.SelectedIndices.Clear()
                    Dim xIndex As Integer = Notes(xNoteIndex).Value \ 10000 - 1
                    If xIndex >= 0 AndAlso xIndex < LBMP.Items.Count Then
                        LBMP.SelectedIndex = xIndex
                        ValidateBmpListView()
                    End If
                End If
            End If
        Else
            Dim xUndo As UndoRedo.LinkedURCmd = Nothing
            Dim xRedo As UndoRedo.LinkedURCmd = Nothing

            Me.RedoRemoveNote(Notes(xNoteIndex), xUndo, xRedo)
            RemoveNote(xNoteIndex)

            AddUndo(xUndo, xRedo)
            RefreshPanelAll()
        End If
    End Sub

    Private Function GetClickedNote(e As MouseEventArgs, xHS As Long, xVS As Long, xHeight As Integer) As Integer
        Dim NoteIndex As Integer = -1
        For xI1 = UBound(Notes) To 0 Step -1
            'If mouse is clicking on a K
            If MouseInNote(e, xHS, xVS, xHeight, Notes(xI1)) Then
                ' found it!
                NoteIndex = xI1
                deltaVPosition = IIf(NTInput, GetMouseVPosition(False) - Notes(xI1).VPosition, 0)

                If NTInput And My.Computer.Keyboard.ShiftKeyDown Then
                    bAdjustUpper = e.Y <= NoteRowToPanelHeight(Notes(xI1).VPosition + Notes(xI1).Length, xVS, xHeight)
                    bAdjustLength = e.Y >= NoteRowToPanelHeight(Notes(xI1).VPosition, xVS, xHeight) - vo.kHeight Or bAdjustUpper
                End If

                Exit For

            End If
        Next

        Return NoteIndex
    End Function

    Private Sub PanelPreviewNoteIndex(NoteIndex As Integer)
        'Play wav
        If ClickStopPreview Then PreviewNote("", True)
        'My.Computer.Audio.Stop()
        If NoteIndex > 0 And PreviewOnClick AndAlso IsColumnSound(Notes(NoteIndex).ColumnIndex) Then
            Dim xI2 As Integer = Notes(NoteIndex).Value \ 10000
            If Notes(NoteIndex).Landmine Then
                xI2 = 0
            Else
                If xI2 <= 0 Then xI2 = 1
                If xI2 > MaxDefinition Then xI2 = MaxDefinition
            End If

            If Not hWAV(xI2) = "" Then ' AndAlso Path.GetExtension(hWAV(xI2)).ToLower = ".wav" Then
                Dim xFileLocation As String = GetBMSFilePath(hWAV(xI2))
                If Not ClickStopPreview Then PreviewNote("", True)
                PreviewNote(xFileLocation, False)
            End If
        End If
    End Sub

    Private Sub HandleCurrentModeOnClick(e As MouseEventArgs, xHS As Long, xVS As Long, xHeight As Integer, ByRef NoteIndex As Integer)
        If TBSelect.Checked Then
            OnSelectModeLeftClick(e, NoteIndex, xHeight, xVS)
        ElseIf NTInput And TBWrite.Checked Then
            TempVPosition = -1
            SelectedColumn = -1
            ShouldDrawTempNote = False

            Dim xVPosition = GetMouseVPosition(gSnap)

            If xVPosition < 0 Or xVPosition >= GetMaxVPosition() Then Exit Sub

            Dim xColumn = GetColumnAtEvent(e, xHS)

            For xI2 As Integer = UBound(Notes) To 1 Step -1
                If Notes(xI2).VPosition = xVPosition And Notes(xI2).ColumnIndex = xColumn Then NoteIndex = xI2 : Exit For
            Next

            Dim Hidden As Boolean = ModifierHiddenActive()

            If NoteIndex > 0 Then
                ReDim SelectedNotes(0)
                SelectedNotes(0) = Notes(NoteIndex)
                Notes(NoteIndex).TempIndex = 0

                'KMouseDown = xITemp
                Notes(NoteIndex).TempMouseDown = True
                Notes(NoteIndex).Length = xVPosition - Notes(NoteIndex).VPosition

                'uVPos = K(xITemp).VPosition
                bAdjustUpper = True

                Dim xUndo As UndoRedo.LinkedURCmd = Nothing
                Dim xRedo As UndoRedo.LinkedURCmd = Nothing


                Me.RedoLongNoteModify(SelectedNotes(0), Notes(NoteIndex).VPosition, Notes(NoteIndex).Length, xUndo, xRedo)
                AddUndo(xUndo, xRedo)
                'With uNote
                '    AddUndo(sCmdKL(.ColumnIndex, .VPosition, .Value, K(xITemp).Length, .Hidden, .Length, True, True), _
                '            sCmdKL(.ColumnIndex, .VPosition, .Value, .Length, .Hidden, K(xITemp).Length, True, True))
                'End With

            ElseIf IsColumnNumeric(xColumn) Then

                Dim xMessage As String = Strings.Messages.PromptEnterNumeric
                If xColumn = niBPM Then xMessage = Strings.Messages.PromptEnterBPM
                If xColumn = niSTOP Then xMessage = Strings.Messages.PromptEnterSTOP
                If xColumn = niSCROLL Then xMessage = Strings.Messages.PromptEnterSCROLL

                Dim valstr As String = InputBox(xMessage, Text)
                Dim value As Double = Val(valstr) * 10000

                If (xColumn = niSCROLL And valstr = "0") Or value <> 0 Then
                    If xColumn <> niSCROLL And value <= 0 Then value = 1

                    Dim xUndo As UndoRedo.LinkedURCmd = Nothing
                    Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
                    Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

                    For xI1 = 1 To UBound(Notes)
                        If Notes(xI1).VPosition = xVPosition AndAlso Notes(xI1).ColumnIndex = xColumn Then _
                            RedoRemoveNote(Notes(xI1), xUndo, xRedo)
                    Next

                    Dim n = New Note(xColumn, xVPosition, value, 0, Hidden)
                    RedoAddNote(n, xUndo, xRedo)

                    AddNote(n)
                    AddUndo(xUndo, xBaseRedo.Next)
                End If

                ShouldDrawTempNote = True

            Else
                Dim xLbl As Integer
                If IsColumnSound(xColumn) Then
                    xLbl = (LWAV.SelectedIndex + 1) * 10000
                Else
                    xLbl = (LBMP.SelectedIndex + 1) * 10000
                End If

                Dim Landmine As Boolean = ModifierLandmineActive()

                ReDim Preserve Notes(UBound(Notes) + 1)
                With Notes(UBound(Notes))
                    .VPosition = xVPosition
                    .ColumnIndex = xColumn
                    .Value = xLbl
                    .Hidden = Hidden
                    .Landmine = Landmine
                    .TempMouseDown = True
                End With

                ReDim SelectedNotes(0)
                SelectedNotes(0) = Notes(UBound(Notes))
                SelectedNotes(0).LNPair = -1

                If TBWavIncrease.Checked Then
                    If IsColumnSound(xColumn) Then
                        IncreaseCurrentWav()
                    Else
                        IncreaseCurrentBmp()
                    End If
                End If

                'KMouseDown = 1

                'uNote.Value = 0
                'uVPos = xVPosition
                uAdded = False

                Dim xUndo As UndoRedo.LinkedURCmd = Nothing
                Dim xRedo As UndoRedo.LinkedURCmd = Nothing
                RedoAddNote(Notes(UBound(Notes)), xUndo, xRedo, TBWavIncrease.Checked)
                AddUndo(xUndo, xRedo)
            End If

            SortByVPositionInsertion()
            UpdatePairing()
            CalculateTotalPlayableNotes()

        ElseIf TBTimeSelect.Checked Then

            Dim xL1 As Double
            If NoteIndex >= 0 Then xL1 = Notes(NoteIndex).VPosition _
                           Else xL1 = (xHeight - xVS * gxHeight - e.Y - 1) / gxHeight

            vSelAdjust = ModifierLongNoteActive()

            vSelMouseOverLine = 0
            If Math.Abs(e.Y - NoteRowToPanelHeight(vSelStart + vSelLength, xVS, xHeight)) <= vo.PEDeltaMouseOver Then
                vSelMouseOverLine = 3
            ElseIf Math.Abs(e.Y - NoteRowToPanelHeight(vSelStart + vSelHalf, xVS, xHeight)) <= vo.PEDeltaMouseOver Then
                vSelMouseOverLine = 2
            ElseIf Math.Abs(e.Y - NoteRowToPanelHeight(vSelStart, xVS, xHeight)) <= vo.PEDeltaMouseOver Then
                vSelMouseOverLine = 1
            End If

            If Not vSelAdjust Then
                If vSelMouseOverLine = 1 Then
                    If gSnap And NoteIndex <= 0 Then xL1 = SnapToGrid(xL1)
                    vSelLength += vSelStart - xL1
                    vSelHalf += vSelStart - xL1
                    vSelStart = xL1

                ElseIf vSelMouseOverLine = 2 Then
                    vSelHalf = xL1
                    If gSnap And NoteIndex <= 0 Then vSelHalf = SnapToGrid(vSelHalf)
                    vSelHalf -= vSelStart

                ElseIf vSelMouseOverLine = 3 Then
                    vSelLength = xL1
                    If gSnap And NoteIndex <= 0 Then vSelLength = SnapToGrid(vSelLength)
                    vSelLength -= vSelStart

                Else
                    vSelLength = 0
                    vSelStart = xL1
                    If gSnap And NoteIndex <= 0 Then vSelStart = SnapToGrid(vSelStart)
                End If
                ValidateSelection()

            Else
                If vSelMouseOverLine = 2 Then
                    SortByVPositionInsertion()
                    vSelPStart = vSelStart
                    vSelPLength = vSelLength
                    vSelPHalf = vSelHalf
                    vSelK = Notes
                    ReDim Preserve vSelK(UBound(vSelK))

                    If gSnap And NoteIndex <= 0 And Not My.Computer.Keyboard.CtrlKeyDown Then xL1 = SnapToGrid(xL1)
                    AddUndo(New UndoRedo.Void, New UndoRedo.Void)
                    BPMChangeHalf(xL1 - vSelHalf - vSelStart, , True)
                    SortByVPositionInsertion()
                    UpdatePairing()
                    CalculateGreatestVPosition()

                ElseIf vSelMouseOverLine = 3 Or vSelMouseOverLine = 1 Then
                    SortByVPositionInsertion()
                    vSelPStart = vSelStart
                    vSelPLength = vSelLength
                    vSelPHalf = vSelHalf
                    vSelK = Notes
                    ReDim Preserve vSelK(UBound(vSelK))

                    If gSnap And NoteIndex <= 0 And Not My.Computer.Keyboard.CtrlKeyDown Then xL1 = SnapToGrid(xL1)
                    AddUndo(New UndoRedo.Void, New UndoRedo.Void)
                    BPMChangeTop(IIf(vSelMouseOverLine = 3, xL1 - vSelStart, vSelStart + vSelLength - xL1) / vSelLength, , True)
                    SortByVPositionInsertion()
                    UpdatePairing()
                    CalculateGreatestVPosition()

                Else
                    vSelLength = xL1
                    If gSnap And NoteIndex <= 0 And Not My.Computer.Keyboard.CtrlKeyDown Then vSelLength = SnapToGrid(vSelLength)
                    vSelLength -= vSelStart
                End If

            End If

            If vSelLength Then
                Dim xVLower As Double = IIf(vSelLength > 0, vSelStart, vSelStart + vSelLength)
                Dim xVUpper As Double = IIf(vSelLength < 0, vSelStart, vSelStart + vSelLength)
                If NTInput Then
                    For xI2 As Integer = 1 To UBound(Notes)
                        Notes(xI2).Selected = Not Notes(xI2).VPosition >= xVUpper And Not Notes(xI2).VPosition + Notes(xI2).Length < xVLower And nEnabled(Notes(xI2).ColumnIndex)
                    Next
                Else
                    For xI2 As Integer = 1 To UBound(Notes)
                        Notes(xI2).Selected = Notes(xI2).VPosition >= xVLower And Notes(xI2).VPosition < xVUpper And nEnabled(Notes(xI2).ColumnIndex)
                    Next
                End If
            Else
                For xI2 As Integer = 1 To UBound(Notes)
                    Notes(xI2).Selected = False
                Next
            End If

        End If
    End Sub

    Private Sub OnSelectModeLeftClick(e As MouseEventArgs, NoteIndex As Integer, xTHeight As Integer, xVS As Integer)
        If NoteIndex >= 0 And e.Clicks = 2 Then
            DoubleClickNoteIndex(NoteIndex)
        ElseIf NoteIndex > 0 Then
            'KMouseDown = -1
            ReDim SelectedNotes(-1)

            'KMouseDown = xITemp
            Notes(NoteIndex).TempMouseDown = True

            If My.Computer.Keyboard.CtrlKeyDown And Not ModifierMultiselectActive() Then
                'If Not K(xITemp).Selected Then K(xITemp).Selected = True
                ctrlPressed = True

            ElseIf ModifierMultiselectActive() Then
                For xI1 = 0 To UBound(Notes)
                    If IsNoteVisible(xI1, xTHeight, xVS) Then
                        If IsLabelMatch(Notes(xI1), NoteIndex) Then
                            Notes(xI1).Selected = Not Notes(xI1).Selected
                        End If
                    End If
                Next
            Else
                ' az description: If the clicked note is not selected, select only this one.
                'Otherwise, we clicked an already selected note
                'and we should rebuild the selected note array.
                If Not Notes(NoteIndex).Selected Then
                    For xI1 = 0 To UBound(Notes)
                        If Notes(xI1).Selected Then Notes(xI1).Selected = False
                    Next
                    Notes(NoteIndex).Selected = True
                End If

                Dim SelectedCount As Integer = 0
                For xI1 = 0 To UBound(Notes)
                    If Notes(xI1).Selected Then SelectedCount += 1
                Next

                ' adjustsingle if selectedcount is 1
                bAdjustSingle = SelectedCount = 1

                ReDim SelectedNotes(SelectedCount)
                SelectedNotes(0) = Notes(NoteIndex)
                Notes(NoteIndex).TempIndex = 0
                Dim idx = 1

                ' Add already selected notes including this one
                For xI1 = 1 To NoteIndex - 1
                    If Notes(xI1).Selected Then
                        Notes(xI1).TempIndex = idx
                        SelectedNotes(idx) = Notes(xI1)
                        idx += 1
                    End If
                Next
                For xI1 = NoteIndex + 1 To UBound(Notes)
                    If Notes(xI1).Selected Then
                        Notes(xI1).TempIndex = idx
                        SelectedNotes(idx) = Notes(xI1)
                        idx += 1
                    End If
                Next

                'uCol = RealColumnToEnabled(K(xITemp).ColumnIndex)
                'uVPos = K(xITemp).VPosition
                'uNote = K(xITemp)
                uAdded = False

            End If

        Else
            ReDim SelectedNotes(-1)
            LastMouseDownLocation = e.Location
            If Not My.Computer.Keyboard.CtrlKeyDown Then
                For xI1 = 0 To UBound(Notes)
                    Notes(xI1).Selected = False
                    Notes(xI1).TempSelected = False
                Next
            Else
                For xI1 = 0 To UBound(Notes)
                    Notes(xI1).TempSelected = Notes(xI1).Selected
                Next
            End If
        End If
    End Sub

    ' Handles a double click on a note in select mode.
    Private Sub DoubleClickNoteIndex(NoteIndex As Integer)
        Dim Note As Note = Notes(NoteIndex)
        Dim NoteColumn As Integer = Note.ColumnIndex

        If IsColumnNumeric(NoteColumn) Then
            'BPM/Stop prompt
            Dim xMessage As String = Strings.Messages.PromptEnterNumeric
            If NoteColumn = niBPM Then xMessage = Strings.Messages.PromptEnterBPM
            If NoteColumn = niSTOP Then xMessage = Strings.Messages.PromptEnterSTOP
            If NoteColumn = niSCROLL Then xMessage = Strings.Messages.PromptEnterSCROLL


            Dim valstr As String = InputBox(xMessage, Me.Text)
            Dim PromptValue As Double = Val(valstr) * 10000
            If (NoteColumn = niSCROLL And valstr = "0") Or PromptValue <> 0 Then

                Dim xUndo As UndoRedo.LinkedURCmd = Nothing
                Dim xRedo As UndoRedo.LinkedURCmd = Nothing
                RedoRelabelNote(Note, PromptValue, xUndo, xRedo)
                If NoteIndex = 0 Then
                    THBPM.Value = PromptValue / 10000
                Else
                    Notes(NoteIndex).Value = PromptValue
                End If
                AddUndo(xUndo, xRedo)
            End If
        Else
            'Label prompt
            Dim xStr As String = Trim(InputBox(Strings.Messages.PromptEnter, Me.Text))
            If Not UseBase62Definitions Then xStr = UCase(xStr)

            If Len(xStr) = 0 Then Return

            If IsDefinitionLabel(xStr) And Not (xStr = "00" Or xStr = "0") Then
                Dim xUndo As UndoRedo.LinkedURCmd = Nothing
                Dim xRedo As UndoRedo.LinkedURCmd = Nothing
                Dim xValue As Integer = DefinitionIndex(xStr) * 10000
                RedoRelabelNote(Note, xValue, xUndo, xRedo)
                Notes(NoteIndex).Value = xValue
                AddUndo(xUndo, xRedo)
                Return
            Else
                MsgBox(Strings.Messages.InvalidLabel, MsgBoxStyle.Critical, Strings.Messages.Err)
            End If

        End If
    End Sub

    Private Function MouseInNote(e As MouseEventArgs, xHS As Long, xVS As Long, xHeight As Integer, note As Note) As Boolean
        Return e.X >= HorizontalPositiontoDisplay(nLeft(note.ColumnIndex), xHS) + 1 And
               e.X <= HorizontalPositiontoDisplay(nLeft(note.ColumnIndex) + GetColumnWidth(note.ColumnIndex), xHS) - 1 And
               e.Y >= NoteRowToPanelHeight(note.VPosition + IIf(NTInput, note.Length, 0), xVS, xHeight) - vo.kHeight And
               e.Y <= NoteRowToPanelHeight(note.VPosition, xVS, xHeight)
    End Function

    Private Sub PMainInMouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles PMainIn.MouseEnter, PMainInL.MouseEnter, PMainInR.MouseEnter
        spMouseOver = sender.Tag
        If Not IsValidPanelIndex(spMouseOver) Then Return
        Dim xPMainIn As Panel = sender
        If AutoFocusMouseEnter AndAlso Me.Focused Then xPMainIn.Focus() : PanelFocus = spMouseOver
        If FirstMouseEnter Then FirstMouseEnter = False : xPMainIn.Focus() : PanelFocus = spMouseOver
    End Sub

    Private Sub PMainInMouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles PMainIn.MouseLeave, PMainInL.MouseLeave, PMainInR.MouseLeave
        KMouseOver = -1
        'KMouseDown = -1
        ReDim SelectedNotes(-1)
        TempVPosition = -1
        SelectedColumn = -1
        RefreshPanelAll()
    End Sub

    Private Sub PMainInMouseMove(ByVal sender As Panel)
        Dim p As Point = sender.PointToClient(Cursor.Position)
        PMainInMouseMove(sender, New MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, 0))
    End Sub

    Private Sub PMainInMouseMove(ByVal sender As Object, ByVal e As MouseEventArgs) Handles PMainIn.MouseMove, PMainInL.MouseMove, PMainInR.MouseMove
        MouseMoveStatus = e.Location

        Dim iI As Integer = sender.Tag
        If Not IsValidPanelIndex(iI) Then Return

        Dim xHS As Long = PanelhBMSCROLL(iI)
        Dim xVS As Long = PanelVScroll(iI)
        Dim xHeight As Integer = spMain(iI).Height
        Dim xWidth As Integer = spMain(iI).Width

        Select Case e.Button
            Case MouseButtons.None
                'If K Is Nothing Then Exit Select
                If MiddleButtonClicked Then Exit Select

                If isFullScreen Then
                    If e.Y < 5 Then ToolStripContainer1.TopToolStripPanelVisible = True Else ToolStripContainer1.TopToolStripPanelVisible = False
                End If

                Dim xMouseRemainInSameRegion As Boolean = False

                Dim noteIndex As Integer
                Dim foundNoteIndex As Integer = -1
                For noteIndex = UBound(Notes) To 0 Step -1
                    If MouseInNote(e, xHS, xVS, xHeight, Notes(noteIndex)) Then
                        foundNoteIndex = noteIndex

                        xMouseRemainInSameRegion = foundNoteIndex = KMouseOver
                        If NTInput Then
                            Dim vy = NoteRowToPanelHeight(Notes(noteIndex).VPosition + Notes(noteIndex).Length,
                                                                                             xVS, xHeight)

                            Dim xbAdjustUpper As Boolean = (e.Y <= vy) And ModifierLongNoteActive()
                            Dim xbAdjustLength As Boolean = (e.Y >= vy - vo.kHeight Or xbAdjustUpper) And ModifierLongNoteActive()
                            xMouseRemainInSameRegion = xMouseRemainInSameRegion And xbAdjustUpper = bAdjustUpper And xbAdjustLength = bAdjustLength
                            bAdjustUpper = xbAdjustUpper
                            bAdjustLength = xbAdjustLength
                        End If

                        Exit For
                    End If
                Next

                Dim xTempbTimeSelectionMode As Boolean = TBTimeSelect.Checked

                If TBSelect.Checked Or xTempbTimeSelectionMode Then

                    If xMouseRemainInSameRegion Then Exit Select
                    If KMouseOver >= 0 Then KMouseOver = -1

                    If xTempbTimeSelectionMode Then

                        Dim xMouseOverLine As Integer = vSelMouseOverLine
                        vSelMouseOverLine = 0

                        If Math.Abs(e.Y - NoteRowToPanelHeight(vSelStart + vSelLength, xVS, xHeight)) <= vo.PEDeltaMouseOver Then
                            vSelMouseOverLine = 3
                        ElseIf Math.Abs(e.Y - NoteRowToPanelHeight(vSelStart + vSelHalf, xVS, xHeight)) <= vo.PEDeltaMouseOver Then
                            vSelMouseOverLine = 2
                        ElseIf Math.Abs(e.Y - NoteRowToPanelHeight(vSelStart, xVS, xHeight)) <= vo.PEDeltaMouseOver Then
                            vSelMouseOverLine = 1
                        End If

                    End If

                    ' draw green highlight
                    If foundNoteIndex > -1 Then
                        DrawNoteHoverHighlight(iI, xHS, xVS, xHeight, foundNoteIndex)
                    End If

                    KMouseOver = foundNoteIndex

                ElseIf TBWrite.Checked Then
                    TempVPosition = (xHeight - xVS * gxHeight - e.Y - 1) / gxHeight 'VPosition of the mouse
                    If gSnap Then TempVPosition = SnapToGrid(TempVPosition)

                    SelectedColumn = GetColumnAtEvent(e, xHS)  'get the enabled column where mouse is 

                    TempLength = 0
                    If foundNoteIndex > -1 Then TempLength = Notes(foundNoteIndex).Length

                    RefreshPanelAll()
                End If

            Case MouseButtons.Left
                If tempFirstMouseDown And Not TBTimeSelect.Checked Then Exit Select

                tempX = 0
                tempY = 0
                If e.X < 0 Or e.X > xWidth Or e.Y < 0 Or e.Y > xHeight Then
                    If e.X < 0 Then tempX = e.X
                    If e.X > xWidth Then tempX = e.X - xWidth
                    If e.Y < 0 Then tempY = e.Y
                    If e.Y > xHeight Then tempY = e.Y - xHeight
                    Timer1.Enabled = True
                Else
                    Timer1.Enabled = False
                End If

                If TBSelect.Checked Then

                    pMouseMove = e.Location

                    'If K Is Nothing Then RefreshPanelAll() : Exit Select

                    If Not LastMouseDownLocation = New Point(-1, -1) Then
                        UpdateSelectionBox(xHS, xVS, xHeight)

                        'ElseIf Not KMouseDown = -1 Then
                    ElseIf SelectedNotes.Length <> 0 Then
                        UpdateSelectedNotes(xHeight, xVS, xHS, e)

                    ElseIf ctrlPressed Then
                        OnDuplicateSelectedNotes(xHeight, xVS, xHS, e)
                    End If

                ElseIf TBWrite.Checked Then

                    If NTInput Then
                        OnWriteModeMouseMove(xHeight, xVS, e)

                    Else
                        TempVPosition = (xHeight - xVS * gxHeight - e.Y - 1) / gxHeight 'VPosition of the mouse
                        If gSnap Then TempVPosition = SnapToGrid(TempVPosition)
                        SelectedColumn = GetColumnAtEvent(e, xHS)  'get the enabled column where mouse is 

                    End If

                ElseIf TBTimeSelect.Checked Then
                    OnTimeSelectClick(xHeight, xHS, xVS, e)
                End If

            Case MouseButtons.Middle
                OnPanelMousePan(e)
        End Select
        Dim col = GetColumnAtEvent(e, xHS)
        Dim vps = GetMouseVPosition(gSnap)
        If vps <> lastVPos Or col <> lastColumn Then
            lastVPos = vps
            lastColumn = col
            POStatusRefresh()
            RefreshPanelAll() 'az: refreshing the line is important now...
        End If

    End Sub

    Dim lastVPos = -1
    Dim lastColumn = -1

    Private Sub UpdateSelectedNotes(xHeight As Double, xvs As Double, xhs As Double, e As MouseEventArgs)
        Dim mouseVPosition As Double

        Dim xITemp As Integer
        For xI1 = 1 To UBound(Notes)
            If Notes(xI1).TempMouseDown Then xITemp = xI1 : Exit For
        Next

        mouseVPosition = GetMouseVPosition(gSnap)

        If bAdjustLength And bAdjustSingle Then
            If bAdjustUpper AndAlso mouseVPosition < Notes(xITemp).VPosition Then
                bAdjustUpper = False
                Notes(xITemp).VPosition += Notes(xITemp).Length
                Notes(xITemp).Length *= -1
            ElseIf Not bAdjustUpper AndAlso mouseVPosition > Notes(xITemp).VPosition + Notes(xITemp).Length Then
                bAdjustUpper = True
                Notes(xITemp).VPosition += Notes(xITemp).Length
                Notes(xITemp).Length *= -1
            End If
        End If

        'If moving
        If Not bAdjustLength Then
            OnSelectModeMoveNotes(e, xhs, xITemp)

        ElseIf bAdjustUpper Then    'If adjusting upper end
            Dim dVPosition = mouseVPosition - Notes(xITemp).VPosition - Notes(xITemp).Length  'delta Length
            '< 0 means shorten, > 0 means lengthen

            OnAdjustUpperEnd(dVPosition)

        Else    'If adjusting lower end
            Dim dVPosition = mouseVPosition - Notes(xITemp).VPosition  'delta VPosition
            '> 0 means shorten, < 0 means lengthen

            OnAdjustLowerEnd(dVPosition)
        End If

        SortByVPositionInsertion()
        UpdatePairing()
        CalculateTotalPlayableNotes(False)
        'Label1.Text = KInfo(KMouseDown)
    End Sub

    Private Sub OnPanelMousePan(e As MouseEventArgs)
        If MiddleButtonMoveMethod = 1 Then
            Dim xI1 As Integer = tempV + (tempY - e.Y) / gxHeight
            Dim xI2 As Integer = tempH + (tempX - e.X) / gxWidth
            If xI1 > 0 Then xI1 = 0
            If xI2 < 0 Then xI2 = 0

            Dim xVScroll As EditorScrollBar = GetPanelVScrollBar(PanelFocus)
            Dim xHScroll As EditorScrollBar = GetPanelHScroll(PanelFocus)
            If xVScroll Is Nothing OrElse xHScroll Is Nothing Then Return

            SetScrollValue(xVScroll, xI1)
            SetScrollValue(xHScroll, xI2)
        End If
    End Sub

    Private Sub OnTimeSelectClick(xHeight As Double, xHS As Double, xvs As Double, e As MouseEventArgs)
        Dim xI1 As Integer
        Dim xITemp As Integer = -1
        If Notes IsNot Nothing Then
            For xI1 = UBound(Notes) To 0 Step -1 ' az: MouseInNote implied, but I'm not sure yet
                If MouseInNote(e, xHS, xvs, xHeight, Notes(xI1)) Then
                    xITemp = xI1
                    Exit For
                End If
            Next
        End If

        If Not vSelAdjust Then
            If vSelMouseOverLine = 1 Then
                Dim xV As Double = (xHeight - xvs * gxHeight - e.Y - 1) / gxHeight
                If xITemp >= 0 Then xV = Notes(xITemp).VPosition
                If gSnap And xITemp <= 0 And Not My.Computer.Keyboard.CtrlKeyDown Then xV = SnapToGrid(xV)
                vSelLength += vSelStart - xV
                vSelHalf += vSelStart - xV
                vSelStart = xV

            ElseIf vSelMouseOverLine = 2 Then
                vSelHalf = (xHeight - xvs * gxHeight - e.Y - 1) / gxHeight
                If xITemp >= 0 Then vSelHalf = Notes(xITemp).VPosition
                If gSnap And xITemp <= 0 And Not My.Computer.Keyboard.CtrlKeyDown Then vSelHalf = SnapToGrid(vSelHalf)
                vSelHalf -= vSelStart

            ElseIf vSelMouseOverLine = 3 Then
                vSelLength = (xHeight - xvs * gxHeight - e.Y - 1) / gxHeight
                If xITemp >= 0 Then vSelLength = Notes(xITemp).VPosition
                If gSnap And xITemp <= 0 And Not My.Computer.Keyboard.CtrlKeyDown Then vSelLength = SnapToGrid(vSelLength)
                vSelLength -= vSelStart

            Else
                If xITemp >= 0 Then
                    vSelLength = Notes(xITemp).VPosition
                Else
                    vSelLength = (xHeight - xvs * gxHeight - e.Y - 1) / gxHeight
                    If gSnap And Not My.Computer.Keyboard.CtrlKeyDown Then vSelLength = SnapToGrid(vSelLength)
                End If
                vSelLength -= vSelStart
                vSelHalf = vSelLength / 2
            End If
            ValidateSelection()

        Else
            Dim xL1 As Double = (xHeight - xvs * gxHeight - e.Y - 1) / gxHeight

            If vSelMouseOverLine = 2 Then
                vSelStart = vSelPStart
                vSelLength = vSelPLength
                vSelHalf = vSelPHalf
                Notes = vSelK
                ReDim Preserve Notes(UBound(Notes))

                If gSnap And Not My.Computer.Keyboard.CtrlKeyDown Then xL1 = SnapToGrid(xL1)
                BPMChangeHalf(xL1 - vSelHalf - vSelStart, , True)
                SortByVPositionInsertion()
                UpdatePairing()
                CalculateGreatestVPosition()

            ElseIf vSelMouseOverLine = 3 Or vSelMouseOverLine = 1 Then
                vSelStart = vSelPStart
                vSelLength = vSelPLength
                vSelHalf = vSelPHalf
                Notes = vSelK
                ReDim Preserve Notes(UBound(Notes))

                If gSnap And Not My.Computer.Keyboard.CtrlKeyDown Then xL1 = SnapToGrid(xL1)
                BPMChangeTop(IIf(vSelMouseOverLine = 3, xL1 - vSelStart, vSelStart + vSelLength - xL1) / vSelLength, , True)
                SortByVPositionInsertion()
                UpdatePairing()
                CalculateGreatestVPosition()

            Else
                vSelLength = xL1
                If gSnap And Not My.Computer.Keyboard.CtrlKeyDown Then vSelLength = SnapToGrid(vSelLength)
                If xITemp >= 0 Then vSelLength = Notes(xITemp).VPosition
                vSelLength -= vSelStart
                ValidateSelection()
            End If
        End If

        If vSelLength Then
            Dim xVLower As Double = IIf(vSelLength > 0, vSelStart, vSelStart + vSelLength)
            Dim xVUpper As Double = IIf(vSelLength < 0, vSelStart, vSelStart + vSelLength)
            If NTInput Then
                For xI2 As Integer = 1 To UBound(Notes)
                    Notes(xI2).Selected = Notes(xI2).VPosition < xVUpper And Notes(xI2).VPosition + Notes(xI2).Length >= xVLower And nEnabled(Notes(xI2).ColumnIndex)
                Next
            Else
                For xI2 As Integer = 1 To UBound(Notes)
                    Notes(xI2).Selected = Notes(xI2).VPosition >= xVLower And Notes(xI2).VPosition < xVUpper And nEnabled(Notes(xI2).ColumnIndex)
                Next
            End If
        Else
            For xI2 As Integer = 1 To UBound(Notes)
                Notes(xI2).Selected = False
            Next
        End If

    End Sub

    Private Sub OnAdjustUpperEnd(dVPosition As Double)
        Dim minLength As Double = 0
        Dim maxHeight As Double = 191999
        For xI1 = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For
            If Notes(xI1).Length + dVPosition < minLength Then minLength = Notes(xI1).Length + dVPosition
            If Notes(xI1).Length + Notes(xI1).VPosition + dVPosition > maxHeight Then maxHeight = Notes(xI1).Length + Notes(xI1).VPosition + dVPosition
        Next
        maxHeight -= 191999

        'declare undo variables
        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        'start moving
        Dim xLen As Double
        For xI1 = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For

            xLen = Notes(xI1).Length + dVPosition - minLength - maxHeight
            RedoLongNoteModify(SelectedNotes(Notes(xI1).TempIndex), Notes(xI1).VPosition, xLen, xUndo, xRedo)

            Notes(xI1).Length = xLen
        Next

        'Add undo
        If dVPosition - minLength - maxHeight <> 0 Then
            AddUndo(xUndo, xBaseRedo.Next, uAdded)
            If Not uAdded Then uAdded = True
        End If
    End Sub


    Private Sub OnAdjustLowerEnd(dVPosition As Double)
        Dim xI1 As Integer
        Dim minLength As Double = 0
        Dim minVPosition As Double = 0
        For xI1 = 1 To UBound(Notes)
            If Notes(xI1).Selected AndAlso Notes(xI1).Length - dVPosition < minLength Then
                minLength = Notes(xI1).Length - dVPosition
            End If
            If Notes(xI1).Selected AndAlso Notes(xI1).VPosition + dVPosition < minVPosition Then
                minVPosition = Notes(xI1).VPosition + dVPosition
            End If
        Next

        'declare undo variables
        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        'start moving
        Dim xVPos As Double
        Dim xLen As Double
        For xI1 = 0 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For

            xVPos = Notes(xI1).VPosition + dVPosition + minLength - minVPosition
            xLen = Notes(xI1).Length - dVPosition - minLength + minVPosition
            Me.RedoLongNoteModify(SelectedNotes(Notes(xI1).TempIndex), xVPos, xLen, xUndo, xRedo)

            Notes(xI1).VPosition = xVPos
            Notes(xI1).Length = xLen
        Next

        'Add undo
        If dVPosition + minLength - minVPosition <> 0 Then
            AddUndo(xUndo, xBaseRedo.Next, uAdded)
            If Not uAdded Then uAdded = True
        End If
    End Sub

    Private Sub OnDuplicateSelectedNotes(xHeight As Double, xVS As Double, xHS As Double, e As MouseEventArgs)
        Dim tempNoteIndex As Integer
        For tempNoteIndex = 1 To UBound(Notes)
            If Notes(tempNoteIndex).TempMouseDown Then Exit For
        Next

        Dim mouseVPosition = GetMouseVPosition(gSnap)
        If DisableVerticalMove Then mouseVPosition = Notes(tempNoteIndex).VPosition

        Dim dVPosition As Double = mouseVPosition - Notes(tempNoteIndex).VPosition  'delta VPosition

        Dim currCol = ColumnArrayIndexToEnabledColumnIndex(GetColumnAtEvent(e, xHS))
        Dim noteCol = ColumnArrayIndexToEnabledColumnIndex(Notes(tempNoteIndex).ColumnIndex)
        Dim colChange As Integer = currCol - noteCol 'delta Column

        'Ks cannot be beyond the left, the upper and the lower boundary
        Dim dstColumn As Integer = 0
        Dim mVPosition As Double = 0
        Dim muVPosition As Double = 191999
        For xI1 = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For

            If ColumnArrayIndexToEnabledColumnIndex(Notes(xI1).ColumnIndex) + colChange < dstColumn Then _
                dstColumn = ColumnArrayIndexToEnabledColumnIndex(Notes(xI1).ColumnIndex) + colChange

            If Notes(xI1).VPosition + dVPosition < mVPosition Then _
                mVPosition = Notes(xI1).VPosition + dVPosition

            If Notes(xI1).VPosition + IIf(NTInput, Notes(xI1).Length, 0) + dVPosition > muVPosition Then _
                muVPosition = Notes(xI1).VPosition + IIf(NTInput, Notes(xI1).Length, 0) + dVPosition

        Next
        muVPosition -= 191999

        'If not moving then exit
        If (Not DuplicatedSelectedNotes) And colChange - dstColumn = 0 And dVPosition - mVPosition - muVPosition = 0 Then _
            Return

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        If Not DuplicatedSelectedNotes Then     'If uAdded = False
            DuplicateSelectedNotes(tempNoteIndex, dVPosition, colChange, dstColumn, mVPosition, muVPosition)
            DuplicatedSelectedNotes = True

        Else
            For i As Integer = 1 To UBound(Notes)
                If Not Notes(i).Selected Then Continue For

                Notes(i).ColumnIndex = EnabledColumnIndexToColumnArrayIndex(ColumnArrayIndexToEnabledColumnIndex(Notes(i).ColumnIndex) + colChange - dstColumn)
                Notes(i).VPosition = Notes(i).VPosition + dVPosition - mVPosition - muVPosition
                Me.RedoAddNote(Notes(i), xUndo, xRedo)
            Next

            AddUndo(xUndo, xBaseRedo.Next, True)
        End If

        SortByVPositionInsertion()
        UpdatePairing()
        CalculateTotalPlayableNotes()
    End Sub


    Private Sub OnWriteModeMouseMove(xHeight As Integer, xVS As Long, e As MouseEventArgs)
        'If Not KMouseDown = -1 Then
        If SelectedNotes.Length <> 0 Then

            Dim xI1 As Integer
            Dim xITemp As Integer
            For xI1 = 1 To UBound(Notes)
                If Notes(xI1).TempMouseDown Then xITemp = xI1 : Exit For
            Next

            Dim mouseVPosition = GetMouseVPosition(gSnap)

            With Notes(xITemp)
                If bAdjustUpper AndAlso mouseVPosition < .VPosition Then
                    bAdjustUpper = False
                    .VPosition += .Length
                    .Length *= -1
                ElseIf Not bAdjustUpper AndAlso mouseVPosition > .VPosition + .Length Then
                    bAdjustUpper = True
                    .VPosition += .Length
                    .Length *= -1
                End If

                If bAdjustUpper Then
                    .Length = mouseVPosition - .VPosition
                Else
                    .Length = .VPosition + .Length - mouseVPosition
                    .VPosition = mouseVPosition
                End If

                If .VPosition < 0 Then .Length += .VPosition : .VPosition = 0
                If .VPosition + .Length >= GetMaxVPosition() Then .Length = GetMaxVPosition() - 1 - .VPosition

                If SelectedNotes(0).LNPair = -1 Then 'If new note
                    Dim xUndo As UndoRedo.LinkedURCmd = Nothing
                    Dim xRedo As UndoRedo.LinkedURCmd = Nothing
                    Me.RedoAddNote(Notes(xITemp), xUndo, xRedo)
                    AddUndo(xUndo, xRedo, True)

                Else 'If existing note
                    Dim xUndo As UndoRedo.LinkedURCmd = Nothing
                    Dim xRedo As UndoRedo.LinkedURCmd = Nothing
                    Me.RedoLongNoteModify(SelectedNotes(0), .VPosition, .Length, xUndo, xRedo)
                    AddUndo(xUndo, xRedo, True)
                End If

                SelectedColumn = .ColumnIndex
                TempVPosition = mouseVPosition
                TempLength = .Length

            End With

            SortByVPositionInsertion()
            UpdatePairing()
            CalculateTotalPlayableNotes()

        End If
    End Sub

    Private Sub OnSelectModeMoveNotes(e As MouseEventArgs, xHS As Long, xITemp As Integer)
        Dim mouseVPosition = GetMouseVPosition(gSnap)
        If DisableVerticalMove Then mouseVPosition = SelectedNotes(0).VPosition
        Dim dVPosition = mouseVPosition - Notes(xITemp).VPosition  'delta VPosition

        Dim mouseColumn As Integer
        Dim xI1 = 0
        Dim mLeft As Integer = e.X / gxWidth + xHS 'horizontal position of the mouse
        If mLeft >= 0 Then
            Do
                If mLeft < nLeft(xI1 + 1) Or xI1 >= gColumns Then mouseColumn = ColumnArrayIndexToEnabledColumnIndex(xI1) : Exit Do 'get the column where mouse is 
                xI1 += 1
            Loop
        End If

        Dim dColumn = mouseColumn - ColumnArrayIndexToEnabledColumnIndex(Notes(xITemp).ColumnIndex) 'get the enabled delta column where mouse is 

        'Ks cannot be beyond the left, the upper and the lower boundary
        mLeft = 0
        Dim mVPosition As Double = 0
        Dim muVPosition As Double = 191999
        For xI1 = 1 To UBound(Notes)
            If Notes(xI1).Selected Then
                mLeft = IIf(ColumnArrayIndexToEnabledColumnIndex(Notes(xI1).ColumnIndex) + dColumn < mLeft,
                            ColumnArrayIndexToEnabledColumnIndex(Notes(xI1).ColumnIndex) + dColumn,
                            mLeft)
                mVPosition = IIf(Notes(xI1).VPosition + dVPosition < mVPosition,
                                 Notes(xI1).VPosition + dVPosition,
                                 mVPosition)
                muVPosition = IIf(Notes(xI1).VPosition + IIf(NTInput, Notes(xI1).Length, 0) + dVPosition > muVPosition,
                                  Notes(xI1).VPosition + IIf(NTInput, Notes(xI1).Length, 0) + dVPosition,
                                  muVPosition)
            End If
        Next
        muVPosition -= 191999

        Dim xCol As Integer
        Dim xVPos As Double

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        'start moving
        For xI1 = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For

            xCol = EnabledColumnIndexToColumnArrayIndex(ColumnArrayIndexToEnabledColumnIndex(Notes(xI1).ColumnIndex) + dColumn - mLeft)
            xVPos = Notes(xI1).VPosition + dVPosition - mVPosition - muVPosition
            Me.RedoMoveNote(SelectedNotes(Notes(xI1).TempIndex), xCol, xVPos, xUndo, xRedo)

            Notes(xI1).ColumnIndex = xCol
            Notes(xI1).VPosition = xVPos
        Next

        'If mouseColumn - uNotes(0).ColumnIndex - mLeft <> 0 Or mouseVPosition - uNotes(0).VPosition - mVPosition - muVPosition <> 0 Then
        AddUndo(xUndo, xBaseRedo.Next, uAdded)
        If Not uAdded Then uAdded = True

        'End If
    End Sub

    Private Sub UpdateSelectionBox(xHS As Long, xVS As Long, xHeight As Integer)
        Dim SelectionBox As New Rectangle(IIf(pMouseMove.X > LastMouseDownLocation.X, LastMouseDownLocation.X, pMouseMove.X),
                                                           IIf(pMouseMove.Y > LastMouseDownLocation.Y, LastMouseDownLocation.Y, pMouseMove.Y),
                                                           Math.Abs(pMouseMove.X - LastMouseDownLocation.X),
                                                           Math.Abs(pMouseMove.Y - LastMouseDownLocation.Y))
        Dim NoteRect As Rectangle

        Dim xI1 As Integer
        For xI1 = 1 To UBound(Notes)
            NoteRect = New Rectangle(HorizontalPositiontoDisplay(nLeft(Notes(xI1).ColumnIndex), xHS) + 1,
                                  NoteRowToPanelHeight(Notes(xI1).VPosition + IIf(NTInput, Notes(xI1).Length, 0), xVS, xHeight) - vo.kHeight,
                                  GetColumnWidth(Notes(xI1).ColumnIndex) * gxWidth - 2,
                                  vo.kHeight + IIf(NTInput, Notes(xI1).Length * gxHeight, 0))


            If NoteRect.IntersectsWith(SelectionBox) Then
                Notes(xI1).Selected = Not Notes(xI1).TempSelected And nEnabled(Notes(xI1).ColumnIndex)
            Else
                Notes(xI1).Selected = Notes(xI1).TempSelected And nEnabled(Notes(xI1).ColumnIndex)
            End If
        Next
    End Sub

    Private Sub DuplicateSelectedNotes(tempNoteIndex As Integer, dVPosition As Double, dColumn As Integer, mLeft As Integer, mVPosition As Double, muVPosition As Double)
        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        Notes(tempNoteIndex).Selected = True

        Dim xSelectedNotesCount As Integer = 0
        For i As Integer = 1 To UBound(Notes)
            If Notes(i).Selected Then xSelectedNotesCount += 1
        Next

        Dim xTempNotes(xSelectedNotesCount - 1) As Note
        Dim xI2 As Integer = 0
        For i As Integer = 1 To UBound(Notes)
            If Not Notes(i).Selected Then Continue For

            xTempNotes(xI2) = Notes(i)
            xTempNotes(xI2).ColumnIndex = EnabledColumnIndexToColumnArrayIndex(ColumnArrayIndexToEnabledColumnIndex(Notes(i).ColumnIndex) + dColumn - mLeft)
            xTempNotes(xI2).VPosition = Notes(i).VPosition + dVPosition - mVPosition - muVPosition
            Me.RedoAddNote(xTempNotes(xI2), xUndo, xRedo)

            Notes(i).Selected = False
            xI2 += 1
        Next
        Notes(tempNoteIndex).TempMouseDown = False

        'copy to K
        Dim xOrigUBound As Integer = UBound(Notes)
        ReDim Preserve Notes(xOrigUBound + xSelectedNotesCount)
        xI2 = 0
        For i As Integer = xOrigUBound + 1 To UBound(Notes)
            Notes(i) = xTempNotes(xI2)
            xI2 += 1
        Next

        AddUndo(xUndo, xBaseRedo.Next)
    End Sub

    Private Sub DrawNoteHoverHighlight(iI As Integer, xHS As Long, xVS As Long, xHeight As Integer, foundNoteIndex As Integer)
        Dim xDispX As Integer = HorizontalPositiontoDisplay(nLeft(Notes(foundNoteIndex).ColumnIndex), xHS)
        Dim xDispY As Integer = IIf(Not NTInput Or (bAdjustLength And Not bAdjustUpper),
                                    NoteRowToPanelHeight(Notes(foundNoteIndex).VPosition, xVS, xHeight) - vo.kHeight - 1,
                                    NoteRowToPanelHeight(Notes(foundNoteIndex).VPosition + Notes(foundNoteIndex).Length, xVS, xHeight) - vo.kHeight - 1)
        Dim xDispW As Integer = GetColumnWidth(Notes(foundNoteIndex).ColumnIndex) * gxWidth + 1
        Dim xDispH As Integer = IIf(Not NTInput Or bAdjustLength,
                                    vo.kHeight + 3,
                                    Notes(foundNoteIndex).Length * gxHeight + vo.kHeight + 3)

        Dim e1 As BufferedGraphics = BufferedGraphicsManager.Current.Allocate(spMain(iI).CreateGraphics, New Rectangle(xDispX, xDispY, xDispW, xDispH))
        e1.Graphics.FillRectangle(vo.Bg, New Rectangle(xDispX, xDispY, xDispW, xDispH))

        If NTInput Then DrawNoteNT(Notes(foundNoteIndex), e1, xHS, xVS, xHeight) Else DrawNote(Notes(foundNoteIndex), e1, xHS, xVS, xHeight)

        e1.Graphics.DrawRectangle(IIf(bAdjustLength, vo.kMouseOverE, vo.kMouseOver), xDispX, xDispY, xDispW - 1, xDispH - 1)

        e1.Render(spMain(iI).CreateGraphics)
        e1.Dispose()
    End Sub

    Private Function GetColumnAtX(x As Integer, xHS As Integer) As Integer
        Dim xI1 As Integer = 0
        Dim mLeft As Integer = x / gxWidth + xHS 'horizontal position of the mouse
        Dim xColumn = 0
        If mLeft >= 0 Then
            Do
                If mLeft < nLeft(xI1 + 1) Or xI1 >= gColumns Then xColumn = xI1 : Exit Do 'get the column where mouse is 
                xI1 += 1
            Loop
        End If

        Return EnabledColumnIndexToColumnArrayIndex(ColumnArrayIndexToEnabledColumnIndex(xColumn))  'get the enabled column where mouse is 
    End Function

    Private Function GetColumnAtEvent(e As MouseEventArgs, xHS As Integer)
        Return GetColumnAtX(e.X, xHS)
    End Function

    ' az: Handle zoom in/out. Should work with any of the three splitters.
    Private Sub PMain_Scroll(sender As Object, e As MouseEventArgs) Handles PMainIn.MouseWheel, PMainInL.MouseWheel, PMainInR.MouseWheel
        If Not My.Computer.Keyboard.CtrlKeyDown Then Exit Sub

        If My.Computer.Keyboard.ShiftKeyDown Then
            HandleGridWidthMouseWheel(e.Delta)
            Exit Sub
        End If

        Dim dv = Math.Round(CGHeight2.Value + e.Delta / 120)
        CGHeight2.Value = Math.Min(CGHeight2.Maximum, Math.Max(CGHeight2.Minimum, dv))
        CGHeight.Value = CGHeight2.Value / 4
    End Sub


    Private Sub PMainInMouseUp(ByVal sender As Object, ByVal e As MouseEventArgs) Handles PMainIn.MouseUp, PMainInL.MouseUp, PMainInR.MouseUp
        tempX = 0
        tempY = 0
        tempV = 0
        tempH = 0
        VSValue = -1
        HSValue = -1
        Timer1.Enabled = False
        'KMouseDown = -1
        ReDim SelectedNotes(-1)

        Dim iI As Integer = sender.Tag
        If Not IsValidPanelIndex(iI) Then Return

        If MiddleButtonClicked AndAlso e.Button = Windows.Forms.MouseButtons.Middle AndAlso
            (MiddleButtonLocation.X - Cursor.Position.X) ^ 2 + (MiddleButtonLocation.Y - Cursor.Position.Y) ^ 2 >= vo.MiddleDeltaRelease Then
            MiddleButtonClicked = False
        End If

        If TBSelect.Checked Then
            LastMouseDownLocation = New Point(-1, -1)
            pMouseMove = New Point(-1, -1)

            If ctrlPressed And Not DuplicatedSelectedNotes And Not ModifierMultiselectActive() Then
                For i As Integer = 1 To UBound(Notes)
                    If Notes(i).TempMouseDown Then Notes(i).Selected = Not Notes(i).Selected : Exit For
                Next
            End If

            ctrlPressed = False
            DuplicatedSelectedNotes = False
            CalculateTotalPlayableNotes()

        ElseIf TBWrite.Checked Then

            If Not NTInput And Not tempFirstMouseDown Then
                Dim xVPosition As Double


                xVPosition = (sender.Height - PanelVScroll(iI) * gxHeight - e.Y - 1) / gxHeight 'VPosition of the mouse
                If gSnap Then xVPosition = SnapToGrid(xVPosition)

                Dim xColumn = GetColumnAtEvent(e, PanelhBMSCROLL(iI))

                If e.Button = Windows.Forms.MouseButtons.Left Then
                    Dim HiddenNote As Boolean = ModifierHiddenActive()
                    Dim LongNote As Boolean = ModifierLongNoteActive()
                    Dim Landmine As Boolean = ModifierLandmineActive()
                    Dim xUndo As UndoRedo.LinkedURCmd = Nothing
                    Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
                    Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

                    If IsColumnNumeric(xColumn) Then
                        Dim xMessage As String = Strings.Messages.PromptEnterNumeric
                        If xColumn = niBPM Then xMessage = Strings.Messages.PromptEnterBPM
                        If xColumn = niSTOP Then xMessage = Strings.Messages.PromptEnterSTOP
                        If xColumn = niSCROLL Then xMessage = Strings.Messages.PromptEnterSCROLL

                        Dim valstr As String = InputBox(xMessage, Me.Text)
                        Dim value As Long = Val(valstr) * 10000

                        If (xColumn = niSCROLL And valstr = "0") Or value <> 0 Then
                            For xI1 = 1 To UBound(Notes)
                                If Notes(xI1).VPosition = xVPosition AndAlso Notes(xI1).ColumnIndex = xColumn Then _
                            RedoRemoveNote(Notes(xI1), xUndo, xRedo)
                            Next

                            Dim n = New Note(xColumn, xVPosition, value, LongNote, HiddenNote)
                            RedoAddNote(n, xUndo, xRedo)
                            AddNote(n)

                            AddUndo(xUndo, xBaseRedo.Next)
                        End If

                    Else
                        Dim xValue As Integer
                        If IsColumnSound(xColumn) Then
                            xValue = (LWAV.SelectedIndex + 1) * 10000
                        Else
                            xValue = (LBMP.SelectedIndex + 1) * 10000
                        End If

                        For xI1 = 1 To UBound(Notes)
                            If Notes(xI1).VPosition = xVPosition AndAlso Notes(xI1).ColumnIndex = xColumn Then _
                            RedoRemoveNote(Notes(xI1), xUndo, xRedo)
                        Next

                        Dim n = New Note(xColumn, xVPosition, xValue,
                                         LongNote, HiddenNote, True, Landmine)

                        RedoAddNote(n, xUndo, xRedo)
                        AddNote(n)

                        AddUndo(xUndo, xRedo)
                    End If
                End If
            End If

            If Not ShouldDrawTempNote Then ShouldDrawTempNote = True
            TempVPosition = -1
            SelectedColumn = -1
        End If
        CalculateGreatestVPosition()
        RefreshPanelAll()
    End Sub

    Private Sub PMainInMouseWheel(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles PMainIn.MouseWheel, PMainInL.MouseWheel, PMainInR.MouseWheel
        If MiddleButtonClicked Then MiddleButtonClicked = False
        If My.Computer.Keyboard.CtrlKeyDown Then Exit Sub

        If My.Computer.Keyboard.ShiftKeyDown Then
            HandleHorizontalMouseWheel(e.Delta)
            Exit Sub
        End If

        Dim xScroll As EditorScrollBar = GetPanelVScrollBar(spMouseOver)
        If xScroll Is Nothing Then Return

        Dim xI1 As Integer = PanelVScroll(spMouseOver) - Math.Sign(e.Delta) * gWheel
        SetScrollValue(xScroll, xI1)
    End Sub

    Private Sub HandleHorizontalMouseWheel(ByVal delta As Integer)
        Dim xScroll As EditorScrollBar = GetPanelHScroll(spMouseOver)

        If xScroll Is Nothing Then
            Return
        End If

        Dim xI1 As Integer = PanelhBMSCROLL(spMouseOver) - Math.Sign(delta) * gWheel
        SetScrollValue(xScroll, xI1)
    End Sub

    Private Sub PMainInPaint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles PMainIn.Paint, PMainInL.Paint, PMainInR.Paint
        If Not IsValidPanelIndex(CInt(sender.Tag)) Then Return
        RefreshPanel(sender.Tag, e.ClipRectangle)
    End Sub
End Class
