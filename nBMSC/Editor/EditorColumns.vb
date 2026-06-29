Imports nBMSC.Editor

Partial Public Class MainWindow

    Public Const niMeasure As Integer = 0
    Public Const niSCROLL As Integer = 1
    Public Const niBPM As Integer = 2
    Public Const niSTOP As Integer = 3
    Public Const niS1 As Integer = 4

    Public Const niA1 As Integer = 5
    Public Const niA2 As Integer = 6
    Public Const niA3 As Integer = 7
    Public Const niA4 As Integer = 8
    Public Const niA5 As Integer = 9
    Public Const niA6 As Integer = 10
    Public Const niA7 As Integer = 11
    Public Const niA8 As Integer = 12
    Public Const niA9 As Integer = 13
    Public Const niAA As Integer = 14
    Public Const niAB As Integer = 15
    Public Const niAC As Integer = 16
    Public Const niAD As Integer = 17
    Public Const niAE As Integer = 18
    Public Const niAF As Integer = 19
    Public Const niAG As Integer = 20
    Public Const niAH As Integer = 21
    Public Const niAI As Integer = 22
    Public Const niAJ As Integer = 23
    Public Const niAK As Integer = 24
    Public Const niAL As Integer = 25
    Public Const niAM As Integer = 26
    Public Const niAN As Integer = 27
    Public Const niAO As Integer = 28
    Public Const niAP As Integer = 29
    Public Const niAQ As Integer = 30
    Public Const niS2 As Integer = 31

    Public Const niD1 As Integer = 32
    Public Const niD2 As Integer = 33
    Public Const niD3 As Integer = 34
    Public Const niD4 As Integer = 35
    Public Const niD5 As Integer = 36
    Public Const niD6 As Integer = 37
    Public Const niD7 As Integer = 38
    Public Const niD8 As Integer = 39
    Public Const niD9 As Integer = 40
    Public Const niDA As Integer = 41
    Public Const niDB As Integer = 42
    Public Const niDC As Integer = 43
    Public Const niDD As Integer = 44
    Public Const niDE As Integer = 45
    Public Const niDF As Integer = 46
    Public Const niDG As Integer = 47
    Public Const niDH As Integer = 48
    Public Const niDI As Integer = 49
    Public Const niDJ As Integer = 50
    Public Const niDK As Integer = 51
    Public Const niDL As Integer = 52
    Public Const niDM As Integer = 53
    Public Const niDN As Integer = 54
    Public Const niDO As Integer = 55
    Public Const niDP As Integer = 56
    Public Const niDQ As Integer = 57
    Public Const niS3 As Integer = 58

    Public Const niBGA As Integer = 59
    Public Const niLAYER As Integer = 60
    Public Const niPOOR As Integer = 61
    Public Const niS4 As Integer = 62
    Public Const niB As Integer = 63

    Public column() As Column = {New Column(0, 50, "Measure", False, True, False, True, 0, 0, &HFF00FFFF, 0, &HFF00FFFF, 0),
                              New Column(50, 60, "SCROLL", True, True, False, True, 99, 0, &HFFFF0000, 0, &HFFFF0000, 0),
                              New Column(110, 60, "BPM", True, True, False, True, 3, 0, &HFFFF0000, 0, &HFFFF0000, 0),
                              New Column(170, 50, "STOP", True, True, False, True, 9, 0, &HFFFF0000, 0, &HFFFF0000, 0),
                              New Column(220, 5, "", False, False, False, True, 0, 0, 0, 0, 0, 0),
                              New Column(225, 40, "AQ", True, False, True, False, 42, &HFFB0B0B0, &HFF000000, &HFF909090, &HFF000000, 0),
                              New Column(225, 40, "A1", True, False, True, True, 43, &HFF808080, &HFF000000, &HFF909090, &HFF000000, 0),
                              New Column(265, 42, "A2", True, False, True, True, 37, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(307, 30, "A3", True, False, True, True, 38, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(337, 42, "A4", True, False, True, True, 39, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(379, 45, "A5", True, False, True, True, 40, &HFFFFC862, &HFF000000, &HFFF7C66A, &HFF000000, &H16F38B0C),
                              New Column(424, 42, "A6", True, False, True, True, 41, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(466, 30, "A7", True, False, True, True, 44, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(496, 42, "A8", True, False, True, True, 45, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(496, 30, "A9", True, False, True, False, 46, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(496, 42, "AA", True, False, True, False, 47, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(496, 30, "AB", True, False, True, False, 48, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(496, 42, "AC", True, False, True, False, 49, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(496, 30, "AD", True, False, True, False, 50, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(496, 42, "AE", True, False, True, False, 51, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(496, 30, "AF", True, False, True, False, 52, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(496, 42, "AG", True, False, True, False, 53, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(496, 30, "AH", True, False, True, False, 54, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(496, 42, "AI", True, False, True, False, 55, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(496, 30, "AJ", True, False, True, False, 56, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(496, 42, "AK", True, False, True, False, 57, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(496, 30, "AL", True, False, True, False, 58, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(496, 42, "AM", True, False, True, False, 59, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(496, 30, "AN", True, False, True, False, 60, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(496, 42, "AO", True, False, True, False, 61, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(496, 30, "AP", True, False, True, False, 62, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(496, 5, "", False, False, False, True, 0, 0, 0, 0, 0, 0),
                              New Column(503, 42, "D1", True, False, True, False, 73, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(503, 30, "D2", True, False, True, False, 74, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(503, 42, "D3", True, False, True, False, 75, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(503, 45, "D4", True, False, True, False, 76, &HFFFFC862, &HFF000000, &HFFF7C66A, &HFF000000, &H16F38B0C),
                              New Column(503, 42, "D5", True, False, True, False, 77, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(503, 30, "D6", True, False, True, False, 80, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(503, 42, "D7", True, False, True, False, 81, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(503, 30, "D9", True, False, True, False, 82, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(503, 42, "DA", True, False, True, False, 83, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(503, 30, "DB", True, False, True, False, 84, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(503, 42, "DC", True, False, True, False, 85, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(503, 30, "DD", True, False, True, False, 86, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(503, 42, "DE", True, False, True, False, 87, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(503, 30, "DF", True, False, True, False, 88, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(503, 42, "DG", True, False, True, False, 89, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(503, 30, "DH", True, False, True, False, 90, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(503, 42, "DI", True, False, True, False, 91, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(503, 30, "DJ", True, False, True, False, 92, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(503, 42, "DK", True, False, True, False, 93, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(503, 30, "DL", True, False, True, False, 94, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(503, 42, "DM", True, False, True, False, 95, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(503, 30, "DN", True, False, True, False, 96, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(503, 42, "DO", True, False, True, False, 97, &HFFB0B0B0, &HFF000000, &HFFC0C0C0, &HFF000000, &H14FFFFFF),
                              New Column(503, 30, "DP", True, False, True, False, 98, &HFF62B0FF, &HFF000000, &HFF6AB0F7, &HFF000000, &H140033FF),
                              New Column(503, 40, "DQ", True, False, True, False, 78, &HFF808080, &HFF000000, &HFF909090, &HFF000000, 0),
                              New Column(503, 40, "D8", True, False, True, False, 79, &HFF808080, &HFF000000, &HFF909090, &HFF000000, 0),
                              New Column(503, 5, "", False, False, False, False, 0, 0, 0, 0, 0, 0),
                              New Column(503, 40, "BGA", True, False, False, False, 4, &HFF8CD78A, &HFF000000, &HFF90D38E, &HFF000000, 0),
                              New Column(503, 40, "LAYER", True, False, False, False, 7, &HFF8CD78A, &HFF000000, &HFF90D38E, &HFF000000, 0),
                              New Column(503, 40, "POOR", True, False, False, False, 6, &HFF8CD78A, &HFF000000, &HFF90D38E, &HFF000000, 0),
                              New Column(503, 5, "", False, False, False, False, 0, 0, 0, 0, 0, 0),
                              New Column(503, 40, "B", True, False, True, True, 1, &HFFE18080, &HFF000000, &HFFDC8585, &HFF000000, 0)}


    Public Const idflBPM As Integer = 5

    Private Function GetBMSChannelBy(note As Note) As String
        Dim iCol = note.ColumnIndex
        Dim xVal = note.Value
        Dim xLong = note.LongNote
        Dim xHidden = note.Hidden
        Dim bmsBaseChannel As Integer = GetColumn(iCol).Identifier
        Dim xLandmine = note.Landmine

        If iCol = niBPM AndAlso (xVal / 10000 <> xVal \ 10000 Or xVal >= 2560000 Or xVal < 0) Then bmsBaseChannel += idflBPM

        If iCol = niSCROLL Then Return "SC"

        ' p1 side
        If iCol >= niA1 And iCol <= niAQ Then
            If xLong Then
                Return C10to36Channel(bmsBaseChannel + (&H5 * 36) - 36)
            ElseIf xHidden Then
                Return C10to36Channel(bmsBaseChannel + (&H3 * 36) - 36)
            ElseIf xLandmine Then
                Return C10to36Channel(bmsBaseChannel + (&HD * 36) - 36)
            Else
                Return C10to36Channel(bmsBaseChannel + (&H1 * 36) - 36)
            End If
        End If

        ' p2 side
        If iCol >= niD1 And iCol <= niDQ Then
            If xLong Then
                Return C10to36Channel(bmsBaseChannel + (&H6 * 36) - 72)
            ElseIf xHidden Then
                Return C10to36Channel(bmsBaseChannel + (&H4 * 36) - 72)
            ElseIf xLandmine Then
                Return C10to36Channel(bmsBaseChannel + (&HE * 36) - 72)
            Else
                Return C10to36Channel(bmsBaseChannel + (&H2 * 36) - 72)
            End If
        End If

        Return Add2Zeros(bmsBaseChannel)
    End Function

    Private Function nLeft(ByVal iCol As Integer) As Integer
        If iCol < niB Then Return column(iCol).Left Else Return column(niB).Left + (iCol - niB) * column(niB).Width
    End Function
    Private Function GetColumnWidth(ByVal iCol As Integer) As Integer
        If Not GetColumn(iCol).isVisible Then Return 0
        If iCol < niB Then Return column(iCol).Width Else Return column(niB).Width
    End Function
    Private Function nTitle(ByVal iCol As Integer) As String
        If iCol < niB Then Return column(iCol).Title Else Return column(niB).Title & (iCol - niB + 1).ToString
    End Function
    Private Function nEnabled(ByVal iCol As Integer) As Boolean
        'If iCol < niB Then Return col(iCol).Enabled And col(iCol).Visible Else Return col(niB).Enabled And col(niB).Visible
        If iCol < niB Then Return column(iCol).isEnabledAfterAll Else Return column(niB).isEnabledAfterAll
    End Function
    Private Function IsColumnNumeric(ByVal iCol As Integer) As Boolean
        If iCol < niB Then Return column(iCol).isNumeric Else Return column(niB).isNumeric
    End Function
    Private Function IsColumnSound(ByVal iCol As Integer) As Boolean
        If iCol < niB Then Return column(iCol).isSound Else Return column(niB).isSound
    End Function
    Private Function IsColumnImage(ByVal iCol As Integer) As Boolean
        If iCol < niB Then Return Not (column(iCol).isNumeric Or column(iCol).isSound) Else Return Not (column(niB).isNumeric Or column(niB).isSound)
    End Function

    Private Sub ChangePlaySide(ByVal swap As Boolean)
        If Rscratch Then
            column(niA1).Identifier = 37
            column(niA2).Identifier = 38
            column(niA3).Identifier = 39
            column(niA4).Identifier = 40
            column(niA5).Identifier = 41
            column(niA6).Identifier = 44
            column(niA7).Identifier = 45
            column(niA8).Identifier = 42
            column(niA9).Identifier = 43

            For i = 0 To UBound(Notes) Step 1
                If Notes(i).ColumnIndex = niA1 Then
                    Notes(i).ColumnIndex = niA8
                ElseIf Notes(i).ColumnIndex = niA2 Then
                    Notes(i).ColumnIndex = niA9
                ElseIf Notes(i).ColumnIndex >= niA3 AndAlso Notes(i).ColumnIndex <= niA9 Then
                    Notes(i).ColumnIndex -= 2
                End If
            Next
        ElseIf swap Then
            column(niA1).Identifier = 42
            column(niA2).Identifier = 43
            column(niA3).Identifier = 37
            column(niA4).Identifier = 38
            column(niA5).Identifier = 39
            column(niA6).Identifier = 40
            column(niA7).Identifier = 41
            column(niA8).Identifier = 44
            column(niA9).Identifier = 45
            For i = 0 To UBound(Notes) Step 1
                If Notes(i).ColumnIndex = niA8 Then
                    Notes(i).ColumnIndex = niA1
                ElseIf Notes(i).ColumnIndex = niA9 Then
                    Notes(i).ColumnIndex = niA2
                ElseIf Notes(i).ColumnIndex >= niA1 AndAlso Notes(i).ColumnIndex <= niA7 Then
                    Notes(i).ColumnIndex += 2
                End If
            Next
        End If
        ChangePlaySideSkin(swap)
    End Sub

    Private Sub ChangePlaySideSkin(ByVal swap As Boolean)
        Dim tLeft(10)
        For i = 0 To 8 Step 1
            tLeft(i) = column(niA1 + i + 1).Left - column(niA1 + i).Left
        Next
        If Rscratch Then
            Dim tTitle = {column(niA1).Title, column(niA2).Title}
            Dim tcBG = {column(niA1).cBG, column(niA2).cBG}
            Dim tcNote = {column(niA1).cNote, column(niA2).cNote}
            Dim tcLNote = {column(niA1).cLNote, column(niA2).cLNote}
            Dim tcText = {column(niA1).cText, column(niA2).cText}
            Dim tcLText = {column(niA1).cLText, column(niA2).cLText}
            Dim tWidth = {column(niA1).Width, column(niA2).Width}
            Dim tVisible = {column(niA1).isVisible, column(niA2).isVisible}
            tLeft(9) = tLeft(0)
            tLeft(10) = tLeft(1)
            For i = 0 To 6 Step 1
                column(niA1 + i).Title = column(niA1 + i + 2).Title
                column(niA1 + i).cBG = column(niA1 + i + 2).cBG
                column(niA1 + i).cText = column(niA1 + i + 2).cText
                column(niA1 + i).cLText = column(niA1 + i + 2).cLText
                column(niA1 + i).setNoteColor(column(niA1 + i + 2).cNote)
                column(niA1 + i).setLNoteColor(column(niA1 + i + 2).cLNote)
                column(niA1 + i).isVisible = column(niA1 + i + 2).isVisible
                column(niA1 + i).Width = column(niA1 + i + 2).Width
                tLeft(i) = tLeft(i + 2)
            Next
            column(niA8).Title = tTitle(0)
            column(niA8).cBG = tcBG(0)
            column(niA8).cText = tcText(0)
            column(niA8).cLText = tcLText(0)
            column(niA8).setNoteColor(tcNote(0))
            column(niA8).setLNoteColor(tcLNote(0))
            column(niA8).isVisible = tVisible(0)
            column(niA8).Width = tWidth(0)
            column(niA9).Title = tTitle(1)
            column(niA9).cBG = tcBG(1)
            column(niA9).cText = tcText(1)
            column(niA9).cLText = tcLText(1)
            column(niA9).setNoteColor(tcNote(1))
            column(niA9).setLNoteColor(tcLNote(1))
            column(niA9).isVisible = tVisible(1)
            column(niA9).Width = tWidth(1)
            tLeft(7) = tLeft(9)
            tLeft(8) = tLeft(10)
        ElseIf swap Then
            Dim tTitle = {column(niA8).Title, column(niA9).Title}
            Dim tcBG = {column(niA8).cBG, column(niA9).cBG}
            Dim tcNote = {column(niA8).cNote, column(niA9).cNote}
            Dim tcLNote = {column(niA8).cLNote, column(niA9).cLNote}
            Dim tcText = {column(niA8).cText, column(niA9).cText}
            Dim tcLText = {column(niA8).cLText, column(niA9).cLText}
            Dim tWidth = {column(niA8).Width, column(niA9).Width}
            Dim tVisible = {column(niA8).isVisible, column(niA9).isVisible}
            tLeft(9) = tLeft(7)
            tLeft(10) = tLeft(8)
            For i = 6 To 0 Step -1
                column(niA1 + i + 2).Title = column(niA1 + i).Title
                column(niA1 + i + 2).cBG = column(niA1 + i).cBG
                column(niA1 + i + 2).cText = column(niA1 + i).cText
                column(niA1 + i + 2).cLText = column(niA1 + i).cLText
                column(niA1 + i + 2).setNoteColor(column(niA1 + i).cNote)
                column(niA1 + i + 2).setLNoteColor(column(niA1 + i).cLNote)
                column(niA1 + i + 2).isVisible = column(niA1 + i).isVisible
                column(niA1 + i + 2).Width = column(niA1 + i).Width
                tLeft(i + 2) = tLeft(i)
            Next
            column(niA1).Title = tTitle(0)
            column(niA1).cBG = tcBG(0)
            column(niA1).cText = tcText(0)
            column(niA1).cLText = tcLText(0)
            column(niA1).setNoteColor(tcNote(0))
            column(niA1).setLNoteColor(tcLNote(0))
            column(niA1).isVisible = tVisible(0)
            column(niA1).Width = tWidth(0)
            column(niA2).Title = tTitle(1)
            column(niA2).cBG = tcBG(1)
            column(niA2).cText = tcText(1)
            column(niA2).cLText = tcLText(1)
            column(niA2).setNoteColor(tcNote(1))
            column(niA2).setLNoteColor(tcLNote(1))
            column(niA2).isVisible = tVisible(1)
            column(niA2).Width = tWidth(1)
            tLeft(0) = tLeft(9)
            tLeft(1) = tLeft(10)
        End If
        For i = 0 To 8 Step 1
            column(niA1 + i + 1).Left = column(niA1 + i).Left + tLeft(i)
        Next
    End Sub

    Private Function GetColumn(ByVal iCol As Integer) As Column
        If iCol < niB Then Return column(iCol) Else Return column(niB)
    End Function

    Private Function BMSEChannelToColumnIndex(ByVal I As String)
        Dim Ivalue = Val(I)
        If Ivalue > 100 Then
            Return niB + Ivalue - 101
        ElseIf Ivalue < 100 And Ivalue > 0 Then
            Return BMSChannelToColumn(Mid(I, 2, 2))
        End If
        Return niB ' ??? how did a negative number get here?
    End Function

    Private Function BMSChannelToColumn(ByVal I As String) As Integer
        Dim result As Integer = 0
        Select Case I
            Case "01" : Return niB
            Case "03", "08" : Return niBPM
            Case "09" : Return niSTOP
            Case "SC" : Return niSCROLL
            Case "04" : Return niBGA
            Case "07" : Return niLAYER
            Case "06" : Return niPOOR

            Case "11", "31", "51", "71", "D1" : result = niA3
            Case "12", "32", "52", "72", "D2" : result = niA4
            Case "13", "33", "53", "73", "D3" : result = niA5
            Case "14", "34", "54", "74", "D4" : result = niA6
            Case "15", "35", "55", "75", "D5" : result = niA7
            Case "16", "36", "56", "76", "D6" : result = niA1
            Case "17", "37", "57", "77", "D7" : result = niA2
            Case "18", "38", "58", "78", "D8" : result = niA8
            Case "19", "39", "59", "79", "D9" : result = niA9
            Case "1A", "3A", "5A", "7A", "DA" : result = niAA
            Case "1B", "3B", "5B", "7B", "DB" : result = niAB
            Case "1C", "3C", "5C", "7C", "DC" : result = niAC
            Case "1D", "3D", "5D", "7D", "DD" : result = niAD
            Case "1E", "3E", "5E", "7E", "DE" : result = niAE
            Case "1F", "3F", "5F", "7F", "DF" : result = niAF
            Case "1G", "3G", "5G", "7G", "DG" : result = niAG
            Case "1H", "3H", "5H", "7H", "DH" : result = niAH
            Case "1I", "3I", "5I", "7I", "DI" : result = niAI
            Case "1J", "3J", "5J", "7J", "DJ" : result = niAJ
            Case "1K", "3K", "5K", "7K", "DK" : result = niAK
            Case "1L", "3L", "5L", "7L", "DL" : result = niAL
            Case "1M", "3M", "5M", "7M", "DM" : result = niAM
            Case "1N", "3N", "5N", "7N", "DN" : result = niAN
            Case "1O", "3O", "5O", "7O", "DO" : result = niAO
            Case "1P", "3P", "5P", "7P", "DP" : result = niAP
            Case "1Q", "3Q", "5Q", "7Q", "DQ" : result = niAQ

            Case "21", "41", "61", "81", "E1" : result = niD1
            Case "22", "42", "62", "82", "E2" : result = niD2
            Case "23", "43", "63", "83", "E3" : result = niD3
            Case "24", "44", "64", "84", "E4" : result = niD4
            Case "25", "45", "65", "85", "E5" : result = niD5
            Case "26", "46", "66", "86", "E6" : result = niDP
            Case "27", "47", "67", "87", "E7" : result = niDQ
            Case "28", "48", "68", "88", "E8" : result = niD6
            Case "29", "49", "69", "89", "E9" : result = niD7
            Case "2A", "4A", "6A", "8A", "EA" : result = niD8
            Case "2B", "4B", "6B", "8B", "EB" : result = niD9
            Case "2C", "4C", "6C", "8C", "EC" : result = niDA
            Case "2D", "4D", "6D", "8D", "ED" : result = niDB
            Case "2E", "4E", "6E", "8E", "EE" : result = niDC
            Case "2F", "4F", "6F", "8F", "EF" : result = niDD
            Case "2G", "4G", "6G", "8G", "EG" : result = niDE
            Case "2H", "4H", "6H", "8H", "EH" : result = niDF
            Case "2I", "4I", "6I", "8I", "EI" : result = niDG
            Case "2J", "4J", "6J", "8J", "EJ" : result = niDH
            Case "2K", "4K", "6K", "8K", "EK" : result = niDI
            Case "2L", "4L", "6L", "8L", "EL" : result = niDJ
            Case "2M", "4M", "6M", "8M", "EM" : result = niDK
            Case "2N", "4N", "6N", "8N", "EN" : result = niDL
            Case "2O", "4O", "6O", "8O", "EO" : result = niDM
            Case "2P", "4P", "6P", "8P", "EP" : result = niDN
            Case "2Q", "4Q", "6Q", "8Q", "EQ" : result = niDO
        End Select
        If Rscratch Then
            If result = niA2 Then
                result = niA9
            ElseIf result >= niA3 AndAlso result <= niA9 Then
                result -= 1
            End If
        End If
        Return result
    End Function

End Class
