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
            .WriteAttributeString("UiCulture", UiCulture)
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
        Return ThemeMetadata.TryReadThemeName(xStr.FullName, themeName)
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

                SetConfiguredUiCulture(.GetAttribute("UiCulture"))
                ApplyLanguage()

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

    Private Sub LoadLang(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim xMenuItem As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        If xMenuItem Is Nothing Then Return

        Dim xCultureName As String = TryCast(xMenuItem.Tag, String)
        If xCultureName = "" Then Return

        SetUiCulture(xCultureName, True)
        ApplyLanguage()
        If Not IsInitializing Then SaveSettings(My.Application.Info.DirectoryPath & "\nBMSC.Settings.xml", False)
    End Sub
    Private Sub LoadTheme(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim xThemePath As String = sender.ToolTipText
        'SaveTheme = True
        If Not LoadThemeFile(xThemePath) Then Return
        ChangePlaySideSkin(False)
        RefreshPanelAll()
    End Sub
End Class
