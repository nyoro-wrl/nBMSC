Imports iBMSC.Editor

Partial Public Class MainWindow
    Private Sub PerformCommand(ByVal sCmd As UndoRedo.LinkedURCmd)
        For xI2 As Integer = 1 To UBound(Notes)
            Notes(xI2).Selected = False
        Next
        LBeat.SelectedIndices.Clear()

        Do While sCmd IsNot Nothing
            Dim xType As Byte = sCmd.ofType

            Select Case xType
                Case UndoRedo.opAddNote
                    Dim xCmd As UndoRedo.AddNote = sCmd

                    ReDim Preserve Notes(UBound(Notes) + 1)
                    Notes(UBound(Notes)) = xCmd.note

                    If TBWavIncrease.Checked Then
                        If IsColumnSound(xCmd.note.ColumnIndex) Then
                            IncreaseCurrentWav()
                        Else
                            IncreaseCurrentBmp()
                        End If
                    End If
                Case UndoRedo.opRemoveNote
                    Dim xCmd As UndoRedo.RemoveNote = sCmd
                    Dim xI2 As Integer = FindNoteIndex(xCmd.note)

                    If xI2 < Notes.Length Then
                        For xI3 As Integer = xI2 + 1 To UBound(Notes)
                            Notes(xI3 - 1) = Notes(xI3)
                        Next
                        ReDim Preserve Notes(UBound(Notes) - 1)
                    End If

                    If TBWavIncrease.Checked Then
                        If IsColumnSound(xCmd.note.ColumnIndex) Then
                            DecreaseCurrentWav()
                        Else
                            DecreaseCurrentBmp()
                        End If
                    End If

                Case UndoRedo.opChangeNote
                    Dim xCmd As UndoRedo.ChangeNote = sCmd
                    Dim xI2 As Integer = FindNoteIndex(xCmd.note)

                    If xI2 < Notes.Length Then
                        Notes(xI2) = xCmd.note
                    End If

                Case UndoRedo.opMoveNote
                    Dim xCmd As UndoRedo.MoveNote = sCmd
                    Dim xI2 As Integer = FindNoteIndex(xCmd.note)

                    If xI2 < Notes.Length Then
                        With Notes(xI2)
                            .ColumnIndex = xCmd.NColumnIndex
                            .VPosition = xCmd.NVPosition
                            .Selected = xCmd.note.Selected And nEnabled(.ColumnIndex)
                        End With
                    End If

                Case UndoRedo.opLongNoteModify
                    Dim xCmd As UndoRedo.LongNoteModify = sCmd
                    Dim xI2 As Integer = FindNoteIndex(xCmd.note)

                    If xI2 < Notes.Length Then
                        With Notes(xI2)
                            If NTInput Then
                                .VPosition = xCmd.NVPosition
                                .Length = xCmd.NLongNote
                            Else
                                .LongNote = xCmd.NLongNote
                            End If
                            .Selected = xCmd.note.Selected And nEnabled(.ColumnIndex)
                        End With
                    End If

                Case UndoRedo.opHiddenNoteModify
                    Dim xCmd As UndoRedo.HiddenNoteModify = sCmd
                    Dim xI2 As Integer = FindNoteIndex(xCmd.note)

                    If xI2 < Notes.Length Then
                        Notes(xI2).Hidden = xCmd.NHidden
                        Notes(xI2).Selected = xCmd.note.Selected And nEnabled(Notes(xI2).ColumnIndex)
                    End If

                Case UndoRedo.opRelabelNote
                    Dim xCmd As UndoRedo.RelabelNote = sCmd
                    Dim xI2 As Integer = FindNoteIndex(xCmd.note)

                    If xI2 < Notes.Length Then
                        Notes(xI2).Value = xCmd.NValue
                        Notes(xI2).Selected = xCmd.note.Selected And nEnabled(Notes(xI2).ColumnIndex)
                    End If

                Case UndoRedo.opRemoveAllNotes
                    ReDim Preserve Notes(0)

                Case UndoRedo.opChangeMeasureLength
                    Dim xCmd As UndoRedo.ChangeMeasureLength = sCmd
                    Dim xxD As Long = GetDenominator(xCmd.Value / 192)
                    'Dim xDenom As Integer = 192 / GCD(xCmd.Value, 192.0R)
                    'If xDenom < 4 Then xDenom = 4
                    For Each xM As Integer In xCmd.Indices
                        MeasureLength(xM) = xCmd.Value
                        LBeat.Items(xM) = Add3Zeros(xM) & ": " & (xCmd.Value / 192) & IIf(xxD > 10000, "", " ( " & CLng(xCmd.Value / 192 * xxD) & " / " & xxD & " ) ")
                        LBeat.SelectedIndices.Add(xM)
                    Next
                    UpdateMeasureBottom()

                Case UndoRedo.opChangeTimeSelection
                    Dim xCmd As UndoRedo.ChangeTimeSelection = sCmd
                    vSelStart = xCmd.SelStart
                    vSelLength = xCmd.SelLength
                    vSelHalf = xCmd.SelHalf
                    If xCmd.Selected Then
                        Dim xSelLo As Double = vSelStart + IIf(vSelLength < 0, vSelLength, 0)
                        Dim xSelHi As Double = vSelStart + IIf(vSelLength > 0, vSelLength, 0)
                        For xI2 As Integer = 1 To UBound(Notes)
                            Notes(xI2).Selected = Notes(xI2).VPosition >= xSelLo AndAlso
                                              Notes(xI2).VPosition < xSelHi AndAlso
                                              nEnabled(Notes(xI2).ColumnIndex)
                        Next
                    End If

                Case UndoRedo.opNT
                    Dim xCmd As UndoRedo.NT = sCmd
                    NTInput = xCmd.BecomeNT
                    TBNTInput.Checked = NTInput
                    mnNTInput.Checked = NTInput

                    POBLong.Enabled = Not NTInput
                    POBLongShort.Enabled = Not NTInput
                    bAdjustLength = False
                    bAdjustUpper = False

                    If xCmd.AutoConvert Then
                        If NTInput Then ConvertBMSE2NT() Else ConvertNT2BMSE()
                    End If
                Case UndoRedo.opWavAutoincFlag
                    Dim xcmd As UndoRedo.WavAutoincFlag = sCmd
                    SetWavIncreaseChecked(xcmd.Checked)

                Case UndoRedo.opVoid

                Case UndoRedo.opNoOperation
                    'Exit Do

            End Select

            sCmd = sCmd.Next
        Loop

        THBPM.Value = Notes(0).Value / 10000
        If IsSaved Then SetIsSaved(False)

        SortByVPositionInsertion()
        UpdatePairing()
        CalculateTotalPlayableNotes()
        CalculateGreatestVPosition()
        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Private Sub AddUndo(ByVal sCUndo As UndoRedo.LinkedURCmd, ByVal sCRedo As UndoRedo.LinkedURCmd, Optional ByVal OverWrite As Boolean = False)
        If sCUndo Is Nothing And sCRedo Is Nothing Then Exit Sub
        If IsSaved Then SetIsSaved(False)
        Dim xHadRedo As Boolean = CanRedo()
        If Not OverWrite Then sI = sIA()

        StoreUndoRedoSlot(sI, sCUndo, sCRedo)
        If xHadRedo Then
            ClearRedoHistory(sIA())
        Else
            SetUndoRedoEnd(sIA())
        End If
        EnforceUndoRedoHistoryLimit()
        TBUndo.Enabled = True
        TBRedo.Enabled = False
        mnUndo.Enabled = True
        mnRedo.Enabled = False
    End Sub

    Private Sub ClearUndo()
        ReDim sUndo(UndoRedoHistoryCount - 1)
        ReDim sRedo(UndoRedoHistoryCount - 1)
        sI = 0
        UndoRedoMemoryUsedBytes = 0
        SetUndoRedoEnd(0)
        SetUndoRedoEnd(sIA())
        RefreshUndoRedoEnabled()
    End Sub

    Private Sub ClearRedoHistory(ByVal startIndex As Integer)
        Dim xI As Integer = startIndex

        Do While xI <> sI
            Dim xIsEnd As Boolean = IsUndoRedoEnd(sUndo(xI)) AndAlso IsUndoRedoEnd(sRedo(xI))
            ClearUndoRedoSlot(xI)

            If xIsEnd Then Exit Do
            xI = NextUndoIndex(xI, sUndo.Length)
        Loop

        SetUndoRedoEnd(startIndex)
    End Sub

    Private Function FindOldestUndoIndex() As Integer
        Dim xI As Integer = sI
        Dim xOldest As Integer = -1

        For xCount As Integer = 1 To sUndo.Length
            If IsUndoRedoEnd(sUndo(xI)) Then Exit For

            xOldest = xI
            xI = PreviousUndoIndex(xI, sUndo.Length)
        Next

        Return xOldest
    End Function

    Private Function FindFarthestRedoIndex() As Integer
        Dim xI As Integer = sIA()
        Dim xFarthest As Integer = -1

        For xCount As Integer = 1 To sRedo.Length
            If IsUndoRedoEnd(sRedo(xI)) Then Exit For

            xFarthest = xI
            xI = NextUndoIndex(xI, sRedo.Length)
        Next

        Return xFarthest
    End Function

    Private Sub EnforceUndoRedoHistoryLimit()
        NormalizeUndoRedoMemoryLimit()

        Do While UndoRedoMemoryUsedBytes > UndoRedoMemoryLimitBytes()
            Dim xOldestUndo As Integer = FindOldestUndoIndex()
            If xOldestUndo <> -1 AndAlso xOldestUndo <> sI Then
                SetUndoRedoEnd(xOldestUndo)
                Continue Do
            End If

            Dim xFarthestRedo As Integer = FindFarthestRedoIndex()
            If xFarthestRedo = -1 Then Exit Do

            SetUndoRedoEnd(xFarthestRedo)
        Loop

        RefreshUndoRedoEnabled()
    End Sub

    Private Structure UndoRedoHistorySlot
        Public UndoCmd As UndoRedo.LinkedURCmd
        Public RedoCmd As UndoRedo.LinkedURCmd

        Public Sub New(ByVal undoCmd As UndoRedo.LinkedURCmd, ByVal redoCmd As UndoRedo.LinkedURCmd)
            Me.UndoCmd = undoCmd
            Me.RedoCmd = redoCmd
        End Sub
    End Structure

    Private Function GetUndoHistory() As List(Of UndoRedoHistorySlot)
        Dim xHistory As New List(Of UndoRedoHistorySlot)
        Dim xI As Integer = sI

        For xCount As Integer = 1 To sUndo.Length
            If IsUndoRedoEnd(sUndo(xI)) Then Exit For

            xHistory.Add(New UndoRedoHistorySlot(sUndo(xI), sRedo(xI)))
            xI = PreviousUndoIndex(xI, sUndo.Length)
        Next

        xHistory.Reverse()
        Return xHistory
    End Function

    Private Function GetRedoHistory() As List(Of UndoRedoHistorySlot)
        Dim xHistory As New List(Of UndoRedoHistorySlot)
        Dim xI As Integer = sIA()

        For xCount As Integer = 1 To sRedo.Length
            If IsUndoRedoEnd(sRedo(xI)) Then Exit For

            xHistory.Add(New UndoRedoHistorySlot(sUndo(xI), sRedo(xI)))
            xI = NextUndoIndex(xI, sRedo.Length)
        Next

        Return xHistory
    End Function

    Private Sub ImportUndoRedoHistory(ByVal undoList() As UndoRedo.LinkedURCmd,
                                      ByVal redoList() As UndoRedo.LinkedURCmd,
                                      ByVal pointer As Integer)
        ClearUndo()

        If undoList Is Nothing OrElse redoList Is Nothing Then Exit Sub
        If undoList.Length = 0 OrElse undoList.Length <> redoList.Length Then Exit Sub
        If pointer < 0 OrElse pointer >= undoList.Length Then Exit Sub

        Dim xUndoHistory As New List(Of UndoRedoHistorySlot)
        Dim xI As Integer = pointer

        For xCount As Integer = 1 To undoList.Length
            If IsUndoRedoEnd(undoList(xI)) Then Exit For

            xUndoHistory.Add(New UndoRedoHistorySlot(undoList(xI), redoList(xI)))
            xI = PreviousUndoIndex(xI, undoList.Length)
        Next

        xUndoHistory.Reverse()
        If xUndoHistory.Count > MaxUndoRedoSteps Then
            xUndoHistory.RemoveRange(0, xUndoHistory.Count - MaxUndoRedoSteps)
        End If

        For Each xSlot As UndoRedoHistorySlot In xUndoHistory
            sI = sIA()
            StoreUndoRedoSlot(sI, xSlot.UndoCmd, xSlot.RedoCmd)
        Next

        Dim xRedoHistory As New List(Of UndoRedoHistorySlot)
        xI = NextUndoIndex(pointer, redoList.Length)

        For xCount As Integer = 1 To redoList.Length
            If IsUndoRedoEnd(redoList(xI)) Then Exit For

            xRedoHistory.Add(New UndoRedoHistorySlot(undoList(xI), redoList(xI)))
            xI = NextUndoIndex(xI, redoList.Length)
        Next

        Dim xRedoLimit As Integer = MaxUndoRedoSteps - xUndoHistory.Count
        If xRedoHistory.Count > xRedoLimit Then
            xRedoHistory.RemoveRange(xRedoLimit, xRedoHistory.Count - xRedoLimit)
        End If

        xI = sIA()
        For Each xSlot As UndoRedoHistorySlot In xRedoHistory
            StoreUndoRedoSlot(xI, xSlot.UndoCmd, xSlot.RedoCmd)
            xI = NextUndoIndex(xI, sUndo.Length)
        Next

        SetUndoRedoEnd(xI)
        EnforceUndoRedoHistoryLimit()
        RefreshUndoRedoEnabled()
    End Sub

    Private Function ReadUndoRedoCommandList(ByVal br As BinaryReader) As UndoRedo.LinkedURCmd
        Dim xCount As Integer = br.ReadInt32
        Dim xBase As New UndoRedo.Void
        Dim xIterator As UndoRedo.LinkedURCmd = xBase

        For xI As Integer = 1 To xCount
            Dim xByteLen As Integer = br.ReadInt32
            Dim xByte() As Byte = br.ReadBytes(xByteLen)
            Dim xCmd As UndoRedo.LinkedURCmd = UndoRedo.fromBytes(xByte)

            If xCmd Is Nothing Then Continue For
            xIterator.Next = xCmd
            xIterator = xIterator.Next
        Next

        Return xBase.Next
    End Function

    Private Sub WriteUndoRedoCommandList(ByVal bw As BinaryWriter, ByVal cmd As UndoRedo.LinkedURCmd)
        Dim xCount As Integer = 0
        Dim xIterator As UndoRedo.LinkedURCmd = cmd

        While xIterator IsNot Nothing
            xCount += 1
            xIterator = xIterator.Next
        End While

        bw.Write(xCount)

        xIterator = cmd
        For xI As Integer = 1 To xCount
            Dim xBytes() As Byte = xIterator.toBytes
            bw.Write(xBytes.Length)
            bw.Write(xBytes)
            xIterator = xIterator.Next
        Next
    End Sub

    Private Sub RedoAddNote(ByVal note As Note,
                            ByRef BaseUndo As UndoRedo.LinkedURCmd,
                            ByRef BaseRedo As UndoRedo.LinkedURCmd,
                            Optional autoinc As Boolean = False)
        Dim xUndo As New UndoRedo.RemoveNote(note)
        Dim xRedo As New UndoRedo.AddNote(note)
        xUndo.Next = BaseUndo
        BaseUndo = xUndo
        If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
        BaseRedo = xRedo
    End Sub

    Private Sub RedoAddNote(ByVal xIndices() As Integer, ByVal xSel As Boolean, ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
        For xI1 As Integer = 0 To UBound(xIndices)
            Dim xUndo As New UndoRedo.RemoveNote(Notes(xI1))
            Dim xRedo As New UndoRedo.AddNote(Notes(xI1))
            xUndo.Next = BaseUndo
            BaseUndo = xUndo
            If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
            BaseRedo = xRedo
        Next
    End Sub

    Private Sub RedoAddNoteSelected(ByVal xSel As Boolean, ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
        For xI1 As Integer = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For

            Dim xUndo As New UndoRedo.RemoveNote(Notes(xI1))
            Dim xRedo As New UndoRedo.AddNote(Notes(xI1))
            xUndo.Next = BaseUndo
                BaseUndo = xUndo
                If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
                BaseRedo = xRedo

        Next
    End Sub

    Private Sub RedoAddNoteAll(ByVal xSel As Boolean, ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
        For xI1 As Integer = 1 To UBound(Notes)

            Dim xRedo As New UndoRedo.AddNote(Notes(xI1))
            If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
                BaseRedo = xRedo

        Next
        Dim xUndo As New UndoRedo.RemoveAllNotes
        xUndo.Next = BaseUndo
        BaseUndo = xUndo
    End Sub


    Private Sub RedoRemoveNote(ByVal xN As Note, ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
        Dim xUndo As New UndoRedo.AddNote(xN)
        Dim xRedo As New UndoRedo.RemoveNote(xN)
        xUndo.Next = BaseUndo
        BaseUndo = xUndo
        If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
        BaseRedo = xRedo
    End Sub

    Private Sub RedoRemoveNote(ByVal xIndices() As Integer, ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
        For xI1 As Integer = 0 To UBound(xIndices)
            Dim xUndo As New UndoRedo.AddNote(Notes(xIndices(xI1)))
            Dim xRedo As New UndoRedo.RemoveNote(Notes(xIndices(xI1)))
            xUndo.Next = BaseUndo
            BaseUndo = xUndo
            If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
            BaseRedo = xRedo
        Next
    End Sub

    Private Sub RedoRemoveNoteSelected(ByVal xSel As Boolean, ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
        For xI1 As Integer = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For
            Dim xUndo As New UndoRedo.AddNote(Notes(xI1))
            Dim xRedo As New UndoRedo.RemoveNote(Notes(xI1))
            xUndo.Next = BaseUndo
            BaseUndo = xUndo
            If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
            BaseRedo = xRedo
        Next
    End Sub

    Private Sub RedoRemoveNoteAll(ByVal xSel As Boolean, ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
        For xI1 As Integer = 1 To UBound(Notes)
            With Notes(xI1)
                Dim xUndo As New UndoRedo.AddNote(Notes(xI1))
                xUndo.Next = BaseUndo
                BaseUndo = xUndo
            End With
        Next
        Dim xRedo As New UndoRedo.RemoveAllNotes
        If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
        BaseRedo = xRedo
    End Sub

    Private Sub RedoChangeNote(note1 As Note, note2 As Note, ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
        Dim xUndo As New UndoRedo.ChangeNote(note2, note1)
        Dim xRedo As New UndoRedo.ChangeNote(note1, note2)
        xUndo.Next = BaseUndo
        BaseUndo = xUndo
        If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
        BaseRedo = xRedo
    End Sub


    Private Sub RedoMoveNote(note As Note, nCol As Integer, nVPos As Double, ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
        Dim noteAfterModification = note
        noteAfterModification.ColumnIndex = nCol
        noteAfterModification.VPosition = nVPos
        Dim xUndo As New UndoRedo.MoveNote(noteAfterModification, note.ColumnIndex, note.VPosition)
        Dim xRedo As New UndoRedo.MoveNote(note, nCol, nVPos)
        xUndo.Next = BaseUndo
        BaseUndo = xUndo
        If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
        BaseRedo = xRedo
    End Sub


    Private Sub RedoLongNoteModify(note As Note, nVPos As Double, nLong As Double, ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
        Dim n = note
        n.VPosition = nVPos
        n.Length = nLong

        Dim xUndo As New UndoRedo.LongNoteModify(n, note.VPosition, note.Length)
        Dim xRedo As New UndoRedo.LongNoteModify(note, nVPos, n.Length)
        xUndo.Next = BaseUndo
        BaseUndo = xUndo
        If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
        BaseRedo = xRedo
    End Sub

    Private Sub RedoHiddenNoteModify(xN As Note, nHide As Boolean, xSel As Boolean, ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
        Dim noteAfterModification = xN
        noteAfterModification.Hidden = nHide
        Dim xUndo As New UndoRedo.HiddenNoteModify(noteAfterModification, xN.Hidden)
        Dim xRedo As New UndoRedo.HiddenNoteModify(xN, nHide)
        xUndo.Next = BaseUndo
        BaseUndo = xUndo
        If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
        BaseRedo = xRedo
    End Sub


    Private Sub RedoRelabelNote(ByVal xN As Note, ByVal nVal As Long, ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
        Dim noteAfterModification = xN
        noteAfterModification.Value = nVal
        Dim xUndo As New UndoRedo.RelabelNote(noteAfterModification, xN.Value)
        Dim xRedo As New UndoRedo.RelabelNote(xN, nVal)
        xUndo.Next = BaseUndo
        BaseUndo = xUndo
        If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
        BaseRedo = xRedo
    End Sub

    Private Sub RedoChangeMeasureLengthSelected(ByVal nVal As Double, ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
        Dim xIndices(LBeat.SelectedIndices.Count - 1) As Integer
        LBeat.SelectedIndices.CopyTo(xIndices, 0)
        If xIndices.Length = 0 Then Exit Sub

        Dim xmLen(-1) As Double
        Dim xUndo(-1) As UndoRedo.ChangeMeasureLength
        For Each xI1 As Integer In xIndices
            Dim xI As Integer = Array.IndexOf(xmLen, MeasureLength(xI1))
            If xI = -1 Then
                ReDim Preserve xmLen(UBound(xmLen) + 1)
                ReDim Preserve xUndo(UBound(xUndo) + 1)
                xmLen(UBound(xmLen)) = MeasureLength(xI1)
                xUndo(UBound(xUndo)) = New UndoRedo.ChangeMeasureLength(MeasureLength(xI1), New Integer() {xI1})
            Else
                With xUndo(xI)
                    ReDim Preserve .Indices(UBound(.Indices) + 1)
                    .Indices(UBound(.Indices)) = xI1
                End With
            End If
        Next
        For xI1 As Integer = 0 To UBound(xUndo) - 1
            xUndo(xI1).Next = xUndo(xI1 + 1)
        Next
        xUndo(UBound(xUndo)).Next = BaseUndo
        BaseUndo = xUndo(0)

        Dim xRedo As New UndoRedo.ChangeMeasureLength(nVal, xIndices.Clone)
        If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
        BaseRedo = xRedo
    End Sub

    Private Sub RedoChangeTimeSelection(ByVal pStart As Double, ByVal pLen As Double, ByVal pHalf As Double,
    ByVal nStart As Double, ByVal nLen As Double, ByVal nHalf As Double, ByVal xSel As Boolean, ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
        Dim xUndo As New UndoRedo.ChangeTimeSelection(pStart, pLen, pHalf, xSel)
        Dim xRedo As New UndoRedo.ChangeTimeSelection(nStart, nLen, nHalf, xSel)
        xUndo.Next = BaseUndo
        BaseUndo = xUndo
        If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
        BaseRedo = xRedo
    End Sub

    'Private Sub RedoChangeVisibleColumns(ByVal pBLP As Boolean, ByVal pSTOP As Boolean, ByVal pPlayer As Integer, _
    '                                     ByVal nBLP As Boolean, ByVal nSTOP As Boolean, ByVal nPlayer As Integer, _
    'ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
    '    Dim xUndo As New UndoRedo.ChangeVisibleColumns(pBLP, pSTOP, pPlayer)
    '    Dim xRedo As New UndoRedo.ChangeVisibleColumns(nBLP, nSTOP, nPlayer)
    '    xUndo.Next = BaseUndo
    '    BaseUndo = xUndo
    '    If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
    '    BaseRedo = xRedo
    'End Sub

    Private Sub RedoNT(becomeNT As Boolean, autoConvert As Boolean, ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
        Dim xUndo As New UndoRedo.NT(Not becomeNT, autoConvert)
        Dim xRedo As New UndoRedo.NT(becomeNT, autoConvert)
        xUndo.Next = BaseUndo
        BaseUndo = xUndo
        If BaseRedo IsNot Nothing Then BaseRedo.Next = xRedo
        BaseRedo = xRedo
    End Sub

    Private Sub RedoWavIncrease(wavinc As Boolean, ByRef BaseUndo As UndoRedo.LinkedURCmd, ByRef BaseRedo As UndoRedo.LinkedURCmd)
        Dim xUndo As New UndoRedo.WavAutoincFlag(Not wavinc)
        Dim xRedo As New UndoRedo.WavAutoincFlag(wavinc)
        xUndo.Next = BaseUndo
        BaseUndo = xUndo
        If BaseUndo IsNot Nothing Then BaseRedo.Next = xRedo
        BaseRedo = xRedo
    End Sub
End Class
