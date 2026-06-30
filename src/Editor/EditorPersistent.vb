Imports nBMSC.Editor.Functions

Imports nBMSC.Editor

Partial Public Class MainWindow

    Private Sub XMLWriteColumn(ByVal w As XmlTextWriter, ByVal I As Integer)
        w.WriteStartElement("Column")
        w.WriteAttributeString("Index", I)
        With column(I)
            'w.WriteAttributeString("Left", .Left)
            w.WriteAttributeString("Width", .Width)
            w.WriteAttributeString("Title", .Title)
            w.WriteAttributeString("Display", .isVisible)
            'w.WriteAttributeString("Text", .Text)
            'w.WriteAttributeString("Enabled", .Enabled)
            'w.WriteAttributeString("isNumeric", .isNumeric)
            'w.WriteAttributeString("Visible", .Visible)
            'w.WriteAttributeString("Identifier", .Identifier)
            w.WriteAttributeString("NoteColor", .cNote)
            w.WriteAttributeString("TextColor", .cText.ToArgb)
            w.WriteAttributeString("LongNoteColor", .cLNote)
            w.WriteAttributeString("LongTextColor", .cLText.ToArgb)
            w.WriteAttributeString("BG", .cBG.ToArgb)
        End With
        w.WriteEndElement()
    End Sub

    Private Function ThemeColorText(ByVal color As Color) As String
        Return String.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.R, color.G, color.B, color.A)
    End Function

    Private Function ThemeColorText(ByVal argb As Integer) As String
        Return ThemeColorText(Color.FromArgb(argb))
    End Function

    Private Function ParseThemeColor(ByVal value As String) As Color
        If value.Length <> 9 OrElse Not value.StartsWith("#") Then Throw New FormatException("Invalid color: " & value)

        Dim r As Integer = Integer.Parse(value.Substring(1, 2), Globalization.NumberStyles.HexNumber, Globalization.CultureInfo.InvariantCulture)
        Dim g As Integer = Integer.Parse(value.Substring(3, 2), Globalization.NumberStyles.HexNumber, Globalization.CultureInfo.InvariantCulture)
        Dim b As Integer = Integer.Parse(value.Substring(5, 2), Globalization.NumberStyles.HexNumber, Globalization.CultureInfo.InvariantCulture)
        Dim a As Integer = Integer.Parse(value.Substring(7, 2), Globalization.NumberStyles.HexNumber, Globalization.CultureInfo.InvariantCulture)
        Return Color.FromArgb(a, r, g, b)
    End Function

    Private Function ParseThemeInteger(ByVal value As String) As Integer
        Return Integer.Parse(value, Globalization.NumberStyles.Integer, Globalization.CultureInfo.InvariantCulture)
    End Function

    Private Function ParseThemeSingle(ByVal value As String) As Single
        Return Single.Parse(value, Globalization.NumberStyles.Float, Globalization.CultureInfo.InvariantCulture)
    End Function

    Private Structure ThemeColumnColors
        Public Note As Integer
        Public Text As Color
        Public LongNote As Integer
        Public LongText As Color
        Public Bg As Color
    End Structure

    Private Function ThemeColumnColorsFromColumn(ByVal c As Column) As ThemeColumnColors
        Dim colors As New ThemeColumnColors
        colors.Note = c.cNote
        colors.Text = c.cText
        colors.LongNote = c.cNote
        colors.LongText = c.cText
        colors.Bg = c.cBG
        Return colors
    End Function

    Private Function LoadThemeColors(ByVal n As XmlElement) As ThemeColumnColors
        If Not n.HasAttribute("note") Then Throw New FormatException("Theme colors note is missing.")
        If Not n.HasAttribute("text") Then Throw New FormatException("Theme colors text is missing.")
        If Not n.HasAttribute("bg") Then Throw New FormatException("Theme colors bg is missing.")

        Dim colors As New ThemeColumnColors
        colors.Note = ParseThemeColor(n.GetAttribute("note")).ToArgb
        colors.Text = ParseThemeColor(n.GetAttribute("text"))
        colors.LongNote = colors.Note
        colors.LongText = colors.Text
        colors.Bg = ParseThemeColor(n.GetAttribute("bg"))

        If n.HasAttribute("longNote") Then colors.LongNote = ParseThemeColor(n.GetAttribute("longNote")).ToArgb
        If n.HasAttribute("longText") Then colors.LongText = ParseThemeColor(n.GetAttribute("longText"))
        Return colors
    End Function

    Private Function LoadThemePalette(ByVal root As XmlElement) As Dictionary(Of String, ThemeColumnColors)
        Dim palette As New Dictionary(Of String, ThemeColumnColors)(StringComparer.OrdinalIgnoreCase)
        Dim ePalette As XmlElement = root.Item("palette")
        If ePalette Is Nothing Then Return palette

        For Each node As XmlNode In ePalette.ChildNodes
            If node.NodeType <> XmlNodeType.Element Then Continue For

            Dim eColors As XmlElement = CType(node, XmlElement)
            If eColors.Name <> "colors" Then Throw New FormatException("Unknown theme palette element: " & eColors.Name)
            If Not eColors.HasAttribute("id") OrElse eColors.GetAttribute("id") = "" Then Throw New FormatException("Theme colors id is missing.")

            Dim id As String = eColors.GetAttribute("id")
            If palette.ContainsKey(id) Then Throw New FormatException("Duplicate theme colors id: " & id)
            palette.Add(id, LoadThemeColors(eColors))
        Next

        Return palette
    End Function

    Private Function ResolveThemeLayoutColors(ByVal n As XmlElement, ByVal palette As Dictionary(Of String, ThemeColumnColors), ByVal baseColumn As Column) As ThemeColumnColors
        Dim colors As ThemeColumnColors = ThemeColumnColorsFromColumn(baseColumn)

        If n.HasAttribute("colors") Then
            Dim colorsId As String = n.GetAttribute("colors")
            If colorsId = "" Then Throw New FormatException("Theme colors reference is empty.")
            If Not palette.ContainsKey(colorsId) Then Throw New FormatException("Unknown theme colors: " & colorsId)
            colors = palette(colorsId)
        End If

        If n.HasAttribute("note") Then
            colors.Note = ParseThemeColor(n.GetAttribute("note")).ToArgb
            If Not n.HasAttribute("longNote") Then colors.LongNote = colors.Note
        End If
        If n.HasAttribute("text") Then
            colors.Text = ParseThemeColor(n.GetAttribute("text"))
            If Not n.HasAttribute("longText") Then colors.LongText = colors.Text
        End If
        If n.HasAttribute("longNote") Then colors.LongNote = ParseThemeColor(n.GetAttribute("longNote")).ToArgb
        If n.HasAttribute("longText") Then colors.LongText = ParseThemeColor(n.GetAttribute("longText"))
        If n.HasAttribute("bg") Then colors.Bg = ParseThemeColor(n.GetAttribute("bg"))

        Return colors
    End Function

    Private Function ThemeLaneColumnIndex(ByVal channel As String) As Integer
        channel = channel.Trim().ToUpperInvariant()
        If channel.Length <> 2 Then Throw New FormatException("Invalid lane channel: " & channel)

        Dim keyIndex As Integer = "123456789ABCDEFGHIJKLMNOPQ".IndexOf(channel.Chars(1))
        If keyIndex < 0 Then Throw New FormatException("Invalid lane channel: " & channel)

        Dim p1Columns() As Integer = {
            niA3, niA4, niA5, niA6, niA7, niA1, niA2, niA8, niA9,
            niAA, niAB, niAC, niAD, niAE, niAF, niAG, niAH, niAI,
            niAJ, niAK, niAL, niAM, niAN, niAO, niAP, niAQ}
        Dim p2Columns() As Integer = {
            niD1, niD2, niD3, niD4, niD5, niDP, niDQ, niD6, niD7,
            niD8, niD9, niDA, niDB, niDC, niDD, niDE, niDF, niDG,
            niDH, niDI, niDJ, niDK, niDL, niDM, niDN, niDO}

        Select Case channel.Chars(0)
            Case "1"c : Return p1Columns(keyIndex)
            Case "2"c : Return p2Columns(keyIndex)
        End Select

        Throw New FormatException("Invalid lane channel: " & channel)
    End Function

    Private Function ThemeSingleChannelColumnIndex(ByVal channel As String) As Integer
        Select Case channel.Trim().ToUpperInvariant()
            Case "SC" : Return niSCROLL
            Case "09" : Return niSTOP
            Case "04" : Return niBGA
            Case "07" : Return niLAYER
            Case "06" : Return niPOOR
        End Select

        Throw New FormatException("Unsupported theme channel: " & channel)
    End Function

    Private Function ThemeSingleChannel(ByVal index As Integer) As String
        Select Case index
            Case niSCROLL : Return "SC"
            Case niSTOP : Return "09"
            Case niBGA : Return "04"
            Case niLAYER : Return "07"
            Case niPOOR : Return "06"
        End Select

        Return ""
    End Function

    Private Function ThemeElementName(ByVal index As Integer) As String
        If index = niMeasure Then Return "measure"
        If index = niBPM Then Return "bpm"
        If index = niB Then Return "bgm"
        If index >= niA1 AndAlso index <= niAQ Then Return "lane"
        If index >= niD1 AndAlso index <= niDQ Then Return "lane"
        If ThemeSingleChannel(index) <> "" Then Return "channel"
        Return ""
    End Function

    Private Sub XMLWriteThemeLayoutElement(ByVal w As XmlTextWriter, ByVal index As Integer)
        Dim elementName As String = ThemeElementName(index)
        If elementName = "" Then Return

        w.WriteStartElement(elementName)
        With column(index)
            If .Title <> "" Then w.WriteAttributeString("label", .Title)
            If elementName = "lane" Then w.WriteAttributeString("channel", C10to36Channel(.Identifier))
            If elementName = "channel" Then w.WriteAttributeString("channel", ThemeSingleChannel(index))
            w.WriteAttributeString("width", .Width)
            w.WriteAttributeString("note", ThemeColorText(.cNote))
            w.WriteAttributeString("text", ThemeColorText(.cText))
            If .cLNote <> .cNote Then w.WriteAttributeString("longNote", ThemeColorText(.cLNote))
            If .cLText.ToArgb <> .cText.ToArgb Then w.WriteAttributeString("longText", ThemeColorText(.cLText))
            w.WriteAttributeString("bg", ThemeColorText(.cBG))
        End With
        w.WriteEndElement()
    End Sub

    Private Sub XMLWriteThemeColor(ByVal w As XmlTextWriter, ByVal id As String, ByVal color As Color)
        w.WriteStartElement("color")
        w.WriteAttributeString("id", id)
        w.WriteAttributeString("value", ThemeColorText(color))
        w.WriteEndElement()
    End Sub

    Private Sub XMLWriteThemeFont(ByVal w As XmlTextWriter, ByVal id As String, ByVal font As Font)
        w.WriteStartElement("font")
        w.WriteAttributeString("id", id)
        w.WriteAttributeString("name", font.FontFamily.Name)
        w.WriteAttributeString("size", WriteDecimalWithDot(font.SizeInPoints))
        w.WriteAttributeString("style", font.Style.ToString())
        w.WriteEndElement()
    End Sub

    Private Sub SaveTheme(ByVal filePath As String)
        Dim xSavedThemeColumnOrder As New List(Of Integer)

        Using w As New XmlTextWriter(filePath, New System.Text.UTF8Encoding(False))
            With w
                .WriteStartDocument()
                .Formatting = Formatting.Indented
                .Indentation = 4

                .WriteStartElement("nBMSCTheme")
                .WriteAttributeString("version", "1")
                .WriteAttributeString("name", IO.Path.GetFileNameWithoutExtension(filePath))

                .WriteStartElement("layout")
                If Not ThemePlayerGap Then .WriteAttributeString("playerGap", "false")
                If ThemeAlwaysShow2P Then .WriteAttributeString("alwaysShow2P", "true")
                For Each i As Integer In ThemeColumnDisplayOrder()
                    If Not column(i).isVisible Then Continue For
                    If IsThemeSpacerColumn(i) Then Continue For
                    XMLWriteThemeLayoutElement(w, i)
                    xSavedThemeColumnOrder.Add(i)
                Next
                .WriteEndElement()

                .WriteStartElement("visual")
                XMLWriteThemeColor(w, "columnTitle", vo.ColumnTitle.Color)
                XMLWriteThemeFont(w, "columnTitle", vo.ColumnTitleFont)
                XMLWriteThemeColor(w, "background", vo.Bg.Color)
                XMLWriteThemeColor(w, "grid", vo.pGrid.Color)
                XMLWriteThemeColor(w, "subGrid", vo.pSub.Color)
                XMLWriteThemeColor(w, "verticalLine", vo.pVLine.Color)
                XMLWriteThemeColor(w, "measureLine", vo.pMLine.Color)
                XMLWriteThemeColor(w, "bgmWave", vo.pBGMWav.Color)
                XMLWriteThemeColor(w, "selectionBox", vo.SelBox.Color)
                XMLWriteThemeColor(w, "timeCursor", vo.PECursor.Color)
                XMLWriteThemeColor(w, "timeHalf", vo.PEHalf.Color)
                XMLWriteThemeColor(w, "timeMouseOver", vo.PEMouseOver.Color)
                XMLWriteThemeColor(w, "timeSelection", vo.PESel.Color)
                XMLWriteThemeColor(w, "timeBpm", vo.PEBPM.Color)
                XMLWriteThemeFont(w, "timeBpm", vo.PEBPMFont)
                .WriteStartElement("spacing")
                .WriteAttributeString("columnGap", ThemeColumnGap)
                .WriteEndElement()
                .WriteStartElement("note")
                .WriteAttributeString("height", vo.kHeight)
                .WriteAttributeString("hiddenOpacity", WriteDecimalWithDot(vo.kOpacity))
                .WriteEndElement()
                XMLWriteThemeFont(w, "noteLabel", vo.kFont)
                XMLWriteThemeFont(w, "measureLabel", vo.kMFont)
                .WriteStartElement("labelOffset")
                .WriteAttributeString("vertical", vo.kLabelVShift)
                .WriteAttributeString("horizontal", vo.kLabelHShift)
                .WriteAttributeString("longHorizontal", vo.kLabelHShiftL)
                .WriteEndElement()
                XMLWriteThemeColor(w, "noteMouseOver", vo.kMouseOver.Color)
                XMLWriteThemeColor(w, "noteAdjustBorder", vo.kMouseOverE.Color)
                XMLWriteThemeColor(w, "noteSelected", vo.kSelected.Color)
                .WriteEndElement()

                .WriteEndElement()
                .WriteEndDocument()
            End With
        End Using
        CurrentThemePath = IO.Path.GetFullPath(filePath)
        ReDim ThemeColumnVisible(UBound(column))
        For i As Integer = 0 To UBound(column)
            ThemeColumnVisible(i) = column(i).isVisible AndAlso Not IsThemeSpacerColumn(i)
        Next
        ThemeColumnOrder = xSavedThemeColumnOrder.ToArray()
    End Sub

    Private Sub XMLWriteFont(ByVal w As XmlTextWriter, ByVal local As String, ByVal f As Font)
        w.WriteStartElement(local)
        w.WriteAttributeString("Name", f.FontFamily.Name)
        w.WriteAttributeString("Size", f.SizeInPoints)
        w.WriteAttributeString("Style", f.Style)
        w.WriteEndElement()
    End Sub

    Private Sub XMLWritePlayerArguments(ByVal w As XmlTextWriter, ByVal I As Integer)
        w.WriteStartElement("Player")
        w.WriteAttributeString("Index", I)
        w.WriteAttributeString("Path", pArgs(I).Path)
        w.WriteAttributeString("FromBeginning", pArgs(I).aBegin)
        w.WriteAttributeString("FromHere", pArgs(I).aHere)
        w.WriteAttributeString("Stop", pArgs(I).aStop)
        w.WriteEndElement()
    End Sub

    Private Function DefaultThemePath() As String
        Return IO.Path.Combine(My.Application.Info.DirectoryPath, "Theme\7key.xml")
    End Function

    Private Function ResolveThemePath(ByVal filePath As String) As String
        If filePath = "" Then Return DefaultThemePath()
        If IO.Path.IsPathRooted(filePath) Then Return filePath
        Return IO.Path.Combine(My.Application.Info.DirectoryPath, filePath)
    End Function

    Private Function ThemePathForSettings() As String
        Dim xThemePath As String = If(CurrentThemePath = "", DefaultThemePath(), CurrentThemePath)
        Dim xBasePath As String = My.Application.Info.DirectoryPath.TrimEnd("\"c)
        Dim xFullPath As String = IO.Path.GetFullPath(xThemePath)
        If xFullPath.StartsWith(xBasePath & "\", StringComparison.OrdinalIgnoreCase) Then
            Return xFullPath.Substring(xBasePath.Length + 1)
        End If
        Return xFullPath
    End Function

    Private Function LoadThemeOrDefault(ByVal filePath As String) As Boolean
        If LoadThemeFile(ResolveThemePath(filePath)) Then Return True

        Dim xDefaultThemePath As String = DefaultThemePath()
        If String.Equals(ResolveThemePath(filePath), xDefaultThemePath, StringComparison.OrdinalIgnoreCase) Then Return False
        Return LoadThemeFile(xDefaultThemePath)
    End Function

    Private Sub SaveSettings(ByVal Path As String, ByVal ThemeOnly As Boolean)
        If ThemeOnly Then
            SaveTheme(Path)

            Return
        End If

        Dim w As New XmlTextWriter(Path, System.Text.Encoding.Unicode)
        With w
            .WriteStartDocument()
            .Formatting = Formatting.Indented
            .Indentation = 4

            .WriteStartElement("iBMSC")
            .WriteAttributeString("Major", My.Application.Info.Version.Major)
            .WriteAttributeString("Minor", My.Application.Info.Version.Minor)
            .WriteAttributeString("Build", My.Application.Info.Version.Build)

            .WriteStartElement("Form")
            .WriteAttributeString("WindowState", IIf(isFullScreen, previousWindowState, Me.WindowState))
            .WriteAttributeString("Width", IIf(isFullScreen, previousWindowPosition.Width, Me.Width))
            .WriteAttributeString("Height", IIf(isFullScreen, previousWindowPosition.Height, Me.Height))
            .WriteAttributeString("Top", IIf(isFullScreen, previousWindowPosition.Top, Me.Top))
            .WriteAttributeString("Left", IIf(isFullScreen, previousWindowPosition.Left, Me.Left))
            .WriteEndElement()

            .WriteStartElement("Recent")
            .WriteAttributeString("Recent0", Recent(0))
            .WriteAttributeString("Recent1", Recent(1))
            .WriteAttributeString("Recent2", Recent(2))
            .WriteAttributeString("Recent3", Recent(3))
            .WriteAttributeString("Recent4", Recent(4))
            .WriteEndElement()

            .WriteStartElement("Edit")
            .WriteAttributeString("NTInput", NTInput)
            .WriteAttributeString("Language", DispLang)
            '.WriteAttributeString("SortingMethod", SortingMethod)
            .WriteAttributeString("ErrorCheck", ErrorCheck)
            .WriteAttributeString("AutoFocusMouseEnter", AutoFocusMouseEnter)
            .WriteAttributeString("FirstClickDisabled", FirstClickDisabled)
            .WriteAttributeString("ShowFileName", ShowFileName)
            .WriteAttributeString("ChangePlaySide", Rscratch)
            .WriteAttributeString("SyncSplitViewScroll", SyncSplitViewScroll)
            .WriteAttributeString("MiddleButtonMoveMethod", MiddleButtonMoveMethod)
            .WriteAttributeString("AutoSaveInterval", AutoSaveInterval)
            .WriteAttributeString("PreviewOnClick", PreviewOnClick)
            '.WriteAttributeString("PreviewErrorCheck", PreviewErrorCheck)
            .WriteAttributeString("ClickStopPreview", ClickStopPreview)
            .WriteAttributeString("SkipClippedMeasure", SkipClippedMeasure)
            .WriteAttributeString("LaneHighlight", LaneHighlight)
            .WriteAttributeString("UndoRedoMemoryLimitMB", UndoRedoMemoryLimitMB)
            .WriteEndElement()

            .WriteStartElement("Save")
            .WriteAttributeString("InputTextEncoding", TextEncodingModeToString(InputTextEncoding))
            .WriteAttributeString("OutputTextEncoding", TextEncodingModeToString(OutputTextEncoding))
            .WriteAttributeString("BMSGridLimit", BMSGridLimit)
            .WriteAttributeString("BeepWhileSaved", BeepWhileSaved)
            .WriteAttributeString("NewBMSUseBase62Definitions", NewBMSUseBase62Definitions)
            .WriteAttributeString("BPMDefinitionMode", BPMDefinitionMode)
            .WriteAttributeString("STOPDefinitionMode", STOPDefinitionMode)
            .WriteEndElement()

            .WriteStartElement("Update")
            .WriteAttributeString("SkippedTag", SkippedUpdateTag)
            .WriteAttributeString("CheckOnStartup", CheckUpdatesOnStartup)
            .WriteEndElement()

            .WriteStartElement("WAV")
            .WriteAttributeString("WAVMultiSelect", WAVMultiSelect)
            .WriteAttributeString("WAVChangeLabel", WAVChangeLabel)
            .WriteAttributeString("WAVEmptyfill", WAVEmptyfill)
            .WriteAttributeString("BeatChangeMode", BeatChangeMode)
            .WriteEndElement()

            .WriteStartElement("ShowHide")
            StoreVisibleSplitViewRatios()
            .WriteAttributeString("showMenu", mnSMenu.Checked)
            .WriteAttributeString("showTB", mnSTB.Checked)
            .WriteAttributeString("showOpPanel", mnSOP.Checked)
            .WriteAttributeString("showStatus", mnSStatus.Checked)
            .WriteAttributeString("rightSplitRatios", GetSplitPanelRatiosSetting())
            .WriteEndElement()

            .WriteStartElement("Grid")
            .WriteAttributeString("gSnap", gSnap)
            .WriteAttributeString("gDisableVertical", DisableVerticalMove)
            .WriteAttributeString("gWheel", gWheel)
            .WriteAttributeString("gPgUpDn", gPgUpDn)
            .WriteAttributeString("gShow", gShowGrid)
            .WriteAttributeString("gShowS", gShowSubGrid)
            .WriteAttributeString("gShowBG", gShowBG)
            .WriteAttributeString("gShowM", gShowMeasureNumber)
            .WriteAttributeString("gShowV", gShowVerticalLine)
            .WriteAttributeString("gShowMB", gShowMeasureBar)
            .WriteAttributeString("gShowC", gShowC)
            .WriteAttributeString("gBPM", gBPM)
            .WriteAttributeString("gSTOP", gSTOP)
            .WriteAttributeString("gSCROLL", gSCROLL)
            .WriteAttributeString("gBLP", gDisplayBGAColumn)
            .WriteAttributeString("gP2", CHPlayer.SelectedIndex)
            .WriteAttributeString("gCol", CGB.Value)
            .WriteAttributeString("gDivide", gDivide)
            .WriteAttributeString("gSub", gSub)
            .WriteAttributeString("gSlash", gSlash)
            .WriteAttributeString("gxHeight", gxHeight)
            .WriteAttributeString("gxWidth", gxWidth)
            .WriteEndElement()

            .WriteStartElement("WaveForm")
            .WriteAttributeString("wLock", wLock)
            .WriteAttributeString("wPosition", wPosition)
            .WriteAttributeString("wLeft", wLeft)
            .WriteAttributeString("wWidth", wWidth)
            .WriteAttributeString("wPrecision", wPrecision)
            .WriteEndElement()

            .WriteStartElement("Player")
            .WriteAttributeString("Count", pArgs.Length)
            .WriteAttributeString("CurrentPlayer", CurrentPlayer)
            For i As Integer = 0 To UBound(pArgs)
                XMLWritePlayerArguments(w, i) : Next
            .WriteEndElement()

            .WriteStartElement("VisualOptions")
            XMLWriteValue(w, "TSDeltaMouseOver", vo.PEDeltaMouseOver)
            XMLWriteValue(w, "MiddleDeltaRelease", vo.MiddleDeltaRelease)
            .WriteEndElement()

            .WriteStartElement("Theme")
            .WriteAttributeString("Path", ThemePathForSettings())
            .WriteEndElement()

            .WriteEndElement()
            .WriteEndDocument()
            .Close()
        End With
    End Sub

    Private Sub XMLLoadElementValue(ByVal n As XmlElement, ByRef v As Integer)
        If n Is Nothing Then Exit Sub
        XMLLoadAttribute(n.GetAttribute("Value"), v)
    End Sub
    Private Sub XMLLoadElementValue(ByVal n As XmlElement, ByRef v As Single)
        If n Is Nothing Then Exit Sub
        XMLLoadAttribute(n.GetAttribute("Value"), v)
    End Sub
    Private Sub XMLLoadElementValue(ByVal n As XmlElement, ByRef v As Color)
        If n Is Nothing Then Exit Sub
        XMLLoadAttribute(n.GetAttribute("Value"), v)
    End Sub

    Private Sub XMLLoadElementValue(ByVal n As XmlElement, ByRef v As Font)
        If n Is Nothing Then Exit Sub

        Dim xName As String = Me.Font.FontFamily.Name
        Dim xSize As Integer = Me.Font.Size
        Dim xStyle As Integer = Me.Font.Style
        XMLLoadAttribute(n.GetAttribute("Name"), xName)
        XMLLoadAttribute(n.GetAttribute("Size"), xSize)
        XMLLoadAttribute(n.GetAttribute("Style"), xStyle)
        v = New Font(xName, xSize, CType(xStyle, System.Drawing.FontStyle))
    End Sub

    Private Function CreateThemeColumns() As Column()
        Dim xColumns() As Column = CType(InitialColumns.Clone(), Column())
        For i As Integer = 0 To UBound(xColumns)
            xColumns(i).isVisible = False
            xColumns(i).Title = ""
        Next
        Return xColumns
    End Function

    Private Function ThemeElementById(ByVal parent As XmlElement, ByVal elementName As String, ByVal id As String) As XmlElement
        For Each n As XmlElement In parent.GetElementsByTagName(elementName)
            If String.Equals(n.GetAttribute("id"), id, StringComparison.OrdinalIgnoreCase) Then Return n
        Next
        Return Nothing
    End Function

    Private Function ThemeColor(ByVal parent As XmlElement, ByVal id As String, ByVal current As Color) As Color
        Dim n As XmlElement = ThemeElementById(parent, "color", id)
        If n Is Nothing OrElse Not n.HasAttribute("value") Then Return current
        Return ParseThemeColor(n.GetAttribute("value"))
    End Function

    Private Function ThemeFont(ByVal parent As XmlElement, ByVal id As String, ByVal current As Font) As Font
        Dim n As XmlElement = ThemeElementById(parent, "font", id)
        If n Is Nothing Then Return current

        Dim xName As String = current.FontFamily.Name
        Dim xSize As Single = current.SizeInPoints
        Dim xStyle As FontStyle = current.Style
        If n.HasAttribute("name") Then xName = n.GetAttribute("name")
        If n.HasAttribute("size") Then xSize = ParseThemeSingle(n.GetAttribute("size"))
        If n.HasAttribute("style") Then xStyle = CType([Enum].Parse(GetType(FontStyle), n.GetAttribute("style"), True), FontStyle)
        Return New Font(xName, xSize, xStyle)
    End Function

    Private Function LoadThemeLayoutElement(ByVal n As XmlElement, ByVal xColumns() As Column, ByVal palette As Dictionary(Of String, ThemeColumnColors)) As Integer
        Dim index As Integer = -1
        Select Case n.Name
            Case "measure"
                index = niMeasure
            Case "bpm"
                index = niBPM
            Case "bgm"
                index = niB
            Case "lane"
                If Not n.HasAttribute("channel") Then Throw New FormatException("Theme lane channel is missing.")
                index = ThemeLaneColumnIndex(n.GetAttribute("channel"))
                xColumns(index).Identifier = C36ChannelTo10(n.GetAttribute("channel"))
            Case "channel"
                If Not n.HasAttribute("channel") Then Throw New FormatException("Theme channel is missing.")
                index = ThemeSingleChannelColumnIndex(n.GetAttribute("channel"))
            Case Else
                Throw New FormatException("Unknown theme layout element: " & n.Name)
        End Select

        If index < 0 OrElse index > UBound(xColumns) Then Throw New FormatException("Unknown theme column.")

        With xColumns(index)
            Dim colors As ThemeColumnColors = ResolveThemeLayoutColors(n, palette, xColumns(index))
            If n.HasAttribute("width") Then .Width = ParseThemeInteger(n.GetAttribute("width"))
            .Title = If(n.HasAttribute("label"), n.GetAttribute("label"), "")
            .setNoteColor(colors.Note)
            .cText = colors.Text
            .setLNoteColor(colors.LongNote)
            .cLText = colors.LongText
            .cBG = colors.Bg
            .isVisible = True
        End With
        Return index
    End Function

    Private Function LoadThemeVisual(ByVal n As XmlElement, ByRef columnGap As Integer) As visualSettings
        Dim xVo As New visualSettings()
        If n Is Nothing Then Return xVo

        xVo.ColumnTitle.Color = ThemeColor(n, "columnTitle", xVo.ColumnTitle.Color)
        xVo.ColumnTitleFont = ThemeFont(n, "columnTitle", xVo.ColumnTitleFont)
        xVo.Bg.Color = ThemeColor(n, "background", xVo.Bg.Color)
        xVo.pGrid.Color = ThemeColor(n, "grid", xVo.pGrid.Color)
        xVo.pSub.Color = ThemeColor(n, "subGrid", xVo.pSub.Color)
        xVo.pVLine.Color = ThemeColor(n, "verticalLine", xVo.pVLine.Color)
        xVo.pMLine.Color = ThemeColor(n, "measureLine", xVo.pMLine.Color)
        xVo.pBGMWav.Color = ThemeColor(n, "bgmWave", xVo.pBGMWav.Color)
        xVo.SelBox.Color = ThemeColor(n, "selectionBox", xVo.SelBox.Color)
        xVo.PECursor.Color = ThemeColor(n, "timeCursor", xVo.PECursor.Color)
        xVo.PEHalf.Color = ThemeColor(n, "timeHalf", xVo.PEHalf.Color)
        xVo.PEMouseOver.Color = ThemeColor(n, "timeMouseOver", xVo.PEMouseOver.Color)
        xVo.PESel.Color = ThemeColor(n, "timeSelection", xVo.PESel.Color)
        xVo.PEBPM.Color = ThemeColor(n, "timeBpm", xVo.PEBPM.Color)
        xVo.PEBPMFont = ThemeFont(n, "timeBpm", xVo.PEBPMFont)

        Dim eSpacing As XmlElement = n.Item("spacing")
        If eSpacing IsNot Nothing AndAlso eSpacing.HasAttribute("columnGap") Then
            columnGap = Math.Max(0, ParseThemeInteger(eSpacing.GetAttribute("columnGap")))
        End If

        Dim eNote As XmlElement = n.Item("note")
        If eNote IsNot Nothing Then
            If eNote.HasAttribute("height") Then xVo.kHeight = ParseThemeInteger(eNote.GetAttribute("height"))
            If eNote.HasAttribute("hiddenOpacity") Then xVo.kOpacity = ParseThemeSingle(eNote.GetAttribute("hiddenOpacity"))
        End If

        xVo.kFont = ThemeFont(n, "noteLabel", xVo.kFont)
        xVo.kMFont = ThemeFont(n, "measureLabel", xVo.kMFont)

        Dim eLabelOffset As XmlElement = n.Item("labelOffset")
        If eLabelOffset IsNot Nothing Then
            If eLabelOffset.HasAttribute("vertical") Then xVo.kLabelVShift = ParseThemeInteger(eLabelOffset.GetAttribute("vertical"))
            If eLabelOffset.HasAttribute("horizontal") Then xVo.kLabelHShift = ParseThemeInteger(eLabelOffset.GetAttribute("horizontal"))
            If eLabelOffset.HasAttribute("longHorizontal") Then xVo.kLabelHShiftL = ParseThemeInteger(eLabelOffset.GetAttribute("longHorizontal"))
        End If

        xVo.kMouseOver.Color = ThemeColor(n, "noteMouseOver", xVo.kMouseOver.Color)
        xVo.kMouseOverE.Color = ThemeColor(n, "noteAdjustBorder", xVo.kMouseOverE.Color)
        xVo.kSelected.Color = ThemeColor(n, "noteSelected", xVo.kSelected.Color)
        Return xVo
    End Function

    Private Sub ApplyThemeVisualState()
        TWTransparency.Value = vo.pBGMWav.Color.A
        TWTransparency2.Value = vo.pBGMWav.Color.A
        TWSaturation.Value = vo.pBGMWav.Color.GetSaturation * 1000
        TWSaturation2.Value = vo.pBGMWav.Color.GetSaturation * 1000
    End Sub

    Private Function LoadThemeFile(ByVal filePath As String) As Boolean
        Try
            Dim Doc As New XmlDocument
            Using FileStream As New IO.FileStream(filePath, FileMode.Open, FileAccess.Read)
                Doc.Load(FileStream)
            End Using

            Dim Root As XmlElement = Doc.Item("nBMSCTheme")
            If Root Is Nothing OrElse Root.GetAttribute("version") <> "1" Then Throw New FormatException("Unsupported theme file.")

            Dim eLayout As XmlElement = Root.Item("layout")
            If eLayout Is Nothing Then Throw New FormatException("Theme layout is missing.")

            Dim xThemePlayerGap As Boolean = True
            If eLayout.HasAttribute("playerGap") Then xThemePlayerGap = Boolean.Parse(eLayout.GetAttribute("playerGap"))
            Dim xThemeAlwaysShow2P As Boolean = False
            If eLayout.HasAttribute("alwaysShow2P") Then xThemeAlwaysShow2P = Boolean.Parse(eLayout.GetAttribute("alwaysShow2P"))

            Dim xPalette As Dictionary(Of String, ThemeColumnColors) = LoadThemePalette(Root)
            Dim xColumns() As Column = CreateThemeColumns()
            Dim xThemeColumnVisible(UBound(xColumns)) As Boolean
            Dim xThemeColumnOrder As New List(Of Integer)
            For Each eeNode As XmlNode In eLayout.ChildNodes
                If eeNode.NodeType <> XmlNodeType.Element Then Continue For
                Dim xColumnIndex As Integer = LoadThemeLayoutElement(CType(eeNode, XmlElement), xColumns, xPalette)
                If xThemeColumnVisible(xColumnIndex) Then Throw New FormatException("Duplicate theme layout column.")
                xThemeColumnVisible(xColumnIndex) = True
                xThemeColumnOrder.Add(xColumnIndex)
            Next

            If iPlayer = 0 AndAlso Not xThemeAlwaysShow2P Then
                For i = niD1 To niDQ
                    xColumns(i).isVisible = False
                Next
            End If

            Dim xThemeColumnGap As Integer = 5
            Dim xVo As visualSettings = LoadThemeVisual(Root.Item("visual"), xThemeColumnGap)
            column = xColumns
            vo = xVo
            ThemeColumnGap = xThemeColumnGap
            ThemePlayerGap = xThemePlayerGap
            ThemeAlwaysShow2P = xThemeAlwaysShow2P
            ThemeColumnVisible = xThemeColumnVisible
            ThemeColumnOrder = xThemeColumnOrder.ToArray()
            CurrentThemePath = IO.Path.GetFullPath(filePath)
            ApplyThemeVisualState()
            CalculateGreatestColumn()
            Return True

        Catch ex As Exception
            MsgBox(filePath & vbCrLf & vbCrLf & ex.Message, MsgBoxStyle.Exclamation)
            Return False
        End Try
    End Function

    Private Function TryGetThemeName(ByVal xStr As FileInfo, ByRef themeName As String) As Boolean
        Try
            Dim Doc As New XmlDocument
            Using FileStream As New IO.FileStream(xStr.FullName, FileMode.Open, FileAccess.Read)
                Doc.Load(FileStream)
            End Using

            Dim Root As XmlElement = Doc.Item("nBMSCTheme")
            If Root Is Nothing OrElse Root.GetAttribute("version") <> "1" Then Return False

            themeName = Root.GetAttribute("name")
            If themeName = "" Then themeName = IO.Path.GetFileNameWithoutExtension(xStr.Name)
            Return True

        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Sub XMLLoadPlayer(ByVal n As XmlElement)
        Dim i As Integer = -1
        XMLLoadAttribute(n.GetAttribute("Index"), i)
        If i < 0 Or i > UBound(pArgs) Then Exit Sub

        XMLLoadAttribute(n.GetAttribute("Path"), pArgs(i).Path)
        XMLLoadAttribute(n.GetAttribute("FromBeginning"), pArgs(i).aBegin)
        XMLLoadAttribute(n.GetAttribute("FromHere"), pArgs(i).aHere)
        XMLLoadAttribute(n.GetAttribute("Stop"), pArgs(i).aStop)
    End Sub

    Private Sub XMLLoadColumn(ByVal n As XmlElement)
        Dim i As Integer = -1
        XMLLoadAttribute(n.GetAttribute("Index"), i)
        If i < 0 Or i > UBound(column) Then Exit Sub

        With column(i)
            'XMLLoadAttribute(n.GetAttribute("Left"), .Left)
            XMLLoadAttribute(n.GetAttribute("Width"), .Width)
            If n.HasAttribute("Title") Then .Title = n.GetAttribute("Title")
            'XMLLoadAttribute(n.GetAttribute("Text"), .Text)
            Dim Display As Boolean
            Dim attr = n.GetAttribute("Display")
            XMLLoadAttribute(attr, Display)
            .isVisible = IIf(String.IsNullOrEmpty(attr), .isVisible, Display)

            'XMLLoadAttribute(n.GetAttribute("isNumeric"), .isNumeric)
            'XMLLoadAttribute(n.GetAttribute("Visible"), .Visible)
            'XMLLoadAttribute(n.GetAttribute("Identifier"), .Identifier)
            XMLLoadAttribute(n.GetAttribute("NoteColor"), .cNote)
            .setNoteColor(.cNote)
            XMLLoadAttribute(n.GetAttribute("TextColor"), .cText)
            XMLLoadAttribute(n.GetAttribute("LongNoteColor"), .cLNote)
            .setLNoteColor(.cLNote)
            XMLLoadAttribute(n.GetAttribute("LongTextColor"), .cLText)
            XMLLoadAttribute(n.GetAttribute("BG"), .cBG)
        End With
    End Sub

    Private Function LoadInputTextEncodingMode(ByVal eSave As XmlElement) As TextEncodingMode
        If eSave.HasAttribute("InputTextEncoding") Then
            Return ParseTextEncodingMode(eSave.GetAttribute("InputTextEncoding"), TextEncodingMode.Auto)
        End If

        If eSave.HasAttribute("TextEncoding") Then
            Return ParseTextEncodingMode(eSave.GetAttribute("TextEncoding"), TextEncodingMode.Auto)
        End If

        Return TextEncodingMode.Auto
    End Function

    Private Function LoadOutputTextEncodingMode(ByVal eSave As XmlElement) As TextEncodingMode
        If eSave.HasAttribute("OutputTextEncoding") Then
            Return CoerceOutputTextEncodingMode(ParseTextEncodingMode(eSave.GetAttribute("OutputTextEncoding"), TextEncodingMode.SystemDefault))
        End If

        If eSave.HasAttribute("DefaultTextEncoding") Then
            Return CoerceOutputTextEncodingMode(ParseTextEncodingMode(eSave.GetAttribute("DefaultTextEncoding"), TextEncodingMode.SystemDefault))
        End If

        If eSave.HasAttribute("TextEncoding") Then
            Return CoerceOutputTextEncodingMode(ParseTextEncodingMode(eSave.GetAttribute("TextEncoding"), TextEncodingMode.SystemDefault))
        End If

        Return TextEncodingMode.SystemDefault
    End Function

    Private Function IsSettingsBeforeVersion(ByVal xMajor As Integer, ByVal xMinor As Integer, ByVal xBuild As Integer, ByVal yMajor As Integer, ByVal yMinor As Integer, ByVal yBuild As Integer) As Boolean
        If xMajor <> yMajor Then Return xMajor < yMajor
        If xMinor <> yMinor Then Return xMinor < yMinor
        Return xBuild < yBuild
    End Function

    Private Sub LoadSettings(ByVal Path As String)
        If Not My.Computer.FileSystem.FileExists(Path) Then Return

        'Dim xTempFileName As String = ""
        'Do
        'Try
        'xTempFileName = Me.RandomFileName(".xml")
        'File.Copy(Path, xTempFileName)
        'Catch
        'Continue Do
        'End Try
        'Exit Do
        'Loop
        Dim Doc As New XmlDocument
        Dim FileStream As New IO.FileStream(Path, FileMode.Open, FileAccess.Read)
        Doc.Load(FileStream)
        Dim xThemePath As String = DefaultThemePath()

        Dim Root As XmlElement = Doc.Item("iBMSC")
        If Root Is Nothing Then GoTo EndOfSub

        'version
        Dim Major As Integer = My.Application.Info.Version.Major
        Dim Minor As Integer = My.Application.Info.Version.Minor
        Dim Build As Integer = My.Application.Info.Version.Build
        Dim HasSettingsVersion As Boolean = False
        Try
            Dim xMajor As Integer = Val(Root.Attributes("Major").Value)
            Dim xMinor As Integer = Val(Root.Attributes("Minor").Value)
            Dim xBuild As Integer = Val(Root.Attributes("Build").Value)
            Major = xMajor
            Minor = xMinor
            Build = xBuild
            HasSettingsVersion = True
        Catch ex As Exception
        End Try

        'form
        Dim eForm As XmlElement = Root.Item("Form")
        If eForm IsNot Nothing Then
            With eForm
                Select Case Val(.GetAttribute("WindowState"))
                    Case FormWindowState.Normal
                        Me.WindowState = FormWindowState.Normal
                        XMLLoadAttribute(.GetAttribute("Width"), Me.Width)
                        XMLLoadAttribute(.GetAttribute("Height"), Me.Height)
                        XMLLoadAttribute(.GetAttribute("Top"), Me.Top)
                        XMLLoadAttribute(.GetAttribute("Left"), Me.Left)
                    Case FormWindowState.Maximized
                        Me.WindowState = FormWindowState.Maximized
                End Select
            End With
        End If

        'recent
        Dim eRecent As XmlElement = Root.Item("Recent")
        If eRecent IsNot Nothing Then
            With eRecent
                XMLLoadAttribute(.GetAttribute("Recent0"), Recent(0)) : SetRecent(0, Recent(0))
                XMLLoadAttribute(.GetAttribute("Recent1"), Recent(1)) : SetRecent(1, Recent(1))
                XMLLoadAttribute(.GetAttribute("Recent2"), Recent(2)) : SetRecent(2, Recent(2))
                XMLLoadAttribute(.GetAttribute("Recent3"), Recent(3)) : SetRecent(3, Recent(3))
                XMLLoadAttribute(.GetAttribute("Recent4"), Recent(4)) : SetRecent(4, Recent(4))
            End With
        End If

        'edit
        Dim eEdit As XmlElement = Root.Item("Edit")
        If eEdit IsNot Nothing Then
            With eEdit
                XMLLoadAttribute(.GetAttribute("NTInput"), NTInput)
                TBNTInput.Checked = NTInput
                mnNTInput.Checked = NTInput
                POBLong.Enabled = Not NTInput
                POBLongShort.Enabled = Not NTInput

                Dim xLanguage As String = .GetAttribute("Language")
                If xLanguage = "" OrElse Not LoadLocale(My.Application.Info.DirectoryPath & "\" & xLanguage, True, False) Then
                    LoadAutomaticLocale()
                End If

                'XMLLoadAttribute(.GetAttribute("SortingMethod"), SortingMethod)

                XMLLoadAttribute(.GetAttribute("ErrorCheck"), ErrorCheck)
                TBErrorCheck.Checked = ErrorCheck
                TBErrorCheck_Click(TBErrorCheck, New System.EventArgs)

                XMLLoadAttribute(.GetAttribute("ShowFileName"), ShowFileName)
                TBShowFileName.Checked = ShowFileName
                TBShowFileName_Click(TBShowFileName, New System.EventArgs)

                XMLLoadAttribute(.GetAttribute("MiddleButtonMoveMethod"), MiddleButtonMoveMethod)
                XMLLoadAttribute(.GetAttribute("AutoFocusMouseEnter"), AutoFocusMouseEnter)
                XMLLoadAttribute(.GetAttribute("FirstClickDisabled"), FirstClickDisabled)

                XMLLoadAttribute(.GetAttribute("AutoSaveInterval"), AutoSaveInterval)
                If AutoSaveInterval Then AutoSaveTimer.Interval = AutoSaveInterval Else AutoSaveTimer.Enabled = False

                XMLLoadAttribute(.GetAttribute("PreviewOnClick"), PreviewOnClick)
                TBPreviewOnClick.Checked = PreviewOnClick
                TBPreviewOnClick_Click(TBPreviewOnClick, New System.EventArgs)

                XMLLoadAttribute(.GetAttribute("ChangePlaySide"), Rscratch)
                TBChangePlaySide.Checked = Rscratch
                TBChangePlaySide_Click(TBChangePlaySide, New System.EventArgs)

                XMLLoadAttribute(.GetAttribute("SyncSplitViewScroll"), SyncSplitViewScroll)
                SetSplitViewScrollSync(SyncSplitViewScroll)

                XMLLoadAttribute(.GetAttribute("ClickStopPreview"), ClickStopPreview)
                XMLLoadAttribute(.GetAttribute("SkipClippedMeasure"), SkipClippedMeasure)
                XMLLoadAttribute(.GetAttribute("LaneHighlight"), LaneHighlight)
                LaneHighlight = Math.Min(100, Math.Max(0, LaneHighlight))
                XMLLoadAttribute(.GetAttribute("UndoRedoMemoryLimitMB"), UndoRedoMemoryLimitMB)
                NormalizeUndoRedoMemoryLimit()
                EnforceUndoRedoHistoryLimit()
            End With
        End If

        'save
        Dim eSave As XmlElement = Root.Item("Save")
        If eSave IsNot Nothing Then
            With eSave
                If Not HasSettingsVersion OrElse IsSettingsBeforeVersion(Major, Minor, Build, 5, 1, 0) Then
                    InputTextEncoding = TextEncodingMode.Auto
                    OutputTextEncoding = TextEncodingMode.SystemDefault
                Else
                    InputTextEncoding = LoadInputTextEncodingMode(eSave)
                    OutputTextEncoding = LoadOutputTextEncodingMode(eSave)
                End If

                XMLLoadAttribute(.GetAttribute("BMSGridLimit"), BMSGridLimit)
                XMLLoadAttribute(.GetAttribute("BeepWhileSaved"), BeepWhileSaved)
                XMLLoadAttribute(.GetAttribute("NewBMSUseBase62Definitions"), NewBMSUseBase62Definitions)

                If .GetAttribute("BPMDefinitionMode").Length > 0 Then
                    XMLLoadAttribute(.GetAttribute("BPMDefinitionMode"), BPMDefinitionMode)
                Else
                    Dim xBPMx As Boolean = False
                    XMLLoadAttribute(.GetAttribute("BPMx1296"), xBPMx)
                    If xBPMx Then BPMDefinitionMode = DefinitionModeBase36
                End If

                If .GetAttribute("STOPDefinitionMode").Length > 0 Then
                    XMLLoadAttribute(.GetAttribute("STOPDefinitionMode"), STOPDefinitionMode)
                Else
                    Dim xSTOPx As Boolean = False
                    XMLLoadAttribute(.GetAttribute("STOPx1296"), xSTOPx)
                    If xSTOPx Then STOPDefinitionMode = DefinitionModeBase36
                End If
            End With
        End If

        'update
        Dim eUpdate As XmlElement = Root.Item("Update")
        If eUpdate IsNot Nothing Then
            XMLLoadAttribute(eUpdate.GetAttribute("SkippedTag"), SkippedUpdateTag)
            XMLLoadAttribute(eUpdate.GetAttribute("CheckOnStartup"), CheckUpdatesOnStartup)
            mnUpdateStartup.Checked = CheckUpdatesOnStartup
        End If

        'WAV
        Dim eWAV As XmlElement = Root.Item("WAV")
        If eWAV IsNot Nothing Then
            With eWAV
                XMLLoadAttribute(.GetAttribute("WAVMultiSelect"), WAVMultiSelect)
                CWAVMultiSelect.Checked = WAVMultiSelect
                CWAVMultiSelect_CheckedChanged(CWAVMultiSelect, New EventArgs)

                XMLLoadAttribute(.GetAttribute("WAVChangeLabel"), WAVChangeLabel)
                CWAVChangeLabel.Checked = WAVChangeLabel
                CWAVChangeLabel_CheckedChanged(CWAVChangeLabel, New EventArgs)

                XMLLoadAttribute(.GetAttribute("WAVEmptyfill"), WAVEmptyfill)
                CWAVEmptyfill.Checked = WAVEmptyfill
                CWAVEmptyfill_CheckedChanged(CWAVEmptyfill, New EventArgs)

                Dim xInt As Integer = CInt(.GetAttribute("BeatChangeMode"))
                Dim xBeatOpList As RadioButton() = {CBeatPreserve, CBeatMeasure, CBeatCut, CBeatScale}
                If xInt >= 0 And xInt < xBeatOpList.Length Then
                    xBeatOpList(xInt).Checked = True
                    CBeatPreserve_Click(xBeatOpList(xInt), New System.EventArgs)
                End If
            End With
        End If

        'ShowHide
        Dim eShowHide As XmlElement = Root.Item("ShowHide")
        If eShowHide IsNot Nothing Then
            With eShowHide
                XMLLoadAttribute(.GetAttribute("showMenu"), mnSMenu.Checked)
                XMLLoadAttribute(.GetAttribute("showTB"), mnSTB.Checked)
                XMLLoadAttribute(.GetAttribute("showOpPanel"), mnSOP.Checked)
                XMLLoadAttribute(.GetAttribute("showStatus"), mnSStatus.Checked)
                If .HasAttribute("rightSplitRatios") Then LoadSplitPanelRatiosSetting(.GetAttribute("rightSplitRatios"))
            End With
        End If

        'Grid
        Dim eGrid As XmlElement = Root.Item("Grid")
        If eGrid IsNot Nothing Then
            With eGrid
                XMLLoadAttribute(.GetAttribute("gSnap"), CGSnap.Checked)
                XMLLoadAttribute(.GetAttribute("gDisableVertical"), CGDisableVertical.Checked)
                XMLLoadAttribute(.GetAttribute("gWheel"), gWheel)
                XMLLoadAttribute(.GetAttribute("gPgUpDn"), gPgUpDn)
                XMLLoadAttribute(.GetAttribute("gShow"), CGShow.Checked)
                XMLLoadAttribute(.GetAttribute("gShowS"), CGShowS.Checked)
                XMLLoadAttribute(.GetAttribute("gShowBG"), CGShowBG.Checked)
                XMLLoadAttribute(.GetAttribute("gShowM"), CGShowM.Checked)
                XMLLoadAttribute(.GetAttribute("gShowV"), CGShowV.Checked)
                XMLLoadAttribute(.GetAttribute("gShowMB"), CGShowMB.Checked)
                XMLLoadAttribute(.GetAttribute("gShowC"), CGShowC.Checked)
                XMLLoadAttribute(.GetAttribute("gBPM"), CGBPM.Checked)
                XMLLoadAttribute(.GetAttribute("gSTOP"), CGSTOP.Checked)
                XMLLoadAttribute(.GetAttribute("gSCROLL"), CGSCROLL.Checked)
                XMLLoadAttribute(.GetAttribute("gBLP"), CGBLP.Checked)
                XMLLoadAttribute(.GetAttribute("gP2"), CHPlayer.SelectedIndex)
                XMLLoadAttribute(.GetAttribute("gCol"), CGB.Value)
                XMLLoadAttribute(.GetAttribute("gxHeight"), CGHeight.Value)
                XMLLoadAttribute(.GetAttribute("gxWidth"), CGWidth.Value)
                XMLLoadAttribute(.GetAttribute("gSlash"), gSlash)

                Dim xgDivide As Integer = CInt(.GetAttribute("gDivide"))
                If xgDivide >= CGDivide.Minimum And xgDivide <= CGDivide.Maximum Then CGDivide.Value = xgDivide

                Dim xgSub As Integer = CInt(.GetAttribute("gSub"))
                If xgSub >= CGSub.Minimum And xgSub <= CGSub.Maximum Then CGSub.Value = xgSub
            End With
        End If

        'WaveForm
        Dim eWaveForm As XmlElement = Root.Item("WaveForm")
        If eWaveForm IsNot Nothing Then
            With eWaveForm
                XMLLoadAttribute(.GetAttribute("wLock"), BWLock.Checked)
                XMLLoadAttribute(.GetAttribute("wPosition"), TWPosition.Value)
                XMLLoadAttribute(.GetAttribute("wLeft"), TWLeft.Value)
                XMLLoadAttribute(.GetAttribute("wWidth"), TWWidth.Value)
                XMLLoadAttribute(.GetAttribute("wPrecision"), TWPrecision.Value)
            End With
        End If

        'Player
        Dim ePlayer As XmlElement = Root.Item("Player")
        If ePlayer IsNot Nothing Then
            With ePlayer
                XMLLoadAttribute(.GetAttribute("CurrentPlayer"), CurrentPlayer)

                Dim xCount As Integer = .GetAttribute("Count")
                If xCount > 0 Then ReDim Preserve pArgs(xCount - 1)
            End With

            For Each eePlayer As XmlElement In ePlayer.ChildNodes
                Me.XMLLoadPlayer(eePlayer)
            Next
        End If

        Dim eTheme As XmlElement = Root.Item("Theme")
        If eTheme IsNot Nothing AndAlso eTheme.HasAttribute("Path") Then xThemePath = eTheme.GetAttribute("Path")
        If LoadThemeOrDefault(xThemePath) Then ChangePlaySideSkin(False)

        Dim eVisualOptions As XmlElement = Root.Item("VisualOptions")
        If eVisualOptions IsNot Nothing Then
            XMLLoadElementValue(eVisualOptions.Item("TSDeltaMouseOver"), vo.PEDeltaMouseOver)
            XMLLoadElementValue(eVisualOptions.Item("MiddleDeltaRelease"), vo.MiddleDeltaRelease)
        Else
            Dim eOldVisualSettings As XmlElement = Root.Item("VisualSettings")
            If eOldVisualSettings IsNot Nothing Then
                XMLLoadElementValue(eOldVisualSettings.Item("TSDeltaMouseOver"), vo.PEDeltaMouseOver)
                XMLLoadElementValue(eOldVisualSettings.Item("MiddleDeltaRelease"), vo.MiddleDeltaRelease)
            End If
        End If

EndOfSub:
        CalculateGreatestColumn()
        FileStream.Close()
        'File.Delete(xTempFileName)
    End Sub

    Private Sub XMLLoadLocaleMenu(ByVal n As XmlElement, ByRef target As String)
        If n Is Nothing Then Exit Sub
        target = n.InnerText
    End Sub

    Private Sub XMLLoadLocale(ByVal n As XmlElement, ByRef target As String)
        If n IsNot Nothing Then target = n.InnerText
    End Sub

    Private Sub XMLLoadLocaleToolTipUniversal(ByVal n As XmlElement, ByVal target As Control)
        If n Is Nothing Then Exit Sub
        ToolTipUniversal.SetToolTip(target, n.InnerText)
    End Sub

    Private Function AutomaticLocalePath() As String
        Dim xCultureName As String = Globalization.CultureInfo.CurrentUICulture.Name
        Dim xLanguageName As String = Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName

        Select Case xLanguageName
            Case "ja"
                Return My.Application.Info.DirectoryPath & "\Language\jpn.xml"
            Case "ko"
                Return My.Application.Info.DirectoryPath & "\Language\kor.xml"
            Case "zh"
                If xCultureName.Equals("zh-Hans", StringComparison.OrdinalIgnoreCase) OrElse
                   xCultureName.Equals("zh-CN", StringComparison.OrdinalIgnoreCase) OrElse
                   xCultureName.Equals("zh-SG", StringComparison.OrdinalIgnoreCase) Then
                    Return My.Application.Info.DirectoryPath & "\Language\chs.xml"
                End If
        End Select

        Return ""
    End Function

    Private Function LoadAutomaticLocale() As Boolean
        DispLang = ""

        Dim xPath As String = AutomaticLocalePath()
        If xPath = "" Then Return False

        Return LoadLocale(xPath, False, False)
    End Function

    Private Function LoadLocale(ByVal Path As String, Optional ByVal SaveSelection As Boolean = True, Optional ByVal ShowError As Boolean = True) As Boolean
        If Not My.Computer.FileSystem.FileExists(Path) Then Return False

        Dim Doc As XmlDocument = Nothing
        Dim FileStream As IO.FileStream = Nothing

        Dim xPOGridPart2 As Boolean = POGridPart2.Visible
        POHeaderPart2.Visible = True
        POGridPart2.Visible = True
        POWaveFormPart2.Visible = True

        Try
            Doc = New XmlDocument
            FileStream = New IO.FileStream(Path, FileMode.Open, FileAccess.Read)
            Doc.Load(FileStream)

            Dim Root As XmlElement = Doc.Item("iBMSC.Locale")
            If Root Is Nothing Then Throw New NullReferenceException

            XMLLoadLocale(Root.Item("OK"), Strings.OK)
            XMLLoadLocale(Root.Item("Cancel"), Strings.Cancel)
            XMLLoadLocale(Root.Item("None"), Strings.None)

            Dim eFont As XmlElement = Root.Item("Font")
            If eFont IsNot Nothing Then
                Dim xSize As Integer = 9
                If eFont.HasAttribute("Size") Then xSize = Val(eFont.GetAttribute("Size"))

                Dim fRegular As New Font(Me.Font.FontFamily, xSize, FontStyle.Regular)
                Dim xChildNode As XmlNode = eFont.LastChild
                Do While xChildNode IsNot Nothing
                    If xChildNode.LocalName <> "Family" Then Continue Do
                    If isFontInstalled(xChildNode.InnerText) Then
                        fRegular = New Font(xChildNode.InnerText, xSize)
                    End If
                    xChildNode = xChildNode.PreviousSibling
                Loop

                Dim rList() As Object = {Me, mnSys, Menu1, mnMain, cmnLanguage, cmnTheme, cmnConversion, TBMain, FStatus, FStatus2}
                For Each c As Object In rList
                    Try
                        c.Font = fRegular
                    Catch ex As Exception
                    End Try
                Next

                Dim fBold As New Font(fRegular, FontStyle.Bold)

                Dim bList() As Object = {TBStatistics, FSSS, FSSL, FSSH, TVCM, TVCD, TVCBPM, FSP1, FSP3, FSP2, PMain, PMainIn, PMainR, PMainInR, PMainL, PMainInL}
                For Each c As Object In bList
                    Try
                        c.Font = fBold
                    Catch ex As Exception
                    End Try
                Next
                ApplySplitPaneFont(fBold)
            End If

            Dim eMonoFont As XmlElement = Root.Item("MonoFont")
            If eMonoFont IsNot Nothing Then
                Dim xSize As Integer = 9
                If eMonoFont.HasAttribute("Size") Then xSize = Val(eMonoFont.GetAttribute("Size"))

                Dim fMono As New Font(POWAVInner.Font.FontFamily, xSize)
                Dim xChildNode As XmlNode = eMonoFont.LastChild
                Do While xChildNode IsNot Nothing
                    If xChildNode.LocalName <> "Family" Then Continue Do
                    If isFontInstalled(xChildNode.InnerText) Then
                        fMono = New Font(xChildNode.InnerText, xSize)
                    End If
                    xChildNode = xChildNode.PreviousSibling
                Loop

                Dim mList() As Object = {LWAV, LBMP, LBeat, TExpansion}
                For Each c As Object In mList
                    Try
                        c.font = fMono
                    Catch ex As Exception
                    End Try
                Next
            End If

            Dim eMenu As XmlElement = Root.Item("Menu")
            If eMenu IsNot Nothing Then

                Dim eFile As XmlElement = eMenu.Item("File")
                If eFile IsNot Nothing Then
                    XMLLoadLocaleMenu(eFile.Item("Title"), mnFile.Text)
                    XMLLoadLocaleMenu(eFile.Item("New"), mnNew.Text)
                    XMLLoadLocaleMenu(eFile.Item("Open"), mnOpen.Text)
                    XMLLoadLocaleMenu(eFile.Item("ImportNBMSC"), mnImportNBMSC.Text)
                    XMLLoadLocaleMenu(eFile.Item("Save"), mnSave.Text)
                    XMLLoadLocaleMenu(eFile.Item("SaveAs"), mnSaveAs.Text)
                    XMLLoadLocaleMenu(eFile.Item("ExportNBMSC"), mnExportNBMSC.Text)
                    XMLLoadLocaleMenu(eFile.Item("ExportBMSON"), mnExportBMSON.Text)
                    If Recent(0) = "" Then XMLLoadLocaleMenu(eFile.Item("Recent0"), mnOpenR0.Text)
                    If Recent(1) = "" Then XMLLoadLocaleMenu(eFile.Item("Recent1"), mnOpenR1.Text)
                    If Recent(2) = "" Then XMLLoadLocaleMenu(eFile.Item("Recent2"), mnOpenR2.Text)
                    If Recent(3) = "" Then XMLLoadLocaleMenu(eFile.Item("Recent3"), mnOpenR3.Text)
                    If Recent(4) = "" Then XMLLoadLocaleMenu(eFile.Item("Recent4"), mnOpenR4.Text)
                    XMLLoadLocaleMenu(eFile.Item("Quit"), mnQuit.Text)
                End If

                Dim eEdit As XmlElement = eMenu.Item("Edit")
                If eEdit IsNot Nothing Then
                    XMLLoadLocaleMenu(eEdit.Item("Title"), mnEdit.Text)
                    XMLLoadLocaleMenu(eEdit.Item("Undo"), mnUndo.Text)
                    XMLLoadLocaleMenu(eEdit.Item("Redo"), mnRedo.Text)
                    XMLLoadLocaleMenu(eEdit.Item("Cut"), mnCut.Text)
                    XMLLoadLocaleMenu(eEdit.Item("Copy"), mnCopy.Text)
                    XMLLoadLocaleMenu(eEdit.Item("Paste"), mnPaste.Text)
                    XMLLoadLocaleMenu(eEdit.Item("Delete"), mnDelete.Text)
                    XMLLoadLocaleMenu(eEdit.Item("SelectAll"), mnSelectAll.Text)
                    XMLLoadLocaleMenu(eEdit.Item("GoToMeasure"), mnGotoMeasure.Text)
                    XMLLoadLocaleMenu(eEdit.Item("Find"), mnFind.Text)
                    XMLLoadLocaleMenu(eEdit.Item("Stat"), mnStatistics.Text)
                    XMLLoadLocaleMenu(eEdit.Item("TimeSelectionTool"), mnTimeSelect.Text)
                    XMLLoadLocaleMenu(eEdit.Item("SelectTool"), mnSelect.Text)
                    XMLLoadLocaleMenu(eEdit.Item("WriteTool"), mnWrite.Text)
                End If

                Dim eView As XmlElement = eMenu.Item("View")
                If eView IsNot Nothing Then
                    XMLLoadLocaleMenu(eView.Item("Title"), mnSys.Text)
                End If

                Dim eOptions As XmlElement = eMenu.Item("Options")
                If eOptions IsNot Nothing Then
                    XMLLoadLocaleMenu(eOptions.Item("Title"), mnOptions.Text)
                    XMLLoadLocaleMenu(eOptions.Item("NT"), mnNTInput.Text)
                    XMLLoadLocaleMenu(eOptions.Item("ErrorCheck"), mnErrorCheck.Text)
                    XMLLoadLocaleMenu(eOptions.Item("PreviewOnClick"), mnPreviewOnClick.Text)
                    XMLLoadLocaleMenu(eOptions.Item("ShowFileName"), mnShowFileName.Text)
                    XMLLoadLocaleMenu(eOptions.Item("ChangePlaySide"), mnChangePlaySide.Text)
                    XMLLoadLocaleMenu(eOptions.Item("WavIncrease"), mnWavIncrease.Text)
                    XMLLoadLocaleMenu(eOptions.Item("SyncSplitViewScroll"), mnSyncSplitViewScroll.Text)
                    XMLLoadLocaleMenu(eOptions.Item("SlashGrid"), mnSlashGrid.Text)
                    XMLLoadLocaleMenu(eOptions.Item("GeneralOptions"), mnGOptions.Text)
                    XMLLoadLocaleMenu(eOptions.Item("PlayerOptions"), mnPOptions.Text)
                    XMLLoadLocaleMenu(eOptions.Item("Language"), mnLanguage.Text)
                    XMLLoadLocaleMenu(eOptions.Item("Theme"), mnTheme.Text)
                End If

                XMLLoadLocaleMenu(eMenu.Item("Conversion"), mnConversion.Text)

                Dim ePreview As XmlElement = eMenu.Item("Preview")
                If ePreview IsNot Nothing Then
                    XMLLoadLocaleMenu(ePreview.Item("Title"), mnPreview.Text)
                    XMLLoadLocaleMenu(ePreview.Item("PlayBegin"), mnPlayB.Text)
                    XMLLoadLocaleMenu(ePreview.Item("PlayHere"), mnPlay.Text)
                    XMLLoadLocaleMenu(ePreview.Item("PlayStop"), mnStop.Text)
                End If

                Dim eHelp As XmlElement = eMenu.Item("Help")
                If eHelp IsNot Nothing Then
                    XMLLoadLocaleMenu(eHelp.Item("Title"), mnHelp.Text)
                    XMLLoadLocaleMenu(eHelp.Item("OpenAppFolder"), mnOpenAppFolder.Text)
                    XMLLoadLocaleMenu(eHelp.Item("CheckUpdates"), mnUpdate.Text)
                    XMLLoadLocaleMenu(eHelp.Item("CheckUpdatesOnStartup"), mnUpdateStartup.Text)
                End If
            End If

            Dim eToolBar As XmlElement = Root.Item("ToolBar")
            If eToolBar IsNot Nothing Then
                XMLLoadLocale(eToolBar.Item("New"), TBNew.Text)
                XMLLoadLocale(eToolBar.Item("Open"), TBOpen.Text)
                XMLLoadLocale(eToolBar.Item("Save"), TBSave.Text)
                XMLLoadLocale(eToolBar.Item("Cut"), TBCut.Text)
                XMLLoadLocale(eToolBar.Item("Copy"), TBCopy.Text)
                XMLLoadLocale(eToolBar.Item("Paste"), TBPaste.Text)
                XMLLoadLocale(eToolBar.Item("Find"), TBFind.Text)
                XMLLoadLocale(eToolBar.Item("Stat"), TBStatistics.ToolTipText)
                XMLLoadLocale(eToolBar.Item("Conversion"), POConvert.Text)
                XMLLoadLocale(eToolBar.Item("ErrorCheck"), TBErrorCheck.Text)
                XMLLoadLocale(eToolBar.Item("PreviewOnClick"), TBPreviewOnClick.Text)
                XMLLoadLocale(eToolBar.Item("ShowFileName"), TBShowFileName.Text)
                XMLLoadLocale(eToolBar.Item("ChangePlaySide"), TBChangePlaySide.Text)
                XMLLoadLocale(eToolBar.Item("AddSplitView"), TBAddSplitView.Text)
                XMLLoadLocale(eToolBar.Item("RemoveSplitView"), TBRemoveSplitView.Text)
                XMLLoadLocale(eToolBar.Item("SyncSplitViewScroll"), TBSyncSplitViewScroll.Text)
                XMLLoadLocale(eToolBar.Item("Undo"), TBUndo.Text)
                XMLLoadLocale(eToolBar.Item("Redo"), TBRedo.Text)
                XMLLoadLocale(eToolBar.Item("NT"), TBNTInput.Text)
                XMLLoadLocale(eToolBar.Item("WavIncrease"), TBWavIncrease.Text)
                XMLLoadLocale(eToolBar.Item("WavIncrease"), mnWavIncrease.Text)
                XMLLoadLocale(eToolBar.Item("TimeSelectionTool"), TBTimeSelect.Text)
                XMLLoadLocale(eToolBar.Item("SelectTool"), TBSelect.Text)
                XMLLoadLocale(eToolBar.Item("WriteTool"), TBWrite.Text)
                XMLLoadLocale(eToolBar.Item("PlayBegin"), TBPlayB.Text)
                XMLLoadLocale(eToolBar.Item("PlayHere"), TBPlay.Text)
                XMLLoadLocale(eToolBar.Item("PlayStop"), TBStop.Text)
                XMLLoadLocale(eToolBar.Item("Language"), TBLanguage.Text)
                XMLLoadLocale(eToolBar.Item("Theme"), TBTheme.Text)
                ' XMLLoadLocale(eToolBar.Item("About"), TBAbout.Text)
            End If

            Dim eStatusBar As XmlElement = Root.Item("StatusBar")
            If eStatusBar IsNot Nothing Then
                XMLLoadLocale(eStatusBar.Item("ColumnCaption"), FSC.ToolTipText)
                XMLLoadLocale(eStatusBar.Item("NoteIndex"), FSW.ToolTipText)
                XMLLoadLocale(eStatusBar.Item("MeasureIndex"), FSM.ToolTipText)
                XMLLoadLocale(eStatusBar.Item("GridResolution"), FSP1.ToolTipText)
                XMLLoadLocale(eStatusBar.Item("ReducedResolution"), FSP3.ToolTipText)
                XMLLoadLocale(eStatusBar.Item("MeasureResolution"), FSP2.ToolTipText)
                XMLLoadLocale(eStatusBar.Item("AbsolutePosition"), FSP4.ToolTipText)
                XMLLoadLocale(eStatusBar.Item("Length"), Strings.StatusBar.Length)
                XMLLoadLocale(eStatusBar.Item("LongNote"), Strings.StatusBar.LongNote)
                XMLLoadLocale(eStatusBar.Item("Hidden"), Strings.StatusBar.Hidden)
                XMLLoadLocale(eStatusBar.Item("LandMine"), Strings.StatusBar.LandMine)
                XMLLoadLocale(eStatusBar.Item("Error"), Strings.StatusBar.Err)
                XMLLoadLocale(eStatusBar.Item("SelStart"), FSSS.ToolTipText)
                XMLLoadLocale(eStatusBar.Item("SelLength"), FSSL.ToolTipText)
                XMLLoadLocale(eStatusBar.Item("SelSplit"), FSSH.ToolTipText)
                XMLLoadLocale(eStatusBar.Item("Reverse"), BVCReverse.Text)
                XMLLoadLocale(eStatusBar.Item("ByMultiple"), BVCApply.Text)
                XMLLoadLocale(eStatusBar.Item("ByValue"), BVCCalculate.Text)
            End If

            Dim eSubMenu As XmlElement = Root.Item("SubMenu")
            If eSubMenu IsNot Nothing Then

                Dim eShowHide As XmlElement = eSubMenu.Item("ShowHide")
                If eShowHide IsNot Nothing Then
                    'Dim xToolTip As String = ToolTipUniversal.GetToolTip(ttlIcon)
                    'XMLLoadLocaleMenu(eShowHide.Item("ToolTip"), xToolTip)
                    'ToolTipUniversal.SetToolTip(ttlIcon, xToolTip)

                    XMLLoadLocaleMenu(eShowHide.Item("Menu"), mnSMenu.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("ToolBar"), mnSTB.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("OptionsPanel"), mnSOP.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("StatusBar"), mnSStatus.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("AddSplitView"), mnSAddSplitView.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("RemoveSplitView"), mnSRemoveSplitView.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("CloseSplitView"), EditorContextCloseSplitView.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("Grid"), CGShow.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("Sub"), CGShowS.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("BG"), CGShowBG.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("MeasureIndex"), CGShowM.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("MeasureLine"), CGShowMB.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("Vertical"), CGShowV.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("ColumnCaption"), CGShowC.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("BPM"), CGBPM.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("STOP"), CGSTOP.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("SCROLL"), CGSCROLL.Text)
                    XMLLoadLocaleMenu(eShowHide.Item("BLP"), CGBLP.Text)
                End If

                Dim eInsertMeasure As XmlElement = eSubMenu.Item("InsertMeasure")
                If eInsertMeasure IsNot Nothing Then
                    XMLLoadLocaleMenu(eInsertMeasure.Item("Insert"), MInsert.Text)
                    XMLLoadLocaleMenu(eInsertMeasure.Item("Remove"), MRemove.Text)
                End If

                Dim eLanguage As XmlElement = eSubMenu.Item("Language")
                If eLanguage IsNot Nothing Then
                    XMLLoadLocaleMenu(eLanguage.Item("Default"), TBLangDef.Text)
                    XMLLoadLocaleMenu(eLanguage.Item("Refresh"), TBLangRefresh.Text)
                End If

                Dim eTheme As XmlElement = eSubMenu.Item("Theme")
                If eTheme IsNot Nothing Then
                    XMLLoadLocaleMenu(eTheme.Item("Default"), TBThemeDef.Text)
                    XMLLoadLocaleMenu(eTheme.Item("Save"), TBThemeSave.Text)
                    XMLLoadLocaleMenu(eTheme.Item("Refresh"), TBThemeRefresh.Text)
                End If

                Dim eConvert As XmlElement = eSubMenu.Item("Convert")
                If eConvert IsNot Nothing Then
                    XMLLoadLocaleMenu(eConvert.Item("Long"), POBLong.Text)
                    XMLLoadLocaleMenu(eConvert.Item("Short"), POBShort.Text)
                    XMLLoadLocaleMenu(eConvert.Item("LongShort"), POBLongShort.Text)
                    XMLLoadLocaleMenu(eConvert.Item("Hidden"), POBHidden.Text)
                    XMLLoadLocaleMenu(eConvert.Item("Visible"), POBVisible.Text)
                    XMLLoadLocaleMenu(eConvert.Item("HiddenVisible"), POBHiddenVisible.Text)
                    XMLLoadLocaleMenu(eConvert.Item("Landmine"), POBLandmine.Text)
                    XMLLoadLocaleMenu(eConvert.Item("NormalLandmine"), POBNormalLandmine.Text)
                    XMLLoadLocaleMenu(eConvert.Item("Relabel"), POBModify.Text)
                    XMLLoadLocaleMenu(eConvert.Item("Mirror"), POBMirror.Text)
                End If

                Dim eWAV As XmlElement = eSubMenu.Item("WAV")
                If eWAV IsNot Nothing Then
                    XMLLoadLocaleMenu(eWAV.Item("Base62"), CWAVBase62.Text)
                    XMLLoadLocaleMenu(eWAV.Item("Base62"), CBMPBase62.Text)
                    XMLLoadLocaleMenu(eWAV.Item("MultiSelection"), CWAVMultiSelect.Text)
                    XMLLoadLocaleMenu(eWAV.Item("Synchronize"), CWAVChangeLabel.Text)
                    XMLLoadLocaleMenu(eWAV.Item("Emptyfill"), CWAVEmptyfill.Text)
                End If

                Dim eBeat As XmlElement = eSubMenu.Item("Beat")
                If eBeat IsNot Nothing Then
                    XMLLoadLocaleMenu(eBeat.Item("Absolute"), CBeatPreserve.Text)
                    XMLLoadLocaleMenu(eBeat.Item("Measure"), CBeatMeasure.Text)
                    XMLLoadLocaleMenu(eBeat.Item("Cut"), CBeatCut.Text)
                    XMLLoadLocaleMenu(eBeat.Item("Scale"), CBeatScale.Text)
                End If
            End If

            Dim eOptionsPanel As XmlElement = Root.Item("OptionsPanel")
            If eOptionsPanel IsNot Nothing Then

                Dim eHeader As XmlElement = eOptionsPanel.Item("Header")
                If eHeader IsNot Nothing Then
                    XMLLoadLocale(eHeader.Item("Header"), POHeaderTabButton.Text)
                    XMLLoadLocale(eHeader.Item("Title"), Label3.Text)
                    XMLLoadLocale(eHeader.Item("Artist"), Label4.Text)
                    XMLLoadLocale(eHeader.Item("Genre"), Label2.Text)
                    XMLLoadLocale(eHeader.Item("BPM"), Label9.Text)
                    XMLLoadLocale(eHeader.Item("Player"), Label8.Text)
                    XMLLoadLocale(eHeader.Item("Rank"), Label10.Text)
                    XMLLoadLocale(eHeader.Item("PlayLevel"), Label6.Text)
                    XMLLoadLocale(eHeader.Item("SubTitle"), Label15.Text)
                    XMLLoadLocale(eHeader.Item("SubArtist"), Label17.Text)
                    'XMLLoadLocale(eHeader.Item("Maker"), Label14.Text)
                    XMLLoadLocale(eHeader.Item("StageFile"), Label16.Text)
                    XMLLoadLocale(eHeader.Item("Banner"), Label12.Text)
                    XMLLoadLocale(eHeader.Item("BackBMP"), Label11.Text)
                    'XMLLoadLocale(eHeader.Item("MidiFile"), Label18.Text)
                    XMLLoadLocale(eHeader.Item("Difficulty"), Label21.Text)
                    XMLLoadLocale(eHeader.Item("ExRank"), Label23.Text)
                    XMLLoadLocale(eHeader.Item("Total"), Label20.Text)
                    XMLLoadLocale(eHeader.Item("RecommendedTotal"), LRecommendedTotalCaption.Text)
                    UpdateRecommendedTotal()
                    'XMLLoadLocale(eHeader.Item("VolWav"), Label22.Text)
                    XMLLoadLocale(eHeader.Item("Comment"), Label19.Text)
                    'XMLLoadLocale(eHeader.Item("LnType"), Label13.Text)
                    XMLLoadLocale(eHeader.Item("LnObj"), Label24.Text)
                    XMLLoadLocale(eHeader.Item("LandMine"), Label26.Text)
                    XMLLoadLocale(eHeader.Item("MissBMP"), Label27.Text)
                    XMLLoadLocale(eHeader.Item("Preview"), Label28.Text)
                    XMLLoadLocale(eHeader.Item("LnMode"), Label29.Text)

                    RemoveHandler CHPlayer.SelectedIndexChanged, AddressOf CHPlayer_SelectedIndexChanged
                    XMLLoadLocale(eHeader.Item("Player1"), CHPlayer.Items.Item(0))
                    XMLLoadLocale(eHeader.Item("Player3"), CHPlayer.Items.Item(1))
                    XMLLoadLocale(eHeader.Item("Player2"), CHPlayer.Items.Item(2))
                    AddHandler CHPlayer.SelectedIndexChanged, AddressOf CHPlayer_SelectedIndexChanged

                    RemoveHandler CHRank.SelectedIndexChanged, AddressOf THGenre_TextChanged
                    XMLLoadLocale(eHeader.Item("Rank0"), CHRank.Items.Item(0))
                    XMLLoadLocale(eHeader.Item("Rank1"), CHRank.Items.Item(1))
                    XMLLoadLocale(eHeader.Item("Rank2"), CHRank.Items.Item(2))
                    XMLLoadLocale(eHeader.Item("Rank3"), CHRank.Items.Item(3))
                    XMLLoadLocale(eHeader.Item("Rank4"), CHRank.Items.Item(4))
                    AddHandler CHRank.SelectedIndexChanged, AddressOf THGenre_TextChanged

                    RemoveHandler CHDifficulty.SelectedIndexChanged, AddressOf THGenre_TextChanged
                    XMLLoadLocale(eHeader.Item("Difficulty0"), CHDifficulty.Items.Item(0))
                    XMLLoadLocale(eHeader.Item("Difficulty1"), CHDifficulty.Items.Item(1))
                    XMLLoadLocale(eHeader.Item("Difficulty2"), CHDifficulty.Items.Item(2))
                    XMLLoadLocale(eHeader.Item("Difficulty3"), CHDifficulty.Items.Item(3))
                    XMLLoadLocale(eHeader.Item("Difficulty4"), CHDifficulty.Items.Item(4))
                    XMLLoadLocale(eHeader.Item("Difficulty5"), CHDifficulty.Items.Item(5))
                    AddHandler CHDifficulty.SelectedIndexChanged, AddressOf THGenre_TextChanged
                End If

                Dim eGrid As XmlElement = eOptionsPanel.Item("Grid")
                If eGrid IsNot Nothing Then
                    XMLLoadLocale(eGrid.Item("Title"), POGridSwitch.Text)
                    XMLLoadLocale(eGrid.Item("Snap"), CGSnap.Text)
                    XMLLoadLocale(eGrid.Item("BCols"), Label1.Text)
                    XMLLoadLocale(eGrid.Item("DisableVertical"), CGDisableVertical.Text)
                    RefreshDisableVerticalToolbar()
                    RefreshGridSnapToolbar()
                End If

                Dim eWaveForm As XmlElement = eOptionsPanel.Item("WaveForm")
                If eWaveForm IsNot Nothing Then
                    XMLLoadLocale(eWaveForm.Item("Title"), POWaveFormTabButton.Text)
                    XMLLoadLocaleToolTipUniversal(eWaveForm.Item("Load"), BWLoad)
                    XMLLoadLocaleToolTipUniversal(eWaveForm.Item("Clear"), BWClear)
                    XMLLoadLocaleToolTipUniversal(eWaveForm.Item("Lock"), BWLock)
                End If

                Dim eWAV As XmlElement = eOptionsPanel.Item("WAV")
                If eWAV IsNot Nothing Then
                    XMLLoadLocale(eWAV.Item("Title"), POWAVTabButton.Text)
                End If

                Dim eBMP As XmlElement = eOptionsPanel.Item("BMP")
                If eBMP IsNot Nothing Then
                    XMLLoadLocale(eBMP.Item("Title"), POBMPTabButton.Text)
                End If

                XMLLoadLocale(eOptionsPanel.Item("Beat"), POBeatTabButton.Text)
                XMLLoadLocale(eOptionsPanel.Item("Beat.Apply"), BBeatApply.Text)
                XMLLoadLocale(eOptionsPanel.Item("Beat.Apply"), BBeatApplyV.Text)
                XMLLoadLocale(eOptionsPanel.Item("Expansion"), POHeaderExpansionSeparatorLabel.Text)
                SyncOptionsTabTitles()
            End If

            Dim eMessages As XmlElement = Root.Item("Messages")
            If eMessages IsNot Nothing Then
                XMLLoadLocale(eMessages.Item("Err"), Strings.Messages.Err)
                XMLLoadLocale(eMessages.Item("SaveOnExit"), Strings.Messages.SaveOnExit)
                XMLLoadLocale(eMessages.Item("SaveOnExit1"), Strings.Messages.SaveOnExit1)
                XMLLoadLocale(eMessages.Item("SaveOnExit2"), Strings.Messages.SaveOnExit2)
                XMLLoadLocale(eMessages.Item("PromptEnter"), Strings.Messages.PromptEnter)
                XMLLoadLocale(eMessages.Item("PromptEnterNumeric"), Strings.Messages.PromptEnterNumeric)
                XMLLoadLocale(eMessages.Item("PromptEnterMeasure"), Strings.Messages.PromptEnterMeasure)
                XMLLoadLocale(eMessages.Item("GoToMeasureTitle"), Strings.Messages.GoToMeasureTitle)
                XMLLoadLocale(eMessages.Item("PromptEnterBPM"), Strings.Messages.PromptEnterBPM)
                XMLLoadLocale(eMessages.Item("PromptEnterSTOP"), Strings.Messages.PromptEnterSTOP)
                XMLLoadLocale(eMessages.Item("PromptEnterSCROLL"), Strings.Messages.PromptEnterSCROLL)
                XMLLoadLocale(eMessages.Item("PromptSlashValue"), Strings.Messages.PromptSlashValue)
                XMLLoadLocale(eMessages.Item("InvalidLabel"), Strings.Messages.InvalidLabel)
                XMLLoadLocale(eMessages.Item("CannotFind"), Strings.Messages.CannotFind)
                XMLLoadLocale(eMessages.Item("PleaseRespecifyPath"), Strings.Messages.PleaseRespecifyPath)
                XMLLoadLocale(eMessages.Item("PlayerNotFound"), Strings.Messages.PlayerNotFound)
                XMLLoadLocale(eMessages.Item("PreviewDelError"), Strings.Messages.PreviewDelError)
                XMLLoadLocale(eMessages.Item("NegativeFactorError"), Strings.Messages.NegativeFactorError)
                XMLLoadLocale(eMessages.Item("NegativeDivisorError"), Strings.Messages.NegativeDivisorError)
                XMLLoadLocale(eMessages.Item("PreferencePostpone"), Strings.Messages.PreferencePostpone)
                XMLLoadLocale(eMessages.Item("EraserObsolete"), Strings.Messages.EraserObsolete)
                XMLLoadLocale(eMessages.Item("SaveWarning"), Strings.Messages.SaveWarning)
                XMLLoadLocale(eMessages.Item("NoteOverlapError"), Strings.Messages.NoteOverlapError)
                XMLLoadLocale(eMessages.Item("BPMOverflowError"), Strings.Messages.BPMOverflowError)
                XMLLoadLocale(eMessages.Item("STOPOverflowError"), Strings.Messages.STOPOverflowError)
                XMLLoadLocale(eMessages.Item("SCROLLOverflowError"), Strings.Messages.SCROLLOverflowError)
                XMLLoadLocale(eMessages.Item("SavedFileWillContainErrors"), Strings.Messages.SavedFileWillContainErrors)
                XMLLoadLocale(eMessages.Item("FileAssociationPrompt"), Strings.Messages.FileAssociationPrompt)
                XMLLoadLocale(eMessages.Item("FileAssociationError"), Strings.Messages.FileAssociationError)
                XMLLoadLocale(eMessages.Item("RestoreDefaultSettings"), Strings.Messages.RestoreDefaultSettings)
                XMLLoadLocale(eMessages.Item("RestoreAutosavedFile"), Strings.Messages.RestoreAutosavedFile)
                XMLLoadLocale(eMessages.Item("UpdateCheckTitle"), Strings.Messages.UpdateCheckTitle)
                XMLLoadLocale(eMessages.Item("UpdateAvailable"), Strings.Messages.UpdateAvailable)
                XMLLoadLocale(eMessages.Item("UpdateLatest"), Strings.Messages.UpdateLatest)
                XMLLoadLocale(eMessages.Item("UpdateCheckFailed"), Strings.Messages.UpdateCheckFailed)
                XMLLoadLocale(eMessages.Item("UpdateVersionUnsupported"), Strings.Messages.UpdateVersionUnsupported)
                XMLLoadLocale(eMessages.Item("UpdateOpenRelease"), Strings.Messages.UpdateOpenRelease)
                XMLLoadLocale(eMessages.Item("UpdateLater"), Strings.Messages.UpdateLater)
                XMLLoadLocale(eMessages.Item("UpdateSkipVersion"), Strings.Messages.UpdateSkipVersion)
            End If

            Dim eFileType As XmlElement = Root.Item("FileType")
            If eFileType IsNot Nothing Then
                XMLLoadLocale(eFileType.Item("_all"), Strings.FileType._all)
                XMLLoadLocale(eFileType.Item("_bms"), Strings.FileType._bms)
                XMLLoadLocale(eFileType.Item("BMS"), Strings.FileType.BMS)
                XMLLoadLocale(eFileType.Item("BME"), Strings.FileType.BME)
                XMLLoadLocale(eFileType.Item("BML"), Strings.FileType.BML)
                XMLLoadLocale(eFileType.Item("PMS"), Strings.FileType.PMS)
                XMLLoadLocale(eFileType.Item("TXT"), Strings.FileType.TXT)
                XMLLoadLocale(eFileType.Item("NBMSC"), Strings.FileType.NBMSC)
                XMLLoadLocale(eFileType.Item("XML"), Strings.FileType.XML)
                XMLLoadLocale(eFileType.Item("THEME_XML"), Strings.FileType.THEME_XML)
                XMLLoadLocale(eFileType.Item("_audio"), Strings.FileType._audio)
                XMLLoadLocale(eFileType.Item("_wave"), Strings.FileType._wave)
                XMLLoadLocale(eFileType.Item("WAV"), Strings.FileType.WAV)
                XMLLoadLocale(eFileType.Item("OGG"), Strings.FileType.OGG)
                XMLLoadLocale(eFileType.Item("MP3"), Strings.FileType.MP3)
                XMLLoadLocale(eFileType.Item("FLAC"), Strings.FileType.FLAC)
                XMLLoadLocale(eFileType.Item("MID"), Strings.FileType.MID)
                XMLLoadLocale(eFileType.Item("_image"), Strings.FileType._image)
                XMLLoadLocale(eFileType.Item("_movie"), Strings.FileType._movie)
                XMLLoadLocale(eFileType.Item("BMP"), Strings.FileType.BMP)
                XMLLoadLocale(eFileType.Item("PNG"), Strings.FileType.PNG)
                XMLLoadLocale(eFileType.Item("JPG"), Strings.FileType.JPG)
                XMLLoadLocale(eFileType.Item("GIF"), Strings.FileType.GIF)
                XMLLoadLocale(eFileType.Item("MPG"), Strings.FileType.MPG)
                XMLLoadLocale(eFileType.Item("AVI"), Strings.FileType.AVI)
                XMLLoadLocale(eFileType.Item("MP4"), Strings.FileType.MP4)
                XMLLoadLocale(eFileType.Item("WMV"), Strings.FileType.WMV)
                XMLLoadLocale(eFileType.Item("WEBM"), Strings.FileType.WEBM)
                XMLLoadLocale(eFileType.Item("EXE"), Strings.FileType.EXE)
            End If

            Dim eStatistics As XmlElement = Root.Item("Statistics")
            If eStatistics IsNot Nothing Then
                XMLLoadLocale(eStatistics.Item("Title"), Strings.fStatistics.Title)
                XMLLoadLocale(eStatistics.Item("lBPM"), Strings.fStatistics.lBPM)
                XMLLoadLocale(eStatistics.Item("lSTOP"), Strings.fStatistics.lSTOP)
                XMLLoadLocale(eStatistics.Item("lSCROLL"), Strings.fStatistics.lSCROLL)
                XMLLoadLocale(eStatistics.Item("lA"), Strings.fStatistics.lA)
                XMLLoadLocale(eStatistics.Item("lD"), Strings.fStatistics.lD)
                XMLLoadLocale(eStatistics.Item("lBGM"), Strings.fStatistics.lBGM)
                XMLLoadLocale(eStatistics.Item("lTotal"), Strings.fStatistics.lTotal)
                XMLLoadLocale(eStatistics.Item("lShort"), Strings.fStatistics.lShort)
                XMLLoadLocale(eStatistics.Item("lLong"), Strings.fStatistics.lLong)
                XMLLoadLocale(eStatistics.Item("lLnObj"), Strings.fStatistics.lLnObj)
                XMLLoadLocale(eStatistics.Item("lHidden"), Strings.fStatistics.lHidden)
                XMLLoadLocale(eStatistics.Item("lLandMine"), Strings.fStatistics.lLandMine)
                XMLLoadLocale(eStatistics.Item("lErrors"), Strings.fStatistics.lErrors)
            End If

            Dim ePlayerOptions As XmlElement = Root.Item("PlayerOptions")
            If ePlayerOptions IsNot Nothing Then
                XMLLoadLocale(ePlayerOptions.Item("Title"), Strings.fopPlayer.Title)
                XMLLoadLocale(ePlayerOptions.Item("Add"), Strings.fopPlayer.Add)
                XMLLoadLocale(ePlayerOptions.Item("Remove"), Strings.fopPlayer.Remove)
                XMLLoadLocale(ePlayerOptions.Item("Path"), Strings.fopPlayer.Path)
                XMLLoadLocale(ePlayerOptions.Item("PlayFromBeginning"), Strings.fopPlayer.PlayFromBeginning)
                XMLLoadLocale(ePlayerOptions.Item("PlayFromHere"), Strings.fopPlayer.PlayFromHere)
                XMLLoadLocale(ePlayerOptions.Item("StopPlaying"), Strings.fopPlayer.StopPlaying)
                XMLLoadLocale(ePlayerOptions.Item("References"), Strings.fopPlayer.References)
                XMLLoadLocale(ePlayerOptions.Item("DirectoryOfApp"), Strings.fopPlayer.DirectoryOfApp)
                XMLLoadLocale(ePlayerOptions.Item("CurrMeasure"), Strings.fopPlayer.CurrMeasure)
                XMLLoadLocale(ePlayerOptions.Item("FileName"), Strings.fopPlayer.FileName)
                XMLLoadLocale(ePlayerOptions.Item("RestoreDefault"), Strings.fopPlayer.RestoreDefault)
            End If

            Dim eVisualOptions As XmlElement = Root.Item("VisualOptions")
            If eVisualOptions IsNot Nothing Then
                XMLLoadLocale(eVisualOptions.Item("Title"), Strings.fopVisual.Title)
                XMLLoadLocale(eVisualOptions.Item("Width"), Strings.fopVisual.Width)
                XMLLoadLocale(eVisualOptions.Item("Caption"), Strings.fopVisual.Caption)
                XMLLoadLocale(eVisualOptions.Item("Note"), Strings.fopVisual.Note)
                XMLLoadLocale(eVisualOptions.Item("Label"), Strings.fopVisual.Label)
                XMLLoadLocale(eVisualOptions.Item("LongNote"), Strings.fopVisual.LongNote)
                XMLLoadLocale(eVisualOptions.Item("LongNoteLabel"), Strings.fopVisual.LongNoteLabel)
                XMLLoadLocale(eVisualOptions.Item("Bg"), Strings.fopVisual.Bg)
                XMLLoadLocale(eVisualOptions.Item("ColumnCaption"), Strings.fopVisual.ColumnCaption)
                XMLLoadLocale(eVisualOptions.Item("ColumnCaptionFont"), Strings.fopVisual.ColumnCaptionFont)
                XMLLoadLocale(eVisualOptions.Item("Background"), Strings.fopVisual.Background)
                XMLLoadLocale(eVisualOptions.Item("Grid"), Strings.fopVisual.Grid)
                XMLLoadLocale(eVisualOptions.Item("SubGrid"), Strings.fopVisual.SubGrid)
                XMLLoadLocale(eVisualOptions.Item("VerticalLine"), Strings.fopVisual.VerticalLine)
                XMLLoadLocale(eVisualOptions.Item("MeasureBarLine"), Strings.fopVisual.MeasureBarLine)
                XMLLoadLocale(eVisualOptions.Item("BGMWaveform"), Strings.fopVisual.BGMWaveform)
                XMLLoadLocale(eVisualOptions.Item("NoteHeight"), Strings.fopVisual.NoteHeight)
                XMLLoadLocale(eVisualOptions.Item("NoteLabel"), Strings.fopVisual.NoteLabel)
                XMLLoadLocale(eVisualOptions.Item("MeasureLabel"), Strings.fopVisual.MeasureLabel)
                XMLLoadLocale(eVisualOptions.Item("LabelVerticalShift"), Strings.fopVisual.LabelVerticalShift)
                XMLLoadLocale(eVisualOptions.Item("LabelHorizontalShift"), Strings.fopVisual.LabelHorizontalShift)
                XMLLoadLocale(eVisualOptions.Item("LongNoteLabelHorizontalShift"), Strings.fopVisual.LongNoteLabelHorizontalShift)
                XMLLoadLocale(eVisualOptions.Item("HiddenNoteOpacity"), Strings.fopVisual.HiddenNoteOpacity)
                XMLLoadLocale(eVisualOptions.Item("NoteBorderOnMouseOver"), Strings.fopVisual.NoteBorderOnMouseOver)
                XMLLoadLocale(eVisualOptions.Item("NoteBorderOnSelection"), Strings.fopVisual.NoteBorderOnSelection)
                XMLLoadLocale(eVisualOptions.Item("NoteBorderOnAdjustingLength"), Strings.fopVisual.NoteBorderOnAdjustingLength)
                XMLLoadLocale(eVisualOptions.Item("SelectionBoxBorder"), Strings.fopVisual.SelectionBoxBorder)
                XMLLoadLocale(eVisualOptions.Item("TSCursor"), Strings.fopVisual.TSCursor)
                XMLLoadLocale(eVisualOptions.Item("TSSplitView"), Strings.fopVisual.TSSplitView)
                XMLLoadLocale(eVisualOptions.Item("TSCursorSensitivity"), Strings.fopVisual.TSCursorSensitivity)
                XMLLoadLocale(eVisualOptions.Item("TSMouseOverBorder"), Strings.fopVisual.TSMouseOverBorder)
                XMLLoadLocale(eVisualOptions.Item("TSFill"), Strings.fopVisual.TSFill)
                XMLLoadLocale(eVisualOptions.Item("TSBPM"), Strings.fopVisual.TSBPM)
                XMLLoadLocale(eVisualOptions.Item("TSBPMFont"), Strings.fopVisual.TSBPMFont)
                XMLLoadLocale(eVisualOptions.Item("MiddleSensitivity"), Strings.fopVisual.MiddleSensitivity)
            End If

            Dim eGeneralOptions As XmlElement = Root.Item("GeneralOptions")
            If eGeneralOptions IsNot Nothing Then
                XMLLoadLocale(eGeneralOptions.Item("Title"), Strings.fopGeneral.Title)
                XMLLoadLocale(eGeneralOptions.Item("MouseWheel"), Strings.fopGeneral.MouseWheel)
                XMLLoadLocale(eGeneralOptions.Item("InputTextEncoding"), Strings.fopGeneral.InputTextEncoding)
                XMLLoadLocale(eGeneralOptions.Item("OutputTextEncoding"), Strings.fopGeneral.OutputTextEncoding)
                XMLLoadLocale(eGeneralOptions.Item("PageUpDown"), Strings.fopGeneral.PageUpDown)
                XMLLoadLocale(eGeneralOptions.Item("MiddleButton"), Strings.fopGeneral.MiddleButton)
                XMLLoadLocale(eGeneralOptions.Item("MiddleButtonAuto"), Strings.fopGeneral.MiddleButtonAuto)
                XMLLoadLocale(eGeneralOptions.Item("MiddleButtonDrag"), Strings.fopGeneral.MiddleButtonDrag)
                XMLLoadLocale(eGeneralOptions.Item("AssociateFileType"), Strings.fopGeneral.AssociateFileType)
                XMLLoadLocale(eGeneralOptions.Item("MaxGridPartition"), Strings.fopGeneral.MaxGridPartition)
                XMLLoadLocale(eGeneralOptions.Item("BeepWhileSaved"), Strings.fopGeneral.BeepWhileSaved)
                XMLLoadLocale(eGeneralOptions.Item("NewBMSUseBase62"), Strings.fopGeneral.NewBMSUseBase62)
                XMLLoadLocale(eGeneralOptions.Item("BPMDefinitionMode"), Strings.fopGeneral.BPMDefinitionMode)
                XMLLoadLocale(eGeneralOptions.Item("STOPDefinitionMode"), Strings.fopGeneral.STOPDefinitionMode)
                XMLLoadLocale(eGeneralOptions.Item("DefinitionModeDefault"), Strings.fopGeneral.DefinitionModeDefault)
                XMLLoadLocale(eGeneralOptions.Item("DefinitionModeBase36"), Strings.fopGeneral.DefinitionModeBase36)
                XMLLoadLocale(eGeneralOptions.Item("DefinitionModeBase62"), Strings.fopGeneral.DefinitionModeBase62)
                XMLLoadLocale(eGeneralOptions.Item("AutoFocusOnMouseEnter"), Strings.fopGeneral.AutoFocusOnMouseEnter)
                XMLLoadLocale(eGeneralOptions.Item("DisableFirstClick"), Strings.fopGeneral.DisableFirstClick)
                XMLLoadLocale(eGeneralOptions.Item("AutoSave"), Strings.fopGeneral.AutoSave)
                XMLLoadLocale(eGeneralOptions.Item("minutes"), Strings.fopGeneral.minutes)
                XMLLoadLocale(eGeneralOptions.Item("StopPreviewOnClick"), Strings.fopGeneral.StopPreviewOnClick)
                XMLLoadLocale(eGeneralOptions.Item("SkipClippedMeasure"), Strings.fopGeneral.SkipClippedMeasure)
                XMLLoadLocale(eGeneralOptions.Item("LaneHighlight"), Strings.fopGeneral.LaneHighlight)
                XMLLoadLocale(eGeneralOptions.Item("MinimumBGMLanes"), Strings.fopGeneral.MinimumBGMLanes)
                XMLLoadLocale(eGeneralOptions.Item("UndoRedoMemoryLimit"), Strings.fopGeneral.UndoRedoMemoryLimit)
            End If

            Dim eEncoding As XmlElement = Root.Item("Encoding")
            If eEncoding IsNot Nothing Then
                XMLLoadLocale(eEncoding.Item("Auto"), Strings.Encoding.Auto)
                XMLLoadLocale(eEncoding.Item("SystemDefault"), Strings.Encoding.SystemDefault)
                XMLLoadLocale(eEncoding.Item("ReloadWithEncoding"), Strings.Encoding.ReloadWithEncoding)
                SyncEncodingMenuText()
            End If

            Dim eFind As XmlElement = Root.Item("Find")
            If eFind IsNot Nothing Then
                XMLLoadLocale(eFind.Item("NoteRange"), Strings.fFind.NoteRange)
                XMLLoadLocale(eFind.Item("MeasureRange"), Strings.fFind.MeasureRange)
                XMLLoadLocale(eFind.Item("LabelRange"), Strings.fFind.LabelRange)
                XMLLoadLocale(eFind.Item("ValueRange"), Strings.fFind.ValueRange)
                XMLLoadLocale(eFind.Item("to"), Strings.fFind.to_)
                XMLLoadLocale(eFind.Item("Selected"), Strings.fFind.Selected)
                XMLLoadLocale(eFind.Item("UnSelected"), Strings.fFind.UnSelected)
                XMLLoadLocale(eFind.Item("ShortNote"), Strings.fFind.ShortNote)
                XMLLoadLocale(eFind.Item("LongNote"), Strings.fFind.LongNote)
                XMLLoadLocale(eFind.Item("Hidden"), Strings.fFind.Hidden)
                XMLLoadLocale(eFind.Item("Visible"), Strings.fFind.Visible)
                XMLLoadLocale(eFind.Item("Column"), Strings.fFind.Column)
                XMLLoadLocale(eFind.Item("SelectAll"), Strings.fFind.SelectAll)
                XMLLoadLocale(eFind.Item("SelectInverse"), Strings.fFind.SelectInverse)
                XMLLoadLocale(eFind.Item("UnselectAll"), Strings.fFind.UnselectAll)
                XMLLoadLocale(eFind.Item("Operation"), Strings.fFind.Operation)
                XMLLoadLocale(eFind.Item("ReplaceWithLabel"), Strings.fFind.ReplaceWithLabel)
                XMLLoadLocale(eFind.Item("ReplaceWithValue"), Strings.fFind.ReplaceWithValue)
                XMLLoadLocale(eFind.Item("Select"), Strings.fFind.Select_)
                XMLLoadLocale(eFind.Item("Unselect"), Strings.fFind.Unselect_)
                XMLLoadLocale(eFind.Item("Delete"), Strings.fFind.Delete_)
                XMLLoadLocale(eFind.Item("Close"), Strings.fFind.Close_)
            End If

            Dim eImportBMSON As XmlElement = Root.Item("ImportBMSON")
            If eImportBMSON IsNot Nothing Then
                XMLLoadLocale(eImportBMSON.Item("Message"), Strings.fImportBMSON.Message)
            End If

            Dim eFileAssociation As XmlElement = Root.Item("FileAssociation")
            If eFileAssociation IsNot Nothing Then
                XMLLoadLocale(eFileAssociation.Item("BMS"), Strings.FileAssociation.BMS)
                XMLLoadLocale(eFileAssociation.Item("BME"), Strings.FileAssociation.BME)
                XMLLoadLocale(eFileAssociation.Item("BML"), Strings.FileAssociation.BML)
                XMLLoadLocale(eFileAssociation.Item("PMS"), Strings.FileAssociation.PMS)
                XMLLoadLocale(eFileAssociation.Item("NBMSC"), Strings.FileAssociation.NBMSC)
                XMLLoadLocale(eFileAssociation.Item("Open"), Strings.FileAssociation.Open)
                XMLLoadLocale(eFileAssociation.Item("Preview"), Strings.FileAssociation.Preview)
                XMLLoadLocale(eFileAssociation.Item("ViewCode"), Strings.FileAssociation.ViewCode)
            End If

            RefreshMenuShortcutDisplay()
            If SaveSelection Then DispLang = Path.Replace(My.Application.Info.DirectoryPath & "\", "")
            Return True

        Catch ex As Exception
            If ShowError Then MsgBox(Path & vbCrLf & vbCrLf & ex.Message, MsgBoxStyle.Exclamation)
            Return False

        Finally
            If FileStream IsNot Nothing Then FileStream.Close()

            POGridPart2.Visible = xPOGridPart2
        End Try

        'File.Delete(xTempFileName)
    End Function

    Private Sub LoadLang(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim xFN2 As String = sender.ToolTipText
        'ReadLanguagePack(xFN2)
        LoadLocale(xFN2, True, True)
    End Sub

    Private Sub LoadLocaleXML(xStr As FileInfo)
        Dim d As New XmlDocument
        Dim fs As New FileStream(xStr.FullName, FileMode.Open, FileAccess.Read)

        Try
            d.Load(fs)
            Dim xName As String = d.Item("iBMSC.Locale").GetAttribute("Name")
            If xName = "" Then xName = xStr.Name

            cmnLanguage.Items.Add(xName, Nothing, AddressOf LoadLang)
            cmnLanguage.Items(cmnLanguage.Items.Count - 1).ToolTipText = xStr.FullName

        Catch ex As Exception
            MsgBox(xStr.FullName & vbCrLf & vbCrLf & ex.Message, MsgBoxStyle.Exclamation)

        End Try

        fs.Close()
    End Sub

    Private Sub LoadTheme(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim xThemePath As String = sender.ToolTipText
        'SaveTheme = True
        If Not LoadThemeFile(xThemePath) Then Return
        ChangePlaySideSkin(False)
        RefreshPanelAll()
    End Sub
End Class
