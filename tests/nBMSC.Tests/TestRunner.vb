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
        Run("theme gaps follow visible groups", AddressOf ThemeGapsFollowVisibleGroups)
        Run("theme gaps retain both border lines", AddressOf ThemeGapsRetainBothBorderLines)
        Run("up arrow follows variable chart bounds", AddressOf UpArrowFollowsVariableChartBounds)
        Run("player missing prompt decisions", AddressOf PlayerMissingPromptDecisions)
        Run("chart text encoding modes", AddressOf ChartTextEncodingModes)
        Run("chart text encoding detection", AddressOf ChartTextEncodingDetection)
        Run("version tag parsing", AddressOf VersionTagParsing)
        Run("undo redo serialization", AddressOf UndoRedoSerialization)
        Run("measure insertion copies following value", AddressOf MeasureInsertionBeforeSelection)
        Run("multiple measure removal", AddressOf MultipleMeasureRemoval)
        Run("measure removal undo redo", AddressOf MeasureRemovalUndoRedo)
        Run("context measure insertion undo redo", AddressOf ContextMeasureInsertionUndoRedo)
        Run("context measure removal undo redo", AddressOf ContextMeasureRemovalUndoRedo)
        Run("measure join availability", AddressOf MeasureJoinAvailability)
        Run("multiple measure join", AddressOf MultipleMeasureJoin)

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
            "#00102:0.5",
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
        AssertEqual("#00102:0.5", result.Blocks(0).Branches(0).Lines(0), "first branch measure length")
        AssertEqual("#00111:01", result.Blocks(0).Branches(0).Lines(1), "first branch data")
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

    Private Sub ThemeGapsFollowVisibleGroups()
        Using window As New MainWindow()
            For i As Integer = 0 To window.column.Length - 1
                window.column(i).isVisible = False
            Next

            SetPrivateField(window, "ThemeColumnGap", 5)
            SetPrivateField(window, "ThemePlayerGap", True)
            SetPrivateField(window, "ThemeColumnOrder", New Integer() {
                                MainWindow.niA1,
                                MainWindow.niA2,
                                MainWindow.niD1,
                                MainWindow.niBGA,
                                MainWindow.niB})

            window.column(MainWindow.niA1).Width = 40
            window.column(MainWindow.niA2).Width = 40
            window.column(MainWindow.niD1).Width = 40
            window.column(MainWindow.niBGA).Width = 40
            window.column(MainWindow.niB).Width = 40
            window.column(MainWindow.niA1).isVisible = True
            window.column(MainWindow.niA2).isVisible = True
            window.column(MainWindow.niBGA).isVisible = True
            window.column(MainWindow.niB).isVisible = True

            InvokePrivate(window, "UpdateColumnLefts", New Object() {})
            AssertEqual(0, window.column(MainWindow.niA1).Left, "first visible group should start without a gap")
            AssertEqual(40, window.column(MainWindow.niA2).Left, "columns in the same group should be adjacent")
            AssertEqual(85, window.column(MainWindow.niBGA).Left, "BGA group should follow one group gap")
            AssertEqual(130, window.column(MainWindow.niB).Left, "BGM group should follow one group gap")

            window.column(MainWindow.niBGA).isVisible = False
            InvokePrivate(window, "UpdateColumnLefts", New Object() {})
            AssertEqual(85, window.column(MainWindow.niB).Left, "hiding BGA should keep one gap between playable and BGM groups")

            window.column(MainWindow.niA2).isVisible = False
            window.column(MainWindow.niD1).isVisible = True
            SetPrivateField(window, "ThemePlayerGap", False)
            InvokePrivate(window, "UpdateColumnLefts", New Object() {})
            AssertEqual(40, window.column(MainWindow.niD1).Left, "disabled player gap should keep player groups adjacent")

            SetPrivateField(window, "ThemePlayerGap", True)
            InvokePrivate(window, "UpdateColumnLefts", New Object() {})
            AssertEqual(45, window.column(MainWindow.niD1).Left, "enabled player gap should separate player groups")
        End Using
    End Sub

    Private Sub ThemeGapsRetainBothBorderLines()
        Using window As New MainWindow()
            For i As Integer = 0 To window.column.Length - 1
                window.column(i).isVisible = False
            Next

            SetPrivateField(window, "ThemeColumnGap", 5)
            SetPrivateField(window, "ThemeColumnOrder", New Integer() {
                                MainWindow.niA1,
                                MainWindow.niBGA,
                                MainWindow.niB})
            SetPrivateField(window, "gxWidth", 1.0F)
            SetPrivateField(window, "gShowVerticalLine", True)
            SetPrivateField(window, "gShowGrid", False)
            SetPrivateField(window, "gShowSubGrid", False)
            SetPrivateField(window, "gShowMeasureBar", False)
            SetPrivateField(window, "gShowMeasureNumber", False)

            window.column(MainWindow.niA1).Width = 10
            window.column(MainWindow.niBGA).Width = 10
            window.column(MainWindow.niB).Width = 10
            window.column(MainWindow.niA1).isVisible = True
            window.column(MainWindow.niBGA).isVisible = True
            window.column(MainWindow.niB).isVisible = True
            InvokePrivate(window, "UpdateColumnLefts", New Object() {})
            AssertThemeGapBorders(window, New Integer() {10, 25}, "visible BGA")

            window.column(MainWindow.niBGA).isVisible = False
            InvokePrivate(window, "UpdateColumnLefts", New Object() {})
            AssertThemeGapBorders(window, New Integer() {10}, "hidden BGA")
        End Using
    End Sub

    Private Sub AssertThemeGapBorders(ByVal window As MainWindow,
                                      ByVal gapStarts() As Integer,
                                      ByVal state As String)
        Using bitmap As New System.Drawing.Bitmap(40, 10)
            Using graphics As System.Drawing.Graphics = System.Drawing.Graphics.FromImage(bitmap)
                Using buffer As System.Drawing.BufferedGraphics = System.Drawing.BufferedGraphicsManager.Current.Allocate(
                        graphics,
                        New System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height))
                    buffer.Graphics.Clear(System.Drawing.Color.Black)
                    InvokePrivate(window, "DrawPanelLines", New Object() {buffer, bitmap.Height, bitmap.Width, 0, 0, 1})
                    buffer.Render(graphics)
                End Using
            End Using

            Dim lineColor As Integer = PrivateField(Of visualSettings)(window, "vo").pVLine.Color.ToArgb()
            For Each gapStart As Integer In gapStarts
                AssertEqual(lineColor, bitmap.GetPixel(gapStart, 5).ToArgb(), state & " gap should retain its left border")
                AssertEqual(System.Drawing.Color.Black.ToArgb(), bitmap.GetPixel(gapStart + 1, 5).ToArgb(), state & " gap interior should remain black")
                AssertEqual(lineColor, bitmap.GetPixel(gapStart + 5, 5).ToArgb(), state & " gap should retain its right border")
            Next
        End Using
    End Sub

    Private Sub UpArrowFollowsVariableChartBounds()
        Using window As New MainWindow()
            Dim lengths(999) As Double
            For i As Integer = 0 To 999
                lengths(i) = 96.0R
            Next

            Dim notes(1) As Note
            notes(0) = PrivateField(Of Note())(window, "Notes")(0)
            notes(1) = New Note(MainWindow.niA1, 120.0R, 10000)
            notes(1).Selected = True
            window.column(MainWindow.niA1).isVisible = True
            SetPrivateField(window, "MeasureLength", lengths)
            SetPrivateField(window, "Notes", notes)
            SetPrivateField(window, "gDivide", 16)
            InvokePrivate(window, "UpdateMeasureBottom", New Object() {})

            Dim panel As System.Windows.Forms.Panel = DirectCast(window.Controls.Find("PMainIn", True)(0), System.Windows.Forms.Panel)
            InvokePrivate(window,
                          "PMainInPreviewKeyDown",
                          New Object() {panel, New System.Windows.Forms.PreviewKeyDownEventArgs(System.Windows.Forms.Keys.Up)})
            AssertApprox(132.0R, PrivateField(Of Note())(window, "Notes")(1).VPosition, "up arrow should move one grid step")

            InvokePrivate(window,
                          "PMainInPreviewKeyDown",
                          New Object() {panel, New System.Windows.Forms.PreviewKeyDownEventArgs(System.Windows.Forms.Keys.Down)})
            AssertApprox(120.0R, PrivateField(Of Note())(window, "Notes")(1).VPosition, "down arrow should return one grid step")
        End Using
    End Sub

    Private Sub PlayerMissingPromptDecisions()
        Using window As New MainWindow()
            AssertTrue(InvokePrivateFunction(Of Boolean)(window,
                                                          "ShouldOpenPlayerPathDialog",
                                                          New Object() {MsgBoxResult.Ok}),
                       "OK should open the player path dialog")
            AssertFalse(InvokePrivateFunction(Of Boolean)(window,
                                                           "ShouldOpenPlayerPathDialog",
                                                           New Object() {MsgBoxResult.Cancel}),
                        "closing or cancelling should stop before the player path dialog")
        End Using
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
        randomBlock.SetMeasureLength(3, 13, 96.0R)
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
        AssertApprox(96.0R, roundTripRandomInsert.Block.GetMeasureLengthOverrides(3)(13), "random insert measure length")
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

    Private Sub MeasureRemovalUndoRedo()
        Dim baseMeasureLengths(999) As Double
        For i As Integer = 0 To 999
            baseMeasureLengths(i) = 192.0R
        Next
        baseMeasureLengths(10) = 96.0R
        baseMeasureLengths(11) = 384.0R
        baseMeasureLengths(12) = 48.0R
        baseMeasureLengths(999) = 72.0R

        Dim block As New BmsRandomBlock(2)
        block.SetMeasureLength(1, 9, 100.0R)
        block.SetMeasureLength(1, 10, 110.0R)
        block.SetMeasureLength(1, 11, 120.0R)
        block.SetMeasureLength(1, 999, 130.0R)
        block.SetMeasureLength(2, 10, 140.0R)
        Dim blocks As New List(Of BmsRandomBlock) From {block}

        Dim notesBefore As Note() = {
            New Note(0, -1.0R, 1200000),
            New Note(3, 1920.0R, 10000, 0, False, True, False, 0, 1)
        }
        Dim undoState As New UndoRedo.MeasureStateChange(baseMeasureLengths, baseMeasureLengths, notesBefore, blocks)

        BmsMeasureStructure.RemoveMeasure(baseMeasureLengths, blocks, 10)

        AssertApprox(384.0R, baseMeasureLengths(10), "next base measure should shift into removed measure")
        AssertApprox(48.0R, baseMeasureLengths(11), "following base measure should shift")
        AssertApprox(72.0R, baseMeasureLengths(998), "last base measure should shift to 998")
        AssertApprox(72.0R, baseMeasureLengths(999), "last base measure should remain available")
        AssertApprox(100.0R, block.GetMeasureLengthOverrides(1)(9), "earlier random override should remain")
        AssertApprox(120.0R, block.GetMeasureLengthOverrides(1)(10), "next random override should shift into removed measure")
        AssertApprox(130.0R, block.GetMeasureLengthOverrides(1)(998), "last random override should shift to 998")
        AssertApprox(130.0R, block.GetMeasureLengthOverrides(1)(999), "last random override should remain available")
        AssertFalse(block.MeasureLengthByValue.ContainsKey(2), "removed-only random override should disappear")

        Dim notesAfter As Note() = {
            New Note(0, -1.0R, 1200000),
            New Note(3, 1824.0R, 10000, 0, False, True, False, 0, 1)
        }
        Dim redoState As New UndoRedo.MeasureStateChange(baseMeasureLengths, baseMeasureLengths, notesAfter, blocks)
        Dim roundTripUndo As UndoRedo.MeasureStateChange = DirectCast(UndoRedo.fromBytes(undoState.toBytes()), UndoRedo.MeasureStateChange)
        Dim roundTripRedo As UndoRedo.MeasureStateChange = DirectCast(UndoRedo.fromBytes(redoState.toBytes()), UndoRedo.MeasureStateChange)

        Dim restoredBase() As Double = roundTripUndo.CopyBaseMeasureLengths()
        roundTripUndo.RestoreRandomMeasureLengths(blocks)
        Dim restoredNotes() As Note = roundTripUndo.CopyNotes()
        AssertEqual(UndoRedo.opMeasureStateChange, roundTripUndo.ofType(), "measure undo command type")
        AssertApprox(96.0R, restoredBase(10), "undo should restore removed base measure length")
        AssertApprox(384.0R, restoredBase(11), "undo should restore following base measure length")
        AssertApprox(110.0R, block.GetMeasureLengthOverrides(1)(10), "undo should restore removed random override")
        AssertApprox(120.0R, block.GetMeasureLengthOverrides(1)(11), "undo should restore following random override")
        AssertApprox(140.0R, block.GetMeasureLengthOverrides(2)(10), "undo should restore an override-only branch")
        AssertApprox(1920.0R, restoredNotes(1).VPosition, "undo should restore note position")

        restoredBase = roundTripRedo.CopyBaseMeasureLengths()
        roundTripRedo.RestoreRandomMeasureLengths(blocks)
        restoredNotes = roundTripRedo.CopyNotes()
        AssertApprox(384.0R, restoredBase(10), "redo should restore removed base state")
        AssertApprox(120.0R, block.GetMeasureLengthOverrides(1)(10), "redo should restore shifted random override")
        AssertFalse(block.MeasureLengthByValue.ContainsKey(2), "redo should remove the override-only branch again")
        AssertApprox(1824.0R, restoredNotes(1).VPosition, "redo should restore shifted note position")
    End Sub

    Private Sub ContextMeasureInsertionUndoRedo()
        Using window As New MainWindow()
            Dim lengths() As Double = VariableMeasureLengths()
            Dim notesBefore() As Note = ContextMeasureNotes()
            Dim blocks As List(Of BmsRandomBlock) = ContextMeasureRandomBlocks()
            PrepareContextMeasureWindow(window, lengths, notesBefore, blocks, 4)

            InvokePrivate(window, "MInsert_Click", New Object() {Nothing, EventArgs.Empty})
            AssertContextInsertionState(window, blocks, "insert")

            InvokePrivate(window, "TBUndo_Click", New Object() {Nothing, EventArgs.Empty})
            AssertContextOriginalState(window, blocks, "insert undo")

            InvokePrivate(window, "TBRedo_Click", New Object() {Nothing, EventArgs.Empty})
            AssertContextInsertionState(window, blocks, "insert redo")
        End Using
    End Sub

    Private Sub ContextMeasureRemovalUndoRedo()
        Using window As New MainWindow()
            Dim lengths() As Double = VariableMeasureLengths()
            Dim notesBefore() As Note = ContextMeasureNotes()
            Dim blocks As List(Of BmsRandomBlock) = ContextMeasureRandomBlocks()
            PrepareContextMeasureWindow(window, lengths, notesBefore, blocks, 4)

            InvokePrivate(window, "MRemove_Click", New Object() {Nothing, EventArgs.Empty})
            AssertContextRemovalState(window, blocks, "remove")

            InvokePrivate(window, "TBUndo_Click", New Object() {Nothing, EventArgs.Empty})
            AssertContextOriginalState(window, blocks, "remove undo")

            InvokePrivate(window, "TBRedo_Click", New Object() {Nothing, EventArgs.Empty})
            AssertContextRemovalState(window, blocks, "remove redo")
        End Using
    End Sub

    Private Function VariableMeasureLengths() As Double()
        Dim lengths(999) As Double
        For i As Integer = 0 To 999
            lengths(i) = 192.0R
        Next
        lengths(4) = 204.0R
        lengths(5) = 207.0R
        lengths(6) = 183.0R
        Return lengths
    End Function

    Private Function ContextMeasureNotes() As Note()
        Return New Note() {
            New Note(0, -1.0R, 1200000),
            New Note(3, 700.0R, 10000, 341.0R),
            New Note(4, 800.0R, 20000),
            New Note(5, 819.0R, 30000, 222.0R),
            New Note(6, 1041.0R, 40000)
        }
    End Function

    Private Function ContextMeasureRandomBlocks() As List(Of BmsRandomBlock)
        Dim block As New BmsRandomBlock(1)
        block.SetMeasureLength(1, 4, 222.0R)
        block.SetMeasureLength(1, 5, 144.0R)
        Return New List(Of BmsRandomBlock) From {block}
    End Function

    Private Sub PrepareContextMeasureWindow(ByVal window As MainWindow,
                                            ByVal lengths() As Double,
                                            ByVal notes() As Note,
                                            ByVal blocks As List(Of BmsRandomBlock),
                                            ByVal measureIndex As Integer)
        notes(0) = PrivateField(Of Note())(window, "Notes")(0)
        SetPrivateField(window, "MeasureLength", CType(lengths.Clone(), Double()))
        SetPrivateField(window, "BaseMeasureLength", CType(lengths.Clone(), Double()))
        SetPrivateField(window, "Notes", CType(notes.Clone(), Note()))
        SetPrivateField(window, "RandomBlocks", blocks)
        SetPrivateField(window, "SelectedRandomIndex", -1)
        SetPrivateField(window, "NTInput", True)
        InvokePrivate(window, "UpdateMeasureBottom", New Object() {})

        Dim bottoms() As Double = PrivateField(Of Double())(window, "MeasureBottom")
        SetPrivateField(window, "menuVPosition", bottoms(measureIndex) + 1.0R)
    End Sub

    Private Sub AssertContextNotes(ByVal window As MainWindow,
                                   ByVal expectedPositions() As Double,
                                   ByVal expectedLengths() As Double,
                                   ByVal operation As String)
        Dim notes() As Note = PrivateField(Of Note())(window, "Notes")
        AssertEqual(expectedPositions.Length + 1, notes.Length, operation & " note count")
        AssertEqual(expectedPositions.Length, expectedLengths.Length, operation & " expected data")

        For i As Integer = 0 To expectedPositions.Length - 1
            AssertApprox(expectedPositions(i), notes(i + 1).VPosition, operation & " note " & i.ToString() & " position")
            AssertApprox(expectedLengths(i), notes(i + 1).Length, operation & " note " & i.ToString() & " length")
        Next
    End Sub

    Private Sub AssertContextOriginalState(ByVal window As MainWindow,
                                           ByVal blocks As List(Of BmsRandomBlock),
                                           ByVal operation As String)
        Dim baseLengths() As Double = PrivateField(Of Double())(window, "BaseMeasureLength")
        Dim displayLengths() As Double = PrivateField(Of Double())(window, "MeasureLength")
        AssertApprox(204.0R, baseLengths(4), operation & " should restore base measure 4")
        AssertApprox(207.0R, baseLengths(5), operation & " should restore base measure 5")
        AssertApprox(183.0R, baseLengths(6), operation & " should restore base measure 6")
        AssertApprox(204.0R, displayLengths(4), operation & " should restore display measure 4")
        AssertApprox(207.0R, displayLengths(5), operation & " should restore display measure 5")
        AssertContextNotes(window,
                           New Double() {700.0R, 800.0R, 819.0R, 1041.0R},
                           New Double() {341.0R, 0.0R, 222.0R, 0.0R},
                           operation)

        Dim measureOverrides As Dictionary(Of Integer, Double) = blocks(0).GetMeasureLengthOverrides(1)
        AssertApprox(222.0R, measureOverrides(4), operation & " should restore random measure 4")
        AssertApprox(144.0R, measureOverrides(5), operation & " should restore random measure 5")
        AssertEqual(2, measureOverrides.Count, operation & " should restore random override count")
    End Sub

    Private Sub AssertContextInsertionState(ByVal window As MainWindow,
                                            ByVal blocks As List(Of BmsRandomBlock),
                                            ByVal operation As String)
        Dim baseLengths() As Double = PrivateField(Of Double())(window, "BaseMeasureLength")
        Dim displayLengths() As Double = PrivateField(Of Double())(window, "MeasureLength")
        AssertApprox(204.0R, baseLengths(4), operation & " should copy base measure 4")
        AssertApprox(204.0R, baseLengths(5), operation & " should shift base measure 4")
        AssertApprox(207.0R, baseLengths(6), operation & " should shift base measure 5")
        AssertApprox(183.0R, baseLengths(7), operation & " should shift base measure 6")
        AssertApprox(204.0R, displayLengths(4), operation & " should copy display measure 4")
        AssertApprox(204.0R, displayLengths(5), operation & " should shift display measure 4")
        AssertContextNotes(window,
                           New Double() {700.0R, 1004.0R, 1023.0R, 1245.0R},
                           New Double() {545.0R, 0.0R, 222.0R, 0.0R},
                           operation)

        Dim measureOverrides As Dictionary(Of Integer, Double) = blocks(0).GetMeasureLengthOverrides(1)
        AssertApprox(222.0R, measureOverrides(4), operation & " should copy random measure 4")
        AssertApprox(222.0R, measureOverrides(5), operation & " should shift random measure 4")
        AssertApprox(144.0R, measureOverrides(6), operation & " should shift random measure 5")
        AssertEqual(3, measureOverrides.Count, operation & " should preserve random override count")
    End Sub

    Private Sub AssertContextRemovalState(ByVal window As MainWindow,
                                          ByVal blocks As List(Of BmsRandomBlock),
                                          ByVal operation As String)
        Dim baseLengths() As Double = PrivateField(Of Double())(window, "BaseMeasureLength")
        Dim displayLengths() As Double = PrivateField(Of Double())(window, "MeasureLength")
        AssertApprox(207.0R, baseLengths(4), operation & " should shift base measure 5")
        AssertApprox(183.0R, baseLengths(5), operation & " should shift base measure 6")
        AssertApprox(207.0R, displayLengths(4), operation & " should shift display measure 5")
        AssertContextNotes(window,
                           New Double() {700.0R, 768.0R, 837.0R},
                           New Double() {137.0R, 69.0R, 0.0R},
                           operation)

        Dim measureOverrides As Dictionary(Of Integer, Double) = blocks(0).GetMeasureLengthOverrides(1)
        AssertApprox(144.0R, measureOverrides(4), operation & " should shift random measure 5")
        AssertEqual(1, measureOverrides.Count, operation & " should remove the random override")
    End Sub

    Private Function PrivateField(Of T)(ByVal target As Object, ByVal name As String) As T
        Dim field As Reflection.FieldInfo = target.GetType().GetField(name, Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
        If field Is Nothing Then Throw New InvalidOperationException("Missing field: " & name)
        Return DirectCast(field.GetValue(target), T)
    End Function

    Private Sub SetPrivateField(ByVal target As Object, ByVal name As String, ByVal value As Object)
        Dim field As Reflection.FieldInfo = target.GetType().GetField(name, Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
        If field Is Nothing Then Throw New InvalidOperationException("Missing field: " & name)
        field.SetValue(target, value)
    End Sub

    Private Sub InvokePrivate(ByVal target As Object, ByVal name As String, ByVal arguments() As Object)
        Dim method As Reflection.MethodInfo = target.GetType().GetMethod(name, Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic)
        If method Is Nothing Then Throw New InvalidOperationException("Missing method: " & name)
        Try
            method.Invoke(target, arguments)
        Catch ex As Reflection.TargetInvocationException When ex.InnerException IsNot Nothing
            Throw New InvalidOperationException(name & " failed: " & ex.InnerException.ToString(), ex.InnerException)
        End Try
    End Sub

    Private Function InvokePrivateFunction(Of T)(ByVal target As Object,
                                                  ByVal name As String,
                                                  ByVal arguments() As Object) As T
        Dim method As Reflection.MethodInfo = target.GetType().GetMethod(
            name,
            Reflection.BindingFlags.Instance Or Reflection.BindingFlags.Static Or Reflection.BindingFlags.NonPublic)
        If method Is Nothing Then Throw New InvalidOperationException("Missing method: " & name)
        Try
            Return DirectCast(method.Invoke(target, arguments), T)
        Catch ex As Reflection.TargetInvocationException When ex.InnerException IsNot Nothing
            Throw New InvalidOperationException(name & " failed: " & ex.InnerException.ToString(), ex.InnerException)
        End Try
    End Function

    Private Sub MeasureInsertionBeforeSelection()
        Dim baseMeasureLengths(999) As Double
        For i As Integer = 0 To 999
            baseMeasureLengths(i) = 192.0R + i
        Next

        Dim block As New BmsRandomBlock(1)
        block.SetMeasureLength(1, 1, 101.0R)
        block.SetMeasureLength(1, 2, 102.0R)
        block.SetMeasureLength(1, 3, 103.0R)
        block.SetMeasureLength(1, 4, 104.0R)
        block.SetMeasureLength(1, 5, 105.0R)
        block.SetMeasureLength(1, 6, 106.0R)
        block.SetMeasureLength(1, 999, 199.0R)
        Dim blocks As New List(Of BmsRandomBlock) From {block}

        BmsMeasureStructure.InsertMeasuresBeforeSelection(baseMeasureLengths, blocks, New Integer() {2, 3, 5})

        AssertApprox(194.0R, baseMeasureLengths(2), "first inserted measure should copy the following value")
        AssertApprox(194.0R, baseMeasureLengths(3), "all measures inserted for a contiguous selection should copy its first value")
        AssertApprox(194.0R, baseMeasureLengths(4), "first selected measure should follow its inserted measures")
        AssertApprox(195.0R, baseMeasureLengths(5), "second selected measure should remain contiguous")
        AssertApprox(196.0R, baseMeasureLengths(6), "unselected measure between groups should remain after the first group")
        AssertApprox(197.0R, baseMeasureLengths(7), "separate selection group should copy its own following value")
        AssertApprox(197.0R, baseMeasureLengths(8), "separate selected measure should follow its inserted measure")
        AssertApprox(1188.0R, baseMeasureLengths(999), "overflowing tail measures should be discarded")

        Dim measureOverrides As Dictionary(Of Integer, Double) = block.GetMeasureLengthOverrides(1)
        AssertApprox(101.0R, measureOverrides(1), "earlier random override should remain")
        AssertApprox(102.0R, measureOverrides(2), "inserted measure should copy the following random override")
        AssertApprox(102.0R, measureOverrides(3), "contiguous inserted measures should copy the same random override")
        AssertApprox(102.0R, measureOverrides(4), "first selected random override should shift after inserted group")
        AssertApprox(103.0R, measureOverrides(5), "contiguous selected random override should remain contiguous")
        AssertApprox(104.0R, measureOverrides(6), "intermediate random override should remain between groups")
        AssertApprox(105.0R, measureOverrides(7), "separate inserted measure should copy its following random override")
        AssertApprox(105.0R, measureOverrides(8), "separate selected random override should shift")
        AssertApprox(106.0R, measureOverrides(9), "following random override should shift")
        AssertFalse(measureOverrides.ContainsKey(999), "overflowing random override should be discarded")
    End Sub

    Private Sub MultipleMeasureRemoval()
        Dim baseMeasureLengths(999) As Double
        For i As Integer = 0 To 999
            baseMeasureLengths(i) = 192.0R + i
        Next

        Dim block As New BmsRandomBlock(1)
        block.SetMeasureLength(1, 2, 102.0R)
        block.SetMeasureLength(1, 3, 103.0R)
        block.SetMeasureLength(1, 4, 104.0R)
        block.SetMeasureLength(1, 5, 105.0R)
        block.SetMeasureLength(1, 6, 106.0R)
        block.SetMeasureLength(1, 999, 199.0R)
        Dim blocks As New List(Of BmsRandomBlock) From {block}

        BmsMeasureStructure.RemoveSelectedMeasures(baseMeasureLengths, blocks, New Integer() {2, 3, 5})

        AssertApprox(196.0R, baseMeasureLengths(2), "first unselected following measure should shift to the first removed row")
        AssertApprox(198.0R, baseMeasureLengths(3), "measure following the separate removed row should shift")
        AssertApprox(1191.0R, baseMeasureLengths(996), "last original measure should shift by the removal count")
        AssertApprox(1191.0R, baseMeasureLengths(999), "last measure should remain available after multiple removal")

        Dim measureOverrides As Dictionary(Of Integer, Double) = block.GetMeasureLengthOverrides(1)
        AssertApprox(104.0R, measureOverrides(2), "unselected random override should shift into the first removed row")
        AssertApprox(106.0R, measureOverrides(3), "random override following all removed rows should shift")
        AssertApprox(199.0R, measureOverrides(996), "last random override should shift by the removal count")
        AssertApprox(199.0R, measureOverrides(999), "last random override should remain available")
        AssertEqual(6, measureOverrides.Count, "removed random overrides should not remain")
    End Sub

    Private Sub MeasureJoinAvailability()
        AssertFalse(BmsMeasureStructure.CanJoinSelectedMeasures(New Integer() {}, 999), "empty selection should not be joinable")
        AssertFalse(BmsMeasureStructure.CanJoinSelectedMeasures(New Integer() {2}, 999), "single selection should not be joinable")
        AssertFalse(BmsMeasureStructure.CanJoinSelectedMeasures(New Integer() {2, 4}, 999), "separate selections should not be joinable")
        AssertTrue(BmsMeasureStructure.CanJoinSelectedMeasures(New Integer() {2, 3}, 999), "adjacent selections should be joinable")
        AssertTrue(BmsMeasureStructure.CanJoinSelectedMeasures(New Integer() {2, 4, 5}, 999), "one adjacent group should enable joining")
    End Sub

    Private Sub MultipleMeasureJoin()
        Dim baseMeasureLengths(999) As Double
        For i As Integer = 0 To 999
            baseMeasureLengths(i) = 192.0R + i
        Next

        Dim block As New BmsRandomBlock(2)
        block.SetMeasureLength(1, 2, 102.0R)
        block.SetMeasureLength(1, 3, 103.0R)
        block.SetMeasureLength(1, 4, 104.0R)
        block.SetMeasureLength(1, 5, 105.0R)
        block.SetMeasureLength(1, 7, 107.0R)
        block.SetMeasureLength(1, 9, 109.0R)
        block.SetMeasureLength(1, 10, 110.0R)
        block.SetMeasureLength(1, 999, 199.0R)
        block.SetMeasureLength(2, 2, 190.0R)
        block.SetMeasureLength(2, 3, 199.0R)
        Dim blocks As New List(Of BmsRandomBlock) From {block}

        BmsMeasureStructure.JoinSelectedMeasures(baseMeasureLengths, blocks, New Integer() {2, 3, 5, 7, 8, 9})

        AssertApprox(389.0R, baseMeasureLengths(2), "first adjacent group should become one summed measure")
        AssertApprox(196.0R, baseMeasureLengths(3), "measure following the first group should be compacted")
        AssertApprox(197.0R, baseMeasureLengths(4), "separate single selection should remain")
        AssertApprox(198.0R, baseMeasureLengths(5), "unselected measure between groups should remain")
        AssertApprox(600.0R, baseMeasureLengths(6), "second adjacent group should become one summed measure")
        AssertApprox(202.0R, baseMeasureLengths(7), "measure following all joined groups should be compacted")
        AssertApprox(1191.0R, baseMeasureLengths(996), "tail measures should be compacted by the joined row count")
        AssertApprox(1191.0R, baseMeasureLengths(999), "the final measure should remain available after joining")

        Dim measureOverrides As Dictionary(Of Integer, Double) = block.GetMeasureLengthOverrides(1)
        AssertApprox(205.0R, measureOverrides(2), "random lengths in the first group should be summed")
        AssertApprox(104.0R, measureOverrides(3), "following random override should shift after the first group")
        AssertApprox(105.0R, measureOverrides(4), "single selected random override should remain")
        AssertApprox(416.0R, measureOverrides(6), "missing random overrides should use base lengths while summing")
        AssertApprox(110.0R, measureOverrides(7), "random override following all groups should shift")
        AssertApprox(199.0R, measureOverrides(996), "last random override should shift by the joined row count")
        AssertApprox(199.0R, measureOverrides(999), "last random override should remain available")
        AssertEqual(9, measureOverrides.Count, "only joined and shifted random overrides should remain")

        Dim equivalentOverrides As Dictionary(Of Integer, Double) = block.GetMeasureLengthOverrides(2)
        AssertApprox(389.0R, equivalentOverrides(2), "an explicit joined override should remain explicit")
        AssertEqual(1, equivalentOverrides.Count, "joining should not add unrelated random overrides")
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
