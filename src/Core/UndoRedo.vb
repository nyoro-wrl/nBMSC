Imports nBMSC.Editor

Public Class UndoRedo
    Public Const opVoid As Byte = 0
    Public Const opAddNote As Byte = 1
    Public Const opRemoveNote As Byte = 2
    Public Const opChangeNote As Byte = 3
    Public Const opMoveNote As Byte = 4
    Public Const opLongNoteModify As Byte = 5
    Public Const opHiddenNoteModify As Byte = 6
    Public Const opRelabelNote As Byte = 7
    Public Const opLandmineNoteModify As Byte = 8
    Public Const opRemoveAllNotes As Byte = 15
    Public Const opChangeMeasureLength As Byte = 16
    Public Const opChangeTimeSelection As Byte = 17
    Public Const opNT As Byte = 18
    'Public Const opChangeVisibleColumns As Byte = 19
    Public Const opWavAutoincFlag As Byte = 20
    Public Const opDefinitionChange As Byte = 21
    Public Const opRandomBlockInsert As Byte = 22
    Public Const opRandomBlockRemove As Byte = 23
    Public Const opRandomDefinitionChange As Byte = 24

    Public Const opNoOperation As Byte = 255

    Private Const trueByte As Byte = 1
    Private Const falseByte As Byte = 0

    Private Const CommandObjectEstimateBytes As Long = 16
    Private Const LinkReferenceEstimateBytes As Long = 8
    Private Const NoteEstimateBytes As Long = 32
    Private Const ArrayEstimateBytes As Long = 24
    Private Const RandomBlockEstimateBytes As Long = 40



    Public MustInherit Class LinkedURCmd
        Public [Next] As LinkedURCmd = Nothing
        Public MustOverride Function ofType() As Byte
        Public MustOverride Function toBytes() As Byte()
        'Public MustOverride Sub fromBytes(ByVal b As Byte())

        Public Overridable Function EstimateBytes() As Long
            Return CommandObjectEstimateBytes + LinkReferenceEstimateBytes
        End Function
    End Class



    Public Shared Function EstimateCommandBytes(ByVal cmd As LinkedURCmd) As Long
        Dim xBytes As Long = 0

        Do While cmd IsNot Nothing
            If cmd.ofType <> opNoOperation Then xBytes += cmd.EstimateBytes()
            cmd = cmd.Next
        Loop

        Return xBytes
    End Function



    Public Shared Function fromBytes(ByVal b() As Byte) As LinkedURCmd
        If b Is Nothing Then Return Nothing
        If b.Length = 0 Then Return Nothing

        Select Case b(0)
            Case opVoid : Return New Void(b)
            Case opAddNote : Return New AddNote(b)
            Case opRemoveNote : Return New RemoveNote(b)
            Case opChangeNote : Return New ChangeNote(b)
            Case opMoveNote : Return New MoveNote(b)
            Case opLongNoteModify : Return New LongNoteModify(b)
            Case opHiddenNoteModify : Return New HiddenNoteModify(b)
            Case opRelabelNote : Return New RelabelNote(b)
            Case opLandmineNoteModify : Return New LandmineNoteModify(b)
            Case opRemoveAllNotes : Return New RemoveAllNotes(b)
            Case opChangeMeasureLength : Return New ChangeMeasureLength(b)
            Case opChangeTimeSelection : Return New ChangeTimeSelection(b)
            Case opNT : Return New NT(b)
                'Case opChangeVisibleColumns : Return New ChangeVisibleColumns(b)
            Case opWavAutoincFlag : Return New WavAutoincFlag(b)
            Case opDefinitionChange : Return New DefinitionChange(b)
            Case opRandomBlockInsert : Return New RandomBlockInsert(b)
            Case opRandomBlockRemove : Return New RandomBlockRemove(b)
            Case opRandomDefinitionChange : Return New RandomDefinitionChange(b)
            Case opNoOperation : Return New NoOperation(b)
            Case Else : Return Nothing
        End Select
    End Function

    Private Shared Function CloneRandomBlock(ByVal block As BmsRandomBlock) As BmsRandomBlock
        Dim copy As New BmsRandomBlock()
        If block Is Nothing Then Return copy

        copy.DefinitionValue = block.DefinitionValue
        copy.CurrentValue = block.CurrentValue
        copy.ViewMode = block.ViewMode

        For Each pair As KeyValuePair(Of Integer, String) In block.ExtraTextByValue
            copy.SetExtraText(pair.Key, pair.Value)
        Next

        copy.Normalize()
        Return copy
    End Function

    Private Shared Sub WriteRandomBlock(ByVal bw As BinaryWriter, ByVal block As BmsRandomBlock)
        Dim copy As BmsRandomBlock = CloneRandomBlock(block)
        bw.Write(copy.DefinitionValue)
        bw.Write(copy.CurrentValue)
        bw.Write(CInt(copy.ViewMode))
        bw.Write(copy.ExtraTextByValue.Count)

        For Each pair As KeyValuePair(Of Integer, String) In copy.ExtraTextByValue
            bw.Write(pair.Key)
            bw.Write(If(pair.Value, ""))
        Next
    End Sub

    Private Shared Function ReadRandomBlock(ByVal br As BinaryReader) As BmsRandomBlock
        Dim block As New BmsRandomBlock()
        block.DefinitionValue = br.ReadInt32()
        block.CurrentValue = br.ReadInt32()
        block.ViewMode = CType(br.ReadInt32(), BmsRandomViewMode)

        Dim extraCount As Integer = br.ReadInt32()
        For i As Integer = 1 To extraCount
            block.SetExtraText(br.ReadInt32(), br.ReadString())
        Next

        block.Normalize()
        Return block
    End Function

    Private Shared Function CloneNotes(ByVal notes() As Note) As Note()
        If notes Is Nothing Then Return New Note() {}

        Return DirectCast(notes.Clone(), Note())
    End Function


    Public Class Void : Inherits LinkedURCmd
        '1 = 1
        Public Overrides Function toBytes() As Byte()
            toBytes = New Byte() {opVoid}
        End Function

        Public Sub New()
        End Sub

        Public Sub New(ByVal b() As Byte)
        End Sub

        Public Overrides Function ofType() As Byte
            Return opVoid
        End Function
    End Class

    Public MustInherit Class LinkedURNoteCmd : Inherits LinkedURCmd
        Public note As Note
        Protected LastReadRandomFields As Boolean = True

        Public Sub New()

        End Sub

        Public Sub New(ByVal b As Note)
            note = b
        End Sub

        Public Sub New(ByVal b() As Byte)
            FromBinaryReader(New BinaryReader(New MemoryStream(b)))
        End Sub

        Public Sub FromBinaryReader(ByRef br As BinaryReader)
            Dim commandType As Byte = br.ReadByte()
            LastReadRandomFields = ShouldReadRandomFields(commandType, br)
            note.FromBinReader(br, LastReadRandomFields)
        End Sub

        Private Shared Function ShouldReadRandomFields(ByVal commandType As Byte, ByVal br As BinaryReader) As Boolean
            Dim remaining As Long = br.BaseStream.Length - br.BaseStream.Position

            Select Case commandType
                Case opAddNote, opRemoveNote
                    Return remaining >= Note.BinarySize
                Case opChangeNote
                    Return remaining >= Note.BinarySize * 2
                Case opMoveNote
                    Return remaining >= Note.BinarySize + 12
                Case opLongNoteModify
                    Return remaining >= Note.BinarySize + 16
                Case opHiddenNoteModify, opLandmineNoteModify
                    Return remaining >= Note.BinarySize + 1
                Case opRelabelNote
                    Return remaining >= Note.BinarySize + 8
            End Select

            Return remaining >= Note.BinarySize
        End Function

        Public Sub WriteBinWriter(ByRef bw As BinaryWriter)
            bw.Write(ofType())
            bw.Write(note.ToBytes())
        End Sub

        Public MustOverride Overrides Function ofType() As Byte

        Public Overrides Function toBytes() As Byte()
            Dim ms = New MemoryStream()
            Dim bw As New BinaryWriter(ms)
            WriteBinWriter(bw)

            Return ms.ToArray()
        End Function

        Public Overrides Function EstimateBytes() As Long
            Return MyBase.EstimateBytes() + NoteEstimateBytes
        End Function
    End Class

    Public Class AddNote : Inherits LinkedURNoteCmd
        Public Sub New(_note As Note)
            note = _note
        End Sub

        Public Sub New(ByVal b() As Byte)
            MyBase.New(b)
        End Sub

        Public Overrides Function ofType() As Byte
            Return opAddNote
        End Function
    End Class



    Public Class RemoveNote : Inherits LinkedURNoteCmd
        Public Sub New(_note As Note)
            note = _note
        End Sub

        Public Sub New(ByVal b() As Byte)
            MyBase.New(b)
        End Sub

        Public Overrides Function ofType() As Byte
            Return opRemoveNote
        End Function
    End Class



    Public Class ChangeNote : Inherits LinkedURNoteCmd
        Public NNote As Note

        Public Overrides Function toBytes() As Byte()
            Dim ms = New MemoryStream()
            Dim bw = New BinaryWriter(ms)
            WriteBinWriter(bw)
            NNote.WriteBinWriter(bw)
            Return ms.ToArray()
        End Function

        Public Overrides Function EstimateBytes() As Long
            Return MyBase.EstimateBytes() + NoteEstimateBytes
        End Function

        Public Sub New(ByVal b() As Byte)
            Dim br = New BinaryReader(New MemoryStream(b))
            FromBinaryReader(br)
            NNote.FromBinReader(br, LastReadRandomFields)
        End Sub

        Public Sub New(note1 As Note, note2 As Note)
            note = note1
            NNote = note2
        End Sub

        Public Overrides Function ofType() As Byte
            Return opChangeNote
        End Function
    End Class



    Public Class MoveNote : Inherits LinkedURNoteCmd
        Public NColumnIndex As Integer = 0
        Public NVPosition As Double = 0

        Public Overrides Function toBytes() As Byte()
            Dim ms = New MemoryStream()
            Dim bw As New BinaryWriter(ms)
            WriteBinWriter(bw)
            bw.Write(NColumnIndex)
            bw.Write(NVPosition)

            Return ms.ToArray()
        End Function

        Public Overrides Function EstimateBytes() As Long
            Return MyBase.EstimateBytes() + 12
        End Function

        Public Sub New(ByVal b() As Byte)
            Dim br As New BinaryReader(New MemoryStream(b))
            FromBinaryReader(br)
            NColumnIndex = br.ReadInt32()
            NVPosition = br.ReadDouble()
        End Sub

        Public Sub New(_note As Note, _ColIndex As Integer, _VPos As Double)
            note = _note
            NColumnIndex = _ColIndex
            NVPosition = _VPos
        End Sub

        Public Overrides Function ofType() As Byte
            Return opMoveNote
        End Function
    End Class



    Public Class LongNoteModify : Inherits LinkedURNoteCmd
        Public NVPosition As Double = 0
        Public NLongNote As Double = 0

        Public Overrides Function toBytes() As Byte()
            Dim ms = New MemoryStream()
            Dim bw = New BinaryWriter(ms)
            WriteBinWriter(bw)
            bw.Write(NVPosition)
            bw.Write(NLongNote)

            Return ms.ToArray()
        End Function

        Public Overrides Function EstimateBytes() As Long
            Return MyBase.EstimateBytes() + 16
        End Function

        Public Sub New(ByVal b() As Byte)
            Dim br = New BinaryReader(New MemoryStream(b))
            FromBinaryReader(br)
            NVPosition = br.ReadDouble()
            NLongNote = br.ReadDouble()
        End Sub

        Public Sub New(_note As Note, ByVal xNVPosition As Double, ByVal xNLongNote As Double)
            note = _note
            NVPosition = xNVPosition
            NLongNote = xNLongNote
        End Sub

        Public Overrides Function ofType() As Byte
            Return opLongNoteModify
        End Function
    End Class



    Public Class HiddenNoteModify : Inherits LinkedURNoteCmd
        Public NHidden As Boolean = False

        Public Overrides Function toBytes() As Byte()
            Dim MS = New MemoryStream()
            Dim bw = New BinaryWriter(MS)
            WriteBinWriter(bw)
            bw.Write(NHidden)
            Return MS.ToArray()
        End Function

        Public Overrides Function EstimateBytes() As Long
            Return MyBase.EstimateBytes() + 4
        End Function

        Public Sub New(ByVal b() As Byte)
            Dim br = New BinaryReader(New MemoryStream(b))
            FromBinaryReader(br)
            NHidden = br.ReadBoolean()
        End Sub

        Public Sub New(_note As Note, ByVal xNHidden As Boolean)
            note = _note
            NHidden = xNHidden
        End Sub

        Public Overrides Function ofType() As Byte
            Return opHiddenNoteModify
        End Function
    End Class



    Public Class LandmineNoteModify : Inherits LinkedURNoteCmd
        Public NLandmine As Boolean = False

        Public Overrides Function toBytes() As Byte()
            Dim MS = New MemoryStream()
            Dim bw = New BinaryWriter(MS)
            WriteBinWriter(bw)
            bw.Write(NLandmine)
            Return MS.ToArray()
        End Function

        Public Overrides Function EstimateBytes() As Long
            Return MyBase.EstimateBytes() + 4
        End Function

        Public Sub New(ByVal b() As Byte)
            Dim br = New BinaryReader(New MemoryStream(b))
            FromBinaryReader(br)
            NLandmine = br.ReadBoolean()
        End Sub

        Public Sub New(_note As Note, ByVal xNLandmine As Boolean)
            note = _note
            NLandmine = xNLandmine
        End Sub

        Public Overrides Function ofType() As Byte
            Return opLandmineNoteModify
        End Function
    End Class



    Public Class RelabelNote : Inherits LinkedURNoteCmd
        '1 + 25 + 4 + 1 = 31

        Public NValue As Long = 10000

        Public Overrides Function toBytes() As Byte()
            Dim ms = New MemoryStream()
            Dim bw = New BinaryWriter(ms)
            WriteBinWriter(bw)
            bw.Write(NValue)

            Return ms.ToArray()
        End Function

        Public Overrides Function EstimateBytes() As Long
            Return MyBase.EstimateBytes() + 8
        End Function

        Public Sub New(ByVal b() As Byte)
            Dim br = New BinaryReader(New MemoryStream(b))
            FromBinaryReader(br)
            NValue = br.ReadInt64
        End Sub

        Public Sub New(_note As Note, ByVal xNValue As Long)
            note = _note
            NValue = xNValue
        End Sub

        Public Overrides Function ofType() As Byte
            Return opRelabelNote
        End Function
    End Class



    Public Class RemoveAllNotes : Inherits LinkedURCmd
        '1 = 1
        Public Overrides Function toBytes() As Byte()
            toBytes = New Byte() {opRemoveAllNotes}
        End Function

        Public Sub New(ByVal b() As Byte)
        End Sub

        Public Sub New()
        End Sub

        Public Overrides Function ofType() As Byte
            Return opRemoveAllNotes
        End Function
    End Class



    Public Class ChangeMeasureLength : Inherits LinkedURCmd
        '1 + 8 + 4 + 4 * Indices.Length = 13 + 4 * Indices.Length
        Public Value As Double = 192
        Public Indices() As Integer = {}

        Public Overrides Function toBytes() As Byte()
            Dim xVal() As Byte = BitConverter.GetBytes(Value)
            Dim xUbound() As Byte = BitConverter.GetBytes(UBound(Indices))
            Dim xToBytes() As Byte = {opChangeMeasureLength,
                                      xVal(0), xVal(1), xVal(2), xVal(3), xVal(4), xVal(5), xVal(6), xVal(7),
                                      xUbound(0), xUbound(1), xUbound(2), xUbound(3)}
            ReDim Preserve xToBytes(12 + 4 * Indices.Length)
            For xI1 As Integer = 13 To UBound(xToBytes) Step 4
                Dim xId() As Byte = BitConverter.GetBytes(Indices((xI1 - 13) \ 4))
                xToBytes(xI1 + 0) = xId(0)
                xToBytes(xI1 + 1) = xId(1)
                xToBytes(xI1 + 2) = xId(2)
                xToBytes(xI1 + 3) = xId(3)
            Next
            Return xToBytes
        End Function

        Public Overrides Function EstimateBytes() As Long
            Dim xCount As Long = 0
            If Indices IsNot Nothing Then xCount = Indices.Length
            Return MyBase.EstimateBytes() + 8 + ArrayEstimateBytes + 4 * xCount
        End Function

        Public Sub New(ByVal b() As Byte)
            Value = BitConverter.ToDouble(b, 1)
            Dim xUbound As Integer = BitConverter.ToInt32(b, 9)
            ReDim Preserve Indices(xUbound)
            For xI1 As Integer = 0 To xUbound
                Indices(xI1) = BitConverter.ToInt32(b, 13 + xI1 * 4)
            Next
        End Sub

        Public Sub New(ByVal xValue As Double, ByVal xIndices() As Integer)
            Value = xValue
            Indices = xIndices
        End Sub

        Public Overrides Function ofType() As Byte
            Return opChangeMeasureLength
        End Function
    End Class



    Public Class ChangeTimeSelection : Inherits LinkedURCmd
        '1 + 8 + 8 + 8 + 1 = 26
        Public SelStart As Double = 0
        Public SelLength As Double = 0
        Public SelHalf As Double = 0
        Public Selected As Boolean = False

        Public Overrides Function toBytes() As Byte()
            Dim xSta() As Byte = BitConverter.GetBytes(SelStart)
            Dim xLen() As Byte = BitConverter.GetBytes(SelLength)
            Dim xHalf() As Byte = BitConverter.GetBytes(SelHalf)
            toBytes = New Byte() {opChangeTimeSelection,
                                  xSta(0), xSta(1), xSta(2), xSta(3), xSta(4), xSta(5), xSta(6), xSta(7),
                                  xLen(0), xLen(1), xLen(2), xLen(3), xLen(4), xLen(5), xLen(6), xLen(7),
                                  xHalf(0), xHalf(1), xHalf(2), xHalf(3), xHalf(4), xHalf(5), xHalf(6), xHalf(7),
                                  IIf(Selected, trueByte, falseByte)}
        End Function

        Public Overrides Function EstimateBytes() As Long
            Return MyBase.EstimateBytes() + 28
        End Function

        Public Sub New(ByVal b() As Byte)
            SelStart = BitConverter.ToDouble(b, 1)
            SelLength = BitConverter.ToDouble(b, 9)
            SelHalf = BitConverter.ToDouble(b, 17)
            Selected = CBool(b(25))
        End Sub

        Public Sub New(ByVal xSelStart As Double, ByVal xSelLength As Double, ByVal xSelHalf As Double, ByVal xSelected As Boolean)
            SelStart = xSelStart
            SelLength = xSelLength
            SelHalf = xSelHalf
            Selected = xSelected
        End Sub

        Public Overrides Function ofType() As Byte
            Return opChangeTimeSelection
        End Function
    End Class



    Public Class NT : Inherits LinkedURCmd
        '1 + 1 + 1 = 3
        Public BecomeNT As Boolean = False
        Public AutoConvert As Boolean = False

        Public Overrides Function toBytes() As Byte()
            toBytes = New Byte() {opNT,
                                  IIf(BecomeNT, trueByte, falseByte),
                                  IIf(AutoConvert, trueByte, falseByte)}
        End Function

        Public Overrides Function EstimateBytes() As Long
            Return MyBase.EstimateBytes() + 8
        End Function

        Public Sub New(ByVal b() As Byte)
            BecomeNT = CBool(b(1))
            AutoConvert = CBool(b(2))
        End Sub

        Public Sub New(ByVal xBecomeNT As Boolean, ByVal xAutoConvert As Boolean)
            BecomeNT = xBecomeNT
            AutoConvert = xAutoConvert
        End Sub

        Public Overrides Function ofType() As Byte
            Return opNT
        End Function
    End Class

    Public Class WavAutoincFlag : Inherits LinkedURCmd
        Public Checked As Boolean = False

        Public Sub New(ByVal _checked As Boolean)
            Checked = _checked
        End Sub
        Public Overrides Function toBytes() As Byte()
            toBytes = New Byte() {opWavAutoincFlag,
                                  IIf(Checked, trueByte, falseByte)}
        End Function

        Public Overrides Function EstimateBytes() As Long
            Return MyBase.EstimateBytes() + 4
        End Function

        Public Sub New(ByVal b() As Byte)
            Checked = CBool(b(1))
        End Sub

        Public Overrides Function ofType() As Byte
            Return opWavAutoincFlag
        End Function

    End Class

    Public Class DefinitionChange : Inherits LinkedURCmd
        Public IsWav As Boolean = True
        Public Index As Integer = 0
        Public Value As String = ""

        Public Sub New(ByVal isWav As Boolean, ByVal index As Integer, ByVal value As String)
            Me.IsWav = isWav
            Me.Index = index
            Me.Value = If(value, "")
        End Sub

        Public Sub New(ByVal b() As Byte)
            Dim br As New BinaryReader(New MemoryStream(b))
            br.ReadByte()
            IsWav = br.ReadBoolean()
            Index = br.ReadInt32()
            Value = br.ReadString()
        End Sub

        Public Overrides Function toBytes() As Byte()
            Dim ms As New MemoryStream()
            Dim bw As New BinaryWriter(ms)
            bw.Write(ofType())
            bw.Write(IsWav)
            bw.Write(Index)
            bw.Write(Value)

            Return ms.ToArray()
        End Function

        Public Overrides Function EstimateBytes() As Long
            Return MyBase.EstimateBytes() + 8 + If(Value Is Nothing, 0, Value.Length * 2)
        End Function

        Public Overrides Function ofType() As Byte
            Return opDefinitionChange
        End Function
    End Class

    Public Class RandomBlockInsert : Inherits LinkedURCmd
        Public Index As Integer = 0
        Public SelectAfter As Integer = -1
        Public Block As BmsRandomBlock = New BmsRandomBlock()
        Public Notes() As Note = New Note() {}

        Public Sub New(ByVal index As Integer, ByVal block As BmsRandomBlock, ByVal selectAfter As Integer)
            Me.New(index, block, New Note() {}, selectAfter)
        End Sub

        Public Sub New(ByVal index As Integer, ByVal block As BmsRandomBlock, ByVal notes() As Note, ByVal selectAfter As Integer)
            Me.Index = index
            Me.Block = CloneRandomBlock(block)
            Me.Notes = CloneNotes(notes)
            Me.SelectAfter = selectAfter
        End Sub

        Public Sub New(ByVal b() As Byte)
            Dim br As New BinaryReader(New MemoryStream(b))
            br.ReadByte()
            Index = br.ReadInt32()
            SelectAfter = br.ReadInt32()
            Block = ReadRandomBlock(br)

            Dim noteCount As Integer = br.ReadInt32()
            If noteCount <= 0 Then
                Notes = New Note() {}
            Else
                ReDim Notes(noteCount - 1)
                For i As Integer = 0 To noteCount - 1
                    Notes(i) = New Note()
                    Notes(i).FromBinReader(br)
                Next
            End If
        End Sub

        Public Overrides Function toBytes() As Byte()
            Dim ms As New MemoryStream()
            Dim bw As New BinaryWriter(ms)
            bw.Write(ofType())
            bw.Write(Index)
            bw.Write(SelectAfter)
            WriteRandomBlock(bw, Block)
            bw.Write(If(Notes Is Nothing, 0, Notes.Length))

            If Notes IsNot Nothing Then
                For Each note As Note In Notes
                    note.WriteBinWriter(bw)
                Next
            End If

            Return ms.ToArray()
        End Function

        Public Overrides Function EstimateBytes() As Long
            Dim xNoteCount As Long = 0
            If Notes IsNot Nothing Then xNoteCount = Notes.Length

            Return MyBase.EstimateBytes() + 8 + RandomBlockEstimateBytes + ArrayEstimateBytes + NoteEstimateBytes * xNoteCount
        End Function

        Public Overrides Function ofType() As Byte
            Return opRandomBlockInsert
        End Function
    End Class

    Public Class RandomBlockRemove : Inherits LinkedURCmd
        Public Index As Integer = 0
        Public SelectAfter As Integer = -1

        Public Sub New(ByVal index As Integer, ByVal selectAfter As Integer)
            Me.Index = index
            Me.SelectAfter = selectAfter
        End Sub

        Public Sub New(ByVal b() As Byte)
            Dim br As New BinaryReader(New MemoryStream(b))
            br.ReadByte()
            Index = br.ReadInt32()
            SelectAfter = br.ReadInt32()
        End Sub

        Public Overrides Function toBytes() As Byte()
            Dim ms As New MemoryStream()
            Dim bw As New BinaryWriter(ms)
            bw.Write(ofType())
            bw.Write(Index)
            bw.Write(SelectAfter)

            Return ms.ToArray()
        End Function

        Public Overrides Function EstimateBytes() As Long
            Return MyBase.EstimateBytes() + 8
        End Function

        Public Overrides Function ofType() As Byte
            Return opRandomBlockRemove
        End Function
    End Class

    Public Class RandomDefinitionChange : Inherits LinkedURCmd
        Public Index As Integer = 0
        Public Value As Integer = 1

        Public Sub New(ByVal index As Integer, ByVal value As Integer)
            Me.Index = index
            Me.Value = Math.Max(1, value)
        End Sub

        Public Sub New(ByVal b() As Byte)
            Dim br As New BinaryReader(New MemoryStream(b))
            br.ReadByte()
            Index = br.ReadInt32()
            Value = Math.Max(1, br.ReadInt32())
        End Sub

        Public Overrides Function toBytes() As Byte()
            Dim ms As New MemoryStream()
            Dim bw As New BinaryWriter(ms)
            bw.Write(ofType())
            bw.Write(Index)
            bw.Write(Value)

            Return ms.ToArray()
        End Function

        Public Overrides Function EstimateBytes() As Long
            Return MyBase.EstimateBytes() + 8
        End Function

        Public Overrides Function ofType() As Byte
            Return opRandomDefinitionChange
        End Function
    End Class




    Public Class NoOperation : Inherits LinkedURCmd
        '1 = 1
        Public Overrides Function toBytes() As Byte()
            toBytes = New Byte() {opNoOperation}
        End Function

        Public Sub New()
        End Sub

        Public Sub New(ByVal b() As Byte)
        End Sub

        Public Overrides Function ofType() As Byte
            Return opNoOperation
        End Function
    End Class
End Class
