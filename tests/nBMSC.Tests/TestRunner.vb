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
    Private Declare Function GetKeyboardState Lib "user32.dll" (ByVal keyState() As Byte) As Integer
    Private Declare Function SetKeyboardState Lib "user32.dll" (ByVal keyState() As Byte) As Integer
    Private Const VirtualKeyControl As Integer = &H11

    <STAThread()>
    Public Function Main() As Integer
        Run("definition label conversion", AddressOf DefinitionLabelConversion)
        Run("channel label conversion", AddressOf ChannelLabelConversion)
        Run("label validation", AddressOf LabelValidation)
        Run("BMS definition labels", AddressOf BmsDefinitionLabels)
        Run("BMS base62 header loading", AddressOf BmsBase62HeaderLoading)
        Run("BMS base62 save header", AddressOf BmsBase62SaveHeader)
        Run("BMS base36 save header", AddressOf BmsBase36SaveHeader)
        Run("BMS base62 round trip", AddressOf BmsBase62RoundTrip)
        Run("base62 option toggle", AddressOf Base62OptionToggle)
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
        Run("Alt arrows translate selection between measures", AddressOf AltArrowsTranslateSelectionBetweenMeasures)
        Run("Alt arrows respect chart bounds", AddressOf AltArrowsRespectChartBounds)
        Run("Alt Shift arrows translate long note ends", AddressOf AltShiftArrowsTranslateLongNoteEnds)
        Run("arrow modifier compatibility", AddressOf ArrowModifierCompatibility)
        Run("Ctrl Alt arrows duplicate notes", AddressOf CtrlAltArrowsDuplicateNotes)
        Run("Ctrl Alt Shift arrows duplicate across measures", AddressOf CtrlAltShiftArrowsDuplicateAcrossMeasures)
        Run("Ctrl Alt arrows clamp chart bounds", AddressOf CtrlAltArrowsClampChartBounds)
        Run("Ctrl Alt duplicate follows vertical scroll", AddressOf CtrlAltDuplicateFollowsVerticalScroll)
        Run("player missing prompt decisions", AddressOf PlayerMissingPromptDecisions)
        Run("text input dialog selects existing value", AddressOf TextInputDialogSelectsExistingValue)
        Run("selected note input defaults", AddressOf SelectedNoteInputDefaults)
        Run("paste alignment clipboard semantics", AddressOf PasteAlignmentClipboardSemantics)
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
        RunBeatChangeModeTests()

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
        AssertTrue(nBMSC.Editor.BmsDefinitionLabels.ContainsBase62Definitions(New String() {"#BASE 62"}), "base62 header should enable base62")
        AssertTrue(nBMSC.Editor.BmsDefinitionLabels.ContainsBase62Definitions(New String() {"#base" & vbTab & "62"}), "base62 header should ignore case and accept whitespace")
        AssertFalse(nBMSC.Editor.BmsDefinitionLabels.ContainsBase62Definitions(New String() {"#BASE62"}), "base62 header should require whitespace")
        AssertFalse(nBMSC.Editor.BmsDefinitionLabels.ContainsBase62Definitions(New String() {"#BASE 620"}), "base62 header should require the exact value")
    End Sub

    Private Sub BmsBase62HeaderLoading()
        Dim emptyChart As String = String.Join(vbCrLf, New String() {
            "#TITLE Empty base62 chart",
            "#BASE 62"})

        Using window As New MainWindow()
            InvokePrivate(window, "OpenBMS", New Object() {emptyChart, "", Nothing})
            AssertTrue(PrivateField(Of Boolean)(window, "UseBase62Definitions"),
                       "explicit base62 header should enable base62 for an empty chart")
            AssertEqual("",
                        DirectCast(window.Controls.Find("TExpansion", True)(0), System.Windows.Forms.TextBox).Text,
                        "explicit base62 header should not enter the expansion field for an empty chart")
        End Using

        Dim uppercaseChart As String = String.Join(vbCrLf, New String() {
            "#TITLE Uppercase base62 chart",
            "#WAV0A kick.wav",
            "#BASE 62",
            "#00111:0A"})

        Using window As New MainWindow()
            InvokePrivate(window, "OpenBMS", New Object() {uppercaseChart, "", Nothing})
            AssertTrue(PrivateField(Of Boolean)(window, "UseBase62Definitions"),
                       "explicit base62 header should enable base62 for uppercase-only definitions")
            AssertEqual("",
                        DirectCast(window.Controls.Find("TExpansion", True)(0), System.Windows.Forms.TextBox).Text,
                        "explicit base62 header should not enter the expansion field when it follows definitions")
            AssertEqual("kick.wav",
                        PrivateField(Of String())(window, "hWAV")(10),
                        "uppercase definition should still load at its base36-compatible index")
            AssertEqual(100000,
                        PrivateField(Of Note())(window, "Notes")(1).Value,
                        "uppercase note definition should still load at its base36-compatible index")
        End Using
    End Sub

    Private Sub BmsBase62SaveHeader()
        Dim saved As String = SaveBmsForTest(Base62ChartWithLateHeader())
        Dim baseIndex As Integer = ExactBmsLineIndex(saved, "#BASE 62")
        Dim wavIndex As Integer = ExactBmsLineIndex(saved, "#WAV0a kick.wav")

        AssertEqual(1, CountBmsLines(saved, "#BASE 62"),
                    "base62 save should contain exactly one base declaration")
        AssertTrue(baseIndex >= 0, "base62 save should contain the base declaration")
        AssertContains(saved, "#RANK 3" & vbCrLf & "#BASE 62" & vbCrLf & vbCrLf,
                       "base62 declaration should immediately follow RANK before the header section break")
        AssertTrue(wavIndex >= 0, "base62 save should contain the indexed definition")
        AssertTrue(baseIndex < wavIndex,
                   "base62 declaration should be in the header before indexed definitions")
        AssertContains(saved, "#FOO keep-me", "base62 save should preserve unrelated expansion text")
    End Sub

    Private Sub BmsBase36SaveHeader()
        Dim chart As String = String.Join(vbCrLf, New String() {
            "#TITLE Base36 chart",
            "#WAV0A kick.wav",
            "#00111:0A"})
        Dim saved As String = SaveBmsForTest(chart)

        AssertEqual(0, CountBmsLines(saved, "#BASE 62"),
                    "base36 save should not contain a base declaration")
        AssertTrue(ExactBmsLineIndex(saved, "#WAV0A kick.wav") >= 0,
                   "base36 save should preserve uppercase definition labels")
    End Sub

    Private Sub BmsBase62RoundTrip()
        Dim saved As String = SaveBmsForTest(Base62ChartWithLateHeader())

        Using window As New MainWindow()
            InvokePrivate(window, "OpenBMS", New Object() {saved, "", Nothing})
            AssertTrue(PrivateField(Of Boolean)(window, "UseBase62Definitions"),
                       "saved base62 chart should reopen with base62 enabled")
            Dim expansion As String = DirectCast(window.Controls.Find("TExpansion", True)(0), System.Windows.Forms.TextBox).Text
            AssertFalse(expansion.IndexOf("#BASE 62", StringComparison.OrdinalIgnoreCase) >= 0,
                        "saved base62 header should not return to the expansion field")
            AssertContains(expansion, "#FOO keep-me", "saved base62 chart should retain unrelated expansion text")
            AssertEqual("kick.wav",
                        PrivateField(Of String())(window, "hWAV")(36),
                        "saved base62 definition should round-trip at its base62 index")
            AssertEqual(360000,
                        PrivateField(Of Note())(window, "Notes")(1).Value,
                        "saved base62 note should round-trip at its base62 index")
        End Using
    End Sub

    Private Sub Base62OptionToggle()
        Using window As New MainWindow()
            Dim chart As String = String.Join(vbCrLf, New String() {
                "#TITLE Base36 chart",
                "#WAV0A kick.wav",
                "#00111:0A"})
            InvokePrivate(window, "OpenBMS", New Object() {chart, "", Nothing})

            DirectCast(window.Controls.Find("CWAVBase62", True)(0), System.Windows.Forms.CheckBox).Checked = True

            AssertTrue(PrivateField(Of Boolean)(window, "UseBase62Definitions"),
                       "extend to base62 option should enable base62")
            AssertTrue(DirectCast(window.Controls.Find("CWAVBase62", True)(0), System.Windows.Forms.CheckBox).Checked,
                       "extend to base62 option should enable the WAV base62 control")
            AssertTrue(DirectCast(window.Controls.Find("CBMPBase62", True)(0), System.Windows.Forms.CheckBox).Checked,
                       "extend to base62 option should enable the BMP base62 control")

            Dim saved As String = InvokePrivateFunction(Of String)(window, "SaveBMS", New Object() {})
            AssertEqual(1, CountBmsLines(saved, "#BASE 62"),
                        "enabling extend to base62 should emit one base declaration")

            DirectCast(window.Controls.Find("CWAVBase62", True)(0), System.Windows.Forms.CheckBox).Checked = False
            AssertFalse(PrivateField(Of Boolean)(window, "UseBase62Definitions"),
                        "disabling extend to base62 should disable base62")

            saved = InvokePrivateFunction(Of String)(window, "SaveBMS", New Object() {})
            AssertEqual(0, CountBmsLines(saved, "#BASE 62"),
                        "disabling extend to base62 should omit the base declaration")
        End Using
    End Sub

    Private Function Base62ChartWithLateHeader() As String
        Return String.Join(vbCrLf, New String() {
            "#TITLE Base62 chart",
            "#WAV0a kick.wav",
            "#BASE 62",
            "#FOO keep-me",
            "#00111:0a"})
    End Function

    Private Function SaveBmsForTest(ByVal chart As String) As String
        Using window As New MainWindow()
            InvokePrivate(window, "OpenBMS", New Object() {chart, "", Nothing})
            Return InvokePrivateFunction(Of String)(window, "SaveBMS", New Object() {})
        End Using
    End Function

    Private Function ExactBmsLineIndex(ByVal text As String, ByVal expected As String) As Integer
        Dim normalized As String = text.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
        Dim lines() As String = normalized.Split(New Char() {ChrW(10)}, StringSplitOptions.None)
        For i As Integer = 0 To lines.Length - 1
            If String.Equals(lines(i).Trim(), expected, StringComparison.Ordinal) Then Return i
        Next

        Return -1
    End Function

    Private Function CountBmsLines(ByVal text As String, ByVal expected As String) As Integer
        Dim normalized As String = text.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
        Dim lines() As String = normalized.Split(New Char() {ChrW(10)}, StringSplitOptions.None)
        Dim count As Integer = 0
        For Each line As String In lines
            If String.Equals(line.Trim(), expected, StringComparison.Ordinal) Then count += 1
        Next

        Return count
    End Function

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

    Private Function CreateArrowTestWindow(ByVal positions() As Double, ByVal noteLengths() As Double) As MainWindow
        Dim window As New MainWindow()
        Dim lengths(999) As Double
        For i As Integer = 0 To 999
            lengths(i) = 192.0R
        Next
        lengths(1) = 96.0R
        lengths(2) = 384.0R
        Dim notes(positions.Length) As Note
        notes(0) = PrivateField(Of Note())(window, "Notes")(0)
        For i As Integer = 0 To positions.Length - 1
            notes(i + 1) = New Note(MainWindow.niA1 + i, positions(i), (i + 1) * 10000)
            notes(i + 1).Length = noteLengths(i)
            notes(i + 1).LongNote = noteLengths(i) > 0
            notes(i + 1).Selected = True
            window.column(MainWindow.niA1 + i).isVisible = True
        Next
        SetPrivateField(window, "MeasureLength", lengths)
        SetPrivateField(window, "Notes", notes)
        SetPrivateField(window, "gDivide", 16)
        SetPrivateField(window, "NTInput", True)
        InvokePrivate(window, "UpdateMeasureBottom", New Object() {})
        Return window
    End Function

    Private Sub PressArrow(ByVal window As MainWindow, ByVal keyData As System.Windows.Forms.Keys)
        Dim panel As System.Windows.Forms.Panel = DirectCast(window.Controls.Find("PMainIn", True)(0), System.Windows.Forms.Panel)
        InvokePrivate(window, "PMainInPreviewKeyDown",
                      New Object() {panel, New System.Windows.Forms.PreviewKeyDownEventArgs(keyData)})
    End Sub

    Private Sub AltArrowsTranslateSelectionBetweenMeasures()
        Using window As MainWindow = CreateArrowTestWindow(New Double() {144, 240}, New Double() {24, 96})
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Up)
            AssertContextNotes(window, New Double() {264, 360}, New Double() {24, 96}, "three-quarter anchor stays in the adjacent short measure; all notes share its displacement")
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Up)
            AssertContextNotes(window, New Double() {576, 672}, New Double() {24, 96}, "anchor stays at three quarters in the longer measure")
            InvokePrivate(window, "TBUndo_Click", New Object() {Nothing, EventArgs.Empty})
            AssertContextNotes(window, New Double() {144, 240}, New Double() {24, 96}, "one undo restores the key repeat")
            InvokePrivate(window, "TBRedo_Click", New Object() {Nothing, EventArgs.Empty})
            AssertContextNotes(window, New Double() {576, 672}, New Double() {24, 96}, "redo restores the whole movement")
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Down)
            AssertContextNotes(window, New Double() {264, 360}, New Double() {24, 96}, "Down returns to the adjacent short measure")
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Down)
            AssertContextNotes(window, New Double() {144, 240}, New Double() {24, 96}, "Up and Down are reversible across changing measure lengths")
        End Using
        Using window As MainWindow = CreateArrowTestWindow(New Double() {0, 144}, New Double() {0, 24})
            PrivateField(Of Note())(window, "Notes")(1).Selected = False
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Up)
            AssertContextNotes(window, New Double() {0, 264}, New Double() {0, 24}, "unselected notes neither anchor nor move")
        End Using
    End Sub

    Private Sub AltArrowsRespectChartBounds()
        Using window As MainWindow = CreateArrowTestWindow(New Double() {24, 120}, New Double() {48, 96})
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Down)
            AssertContextNotes(window, New Double() {0, 96}, New Double() {48, 96}, "chart start clamps the whole selection")
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Down)
            AssertContextNotes(window, New Double() {0, 96}, New Double() {48, 96}, "chart start cannot be crossed")
        End Using
        Using window As MainWindow = CreateArrowTestWindow(New Double() {192000, 192024}, New Double() {12, 48})
            'The variable measure map ends at 192096; the last legal endpoint is 192095.
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Up)
            AssertContextNotes(window, New Double() {192023, 192047}, New Double() {12, 48}, "chart end clamps all starts while preserving long lengths")
        End Using
        Using window As MainWindow = CreateArrowTestWindow(New Double() {288}, New Double() {0})
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Down)
            AssertContextNotes(window, New Double() {192}, New Double() {0}, "Down on a measure line goes to the preceding measure line")
        End Using
    End Sub

    Private Sub AltShiftArrowsTranslateLongNoteEnds()
        Using window As MainWindow = CreateArrowTestWindow(New Double() {24, 120}, New Double() {360, 312})
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Shift Or System.Windows.Forms.Keys.Up)
            AssertContextNotes(window, New Double() {24, 120}, New Double() {696, 648}, "earliest end moves from quarter position to quarter position; starts and end spacing stay fixed")
            InvokePrivate(window, "TBUndo_Click", New Object() {Nothing, EventArgs.Empty})
            AssertContextNotes(window, New Double() {24, 120}, New Double() {360, 312}, "undo restores both ends")
            InvokePrivate(window, "TBRedo_Click", New Object() {Nothing, EventArgs.Empty})
            AssertContextNotes(window, New Double() {24, 120}, New Double() {696, 648}, "redo restores both ends")
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Shift Or System.Windows.Forms.Keys.Down)
            AssertContextNotes(window, New Double() {24, 120}, New Double() {360, 312}, "Down returns the ends to their original positions")
        End Using
        Using window As MainWindow = CreateArrowTestWindow(New Double() {216, 312}, New Double() {12, 48})
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Shift Or System.Windows.Forms.Keys.Down)
            AssertContextNotes(window, New Double() {216, 312}, New Double() {0, 36}, "shortest long note limits the shared contraction")
            AssertFalse(PrivateField(Of Note())(window, "Notes")(1).LongNote, "zero length becomes a normal note")
        End Using
        Using window As MainWindow = CreateArrowTestWindow(New Double() {192000, 192024}, New Double() {12, 48})
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Shift Or System.Windows.Forms.Keys.Up)
            AssertContextNotes(window, New Double() {192000, 192024}, New Double() {35, 71}, "chart end limits shared extension")
        End Using
        Using window As MainWindow = CreateArrowTestWindow(New Double() {216, 312}, New Double() {0, 48})
            PrivateField(Of Note())(window, "Notes")(1).ColumnIndex = MainWindow.niBPM
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Shift Or System.Windows.Forms.Keys.Up)
            AssertContextNotes(window, New Double() {216, 312}, New Double() {0, 396}, "numeric events do not anchor or resize")
        End Using
        Using window As MainWindow = CreateArrowTestWindow(New Double() {240}, New Double() {0})
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Shift Or System.Windows.Forms.Keys.Up)
            AssertContextNotes(window, New Double() {240}, New Double() {240}, "a normal note can extend to the next measure's midpoint")
            AssertTrue(PrivateField(Of Note())(window, "Notes")(1).LongNote, "extension creates a long note")
        End Using
    End Sub

    Private Sub ArrowModifierCompatibility()
        Using window As MainWindow = CreateArrowTestWindow(New Double() {216}, New Double() {24})
            PressArrow(window, System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.Up)
            AssertContextNotes(window, New Double() {217}, New Double() {24}, "Ctrl remains one unit")
            PressArrow(window, System.Windows.Forms.Keys.Shift Or System.Windows.Forms.Keys.Up)
            AssertContextNotes(window, New Double() {217}, New Double() {36}, "Shift remains one grid unit")
            PressArrow(window, System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.Shift Or System.Windows.Forms.Keys.Down)
            AssertContextNotes(window, New Double() {217}, New Double() {35}, "Ctrl Shift remains one unit")
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.Up)
            AssertEqual(3, PrivateField(Of Note())(window, "Notes").Length,
                        "Ctrl Alt should add one grid-step copy")
            AssertDuplicateNote(window, MainWindow.niA1, 217.0R, 35.0R, 10000, False,
                                False, False, -1, 0, "Ctrl Alt should keep the original note")
            AssertDuplicateNote(window, MainWindow.niA1, 229.0R, 35.0R, 10000, True,
                                False, False, -1, 0, "Ctrl Alt should select the copied note")
            SetPrivateField(window, "NTInput", False)
            PressArrow(window, System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Shift Or System.Windows.Forms.Keys.Up)
            AssertDuplicateNote(window, MainWindow.niA1, 217.0R, 35.0R, 10000, False,
                                False, False, -1, 0, "Shift still does nothing outside NT input")
            AssertDuplicateNote(window, MainWindow.niA1, 229.0R, 35.0R, 10000, True,
                                False, False, -1, 0, "Shift should not change the copied note outside NT input")
        End Using

        Using window As MainWindow = CreateArrowTestWindow(New Double() {264}, New Double() {0})
            SetPrivateField(window, "NTInput", False)
            PressArrow(window, System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Shift Or System.Windows.Forms.Keys.Up)
            AssertEqual(3, PrivateField(Of Note())(window, "Notes").Length,
                        "Ctrl Alt Shift should duplicate normal notes outside NT input")
            AssertDuplicateNote(window, MainWindow.niA1, 264.0R, 0.0R, 10000, False,
                                False, False, -1, 0, "non-NT measure copy should retain the original")
            AssertDuplicateNote(window, MainWindow.niA1, 576.0R, 0.0R, 10000, True,
                                False, False, -1, 0, "non-NT measure copy should keep the relative measure position")
        End Using
    End Sub

    Private Sub CtrlAltArrowsDuplicateNotes()
        Using window As MainWindow = CreateArrowTestWindow(New Double() {120, 216, 500}, New Double() {24, 0, 12})
            Dim notes() As Note = PrivateField(Of Note())(window, "Notes")
            notes(1).Hidden = True
            notes(1).RandomIndex = 7
            notes(1).RandomValue = 4
            notes(2).Landmine = True
            notes(2).RandomIndex = 8
            notes(2).RandomValue = 5
            notes(3).Selected = False

            PressArrow(window, System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Up)
            AssertEqual(6, PrivateField(Of Note())(window, "Notes").Length,
                        "one grid copy should add one copy for each selected note")
            AssertDuplicateNote(window, MainWindow.niA1, 120.0R, 24.0R, 10000, False,
                                True, False, 7, 4, "grid Up should retain the original long note")
            AssertDuplicateNote(window, MainWindow.niA1, 132.0R, 24.0R, 10000, True,
                                True, False, 7, 4, "grid Up should copy long note attributes")
            AssertDuplicateNote(window, MainWindow.niA2, 216.0R, 0.0R, 20000, False,
                                False, True, 8, 5, "grid Up should retain the original normal note")
            AssertDuplicateNote(window, MainWindow.niA2, 228.0R, 0.0R, 20000, True,
                                False, True, 8, 5, "grid Up should copy normal note attributes")
            AssertDuplicateNote(window, MainWindow.niA3, 500.0R, 12.0R, 30000, False,
                                False, False, -1, 0, "unselected notes should remain untouched")
            AssertEqual(2, CountSelectedNotes(window), "only the first copies should be selected")

            PressArrow(window, System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Up)
            AssertEqual(8, PrivateField(Of Note())(window, "Notes").Length,
                        "repeating grid Up should copy the currently selected notes")
            AssertDuplicateNote(window, MainWindow.niA1, 132.0R, 24.0R, 10000, False,
                                True, False, 7, 4, "repeat should deselect the previous copy")
            AssertDuplicateNote(window, MainWindow.niA1, 144.0R, 24.0R, 10000, True,
                                True, False, 7, 4, "repeat should select only the newest copy")
            AssertDuplicateNote(window, MainWindow.niA2, 228.0R, 0.0R, 20000, False,
                                False, True, 8, 5, "repeat should retain the first normal copy")
            AssertDuplicateNote(window, MainWindow.niA2, 240.0R, 0.0R, 20000, True,
                                False, True, 8, 5, "repeat should select the newest normal copy")
            AssertEqual(2, CountSelectedNotes(window), "a repeated copy should replace the selection")

            InvokePrivate(window, "TBUndo_Click", New Object() {Nothing, EventArgs.Empty})
            AssertEqual(6, PrivateField(Of Note())(window, "Notes").Length,
                        "undo should remove the most recent repeated copies")
            AssertNoNoteAt(window, MainWindow.niA1, 144.0R, "undo should remove the newest long-note copy")
            AssertNoNoteAt(window, MainWindow.niA2, 240.0R, "undo should remove the newest normal-note copy")

            InvokePrivate(window, "TBRedo_Click", New Object() {Nothing, EventArgs.Empty})
            AssertEqual(8, PrivateField(Of Note())(window, "Notes").Length,
                        "redo should restore the most recent repeated copies")
            AssertDuplicateNote(window, MainWindow.niA1, 144.0R, 24.0R, 10000, True,
                                True, False, 7, 4, "redo should restore the selected long-note copy")
            AssertDuplicateNote(window, MainWindow.niA2, 240.0R, 0.0R, 20000, True,
                                False, True, 8, 5, "redo should restore the selected normal-note copy")
        End Using

        Using window As MainWindow = CreateArrowTestWindow(New Double() {120, 216, 500}, New Double() {24, 0, 12})
            PrivateField(Of Note())(window, "Notes")(3).Selected = False
            PressArrow(window, System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Down)
            AssertEqual(6, PrivateField(Of Note())(window, "Notes").Length,
                        "grid Down should add one copy for each selected note")
            AssertDuplicateNote(window, MainWindow.niA1, 120.0R, 24.0R, 10000, False,
                                False, False, -1, 0, "grid Down should retain the original")
            AssertDuplicateNote(window, MainWindow.niA1, 108.0R, 24.0R, 10000, True,
                                False, False, -1, 0, "grid Down should place the copy one grid unit below")
            AssertDuplicateNote(window, MainWindow.niA2, 216.0R, 0.0R, 20000, False,
                                False, False, -1, 0, "grid Down should retain the second original")
            AssertDuplicateNote(window, MainWindow.niA2, 204.0R, 0.0R, 20000, True,
                                False, False, -1, 0, "grid Down should place the second copy one grid unit below")
        End Using
    End Sub

    Private Sub CtrlAltShiftArrowsDuplicateAcrossMeasures()
        Using window As MainWindow = CreateArrowTestWindow(New Double() {144, 240}, New Double() {24, 96})
            PressArrow(window, System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Shift Or System.Windows.Forms.Keys.Up)
            AssertEqual(5, PrivateField(Of Note())(window, "Notes").Length,
                        "measure copy should add one copy for each selected note")
            AssertDuplicateNote(window, MainWindow.niA1, 144.0R, 24.0R, 10000, False,
                                False, False, -1, 0, "measure Up should retain the first original")
            AssertDuplicateNote(window, MainWindow.niA1, 264.0R, 24.0R, 10000, True,
                                False, False, -1, 0, "measure Up should keep the relative position in the next measure")
            AssertDuplicateNote(window, MainWindow.niA2, 240.0R, 96.0R, 20000, False,
                                False, False, -1, 0, "measure Up should retain the second original")
            AssertDuplicateNote(window, MainWindow.niA2, 360.0R, 96.0R, 20000, True,
                                False, False, -1, 0, "measure Up should preserve spacing across variable measures")

            PressArrow(window, System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Shift Or System.Windows.Forms.Keys.Up)
            AssertEqual(7, PrivateField(Of Note())(window, "Notes").Length,
                        "repeating measure Up should use the new selected copies")
            AssertDuplicateNote(window, MainWindow.niA1, 576.0R, 24.0R, 10000, True,
                                False, False, -1, 0, "repeat should use the next variable measure's relative position")
            AssertDuplicateNote(window, MainWindow.niA2, 672.0R, 96.0R, 20000, True,
                                False, False, -1, 0, "repeat should preserve the selected spacing")
        End Using

        Using window As MainWindow = CreateArrowTestWindow(New Double() {264, 360}, New Double() {24, 96})
            PressArrow(window, System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Shift Or System.Windows.Forms.Keys.Down)
            AssertEqual(5, PrivateField(Of Note())(window, "Notes").Length,
                        "measure Down should add one copy for each selected note")
            AssertDuplicateNote(window, MainWindow.niA1, 264.0R, 24.0R, 10000, False,
                                False, False, -1, 0, "measure Down should retain the first original")
            AssertDuplicateNote(window, MainWindow.niA1, 144.0R, 24.0R, 10000, True,
                                False, False, -1, 0, "measure Down should keep the relative position in the previous measure")
            AssertDuplicateNote(window, MainWindow.niA2, 360.0R, 96.0R, 20000, False,
                                False, False, -1, 0, "measure Down should retain the second original")
            AssertDuplicateNote(window, MainWindow.niA2, 240.0R, 96.0R, 20000, True,
                                False, False, -1, 0, "measure Down should preserve spacing across variable measures")
        End Using
    End Sub

    Private Sub CtrlAltArrowsClampChartBounds()
        Using window As MainWindow = CreateArrowTestWindow(New Double() {192070, 192080}, New Double() {20, 10})
            PressArrow(window, System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Up)
            AssertEqual(5, PrivateField(Of Note())(window, "Notes").Length,
                        "upper-bound copy should still be created when a common clamp is available")
            AssertDuplicateNote(window, MainWindow.niA1, 192070.0R, 20.0R, 10000, False,
                                False, False, -1, 0, "upper-bound clamp should retain the first original")
            AssertDuplicateNote(window, MainWindow.niA1, 192075.0R, 20.0R, 10000, True,
                                False, False, -1, 0, "upper-bound clamp should preserve the first note length")
            AssertDuplicateNote(window, MainWindow.niA2, 192080.0R, 10.0R, 20000, False,
                                False, False, -1, 0, "upper-bound clamp should retain the second original")
            AssertDuplicateNote(window, MainWindow.niA2, 192085.0R, 10.0R, 20000, True,
                                False, False, -1, 0, "upper-bound clamp should preserve the second note length")
        End Using

        Using window As MainWindow = CreateArrowTestWindow(New Double() {6, 30}, New Double() {24, 12})
            PressArrow(window, System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Down)
            AssertEqual(5, PrivateField(Of Note())(window, "Notes").Length,
                        "lower-bound copy should still be created when a common clamp is available")
            AssertDuplicateNote(window, MainWindow.niA1, 6.0R, 24.0R, 10000, False,
                                False, False, -1, 0, "lower-bound clamp should retain the first original")
            AssertDuplicateNote(window, MainWindow.niA1, 0.0R, 24.0R, 10000, True,
                                False, False, -1, 0, "lower-bound clamp should preserve the first note length")
            AssertDuplicateNote(window, MainWindow.niA2, 30.0R, 12.0R, 20000, False,
                                False, False, -1, 0, "lower-bound clamp should retain the second original")
            AssertDuplicateNote(window, MainWindow.niA2, 24.0R, 12.0R, 20000, True,
                                False, False, -1, 0, "lower-bound clamp should preserve the second note length")
        End Using

        Using window As MainWindow = CreateArrowTestWindow(New Double() {0, 24}, New Double() {0, 0})
            PressArrow(window, System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Down)
            AssertEqual(3, PrivateField(Of Note())(window, "Notes").Length,
                        "a zero clamped delta should not create any copy")
            AssertEqual(2, CountSelectedNotes(window), "a zero clamped delta should leave the source selection intact")
        End Using
    End Sub

    Private Sub CtrlAltDuplicateFollowsVerticalScroll()
        Dim previousKeyboardState(255) As Byte
        If GetKeyboardState(previousKeyboardState) = 0 Then
            Throw New InvalidOperationException("GetKeyboardState failed")
        End If

        Try
            Dim controlKeyboardState() As Byte = CType(previousKeyboardState.Clone(), Byte())
            controlKeyboardState(VirtualKeyControl) = &H80
            If SetKeyboardState(controlKeyboardState) = 0 Then
                Throw New InvalidOperationException("SetKeyboardState failed")
            End If
            AssertTrue(My.Computer.Keyboard.CtrlKeyDown,
                       "the test must set the real Ctrl keyboard state before invoking the event")

            Using window As MainWindow = CreateArrowTestWindow(New Double() {630}, New Double() {0})
                InvokePrivate(window, "CalculateGreatestVPosition", New Object() {})
                Dim panel As System.Windows.Forms.Panel = DirectCast(window.Controls.Find("PMainIn", True)(0), System.Windows.Forms.Panel)
                Dim beforePanelScroll As Integer = PrivateField(Of Integer())(window, "PanelVScroll")(0)
                Dim beforeScrollBar As Integer = MainPanelScrollValue(window)
                AssertEqual(0, beforePanelScroll, "the follow test should start at the top of the chart")
                AssertEqual(beforePanelScroll, beforeScrollBar, "panel and scrollbar should start synchronized")

                PressArrow(window, System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.Up)
                AssertEqual(3, PrivateField(Of Note())(window, "Notes").Length,
                            "the follow test should create one copy")
                AssertDuplicateNote(window, MainWindow.niA1, 642.0R, 0.0R, 10000, True,
                                    False, False, -1, 0, "the copied note should remain selected")

                Dim afterPanelScroll As Integer = PrivateField(Of Integer())(window, "PanelVScroll")(0)
                Dim afterScrollBar As Integer = MainPanelScrollValue(window)
                AssertTrue(afterPanelScroll < beforePanelScroll,
                           "a copied note beyond the visible area should move the panel scroll position")
                AssertEqual(afterPanelScroll, afterScrollBar,
                            "the panel scroll field and scrollbar should remain synchronized after following")

                InvokePrivate(window, "PMainInMouseWheel",
                              New Object() {panel,
                                            New System.Windows.Forms.MouseEventArgs(System.Windows.Forms.MouseButtons.None,
                                                                                    0, 0, 0, 120)})
                AssertEqual(afterPanelScroll, PrivateField(Of Integer())(window, "PanelVScroll")(0),
                            "Ctrl wheel should not change the panel scroll position")
                AssertEqual(afterScrollBar, MainPanelScrollValue(window),
                            "Ctrl wheel should not change the scrollbar value")
            End Using
        Finally
            SetKeyboardState(previousKeyboardState)
        End Try
    End Sub

    Private Function MainPanelScrollValue(ByVal window As MainWindow) As Integer
        Dim scrollBar As Object = InvokePrivateFunction(Of Object)(window, "GetPanelVScrollBar", New Object() {0})
        Dim valueProperty As Reflection.PropertyInfo = scrollBar.GetType().GetProperty("Value")
        If valueProperty Is Nothing Then Throw New InvalidOperationException("Missing scrollbar Value property")
        Return CInt(valueProperty.GetValue(scrollBar, Nothing))
    End Function

    Private Function CountSelectedNotes(ByVal window As MainWindow) As Integer
        Dim count As Integer = 0
        Dim notes() As Note = PrivateField(Of Note())(window, "Notes")
        For i As Integer = 1 To UBound(notes)
            If notes(i).Selected Then count += 1
        Next

        Return count
    End Function

    Private Sub AssertDuplicateNote(ByVal window As MainWindow,
                                    ByVal columnIndex As Integer,
                                    ByVal expectedPosition As Double,
                                    ByVal expectedLength As Double,
                                    ByVal expectedValue As Long,
                                    ByVal expectedSelected As Boolean,
                                    ByVal expectedHidden As Boolean,
                                    ByVal expectedLandmine As Boolean,
                                    ByVal expectedRandomIndex As Integer,
                                    ByVal expectedRandomValue As Integer,
                                    ByVal operation As String)
        Dim notes() As Note = PrivateField(Of Note())(window, "Notes")
        For i As Integer = 1 To UBound(notes)
            If notes(i).ColumnIndex <> columnIndex Then Continue For
            If Math.Abs(notes(i).VPosition - expectedPosition) > 0.0000001R Then Continue For

            AssertApprox(expectedLength, notes(i).Length, operation & " length")
            AssertEqual(expectedLength > 0.0R, notes(i).LongNote, operation & " long-note flag")
            AssertEqual(expectedValue, notes(i).Value, operation & " value")
            AssertEqual(expectedSelected, notes(i).Selected, operation & " selection")
            AssertEqual(expectedHidden, notes(i).Hidden, operation & " hidden flag")
            AssertEqual(expectedLandmine, notes(i).Landmine, operation & " landmine flag")
            AssertEqual(expectedRandomIndex, notes(i).RandomIndex, operation & " random index")
            AssertEqual(expectedRandomValue, notes(i).RandomValue, operation & " random value")
            Return
        Next

        Throw New InvalidOperationException(operation & ": missing note at column " & columnIndex.ToString() &
                                             ", position " & expectedPosition.ToString())
    End Sub

    Private Sub AssertNoNoteAt(ByVal window As MainWindow,
                               ByVal columnIndex As Integer,
                               ByVal position As Double,
                               ByVal operation As String)
        Dim notes() As Note = PrivateField(Of Note())(window, "Notes")
        For i As Integer = 1 To UBound(notes)
            If notes(i).ColumnIndex = columnIndex AndAlso Math.Abs(notes(i).VPosition - position) <= 0.0000001R Then
                Throw New InvalidOperationException(operation & ": unexpected note at position " & position.ToString())
            End If
        Next
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

    Private Sub TextInputDialogSelectsExistingValue()
        Using dialog As New TextInputDialog("Enter a value.", "Edit value", "123.45")
            Dim input As Windows.Forms.TextBox = DirectCast(dialog.Controls.Find("InputTextBox", True)(0), Windows.Forms.TextBox)
            Dim prompt As Windows.Forms.Label = DirectCast(dialog.Controls.Find("PromptLabel", True)(0), Windows.Forms.Label)
            Dim okButton As Windows.Forms.Button = DirectCast(dialog.Controls.Find("OKInputButton", True)(0), Windows.Forms.Button)
            Dim cancelButton As Windows.Forms.Button = DirectCast(dialog.Controls.Find("CancelInputButton", True)(0), Windows.Forms.Button)
            AssertEqual("Edit value", dialog.Text, "input dialog title")
            AssertEqual("123.45", input.Text, "input dialog existing value")

            Dim legacyType As Type = GetType(Microsoft.VisualBasic.Interaction).Assembly.GetType("Microsoft.VisualBasic.CompilerServices.VBInputBox", True)
            Using legacy As Windows.Forms.Form = DirectCast(Activator.CreateInstance(
                    legacyType,
                    Reflection.BindingFlags.Instance Or Reflection.BindingFlags.NonPublic,
                    Nothing,
                    New Object() {"Enter a value.", "Edit value", "123.45", -1, -1},
                    Globalization.CultureInfo.InvariantCulture), Windows.Forms.Form)
                AssertEqual(legacy.ClientSize, dialog.ClientSize, "legacy input dialog client size")
                AssertEqual(legacy.Controls.Find("Label", True)(0).Bounds, prompt.Bounds, "legacy input dialog prompt bounds")
                AssertEqual(legacy.Controls.Find("TextBox", True)(0).Bounds, input.Bounds, "legacy input dialog text bounds")
                AssertEqual(legacy.Controls.Find("OKButton", True)(0).Bounds, okButton.Bounds, "legacy input dialog OK bounds")
                AssertEqual(legacy.Controls.Find("MyCancelButton", True)(0).Bounds, cancelButton.Bounds, "legacy input dialog cancel bounds")
            End Using

            InvokePrivate(dialog, "SelectInputText", New Object() {})
            AssertEqual(0, input.SelectionStart, "input dialog selection start")
            AssertEqual(input.Text.Length, input.SelectionLength, "input dialog selection length")
        End Using
    End Sub

    Private Sub SelectedNoteInputDefaults()
        Using window As New MainWindow()
            Dim notes() As Note = {
                PrivateField(Of Note())(window, "Notes")(0),
                New Note(MainWindow.niBPM, 192.0R, 1285000, 0.0R, False, True),
                New Note(MainWindow.niSTOP, 384.0R, 1285000, 0.0R, False, True),
                New Note(MainWindow.niA1, 576.0R, 100000, 0.0R, False, True),
                New Note(MainWindow.niA1, 768.0R, 100000, 0.0R, False, True)
            }
            For i As Integer = 1 To UBound(notes)
                notes(i).Selected = True
            Next
            SetPrivateField(window, "Notes", notes)

            AssertEqual("128.5",
                        InvokePrivateFunction(Of String)(window, "SelectedNotePromptDefault", New Object() {True}),
                        "matching numeric note values")
            AssertEqual("0A",
                        InvokePrivateFunction(Of String)(window, "SelectedNotePromptDefault", New Object() {False}),
                        "matching definition labels")

            notes(2).Value = 640000
            notes(4).Value = 110000
            SetPrivateField(window, "Notes", notes)
            AssertEqual("",
                        InvokePrivateFunction(Of String)(window, "SelectedNotePromptDefault", New Object() {True}),
                        "mixed numeric note values")
            AssertEqual("",
                        InvokePrivateFunction(Of String)(window, "SelectedNotePromptDefault", New Object() {False}),
                        "mixed definition labels")
        End Using
    End Sub

    Private Sub PasteAlignmentClipboardSemantics()
        Dim original As System.Windows.Forms.IDataObject = System.Windows.Forms.Clipboard.GetDataObject()
        Dim saved As New System.Windows.Forms.DataObject()
        If original IsNot Nothing Then
            For Each format As String In original.GetFormats(False)
                saved.SetData(format, False, original.GetData(format, False))
            Next
        End If

        Try
            'Source measure length, first offset, target length, expected first offset.
            Dim cases As Double(,) = {{160, 120, 96, 56}, {192, 96, 96, 0},
                                     {192, 60, 48, 12}, {192, 40, 40, 0},
                                     {192, 40, 20, 0}, {192, 40, 48, 40},
                                     {192, 96, 48, 0}}
            For Each ntInput As Boolean In New Boolean() {False, True}
                For row As Integer = 0 To cases.GetLength(0) - 1
                    Dim operation As String = "paste case " & row.ToString() & " NT=" & ntInput.ToString()
                    Dim sourceLength As Double = cases(row, 0)
                    Dim firstOffset As Double = cases(row, 1)
                    Dim noteLength As Double = If(ntInput, 12.0R, 0.0R)
                    Dim lengths(999) As Double
                    For i As Integer = 0 To UBound(lengths)
                        lengths(i) = 192.0R
                    Next
                    lengths(1) = sourceLength
                    Using source As New MainWindow()
                        SetPrivateField(source, "MeasureLength", lengths)
                        SetPrivateField(source, "NTInput", ntInput)
                        SetPrivateField(source, "gSub", 4)
                        SetPrivateField(source, "Notes", New Note() {
                            New Note(MainWindow.niBPM, -1.0R, 1200000),
                            New Note(MainWindow.niA1, 192.0R + firstOffset, 10000, noteLength) With {.Selected = True},
                            New Note(MainWindow.niA2, 192.0R + sourceLength + 60.0R, 20000) With {.Selected = True}
                        })
                        InvokePrivate(source, "UpdateMeasureBottom", New Object() {})
                        InvokePrivate(source, "CopyNotes", New Object() {True})

                        'Pasting must use the copied measure/grid, even into another window.
                        lengths(1) = 64.0R
                        SetPrivateField(source, "gSub", 8)
                        InvokePrivate(source, "UpdateMeasureBottom", New Object() {})
                    End Using
                    Using target As New MainWindow()
                        Dim targetLengths(999) As Double
                        For i As Integer = 0 To UBound(targetLengths)
                            targetLengths(i) = 192.0R
                        Next
                        targetLengths(1) = cases(row, 2)
                        SetPrivateField(target, "MeasureLength", targetLengths)
                        SetPrivateField(target, "NTInput", ntInput)
                        SetPrivateField(target, "gSub", 2)
                        SetPrivateField(target, "Notes", New Note() {New Note(MainWindow.niBPM, -1.0R, 1200000)})
                        InvokePrivate(target, "UpdateMeasureBottom", New Object() {})
                        InvokePrivate(target, "AddNotesFromClipboard", New Object() {True, True, 192.0R})

                        Dim notes() As Note = PrivateField(Of Note())(target, "Notes")
                        AssertEqual(3, notes.Length, operation & " count")
                        AssertApprox(192.0R + cases(row, 3), notes(1).VPosition, operation & " first position")
                        AssertApprox(sourceLength + 60.0R - firstOffset,
                                     notes(2).VPosition - notes(1).VPosition, operation & " spacing across measures")
                        AssertApprox(noteLength, notes(1).Length, operation & " long note length")
                        AssertEqual(MainWindow.niA1, notes(1).ColumnIndex, operation & " first column")
                        AssertEqual(MainWindow.niA2, notes(2).ColumnIndex, operation & " second column")
                        AssertTrue(notes(1).Selected AndAlso notes(2).Selected, operation & " selection")
                    End Using
                Next
            Next
        Finally
            If original Is Nothing Then
                System.Windows.Forms.Clipboard.Clear()
            Else
                System.Windows.Forms.Clipboard.SetDataObject(saved, True)
            End If
        End Try
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

    Private Sub RunBeatChangeModeTests()
        For Each ntInput As Boolean In New Boolean() {True, False}
            For Each targetRatio As Double In New Double() {1.5R, 0.5R}
                For Each useRandomLayer As Boolean In New Boolean() {False, True}
                    For operationMode As Integer = 0 To 3
                        Dim currentNtInput As Boolean = ntInput
                        Dim currentTargetRatio As Double = targetRatio
                        Dim currentUseRandomLayer As Boolean = useRandomLayer
                        Dim currentOperationMode As Integer = operationMode
                        Dim inputName As String = If(currentNtInput, "NT", "BMSE")
                        Dim lengthName As String = If(currentTargetRatio > 1.0R, "expand", "shrink")
                        Dim layerName As String = If(currentUseRandomLayer, "random", "common")
                        Dim testName As String = "beat mode " & currentOperationMode.ToString() & " " & inputName & " " & lengthName & " " & layerName & " undo redo mode matrix"

                        Run(testName,
                            Sub()
                                BeatChangeModeUndoRedoCase(currentNtInput,
                                                           currentTargetRatio,
                                                           currentUseRandomLayer,
                                                           currentOperationMode)
                            End Sub)
                    Next
                Next
            Next
        Next
    End Sub

    Private Sub BeatChangeModeUndoRedoCase(ByVal ntInput As Boolean,
                                           ByVal targetRatio As Double,
                                           ByVal useRandomLayer As Boolean,
                                           ByVal operationMode As Integer)
        Using window As New MainWindow()
            PrepareBeatChangeModeWindow(window, ntInput, useRandomLayer)
            SetPrivateField(window, "BeatChangeMode", operationMode)

            Dim before As UndoRedo.MeasureStateChange = CaptureBeatState(window)
            InvokePrivate(window, "ApplyBeat", New Object() {targetRatio, targetRatio.ToString()})
            Dim after As UndoRedo.MeasureStateChange = CaptureBeatState(window)
            AssertDoubleBits(192.0R * targetRatio,
                             after.CopyBaseMeasureLengths()(2),
                             "beat operation should change the selected base measure")

            For undoMode As Integer = 0 To 3
                For redoMode As Integer = 0 To 3
                    SetPrivateField(window, "BeatChangeMode", undoMode)
                    InvokePrivate(window, "TBUndo_Click", New Object() {Nothing, EventArgs.Empty})
                    AssertBeatState(before,
                                    window,
                                    "operation mode " & operationMode.ToString() &
                                    ", undo mode " & undoMode.ToString())

                    SetPrivateField(window, "BeatChangeMode", redoMode)
                    InvokePrivate(window, "TBRedo_Click", New Object() {Nothing, EventArgs.Empty})
                    AssertBeatState(after,
                                    window,
                                    "operation mode " & operationMode.ToString() &
                                    ", undo mode " & undoMode.ToString() &
                                    ", redo mode " & redoMode.ToString())
                Next
            Next

            If useRandomLayer Then
                SetPrivateField(window, "BeatChangeMode", (operationMode + 1) Mod 4)
                InvokePrivate(window, "TBUndo_Click", New Object() {Nothing, EventArgs.Empty})
                SetBeatChangeRandomValue(window, 2)
                Dim beforeSecondRandomValue As UndoRedo.MeasureStateChange = CaptureBeatState(window)

                SetBeatChangeRandomValue(window, 1)
                InvokePrivate(window, "TBRedo_Click", New Object() {Nothing, EventArgs.Empty})
                SetBeatChangeRandomValue(window, 2)
                Dim afterSecondRandomValue As UndoRedo.MeasureStateChange = CaptureBeatState(window)

                SetPrivateField(window, "BeatChangeMode", (operationMode + 2) Mod 4)
                InvokePrivate(window, "TBUndo_Click", New Object() {Nothing, EventArgs.Empty})
                AssertBeatState(beforeSecondRandomValue,
                                window,
                                "operation mode " & operationMode.ToString() & " undo after random value change")

                SetPrivateField(window, "BeatChangeMode", (operationMode + 3) Mod 4)
                InvokePrivate(window, "TBRedo_Click", New Object() {Nothing, EventArgs.Empty})
                AssertBeatState(afterSecondRandomValue,
                                window,
                                "operation mode " & operationMode.ToString() & " redo after random value change")
            End If
        End Using
    End Sub

    Private Sub SetBeatChangeRandomValue(ByVal window As MainWindow, ByVal value As Integer)
        Dim blocks As List(Of BmsRandomBlock) = PrivateField(Of List(Of BmsRandomBlock))(window, "RandomBlocks")
        blocks(0).CurrentValue = value
        blocks(0).Normalize()
        InvokePrivate(window, "ApplySelectedRandomMeasureMap", New Object() {})
    End Sub

    Private Sub PrepareBeatChangeModeWindow(ByVal window As MainWindow,
                                            ByVal ntInput As Boolean,
                                            ByVal useRandomLayer As Boolean)
        Dim baseLengths(999) As Double
        For i As Integer = 0 To 999
            baseLengths(i) = 192.0R
        Next

        Dim displayLengths() As Double = CType(baseLengths.Clone(), Double())
        Dim blocks As New List(Of BmsRandomBlock)()
        If useRandomLayer Then
            Dim block As New BmsRandomBlock(2)
            block.CurrentValue = 1
            block.SetMeasureLength(1, 1, 168.0R)
            block.SetMeasureLength(1, 2, 216.0R)
            block.SetMeasureLength(2, 2, 120.0R)
            blocks.Add(block)
            displayLengths(1) = 168.0R
            displayLengths(2) = 216.0R
        End If

        Dim notes() As Note = BeatChangeModeNotes(window, ntInput, useRandomLayer)
        SetPrivateField(window, "BaseMeasureLength", baseLengths)
        SetPrivateField(window, "MeasureLength", displayLengths)
        SetPrivateField(window, "Notes", notes)
        SetPrivateField(window, "RandomBlocks", blocks)
        SetPrivateField(window, "SelectedRandomIndex", If(useRandomLayer, 0, -1))
        SetPrivateField(window, "ActiveMeasureRandomIndex", If(useRandomLayer, 0, -1))
        SetPrivateField(window, "ActiveMeasureRandomValue", If(useRandomLayer, 1, 0))
        SetPrivateField(window, "NTInput", ntInput)
        InvokePrivate(window, "UpdateMeasureBottom", New Object() {})

        Dim beatList As Windows.Forms.ListBox = DirectCast(window.Controls.Find("LBeat", True)(0), Windows.Forms.ListBox)
        beatList.Items.Clear()
        For i As Integer = 0 To 999
            beatList.Items.Add(i.ToString())
        Next
        beatList.ClearSelected()
        beatList.SelectedIndices.Add(2)
        InvokePrivate(window, "ClearUndo", New Object() {})
    End Sub

    Private Function BeatChangeModeNotes(ByVal window As MainWindow,
                                         ByVal ntInput As Boolean,
                                         ByVal useRandomLayer As Boolean) As Note()
        Dim randomIndex As Integer = If(useRandomLayer, 0, -1)
        Dim firstRandomValue As Integer = If(useRandomLayer, 1, 0)
        Dim secondRandomValue As Integer = If(useRandomLayer, 2, 0)
        Dim notes() As Note = {
            PrivateField(Of Note())(window, "Notes")(0),
            New Note(3, 350.0R, 10000, If(ntInput, 260.0R, 0.0R), False, False, False, randomIndex, firstRandomValue),
            New Note(4, 360.0R, 20000, If(ntInput, 48.0R, 0.0R), False, False, False, randomIndex, secondRandomValue),
            New Note(5, 400.0R, 30000, If(ntInput, 20.0R, 0.0R), False, False, False, randomIndex, firstRandomValue),
            New Note(6, 450.0R, 40000, If(ntInput, 12.0R, 0.0R), False, False, False, randomIndex, secondRandomValue),
            New Note(7, 500.0R, 50000, If(ntInput, 20.0R, 0.0R), False, False, False, randomIndex, firstRandomValue),
            New Note(8, 510.0R, 60000, If(ntInput, 100.0R, 0.0R), False, False, False, randomIndex, secondRandomValue),
            New Note(9, 540.0R, 70000, If(ntInput, 72.0R, 0.0R), False, False, False, randomIndex, firstRandomValue),
            New Note(10, 600.0R, 80000, 0.0R, False, False, False, randomIndex, secondRandomValue),
            New Note(11, 700.0R, 90000, If(ntInput, 24.0R, 0.0R), False, False, False, randomIndex, firstRandomValue),
            New Note(12, 900.0R, 100000, If(ntInput, 36.0R, 0.0R), False, False, False, randomIndex, secondRandomValue),
            New Note(13, 1200.0R, 110000, If(ntInput, 48.0R, 0.0R), False, False, False, randomIndex, firstRandomValue)
        }
        Return notes
    End Function

    Private Function CaptureBeatState(ByVal window As MainWindow) As UndoRedo.MeasureStateChange
        Return New UndoRedo.MeasureStateChange(
            PrivateField(Of Double())(window, "BaseMeasureLength"),
            PrivateField(Of Double())(window, "MeasureLength"),
            PrivateField(Of Note())(window, "Notes"),
            PrivateField(Of List(Of BmsRandomBlock))(window, "RandomBlocks"))
    End Function

    Private Sub AssertBeatState(ByVal expected As UndoRedo.MeasureStateChange,
                                ByVal window As MainWindow,
                                ByVal operation As String)
        Dim actual As UndoRedo.MeasureStateChange = CaptureBeatState(window)
        AssertDoubleArrayBits(expected.CopyBaseMeasureLengths(),
                              actual.CopyBaseMeasureLengths(),
                              operation & " base measure lengths")
        AssertDoubleArrayBits(expected.CopyDisplayMeasureLengths(),
                              actual.CopyDisplayMeasureLengths(),
                              operation & " display measure lengths")
        AssertNoteArrays(expected.CopyNotes(), actual.CopyNotes(), operation & " notes")
        AssertRandomMeasureLengths(expected.RandomMeasureLengths,
                                   actual.RandomMeasureLengths,
                                   operation & " random measure lengths")
    End Sub

    Private Sub AssertDoubleArrayBits(ByVal expected() As Double,
                                      ByVal actual() As Double,
                                      ByVal message As String)
        AssertEqual(expected.Length, actual.Length, message & " count")
        For i As Integer = 0 To expected.Length - 1
            AssertDoubleBits(expected(i), actual(i), message & " at " & i.ToString())
        Next
    End Sub

    Private Sub AssertNoteArrays(ByVal expected() As Note,
                                 ByVal actual() As Note,
                                 ByVal message As String)
        AssertEqual(expected.Length, actual.Length, message & " count")
        For i As Integer = 0 To expected.Length - 1
            AssertDoubleBits(expected(i).VPosition, actual(i).VPosition, message & " position " & i.ToString())
            AssertDoubleBits(expected(i).Length, actual(i).Length, message & " length " & i.ToString())
            AssertEqual(expected(i).ColumnIndex, actual(i).ColumnIndex, message & " column " & i.ToString())
            AssertEqual(expected(i).Value, actual(i).Value, message & " value " & i.ToString())
            AssertEqual(expected(i).LongNote, actual(i).LongNote, message & " long flag " & i.ToString())
            AssertEqual(expected(i).Hidden, actual(i).Hidden, message & " hidden flag " & i.ToString())
            AssertEqual(expected(i).Landmine, actual(i).Landmine, message & " landmine flag " & i.ToString())
            AssertEqual(expected(i).RandomIndex, actual(i).RandomIndex, message & " random index " & i.ToString())
            AssertEqual(expected(i).RandomValue, actual(i).RandomValue, message & " random value " & i.ToString())
        Next
    End Sub

    Private Sub AssertRandomMeasureLengths(ByVal expected() As UndoRedo.MeasureStateChange.RandomMeasureLength,
                                           ByVal actual() As UndoRedo.MeasureStateChange.RandomMeasureLength,
                                           ByVal message As String)
        AssertEqual(expected.Length, actual.Length, message & " count")
        For Each expectedItem As UndoRedo.MeasureStateChange.RandomMeasureLength In expected
            Dim found As Boolean = False
            For Each actualItem As UndoRedo.MeasureStateChange.RandomMeasureLength In actual
                If actualItem.RandomIndex = expectedItem.RandomIndex AndAlso
                   actualItem.RandomValue = expectedItem.RandomValue AndAlso
                   actualItem.MeasureIndex = expectedItem.MeasureIndex Then
                    AssertDoubleBits(expectedItem.Length, actualItem.Length, message & " value")
                    found = True
                    Exit For
                End If
            Next
            AssertTrue(found, message & " missing entry")
        Next
    End Sub

    Private Sub AssertDoubleBits(ByVal expected As Double,
                                 ByVal actual As Double,
                                 ByVal message As String)
        If BitConverter.DoubleToInt64Bits(expected) <> BitConverter.DoubleToInt64Bits(actual) Then
            Throw New InvalidOperationException(message & ": expected " & expected.ToString("R") & ", actual " & actual.ToString("R"))
        End If
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
