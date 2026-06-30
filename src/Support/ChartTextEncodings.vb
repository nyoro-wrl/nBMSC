Imports System.Text

Public Enum TextEncodingMode
    Auto = 0
    SystemDefault = 1
    SJIS = 2
    EUCKR = 3
    UTF8 = 4
    UTF16LE = 5
    UTF16BE = 6
    UTF32LE = 7
    UTF32BE = 8
End Enum

Public NotInheritable Class ChartTextEncodings
    Private Sub New()
    End Sub

    Public Shared Function ShiftJisEncoding() As Encoding
        Return Encoding.GetEncoding(932)
    End Function

    Public Shared Function InputModes() As TextEncodingMode()
        Return New TextEncodingMode() {
            TextEncodingMode.Auto,
            TextEncodingMode.SystemDefault,
            TextEncodingMode.SJIS,
            TextEncodingMode.EUCKR,
            TextEncodingMode.UTF8,
            TextEncodingMode.UTF16LE,
            TextEncodingMode.UTF16BE,
            TextEncodingMode.UTF32LE,
            TextEncodingMode.UTF32BE
        }
    End Function

    Public Shared Function OutputModeToIndex(ByVal mode As TextEncodingMode) As Integer
        Select Case mode
            Case TextEncodingMode.Auto : Return 0
            Case TextEncodingMode.SystemDefault : Return 1
            Case TextEncodingMode.SJIS : Return 2
            Case TextEncodingMode.UTF8 : Return 3
        End Select

        Return 0
    End Function

    Public Shared Function OutputIndexToMode(ByVal index As Integer) As TextEncodingMode
        Select Case index
            Case 0 : Return TextEncodingMode.Auto
            Case 1 : Return TextEncodingMode.SystemDefault
            Case 2 : Return TextEncodingMode.SJIS
            Case 3 : Return TextEncodingMode.UTF8
        End Select

        Return TextEncodingMode.SystemDefault
    End Function

    Public Shared Function ModeToString(ByVal mode As TextEncodingMode) As String
        Select Case mode
            Case TextEncodingMode.Auto : Return "Auto"
            Case TextEncodingMode.SystemDefault : Return "System"
            Case TextEncodingMode.SJIS : Return "SJIS"
            Case TextEncodingMode.EUCKR : Return "EUCKR"
            Case TextEncodingMode.UTF8 : Return "UTF8"
            Case TextEncodingMode.UTF16LE : Return "UTF16LE"
            Case TextEncodingMode.UTF16BE : Return "UTF16BE"
            Case TextEncodingMode.UTF32LE : Return "UTF32LE"
            Case TextEncodingMode.UTF32BE : Return "UTF32BE"
        End Select

        Return "Auto"
    End Function

    Public Shared Function ParseMode(ByVal value As String, ByVal defaultMode As TextEncodingMode) As TextEncodingMode
        If value Is Nothing Then Return defaultMode

        Select Case UCase(Replace(Replace(Replace(value, "-", ""), "_", ""), " ", ""))
            Case "AUTO" : Return TextEncodingMode.Auto
            Case "SYSTEM", "SYSTEMANSI", "ANSI", "DEFAULT" : Return TextEncodingMode.SystemDefault
            Case "SJIS", "SHIFTJIS", "CP932", "932" : Return TextEncodingMode.SJIS
            Case "EUCKR", "CP949", "949" : Return TextEncodingMode.EUCKR
            Case "UTF8", "UTF7" : Return TextEncodingMode.UTF8
            Case "UTF16LE", "LITTLEENDIANUTF16", "UNICODE" : Return TextEncodingMode.UTF16LE
            Case "UTF16BE", "BIGENDIANUTF16" : Return TextEncodingMode.UTF16BE
            Case "UTF32LE", "LITTLEENDIANUTF32" : Return TextEncodingMode.UTF32LE
            Case "UTF32BE", "BIGENDIANUTF32" : Return TextEncodingMode.UTF32BE
            Case "ASCII" : Return TextEncodingMode.SJIS
        End Select

        Return defaultMode
    End Function

    Public Shared Function CoerceOutputMode(ByVal mode As TextEncodingMode) As TextEncodingMode
        Select Case mode
            Case TextEncodingMode.Auto, TextEncodingMode.SystemDefault, TextEncodingMode.SJIS, TextEncodingMode.UTF8
                Return mode
        End Select

        Return TextEncodingMode.SystemDefault
    End Function

    Public Shared Function ToEncoding(ByVal mode As TextEncodingMode) As Encoding
        Select Case mode
            Case TextEncodingMode.SystemDefault
                Return Encoding.Default
            Case TextEncodingMode.SJIS
                Return ShiftJisEncoding()
            Case TextEncodingMode.EUCKR
                Return Encoding.GetEncoding("EUC-KR")
            Case TextEncodingMode.UTF8
                Return New UTF8Encoding(True, False)
            Case TextEncodingMode.UTF16LE
                Return New UnicodeEncoding(False, True)
            Case TextEncodingMode.UTF16BE
                Return New UnicodeEncoding(True, True)
            Case TextEncodingMode.UTF32LE
                Return New UTF32Encoding(False, True)
            Case TextEncodingMode.UTF32BE
                Return New UTF32Encoding(True, True)
        End Select

        Return ShiftJisEncoding()
    End Function

    Public Shared Function FromEncoding(ByVal encoding As Encoding) As TextEncodingMode
        If encoding Is Nothing Then Return TextEncodingMode.SJIS

        Select Case encoding.CodePage
            Case 932
                Return TextEncodingMode.SJIS
            Case Encoding.GetEncoding("EUC-KR").CodePage
                Return TextEncodingMode.EUCKR
            Case Encoding.Default.CodePage
                Return TextEncodingMode.SystemDefault
            Case Encoding.UTF8.CodePage
                Return TextEncodingMode.UTF8
            Case Encoding.Unicode.CodePage
                Return TextEncodingMode.UTF16LE
            Case Encoding.BigEndianUnicode.CodePage
                Return TextEncodingMode.UTF16BE
            Case Encoding.UTF32.CodePage
                Return TextEncodingMode.UTF32LE
        End Select

        If encoding.WebName = "utf-32BE" OrElse encoding.WebName = "utf-32be" Then Return TextEncodingMode.UTF32BE
        Return TextEncodingMode.SJIS
    End Function

    Public Shared Function HasBom(ByVal bytes() As Byte, ByVal bom() As Byte) As Boolean
        If bytes.Length < bom.Length Then Return False
        For i As Integer = 0 To bom.Length - 1
            If bytes(i) <> bom(i) Then Return False
        Next
        Return True
    End Function

    Public Shared Function EncodingFromBom(ByVal bytes() As Byte) As Encoding
        If HasBom(bytes, New Byte() {&H0, &H0, &HFE, &HFF}) Then Return New UTF32Encoding(True, True)
        If HasBom(bytes, New Byte() {&HFF, &HFE, &H0, &H0}) Then Return New UTF32Encoding(False, True)
        If HasBom(bytes, New Byte() {&HEF, &HBB, &HBF}) Then Return New UTF8Encoding(True, False)
        If HasBom(bytes, New Byte() {&HFE, &HFF}) Then Return New UnicodeEncoding(True, True)
        If HasBom(bytes, New Byte() {&HFF, &HFE}) Then Return New UnicodeEncoding(False, True)
        Return Nothing
    End Function

    Public Shared Function PreambleLength(ByVal bytes() As Byte, ByVal encoding As Encoding) As Integer
        Dim preamble() As Byte = encoding.GetPreamble()
        If preamble Is Nothing OrElse preamble.Length = 0 Then Return 0
        If HasBom(bytes, preamble) Then Return preamble.Length
        Return 0
    End Function

    Public Shared Function IsValidShiftJis(ByVal bytes() As Byte, Optional ByVal size As Integer = 1024 * 64) As Boolean
        If bytes.Length < 2 Then Return False

        Dim limit As Integer = Math.Min(bytes.Length, size)
        Dim i As Integer = 0
        While i < limit
            Dim value As Byte = bytes(i)
            If value <= &H7F OrElse (value >= &HA1 AndAlso value <= &HDF) Then
                i += 1
            ElseIf (value >= &H81 AndAlso value <= &H9F) OrElse (value >= &HE0 AndAlso value <= &HEF) Then
                If i + 1 >= limit Then Return False

                Dim nextValue As Byte = bytes(i + 1)
                If (nextValue >= &H40 AndAlso nextValue <= &H7E) OrElse (nextValue >= &H80 AndAlso nextValue <= &HFC) Then
                    i += 2
                Else
                    Return False
                End If
            Else
                Return False
            End If
        End While

        Return True
    End Function

    Public Shared Function IsValidEucKr(ByVal bytes() As Byte, Optional ByVal size As Integer = 1024 * 64) As Boolean
        If bytes.Length < 2 Then Return False

        Dim limit As Integer = Math.Min(bytes.Length, size)
        Dim i As Integer = 0
        While i < limit
            Dim value As Byte = bytes(i)
            If value <= &H7F Then
                i += 1
            ElseIf value >= &H81 AndAlso value <= &HFE Then
                If i + 1 >= limit Then Return False

                Dim nextValue As Byte = bytes(i + 1)
                If nextValue >= &H81 AndAlso nextValue <= &HFE Then
                    i += 2
                Else
                    Return False
                End If
            Else
                Return False
            End If
        End While

        Return True
    End Function

    Public Shared Function IsValidUtf8(ByVal bytes() As Byte) As Boolean
        Try
            Dim utf8 As New UTF8Encoding(False, True)
            utf8.GetString(bytes)
            Return True
        Catch ex As DecoderFallbackException
            Return False
        End Try
    End Function

    Public Shared Function LooksLikeChartText(ByVal text As String) As Boolean
        Return text.Contains("#") AndAlso
               (text.Contains(vbCrLf) OrElse text.Contains(vbCr) OrElse text.Contains(vbLf))
    End Function

    Public Shared Function IsRoundTripEncoding(ByVal bytes() As Byte, ByVal encoding As Encoding, Optional ByVal size As Integer = 1024 * 64) As Boolean
        Try
            Dim length As Integer = Math.Min(bytes.Length, size)
            If length = 0 Then Return False
            Dim sample(length - 1) As Byte
            Array.Copy(bytes, sample, length)
            Dim text As String = encoding.GetString(sample)
            If Not LooksLikeChartText(text) Then Return False

            Dim roundTrip() As Byte = encoding.GetBytes(text)
            If roundTrip.Length <> sample.Length Then Return False
            For i As Integer = 0 To sample.Length - 1
                If sample(i) <> roundTrip(i) Then Return False
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Shared Function DetectEncoding(ByVal bytes() As Byte) As Encoding
        Dim encoding As Encoding = EncodingFromBom(bytes)
        If encoding IsNot Nothing Then Return encoding

        If IsValidShiftJis(bytes) Then Return ShiftJisEncoding()
        If IsValidEucKr(bytes) Then Return Encoding.GetEncoding("EUC-KR")
        If IsValidUtf8(bytes) Then Return New UTF8Encoding(True, False)

        Dim encodings() As Encoding = {
            New UnicodeEncoding(True, True, True),
            New UnicodeEncoding(False, True, True),
            New UTF32Encoding(True, True, True),
            New UTF32Encoding(False, True, True)
        }
        For Each candidate As Encoding In encodings
            If IsRoundTripEncoding(bytes, candidate) Then Return candidate
        Next

        Return ShiftJisEncoding()
    End Function

    Public Shared Function DecodeText(ByVal bytes() As Byte, ByVal mode As TextEncodingMode, ByRef encoding As Encoding) As String
        If mode = TextEncodingMode.Auto Then
            encoding = DetectEncoding(bytes)
        Else
            encoding = ToEncoding(mode)
        End If

        Dim offset As Integer = PreambleLength(bytes, encoding)
        Return encoding.GetString(bytes, offset, bytes.Length - offset)
    End Function
End Class
