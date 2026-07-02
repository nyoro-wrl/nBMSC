Option Strict On

Imports System
Imports System.IO
Imports System.Text
Imports System.Threading
Imports nBMSC
Imports nBMSC.Editor

Module TestRunner
    Private Passed As Integer
    Private Failed As Integer
    Private Const TestTimeoutMilliseconds As Integer = 10000

    <STAThread()>
    Public Function Main() As Integer
        Run("definition label conversion", AddressOf DefinitionLabelConversion)
        Run("channel label conversion", AddressOf ChannelLabelConversion)
        Run("label validation", AddressOf LabelValidation)
        Run("BMS definition labels", AddressOf BmsDefinitionLabels)
        Run("BMS random parser", AddressOf BmsRandomParserParsing)
        Run("BMS random parser omitted end", AddressOf BmsRandomParserOmittedEnd)
        Run("BMS random parser unsupported raw", AddressOf BmsRandomParserUnsupportedRaw)
        Run("chart calculations", AddressOf ChartCalculations)
        Run("chart paths", AddressOf ChartPaths)
        Run("chart mode detection", AddressOf ChartModeDetection)
        Run("theme metadata", AddressOf ThemeMetadata)
        Run("chart text encoding modes", AddressOf ChartTextEncodingModes)
        Run("chart text encoding detection", AddressOf ChartTextEncodingDetection)
        Run("version tag parsing", AddressOf VersionTagParsing)
        Run("undo redo serialization", AddressOf UndoRedoSerialization)

        Console.WriteLine()
        Console.WriteLine("Passed: " & Passed.ToString())
        Console.WriteLine("Failed: " & Failed.ToString())

        If Failed > 0 Then
            Return 1
        End If

        Return 0
    End Function

    Private Sub Run(ByVal name As String, ByVal test As Action)
        Dim caught As Exception = Nothing
        Dim worker As New Thread(Sub()
                                     Try
                                         test()
                                     Catch ex As Exception
                                         caught = ex
                                     End Try
                                 End Sub)
        worker.IsBackground = True
        worker.SetApartmentState(ApartmentState.STA)
        worker.Start()

        If Not worker.Join(TestTimeoutMilliseconds) Then
            Failed += 1
            Console.WriteLine("[FAIL] " & name)
            Console.WriteLine("Timed out after " & TestTimeoutMilliseconds.ToString() & " ms")
            Try
                worker.Abort()
            Catch ex As Exception
            End Try
            Return
        End If

        If caught Is Nothing Then
            Passed += 1
            Console.WriteLine("[PASS] " & name)
        Else
            Failed += 1
            Console.WriteLine("[FAIL] " & name)
            Console.WriteLine(caught.Message)
        End If
    End Sub

    Private Sub DefinitionLabelConversion()
        AssertEqual("00", Functions.C10to36(0), "zero should be 00")
        AssertEqual("0A", Functions.C10to36(10), "10 should be 0A")
        AssertEqual("0z", Functions.C10to36(61), "61 should be 0z")
        AssertEqual("10", Functions.C10to36(62), "62 should be 10")
        AssertEqual("zz", Functions.C10to36(Functions.MaxDefinition), "max definition should be zz")
        AssertEqual("00", Functions.C10to36(-1), "negative values should clamp to 00")
        AssertEqual("zz", Functions.C10to36(Functions.MaxDefinition + 1), "too large values should clamp")
        AssertEqual(0, Functions.C36to10("00"), "00 should be 0")
        AssertEqual(3843, Functions.C36to10("zz"), "zz should be max definition")
    End Sub

    Private Sub ChannelLabelConversion()
        AssertEqual("00", Functions.C10to36Channel(0), "zero channel should be 00")
        AssertEqual("0Z", Functions.C10to36Channel(35), "35 should be 0Z")
        AssertEqual("10", Functions.C10to36Channel(36), "36 should be 10")
        AssertEqual("ZZ", Functions.C10to36Channel(Functions.MaxBase36Definition), "max channel should be ZZ")
        AssertEqual(1295, Functions.C36ChannelTo10("ZZ"), "ZZ should be max channel")
        AssertEqual(1295, Functions.C36ChannelTo10("zz"), "channel parsing should ignore case")
    End Sub

    Private Sub LabelValidation()
        AssertTrue(Functions.IsBase36("0AZ9"), "uppercase base36 should be valid")
        AssertFalse(Functions.IsBase36("0Az"), "lowercase should not be valid base36")
        AssertTrue(Functions.IsBase62("0Az"), "mixed case base62 should be valid")
        AssertFalse(Functions.IsBase62("0A-"), "symbols should not be valid base62")
    End Sub

    Private Sub BmsDefinitionLabels()
        AssertEqual(Functions.MaxBase36Definition, nBMSC.Editor.BmsDefinitionLabels.DisplayMax(False), "base36 display max")
        AssertEqual(Functions.MaxDefinition, nBMSC.Editor.BmsDefinitionLabels.DisplayMax(True), "base62 display max")
        AssertEqual("0A", nBMSC.Editor.BmsDefinitionLabels.Label(10, False), "base36 label")
        AssertEqual("0a", nBMSC.Editor.BmsDefinitionLabels.Label(36, True), "base62 label")
        AssertEqual(10, nBMSC.Editor.BmsDefinitionLabels.Index("0A", False), "base36 index")
        AssertEqual(36, nBMSC.Editor.BmsDefinitionLabels.Index("0a", True), "base62 index")
        AssertFalse(nBMSC.Editor.BmsDefinitionLabels.IsLabel("0a", False), "base36 rejects lowercase")
        AssertTrue(nBMSC.Editor.BmsDefinitionLabels.IsLabel("0a", True), "base62 accepts lowercase")

        AssertFalse(nBMSC.Editor.BmsDefinitionLabels.ContainsBase62Definitions(New String() {"#WAV0A sound.wav", "#00111:000A"}), "uppercase labels are base36-compatible")
        AssertTrue(nBMSC.Editor.BmsDefinitionLabels.ContainsBase62Definitions(New String() {"#WAV0a sound.wav"}), "lowercase definition label requires base62")
        AssertTrue(nBMSC.Editor.BmsDefinitionLabels.ContainsBase62Definitions(New String() {"#BPM0a 120"}), "lowercase BPM definition requires base62")
        AssertTrue(nBMSC.Editor.BmsDefinitionLabels.ContainsBase62Definitions(New String() {"#STOP0a 192"}), "lowercase STOP definition requires base62")
        AssertTrue(nBMSC.Editor.BmsDefinitionLabels.ContainsBase62Definitions(New String() {"#SCROLL0a 1"}), "lowercase SCROLL definition requires base62")
        AssertTrue(nBMSC.Editor.BmsDefinitionLabels.ContainsBase62Definitions(New String() {"#LNOBJ 0a"}), "lowercase LNOBJ requires base62")
        AssertTrue(nBMSC.Editor.BmsDefinitionLabels.ContainsBase62Definitions(New String() {"#00111:000a"}), "lowercase note label requires base62")
        AssertTrue(nBMSC.Editor.BmsDefinitionLabels.ContainsBase62Definitions(New String() {"#00108:000a"}), "lowercase BPM note label requires base62")
        AssertTrue(nBMSC.Editor.BmsDefinitionLabels.ContainsBase62Definitions(New String() {"#00109:000a"}), "lowercase STOP note label requires base62")
        AssertFalse(nBMSC.Editor.BmsDefinitionLabels.ContainsBase62Definitions(New String() {"#00103:000a"}), "BPM hex channel should not force base62")
    End Sub

    Private Sub BmsRandomParserParsing()
        Dim result As BmsRandomParseResult = BmsRandomParser.Parse(New String() {
            "#TITLE Test",
            "#RANDOM 3",
            "#IF 1",
            "#00111:01",
            "#ENDIF",
            "#IF 2",
            "#00112:01",
            "#ENDIF",
            "#ENDRANDOM",
            "#ARTIST Tester"
        })

        AssertEqual(2, result.TopLevelLines.Count, "top level line count")
        AssertEqual(1, result.Blocks.Count, "random block count")
        AssertEqual(3, result.Blocks(0).DefinitionValue, "random definition value")
        AssertFalse(result.Blocks(0).IsRawText, "simple random should be structured")
        AssertEqual(2, result.Blocks(0).Branches.Count, "branch count")
        AssertEqual(1, result.Blocks(0).Branches(0).Value, "first branch value")
        AssertEqual("#00111:01", result.Blocks(0).Branches(0).Lines(0), "first branch data")
    End Sub

    Private Sub BmsRandomParserOmittedEnd()
        Dim result As BmsRandomParseResult = BmsRandomParser.Parse(New String() {
            "#RANDOM 2",
            "#IF 1",
            "#00111:01",
            "#ENDIF",
            "#IF 2",
            "#00112:01",
            "#ENDIF",
            "#00113:01"
        })

        AssertEqual(1, result.Blocks.Count, "omitted random block count")
        AssertFalse(result.Blocks(0).IsRawText, "omitted random should be structured")
        AssertEqual(2, result.Blocks(0).Branches.Count, "omitted branch count")
        AssertEqual(1, result.TopLevelLines.Count, "line after omitted random should be top level")
        AssertEqual("#00113:01", result.TopLevelLines(0), "top level line after omitted random")
    End Sub

    Private Sub BmsRandomParserUnsupportedRaw()
        Dim result As BmsRandomParseResult = BmsRandomParser.Parse(New String() {
            "#RANDOM 2",
            "#IF 1",
            "#00111:01",
            "#ENDIF",
            "#00113:01",
            "#ENDRANDOM"
        })

        AssertEqual(1, result.Blocks.Count, "raw random block count")
        AssertTrue(result.Blocks(0).IsRawText, "orphan line in explicit random should be raw text")
        AssertEqual(0, result.Blocks(0).Branches.Count, "raw random should not expose structured branches")
        AssertContains(JoinLines(result.Blocks(0).RawLines), "#00113:01", "raw random should keep orphan data line")
    End Sub

    Private Sub ChartCalculations()
        AssertEqual(260, nBMSC.Editor.ChartCalculations.CalculateRecommendedTotal(0), "zero notes recommended total")
        AssertEqual(260, nBMSC.Editor.ChartCalculations.CalculateRecommendedTotal(50), "low note count minimum")
        AssertEqual(380, nBMSC.Editor.ChartCalculations.CalculateRecommendedTotal(650), "nominal recommended total")
    End Sub

    Private Sub ChartPaths()
        AssertEqual("sound.wav", nBMSC.Editor.ChartPaths.GetFileName("C:\Charts\Song\sound.wav"), "windows file name")
        AssertEqual("C:\Charts\Song", nBMSC.Editor.ChartPaths.ExcludeFileName("C:\Charts\Song\sound.wav"), "windows directory")
        AssertEqual("sound.wav", nBMSC.Editor.ChartPaths.MakeBmsReferencePath("", "C:\Charts\Song\sound.wav"), "rooted path without base should become file name")
        AssertEqual("audio\sound.wav", nBMSC.Editor.ChartPaths.MakeBmsReferencePath("C:\Charts\Song", "C:\Charts\Song\audio\sound.wav"), "child path should be relative")
        AssertEqual("..\Shared\sound.wav", nBMSC.Editor.ChartPaths.MakeBmsReferencePath("C:\Charts\Song", "C:\Charts\Shared\sound.wav"), "sibling path should be relative")
        AssertEqual("audio\sound.wav", nBMSC.Editor.ChartPaths.ResolveBmsFilePath("", "audio\sound.wav"), "relative path without base should stay relative")
        AssertEqual("C:\Charts\Song\audio\sound.wav", nBMSC.Editor.ChartPaths.ResolveBmsFilePath("C:\Charts\Song", "audio\sound.wav"), "relative path should resolve against base")
    End Sub

    Private Sub ChartModeDetection()
        AssertEqual(ChartMode.Key9, ChartModes.DetectFromBms("C:\Charts\song.pms", False, False, False, False), "pms extension should be 9key")
        AssertEqual(ChartMode.Key7, ChartModes.DetectFromBms("C:\Charts\song.bms", False, False, False, False), "empty chart should default to 7key")

        Dim hasPlayable As Boolean = False
        Dim has24 As Boolean = False
        Dim has7 As Boolean = False
        Dim has5 As Boolean = False
        ChartModes.ObserveBmsChannel("11", "01", hasPlayable, has24, has7, has5)
        AssertTrue(hasPlayable, "5key zone should be playable")
        AssertFalse(has24, "5key zone should not be 24key")
        AssertFalse(has7, "5key zone should not be 7key")
        AssertTrue(has5, "5key zone should be 5key")
        AssertEqual(ChartMode.Key5, ChartModes.DetectFromBms("C:\Charts\song.bms", hasPlayable, has24, has7, has5), "5key zone should be 5key")

        hasPlayable = False
        has24 = False
        has7 = False
        has5 = False
        ChartModes.ObserveBmsChannel("18", "01", hasPlayable, has24, has7, has5)
        AssertEqual(ChartMode.Key7, ChartModes.DetectFromBms("C:\Charts\song.bms", hasPlayable, has24, has7, has5), "7key zone should be 7key")

        hasPlayable = False
        has24 = False
        has7 = False
        has5 = False
        ChartModes.ObserveBmsChannel("1A", "01", hasPlayable, has24, has7, has5)
        AssertEqual(ChartMode.Key24, ChartModes.DetectFromBms("C:\Charts\song.bms", hasPlayable, has24, has7, has5), "24key zone should be 24key")

        hasPlayable = False
        has24 = False
        has7 = False
        has5 = False
        ChartModes.ObserveBmsChannel("D7", "01", hasPlayable, has24, has7, has5)
        AssertEqual(ChartMode.Key24, ChartModes.DetectFromBms("C:\Charts\song.bms", hasPlayable, has24, has7, has5), "24key landmine zone should be 24key")
    End Sub

    Private Sub ThemeMetadata()
        Dim themePath As String = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") & ".xml")

        Try
            File.WriteAllText(themePath, "<nBMSCTheme version=""1"" name=""Compact"" />", Encoding.UTF8)

            Dim themeName As String = ""
            AssertTrue(nBMSC.ThemeMetadata.TryReadThemeName(themePath, themeName), "valid theme should read")
            AssertEqual("Compact", themeName, "theme name")

            File.WriteAllText(themePath, "<nBMSCTheme version=""1"" />", Encoding.UTF8)
            themeName = ""
            AssertTrue(nBMSC.ThemeMetadata.TryReadThemeName(themePath, themeName), "valid unnamed theme should read")
            AssertEqual(Path.GetFileNameWithoutExtension(themePath), themeName, "fallback theme name")

            File.WriteAllText(themePath, "<nBMSCTheme version=""2"" name=""Compact"" />", Encoding.UTF8)
            themeName = ""
            AssertFalse(nBMSC.ThemeMetadata.TryReadThemeName(themePath, themeName), "unsupported theme version should fail")
        Finally
            If File.Exists(themePath) Then File.Delete(themePath)
        End Try
    End Sub

    Private Sub ChartTextEncodingModes()
        AssertEqual(TextEncodingMode.SJIS, ChartTextEncodings.ParseMode("Shift-JIS", TextEncodingMode.Auto), "Shift-JIS alias")
        AssertEqual(TextEncodingMode.EUCKR, ChartTextEncodings.ParseMode("cp949", TextEncodingMode.Auto), "EUC-KR alias")
        AssertEqual(TextEncodingMode.UTF16LE, ChartTextEncodings.ParseMode("little endian utf16", TextEncodingMode.Auto), "UTF-16LE alias")
        AssertEqual(TextEncodingMode.SJIS, ChartTextEncodings.ParseMode("ascii", TextEncodingMode.Auto), "ASCII compatibility")
        AssertEqual(TextEncodingMode.UTF8, ChartTextEncodings.ParseMode("unknown", TextEncodingMode.UTF8), "unknown value should use default")

        AssertEqual(3, ChartTextEncodings.OutputModeToIndex(TextEncodingMode.UTF8), "UTF-8 output index")
        AssertEqual(TextEncodingMode.SystemDefault, ChartTextEncodings.OutputIndexToMode(99), "invalid output index")
        AssertEqual(TextEncodingMode.SystemDefault, ChartTextEncodings.CoerceOutputMode(TextEncodingMode.UTF16BE), "unsupported output mode")
        AssertEqual("UTF32BE", ChartTextEncodings.ModeToString(TextEncodingMode.UTF32BE), "mode config text")
    End Sub

    Private Sub ChartTextEncodingDetection()
        Dim text As String = "#TITLE Test" & vbCrLf
        Dim utf8Bom() As Byte = Combine(New Byte() {&HEF, &HBB, &HBF}, Encoding.UTF8.GetBytes(text))
        Dim detected As Encoding = ChartTextEncodings.DetectEncoding(utf8Bom)

        AssertEqual(TextEncodingMode.UTF8, ChartTextEncodings.FromEncoding(detected), "UTF-8 BOM detection")
        AssertEqual(3, ChartTextEncodings.PreambleLength(utf8Bom, detected), "UTF-8 BOM length")

        Dim decodedEncoding As Encoding = Nothing
        AssertEqual(text, ChartTextEncodings.DecodeText(utf8Bom, TextEncodingMode.Auto, decodedEncoding), "UTF-8 BOM should be stripped")
        AssertEqual(TextEncodingMode.UTF8, ChartTextEncodings.FromEncoding(decodedEncoding), "decoded UTF-8 mode")

        Dim sjisText As String = "#TITLE テスト" & vbCrLf
        Dim sjisBytes() As Byte = ChartTextEncodings.ShiftJisEncoding().GetBytes(sjisText)
        AssertEqual(TextEncodingMode.SJIS, ChartTextEncodings.FromEncoding(ChartTextEncodings.DetectEncoding(sjisBytes)), "Shift-JIS detection")

        AssertEqual(TextEncodingMode.EUCKR, ChartTextEncodings.FromEncoding(ChartTextEncodings.DetectEncoding(New Byte() {&HF0, &HA1})), "EUC-KR byte validation fallback")
        AssertFalse(ChartTextEncodings.IsValidUtf8(New Byte() {&HC3, &H28}), "invalid UTF-8 should be rejected")
    End Sub

    Private Sub VersionTagParsing()
        Dim version As Version = Nothing

        AssertTrue(UpdateChecker.TryParseVersionTag("5.1.0", version), "plain version should parse")
        AssertEqual(New Version(5, 1, 0), version, "plain version value")

        version = Nothing
        AssertTrue(UpdateChecker.TryParseVersionTag("v5.1.0", version), "v-prefixed version should parse")
        AssertEqual(New Version(5, 1, 0), version, "v-prefixed version value")

        version = Nothing
        AssertFalse(UpdateChecker.TryParseVersionTag("5.1", version), "major.minor should not parse")
        AssertFalse(UpdateChecker.TryParseVersionTag("5.1.0.0", version), "four part versions should not parse")
        AssertFalse(UpdateChecker.TryParseVersionTag("", version), "empty versions should not parse")
    End Sub

    Private Sub UndoRedoSerialization()
        Dim note As New Note(3, 192.0R, 120000, 48.0R, True, False, True, 2, 3)
        Dim move As New UndoRedo.MoveNote(note, 6, 384.0R)
        Dim roundTripMove As UndoRedo.MoveNote = DirectCast(UndoRedo.fromBytes(move.toBytes()), UndoRedo.MoveNote)

        AssertEqual(UndoRedo.opMoveNote, roundTripMove.ofType(), "move command type")
        AssertEqual(6, roundTripMove.NColumnIndex, "move command target column")
        AssertApprox(384.0R, roundTripMove.NVPosition, "move command target position")
        AssertEqual(note.ColumnIndex, roundTripMove.note.ColumnIndex, "note column")
        AssertApprox(note.VPosition, roundTripMove.note.VPosition, "note position")
        AssertEqual(note.Value, roundTripMove.note.Value, "note value")
        AssertEqual(note.LongNote, roundTripMove.note.LongNote, "note long flag")
        AssertApprox(note.Length, roundTripMove.note.Length, "note length")
        AssertEqual(note.Hidden, roundTripMove.note.Hidden, "note hidden flag")
        AssertEqual(note.Landmine, roundTripMove.note.Landmine, "note landmine flag")
        AssertEqual(note.RandomIndex, roundTripMove.note.RandomIndex, "note random index")
        AssertEqual(note.RandomValue, roundTripMove.note.RandomValue, "note random value")

        Dim updatedNote As Note = note
        updatedNote.Value = 130000
        updatedNote.RandomValue = 4
        Dim noteChange As New UndoRedo.ChangeNote(note, updatedNote)
        Dim roundTripNoteChange As UndoRedo.ChangeNote = DirectCast(UndoRedo.fromBytes(noteChange.toBytes()), UndoRedo.ChangeNote)

        AssertEqual(UndoRedo.opChangeNote, roundTripNoteChange.ofType(), "note change command type")
        AssertEqual(note.Value, roundTripNoteChange.note.Value, "note change old value")
        AssertEqual(updatedNote.Value, roundTripNoteChange.NNote.Value, "note change new value")
        AssertEqual(note.RandomValue, roundTripNoteChange.note.RandomValue, "note change old random value")
        AssertEqual(updatedNote.RandomValue, roundTripNoteChange.NNote.RandomValue, "note change new random value")

        Dim longNote As New UndoRedo.LongNoteModify(note, 128.0R, 96.0R)
        Dim roundTripLongNote As UndoRedo.LongNoteModify = DirectCast(UndoRedo.fromBytes(longNote.toBytes()), UndoRedo.LongNoteModify)

        AssertEqual(UndoRedo.opLongNoteModify, roundTripLongNote.ofType(), "long note command type")
        AssertApprox(128.0R, roundTripLongNote.NVPosition, "long note target position")
        AssertApprox(96.0R, roundTripLongNote.NLongNote, "long note target length")

        Dim measureLength As New UndoRedo.ChangeMeasureLength(256.0R, New Integer() {2, 4})
        Dim roundTripMeasureLength As UndoRedo.ChangeMeasureLength = DirectCast(UndoRedo.fromBytes(measureLength.toBytes()), UndoRedo.ChangeMeasureLength)

        AssertEqual(UndoRedo.opChangeMeasureLength, roundTripMeasureLength.ofType(), "measure length command type")
        AssertApprox(256.0R, roundTripMeasureLength.Value, "measure length value")
        AssertEqual(2, roundTripMeasureLength.Indices.Length, "measure length index count")
        AssertEqual(2, roundTripMeasureLength.Indices(0), "measure length first index")
        AssertEqual(4, roundTripMeasureLength.Indices(1), "measure length second index")

        Dim selection As New UndoRedo.ChangeTimeSelection(10.0R, 20.0R, 30.0R, True)
        Dim roundTripSelection As UndoRedo.ChangeTimeSelection = DirectCast(UndoRedo.fromBytes(selection.toBytes()), UndoRedo.ChangeTimeSelection)

        AssertEqual(UndoRedo.opChangeTimeSelection, roundTripSelection.ofType(), "time selection command type")
        AssertApprox(10.0R, roundTripSelection.SelStart, "time selection start")
        AssertApprox(20.0R, roundTripSelection.SelLength, "time selection length")
        AssertApprox(30.0R, roundTripSelection.SelHalf, "time selection half")
        AssertTrue(roundTripSelection.Selected, "time selection selected flag")

        Dim change As New UndoRedo.DefinitionChange(False, 62, "kick.wav")
        Dim roundTripChange As UndoRedo.DefinitionChange = DirectCast(UndoRedo.fromBytes(change.toBytes()), UndoRedo.DefinitionChange)

        AssertEqual(UndoRedo.opDefinitionChange, roundTripChange.ofType(), "definition command type")
        AssertFalse(roundTripChange.IsWav, "definition command target")
        AssertEqual(62, roundTripChange.Index, "definition command index")
        AssertEqual("kick.wav", roundTripChange.Value, "definition command value")

        Dim randomBlock As New BmsRandomBlock(4)
        randomBlock.CurrentValue = 3
        randomBlock.ViewMode = BmsRandomViewMode.AllBranches
        randomBlock.SetExtraText(3, "#GENRE random")
        Dim randomNotes As Note() = {New Note(5, 384.0R, 10000, 0, False, True, False, 1, 3)}
        Dim randomInsert As New UndoRedo.RandomBlockInsert(1, randomBlock, randomNotes, 1)
        Dim roundTripRandomInsert As UndoRedo.RandomBlockInsert = DirectCast(UndoRedo.fromBytes(randomInsert.toBytes()), UndoRedo.RandomBlockInsert)

        AssertEqual(UndoRedo.opRandomBlockInsert, roundTripRandomInsert.ofType(), "random insert command type")
        AssertEqual(1, roundTripRandomInsert.Index, "random insert index")
        AssertEqual(1, roundTripRandomInsert.SelectAfter, "random insert selected index")
        AssertEqual(4, roundTripRandomInsert.Block.DefinitionValue, "random insert definition")
        AssertEqual(3, roundTripRandomInsert.Block.CurrentValue, "random insert current value")
        AssertEqual(BmsRandomViewMode.AllBranches, roundTripRandomInsert.Block.ViewMode, "random insert view")
        AssertEqual("#GENRE random", roundTripRandomInsert.Block.GetExtraText(3), "random insert extra")
        AssertEqual(1, roundTripRandomInsert.Notes.Length, "random insert note count")
        AssertEqual(3, roundTripRandomInsert.Notes(0).RandomValue, "random insert note random value")

        Dim randomRemove As New UndoRedo.RandomBlockRemove(2, 1)
        Dim roundTripRandomRemove As UndoRedo.RandomBlockRemove = DirectCast(UndoRedo.fromBytes(randomRemove.toBytes()), UndoRedo.RandomBlockRemove)

        AssertEqual(UndoRedo.opRandomBlockRemove, roundTripRandomRemove.ofType(), "random remove command type")
        AssertEqual(2, roundTripRandomRemove.Index, "random remove index")
        AssertEqual(1, roundTripRandomRemove.SelectAfter, "random remove selected index")

        Dim randomDefinition As New UndoRedo.RandomDefinitionChange(3, 8)
        Dim roundTripRandomDefinition As UndoRedo.RandomDefinitionChange = DirectCast(UndoRedo.fromBytes(randomDefinition.toBytes()), UndoRedo.RandomDefinitionChange)

        AssertEqual(UndoRedo.opRandomDefinitionChange, roundTripRandomDefinition.ofType(), "random definition command type")
        AssertEqual(3, roundTripRandomDefinition.Index, "random definition index")
        AssertEqual(8, roundTripRandomDefinition.Value, "random definition value")

        Dim legacyBytes As Byte() = LegacyMoveNoteBytes(New Note(4, 96.0R, 250000, 0, False, False, False), 8, 192.0R)
        Dim legacyMove As UndoRedo.MoveNote = DirectCast(UndoRedo.fromBytes(legacyBytes), UndoRedo.MoveNote)

        AssertEqual(UndoRedo.opMoveNote, legacyMove.ofType(), "legacy move command type")
        AssertEqual(-1, legacyMove.note.RandomIndex, "legacy note should default to common random index")
        AssertEqual(0, legacyMove.note.RandomValue, "legacy note should default to common random value")
        AssertEqual(8, legacyMove.NColumnIndex, "legacy move target column")
        AssertApprox(192.0R, legacyMove.NVPosition, "legacy move target position")
    End Sub

    Private Sub AssertTrue(ByVal condition As Boolean, ByVal message As String)
        If Not condition Then
            Throw New InvalidOperationException(message)
        End If
    End Sub

    Private Sub AssertFalse(ByVal condition As Boolean, ByVal message As String)
        If condition Then
            Throw New InvalidOperationException(message)
        End If
    End Sub

    Private Sub AssertContains(ByVal text As String, ByVal value As String, ByVal message As String)
        If text Is Nothing OrElse Not text.Contains(value) Then
            Throw New InvalidOperationException(message & ": expected to contain <" & value & ">")
        End If
    End Sub

    Private Sub AssertEqual(Of T)(ByVal expected As T, ByVal actual As T, ByVal message As String)
        If Not EqualityComparer(Of T).Default.Equals(expected, actual) Then
            Throw New InvalidOperationException(message & ": expected <" & expected.ToString() & "> but was <" & actual.ToString() & ">")
        End If
    End Sub

    Private Sub AssertApprox(ByVal expected As Double, ByVal actual As Double, ByVal message As String)
        If Math.Abs(expected - actual) > 0.0000001R Then
            Throw New InvalidOperationException(message & ": expected <" & expected.ToString() & "> but was <" & actual.ToString() & ">")
        End If
    End Sub

    Private Function Combine(ByVal first() As Byte, ByVal second() As Byte) As Byte()
        Dim result(first.Length + second.Length - 1) As Byte
        Array.Copy(first, 0, result, 0, first.Length)
        Array.Copy(second, 0, result, first.Length, second.Length)
        Return result
    End Function

    Private Function JoinLines(ByVal lines As IEnumerable(Of String)) As String
        Return String.Join(vbCrLf, New List(Of String)(lines).ToArray())
    End Function

    Private Function LegacyMoveNoteBytes(ByVal note As Note, ByVal columnIndex As Integer, ByVal vPosition As Double) As Byte()
        Dim ms As New MemoryStream()
        Dim bw As New BinaryWriter(ms)

        bw.Write(UndoRedo.opMoveNote)
        WriteLegacyNote(bw, note)
        bw.Write(columnIndex)
        bw.Write(vPosition)

        Return ms.ToArray()
    End Function

    Private Sub WriteLegacyNote(ByVal bw As BinaryWriter, ByVal note As Note)
        bw.Write(note.VPosition)
        bw.Write(note.ColumnIndex)
        bw.Write(note.Value)
        bw.Write(note.LongNote)
        bw.Write(note.Length)
        bw.Write(note.Hidden)
        bw.Write(note.Landmine)
    End Sub
End Module
