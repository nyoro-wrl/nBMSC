Imports System.Linq
Imports System.Collections.Generic
Imports nBMSC.Editor


Public Class MainWindow


    'Public Structure MARGINS
    '    Public Left As Integer
    '    Public Right As Integer
    '    Public Top As Integer
    '    Public Bottom As Integer
    'End Structure

    '<System.Runtime.InteropServices.DllImport("dwmapi.dll")> _
    'Public Shared Function DwmIsCompositionEnabled(ByRef en As Integer) As Integer
    'End Function
    '<System.Runtime.InteropServices.DllImport("dwmapi.dll")> _
    'Public Shared Function DwmExtendFrameIntoClientArea(ByVal hwnd As IntPtr, ByRef margin As MARGINS) As Integer
    'End Function
    Public Declare Function SendMessage Lib "user32.dll" Alias "SendMessageA" (ByVal hwnd As IntPtr, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As Integer) As Integer
    Public Declare Function ReleaseCapture Lib "user32.dll" Alias "ReleaseCapture" () As Integer

    'Private Declare Auto Function GetWindowLong Lib "user32" (ByVal hWnd As IntPtr, ByVal nIndex As Integer) As Integer
    'Private Declare Auto Function SetWindowLong Lib "user32" (ByVal hWnd As IntPtr, ByVal nIndex As Integer, ByVal dwNewLong As Integer) As Integer
    'Private Declare Function SetWindowPos Lib "user32.dll" (ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal x As Integer, ByVal y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal wFlags As Integer) As Integer
    '<DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
    'Private Shared Function SetWindowText(ByVal hwnd As IntPtr, ByVal lpString As String) As Boolean
    'End Function

    'Private Const GWL_STYLE As Integer = -16
    'Private Const WS_CAPTION As Integer = &HC00000
    'Private Const SWP_NOSIZE As Integer = &H1
    'Private Const SWP_NOMOVE As Integer = &H2
    'Private Const SWP_NOZORDER As Integer = &H4
    'Private Const SWP_NOACTIVATE As Integer = &H10
    'Private Const SWP_FRAMECHANGED As Integer = &H20
    'Private Const SWP_REFRESH As Integer = SWP_NOZORDER Or SWP_NOSIZE Or SWP_NOMOVE Or SWP_NOACTIVATE Or SWP_FRAMECHANGED


    Dim MeasureLength(999) As Double
    Dim MeasureBottom(999) As Double

    Public Function MeasureUpper(idx As Integer) As Double
        Return MeasureBottom(idx) + MeasureLength(idx)
    End Function


    Dim Notes() As Note = {New Note(niBPM, -1, 1200000, 0, False)}
    Dim mColumn(999) As Integer  '0 = no column, 1 = 1 column, etc.
    Dim GreatestVPosition As Double    '+ 2000 = -VS.Minimum

    Dim VSValue As Integer = 0 'Store value before ValueChange event
    Dim HSValue As Integer = 0 'Store value before ValueChange event

    'Dim SortingMethod As Integer = 1
    Dim MiddleButtonMoveMethod As Integer = 0
    Dim TextEncoding As System.Text.Encoding = System.Text.Encoding.UTF8
    Dim DispLang As String = ""     'Display Language
    Dim Recent() As String = {"", "", "", "", ""}
    Dim NTInput As Boolean = True
    Dim ShowFileName As Boolean = False

    Dim BeepWhileSaved As Boolean = True
    Const DefinitionModeLegacy As Integer = 0
    Const DefinitionModeBase36 As Integer = 1
    Const DefinitionModeBase62 As Integer = 2
    Dim BPMDefinitionMode As Integer = DefinitionModeLegacy
    Dim STOPDefinitionMode As Integer = DefinitionModeLegacy
    Dim UseBase62Definitions As Boolean = False
    Dim NewBMSUseBase62Definitions As Boolean = False

    Dim IsInitializing As Boolean = True
    Dim FirstMouseEnter As Boolean = True

    Dim WAVMultiSelect As Boolean = True
    Dim WAVChangeLabel As Boolean = True
    Dim WAVEmptyfill As Boolean = True
    Dim BeatChangeMode As Integer = 0

    'Dim FloatTolerance As Double = 0.0001R
    Dim BMSGridLimit As Double = 1.0R

    Dim LnObj As Integer = 0    '0 for none, 1-3843 for 01-zz

    'IO
    Dim FileName As String = "Untitled.bms"
    'Dim TitlePath As New Drawing2D.GraphicsPath
    Dim InitPath As String = ""
    Dim IsSaved As Boolean = True

    'Variables for Drag/Drop
    Dim DDFileName() As String = {}
    Dim SupportedFileExtension() As String = {".bms", ".bme", ".bml", ".pms", ".txt", ".sm", ".nbmsc"}
    Dim SupportedAudioExtension() As String = {".wav", ".mp3", ".ogg", ".flac"}
    Dim SupportedImageExtension() As String = {".bmp", ".png", ".jpg", ".jpeg", ".gif", ".mpg", ".mpeg", ".avi", ".m1v", ".m2v", ".m4v", ".mp4", ".webm", ".wmv"}

    'Variables for theme
    'Dim SaveTheme As Boolean = True

    'Variables for undo/redo
    Private Const MaxUndoRedoSteps As Integer = 100000
    Private Const UndoRedoHistoryCount As Integer = MaxUndoRedoSteps + 1
    Private Const DefaultUndoRedoMemoryLimitMB As Integer = 1024
    Private Const MinUndoRedoMemoryLimitMB As Integer = 16
    Private Const MaxUndoRedoMemoryLimitMB As Integer = 1024
    Dim sUndo(UndoRedoHistoryCount - 1) As UndoRedo.LinkedURCmd
    Dim sRedo(UndoRedoHistoryCount - 1) As UndoRedo.LinkedURCmd
    Dim sI As Integer = 0
    Dim UndoRedoMemoryLimitMB As Integer = DefaultUndoRedoMemoryLimitMB
    Dim UndoRedoMemoryUsedBytes As Long = 0

    'Variables for select tool
    Dim DisableVerticalMove As Boolean = False
    Dim KMouseOver As Integer = -1   'Mouse is on which note (for drawing green outline)
    Dim LastMouseDownLocation As PointF = New Point(-1, -1)          'Mouse is clicked on which point (location for display) (for selection box)
    Dim pMouseMove As PointF = New Point(-1, -1)          'Mouse is moved to which point   (location for display) (for selection box)
    'Dim KMouseDown As Integer = -1   'Mouse is clicked on which note (for moving)
    Dim deltaVPosition As Double = 0   'difference between mouse and VPosition of K
    Dim bAdjustLength As Boolean     'If adjusting note length instead of moving it
    Dim bAdjustUpper As Boolean      'true = Adjusting upper end, false = adjusting lower end
    Dim bAdjustSingle As Boolean     'true if there is only one note to be adjusted
    Dim tempY As Integer
    Dim tempV As Integer
    Dim tempX As Integer
    Dim tempH As Integer
    Dim MiddleButtonLocation As New Point(0, 0)
    Dim MiddleButtonClicked As Boolean = False
    Dim MouseMoveStatus As Point = New Point(0, 0)  'mouse is moved to which point (For Status Panel)
    'Dim uCol As Integer         'temp variables for undo, original enabled columnindex
    'Dim uVPos As Double         'temp variables for undo, original vposition
    'Dim uPairWithI As Double    'temp variables for undo, original note length
    Dim uAdded As Boolean       'temp variables for undo, if undo command is added
    'Dim uNote As Note           'temp variables for undo, original note
    Dim SelectedNotes(-1) As Note        'temp notes for undo
    Dim KeyMoveNotes(-1) As Note
    Dim KeyMoveKey As Keys = Keys.None
    Dim KeyMoveUndoAdded As Boolean = False
    Dim ctrlPressed As Boolean = False          'Indicates if the CTRL key is pressed while mousedown
    Dim DuplicatedSelectedNotes As Boolean = False     'Indicates if duplicate notes of select/unselect note

    'Variables for write tool
    Dim ShouldDrawTempNote As Boolean = False
    Dim SelectedColumn As Integer = -1
    Dim TempVPosition As Double = -1.0#
    Dim TempLength As Double = 0.0#

    'Variables for post effects tool
    Dim vSelStart As Double = 192.0#
    Dim vSelLength As Double = 0.0#
    Dim vSelHalf As Double = 0.0#
    Dim vSelMouseOverLine As Integer = 0  '0 = nothing, 1 = start, 2 = half, 3 = end
    Dim vSelAdjust As Boolean = False
    Dim vSelK() As Note = {}
    Dim vSelPStart As Double = 192.0#
    Dim vSelPLength As Double = 0.0#
    Dim vSelPHalf As Double = 0.0#

    'Variables for Full-Screen Mode
    Dim isFullScreen As Boolean = False
    Dim previousWindowState As FormWindowState = FormWindowState.Normal
    Dim previousWindowPosition As New Rectangle(0, 0, 0, 0)

    'Variables misc
    Dim menuVPosition As Double = 0.0#
    Dim tempResize As Integer = 0

    '----AutoSave Options
    Dim PreviousAutoSavedFileName As String = ""
    Dim AutoSaveInterval As Integer = 120000

    '----ErrorCheck Options
    Dim ErrorCheck As Boolean = True

    '----Header Options
    Dim hWAV(MaxDefinition) As String
    Dim hBMP(MaxDefinition) As String
    Dim hBPM(MaxDefinition) As Long   'x10000
    Dim hSTOP(MaxDefinition) As Long
    Dim hBMSCROLL(MaxDefinition) As Long

    '----Grid Options
    Dim gSnap As Boolean = True
    Dim gShowGrid As Boolean = True 'Grid
    Dim gShowSubGrid As Boolean = True 'Sub
    Dim gShowBG As Boolean = True 'BG Color
    Dim gShowMeasureNumber As Boolean = True 'Measure Label
    Dim gShowVerticalLine As Boolean = True 'Vertical
    Dim gShowMeasureBar As Boolean = True 'Measure Barline
    Dim gShowC As Boolean = True 'Column Caption
    Dim gDivide As Integer = 16
    Dim gSub As Integer = 4
    Dim gSlash As Integer = 192
    Dim gxHeight As Single = 1.0!
    Dim gxWidth As Single = 1.0!
    Dim gWheel As Integer = 96
    Dim gPgUpDn As Integer = 384

    Dim gDisplayBGAColumn As Boolean = True
    Dim gSCROLL As Boolean = True
    Dim gSTOP As Boolean = True
    Dim gBPM As Boolean = True
    'Dim gA8 As Boolean = False
    Dim iPlayer As Integer = 0
    Private Const MaxBGMColumns As Integer = 999
    Private Const BGMColumnTailMargin As Integer = 5
    Dim gColumns As Integer = 82

    '----Visual Options
    Dim vo As New visualSettings()

    Public Sub setVO(ByVal xvo As visualSettings)
        vo = xvo
    End Sub

    '----Preview Options
    Structure PlayerArguments
        Public Path As String
        Public aBegin As String
        Public aHere As String
        Public aStop As String
        Public Sub New(ByVal xPath As String, ByVal xBegin As String, ByVal xHere As String, ByVal xStop As String)
            Path = xPath
            aBegin = xBegin
            aHere = xHere
            aStop = xStop
        End Sub
    End Structure

    Public Shared Function DefaultPlayerArguments() As PlayerArguments()
        Return New PlayerArguments() {New PlayerArguments("<apppath>\mBMplay.exe",
                                                          """<filename>""",
                                                          "-s <measure> ""<filename>""",
                                                          "-t"),
                                      New PlayerArguments("<apppath>\jbmplay.exe",
                                                          """<filename>""",
                                                          "-m <measure> ""<filename>""",
                                                          ""),
                                      New PlayerArguments("<apppath>\uBMplay.exe",
                                                          "-P -N0 ""<filename>""",
                                                          "-P -N<measure> ""<filename>""",
                                                          "-S"),
                                      New PlayerArguments("<apppath>\beatoraja.exe",
                                                          "-a ""<filename>""",
                                                          "-s ""<filename>""",
                                                          ""),
                                      New PlayerArguments("<apppath>\LR2body.exe",
                                                          "-A -NS ""<filename>""",
                                                          "-NS ""<filename>""",
                                                          "-S")}
    End Function

    Public Shared Function PlayerDisplayName(ByVal playerPath As String) As String
        If playerPath Is Nothing Then Return ""

        Dim fslash As Integer = InStrRev(playerPath, "/")
        Dim bslash As Integer = InStrRev(playerPath, "\")
        Dim fileName As String = Mid(playerPath, Math.Max(fslash, bslash) + 1)
        Dim dot As Integer = InStrRev(fileName, ".")
        If dot > 1 Then fileName = Microsoft.VisualBasic.Left(fileName, dot - 1)

        Return fileName
    End Function

    Public pArgs() As PlayerArguments = DefaultPlayerArguments()
    Public CurrentPlayer As Integer = 0
    Dim PreviewOnClick As Boolean = True
    Dim PreviewErrorCheck As Boolean = False
    Dim Rscratch As Boolean = False
    Dim ClickStopPreview As Boolean = True
    Dim SkipClippedMeasure As Boolean = False
    Dim LaneHighlight As Integer = 0
    Dim pTempFileNames() As String = {}
    Dim SkippedUpdateTag As String = ""
    Dim CheckUpdatesOnStartup As Boolean = True

    '----Split Panel Options
    Dim PanelWidth() As Single = {100}
    Dim PanelhBMSCROLL() As Integer = {0}
    Dim PanelVScroll() As Integer = {0}
    Private Const DefaultSplitPanelRatio As Single = 0.25!
    Private Const MaxSplitPanelShowRatio As Single = 0.4!
    Private Const MinSplitPanelWidth As Integer = 25
    Private Const MinMainPanelWidth As Integer = 120
    Private Const MainPanelIndex As Integer = 0
    Private Const EditorScrollBarThickness As Integer = 8
    Private Const SplitPanelSettingSeparator As Char = ";"c
    Dim SyncSplitterScroll As Boolean = False
    Dim UpdatingSplitterControls As Boolean = False
    Dim SyncingPanelScroll As Boolean = False
    Dim PanelFocus As Integer = MainPanelIndex
    Dim spMouseOver As Integer = MainPanelIndex
    Private Const PreviewGlobalShortcutSuppressMilliseconds As Double = 1000.0R
    Private PreviewGlobalShortcutKey As Keys = Keys.None
    Private PreviewGlobalShortcutModifiers As Integer = 0
    Private PreviewGlobalShortcutTime As DateTime = DateTime.MinValue

    Dim AutoFocusMouseEnter As Boolean = False
    Dim FirstClickDisabled As Boolean = True
    Dim tempFirstMouseDown As Boolean = False

    Dim spMain() As Panel = {}
    Private WithEvents mnSyncSplitterScroll As ToolStripMenuItem
    Private WithEvents mnSAddSplitter As ToolStripMenuItem
    Private WithEvents mnSRemoveSplitter As ToolStripMenuItem
    Private WithEvents mnSlashGrid As ToolStripMenuItem
    Private WithEvents TBGridDivide As ToolStripComboBox
    Private WithEvents TBGridSub As ToolStripComboBox
    Private WithEvents TBGridHeight As ToolStripComboBox
    Private WithEvents TBGridWidth As ToolStripComboBox
    Private WithEvents TBDisableVertical As ToolStripButton
    Private WithEvents TBGridSnap As ToolStripButton
    Private UpdatingGridToolbar As Boolean = False
    Private WithEvents TBPlayer As ToolStripComboBox
    Private UpdatingPlayerSelector As Boolean = False
    Private WithEvents TBAddSplitter As ToolStripButton
    Private WithEvents TBRemoveSplitter As ToolStripButton
    Private WithEvents TBSyncSplitterScroll As ToolStripButton
    Private ToolStripSeparatorSplitter As ToolStripSeparator
    Private SplitterResizeCursor As Cursor
    Private EditorContextMenu As ContextMenuStrip
    Private EditorContextPanelIndex As Integer = 0
    Private EditorContextPlayB As ToolStripMenuItem
    Private EditorContextPlay As ToolStripMenuItem
    Private EditorContextInsert As ToolStripMenuItem
    Private EditorContextRemove As ToolStripMenuItem
    Private EditorContextCut As ToolStripMenuItem
    Private EditorContextCopy As ToolStripMenuItem
    Private EditorContextPaste As ToolStripMenuItem
    Private EditorContextDelete As ToolStripMenuItem
    Private EditorContextHidden As ToolStripMenuItem
    Private EditorContextVisible As ToolStripMenuItem
    Private EditorContextHiddenVisible As ToolStripMenuItem
    Private EditorContextLandmine As ToolStripMenuItem
    Private EditorContextNormalLandmine As ToolStripMenuItem
    Private EditorContextModify As ToolStripMenuItem
    Private EditorContextMirror As ToolStripMenuItem
    Private EditorContextCloseSplitter As ToolStripMenuItem
    Private EditorContextEditSeparator As ToolStripSeparator
    Private EditorContextConvertSeparator As ToolStripSeparator
    Private EditorContextModifySeparator As ToolStripSeparator
    Private EditorContextSplitterSeparator As ToolStripSeparator
    Private DefinitionContextMenu As ContextMenuStrip
    Private DefinitionContextDelete As ToolStripMenuItem
    Private DefinitionContextList As ListBox

    Private Class EditorScrollBar
        Inherits Control

        Private _minimum As Integer = 0
        Private _maximum As Integer = 100
        Private _largeChange As Integer = 10
        Private _smallChange As Integer = 1
        Private _value As Integer = 0
        Private _orientation As Orientation = Orientation.Vertical
        Private _dragging As Boolean = False
        Private _dragOffset As Integer = 0
        Private _hotThumb As Boolean = False
        Private Shared ReadOnly TrackColor As Color = Color.FromArgb(240, 240, 240)
        Private Shared ReadOnly ThumbColor As Color = Color.FromArgb(133, 133, 133)

        Public Event ValueChanged As EventHandler

        Public Sub New()
            SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.ResizeRedraw Or ControlStyles.UserPaint, True)
            TabStop = True
            AccessibleRole = AccessibleRole.ScrollBar
            BackColor = TrackColor
        End Sub

        Public Property Orientation As Orientation
            Get
                Return _orientation
            End Get
            Set(value As Orientation)
                _orientation = value
                If value = Orientation.Vertical Then
                    Width = EditorScrollBarThickness
                Else
                    Height = EditorScrollBarThickness
                End If
                Invalidate()
            End Set
        End Property

        Public Property Minimum As Integer
            Get
                Return _minimum
            End Get
            Set(value As Integer)
                _minimum = value
                ClampValue()
                Invalidate()
            End Set
        End Property

        Public Property Maximum As Integer
            Get
                Return _maximum
            End Get
            Set(value As Integer)
                _maximum = value
                ClampValue()
                Invalidate()
            End Set
        End Property

        Public Property LargeChange As Integer
            Get
                Return _largeChange
            End Get
            Set(value As Integer)
                _largeChange = Math.Max(1, value)
                ClampValue()
                Invalidate()
            End Set
        End Property

        Public Property SmallChange As Integer
            Get
                Return _smallChange
            End Get
            Set(value As Integer)
                _smallChange = Math.Max(1, value)
            End Set
        End Property

        Public Property Value As Integer
            Get
                Return _value
            End Get
            Set(value As Integer)
                Dim xValue As Integer = Clamp(value)
                If _value = xValue Then Return

                _value = xValue
                Invalidate()
                RaiseEvent ValueChanged(Me, EventArgs.Empty)
            End Set
        End Property

        Private Function MaxValue() As Integer
            Dim xMax As Integer = _maximum - _largeChange + 1
            If xMax < _minimum Then xMax = _minimum
            Return xMax
        End Function

        Private Function Clamp(value As Integer) As Integer
            If value < _minimum Then value = _minimum
            If value > MaxValue() Then value = MaxValue()
            Return value
        End Function

        Private Sub ClampValue()
            Value = _value
        End Sub

        Private Function TrackLength() As Integer
            If _orientation = Orientation.Vertical Then Return Math.Max(0, Height)
            Return Math.Max(0, Width)
        End Function

        Private Function ThumbRect() As Rectangle
            Dim xTrackLength As Integer = TrackLength()
            If xTrackLength <= 0 Then Return Rectangle.Empty

            Dim xMaxValue As Integer = MaxValue()
            Dim xRange As Integer = xMaxValue - _minimum
            Dim xThumbLength As Integer
            If xRange <= 0 Then
                xThumbLength = xTrackLength
            Else
                xThumbLength = CInt(Math.Max(18, Math.Floor(xTrackLength * (_largeChange / CDbl(xRange + _largeChange)))))
                If xThumbLength > xTrackLength Then xThumbLength = xTrackLength
            End If

            Dim xThumbPosition As Integer = 0
            If xRange > 0 AndAlso xTrackLength > xThumbLength Then
                xThumbPosition = CInt(Math.Round((_value - _minimum) / CDbl(xRange) * (xTrackLength - xThumbLength)))
            End If

            If _orientation = Orientation.Vertical Then
                Return New Rectangle(1, xThumbPosition, Math.Max(1, Width - 2), xThumbLength)
            End If

            Return New Rectangle(xThumbPosition, 1, xThumbLength, Math.Max(1, Height - 2))
        End Function

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            MyBase.OnPaint(e)

            Using xTrack As New SolidBrush(TrackColor)
                e.Graphics.FillRectangle(xTrack, ClientRectangle)
            End Using

            Dim xThumb As Rectangle = ThumbRect()
            If xThumb.IsEmpty Then Return

            Using xBrush As New SolidBrush(ThumbColor)
                e.Graphics.FillRectangle(xBrush, xThumb)
            End Using
        End Sub

        Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
            MyBase.OnMouseDown(e)
            If e.Button <> MouseButtons.Left Then Return

            Focus()
            Dim xThumb As Rectangle = ThumbRect()
            Dim xLocation As Integer = If(_orientation = Orientation.Vertical, e.Y, e.X)
            If xThumb.Contains(e.Location) Then
                _dragging = True
                _dragOffset = xLocation - If(_orientation = Orientation.Vertical, xThumb.Top, xThumb.Left)
                Capture = True
            ElseIf xLocation < If(_orientation = Orientation.Vertical, xThumb.Top, xThumb.Left) Then
                Value -= _largeChange
            Else
                Value += _largeChange
            End If
        End Sub

        Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
            MyBase.OnMouseMove(e)
            Dim xThumb As Rectangle = ThumbRect()
            Dim xHot As Boolean = xThumb.Contains(e.Location)
            If _hotThumb <> xHot Then
                _hotThumb = xHot
                Invalidate()
            End If

            If Not _dragging Then Return

            Dim xTrackLength As Integer = TrackLength()
            Dim xThumbLength As Integer = If(_orientation = Orientation.Vertical, xThumb.Height, xThumb.Width)
            Dim xMovable As Integer = xTrackLength - xThumbLength
            If xMovable <= 0 Then Return

            Dim xLocation As Integer = If(_orientation = Orientation.Vertical, e.Y, e.X)
            Dim xThumbPosition As Integer = Math.Max(0, Math.Min(xMovable, xLocation - _dragOffset))
            Dim xRange As Integer = MaxValue() - _minimum
            Value = _minimum + CInt(Math.Round(xThumbPosition / CDbl(xMovable) * xRange))
        End Sub

        Protected Overrides Sub OnMouseLeave(e As EventArgs)
            MyBase.OnMouseLeave(e)
            If _hotThumb Then
                _hotThumb = False
                Invalidate()
            End If
        End Sub

        Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
            MyBase.OnMouseUp(e)
            If e.Button <> MouseButtons.Left Then Return

            _dragging = False
            Capture = False
            Invalidate()
        End Sub

        Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
            MyBase.OnKeyDown(e)
            Select Case e.KeyCode
                Case Keys.Up, Keys.Left
                    Value -= _smallChange
                Case Keys.Down, Keys.Right
                    Value += _smallChange
                Case Keys.PageUp
                    Value -= _largeChange
                Case Keys.PageDown
                    Value += _largeChange
                Case Keys.Home
                    Value = _minimum
                Case Keys.End
                    Value = MaxValue()
            End Select
        End Sub
    End Class

    Private Class TransparentSplitterGrip
        Inherits Control

        Public Sub New()
            SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.OptimizedDoubleBuffer Or ControlStyles.SupportsTransparentBackColor Or ControlStyles.UserPaint, True)
            BackColor = Color.Transparent
            TabStop = False
        End Sub

        Protected Overrides ReadOnly Property CreateParams As CreateParams
            Get
                Dim xParams As CreateParams = MyBase.CreateParams
                xParams.ExStyle = xParams.ExStyle Or &H20
                Return xParams
            End Get
        End Property

        Protected Overrides Sub OnPaintBackground(ByVal pevent As PaintEventArgs)
        End Sub

        Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        End Sub
    End Class

    Private Class SplitPane
        Public Container As Panel
        Public Canvas As Panel
        Public VScroll As EditorScrollBar
        Public HScroll As EditorScrollBar
        Public Splitter As Control
        Public Ratio As Single
    End Class

    Private Class SplitterDragInfo
        Public PanelIndex As Integer
        Public StartScreenX As Integer
        Public StartWidth As Integer
        Public PairWidth As Integer
    End Class

    Private Class MouseWheelBlocker
        Inherits NativeWindow

        Private Const WM_MOUSEWHEEL As Integer = &H20A
        Private ReadOnly Target As Control

        Public Sub New(ByVal xTarget As Control)
            Target = xTarget
            AddHandler Target.HandleCreated, AddressOf Target_HandleCreated
            AddHandler Target.HandleDestroyed, AddressOf Target_HandleDestroyed
            If Target.IsHandleCreated Then AssignHandle(Target.Handle)
        End Sub

        Public Sub Release()
            RemoveHandler Target.HandleCreated, AddressOf Target_HandleCreated
            RemoveHandler Target.HandleDestroyed, AddressOf Target_HandleDestroyed
            If Handle <> IntPtr.Zero Then ReleaseHandle()
        End Sub

        Private Sub Target_HandleCreated(ByVal sender As Object, ByVal e As EventArgs)
            AssignHandle(Target.Handle)
        End Sub

        Private Sub Target_HandleDestroyed(ByVal sender As Object, ByVal e As EventArgs)
            If Handle <> IntPtr.Zero Then ReleaseHandle()
        End Sub

        Private Function GetScrollParent() As ScrollableControl
            Dim xParent As Control = Target.Parent

            Do While xParent IsNot Nothing
                Dim xScrollable As ScrollableControl = TryCast(xParent, ScrollableControl)
                If xScrollable IsNot Nothing AndAlso xScrollable.AutoScroll Then Return xScrollable
                xParent = xParent.Parent
            Loop

            Return Nothing
        End Function

        Private Function GetWheelDelta(ByVal xWParam As IntPtr) As Integer
            Dim xValue As Integer = CInt((xWParam.ToInt64() >> 16) And 65535)
            If xValue >= 32768 Then xValue -= 65536
            Return xValue
        End Function

        Private Sub ScrollParent(ByVal xScrollable As ScrollableControl, ByVal xDelta As Integer)
            If xScrollable Is Nothing Then Return
            If xDelta = 0 Then Return

            Dim xPoint As Point = xScrollable.PointToClient(Cursor.Position)
            Dim xArgs As New MouseEventArgs(MouseButtons.None, 0, xPoint.X, xPoint.Y, xDelta)
            Dim xMethod As System.Reflection.MethodInfo = GetType(ScrollableControl).GetMethod("OnMouseWheel", System.Reflection.BindingFlags.Instance Or System.Reflection.BindingFlags.NonPublic)
            If xMethod IsNot Nothing Then xMethod.Invoke(xScrollable, New Object() {xArgs})
        End Sub

        Protected Overrides Sub WndProc(ByRef m As Message)
            If m.Msg = WM_MOUSEWHEEL Then
                ScrollParent(GetScrollParent(), GetWheelDelta(m.WParam))
                Return
            End If

            MyBase.WndProc(m)
        End Sub
    End Class

    Private SplitPanes As New List(Of SplitPane)
    Private SplitterDrag As SplitterDragInfo = Nothing
    Private OptionsTabsInitialized As Boolean = False
    Private POTabSelected As Panel = Nothing
    Private HeaderWheelBlockers As New List(Of MouseWheelBlocker)

    '----Find Delete Replace Options
    Dim fdriMesL As Integer
    Dim fdriMesU As Integer
    Dim fdriLblL As Integer
    Dim fdriLblU As Integer
    Dim fdriValL As Integer
    Dim fdriValU As Integer
    Dim fdriCol() As Integer


    Public Sub New()
        InitializeComponent()
        InitializeEditorContextMenu()
        InitializeDefinitionContextMenu()
        InitializeHeaderWheelBlockers()
        InitializePlayerSelector()
        InitializeGridToolbar()
        InitializeOptionsTabs()
        InitializeOptionsMenuItems()
        InitializeSplitterControls()
        ReorderConversionMenu()
        RefreshMenuShortcutDisplay()
        InitializeSplitPanes()
        ApplyToolbarLayoutRules()
        Audio.Initialize()
    End Sub

    Private Sub InitializeHeaderWheelBlockers()
        HeaderWheelBlockers.Clear()
        AddHeaderWheelBlockers(POHeaderPart1)
        AddHeaderWheelBlockers(POHeaderPart2)
    End Sub

    Private Sub AddHeaderWheelBlockers(ByVal xParent As Control)
        For Each xControl As Control In xParent.Controls
            If TypeOf xControl Is ComboBox OrElse TypeOf xControl Is NumericUpDown Then
                HeaderWheelBlockers.Add(New MouseWheelBlocker(xControl))
            End If

            If xControl.Controls.Count > 0 Then AddHeaderWheelBlockers(xControl)
        Next
    End Sub

    Private Sub InitializeEditorContextMenu()
        EditorContextMenu = New ContextMenuStrip(components)
        EditorContextMenu.ShowImageMargin = True
        EditorContextPlayB = New ToolStripMenuItem()
        EditorContextPlay = New ToolStripMenuItem()
        EditorContextInsert = New ToolStripMenuItem()
        EditorContextRemove = New ToolStripMenuItem()
        EditorContextCut = New ToolStripMenuItem()
        EditorContextCopy = New ToolStripMenuItem()
        EditorContextPaste = New ToolStripMenuItem()
        EditorContextDelete = New ToolStripMenuItem()
        EditorContextHidden = New ToolStripMenuItem()
        EditorContextVisible = New ToolStripMenuItem()
        EditorContextHiddenVisible = New ToolStripMenuItem()
        EditorContextLandmine = New ToolStripMenuItem()
        EditorContextNormalLandmine = New ToolStripMenuItem()
        EditorContextModify = New ToolStripMenuItem()
        EditorContextMirror = New ToolStripMenuItem()
        EditorContextCloseSplitter = New ToolStripMenuItem()
        EditorContextEditSeparator = New ToolStripSeparator()
        EditorContextConvertSeparator = New ToolStripSeparator()
        EditorContextModifySeparator = New ToolStripSeparator()
        EditorContextSplitterSeparator = New ToolStripSeparator()
        EditorContextCloseSplitter.Text = "Close Splitter"

        EditorContextMenu.Items.AddRange(New ToolStripItem() {
            EditorContextPlayB,
            EditorContextPlay,
            New ToolStripSeparator(),
            EditorContextInsert,
            EditorContextRemove,
            EditorContextEditSeparator,
            EditorContextCut,
            EditorContextCopy,
            EditorContextPaste,
            EditorContextDelete,
            EditorContextConvertSeparator,
            EditorContextHidden,
            EditorContextLandmine,
            EditorContextVisible,
            EditorContextHiddenVisible,
            EditorContextNormalLandmine,
            EditorContextModifySeparator,
            EditorContextModify,
            EditorContextMirror,
            EditorContextSplitterSeparator,
            EditorContextCloseSplitter})

        AddHandler EditorContextMenu.Opening, AddressOf EditorContextMenu_Opening
        AddHandler EditorContextPlayB.Click, AddressOf TBPlayB_Click
        AddHandler EditorContextPlay.Click, AddressOf TBPlay_Click
        AddHandler EditorContextInsert.Click, AddressOf MInsert_Click
        AddHandler EditorContextRemove.Click, AddressOf MRemove_Click
        AddHandler EditorContextCut.Click, AddressOf TBCut_Click
        AddHandler EditorContextCopy.Click, AddressOf TBCopy_Click
        AddHandler EditorContextPaste.Click, AddressOf EditorContextPaste_Click
        AddHandler EditorContextDelete.Click, AddressOf EditorContextDelete_Click
        AddHandler EditorContextHidden.Click, AddressOf POBHidden_Click
        AddHandler EditorContextVisible.Click, AddressOf POBVisible_Click
        AddHandler EditorContextHiddenVisible.Click, AddressOf POBHiddenVisible_Click
        AddHandler EditorContextLandmine.Click, AddressOf POBLandmine_Click
        AddHandler EditorContextNormalLandmine.Click, AddressOf POBNormalLandmine_Click
        AddHandler EditorContextModify.Click, AddressOf POBModify_Click
        AddHandler EditorContextMirror.Click, AddressOf POBMirror_Click
        AddHandler EditorContextCloseSplitter.Click, AddressOf EditorContextCloseSplitter_Click
    End Sub

    Private Sub EditorContextMenu_Opening(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs)
        RefreshEditorContextMenu()
    End Sub

    Private Sub RefreshEditorContextMenu()
        CopyMenuItem(EditorContextPlayB, mnPlayB)
        CopyMenuItem(EditorContextPlay, mnPlay)
        CopyMenuItem(EditorContextInsert, MInsert)
        CopyMenuItem(EditorContextRemove, MRemove)
        CopyMenuItem(EditorContextCut, mnCut)
        CopyMenuItem(EditorContextCopy, mnCopy)
        CopyMenuItem(EditorContextPaste, mnPaste)
        CopyMenuItem(EditorContextDelete, mnDelete)
        CopyMenuItem(EditorContextHidden, POBHidden, True)
        CopyMenuItem(EditorContextVisible, POBVisible, True)
        CopyMenuItem(EditorContextHiddenVisible, POBHiddenVisible, True)
        CopyMenuItem(EditorContextLandmine, POBLandmine, True)
        CopyMenuItem(EditorContextNormalLandmine, POBNormalLandmine, True)
        CopyMenuItem(EditorContextModify, POBModify, True)
        CopyMenuItem(EditorContextMirror, POBMirror, True)

        EditorContextMenu.Font = Menu1.Font

        Dim xHasSelection As Boolean = HasSelectedNotes()
        Dim xHasPaste As Boolean = HasPasteSource()

        EditorContextCut.Visible = xHasSelection
        EditorContextCopy.Visible = xHasSelection
        EditorContextPaste.Visible = xHasPaste
        EditorContextDelete.Visible = xHasSelection
        EditorContextEditSeparator.Visible = xHasSelection OrElse xHasPaste

        EditorContextHidden.Visible = xHasSelection
        EditorContextVisible.Visible = xHasSelection
        EditorContextHiddenVisible.Visible = xHasSelection
        EditorContextLandmine.Visible = xHasSelection
        EditorContextNormalLandmine.Visible = xHasSelection
        EditorContextConvertSeparator.Visible = xHasSelection

        EditorContextModify.Visible = xHasSelection
        EditorContextMirror.Visible = xHasSelection
        EditorContextModifySeparator.Visible = xHasSelection

        Dim xCanCloseSplitter As Boolean = EditorContextPanelIndex > MainPanelIndex AndAlso IsValidPanelIndex(EditorContextPanelIndex)
        EditorContextCloseSplitter.Visible = xCanCloseSplitter
        EditorContextSplitterSeparator.Visible = xCanCloseSplitter
    End Sub

    Private Sub CopyMenuItem(ByVal xTarget As ToolStripMenuItem, ByVal xSource As ToolStripMenuItem, Optional ByVal xCopyImage As Boolean = False)
        xTarget.Text = RemoveMenuAccessKeys(xSource.Text)
        xTarget.Image = Nothing
        If xCopyImage Then xTarget.Image = xSource.Image
        xTarget.ShortcutKeys = xSource.ShortcutKeys
        xTarget.ShortcutKeyDisplayString = xSource.ShortcutKeyDisplayString
        xTarget.ShowShortcutKeys = xSource.ShowShortcutKeys
    End Sub

    Private Function RemoveMenuAccessKeys(ByVal text As String) As String
        If text Is Nothing Then Return ""

        Dim xBuilder As New System.Text.StringBuilder()
        Dim i As Integer = 0
        Do While i < text.Length
            If text(i) = "&"c Then
                If i + 1 < text.Length AndAlso text(i + 1) = "&"c Then
                    xBuilder.Append("&"c)
                    i += 2
                Else
                    i += 1
                End If
            Else
                xBuilder.Append(text(i))
                i += 1
            End If
        Loop

        Return xBuilder.ToString()
    End Function

    Private Sub RemoveMenuAccessKeys(ByVal items As ToolStripItemCollection)
        If items Is Nothing Then Return

        For Each item As ToolStripItem In items
            item.Text = RemoveMenuAccessKeys(item.Text)

            Dim xDropDownItem As ToolStripDropDownItem = TryCast(item, ToolStripDropDownItem)
            If xDropDownItem IsNot Nothing Then RemoveMenuAccessKeys(xDropDownItem.DropDownItems)
        Next
    End Sub

    Private Sub RefreshMenuShortcutDisplay()
        RemoveMenuAccessKeys(mnMain.Items)
        RemoveMenuAccessKeys(Menu1.Items)
        RemoveMenuAccessKeys(cmnConversion.Items)
        RemoveMenuAccessKeys(cmnLanguage.Items)
        RemoveMenuAccessKeys(cmnTheme.Items)
        If EditorContextMenu IsNot Nothing Then RemoveMenuAccessKeys(EditorContextMenu.Items)
        If DefinitionContextDelete IsNot Nothing Then DefinitionContextDelete.Text = RemoveMenuAccessKeys(mnDelete.Text)

        mnNTInput.ShortcutKeys = Keys.None
        mnNTInput.ShortcutKeyDisplayString = ""
        POBLong.ShortcutKeyDisplayString = "L"
        POBShort.ShortcutKeyDisplayString = "S"
        POBMirror.ShortcutKeyDisplayString = "M"
        If mnSAddSplitter IsNot Nothing Then mnSAddSplitter.ShortcutKeyDisplayString = "Ctrl++"
        If mnSRemoveSplitter IsNot Nothing Then mnSRemoveSplitter.ShortcutKeyDisplayString = "Ctrl+-"
        If mnSyncSplitterScroll IsNot Nothing Then mnSyncSplitterScroll.ShortcutKeyDisplayString = "Ctrl+\"
        If mnSlashGrid IsNot Nothing Then mnSlashGrid.ShortcutKeyDisplayString = "/"

        If TBAddSplitter IsNot Nothing Then TBAddSplitter.ToolTipText = TBAddSplitter.Text & " (Ctrl++)"
        If TBRemoveSplitter IsNot Nothing Then TBRemoveSplitter.ToolTipText = TBRemoveSplitter.Text & " (Ctrl+-)"
        If TBSyncSplitterScroll IsNot Nothing Then TBSyncSplitterScroll.ToolTipText = TBSyncSplitterScroll.Text & " (Ctrl+\)"
    End Sub

    Private Sub ReorderConversionMenu()
        cmnConversion.Items.Clear()
        cmnConversion.Items.AddRange(New ToolStripItem() {
            POBHidden,
            POBLandmine,
            POBVisible,
            POBHiddenVisible,
            POBNormalLandmine,
            ToolStripSeparator11,
            POBModify,
            POBMirror,
            ToolStripSeparator10,
            POBLong,
            POBShort,
            POBLongShort
        })
    End Sub

    Private Function HasSelectedNotes() As Boolean
        For xI1 As Integer = 1 To UBound(Notes)
            If Notes(xI1).Selected Then Return True
        Next

        Return False
    End Function

    Private Function HasPasteSource() As Boolean
        Try
            If Not Clipboard.ContainsText() Then Return False

            Dim xLines() As String = Split(Clipboard.GetText.Replace(vbCrLf, vbLf).Replace(vbCr, vbLf), vbLf)
            If xLines.Length = 0 Then Return False
            Select Case xLines(0)
                Case "nBMSC Clipboard Data", "nBMSC Clipboard Data xNT", "BMSE ClipBoard Object Data Format"
                    For xI1 As Integer = 1 To UBound(xLines)
                        If xLines(xI1).Trim <> "" Then Return True
                    Next
            End Select
        Catch ex As Exception
        End Try

        Return False
    End Function

    Private Sub ShowEditorContextMenu(ByVal xControl As Control, ByVal e As MouseEventArgs, ByVal xVS As Long, ByVal xHeight As Integer)
        If xControl Is Nothing OrElse EditorContextMenu Is Nothing Then Return

        EditorContextPanelIndex = GetSplitPaneIndex(xControl)
        If Not IsValidPanelIndex(EditorContextPanelIndex) Then EditorContextPanelIndex = PanelFocus
        If Not IsValidPanelIndex(EditorContextPanelIndex) Then EditorContextPanelIndex = MainPanelIndex

        menuVPosition = (xHeight - xVS * gxHeight - e.Y - 1) / gxHeight
        If menuVPosition < 0 Then menuVPosition = 0
        If menuVPosition >= GetMaxVPosition() Then menuVPosition = GetMaxVPosition() - 1

        RefreshEditorContextMenu()
        EditorContextMenu.Show(xControl, e.Location)
    End Sub

    Private Function GetSplitPaneIndex(ByVal xControl As Control) As Integer
        If xControl Is Nothing Then Return -1

        For i As Integer = 0 To SplitPanes.Count - 1
            Dim xPane As SplitPane = SplitPanes(i)
            If xPane.Canvas Is xControl Then Return i
            If xPane.Container Is xControl Then Return i
            If xPane.Splitter Is xControl Then Return i
            If xPane.Container IsNot Nothing AndAlso xPane.Container.Contains(xControl) Then Return i
        Next

        Return -1
    End Function

    Private Sub EditorContextDelete_Click(ByVal sender As Object, ByVal e As EventArgs)
        FocusEditorContextPanel()
        mnDelete_Click(mnDelete, e)
    End Sub

    Private Sub EditorContextPaste_Click(ByVal sender As Object, ByVal e As EventArgs)
        FocusEditorContextPanel()
        PasteNotes(MeasureBottom(MeasureAtDisplacement(menuVPosition)))
    End Sub

    Private Sub EditorContextCloseSplitter_Click(ByVal sender As Object, ByVal e As EventArgs)
        RemoveRightSplitPane(EditorContextPanelIndex)
    End Sub

    Private Sub FocusEditorContextPanel()
        If Not IsValidPanelIndex(EditorContextPanelIndex) Then Return

        PanelFocus = EditorContextPanelIndex
        spMain(EditorContextPanelIndex).Focus()
    End Sub

    Private Sub ReleaseHeaderWheelBlockers()
        For Each xBlocker As MouseWheelBlocker In HeaderWheelBlockers
            xBlocker.Release()
        Next

        HeaderWheelBlockers.Clear()
    End Sub

    Private Sub ApplyToolbarLayoutRules()
        For Each item As ToolStripItem In TBMain.Items
            Dim button As ToolStripButton = TryCast(item, ToolStripButton)
            If button Is Nothing Then Continue For
            If Not IsIndependentToolbarToggle(button) Then Continue For

            ' Independent toggle buttons need a 1 px right gap so checked borders do not merge.
            button.Margin = New Padding(button.Margin.Left, button.Margin.Top, 1, button.Margin.Bottom)
        Next
    End Sub

    Private Function IsIndependentToolbarToggle(ByVal button As ToolStripButton) As Boolean
        If IsExclusiveToolbarToggle(button) Then Return False
        If button.CheckOnClick Then Return True

        Select Case button.Name
            Case "TBWavIncrease"
                Return True
        End Select

        Return False
    End Function

    Private Function IsExclusiveToolbarToggle(ByVal button As ToolStripButton) As Boolean
        Select Case button.Name
            Case "TBTimeSelect", "TBSelect", "TBWrite"
                Return True
        End Select

        Return False
    End Function

    Private Sub InitializeDefinitionContextMenu()
        DefinitionContextMenu = New ContextMenuStrip(components)
        DefinitionContextDelete = New ToolStripMenuItem With {
            .Name = "DefinitionContextDelete",
            .Text = RemoveMenuAccessKeys(mnDelete.Text)
        }
        DefinitionContextMenu.Items.Add(DefinitionContextDelete)

        LWAV.ContextMenuStrip = DefinitionContextMenu
        LBMP.ContextMenuStrip = DefinitionContextMenu
        AddHandler DefinitionContextMenu.Opening, AddressOf DefinitionContextMenu_Opening
        AddHandler DefinitionContextDelete.Click, AddressOf DefinitionContextDelete_Click
        AddHandler LWAV.MouseDown, AddressOf DefinitionList_MouseDown
        AddHandler LBMP.MouseDown, AddressOf DefinitionList_MouseDown
    End Sub

    Private Sub DefinitionContextMenu_Opening(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs)
        DefinitionContextList = TryCast(DefinitionContextMenu.SourceControl, ListBox)
        DefinitionContextDelete.Text = RemoveMenuAccessKeys(mnDelete.Text)
        DefinitionContextDelete.Enabled = DefinitionContextList IsNot Nothing AndAlso DefinitionContextList.SelectedIndices.Count > 0
    End Sub

    Private Sub DefinitionList_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs)
        If e.Button <> MouseButtons.Right Then Return

        Dim xList As ListBox = DirectCast(sender, ListBox)
        Dim xIndex As Integer = xList.IndexFromPoint(e.Location)
        If xIndex < 0 Then Return

        If Not xList.SelectedIndices.Contains(xIndex) Then
            xList.ClearSelected()
            xList.SelectedIndex = xIndex
        End If
    End Sub

    Private Sub DefinitionContextDelete_Click(ByVal sender As Object, ByVal e As EventArgs)
        If Object.ReferenceEquals(DefinitionContextList, LWAV) Then
            ClearSelectedWavDefinitions()
        ElseIf Object.ReferenceEquals(DefinitionContextList, LBMP) Then
            ClearSelectedBmpDefinitions()
        End If
    End Sub

    Private Sub ClearSelectedWavDefinitions()
        If LWAV.SelectedIndices.Count = 0 Then Return

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo
        Dim xChanged As Boolean = False

        Dim xIndices(LWAV.SelectedIndices.Count - 1) As Integer
        LWAV.SelectedIndices.CopyTo(xIndices, 0)
        For Each xIndex As Integer In xIndices
            If SetDefinitionValueWithUndo(True, xIndex + 1, "", xUndo, xRedo) Then xChanged = True
        Next

        If xChanged Then AddUndo(xUndo, xBaseRedo.Next)
    End Sub

    Private Sub ClearSelectedBmpDefinitions()
        If LBMP.SelectedIndices.Count = 0 Then Return

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo
        Dim xChanged As Boolean = False

        Dim xIndices(LBMP.SelectedIndices.Count - 1) As Integer
        LBMP.SelectedIndices.CopyTo(xIndices, 0)
        For Each xIndex As Integer In xIndices
            If SetDefinitionValueWithUndo(False, xIndex + 1, "", xUndo, xRedo) Then xChanged = True
        Next

        If xChanged Then AddUndo(xUndo, xBaseRedo.Next)
    End Sub

    Private Sub InitializeOptionsMenuItems()
        mnSlashGrid = New ToolStripMenuItem With {
            .Image = My.Resources.Shortcut,
            .Name = "mnSlashGrid",
            .ShortcutKeyDisplayString = "/",
            .Text = "Slash key grid value"
        }

        Dim menuIndex As Integer = mnOptions.DropDownItems.IndexOf(ToolStripSeparator20)
        If menuIndex >= 0 Then
            mnOptions.DropDownItems.Insert(menuIndex, mnSlashGrid)
        Else
            mnOptions.DropDownItems.Add(mnSlashGrid)
        End If
    End Sub

    Private Sub KeepOptionsPanelDockedRight()
        If Not Object.ReferenceEquals(POptionsScroll.Parent, ToolStripContainer1.ContentPanel) Then Return
        If Not Object.ReferenceEquals(POptionsResizer.Parent, ToolStripContainer1.ContentPanel) Then Return

        Dim xControls As Control.ControlCollection = ToolStripContainer1.ContentPanel.Controls
        xControls.SetChildIndex(POptionsScroll, xControls.Count - 1)
        xControls.SetChildIndex(POptionsResizer, xControls.Count - 2)
    End Sub

    Private Sub InitializeOptionsTabs()
        OptionsTabsInitialized = True
        SyncOptionsTabTitles()
        SelectOptionsTab(POHeader, POHeaderTabButton)
    End Sub

    Private Function GetOptionsTabPanels() As Panel()
        Return New Panel() {POHeader, POWAV, POBMP, POBeat, POWaveForm}
    End Function

    Private Function GetOptionsTabButtons() As Button()
        Return New Button() {POHeaderTabButton, POWAVTabButton, POBMPTabButton, POBeatTabButton, POWaveFormTabButton}
    End Function

    Private Sub OptionsTabButton_Click(ByVal sender As Object, ByVal e As EventArgs) Handles POHeaderTabButton.Click, POWAVTabButton.Click, POBMPTabButton.Click, POBeatTabButton.Click, POWaveFormTabButton.Click
        Dim xButton As Button = TryCast(sender, Button)
        If xButton Is Nothing Then Return

        If Object.ReferenceEquals(xButton, POHeaderTabButton) Then SelectOptionsTab(POHeader, xButton)
        If Object.ReferenceEquals(xButton, POWAVTabButton) Then SelectOptionsTab(POWAV, xButton)
        If Object.ReferenceEquals(xButton, POBMPTabButton) Then SelectOptionsTab(POBMP, xButton)
        If Object.ReferenceEquals(xButton, POBeatTabButton) Then SelectOptionsTab(POBeat, xButton)
        If Object.ReferenceEquals(xButton, POWaveFormTabButton) Then SelectOptionsTab(POWaveForm, xButton)
    End Sub

    Private Sub SelectOptionsTab(ByVal xSelectedPanel As Panel, ByVal xSelectedButton As Button)
        POTabSelected = xSelectedPanel

        For Each xPanel As Panel In GetOptionsTabPanels()
            xPanel.Visible = Object.ReferenceEquals(xPanel, xSelectedPanel)
        Next

        For Each xButton As Button In GetOptionsTabButtons()
            ApplyOptionsTabButtonStyle(xButton, Object.ReferenceEquals(xButton, xSelectedButton))
        Next

        xSelectedPanel.BringToFront()
        ResetOptionsTabScroll(xSelectedPanel)
        QueueResetSelectedOptionsTabScroll()
    End Sub

    Private Sub ResetOptionsTabScroll(ByVal xPanel As ScrollableControl)
        If xPanel Is Nothing Then Return

        Dim xScrollPanel As ScrollableControl = xPanel
        If xPanel.Parent IsNot Nothing AndAlso TypeOf xPanel.Parent Is ScrollableControl Then
            xScrollPanel = CType(xPanel.Parent, ScrollableControl)
        End If

        xScrollPanel.AutoScrollPosition = Point.Empty
        xScrollPanel.PerformLayout()
    End Sub

    Private Sub ResetSelectedOptionsTabScroll()
        If POTabSelected Is Nothing Then Return
        ResetOptionsTabScroll(POTabSelected)
    End Sub

    Private Sub QueueResetSelectedOptionsTabScroll()
        If Me.IsDisposed OrElse Not Me.IsHandleCreated Then Return

        Try
            BeginInvoke(New MethodInvoker(AddressOf ResetSelectedOptionsTabScroll))
        Catch ex As InvalidOperationException
        End Try
    End Sub

    Private Sub SyncOptionsTabTitles()
        ApplyOptionsTabButtonSizes()

        For Each xButton As Button In GetOptionsTabButtons()
            ApplyOptionsTabButtonStyle(xButton, IsSelectedOptionsTabButton(xButton))
        Next
    End Sub

    Private Function IsSelectedOptionsTabButton(ByVal xButton As Button) As Boolean
        If Object.ReferenceEquals(POTabSelected, POHeader) Then Return Object.ReferenceEquals(xButton, POHeaderTabButton)
        If Object.ReferenceEquals(POTabSelected, POWAV) Then Return Object.ReferenceEquals(xButton, POWAVTabButton)
        If Object.ReferenceEquals(POTabSelected, POBMP) Then Return Object.ReferenceEquals(xButton, POBMPTabButton)
        If Object.ReferenceEquals(POTabSelected, POBeat) Then Return Object.ReferenceEquals(xButton, POBeatTabButton)
        If Object.ReferenceEquals(POTabSelected, POWaveForm) Then Return Object.ReferenceEquals(xButton, POWaveFormTabButton)

        Return False
    End Function

    Private Sub ApplyOptionsTabButtonSizes()
        Dim xHeight As Integer = 23

        For Each xButton As Button In GetOptionsTabButtons()
            Dim xTextSize As Size = TextRenderer.MeasureText(xButton.Text, xButton.Font)
            xHeight = Math.Max(xHeight, xTextSize.Height + 9)
        Next

        Dim xRowHeight As Single = CSng(xHeight + 1)
        POTabButtons.RowStyles(0).Height = xRowHeight
        POTabButtons.RowStyles(1).Height = xRowHeight
        POTabButtons.Height = POTabButtons.Padding.Top + POTabButtons.Padding.Bottom + CInt(xRowHeight * 2)

        For Each xButton As Button In GetOptionsTabButtons()
            xButton.Height = xHeight
            xButton.MinimumSize = New Size(0, xHeight)
        Next
    End Sub

    Private Sub ApplyOptionsTabButtonStyle(ByVal xButton As Button, ByVal xSelected As Boolean)
        xButton.BackColor = If(xSelected, Color.FromArgb(218, 235, 249), Color.FromArgb(248, 248, 248))
        xButton.FlatAppearance.BorderColor = If(xSelected, Color.FromArgb(49, 119, 178), Color.FromArgb(205, 205, 205))
    End Sub

    Private Sub InitializeGridToolbar()
        TBGridDivide = CreateGridCombo("TBGridDivide", 47, GridDivideValueTexts())
        TBGridSub = CreateGridCombo("TBGridSub", 47, New String() {"1", "2", "3", "4", "6", "8", "12", "16"})
        TBGridHeight = CreateGridCombo("TBGridHeight", 50, New String() {"x0.25", "x0.5", "x0.75", "x1.0", "x1.25", "x1.5", "x2.0", "x3.0", "x4.0", "x5.0"})
        TBGridWidth = CreateGridCombo("TBGridWidth", 50, New String() {"x0.25", "x0.5", "x0.75", "x1.0", "x1.25", "x1.5", "x2.0", "x3.0", "x4.0", "x5.0"})
        TBDisableVertical = New ToolStripButton With {
            .CheckOnClick = True,
            .DisplayStyle = ToolStripItemDisplayStyle.Image,
            .Image = My.Resources.x16VerticalLock,
            .ImageTransparentColor = Color.Magenta,
            .Name = "TBDisableVertical",
            .Text = CGDisableVertical.Text
        }
        TBGridSnap = New ToolStripButton With {
            .CheckOnClick = True,
            .DisplayStyle = ToolStripItemDisplayStyle.Image,
            .Image = My.Resources.pgmbl,
            .ImageTransparentColor = Color.Magenta,
            .Name = "TBGridSnap",
            .Text = CGSnap.Text
        }

        Dim gridItems As ToolStripItem() = {
            New ToolStripSeparator With {.Name = "ToolStripSeparatorGridStart"},
            New ToolStripLabel With {.Name = "TBGridDivideLabel", .Text = "Grid"},
            TBGridDivide,
            New ToolStripLabel With {.Name = "TBGridSubLabel", .Text = "補助"},
            TBGridSub,
            New ToolStripSeparator With {.Name = "ToolStripSeparatorGridScale"},
            New ToolStripLabel With {.Name = "TBGridHeightLabel", .Text = "高さ"},
            TBGridHeight,
            New ToolStripLabel With {.Name = "TBGridWidthLabel", .Text = "幅"},
            TBGridWidth,
            New ToolStripSeparator With {.Name = "ToolStripSeparatorDisableVertical"},
            TBDisableVertical,
            TBGridSnap
        }

        TBMain.Items.AddRange(gridItems)

        RefreshGridToolbar()
        RefreshDisableVerticalToolbar()
        RefreshGridSnapToolbar()
    End Sub

    Private Function CreateGridCombo(ByVal name As String, ByVal width As Integer, ByVal values() As String) As ToolStripComboBox
        Dim combo As New ToolStripComboBox With {
            .AutoSize = False,
            .DropDownStyle = ComboBoxStyle.DropDown,
            .Name = name,
            .Margin = New Padding(0, 0, 1, 0),
            .Size = New Size(width, 25)
        }
        combo.ComboBox.FlatStyle = FlatStyle.Standard
        combo.ComboBox.Tag = combo
        For Each value As String In values
            combo.Items.Add(value)
        Next

        AddHandler combo.ComboBox.Leave, AddressOf TBGridCombo_Leave
        AddHandler combo.ComboBox.KeyDown, AddressOf TBGridCombo_KeyDown
        AddHandler combo.ComboBox.MouseWheel, AddressOf TBGridCombo_MouseWheel
        Return combo
    End Function

    Private Sub RefreshGridToolbar()
        If TBGridDivide Is Nothing Then Return
        If UpdatingGridToolbar Then Return

        RefreshGridDivideItems()

        UpdatingGridToolbar = True
        TBGridDivide.Text = CInt(CGDivide.Value).ToString()
        TBGridSub.Text = CInt(CGSub.Value).ToString()
        TBGridHeight.Text = GridScaleText(CGHeight.Value)
        TBGridWidth.Text = GridScaleText(CGWidth.Value)
        TBGridDivide.ToolTipText = TBGridDivide.Text
        TBGridSub.ToolTipText = TBGridSub.Text
        TBGridHeight.ToolTipText = TBGridHeight.Text
        TBGridWidth.ToolTipText = TBGridWidth.Text
        UpdatingGridToolbar = False
    End Sub

    Private Function GridPartitionLimit() As Integer
        If BMSGridLimit <= 0 Then Return CInt(CGDivide.Maximum)

        Dim xLimit As Integer = CInt(Math.Round(192.0R / BMSGridLimit))
        Return Math.Max(CInt(CGDivide.Minimum), xLimit)
    End Function

    Private Function GridDivideValueTexts() As String()
        Dim xValues As New List(Of Integer)
        Dim xLimit As Integer = GridPartitionLimit()

        xValues.Add(1)
        AddDoublingGridDivideValues(xValues, 2, xLimit)
        AddDoublingGridDivideValues(xValues, 3, xLimit)
        xValues.Sort()

        Dim xTexts(xValues.Count - 1) As String
        For i As Integer = 0 To xValues.Count - 1
            xTexts(i) = xValues(i).ToString()
        Next

        Return xTexts
    End Function

    Private Sub AddDoublingGridDivideValues(ByVal values As List(Of Integer), ByVal value As Integer, ByVal limit As Integer)
        Do While value <= limit
            values.Add(value)
            If value > Integer.MaxValue \ 2 Then Exit Do
            value *= 2
        Loop
    End Sub

    Private Sub RefreshGridDivideItems()
        Dim xWasUpdating As Boolean = UpdatingGridToolbar
        UpdatingGridToolbar = True
        Try
            Dim xLimit As Integer = GridPartitionLimit()

            If CGDivide.Value > xLimit Then CGDivide.Value = xLimit
            If CGSub.Value > xLimit Then CGSub.Value = xLimit
            CGDivide.Maximum = xLimit
            CGSub.Maximum = xLimit
            If gSlash > xLimit Then gSlash = xLimit

            ReplaceGridComboItems(TBGridDivide, GridDivideValueTexts())
        Finally
            UpdatingGridToolbar = xWasUpdating
        End Try
    End Sub

    Private Sub ReplaceGridComboItems(ByVal item As ToolStripComboBox, ByVal values() As String)
        If item Is Nothing Then Return

        Dim xText As String = item.Text
        If item.Items.Count = values.Length Then
            Dim xSame As Boolean = True
            For i As Integer = 0 To values.Length - 1
                If item.Items(i).ToString() <> values(i) Then
                    xSame = False
                    Exit For
                End If
            Next
            If xSame Then Return
        End If

        item.Items.Clear()
        item.Items.AddRange(values)
        item.Text = xText
    End Sub

    Private Function GridScaleText(ByVal value As Decimal) As String
        Dim xText As String = value.ToString("0.##", Globalization.CultureInfo.InvariantCulture)
        If Not xText.Contains("."c) Then xText &= ".0"
        Return "x" & xText
    End Function

    Private Sub ApplyGridToolbarValue(ByVal item As ToolStripComboBox)
        If UpdatingGridToolbar OrElse item Is Nothing Then Return

        Dim intValue As Integer
        Dim decimalValue As Decimal
        If Object.ReferenceEquals(item, TBGridDivide) Then
            If TryGridInteger(item.Text, intValue, CGDivide.Minimum, CGDivide.Maximum) Then CGDivide.Value = intValue
        ElseIf Object.ReferenceEquals(item, TBGridSub) Then
            If TryGridInteger(item.Text, intValue, CGSub.Minimum, CGSub.Maximum) Then CGSub.Value = intValue
        ElseIf Object.ReferenceEquals(item, TBGridHeight) Then
            If TryGridScale(item.Text, decimalValue, CGHeight.Minimum, CGHeight.Maximum) Then CGHeight.Value = decimalValue
        ElseIf Object.ReferenceEquals(item, TBGridWidth) Then
            If TryGridScale(item.Text, decimalValue, CGWidth.Minimum, CGWidth.Maximum) Then CGWidth.Value = decimalValue
        End If

        RefreshGridToolbar()
    End Sub

    Private Function TryGridInteger(ByVal text As String, ByRef value As Integer, ByVal minimum As Decimal, ByVal maximum As Decimal) As Boolean
        If Not Integer.TryParse(text.Trim(), value) Then Return False
        Return value >= minimum AndAlso value <= maximum
    End Function

    Private Function TryGridScale(ByVal text As String, ByRef value As Decimal, ByVal minimum As Decimal, ByVal maximum As Decimal) As Boolean
        Dim xText As String = text.Trim()
        If xText.StartsWith("x", StringComparison.OrdinalIgnoreCase) OrElse xText.StartsWith("×") Then xText = xText.Substring(1)
        If Not Decimal.TryParse(xText, Globalization.NumberStyles.Number, Globalization.CultureInfo.CurrentCulture, value) AndAlso
           Not Decimal.TryParse(xText, Globalization.NumberStyles.Number, Globalization.CultureInfo.InvariantCulture, value) Then Return False

        Return value >= minimum AndAlso value <= maximum
    End Function

    Private Function TryGridComboValue(ByVal item As ToolStripComboBox, ByVal text As String, ByRef value As Decimal) As Boolean
        Dim intValue As Integer
        If Object.ReferenceEquals(item, TBGridDivide) Then
            If Not TryGridInteger(text, intValue, CGDivide.Minimum, CGDivide.Maximum) Then
                Return False
            End If
            value = intValue
            Return True
        ElseIf Object.ReferenceEquals(item, TBGridSub) Then
            If Not TryGridInteger(text, intValue, CGSub.Minimum, CGSub.Maximum) Then
                Return False
            End If
            value = intValue
            Return True
        ElseIf Object.ReferenceEquals(item, TBGridHeight) Then
            Return TryGridScale(text, value, CGHeight.Minimum, CGHeight.Maximum)
        ElseIf Object.ReferenceEquals(item, TBGridWidth) Then
            Return TryGridScale(text, value, CGWidth.Minimum, CGWidth.Maximum)
        End If

        Return False
    End Function

    Private Function FindGridComboIndex(ByVal item As ToolStripComboBox, ByVal value As Decimal, ByVal direction As Integer) As Integer
        If direction > 0 Then
            For i As Integer = 0 To item.Items.Count - 1
                Dim itemValue As Decimal
                If Not TryGridComboValue(item, item.Items(i).ToString(), itemValue) Then Continue For
                If itemValue > value Then
                    Return i
                End If
                If itemValue = value Then
                    If i < item.Items.Count - 1 Then
                        Return i + 1
                    End If
                    Return -1
                End If
            Next
        Else
            For i As Integer = item.Items.Count - 1 To 0 Step -1
                Dim itemValue As Decimal
                If Not TryGridComboValue(item, item.Items(i).ToString(), itemValue) Then Continue For
                If itemValue < value Then
                    Return i
                End If
                If itemValue = value Then
                    If i > 0 Then
                        Return i - 1
                    End If
                    Return -1
                End If
            Next
        End If

        Return -1
    End Function

    Private Function MoveGridCombo(ByVal combo As ComboBox, ByVal direction As Integer) As Boolean
        Dim item As ToolStripComboBox = TryCast(combo.Tag, ToolStripComboBox)
        If item Is Nothing Then
            Return False
        End If

        Dim text As String = combo.Text
        If item.SelectedIndex >= 0 AndAlso String.Equals(item.Items(item.SelectedIndex).ToString(), text, StringComparison.OrdinalIgnoreCase) Then
            Return False
        End If

        Return MoveGridComboValue(item, direction)
    End Function

    Private Function MoveGridComboValue(ByVal item As ToolStripComboBox, ByVal direction As Integer) As Boolean
        If item Is Nothing Then Return False

        Dim text As String = item.Text
        Dim value As Decimal
        If Not TryGridComboValue(item, text, value) Then
            Return False
        End If

        Dim index As Integer = FindGridComboIndex(item, value, direction)
        If index < 0 Then
            Return False
        End If

        item.SelectedIndex = index
        ApplyGridToolbarValue(item)
        Return True
    End Function

    Private Sub TBGridCombo_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles TBGridDivide.SelectedIndexChanged, TBGridSub.SelectedIndexChanged, TBGridHeight.SelectedIndexChanged, TBGridWidth.SelectedIndexChanged
        ApplyGridToolbarValue(DirectCast(sender, ToolStripComboBox))
    End Sub

    Private Sub TBGridCombo_Leave(ByVal sender As Object, ByVal e As EventArgs)
        ApplyGridToolbarValue(DirectCast(DirectCast(sender, ComboBox).Tag, ToolStripComboBox))
    End Sub

    Private Sub TBGridCombo_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
        If e.KeyCode = Keys.Up OrElse e.KeyCode = Keys.Down Then
            If MoveGridCombo(DirectCast(sender, ComboBox), If(e.KeyCode = Keys.Down, 1, -1)) Then
                e.SuppressKeyPress = True
                Return
            End If
        End If

        If e.KeyCode <> Keys.Enter Then Return

        ApplyGridToolbarValue(DirectCast(DirectCast(sender, ComboBox).Tag, ToolStripComboBox))
        e.SuppressKeyPress = True
    End Sub

    Private Sub TBGridCombo_MouseWheel(ByVal sender As Object, ByVal e As MouseEventArgs)
        If e.Delta = 0 Then
            Return
        End If

        If MoveGridCombo(DirectCast(sender, ComboBox), If(e.Delta < 0, 1, -1)) Then
            Dim handledArgs As HandledMouseEventArgs = TryCast(e, HandledMouseEventArgs)
            If handledArgs IsNot Nothing Then handledArgs.Handled = True
        End If
    End Sub

    Private Sub TBDisableVertical_Click(ByVal sender As Object, ByVal e As EventArgs) Handles TBDisableVertical.Click
        CGDisableVertical.Checked = TBDisableVertical.Checked
    End Sub

    Private Sub TBGridSnap_Click(ByVal sender As Object, ByVal e As EventArgs) Handles TBGridSnap.Click
        CGSnap.Checked = TBGridSnap.Checked
    End Sub

    Private Sub RefreshDisableVerticalToolbar()
        If TBDisableVertical Is Nothing Then Return

        TBDisableVertical.Checked = CGDisableVertical.Checked
        TBDisableVertical.Image = If(CGDisableVertical.Checked, My.Resources.x16VerticalLock, My.Resources.x16VerticalLockN)
        TBDisableVertical.Text = CGDisableVertical.Text
        TBDisableVertical.ToolTipText = CGDisableVertical.Text
    End Sub

    Private Sub RefreshGridSnapToolbar()
        If TBGridSnap Is Nothing Then Return

        TBGridSnap.Checked = CGSnap.Checked
        TBGridSnap.Text = CGSnap.Text
        TBGridSnap.ToolTipText = CGSnap.Text
    End Sub

    Private Sub InitializePlayerSelector()
        TBPlayer = New ToolStripComboBox With {
            .AutoSize = False,
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Name = "TBPlayer",
            .Margin = New Padding(0, 0, 1, 0),
            .Size = New Size(PlayerSelectorWidth(), 25)
        }
        TBPlayer.ComboBox.FlatStyle = FlatStyle.Standard

        Dim toolbarIndex As Integer = TBMain.Items.IndexOf(TBPlayB)
        If toolbarIndex >= 0 Then
            TBMain.Items.Insert(toolbarIndex, TBPlayer)
        Else
            TBMain.Items.Add(TBPlayer)
        End If

        RefreshPlayerSelector()
    End Sub

    Private Function PlayerSelectorWidth() As Integer
        Dim xWidth As Integer = 0
        For Each xArg As PlayerArguments In DefaultPlayerArguments()
            xWidth = Math.Max(xWidth, TextRenderer.MeasureText(PlayerDisplayName(xArg.Path), Me.Font, Size.Empty, TextFormatFlags.NoPadding).Width)
        Next

        Return xWidth + SystemInformation.VerticalScrollBarWidth + 6
    End Function

    Private Sub RefreshPlayerSelector()
        If TBPlayer Is Nothing OrElse pArgs Is Nothing OrElse pArgs.Length = 0 Then Return

        CurrentPlayer = Math.Min(pArgs.Length - 1, Math.Max(0, CurrentPlayer))
        UpdatingPlayerSelector = True
        TBPlayer.Items.Clear()
        For i As Integer = 0 To pArgs.Length - 1
            TBPlayer.Items.Add(PlayerDisplayName(pArgs(i).Path))
        Next

        TBPlayer.SelectedIndex = CurrentPlayer
        TBPlayer.ToolTipText = TBPlayer.Text
        UpdatingPlayerSelector = False
    End Sub

    Private Sub TBPlayer_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs) Handles TBPlayer.SelectedIndexChanged
        If UpdatingPlayerSelector OrElse TBPlayer.SelectedIndex < 0 Then Return

        CurrentPlayer = TBPlayer.SelectedIndex
        TBPlayer.ToolTipText = TBPlayer.Text
    End Sub

    Private Sub InitializeSplitterControls()
        mnSyncSplitterScroll = New ToolStripMenuItem With {
            .CheckOnClick = True,
            .Image = My.Resources.x16Lock,
            .Name = "mnSyncSplitterScroll",
            .Text = "Sync Scroll between Splitters"
        }
        mnSAddSplitter = New ToolStripMenuItem With {
            .Image = My.Resources.x16Add,
            .Name = "mnSAddSplitter",
            .Text = "Add Splitter"
        }
        mnSRemoveSplitter = New ToolStripMenuItem With {
            .Image = My.Resources.x16Remove,
            .Name = "mnSRemoveSplitter",
            .Text = "Remove Splitter"
        }

        If mnSys.DropDownItems.Contains(mnSLSplitter) Then mnSys.DropDownItems.Remove(mnSLSplitter)
        If mnSys.DropDownItems.Contains(mnSRSplitter) Then mnSys.DropDownItems.Remove(mnSRSplitter)

        ToolStripSeparatorSplitter = New ToolStripSeparator With {
            .Name = "ToolStripSeparatorSplitter"
        }
        TBAddSplitter = New ToolStripButton With {
            .DisplayStyle = ToolStripItemDisplayStyle.Image,
            .Image = My.Resources.x16Add,
            .ImageTransparentColor = Color.Magenta,
            .Name = "TBAddSplitter",
            .Text = "Add Splitter"
        }
        TBRemoveSplitter = New ToolStripButton With {
            .DisplayStyle = ToolStripItemDisplayStyle.Image,
            .Image = My.Resources.x16Remove,
            .ImageTransparentColor = Color.Magenta,
            .Name = "TBRemoveSplitter",
            .Text = "Remove Splitter"
        }
        TBSyncSplitterScroll = New ToolStripButton With {
            .CheckOnClick = True,
            .DisplayStyle = ToolStripItemDisplayStyle.Image,
            .Image = My.Resources.x16Lock,
            .ImageTransparentColor = Color.Magenta,
            .Name = "TBSyncSplitterScroll",
            .Text = "Sync Scroll between Splitters"
        }

        Dim toolbarIndex As Integer = TBMain.Items.IndexOf(ToolStripSeparator4)
        Dim splitterItems As ToolStripItem() = {ToolStripSeparatorSplitter, TBAddSplitter, TBRemoveSplitter, TBSyncSplitterScroll}
        If toolbarIndex >= 0 Then
            For i As Integer = 0 To splitterItems.Length - 1
                TBMain.Items.Insert(toolbarIndex + i, splitterItems(i))
            Next
        Else
            TBMain.Items.AddRange(splitterItems)
        End If

        RefreshSplitterControls()
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="xHPosition">Original horizontal position.</param>
    ''' <param name="xHSVal">HS.Value</param>


    Private Function HorizontalPositiontoDisplay(ByVal xHPosition As Integer, ByVal xHSVal As Long) As Integer
        Return CInt(xHPosition * gxWidth - xHSVal * gxWidth)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="xVPosition">Original vertical position.</param>
    ''' <param name="xVSVal">VS.Value</param>
    ''' <param name="xTHeight">Height of the panel. (DisplayRectangle, but not ClipRectangle)</param>


    Private Function NoteRowToPanelHeight(ByVal xVPosition As Double, ByVal xVSVal As Long, ByVal xTHeight As Integer) As Integer
        Return xTHeight - CInt((xVPosition + xVSVal) * gxHeight) - 1
    End Function

    Public Function MeasureAtDisplacement(ByVal xVPos As Double) As Integer
        'Return Math.Floor((xVPos + FloatTolerance) / 192)
        'Return Math.Floor(xVPos / 192)
        Dim xI1 As Integer
        For xI1 = 1 To 999
            If xVPos < MeasureBottom(xI1) Then Exit For
        Next
        Return xI1 - 1
    End Function

    Private Function GetMaxVPosition() As Double
        Return MeasureUpper(999)
    End Function

    Private Function SnapToGrid(ByVal xVPos As Double) As Double
        Dim xOffset As Double = MeasureBottom(MeasureAtDisplacement(xVPos))
        Dim xRatio As Double = 192.0R / gDivide
        Return Math.Floor((xVPos - xOffset) / xRatio) * xRatio + xOffset
    End Function

    Private Function AlignVScrollMinimumToWheel(ByVal value As Integer) As Integer
        If value >= 0 OrElse gWheel <= 0 Then Return value

        Dim xRemainder As Integer = Math.Abs(value) Mod gWheel
        If xRemainder = 0 Then Return value

        Return value - (gWheel - xRemainder)
    End Function

    Private Sub CalculateGreatestVPosition()
        'If K Is Nothing Then Exit Sub
        Dim xI1 As Integer
        GreatestVPosition = 0

        If NTInput Then
            For xI1 = UBound(Notes) To 0 Step -1
                If Notes(xI1).VPosition + Notes(xI1).Length > GreatestVPosition Then GreatestVPosition = Notes(xI1).VPosition + Notes(xI1).Length
            Next
        Else
            For xI1 = UBound(Notes) To 0 Step -1
                If Notes(xI1).VPosition > GreatestVPosition Then GreatestVPosition = Notes(xI1).VPosition
            Next
        End If

        Dim xLimit As Integer = CInt(IIf(GreatestVPosition + 2000 > GetMaxVPosition(), GetMaxVPosition, GreatestVPosition + 2000))
        Dim xI2 As Integer = AlignVScrollMinimumToWheel(-xLimit)
        For i As Integer = 0 To SplitPanes.Count - 1
            SplitPanes(i).VScroll.Minimum = xI2
        Next
    End Sub

    Private Sub CalculateGreatestColumn()
        UpdateColumnLefts()
        Dim xColumns As Integer = Math.Min(MaxBGMColumns, Math.Max(1, CInt(CGB.Value)))
        xColumns = Math.Max(xColumns, CalculateVisibleBGMColumns())

        For xI1 As Integer = 1 To UBound(Notes)
            If Notes(xI1).ColumnIndex >= niB Then
                xColumns = Math.Max(xColumns, Notes(xI1).ColumnIndex - niB + 1 + BGMColumnTailMargin)
            End If
        Next

        xColumns = Math.Min(MaxBGMColumns, xColumns)
        gColumns = niB + xColumns - 1
        UpdateColumnsX()
    End Sub

    Private Function CalculateVisibleBGMColumns() As Integer
        If PMainIn Is Nothing OrElse column(niB).Width <= 0 OrElse gxWidth <= 0 Then
            Return 1
        End If

        Dim xWidth As Integer = 0
        For i As Integer = 0 To SplitPanes.Count - 1
            xWidth = Math.Max(xWidth, SplitPanes(i).Canvas.Width)
        Next
        If xWidth <= 0 Then
            Return 1
        End If

        Dim xRight As Integer = CInt(Math.Ceiling(xWidth / CDbl(gxWidth)))
        Dim xColumns As Integer = CInt(Math.Ceiling((xRight - column(niB).Left) / CDbl(column(niB).Width)))
        Return Math.Min(MaxBGMColumns, Math.Max(1, xColumns))
    End Function


    Private Sub SortByVPositionInsertion() 'Insertion Sort
        If UBound(Notes) <= 0 Then Exit Sub
        Dim xNote As Note
        Dim xI1 As Integer
        Dim xI2 As Integer
        For xI1 = 2 To UBound(Notes)
            xNote = Notes(xI1)
            For xI2 = xI1 - 1 To 1 Step -1
                If Notes(xI2).VPosition > xNote.VPosition Then
                    Notes(xI2 + 1) = Notes(xI2)
                    '                    If KMouseDown = xI2 Then KMouseDown += 1
                    If xI2 = 1 Then
                        Notes(xI2) = xNote
                        '                       If KMouseDown = xI1 Then KMouseDown = xI2
                        Exit For
                    End If
                Else
                    Notes(xI2 + 1) = xNote
                    '                    If KMouseDown = xI1 Then KMouseDown = xI2 + 1
                    Exit For
                End If
            Next
        Next

    End Sub

    Private Sub SortByVPositionQuick(ByVal xMin As Integer, ByVal xMax As Integer) 'Quick Sort
        Dim xNote As Note
        Dim iHi As Integer
        Dim iLo As Integer
        Dim xI1 As Integer

        ' If min >= max, the list contains 0 or 1 items so it is sorted.
        If xMin >= xMax Then Exit Sub

        ' Pick the dividing value.
        xI1 = CInt((xMax - xMin) / 2) + xMin
        xNote = Notes(xI1)

        ' Swap it to the front.
        Notes(xI1) = Notes(xMin)

        iLo = xMin
        iHi = xMax
        Do
            ' Look down from hi for a value < med_value.
            Do While Notes(iHi).VPosition >= xNote.VPosition
                iHi = iHi - 1
                If iHi <= iLo Then Exit Do
            Loop
            If iHi <= iLo Then
                Notes(iLo) = xNote
                Exit Do
            End If

            ' Swap the lo and hi values.
            Notes(iLo) = Notes(iHi)

            ' Look up from lo for a value >= med_value.
            iLo = iLo + 1
            Do While Notes(iLo).VPosition < xNote.VPosition
                iLo = iLo + 1
                If iLo >= iHi Then Exit Do
            Loop
            If iLo >= iHi Then
                iLo = iHi
                Notes(iHi) = xNote
                Exit Do
            End If

            ' Swap the lo and hi values.
            Notes(iHi) = Notes(iLo)
        Loop

        ' Sort the two sublists.
        SortByVPositionQuick(xMin, iLo - 1)
        SortByVPositionQuick(iLo + 1, xMax)
    End Sub

    Private Sub SortByVPositionQuick3(ByVal xMin As Integer, ByVal xMax As Integer)
        Dim xxMin As Integer
        Dim xxMax As Integer
        Dim xxMid As Integer
        Dim xNote As Note
        Dim xNoteMid As Note
        Dim xI1 As Integer
        Dim xI2 As Integer
        Dim xI3 As Integer

        'If xMax = 0 Then
        '    xMin = LBound(K1)
        '    xMax = UBound(K1)
        'End If
        xxMin = xMin
        xxMax = xMax
        xxMid = xMax - xMin + 1
        xI1 = CInt(Int(xxMid * Rnd())) + xMin
        xI2 = CInt(Int(xxMid * Rnd())) + xMin
        xI3 = CInt(Int(xxMid * Rnd())) + xMin
        If Notes(xI1).VPosition <= Notes(xI2).VPosition And Notes(xI2).VPosition <= Notes(xI3).VPosition Then
            xxMid = xI2
        Else
            If Notes(xI2).VPosition <= Notes(xI1).VPosition And Notes(xI1).VPosition <= Notes(xI3).VPosition Then
                xxMid = xI1
            Else
                xxMid = xI3
            End If
        End If
        xNoteMid = Notes(xxMid)
        Do
            Do While Notes(xxMin).VPosition < xNoteMid.VPosition And xxMin < xMax
                xxMin = xxMin + 1
            Loop
            Do While xNoteMid.VPosition < Notes(xxMax).VPosition And xxMax > xMin
                xxMax = xxMax - 1
            Loop
            If xxMin <= xxMax Then
                xNote = Notes(xxMin)
                Notes(xxMin) = Notes(xxMax)
                Notes(xxMax) = xNote
                xxMin = xxMin + 1
                xxMax = xxMax - 1
            End If
        Loop Until xxMin > xxMax
        If xxMax - xMin < xMax - xxMin Then
            If xMin < xxMax Then SortByVPositionQuick3(xMin, xxMax)
            If xxMin < xMax Then SortByVPositionQuick3(xxMin, xMax)
        Else
            If xxMin < xMax Then SortByVPositionQuick3(xxMin, xMax)
            If xMin < xxMax Then SortByVPositionQuick3(xMin, xxMax)
        End If
    End Sub


    Private Sub UpdateMeasureBottom()
        MeasureBottom(0) = 0.0#
        For xI1 As Integer = 0 To 998
            MeasureBottom(xI1 + 1) = MeasureBottom(xI1) + MeasureLength(xI1)
        Next
    End Sub

    Private Function PathIsValid(ByVal sPath As String) As Boolean
        Return File.Exists(sPath) Or Directory.Exists(sPath)
    End Function

    Private Function CurrentPreviewMeasure(ByVal skipClipped As Boolean) As Integer
        Dim vpos As Double = Math.Abs(PanelVScroll(PanelFocus))
        Dim xMeasure As Integer = MeasureAtDisplacement(vpos)

        If skipClipped AndAlso xMeasure < 999 AndAlso vpos > MeasureBottom(xMeasure) + 0.0001R Then
            xMeasure += 1
        End If

        Return xMeasure
    End Function

    Public Function PrevCodeToReal(ByVal InitStr As String, Optional ByVal skipClipped As Boolean = False) As String
        Dim xFileName As String = IIf(Not PathIsValid(FileName),
                                        IIf(InitPath = "", My.Application.Info.DirectoryPath, InitPath),
                                        ExcludeFileName(FileName)) _
                                        & "\___TempBMS.bms"
        Dim xMeasure As Integer = CurrentPreviewMeasure(skipClipped)
        Dim xS1 As String = Replace(InitStr, "<apppath>", My.Application.Info.DirectoryPath)
        Dim xS2 As String = Replace(xS1, "<measure>", xMeasure)
        Dim xS3 As String = Replace(xS2, "<filename>", xFileName)
        Return xS3
    End Function

    Private Sub SetFileName(ByVal xFileName As String)
        FileName = xFileName.Trim
        InitPath = ExcludeFileName(FileName)
        SetIsSaved(IsSaved)
    End Sub

    Private Sub SetIsSaved(ByVal isSaved As Boolean)
        'pttl.Refresh()
        'pIsSaved.Visible = Not xBool
        Dim xVersion As String = FormatVersionText(My.Application.Info.Version)
        Text = IIf(isSaved, "", "*") & GetFileName(FileName) & " - " & My.Application.Info.Title & " " & xVersion
        Me.IsSaved = isSaved
    End Sub

    Private Function FormatVersionText(ByVal xVersion As Version) As String
        If xVersion Is Nothing Then Return ""
        If xVersion.Build < 0 Then Return xVersion.Major & "." & xVersion.Minor

        Return xVersion.Major & "." & xVersion.Minor & "." & xVersion.Build
    End Function

    Private Sub PreviewNote(ByVal xFileLocation As String, ByVal bStop As Boolean)
        If bStop Then
            Audio.StopPlaying()
        End If
        Audio.Play(xFileLocation)
    End Sub

    Private Sub AddNote(note As Note,
               Optional ByVal xSelected As Boolean = False,
               Optional ByVal OverWrite As Boolean = True,
               Optional ByVal SortAndUpdatePairing As Boolean = True)

        If note.VPosition < 0 Or note.VPosition >= GetMaxVPosition() Then Exit Sub

        Dim xI1 As Integer = 1

        If OverWrite Then
            Do While xI1 <= UBound(Notes)
                If Notes(xI1).VPosition = note.VPosition And
                    Notes(xI1).ColumnIndex = note.ColumnIndex Then
                    RemoveNote(xI1)
                Else
                    xI1 += 1
                End If
            Loop
        End If

        ReDim Preserve Notes(UBound(Notes) + 1)
        note.Selected = note.Selected And nEnabled(note.ColumnIndex)
        Notes(UBound(Notes)) = note

        If TBWavIncrease.Checked Then
            If IsColumnSound(note.ColumnIndex) Then
                IncreaseCurrentWav()
            End If
            If IsColumnImage(note.ColumnIndex) Then
                IncreaseCurrentBmp()
            End If
        End If

        If SortAndUpdatePairing Then SortByVPositionInsertion() : UpdatePairing()
        CalculateTotalPlayableNotes()
    End Sub

    Private Sub RemoveNote(ByVal I As Integer, Optional ByVal SortAndUpdatePairing As Boolean = True)
        KMouseOver = -1
        Dim xI2 As Integer

        If TBWavIncrease.Checked Then
            If IsColumnSound(Notes(I).ColumnIndex) Then
                If Notes(I).Value = LWAV.SelectedIndex * 10000 Then
                    DecreaseCurrentWav()
                End If
            Else
                If Notes(I).Value = LBMP.SelectedIndex * 10000 Then
                    DecreaseCurrentBmp()
                End If
            End If
        End If

        For xI2 = I + 1 To UBound(Notes)
            Notes(xI2 - 1) = Notes(xI2)
        Next
        ReDim Preserve Notes(UBound(Notes) - 1)
        If SortAndUpdatePairing Then SortByVPositionInsertion() : UpdatePairing()

    End Sub

    Private Sub AddNotesFromClipboard(Optional ByVal xSelected As Boolean = True, Optional ByVal SortAndUpdatePairing As Boolean = True, Optional ByVal xBaseVPosition As Double = -1.0#)
        Dim xStrLine() As String = Split(Clipboard.GetText, vbCrLf)

        Dim xI1 As Integer
        For xI1 = 0 To UBound(Notes)
            Notes(xI1).Selected = False
        Next

        Dim xVS As Long = PanelVScroll(PanelFocus)
        Dim xPasteBase As Double = If(xBaseVPosition >= 0, xBaseVPosition, MeasureBottom(MeasureAtDisplacement(-xVS) + 1))
        Dim xTempVP As Double
        Dim xKbu() As Note = Notes

        If xStrLine(0) = "nBMSC Clipboard Data" Then
            If NTInput Then ReDim Preserve Notes(0)

            'paste
            Dim xStrSub() As String
            For xI1 = 1 To UBound(xStrLine)
                If xStrLine(xI1).Trim = "" Then Continue For
                xStrSub = Split(xStrLine(xI1), " ")
                xTempVP = Val(xStrSub(1)) + xPasteBase
                If UBound(xStrSub) = 5 And xTempVP >= 0 And xTempVP < GetMaxVPosition() Then
                    ReDim Preserve Notes(UBound(Notes) + 1)
                    With Notes(UBound(Notes))
                        .ColumnIndex = Val(xStrSub(0))
                        .VPosition = xTempVP
                        .Value = Val(xStrSub(2))
                        .LongNote = CBool(Val(xStrSub(3)))
                        .Hidden = CBool(Val(xStrSub(4)))
                        .Landmine = CBool(Val(xStrSub(5)))
                        .Selected = xSelected
                    End With
                    NormalizeNoteType(Notes(UBound(Notes)))
                End If
            Next

            'convert
            If NTInput Then
                ConvertBMSE2NT()

                For xI1 = 1 To UBound(Notes)
                    Notes(xI1 - 1) = Notes(xI1)
                Next
                ReDim Preserve Notes(UBound(Notes) - 1)

                Dim xKn() As Note = Notes
                Notes = xKbu

                Dim xIStart As Integer = Notes.Length
                ReDim Preserve Notes(UBound(Notes) + xKn.Length)

                For xI1 = xIStart To UBound(Notes)
                    Notes(xI1) = xKn(xI1 - xIStart)
                Next
            End If

        ElseIf xStrLine(0) = "nBMSC Clipboard Data xNT" Then
            If Not NTInput Then ReDim Preserve Notes(0)

            'paste
            Dim xStrSub() As String
            For xI1 = 1 To UBound(xStrLine)
                If xStrLine(xI1).Trim = "" Then Continue For
                xStrSub = Split(xStrLine(xI1), " ")
                xTempVP = Val(xStrSub(1)) + xPasteBase
                If UBound(xStrSub) = 5 And xTempVP >= 0 And xTempVP < GetMaxVPosition() Then
                    ReDim Preserve Notes(UBound(Notes) + 1)
                    With Notes(UBound(Notes))
                        .ColumnIndex = Val(xStrSub(0))
                        .VPosition = xTempVP
                        .Value = Val(xStrSub(2))
                        .Length = Val(xStrSub(3))
                        .Hidden = CBool(Val(xStrSub(4)))
                        .Landmine = CBool(Val(xStrSub(5)))
                        .Selected = xSelected
                    End With
                    NormalizeNoteType(Notes(UBound(Notes)))
                End If
            Next

            'convert
            If Not NTInput Then
                ConvertNT2BMSE()

                For xI1 = 1 To UBound(Notes)
                    Notes(xI1 - 1) = Notes(xI1)
                Next
                ReDim Preserve Notes(UBound(Notes) - 1)

                Dim xKn() As Note = Notes
                Notes = xKbu

                Dim xIStart As Integer = Notes.Length
                ReDim Preserve Notes(UBound(Notes) + xKn.Length)

                For xI1 = xIStart To UBound(Notes)
                    Notes(xI1) = xKn(xI1 - xIStart)
                Next
            End If

        ElseIf xStrLine(0) = "BMSE ClipBoard Object Data Format" Then
            If NTInput Then ReDim Preserve Notes(0)

            'paste
            For xI1 = 1 To UBound(xStrLine)
                ' zdr: holy crap this is obtuse
                Dim posStr = Mid(xStrLine(xI1), 5, 7)
                Dim vPos = Val(posStr) + xPasteBase

                Dim bmsIdent = Mid(xStrLine(xI1), 1, 3)
                Dim lineCol = BMSEChannelToColumnIndex(bmsIdent)

                Dim Value = Val(Mid(xStrLine(xI1), 12)) * 10000

                Dim attribute = Mid(xStrLine(xI1), 4, 1)

                Dim validCol = Len(xStrLine(xI1)) > 11 And lineCol > 0
                Dim inRange = vPos >= 0 And vPos < GetMaxVPosition()
                If validCol And inRange Then
                    ReDim Preserve Notes(UBound(Notes) + 1)

                    With Notes(UBound(Notes))
                        .ColumnIndex = lineCol
                        .VPosition = vPos
                        .Value = Value
                        .LongNote = attribute = "2"
                        .Hidden = attribute = "1"
                        .Selected = xSelected And nEnabled(.ColumnIndex)
                    End With
                End If
            Next

            'convert
            If NTInput Then
                ConvertBMSE2NT()

                For xI1 = 1 To UBound(Notes)
                    Notes(xI1 - 1) = Notes(xI1)
                Next
                ReDim Preserve Notes(UBound(Notes) - 1)

                Dim xKn() As Note = Notes
                Notes = xKbu

                Dim xIStart As Integer = Notes.Length
                ReDim Preserve Notes(UBound(Notes) + xKn.Length)

                For xI1 = xIStart To UBound(Notes)
                    Notes(xI1) = xKn(xI1 - xIStart)
                Next
            End If
        End If

        If SortAndUpdatePairing Then SortByVPositionInsertion() : UpdatePairing()
        CalculateTotalPlayableNotes()
    End Sub

    Private Sub CopyNotes(Optional ByVal Unselect As Boolean = True)
        Dim xStrAll As String = "nBMSC Clipboard Data" & IIf(NTInput, " xNT", "")
        Dim xI1 As Integer
        Dim MinMeasure As Double = 999

        For xI1 = 1 To UBound(Notes)
            If Notes(xI1).Selected And MeasureAtDisplacement(Notes(xI1).VPosition) < MinMeasure Then MinMeasure = MeasureAtDisplacement(Notes(xI1).VPosition)
        Next
        MinMeasure = MeasureBottom(MinMeasure)

        If Not NTInput Then
            For xI1 = 1 To UBound(Notes)
                If Notes(xI1).Selected Then
                    xStrAll &= vbCrLf & Notes(xI1).ColumnIndex.ToString & " " &
                                       (Notes(xI1).VPosition - MinMeasure).ToString & " " &
                                        Notes(xI1).Value.ToString & " " &
                                   CInt(Notes(xI1).LongNote).ToString & " " &
                                   CInt(Notes(xI1).Hidden).ToString & " " &
                                   CInt(Notes(xI1).Landmine).ToString
                    Notes(xI1).Selected = Not Unselect
                End If
            Next

        Else
            For xI1 = 1 To UBound(Notes)
                If Notes(xI1).Selected Then
                    xStrAll &= vbCrLf & Notes(xI1).ColumnIndex.ToString & " " &
                                       (Notes(xI1).VPosition - MinMeasure).ToString & " " &
                                        Notes(xI1).Value.ToString & " " &
                                        Notes(xI1).Length.ToString & " " &
                                   CInt(Notes(xI1).Hidden).ToString & " " &
                                   CInt(Notes(xI1).Landmine).ToString
                    Notes(xI1).Selected = Not Unselect
                End If
            Next
        End If

        Clipboard.SetText(xStrAll)
    End Sub

    Private Sub RemoveNotes(Optional ByVal SortAndUpdatePairing As Boolean = True)
        If UBound(Notes) = 0 Then Exit Sub

        KMouseOver = -1
        Dim xI1 As Integer = 1
        Dim xI2 As Integer
        Do
            If Notes(xI1).Selected Then
                For xI2 = xI1 + 1 To UBound(Notes)
                    Notes(xI2 - 1) = Notes(xI2)
                Next
                ReDim Preserve Notes(UBound(Notes) - 1)
                xI1 = 0
            End If
            xI1 += 1
        Loop While xI1 < UBound(Notes) + 1
        If SortAndUpdatePairing Then SortByVPositionInsertion() : UpdatePairing()
        CalculateTotalPlayableNotes()
    End Sub

    Private Function EnabledColumnIndexToColumnArrayIndex(ByVal cEnabled As Integer) As Integer
        Dim xI1 As Integer = 0
        Do
            If xI1 >= gColumns Then Exit Do
            If Not nEnabled(xI1) Then cEnabled += 1
            If xI1 >= cEnabled Then Exit Do
            xI1 += 1
        Loop
        Return cEnabled
    End Function

    Private Function ColumnArrayIndexToEnabledColumnIndex(ByVal cReal As Integer) As Integer
        Dim xI1 As Integer
        For xI1 = 0 To cReal - 1
            If Not nEnabled(xI1) Then cReal -= 1
        Next
        Return cReal
    End Function

    Private Sub Form1_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        If pTempFileNames IsNot Nothing Then
            For Each xStr As String In pTempFileNames
                IO.File.Delete(xStr)
            Next
        End If
        If PreviousAutoSavedFileName <> "" Then IO.File.Delete(PreviousAutoSavedFileName)
    End Sub

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Not IsSaved Then
            Dim xStr As String = Strings.Messages.SaveOnExit
            If e.CloseReason = CloseReason.WindowsShutDown Then xStr = Strings.Messages.SaveOnExit1
            If e.CloseReason = CloseReason.TaskManagerClosing Then xStr = Strings.Messages.SaveOnExit2

            Dim xResult As MsgBoxResult = MsgBox(xStr, MsgBoxStyle.YesNoCancel Or MsgBoxStyle.Question, Me.Text)

            If xResult = MsgBoxResult.Yes Then
                If ExcludeFileName(FileName) = "" Then
                    Dim xDSave As New SaveFileDialog
                    xDSave.Filter = Strings.FileType._bms & "|*.bms;*.bme;*.bml;*.pms;*.txt|" &
                                    Strings.FileType.BMS & "|*.bms|" &
                                    Strings.FileType.BME & "|*.bme|" &
                                    Strings.FileType.BML & "|*.bml|" &
                                    Strings.FileType.PMS & "|*.pms|" &
                                    Strings.FileType.TXT & "|*.txt|" &
                                    Strings.FileType._all & "|*.*"
                    xDSave.DefaultExt = "bms"
                    xDSave.InitialDirectory = InitPath

                    If xDSave.ShowDialog = Windows.Forms.DialogResult.Cancel Then e.Cancel = True : Exit Sub
                    SetFileName(xDSave.FileName)
                End If
                Dim xStrAll As String = SaveBMS()
                My.Computer.FileSystem.WriteAllText(FileName, xStrAll, False, TextEncoding)
                NewRecent(FileName)
                If BeepWhileSaved Then Beep()
            End If

            If xResult = MsgBoxResult.Cancel Then e.Cancel = True
        End If

        If Not e.Cancel Then
            'If SaveTheme Then
            '    My.Computer.FileSystem.WriteAllText(My.Application.Info.DirectoryPath & "\Skin.cff", SaveSkinCFF, False, System.Text.Encoding.Unicode)
            'Else
            '    My.Computer.FileSystem.WriteAllText(My.Application.Info.DirectoryPath & "\Skin.cff", "", False, System.Text.Encoding.Unicode)
            'End If
            '
            'My.Computer.FileSystem.WriteAllText(My.Application.Info.DirectoryPath & "\PlayerArgs.cff", SavePlayerCFF, False, System.Text.Encoding.Unicode)
            'My.Computer.FileSystem.WriteAllText(My.Application.Info.DirectoryPath & "\Config.cff", SaveCFF, False, System.Text.Encoding.Unicode)
            'My.Computer.FileSystem.WriteAllText(My.Application.Info.DirectoryPath & "\PreConfig.cff", "", False, System.Text.Encoding.Unicode)
            Me.SaveSettings(My.Application.Info.DirectoryPath & "\nBMSC.Settings.xml", False)
        End If
    End Sub

    Private Function FilterFileBySupported(ByVal xFile() As String, ByVal xFilter() As String) As String()
        Dim xPath(-1) As String
        For xI1 As Integer = 0 To UBound(xFile)
            If My.Computer.FileSystem.FileExists(xFile(xI1)) And Array.IndexOf(xFilter, Path.GetExtension(xFile(xI1))) <> -1 Then
                ReDim Preserve xPath(UBound(xPath) + 1)
                xPath(UBound(xPath)) = xFile(xI1)
            End If

            If My.Computer.FileSystem.DirectoryExists(xFile(xI1)) Then
                Dim xFileNames() As FileInfo = My.Computer.FileSystem.GetDirectoryInfo(xFile(xI1)).GetFiles()
                For Each xStr As FileInfo In xFileNames
                    If Array.IndexOf(xFilter, xStr.Extension) = -1 Then Continue For
                    ReDim Preserve xPath(UBound(xPath) + 1)
                    xPath(UBound(xPath)) = xStr.FullName
                Next
            End If
        Next

        Return xPath
    End Function

    Private Sub InitializeNewBMS()
        'ReDim K(0)
        'With K(0)
        ' .ColumnIndex = niBPM
        ' .VPosition = -1
        ' .LongNote = False
        ' .Selected = False
        ' .Value = 1200000
        'End With

        THTitle.Text = ""
        THArtist.Text = ""
        THGenre.Text = ""
        THBPM.Value = 120
        If CHPlayer.SelectedIndex = -1 Then CHPlayer.SelectedIndex = 0
        CHRank.SelectedIndex = 3
        THPlayLevel.Text = ""
        THSubTitle.Text = ""
        THSubArtist.Text = ""
        THStageFile.Text = ""
        THBanner.Text = ""
        THBackBMP.Text = ""
        CHDifficulty.SelectedIndex = 0
        THExRank.Text = ""
        THTotal.Text = ""
        THComment.Text = ""
        'THLnType.Text = "1"
        CHLnObj.SelectedIndex = 0

        THLandMine.Text = ""
        THMissBMP.Text = ""
        TExpansion.Text = ""

        THPreview.Text = ""
        CHLnmode.SelectedIndex = 0

        LBeat.Items.Clear()
        For xI1 As Integer = 0 To 999
            MeasureLength(xI1) = 192.0R
            MeasureBottom(xI1) = xI1 * 192.0R
            LBeat.Items.Add(Add3Zeros(xI1) & ": 1 ( 4 / 4 )")
        Next
    End Sub

    Private Sub InitializeOpenBMS()
        CHPlayer.SelectedIndex = 0
        'THLnType.Text = ""
    End Sub

    Private Sub Form1_DragEnter(ByVal sender As Object, ByVal e As DragEventArgs) Handles Me.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
            DDFileName = FilterFileBySupported(CType(e.Data.GetData(DataFormats.FileDrop), String()), SupportedFileExtension)
        Else
            e.Effect = DragDropEffects.None
        End If
        RefreshPanelAll()
    End Sub

    Private Sub Form1_DragLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.DragLeave
        ReDim DDFileName(-1)
        RefreshPanelAll()
    End Sub

    Private Sub Form1_DragDrop(ByVal sender As Object, ByVal e As DragEventArgs) Handles Me.DragDrop
        ReDim DDFileName(-1)
        If Not e.Data.GetDataPresent(DataFormats.FileDrop) Then Return

        Dim xOrigPath() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
        Dim xPath() As String = FilterFileBySupported(xOrigPath, SupportedFileExtension)
        If xPath.Length > 0 Then
            Dim xProg As New fLoadFileProgress(xPath, IsSaved)
            xProg.ShowDialog(Me)
        End If

        RefreshPanelAll()
    End Sub

    Private Sub setFullScreen(ByVal value As Boolean)
        If value Then
            If Me.WindowState = FormWindowState.Minimized Then Exit Sub

            Me.SuspendLayout()
            previousWindowPosition.Location = Me.Location
            previousWindowPosition.Size = Me.Size
            previousWindowState = Me.WindowState

            Me.WindowState = FormWindowState.Normal
            Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None
            Me.WindowState = FormWindowState.Maximized
            ToolStripContainer1.TopToolStripPanelVisible = False

            Me.ResumeLayout()
            isFullScreen = True
        Else
            Me.SuspendLayout()
            Me.FormBorderStyle = Windows.Forms.FormBorderStyle.Sizable
            ToolStripContainer1.TopToolStripPanelVisible = True
            Me.WindowState = FormWindowState.Normal

            Me.WindowState = previousWindowState
            If Me.WindowState = FormWindowState.Normal Then
                Me.Location = previousWindowPosition.Location
                Me.Size = previousWindowPosition.Size
            End If

            Me.ResumeLayout()
            isFullScreen = False
        End If
    End Sub

    Private Sub Form1_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown
        If HandleGlobalShortcut(e) Then Return

        Select Case e.KeyCode
            Case Keys.F11
                setFullScreen(Not isFullScreen)
        End Select
    End Sub

    Private Function HandleGlobalShortcut(ByVal e As KeyEventArgs) As Boolean
        If ConsumePreviewGlobalShortcut(e.KeyCode, e.Control, e.Shift, e.Alt) Then
            e.SuppressKeyPress = True
            Return True
        End If

        If Not HandleGlobalShortcut(e.KeyCode, e.Control, e.Shift, e.Alt) Then Return False

        e.SuppressKeyPress = True
        Return True
    End Function

    Private Function ShortcutModifierMask(ByVal ctrl As Boolean, ByVal shift As Boolean, ByVal alt As Boolean) As Integer
        Dim xMask As Integer = 0
        If ctrl Then xMask = xMask Or 1
        If shift Then xMask = xMask Or 2
        If alt Then xMask = xMask Or 4
        Return xMask
    End Function

    Private Sub RememberPreviewGlobalShortcut(ByVal keyCode As Keys, ByVal ctrl As Boolean, ByVal shift As Boolean, ByVal alt As Boolean)
        PreviewGlobalShortcutKey = keyCode
        PreviewGlobalShortcutModifiers = ShortcutModifierMask(ctrl, shift, alt)
        PreviewGlobalShortcutTime = DateTime.UtcNow
    End Sub

    Private Sub ClearPreviewGlobalShortcut()
        PreviewGlobalShortcutKey = Keys.None
        PreviewGlobalShortcutModifiers = 0
        PreviewGlobalShortcutTime = DateTime.MinValue
    End Sub

    Private Function ConsumePreviewGlobalShortcut(ByVal keyCode As Keys, ByVal ctrl As Boolean, ByVal shift As Boolean, ByVal alt As Boolean) As Boolean
        If PreviewGlobalShortcutKey = Keys.None Then Return False

        If PreviewGlobalShortcutKey <> keyCode OrElse
           PreviewGlobalShortcutModifiers <> ShortcutModifierMask(ctrl, shift, alt) OrElse
           (DateTime.UtcNow - PreviewGlobalShortcutTime).TotalMilliseconds > PreviewGlobalShortcutSuppressMilliseconds Then
            ClearPreviewGlobalShortcut()
            Return False
        End If

        ClearPreviewGlobalShortcut()
        Return True
    End Function

    Private Function IsGlobalShortcutKey(ByVal keyCode As Keys, ByVal ctrl As Boolean, ByVal shift As Boolean, ByVal alt As Boolean) As Boolean
        If Not ctrl OrElse alt Then Return False

        Select Case keyCode
            Case Keys.Oemplus, Keys.Add, Keys.OemMinus, Keys.Subtract
                Return True
            Case Keys.Oem5, Keys.Oem102
                Return Not shift
        End Select

        Return False
    End Function

    Private Function HandleGlobalShortcutFromPreview(ByVal keyCode As Keys, ByVal ctrl As Boolean, ByVal shift As Boolean, ByVal alt As Boolean) As Boolean
        If Not HandleGlobalShortcut(keyCode, ctrl, shift, alt) Then Return False

        RememberPreviewGlobalShortcut(keyCode, ctrl, shift, alt)
        Return True
    End Function

    Private Function HandleGlobalShortcut(ByVal keyCode As Keys, ByVal ctrl As Boolean, ByVal shift As Boolean, ByVal alt As Boolean) As Boolean
        If Not IsGlobalShortcutKey(keyCode, ctrl, shift, alt) Then Return False

        Select Case keyCode
            Case Keys.Oemplus, Keys.Add
                AddRightSplitPane()
            Case Keys.OemMinus, Keys.Subtract
                RemoveRightSplitPane()
            Case Keys.Oem5, Keys.Oem102
                SetSplitterScrollSync(Not SyncSplitterScroll)
            Case Else
                Return False
        End Select

        Return True
    End Function

    Private Sub Form1_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyUp
        If e.KeyCode = KeyMoveKey Then EndKeyMove()
        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Friend Sub ReadFile(ByVal xPath As String)
        Select Case LCase(Path.GetExtension(xPath))
            Case ".bms", ".bme", ".bml", ".pms", ".txt"
                OpenBMS(My.Computer.FileSystem.ReadAllText(xPath, TextEncoding))
                ClearUndo()
                NewRecent(xPath)
                SetFileName(xPath)
                SetIsSaved(True)

            Case ".sm"
                If OpenSM(My.Computer.FileSystem.ReadAllText(xPath, TextEncoding)) Then Return
                InitPath = ExcludeFileName(xPath)
                ClearUndo()
                SetFileName("Untitled.bms")
                SetIsSaved(False)

            Case ".nbmsc"
                OpenNBMSC(xPath)
                InitPath = ExcludeFileName(xPath)
                NewRecent(xPath)
                SetFileName("Imported_" & GetFileName(xPath))
                SetIsSaved(False)

            Case ".bmson"
                MsgBox(Strings.fImportBMSON.Message)
        End Select
    End Sub


    Public Function GCD(ByVal NumA As Double, ByVal NumB As Double) As Double
        Dim xNMax As Double = NumA
        Dim xNMin As Double = NumB
        If NumA < NumB Then
            xNMax = NumB
            xNMin = NumA
        End If
        Do While xNMin >= BMSGridLimit
            GCD = xNMax - Math.Floor(xNMax / xNMin) * xNMin
            xNMax = xNMin
            xNMin = GCD
        Loop
        GCD = xNMax
    End Function
    Public Function GCD(ByVal NumA As Double, ByVal NumB As Double, res As Integer) As Double
        Dim xNMax As Double = NumA
        Dim xNMin As Double = NumB
        Dim minLimit = (192.0R / res)
        If NumA < NumB Then
            xNMax = NumB
            xNMin = NumA
        End If
        Do While xNMin >= minLimit
            GCD = xNMax - Math.Floor(xNMax / xNMin) * xNMin
            xNMax = xNMin
            xNMin = GCD
        Loop
        GCD = xNMax
    End Function

    <DllImport("user32.dll")> Private Shared Function LoadCursorFromFile(ByVal fileName As String) As IntPtr
    End Function
    Public Shared Function ActuallyLoadCursor(ByVal path As String) As Cursor
        Return New Cursor(LoadCursorFromFile(path))
    End Function

    Private Sub Unload() Handles MyBase.Disposed
        ReleaseHeaderWheelBlockers()
        Audio.Finalize()
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'On Error Resume Next
        Me.TopMost = True
        Me.SuspendLayout()
        Me.Visible = False

        'POBMP.Dispose()
        'POBGA.Dispose()

        'Me.MaximizedBounds = Screen.GetWorkingArea(Me)
        'Me.Visible = False
        SetFileName(FileName)
        'Me.ShowCaption = False
        'SetWindowText(Me.Handle.ToInt32, FileName)

        InitializeNewBMS()
        'nBeatD.SelectedIndex = 4

        Try
            Dim xTempFileName As String = RandomFileName(".cur")
            My.Computer.FileSystem.WriteAllBytes(xTempFileName, My.Resources.CursorResizeLeft, False)
            Dim xLeftCursor As Cursor = ActuallyLoadCursor(xTempFileName)
            My.Computer.FileSystem.WriteAllBytes(xTempFileName, My.Resources.CursorResizeRight, False)
            Dim xRightCursor As Cursor = ActuallyLoadCursor(xTempFileName)
            File.Delete(xTempFileName)

            POptionsResizer.Cursor = xLeftCursor

            SpL.Cursor = xRightCursor
            SpR.Cursor = xLeftCursor
            SplitterResizeCursor = xLeftCursor
        Catch ex As Exception

        End Try

        sI = 0
        SetUndoRedoEnd(0)
        SetUndoRedoEnd(sIA())

        RefreshDefinitionLists()
        CHPlayer.SelectedIndex = 0

        CalculateGreatestVPosition()
        TBLangRefresh_Click(TBLangRefresh, Nothing)
        TBThemeRefresh_Click(TBThemeRefresh, Nothing)

        If My.Computer.FileSystem.FileExists(My.Application.Info.DirectoryPath & "\nBMSC.Settings.xml") Then
            LoadSettings(My.Application.Info.DirectoryPath & "\nBMSC.Settings.xml")
        Else
            LoadInitialPreferences()
        End If
        RefreshPlayerSelector()
        SetUseBase62Definitions(NewBMSUseBase62Definitions)
        RefreshDefinitionLists()
        'On Error GoTo 0
        SetIsSaved(True)

        Dim xStr() As String = Environment.GetCommandLineArgs
        'Dim xStr() As String = {Application.ExecutablePath, "C:\Users\User\Desktop\yang run xuan\SoFtwArES\Games\O2Mania\music\SHOOT!\shoot! -NM-.bms"}

        If xStr.Length = 2 Then
            ReadFile(xStr(1))
            If LCase(Path.GetExtension(xStr(1))) = ".nbmsc" AndAlso GetFileName(xStr(1)).StartsWith("AutoSave_", True, Nothing) Then GoTo 1000
        End If

        'pIsSaved.Visible = Not IsSaved
        IsInitializing = False

        If Process.GetProcessesByName(Process.GetCurrentProcess.ProcessName).Length > 1 Then GoTo 1000
        Dim xFiles() As FileInfo = My.Computer.FileSystem.GetDirectoryInfo(My.Application.Info.DirectoryPath).GetFiles("AutoSave_*.NBMSC")
        If xFiles Is Nothing OrElse xFiles.Length = 0 Then GoTo 1000

        'Me.TopMost = True
        If MsgBox(Replace(Strings.Messages.RestoreAutosavedFile, "{}", xFiles.Length), MsgBoxStyle.YesNo Or MsgBoxStyle.MsgBoxSetForeground) = MsgBoxResult.Yes Then
            For Each xF As FileInfo In xFiles
                'MsgBox(xF.FullName)
                System.Diagnostics.Process.Start(Application.ExecutablePath, """" & xF.FullName & """")
            Next
        End If

        For Each xF As FileInfo In xFiles
            ReDim Preserve pTempFileNames(UBound(pTempFileNames) + 1)
            pTempFileNames(UBound(pTempFileNames)) = xF.FullName
        Next

1000:
        IsInitializing = False
        POStatusRefresh()
        Me.ResumeLayout()

        tempResize = Me.WindowState
        Me.TopMost = False
        Me.WindowState = tempResize

        Me.Visible = True
        UpdateHorizontalScrollMetrics()
        RefreshPanelAll()
        PMainIn.Focus()
        ResetSelectedOptionsTabScroll()
        QueueResetSelectedOptionsTabScroll()
        If CheckUpdatesOnStartup Then BeginInvoke(New MethodInvoker(AddressOf StartStartupUpdateCheck))
    End Sub

    Private Sub LoadInitialPreferences()
        Dim xDataPath As String = My.Application.Info.DirectoryPath & "\Data"
        Dim xLangPath As String = xDataPath & "\jpn.Lang.xml"
        Dim xThemePath As String = xDataPath & "\IIDX.Theme.xml"

        If My.Computer.FileSystem.FileExists(xLangPath) Then
            LoadLocale(xLangPath)
        End If

        If My.Computer.FileSystem.FileExists(xThemePath) Then
            LoadSettings(xThemePath)
            ChangePlaySideSkin(False)
            RefreshGridToolbar()
        End If
    End Sub

    Private Sub UpdatePairing()
        Dim i As Integer, j As Integer

        If NTInput Then
            For i = 0 To UBound(Notes)
                Notes(i).HasError = False
                Notes(i).LNPair = 0
                If Notes(i).Length < 0 Then Notes(i).Length = 0
            Next

            For i = 1 To UBound(Notes)
                If Notes(i).Length <> 0 Then
                    For j = i + 1 To UBound(Notes)
                        If Notes(j).VPosition > Notes(i).VPosition + Notes(i).Length Then Exit For
                        If Notes(j).ColumnIndex = Notes(i).ColumnIndex Then Notes(j).HasError = True
                    Next
                Else
                    For j = i + 1 To UBound(Notes)
                        If Notes(j).VPosition > Notes(i).VPosition Then Exit For
                        If Notes(j).ColumnIndex = Notes(i).ColumnIndex Then Notes(j).HasError = True
                    Next

                    If Notes(i).Value \ 10000 = LnObj AndAlso Not IsColumnNumeric(Notes(i).ColumnIndex) Then
                        For j = i - 1 To 1 Step -1
                            If Notes(j).ColumnIndex <> Notes(i).ColumnIndex Then Continue For
                            If Notes(j).Hidden Then Continue For

                            If Notes(j).Length <> 0 OrElse Notes(j).Value \ 10000 = LnObj Then
                                Notes(i).HasError = True
                            Else
                                Notes(i).LNPair = j
                                Notes(j).LNPair = i
                            End If
                            Exit For
                        Next
                        If j = 0 Then
                            Notes(i).HasError = True
                        End If
                    End If
                End If
            Next

        Else
            For i = 0 To UBound(Notes)
                Notes(i).HasError = False
                Notes(i).LNPair = 0
            Next

            For i = 1 To UBound(Notes)
                If IsColumnSound(Notes(i).ColumnIndex) AndAlso Notes(i).ColumnIndex < niB Then
                    If Notes(i).LongNote Then
                        'LongNote: If overlapping a note, then error.
                        '          Else if already matched by a LongNote below, then match it.
                        '          Otherwise match anything above.
                        '              If ShortNote above then error on above.
                        '          If nothing above then error.
                        For j = i - 1 To 1 Step -1
                            If Notes(j).ColumnIndex <> Notes(i).ColumnIndex Then Continue For
                            If Notes(j).VPosition = Notes(i).VPosition Then
                                Notes(i).HasError = True
                                GoTo EndSearch
                            ElseIf Notes(j).LongNote And Notes(j).LNPair = i Then
                                Notes(i).LNPair = j
                                GoTo EndSearch
                            Else
                                Exit For
                            End If
                        Next

                        For j = i + 1 To UBound(Notes)
                            If Notes(j).ColumnIndex <> Notes(i).ColumnIndex Then Continue For
                            Notes(i).LNPair = j
                            Notes(j).LNPair = i
                            If Not Notes(j).LongNote AndAlso Notes(j).Value \ 10000 <> LnObj Then
                                Notes(j).HasError = True
                            End If
                            Exit For
                        Next

                        If j = UBound(Notes) + 1 Then
                            Notes(i).HasError = True
                        End If
EndSearch:

                    ElseIf Notes(i).Value \ 10000 = LnObj Then
                        'LnObj: Match anything below.
                        '           If matching a LongNote not matching back, then error on below.
                        '           If overlapping a note, then error.
                        '           If mathcing a LnObj below, then error on below.
                        '       If nothing below, then error.
                        For j = i - 1 To 1 Step -1
                            If Notes(i).ColumnIndex <> Notes(j).ColumnIndex Then Continue For
                            If Notes(j).LNPair <> 0 And Notes(j).LNPair <> i Then
                                Notes(j).HasError = True
                            End If
                            Notes(i).LNPair = j
                            Notes(j).LNPair = i
                            If Notes(i).VPosition = Notes(j).VPosition Then
                                Notes(i).HasError = True
                            End If
                            If Notes(j).Value \ 10000 = LnObj Then
                                Notes(j).HasError = True
                            End If
                            Exit For
                        Next

                        If j = 0 Then
                            Notes(i).HasError = True
                        End If
                    Else
                        'ShortNote: If overlapping a note, then error.
                        For j = i - 1 To 1 Step -1
                            If Notes(j).VPosition < Notes(i).VPosition Then Exit For
                            If Notes(j).ColumnIndex <> Notes(i).ColumnIndex Then Continue For
                            Notes(i).HasError = True
                            Exit For
                        Next
                    End If
                Else
                    If Notes(i).Hidden Or Notes(i).Landmine Then
                        Notes(i).HasError = True
                    Else
                        'ShortNote: If overlapping a note, then error.
                        For j = i - 1 To 1 Step -1
                            If Notes(j).VPosition < Notes(i).VPosition Then Exit For
                            If Notes(j).ColumnIndex <> Notes(i).ColumnIndex Then Continue For
                            Notes(i).HasError = True
                            Exit For
                        Next
                    End If
                End If
            Next


        End If

        Dim currentMS = 0.0#
        Dim currentBPM = Notes(0).Value / 10000
        Dim currentBPMVPosition = 0.0#
        For i = 1 To UBound(Notes)
            If Notes(i).ColumnIndex = niBPM Then
                currentMS += (Notes(i).VPosition - currentBPMVPosition) / currentBPM * 1250
                currentBPM = Notes(i).Value / 10000
                currentBPMVPosition = Notes(i).VPosition
            End If
            'K(i).TimeOffset = currentMS + (K(i).VPosition - currentBPMVPosition) / currentBPM * 1250
        Next
    End Sub



    Public Sub ExceptionSave(ByVal Path As String)
        SaveNBMSC(Path)
    End Sub

    Private Function DefinitionDisplayMax() As Integer
        If UseBase62Definitions Then Return MaxDefinition

        Return MaxBase36Definition
    End Function

    Private Function DefinitionLastListIndex() As Integer
        Return DefinitionDisplayMax() - 1
    End Function

    Private Function DefinitionLabel(ByVal xValue As Long) As String
        If UseBase62Definitions Then Return C10to36(xValue)

        Return C10toBase36(xValue)
    End Function

    Private Function DefinitionIndex(ByVal xLabel As String) As Integer
        If UseBase62Definitions Then Return C36to10(xLabel)

        Return CBase36to10(xLabel)
    End Function

    Private Function IsDefinitionLabel(ByVal xLabel As String) As Boolean
        If UseBase62Definitions Then Return IsBase62(xLabel)

        Return IsBase36(xLabel)
    End Function

    Private Function DefinitionModeMax(ByVal xMode As Integer) As Integer
        Select Case xMode
            Case DefinitionModeBase62
                Return MaxDefinition
            Case DefinitionModeBase36
                Return MaxBase36Definition
        End Select

        Return MaxLegacyDefinition
    End Function

    Private Function DefinitionModeLabel(ByVal xValue As Long, ByVal xMode As Integer) As String
        Select Case xMode
            Case DefinitionModeBase62
                Return C10to36(xValue)
            Case DefinitionModeBase36
                Return C10toBase36(xValue)
        End Select

        Return Mid("0" & Hex(xValue), Len(Hex(xValue)))
    End Function

    Private Function DefinitionModeIndex(ByVal xLabel As String, ByVal xMode As Integer) As Integer
        Select Case xMode
            Case DefinitionModeBase62
                Return C36to10(xLabel)
            Case DefinitionModeBase36
                Return CBase36to10(xLabel)
        End Select

        Return Convert.ToInt32(xLabel, 16)
    End Function

    Private Function ContainsBase62Definition(ByVal xLabel As String) As Boolean
        For Each xChar As Char In xLabel
            If xChar >= "a"c AndAlso xChar <= "z"c Then Return True
        Next

        Return False
    End Function

    Private Function ContainsBase62Definitions(ByVal xLines() As String) As Boolean
        For Each xLine As String In xLines
            Dim xLineTrim As String = xLine.Trim

            If xLineTrim.StartsWith("#WAV", StringComparison.CurrentCultureIgnoreCase) OrElse _
               xLineTrim.StartsWith("#BMP", StringComparison.CurrentCultureIgnoreCase) OrElse _
               xLineTrim.StartsWith("#BPM", StringComparison.CurrentCultureIgnoreCase) OrElse _
               xLineTrim.StartsWith("#STOP", StringComparison.CurrentCultureIgnoreCase) OrElse _
               xLineTrim.StartsWith("#SCROLL", StringComparison.CurrentCultureIgnoreCase) Then
                If ContainsBase62Definition(Mid(xLineTrim, 5, 2)) Then Return True

            ElseIf xLineTrim.StartsWith("#LNOBJ", StringComparison.CurrentCultureIgnoreCase) Then
                If ContainsBase62Definition(Mid(xLineTrim, Len("#LNOBJ") + 1).Trim) Then Return True

            ElseIf xLineTrim.StartsWith("#") AndAlso Mid(xLineTrim, 7, 1) = ":" Then
                If Mid(xLineTrim, 5, 2) = "03" Then Continue For

                For xI As Integer = 8 To Len(xLineTrim) - 1 Step 2
                    Dim xLabel As String = Mid(xLineTrim, xI, 2)
                    If xLabel <> "00" AndAlso ContainsBase62Definition(xLabel) Then Return True
                Next
            End If
        Next

        Return False
    End Function

    Private Sub SetUseBase62Definitions(ByVal xValue As Boolean)
        UseBase62Definitions = xValue

        If CWAVBase62.Checked <> xValue Then CWAVBase62.Checked = xValue
        If CBMPBase62.Checked <> xValue Then CBMPBase62.Checked = xValue
    End Sub

    Private Sub RefreshDefinitionLists()
        Dim xWAVIndex As Integer = LWAV.SelectedIndex
        Dim xBMPIndex As Integer = LBMP.SelectedIndex

        LWAV.Visible = False
        LWAV.Items.Clear()
        LBMP.Visible = False
        LBMP.Items.Clear()

        For xI As Integer = 1 To DefinitionDisplayMax()
            LWAV.Items.Add(DefinitionLabel(xI) & ": " & hWAV(xI))
            LBMP.Items.Add(DefinitionLabel(xI) & ": " & hBMP(xI))
        Next

        If LWAV.Items.Count > 0 Then LWAV.SelectedIndex = Math.Max(0, Math.Min(xWAVIndex, LWAV.Items.Count - 1))
        If LBMP.Items.Count > 0 Then LBMP.SelectedIndex = Math.Max(0, Math.Min(xBMPIndex, LBMP.Items.Count - 1))

        LWAV.Visible = True
        LBMP.Visible = True
    End Sub

    Private Sub RefreshWAVItem(ByVal xIndex As Integer)
        If xIndex < 1 OrElse xIndex > LWAV.Items.Count Then Return

        LWAV.Items.Item(xIndex - 1) = DefinitionLabel(xIndex) & ": " & hWAV(xIndex)
    End Sub

    Private Sub RefreshBMPItem(ByVal xIndex As Integer)
        If xIndex < 1 OrElse xIndex > LBMP.Items.Count Then Return

        LBMP.Items.Item(xIndex - 1) = DefinitionLabel(xIndex) & ": " & hBMP(xIndex)
    End Sub

    Private Function DefinitionValue(ByVal isWav As Boolean, ByVal index As Integer) As String
        If isWav Then
            If index < 0 OrElse index > UBound(hWAV) Then Return ""
            Return hWAV(index)
        End If

        If index < 0 OrElse index > UBound(hBMP) Then Return ""
        Return hBMP(index)
    End Function

    Private Sub SetDefinitionValue(ByVal isWav As Boolean, ByVal index As Integer, ByVal value As String)
        If value Is Nothing Then value = ""

        If isWav Then
            If index < 0 OrElse index > UBound(hWAV) Then Return

            hWAV(index) = value
            If index = 0 Then
                If THLandMine.Text <> value Then THLandMine.Text = value
            Else
                RefreshWAVItem(index)
            End If
            Return
        End If

        If index < 0 OrElse index > UBound(hBMP) Then Return

        hBMP(index) = value
        If index = 0 Then
            If THMissBMP.Text <> value Then THMissBMP.Text = value
        Else
            RefreshBMPItem(index)
        End If
    End Sub

    Private Function SetDefinitionValueWithUndo(ByVal isWav As Boolean,
                                                ByVal index As Integer,
                                                ByVal value As String,
                                                ByRef undo As UndoRedo.LinkedURCmd,
                                                ByRef redo As UndoRedo.LinkedURCmd) As Boolean
        If Not RedoDefinitionChange(isWav, index, value, undo, redo) Then Return False

        SetDefinitionValue(isWav, index, value)
        Return True
    End Function

    ''' <summary>
    ''' True if pressed cancel. False elsewise.
    ''' </summary>
    ''' <returns>True if pressed cancel. False elsewise.</returns>

    Private Function ClosingPopSave() As Boolean
        If Not IsSaved Then
            Dim xResult As MsgBoxResult = MsgBox(Strings.Messages.SaveOnExit, MsgBoxStyle.YesNoCancel Or MsgBoxStyle.Question, Me.Text)

            If xResult = MsgBoxResult.Yes Then
                If ExcludeFileName(FileName) = "" Then
                    Dim xDSave As New SaveFileDialog
                    xDSave.Filter = Strings.FileType._bms & "|*.bms;*.bme;*.bml;*.pms;*.txt|" &
                                    Strings.FileType.BMS & "|*.bms|" &
                                    Strings.FileType.BME & "|*.bme|" &
                                    Strings.FileType.BML & "|*.bml|" &
                                    Strings.FileType.PMS & "|*.pms|" &
                                    Strings.FileType.TXT & "|*.txt|" &
                                    Strings.FileType._all & "|*.*"
                    xDSave.DefaultExt = "bms"
                    xDSave.InitialDirectory = InitPath

                    If xDSave.ShowDialog = Windows.Forms.DialogResult.Cancel Then Return True
                    SetFileName(xDSave.FileName)
                End If
                Dim xStrAll As String = SaveBMS()
                My.Computer.FileSystem.WriteAllText(FileName, xStrAll, False, TextEncoding)
                NewRecent(FileName)
                If BeepWhileSaved Then Beep()
            End If

            If xResult = MsgBoxResult.Cancel Then Return True
        End If
        Return False
    End Function

    Private Sub TBNew_Click(ByVal sender As System.Object, ByVal e As EventArgs) Handles TBNew.Click, mnNew.Click

        'KMouseDown = -1
        ReDim SelectedNotes(-1)
        KMouseOver = -1
        If ClosingPopSave() Then Exit Sub

        ClearUndo()
        InitializeNewBMS()

        ReDim Notes(0)
        ReDim mColumn(999)
        ReDim hWAV(MaxDefinition)
        ReDim hBMP(MaxDefinition)
        ReDim hBPM(MaxDefinition)    'x10000
        ReDim hSTOP(MaxDefinition)
        ReDim hBMSCROLL(MaxDefinition)
        SetUseBase62Definitions(NewBMSUseBase62Definitions)
        THGenre.Text = ""
        THTitle.Text = ""
        THArtist.Text = ""
        THPlayLevel.Text = ""

        With Notes(0)
            .ColumnIndex = niBPM
            .VPosition = -1
            '.LongNote = False
            '.Selected = False
            .Value = 1200000
        End With
        THBPM.Value = 120

        RefreshDefinitionLists()

        SetFileName("Untitled.bms")
        SetIsSaved(True)
        'pIsSaved.Visible = Not IsSaved

        CalculateTotalPlayableNotes()
        CalculateGreatestVPosition()
        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Private Sub TBNewC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) 'Handles TBNewC.Click
        'KMouseDown = -1
        ReDim SelectedNotes(-1)
        KMouseOver = -1
        If ClosingPopSave() Then Exit Sub

        ClearUndo()

        ReDim Notes(0)
        ReDim mColumn(999)
        ReDim hWAV(MaxDefinition)
        ReDim hBMP(MaxDefinition)
        ReDim hBPM(MaxDefinition)    'x10000
        ReDim hSTOP(MaxDefinition)
        ReDim hBMSCROLL(MaxDefinition)
        THGenre.Text = ""
        THTitle.Text = ""
        THArtist.Text = ""
        THPlayLevel.Text = ""

        With Notes(0)
            .ColumnIndex = niBPM
            .VPosition = -1
            '.LongNote = False
            '.Selected = False
            .Value = 1200000
        End With
        THBPM.Value = 120

        SetFileName("Untitled.bms")
        SetIsSaved(True)
        'pIsSaved.Visible = Not IsSaved

        If MsgBox("Please copy your code to clipboard and click OK.", MsgBoxStyle.OkCancel, "Create from code") = MsgBoxResult.Cancel Then Exit Sub
        OpenBMS(Clipboard.GetText)
    End Sub

    Private Sub TBOpen_ButtonClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBOpen.ButtonClick, mnOpen.Click
        'KMouseDown = -1
        ReDim SelectedNotes(-1)
        KMouseOver = -1
        If ClosingPopSave() Then Exit Sub

        Dim xDOpen As New OpenFileDialog
        xDOpen.Filter = Strings.FileType._bms & "|*.bms;*.bme;*.bml;*.pms;*.txt"
        xDOpen.DefaultExt = "bms"
        xDOpen.InitialDirectory = IIf(ExcludeFileName(FileName) = "", InitPath, ExcludeFileName(FileName))

        If xDOpen.ShowDialog = Windows.Forms.DialogResult.Cancel Then Exit Sub
        InitPath = ExcludeFileName(xDOpen.FileName)
        OpenBMS(My.Computer.FileSystem.ReadAllText(xDOpen.FileName, TextEncoding))
        ClearUndo()
        SetFileName(xDOpen.FileName)
        NewRecent(FileName)
        SetIsSaved(True)
        'pIsSaved.Visible = Not IsSaved
    End Sub

    Private Sub TBImportNBMSC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBImportNBMSC.Click, mnImportNBMSC.Click
        'KMouseDown = -1
        ReDim SelectedNotes(-1)
        KMouseOver = -1
        If ClosingPopSave() Then Return

        Dim xDOpen As New OpenFileDialog
        xDOpen.Filter = Strings.FileType.NBMSC & "|*.nbmsc"
        xDOpen.DefaultExt = "nbmsc"
        xDOpen.InitialDirectory = IIf(ExcludeFileName(FileName) = "", InitPath, ExcludeFileName(FileName))

        If xDOpen.ShowDialog = Windows.Forms.DialogResult.Cancel Then Return
        InitPath = ExcludeFileName(xDOpen.FileName)
        SetFileName("Imported_" & GetFileName(xDOpen.FileName))
        OpenNBMSC(xDOpen.FileName)
        NewRecent(xDOpen.FileName)
        SetIsSaved(False)
        'pIsSaved.Visible = Not IsSaved
    End Sub

    Private Sub TBImportSM_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBImportSM.Click, mnImportSM.Click
        'KMouseDown = -1
        ReDim SelectedNotes(-1)
        KMouseOver = -1
        If ClosingPopSave() Then Exit Sub

        Dim xDOpen As New OpenFileDialog
        xDOpen.Filter = Strings.FileType.SM & "|*.sm"
        xDOpen.DefaultExt = "sm"
        xDOpen.InitialDirectory = IIf(ExcludeFileName(FileName) = "", InitPath, ExcludeFileName(FileName))

        If xDOpen.ShowDialog = Windows.Forms.DialogResult.Cancel Then Exit Sub
        If OpenSM(My.Computer.FileSystem.ReadAllText(xDOpen.FileName, TextEncoding)) Then Exit Sub
        InitPath = ExcludeFileName(xDOpen.FileName)
        SetFileName("Untitled.bms")
        ClearUndo()
        SetIsSaved(False)
        'pIsSaved.Visible = Not IsSaved
    End Sub

    Private Sub TBSave_ButtonClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBSave.ButtonClick, mnSave.Click
        'KMouseDown = -1
        ReDim SelectedNotes(-1)
        KMouseOver = -1

        If ExcludeFileName(FileName) = "" Then
            Dim xDSave As New SaveFileDialog
            xDSave.Filter = Strings.FileType._bms & "|*.bms;*.bme;*.bml;*.pms;*.txt|" &
                            Strings.FileType.BMS & "|*.bms|" &
                            Strings.FileType.BME & "|*.bme|" &
                            Strings.FileType.BML & "|*.bml|" &
                            Strings.FileType.PMS & "|*.pms|" &
                            Strings.FileType.TXT & "|*.txt|" &
                            Strings.FileType._all & "|*.*"
            xDSave.DefaultExt = "bms"
            xDSave.InitialDirectory = InitPath

            If xDSave.ShowDialog = Windows.Forms.DialogResult.Cancel Then Exit Sub
            InitPath = ExcludeFileName(xDSave.FileName)
            SetFileName(xDSave.FileName)
        End If
        Dim xStrAll As String = SaveBMS()
        My.Computer.FileSystem.WriteAllText(FileName, xStrAll, False, TextEncoding)
        NewRecent(FileName)
        SetFileName(FileName)
        SetIsSaved(True)
        'pIsSaved.Visible = Not IsSaved
        If BeepWhileSaved Then Beep()
    End Sub

    Private Sub TBSaveAs_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBSaveAs.Click, mnSaveAs.Click
        'KMouseDown = -1
        ReDim SelectedNotes(-1)
        KMouseOver = -1

        Dim xDSave As New SaveFileDialog
        xDSave.Filter = Strings.FileType._bms & "|*.bms;*.bme;*.bml;*.pms;*.txt|" &
                        Strings.FileType.BMS & "|*.bms|" &
                        Strings.FileType.BME & "|*.bme|" &
                        Strings.FileType.BML & "|*.bml|" &
                        Strings.FileType.PMS & "|*.pms|" &
                        Strings.FileType.TXT & "|*.txt|" &
                        Strings.FileType._all & "|*.*"
        xDSave.DefaultExt = "bms"
        xDSave.InitialDirectory = IIf(ExcludeFileName(FileName) = "", InitPath, ExcludeFileName(FileName))

        If xDSave.ShowDialog = Windows.Forms.DialogResult.Cancel Then Exit Sub
        InitPath = ExcludeFileName(xDSave.FileName)
        SetFileName(xDSave.FileName)
        Dim xStrAll As String = SaveBMS()
        My.Computer.FileSystem.WriteAllText(FileName, xStrAll, False, TextEncoding)
        NewRecent(FileName)
        SetFileName(FileName)
        SetIsSaved(True)
        'pIsSaved.Visible = Not IsSaved
        If BeepWhileSaved Then Beep()
    End Sub

    Private Sub TBExportNBMSC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBExportNBMSC.Click, mnExportNBMSC.Click
        'KMouseDown = -1
        ReDim SelectedNotes(-1)
        KMouseOver = -1

        Dim xDSave As New SaveFileDialog
        xDSave.Filter = Strings.FileType.NBMSC & "|*.nbmsc"
        xDSave.DefaultExt = "nbmsc"
        xDSave.InitialDirectory = IIf(ExcludeFileName(FileName) = "", InitPath, ExcludeFileName(FileName))
        If xDSave.ShowDialog = Windows.Forms.DialogResult.Cancel Then Exit Sub

        SaveNBMSC(xDSave.FileName)
        'My.Computer.FileSystem.WriteAllText(xDSave.FileName, xStrAll, False, TextEncoding)
        NewRecent(FileName)
        If BeepWhileSaved Then Beep()
    End Sub

    Private Sub TBExportBMSON_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBExportBMSON.Click, mnExportBMSON.Click
        'KMouseDown = -1
        ReDim SelectedNotes(-1)
        KMouseOver = -1

        Dim xDSave As New SaveFileDialog
        xDSave.Filter = Strings.FileType.BMSON & "|*.bmson"
        xDSave.DefaultExt = "bmson"
        xDSave.InitialDirectory = IIf(ExcludeFileName(FileName) = "", InitPath, ExcludeFileName(FileName))
        If xDSave.ShowDialog = Windows.Forms.DialogResult.Cancel Then Exit Sub

        SaveBMSON(xDSave.FileName)
        'My.Computer.FileSystem.WriteAllText(xDSave.FileName, xStrAll, False, TextEncoding)
        NewRecent(FileName)
        If BeepWhileSaved Then Beep()
    End Sub

    Private Sub VSGotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles MainPanelScroll.GotFocus, LeftPanelScroll.GotFocus, RightPanelScroll.GotFocus
        PanelFocus = sender.Tag
        If Not IsValidPanelIndex(PanelFocus) Then Return
        spMain(PanelFocus).Focus()
    End Sub

    Private Sub VSValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles MainPanelScroll.ValueChanged, LeftPanelScroll.ValueChanged, RightPanelScroll.ValueChanged
        Dim iI As Integer = sender.Tag
        If Not IsValidPanelIndex(iI) Then Return

        If SyncingPanelScroll Then
            PanelVScroll(iI) = sender.Value
            RefreshPanel(iI, spMain(iI).DisplayRectangle)
            Exit Sub
        End If

        ' az: We got a wheel event when we're zooming in/out
        If My.Computer.Keyboard.CtrlKeyDown Then
            sender.Value = VSValue ' Undo the scroll
            Exit Sub
        End If

        Dim xOldValue As Integer = PanelVScroll(iI)
        Dim xDelta As Integer = sender.Value - xOldValue
        If SyncSplitterScroll Then
            xDelta = LimitPanelVScrollDelta(xDelta)
            Dim xValue As Integer = xOldValue + xDelta

            If sender.Value <> xValue Then
                SyncingPanelScroll = True
                Try
                    sender.Value = xValue
                Finally
                    SyncingPanelScroll = False
                End Try
            End If
        End If

        If iI = PanelFocus And Not LastMouseDownLocation = New Point(-1, -1) And Not VSValue = -1 Then LastMouseDownLocation.Y += (VSValue - (xOldValue + xDelta)) * gxHeight
        PanelVScroll(iI) = xOldValue + xDelta

        If SyncSplitterScroll Then
            SyncPanelVScroll(iI, xDelta)
        End If

        VSValue = xOldValue + xDelta
        If SyncSplitterScroll Then
            RefreshPanelAll()
        Else
            RefreshPanel(iI, spMain(iI).DisplayRectangle)
        End If
    End Sub

    Private Sub SyncPanelVScroll(ByVal sourceIndex As Integer, ByVal delta As Integer)
        If delta = 0 Then Return

        SyncingPanelScroll = True
        Try
            For i As Integer = 0 To SplitPanes.Count - 1
                If i = sourceIndex Then Continue For
                If Not IsPanelVScrollSynced(i) Then Continue For

                Dim xScroll As EditorScrollBar = GetPanelVScrollBar(i)
                Dim xValue As Integer = ClampPanelVScroll(xScroll, PanelVScroll(i) + delta)
                If xScroll.Value <> xValue Then xScroll.Value = xValue
                PanelVScroll(i) = xValue
            Next
        Finally
            SyncingPanelScroll = False
        End Try
    End Sub

    Private Function LimitPanelVScrollDelta(ByVal delta As Integer) As Integer
        If delta = 0 Then Return 0

        Dim xDelta As Integer = delta
        For i As Integer = 0 To SplitPanes.Count - 1
            If Not IsPanelVScrollSynced(i) Then Continue For

            Dim xScroll As EditorScrollBar = GetPanelVScrollBar(i)
            If delta > 0 Then
                xDelta = Math.Min(xDelta, 0 - PanelVScroll(i))
            Else
                xDelta = Math.Max(xDelta, xScroll.Minimum - PanelVScroll(i))
            End If
        Next

        Return xDelta
    End Function

    Private Function IsPanelVScrollSynced(ByVal panelIndex As Integer) As Boolean
        Return IsValidPanelIndex(panelIndex)
    End Function

    Private Function ClampPanelVScroll(ByVal xScroll As EditorScrollBar, ByVal value As Integer) As Integer
        If value > 0 Then Return 0
        If value < xScroll.Minimum Then Return xScroll.Minimum
        Return value
    End Function

    Private Sub SetPanelVScroll(ByVal panelIndex As Integer, ByVal value As Integer)
        If Not IsValidPanelIndex(panelIndex) Then Return

        Dim xScroll As EditorScrollBar = GetPanelVScrollBar(panelIndex)
        Dim xValue As Integer = ClampPanelVScroll(xScroll, value)

        SyncingPanelScroll = True
        Try
            If xScroll.Value <> xValue Then xScroll.Value = xValue
            PanelVScroll(panelIndex) = xValue
        Finally
            SyncingPanelScroll = False
        End Try
    End Sub

    Private Sub HSGotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles HS.GotFocus, HSL.GotFocus, HSR.GotFocus
        PanelFocus = sender.Tag
        If Not IsValidPanelIndex(PanelFocus) Then Return
        spMain(PanelFocus).Focus()
    End Sub

    Private Sub HSValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles HS.ValueChanged, HSL.ValueChanged, HSR.ValueChanged
        Dim iI As Integer = sender.Tag
        If Not IsValidPanelIndex(iI) Then Return
        If Not LastMouseDownLocation = New Point(-1, -1) And Not HSValue = -1 Then LastMouseDownLocation.X += (HSValue - sender.Value) * gxWidth
        PanelhBMSCROLL(iI) = sender.Value
        HSValue = sender.Value
        RefreshPanel(iI, spMain(iI).DisplayRectangle)
    End Sub

    Private Sub TBSelect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBSelect.Click, mnSelect.Click
        TBSelect.Checked = True
        TBWrite.Checked = False
        TBTimeSelect.Checked = False
        mnSelect.Checked = True
        mnWrite.Checked = False
        mnTimeSelect.Checked = False

        FStatus2.Visible = False
        FStatus.Visible = True

        ShouldDrawTempNote = False
        SelectedColumn = -1
        TempVPosition = -1
        TempLength = 0

        vSelStart = MeasureBottom(MeasureAtDisplacement(-PanelVScroll(PanelFocus)) + 1)
        vSelLength = 0

        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Private Sub TBWrite_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBWrite.Click, mnWrite.Click
        TBSelect.Checked = False
        TBWrite.Checked = True
        TBTimeSelect.Checked = False
        mnSelect.Checked = False
        mnWrite.Checked = True
        mnTimeSelect.Checked = False

        FStatus2.Visible = False
        FStatus.Visible = True

        ShouldDrawTempNote = True
        SelectedColumn = -1
        TempVPosition = -1
        TempLength = 0

        vSelStart = MeasureBottom(MeasureAtDisplacement(-PanelVScroll(PanelFocus)) + 1)
        vSelLength = 0

        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Private Sub TBPostEffects_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBTimeSelect.Click, mnTimeSelect.Click
        TBSelect.Checked = False
        TBWrite.Checked = False
        TBTimeSelect.Checked = True
        mnSelect.Checked = False
        mnWrite.Checked = False
        mnTimeSelect.Checked = True

        FStatus.Visible = False
        FStatus2.Visible = True

        vSelMouseOverLine = 0
        ShouldDrawTempNote = False
        SelectedColumn = -1
        TempVPosition = -1
        TempLength = 0
        ValidateSelection()

        Dim xI1 As Integer
        For xI1 = 0 To UBound(Notes)
            Notes(xI1).Selected = False
        Next
        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Private Sub CGHeight_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles CGHeight.ValueChanged
        gxHeight = CSng(CGHeight.Value)
        CGHeight2.Value = IIf(CGHeight.Value * 4 < CGHeight2.Maximum, CDec(CGHeight.Value * 4), CGHeight2.Maximum)
        RefreshGridToolbar()
        RefreshPanelAll()
    End Sub

    Private Sub CGHeight2_Scroll(ByVal sender As Object, ByVal e As System.EventArgs) Handles CGHeight2.Scroll
        CGHeight.Value = CGHeight2.Value / 4
    End Sub

    Private Sub CGWidth_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles CGWidth.ValueChanged
        gxWidth = CSng(CGWidth.Value)
        CGWidth2.Value = IIf(CGWidth.Value * 4 < CGWidth2.Maximum, CDec(CGWidth.Value * 4), CGWidth2.Maximum)
        RefreshGridToolbar()

        UpdateHorizontalScrollMetrics()
        RefreshPanelAll()
    End Sub

    Private Sub CGWidth2_Scroll(ByVal sender As Object, ByVal e As System.EventArgs) Handles CGWidth2.Scroll
        CGWidth.Value = CGWidth2.Value / 4
    End Sub

    Private Sub CGDivide_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CGDivide.ValueChanged
        gDivide = CGDivide.Value
        RefreshGridToolbar()
        RefreshPanelAll()
    End Sub
    Private Sub CGSub_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CGSub.ValueChanged
        gSub = CGSub.Value
        RefreshGridToolbar()
        RefreshPanelAll()
    End Sub
    Private Sub BGSlash_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BGSlash.Click, mnSlashGrid.Click
        Dim xd As Integer = Val(InputBox(Strings.Messages.PromptSlashValue, , gSlash))
        If xd = 0 Then Exit Sub
        If xd > CGDivide.Maximum Then xd = CGDivide.Maximum
        If xd < CGDivide.Minimum Then xd = CGDivide.Minimum
        gSlash = xd
    End Sub


    Private Sub CGSnap_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CGSnap.CheckedChanged
        gSnap = CGSnap.Checked
        RefreshGridSnapToolbar()
        RefreshPanelAll()
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        If Not IsValidPanelIndex(PanelFocus) Then Return
        ScrollPanelBy(PanelFocus, (tempY / 5) / gxHeight, (tempX / 10) / gxWidth)

        Dim xMEArgs As New System.Windows.Forms.MouseEventArgs(Windows.Forms.MouseButtons.Left, 0, MouseMoveStatus.X, MouseMoveStatus.Y, 0)
        PMainInMouseMove(spMain(PanelFocus), xMEArgs)

    End Sub

    Private Sub TimerMiddle_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TimerMiddle.Tick
        If Not MiddleButtonClicked Then TimerMiddle.Enabled = False : Return
        If Not IsValidPanelIndex(PanelFocus) Then Return

        ScrollPanelBy(PanelFocus,
                      (Cursor.Position.Y - MiddleButtonLocation.Y) / 5 / gxHeight,
                      (Cursor.Position.X - MiddleButtonLocation.X) / 5 / gxWidth)

        Dim xMEArgs As New System.Windows.Forms.MouseEventArgs(Windows.Forms.MouseButtons.Left, 0, MouseMoveStatus.X, MouseMoveStatus.Y, 0)
        PMainInMouseMove(spMain(PanelFocus), xMEArgs)
    End Sub

    Private Sub ScrollPanelBy(ByVal panelIndex As Integer, ByVal vDelta As Double, ByVal hDelta As Double)
        Dim xVScroll As EditorScrollBar = GetPanelVScrollBar(panelIndex)
        Dim xHScroll As EditorScrollBar = GetPanelHScroll(panelIndex)
        If xVScroll Is Nothing OrElse xHScroll Is Nothing Then Return

        SetScrollValue(xVScroll, CInt(xVScroll.Value + vDelta))
        SetScrollValue(xHScroll, CInt(xHScroll.Value + hDelta))
    End Sub

    Private Sub ValidateWavListView()
        Try
            Dim xRect As Rectangle = LWAV.GetItemRectangle(LWAV.SelectedIndex)
            If xRect.Top + xRect.Height > LWAV.DisplayRectangle.Height Then SendMessage(LWAV.Handle, &H115, 1, 0)
        Catch ex As Exception
        End Try
    End Sub

    Private Sub ValidateBmpListView()
        Try
            Dim xRect As Rectangle = LBMP.GetItemRectangle(LBMP.SelectedIndex)
            If xRect.Top + xRect.Height > LBMP.DisplayRectangle.Height Then SendMessage(LBMP.Handle, &H115, 1, 0)
        Catch ex As Exception
        End Try
    End Sub

    Private Sub LWAV_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles LWAV.Click
        If TBWrite.Checked Then FSW.Text = DefinitionLabel(LWAV.SelectedIndex + 1)

        PreviewNote("", True)
        If Not PreviewOnClick Then Exit Sub
        If hWAV(LWAV.SelectedIndex + 1) = "" Then Exit Sub

        Dim xFileLocation As String = GetBMSFilePath(hWAV(LWAV.SelectedIndex + 1))
        PreviewNote(xFileLocation, False)
    End Sub

    Private Sub LWAV_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles LWAV.DoubleClick
        Dim xDWAV As New OpenFileDialog
        xDWAV.DefaultExt = "wav"
        xDWAV.Filter = Strings.FileType._wave & "|*.wav;*.ogg;*.mp3;*.flac|" &
                       Strings.FileType.WAV & "|*.wav|" &
                       Strings.FileType.OGG & "|*.ogg|" &
                       Strings.FileType.MP3 & "|*.mp3|" &
                       Strings.FileType.FLAC & "|*.flac|" &
                       Strings.FileType._all & "|*.*"
        xDWAV.InitialDirectory = IIf(ExcludeFileName(FileName) = "", InitPath, ExcludeFileName(FileName))

        If xDWAV.ShowDialog = Windows.Forms.DialogResult.Cancel Then Exit Sub
        InitPath = ExcludeFileName(xDWAV.FileName)

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo
        If SetDefinitionValueWithUndo(True, LWAV.SelectedIndex + 1, GetBMSRefPath(xDWAV.FileName), xUndo, xRedo) Then
            AddUndo(xUndo, xBaseRedo.Next)
        End If
    End Sub

    Private Sub LWAV_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles LWAV.KeyDown
        Select Case e.KeyCode
            Case Keys.Delete
                ClearSelectedWavDefinitions()
        End Select
    End Sub

    Private Sub LBMP_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles LBMP.DoubleClick
        Dim xDBMP As New OpenFileDialog
        xDBMP.DefaultExt = "bmp"
        xDBMP.Filter = Strings.FileType._image & "|*.bmp;*.png;*.jpg;*.jpeg;.gif|" &
                       Strings.FileType._movie & "|*.mpg;*.m1v;*.m2v;*.avi;*.mp4;*.m4v;*.wmv;*.webm|" &
                       Strings.FileType.BMP & "|*.bmp|" &
                       Strings.FileType.PNG & "|*.png|" &
                       Strings.FileType.JPG & "|*.jpg;*.jpeg|" &
                       Strings.FileType.GIF & "|*.gif|" &
                       Strings.FileType.MP4 & "|*.mp4;*.m4v|" &
                       Strings.FileType.AVI & "|*.avi|" &
                       Strings.FileType.MPG & "|*.mpg;*.m1v;*.m2v|" &
                       Strings.FileType.WMV & "|*.wmv|" &
                       Strings.FileType.WEBM & "|*.webm|" &
                       Strings.FileType._all & "|*.*"
        xDBMP.InitialDirectory = IIf(ExcludeFileName(FileName) = "", InitPath, ExcludeFileName(FileName))

        If xDBMP.ShowDialog = Windows.Forms.DialogResult.Cancel Then Exit Sub
        InitPath = ExcludeFileName(xDBMP.FileName)

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo
        If SetDefinitionValueWithUndo(False, LBMP.SelectedIndex + 1, GetBMSRefPath(xDBMP.FileName), xUndo, xRedo) Then
            AddUndo(xUndo, xBaseRedo.Next)
        End If
    End Sub

    Private Sub LBMP_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles LBMP.KeyDown
        Select Case e.KeyCode
            Case Keys.Delete
                ClearSelectedBmpDefinitions()
        End Select
    End Sub

    Private Sub TBErrorCheck_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBErrorCheck.Click, mnErrorCheck.Click
        ErrorCheck = sender.Checked
        TBErrorCheck.Checked = ErrorCheck
        mnErrorCheck.Checked = ErrorCheck
        TBErrorCheck.Image = IIf(TBErrorCheck.Checked, My.Resources.x16CheckError, My.Resources.x16CheckErrorN)
        mnErrorCheck.Image = IIf(TBErrorCheck.Checked, My.Resources.x16CheckError, My.Resources.x16CheckErrorN)
        RefreshPanelAll()
    End Sub

    Private Sub TBPreviewOnClick_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBPreviewOnClick.Click, mnPreviewOnClick.Click
        PreviewNote("", True)
        PreviewOnClick = sender.Checked
        TBPreviewOnClick.Checked = PreviewOnClick
        mnPreviewOnClick.Checked = PreviewOnClick
        TBPreviewOnClick.Image = IIf(PreviewOnClick, My.Resources.x16PreviewOnClick, My.Resources.x16PreviewOnClickN)
        mnPreviewOnClick.Image = IIf(PreviewOnClick, My.Resources.x16PreviewOnClick, My.Resources.x16PreviewOnClickN)
    End Sub

    Private Sub TBChangePlaySide_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBChangePlaySide.Click, mnChangePlaySide.Click
        Rscratch = sender.Checked
        ChangePlaySide(True)
        TBChangePlaySide.Checked = Rscratch
        mnChangePlaySide.Checked = Rscratch
        TBChangePlaySide.Image = My.Resources.x16ChangePlaySide
        mnChangePlaySide.Image = My.Resources.x16ChangePlaySide
        RefreshPanelAll()
    End Sub

    'Private Sub TBPreviewErrorCheck_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
    '    PreviewErrorCheck = TBPreviewErrorCheck.Checked
    '    TBPreviewErrorCheck.Image = IIf(PreviewErrorCheck, My.Resources.x16PreviewCheck, My.Resources.x16PreviewCheckN)
    'End Sub

    Private Sub TBShowFileName_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBShowFileName.Click, mnShowFileName.Click
        ShowFileName = sender.Checked
        TBShowFileName.Checked = ShowFileName
        mnShowFileName.Checked = ShowFileName
        TBShowFileName.Image = IIf(ShowFileName, My.Resources.x16ShowFileName, My.Resources.x16ShowFileNameN)
        mnShowFileName.Image = IIf(ShowFileName, My.Resources.x16ShowFileName, My.Resources.x16ShowFileNameN)
        RefreshPanelAll()
    End Sub

    Private Sub TBCut_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBCut.Click, mnCut.Click
        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo
        Me.RedoRemoveNoteSelected(True, xUndo, xRedo)
        'Dim xRedo As String = sCmdKDs()
        'Dim xUndo As String = sCmdKs(True)

        Try
            CopyNotes(False)
            RemoveNotes(False)
            AddUndo(xUndo, xBaseRedo.Next)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, Strings.Messages.Err)
        End Try
        SortByVPositionInsertion()
        UpdatePairing()
        CalculateTotalPlayableNotes()
        RefreshPanelAll()
        POStatusRefresh()
        CalculateGreatestVPosition()
    End Sub

    Private Sub TBCopy_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBCopy.Click, mnCopy.Click
        Try
            CopyNotes()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, Strings.Messages.Err)
        End Try
        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Private Sub TBPaste_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBPaste.Click, mnPaste.Click
        PasteNotes()
    End Sub

    Private Sub PasteNotes(Optional ByVal xBaseVPosition As Double = -1.0#)
        AddNotesFromClipboard(True, True, xBaseVPosition)

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo
        Me.RedoAddNoteSelected(True, xUndo, xRedo)
        AddUndo(xUndo, xBaseRedo.Next)

        'AddUndo(sCmdKDs(), sCmdKs(True))

        SortByVPositionInsertion()
        UpdatePairing()
        CalculateTotalPlayableNotes()
        RefreshPanelAll()
        POStatusRefresh()
        CalculateGreatestVPosition()
    End Sub

    'Private Function pArgPath(ByVal I As Integer)
    '    Return Mid(pArgs(I), 1, InStr(pArgs(I), vbCrLf) - 1)
    'End Function

    Private Function GetFileName(ByVal s As String) As String
        Dim fslash As Integer = InStrRev(s, "/")
        Dim bslash As Integer = InStrRev(s, "\")
        Return Mid(s, IIf(fslash > bslash, fslash, bslash) + 1)
    End Function

    Private Function ExcludeFileName(ByVal s As String) As String
        Dim fslash As Integer = InStrRev(s, "/")
        Dim bslash As Integer = InStrRev(s, "\")
        If (bslash Or fslash) = 0 Then Return ""
        Return Mid(s, 1, IIf(fslash > bslash, fslash, bslash) - 1)
    End Function

    Private Function GetBMSDirectory() As String
        Dim xDir As String = ExcludeFileName(FileName)
        If xDir <> "" Then
            Return xDir
        End If

        Return InitPath
    End Function

    Private Function GetBMSFilePath(ByVal xPath As String) As String
        If xPath = "" Then
            Return ""
        End If

        If Path.IsPathRooted(xPath) Then
            Return xPath
        End If

        Dim xDir As String = GetBMSDirectory()
        If xDir = "" Then
            Return xPath
        End If

        Try
            Return Path.GetFullPath(Path.Combine(xDir, xPath))
        Catch
            Return xPath
        End Try
    End Function

    Private Function GetBMSRefPath(ByVal xPath As String) As String
        If xPath = "" Then
            Return ""
        End If

        If Not Path.IsPathRooted(xPath) Then
            Return xPath
        End If

        Dim xDir As String = GetBMSDirectory()
        If xDir = "" Then
            Return GetFileName(xPath)
        End If

        Try
            Dim xBase As String = Path.GetFullPath(xDir)
            Dim xFull As String = Path.GetFullPath(xPath)

            If Not String.Equals(Path.GetPathRoot(xBase), Path.GetPathRoot(xFull), StringComparison.OrdinalIgnoreCase) Then
                Return xFull
            End If

            Return MakeRelativePath(xBase, xFull)
        Catch
            Return GetFileName(xPath)
        End Try
    End Function

    Private Function MakeRelativePath(ByVal xBase As String, ByVal xFull As String) As String
        Dim xSeparators As Char() = {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}
        Dim xBaseParts As String() = Path.GetFullPath(xBase).TrimEnd(xSeparators).Split(xSeparators, StringSplitOptions.RemoveEmptyEntries)
        Dim xFullParts As String() = Path.GetFullPath(xFull).Split(xSeparators, StringSplitOptions.RemoveEmptyEntries)
        Dim xSame As Integer = 0

        Do While xSame < xBaseParts.Length AndAlso
                 xSame < xFullParts.Length AndAlso
                 String.Equals(xBaseParts(xSame), xFullParts(xSame), StringComparison.OrdinalIgnoreCase)
            xSame += 1
        Loop

        Dim xRel As New List(Of String)
        For xI As Integer = xSame To xBaseParts.Length - 1
            xRel.Add("..")
        Next
        For xI As Integer = xSame To xFullParts.Length - 1
            xRel.Add(xFullParts(xI))
        Next

        If xRel.Count = 0 Then
            Return GetFileName(xFull)
        End If

        Return String.Join("\", xRel.ToArray())
    End Function

    Private Sub PlayerMissingPrompt()
        Dim xArg As MainWindow.PlayerArguments = pArgs(CurrentPlayer)
        MsgBox(Strings.Messages.CannotFind.Replace("{}", PrevCodeToReal(xArg.Path)) & vbCrLf &
               Strings.Messages.PleaseRespecifyPath, MsgBoxStyle.Critical, Strings.Messages.PlayerNotFound)

        Dim xDOpen As New OpenFileDialog
        xDOpen.InitialDirectory = IIf(ExcludeFileName(PrevCodeToReal(xArg.Path)) = "",
                                      My.Application.Info.DirectoryPath,
                                      ExcludeFileName(PrevCodeToReal(xArg.Path)))
        xDOpen.FileName = PrevCodeToReal(xArg.Path)
        xDOpen.Filter = Strings.FileType.EXE & "|*.exe"
        xDOpen.DefaultExt = "exe"
        If xDOpen.ShowDialog = Windows.Forms.DialogResult.Cancel Then Exit Sub

        'pArgs(CurrentPlayer) = Replace(xDOpen.FileName, My.Application.Info.DirectoryPath, "<apppath>") & _
        '                                           Mid(pArgs(CurrentPlayer), InStr(pArgs(CurrentPlayer), vbCrLf))
        'xStr = Split(pArgs(CurrentPlayer), vbCrLf)
        pArgs(CurrentPlayer).Path = Replace(xDOpen.FileName, My.Application.Info.DirectoryPath, "<apppath>")
        xArg = pArgs(CurrentPlayer)
        RefreshPlayerSelector()
    End Sub

    Private Sub TBPlay_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBPlay.Click, mnPlay.Click
        'Dim xStr() As String = Split(pArgs(CurrentPlayer), vbCrLf)
        Dim xArg As MainWindow.PlayerArguments = pArgs(CurrentPlayer)

        If Not File.Exists(PrevCodeToReal(xArg.Path)) Then
            PlayerMissingPrompt()
            xArg = pArgs(CurrentPlayer)
        End If

        ' az: Treat it like we cancelled the operation
        If Not File.Exists(PrevCodeToReal(xArg.Path)) Then
            Exit Sub
        End If

        Dim xStrAll As String = SaveBMS()
        Dim xFileName As String = IIf(Not PathIsValid(FileName),
                                      IIf(InitPath = "", My.Application.Info.DirectoryPath, InitPath),
                                      ExcludeFileName(FileName)) & "\___TempBMS.bms"
        My.Computer.FileSystem.WriteAllText(xFileName, xStrAll, False, TextEncoding)

        AddTempFileList(xFileName)
        System.Diagnostics.Process.Start(PrevCodeToReal(xArg.Path), PrevCodeToReal(xArg.aHere, SkipClippedMeasure))
    End Sub

    Private Sub TBPlayB_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBPlayB.Click, mnPlayB.Click
        'Dim xStr() As String = Split(pArgs(CurrentPlayer), vbCrLf)
        Dim xArg As MainWindow.PlayerArguments = pArgs(CurrentPlayer)

        If Not File.Exists(PrevCodeToReal(xArg.Path)) Then
            PlayerMissingPrompt()
            xArg = pArgs(CurrentPlayer)
        End If

        If Not File.Exists(PrevCodeToReal(xArg.Path)) Then
            Exit Sub
        End If

        Dim xStrAll As String = SaveBMS()
        Dim xFileName As String = IIf(Not PathIsValid(FileName),
                                      IIf(InitPath = "", My.Application.Info.DirectoryPath, InitPath),
                                      ExcludeFileName(FileName)) & "\___TempBMS.bms"
        My.Computer.FileSystem.WriteAllText(xFileName, xStrAll, False, TextEncoding)

        AddTempFileList(xFileName)

        System.Diagnostics.Process.Start(PrevCodeToReal(xArg.Path), PrevCodeToReal(xArg.aBegin))
    End Sub

    Private Sub TBStop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBStop.Click, mnStop.Click
        'Dim xStr() As String = Split(pArgs(CurrentPlayer), vbCrLf)
        Dim xArg As MainWindow.PlayerArguments = pArgs(CurrentPlayer)

        If Not File.Exists(PrevCodeToReal(xArg.Path)) Then
            PlayerMissingPrompt()
            xArg = pArgs(CurrentPlayer)
        End If

        If Not File.Exists(PrevCodeToReal(xArg.Path)) Then
            Exit Sub
        End If

        System.Diagnostics.Process.Start(PrevCodeToReal(xArg.Path), PrevCodeToReal(xArg.aStop))
    End Sub

    Private Sub AddTempFileList(ByVal s As String)
        Dim xAdd As Boolean = True
        If pTempFileNames IsNot Nothing Then
            For Each xStr1 As String In pTempFileNames
                If xStr1 = s Then xAdd = False : Exit For
            Next
        End If

        If xAdd Then
            ReDim Preserve pTempFileNames(UBound(pTempFileNames) + 1)
            pTempFileNames(UBound(pTempFileNames)) = s
        End If
    End Sub

    Private Sub TBStatistics_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBStatistics.Click, mnStatistics.Click
        SortByVPositionInsertion()
        UpdatePairing()

        Dim data(6, 5) As Integer
        For i As Integer = 1 To UBound(Notes)
            With Notes(i)
                Dim row As Integer = -1
                If .ColumnIndex = niBPM Then
                    row = 0
                ElseIf .ColumnIndex = niSTOP Then
                    row = 1
                ElseIf .ColumnIndex = niSCROLL Then
                    row = 2
                ElseIf .ColumnIndex >= niA1 AndAlso .ColumnIndex <= niAQ Then
                    row = 3
                ElseIf .ColumnIndex >= niD1 AndAlso .ColumnIndex <= niDQ Then
                    row = 4
                ElseIf .ColumnIndex >= niB Then
                    row = 5
                Else
                    row = 6
                End If


StartCount:     If Not NTInput Then
                    If Not .LongNote Then data(row, 0) += 1
                    If .LongNote Then data(row, 1) += 1
                    If .Value \ 10000 = LnObj Then data(row, 1) += 1
                    If .Hidden Then data(row, 2) += 1
                    If .Landmine Then data(row, 3) += 1
                    If .HasError Then data(row, 4) += 1
                    data(row, 5) += 1

                Else
                    Dim noteUnit As Integer = 1
                    If .Length = 0 Then data(row, 0) += 1
                    If .Length <> 0 Then data(row, 1) += 2 : noteUnit = 2

                    If .Value \ 10000 = LnObj Then data(row, 1) += noteUnit
                    If .Hidden Then data(row, 2) += noteUnit
                    If .Landmine Then data(row, 3) += noteUnit
                    If .HasError Then data(row, 4) += noteUnit
                    data(row, 5) += noteUnit

                End If

                If row <> 6 Then row = 6 : GoTo StartCount
            End With
        Next

        Dim dStat As New dgStatistics(data)
        dStat.ShowDialog()
    End Sub

    ''' <summary>
    ''' Remark: Pls sort and updatepairing before this process.
    ''' </summary>

    Private Sub CalculateTotalPlayableNotes(Optional updateGreatestColumn As Boolean = True)
        Dim xI1 As Integer
        Dim xIAll As Integer = 0

        If Not NTInput Then
            For xI1 = 1 To UBound(Notes)
                If Notes(xI1).ColumnIndex >= niA1 AndAlso Notes(xI1).ColumnIndex <= niAQ Then xIAll += 1
                If (Notes(xI1).ColumnIndex >= niD1 AndAlso Notes(xI1).ColumnIndex <= niDQ) AndAlso column(Notes(xI1).ColumnIndex).isVisible Then xIAll += 1
            Next

        Else
            For xI1 = 1 To UBound(Notes)
                If Notes(xI1).ColumnIndex >= niA1 And Notes(xI1).ColumnIndex <= niAQ Then
                    xIAll += 1
                    If Notes(xI1).Length <> 0 Then xIAll += 1
                End If
                If (Notes(xI1).ColumnIndex >= niD1 And Notes(xI1).ColumnIndex <= niDQ) AndAlso column(Notes(xI1).ColumnIndex).isVisible Then
                    xIAll += 1
                    If Notes(xI1).Length <> 0 Then xIAll += 1
                End If
            Next

        End If

        TBStatistics.Text = xIAll
        If updateGreatestColumn Then CalculateGreatestColumn()
    End Sub

    Private Function CalculateTotalNotes() As Integer
        Dim xI1 As Integer
        Dim xIAll As Integer = 0

        For xI1 = 1 To UBound(Notes)
            If (Notes(xI1).ColumnIndex >= niA1 AndAlso Notes(xI1).ColumnIndex <= niAQ) AndAlso
                   Not Notes(xI1).Hidden AndAlso Not Notes(xI1).Landmine AndAlso column(Notes(xI1).ColumnIndex).isVisible Then
                xIAll += 1
            End If
            If (Notes(xI1).ColumnIndex >= niD1 AndAlso Notes(xI1).ColumnIndex <= niDQ) AndAlso
                   Not Notes(xI1).Hidden AndAlso Not Notes(xI1).Landmine AndAlso column(Notes(xI1).ColumnIndex).isVisible Then
                xIAll += 1
            End If
        Next

        Return xIAll
    End Function

    Public Function GetMouseVPosition(Optional snap As Boolean = True)
        Dim panHeight = spMain(PanelFocus).Height
        Dim panDisplacement = PanelVScroll(PanelFocus)
        Dim vpos = (panHeight - panDisplacement * gxHeight - MouseMoveStatus.Y - 1) / gxHeight
        If snap Then
            Return SnapToGrid(vpos)
        Else
            Return vpos
        End If
    End Function

    Private Sub POStatusRefresh()

        If TBSelect.Checked Then
            Dim xI1 As Integer = KMouseOver
            If xI1 < 0 Then

                TempVPosition = GetMouseVPosition(gSnap)

                SelectedColumn = GetColumnAtX(MouseMoveStatus.X, PanelhBMSCROLL(PanelFocus))

                Dim xMeasure As Integer = MeasureAtDisplacement(TempVPosition)
                Dim xMLength As Double = MeasureLength(xMeasure)
                Dim xVposMod As Double = TempVPosition - MeasureBottom(xMeasure)
                Dim xGCD As Double = GCD(IIf(xVposMod = 0, xMLength, xVposMod), xMLength)

                FSP1.Text = (xVposMod * gDivide / 192).ToString & " / " & (xMLength * gDivide / 192).ToString & "  "
                FSP2.Text = xVposMod.ToString & " / " & xMLength & "  "
                FSP3.Text = CInt(xVposMod / xGCD).ToString & " / " & CInt(xMLength / xGCD).ToString & "  "
                FSP4.Text = TempVPosition.ToString() & "  "
                TimeStatusLabel.Text = GetTimeFromVPosition(TempVPosition).ToString("F4")
                FSC.Text = nTitle(SelectedColumn)
                FSW.Text = ""
                FSM.Text = Add3Zeros(xMeasure)
                FST.Text = ""
                FSH.Text = ""
                FSL.Text = ""
                FSE.Text = ""

            Else
                Dim xMeasure As Integer = MeasureAtDisplacement(Notes(xI1).VPosition)
                Dim xMLength As Double = MeasureLength(xMeasure)
                Dim xVposMod As Double = Notes(xI1).VPosition - MeasureBottom(xMeasure)
                Dim xGCD As Double = GCD(IIf(xVposMod = 0, xMLength, xVposMod), xMLength)

                FSP1.Text = (xVposMod * gDivide / 192).ToString & " / " & (xMLength * gDivide / 192).ToString & "  "
                FSP2.Text = xVposMod.ToString & " / " & xMLength & "  "
                FSP3.Text = CInt(xVposMod / xGCD).ToString & " / " & CInt(xMLength / xGCD).ToString & "  "
                FSP4.Text = Notes(xI1).VPosition.ToString() & "  "
                TimeStatusLabel.Text = GetTimeFromVPosition(TempVPosition).ToString("F4")
                FSC.Text = nTitle(Notes(xI1).ColumnIndex)
                FSW.Text = IIf(IsColumnNumeric(Notes(xI1).ColumnIndex),
                               Notes(xI1).Value / 10000,
                               DefinitionLabel(Notes(xI1).Value \ 10000))
                FSM.Text = Add3Zeros(xMeasure)
                FST.Text = IIf(NTInput, Strings.StatusBar.Length & " = " & Notes(xI1).Length, IIf(Notes(xI1).LongNote, Strings.StatusBar.LongNote, ""))
                FSH.Text = IIf(Notes(xI1).Hidden, Strings.StatusBar.Hidden, "")
                FSL.Text = IIf(Notes(xI1).Landmine, Strings.StatusBar.LandMine, "")
                FSE.Text = IIf(Notes(xI1).HasError, Strings.StatusBar.Err, "")

            End If

        ElseIf TBWrite.Checked Then
            If SelectedColumn < 0 Then Exit Sub

            Dim xMeasure As Integer = MeasureAtDisplacement(TempVPosition)
            Dim xMLength As Double = MeasureLength(xMeasure)
            Dim xVposMod As Double = TempVPosition - MeasureBottom(xMeasure)
            Dim xGCD As Double = GCD(IIf(xVposMod = 0, xMLength, xVposMod), xMLength)

            FSP1.Text = (xVposMod * gDivide / 192).ToString & " / " & (xMLength * gDivide / 192).ToString & "  "
            FSP2.Text = xVposMod.ToString & " / " & xMLength & "  "
            FSP3.Text = CInt(xVposMod / xGCD).ToString & " / " & CInt(xMLength / xGCD).ToString & "  "
            FSP4.Text = TempVPosition.ToString() & "  "
            TimeStatusLabel.Text = GetTimeFromVPosition(TempVPosition).ToString("F4")
            FSC.Text = nTitle(SelectedColumn)
            If IsColumnSound(SelectedColumn) Then
                FSW.Text = DefinitionLabel(LWAV.SelectedIndex + 1)
            ElseIf IsColumnImage(SelectedColumn) Then
                FSW.Text = DefinitionLabel(LBMP.SelectedIndex + 1)
            Else
                FSW.Text = ""
            End If
            FSM.Text = Add3Zeros(xMeasure)
            FST.Text = IIf(NTInput, TempLength, IIf(My.Computer.Keyboard.ShiftKeyDown And Not My.Computer.Keyboard.CtrlKeyDown, Strings.StatusBar.LongNote, ""))
            FSH.Text = IIf(My.Computer.Keyboard.CtrlKeyDown And Not My.Computer.Keyboard.ShiftKeyDown, Strings.StatusBar.Hidden, "")
            FSL.Text = IIf(My.Computer.Keyboard.ShiftKeyDown And My.Computer.Keyboard.CtrlKeyDown, Strings.StatusBar.LandMine, "")

        ElseIf TBTimeSelect.Checked Then
            FSSS.Text = vSelStart
            FSSL.Text = vSelLength
            FSSH.Text = vSelHalf

        End If
        FStatus.Invalidate()
    End Sub

    Private Function GetTimeFromVPosition(vpos As Double) As Double
        Dim timing_notes = (From note In Notes
                            Where note.ColumnIndex = niBPM Or note.ColumnIndex = niSTOP
                            Group By Column = note.ColumnIndex
                               Into NoteGroups = Group).ToDictionary(Function(x) x.Column, Function(x) x.NoteGroups)

        Dim bpm_notes = timing_notes.Item(niBPM)

        Dim stop_notes As IEnumerable(Of Note) = Nothing

        If timing_notes.ContainsKey(niSTOP) Then
            stop_notes = timing_notes.Item(niSTOP)
        End If


        Dim stop_contrib As Double
        Dim bpm_contrib As Double

        For i = 0 To bpm_notes.Count() - 1
            ' az: sum bpm contribution first
            Dim duration = 0.0
            Dim current_note = bpm_notes.ElementAt(i)
            Dim notevpos = Math.Max(0, current_note.VPosition)

            If i + 1 <> bpm_notes.Count() Then
                Dim next_note = bpm_notes.ElementAt(i + 1)
                duration = next_note.VPosition - notevpos
            Else
                duration = vpos - notevpos
            End If

            Dim current_bps = 60 / (current_note.Value / 10000)
            bpm_contrib += current_bps * duration / 48

            If stop_notes Is Nothing Then Continue For

            Dim stops = From stp In stop_notes
                        Where stp.VPosition >= notevpos And
                            stp.VPosition < notevpos + duration

            Dim stop_beats = stops.Sum(Function(x) x.Value / 10000.0) / 48
            stop_contrib += current_bps * stop_beats

        Next

        Return stop_contrib + bpm_contrib
    End Function

    Private Sub POBMirror_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles POBMirror.Click
        Dim xI1 As Integer
        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo
        'xRedo &= sCmdKM(niA1, .VPosition, .Value, IIf(NTInput, .Length, .LongNote), .Hidden, RealColumnToEnabled(niA7) - RealColumnToEnabled(niA1), 0, True) & vbCrLf
        'xUndo &= sCmdKM(niA7, .VPosition, .Value, IIf(NTInput, .Length, .LongNote), .Hidden, RealColumnToEnabled(niA1) - RealColumnToEnabled(niA7), 0, True) & vbCrLf

        Dim xCol As Integer = 0
        For xI1 = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For
            If Rscratch Then
                Select Case Notes(xI1).ColumnIndex
                    Case niA1 : xCol = niA7
                    Case niA2 : xCol = niA6
                    Case niA3 : xCol = niA5
                    Case niA4 : xCol = niA4
                    Case niA5 : xCol = niA3
                    Case niA6 : xCol = niA2
                    Case niA7 : xCol = niA1
                    Case niD1 : xCol = niD7
                    Case niD2 : xCol = niD6
                    Case niD3 : xCol = niD5
                    Case niD4 : xCol = niD4
                    Case niD5 : xCol = niD3
                    Case niD6 : xCol = niD2
                    Case niD7 : xCol = niD1
                    Case Else : Continue For
                End Select
            Else
                Select Case Notes(xI1).ColumnIndex
                    Case niA3 : xCol = niA9
                    Case niA4 : xCol = niA8
                    Case niA5 : xCol = niA7
                    Case niA6 : xCol = niA6
                    Case niA7 : xCol = niA5
                    Case niA8 : xCol = niA4
                    Case niA9 : xCol = niA3
                    Case niD1 : xCol = niD7
                    Case niD2 : xCol = niD6
                    Case niD3 : xCol = niD5
                    Case niD4 : xCol = niD4
                    Case niD5 : xCol = niD3
                    Case niD6 : xCol = niD2
                    Case niD7 : xCol = niD1
                    Case Else : Continue For
                End Select
            End If

            Me.RedoMoveNote(Notes(xI1), xCol, Notes(xI1).VPosition, xUndo, xRedo)
            Notes(xI1).ColumnIndex = xCol
        Next

        AddUndo(xUndo, xBaseRedo.Next)
        UpdatePairing()
        RefreshPanelAll()
    End Sub







    Private Sub ValidateSelection()
        If vSelStart < 0 Then vSelLength += vSelStart : vSelHalf += vSelStart : vSelStart = 0
        If vSelStart > GetMaxVPosition() - 1 Then vSelLength += vSelStart - GetMaxVPosition() + 1 : vSelHalf += vSelStart - GetMaxVPosition() + 1 : vSelStart = GetMaxVPosition() - 1
        If vSelStart + vSelLength < 0 Then vSelLength = -vSelStart
        If vSelStart + vSelLength > GetMaxVPosition() - 1 Then vSelLength = GetMaxVPosition() - 1 - vSelStart

        If Math.Sign(vSelHalf) <> Math.Sign(vSelLength) Then vSelHalf = 0
        If Math.Abs(vSelHalf) > Math.Abs(vSelLength) Then vSelHalf = vSelLength
    End Sub



    Private Sub TVCM_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles TVCM.KeyDown
        If e.KeyCode = Keys.Enter Then
            TVCM.Text = Val(TVCM.Text)
            If Val(TVCM.Text) <= 0 Then
                MsgBox(Strings.Messages.NegativeFactorError, MsgBoxStyle.Critical, Strings.Messages.Err)
                TVCM.Text = 1
                TVCM.Focus()
                TVCM.SelectAll()
            Else
                BVCApply_Click(BVCApply, New System.EventArgs)
            End If
        End If
    End Sub

    Private Sub TVCM_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles TVCM.LostFocus
        TVCM.Text = Val(TVCM.Text)
        If Val(TVCM.Text) <= 0 Then
            MsgBox(Strings.Messages.NegativeFactorError, MsgBoxStyle.Critical, Strings.Messages.Err)
            TVCM.Text = 1
            TVCM.Focus()
            TVCM.SelectAll()
        End If
    End Sub

    Private Sub TVCD_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles TVCD.KeyDown
        If e.KeyCode = Keys.Enter Then
            TVCD.Text = Val(TVCD.Text)
            If Val(TVCD.Text) <= 0 Then
                MsgBox(Strings.Messages.NegativeDivisorError, MsgBoxStyle.Critical, Strings.Messages.Err)
                TVCD.Text = 1
                TVCD.Focus()
                TVCD.SelectAll()
            Else
                BVCApply_Click(BVCApply, New System.EventArgs)
            End If
        End If
    End Sub

    Private Sub TVCD_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles TVCD.LostFocus
        TVCD.Text = Val(TVCD.Text)
        If Val(TVCD.Text) <= 0 Then
            MsgBox(Strings.Messages.NegativeDivisorError, MsgBoxStyle.Critical, Strings.Messages.Err)
            TVCD.Text = 1
            TVCD.Focus()
            TVCD.SelectAll()
        End If
    End Sub

    Private Sub TVCBPM_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles TVCBPM.KeyDown
        If e.KeyCode = Keys.Enter Then
            TVCBPM.Text = Val(TVCBPM.Text)
            If Val(TVCBPM.Text) <= 0 Then
                MsgBox(Strings.Messages.NegativeDivisorError, MsgBoxStyle.Critical, Strings.Messages.Err)
                TVCBPM.Text = Notes(0).Value / 10000
                TVCBPM.Focus()
                TVCBPM.SelectAll()
            Else
                BVCCalculate_Click(BVCCalculate, New System.EventArgs)
            End If
        End If
    End Sub

    Private Sub TVCBPM_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles TVCBPM.LostFocus
        TVCBPM.Text = Val(TVCBPM.Text)
        If Val(TVCBPM.Text) <= 0 Then
            MsgBox(Strings.Messages.NegativeDivisorError, MsgBoxStyle.Critical, Strings.Messages.Err)
            TVCBPM.Text = Notes(0).Value / 10000
            TVCBPM.Focus()
            TVCBPM.SelectAll()
        End If
    End Sub

    Private Function FindNoteIndex(note As Note) As Integer
        Dim xI1 As Integer
        If NTInput Then
            For xI1 = 1 To UBound(Notes)
                If Notes(xI1).equalsNT(note) Then Return xI1
            Next
        Else
            For xI1 = 1 To UBound(Notes)
                If Notes(xI1).equalsBMSE(note) Then Return xI1
            Next
        End If
        Return xI1
    End Function




    Private Function NextUndoIndex(ByVal index As Integer, ByVal count As Integer) As Integer
        If index >= count - 1 Then
            Return 0
        End If

        Return index + 1
    End Function

    Private Function PreviousUndoIndex(ByVal index As Integer, ByVal count As Integer) As Integer
        If index <= 0 Then
            Return count - 1
        End If

        Return index - 1
    End Function

    Private Function sIA() As Integer
        Return NextUndoIndex(sI, sUndo.Length)
    End Function

    Private Function sIM() As Integer
        Return PreviousUndoIndex(sI, sUndo.Length)
    End Function

    Private Function IsUndoRedoEnd(ByVal cmd As UndoRedo.LinkedURCmd) As Boolean
        Return cmd Is Nothing OrElse cmd.ofType = UndoRedo.opNoOperation
    End Function

    Private Function CanUndo() As Boolean
        Return Not IsUndoRedoEnd(sUndo(sI))
    End Function

    Private Function CanRedo() As Boolean
        Return Not IsUndoRedoEnd(sRedo(sIA()))
    End Function

    Private Function UndoRedoMemoryLimitBytes() As Long
        Return CLng(UndoRedoMemoryLimitMB) * 1024 * 1024
    End Function

    Private Sub NormalizeUndoRedoMemoryLimit()
        UndoRedoMemoryLimitMB = Math.Min(MaxUndoRedoMemoryLimitMB, Math.Max(MinUndoRedoMemoryLimitMB, UndoRedoMemoryLimitMB))
    End Sub

    Private Function EstimateUndoRedoSlotBytes(ByVal index As Integer) As Long
        If IsUndoRedoEnd(sUndo(index)) AndAlso IsUndoRedoEnd(sRedo(index)) Then Return 0
        Return UndoRedo.EstimateCommandBytes(sUndo(index)) + UndoRedo.EstimateCommandBytes(sRedo(index))
    End Function

    Private Sub ClearUndoRedoSlot(ByVal index As Integer)
        UndoRedoMemoryUsedBytes -= EstimateUndoRedoSlotBytes(index)
        If UndoRedoMemoryUsedBytes < 0 Then UndoRedoMemoryUsedBytes = 0
        sUndo(index) = Nothing
        sRedo(index) = Nothing
    End Sub

    Private Sub StoreUndoRedoSlot(ByVal index As Integer,
                                  ByVal undoCmd As UndoRedo.LinkedURCmd,
                                  ByVal redoCmd As UndoRedo.LinkedURCmd)
        ClearUndoRedoSlot(index)
        sUndo(index) = undoCmd
        sRedo(index) = redoCmd
        UndoRedoMemoryUsedBytes += EstimateUndoRedoSlotBytes(index)
    End Sub

    Private Sub SetUndoRedoEnd(ByVal index As Integer)
        ClearUndoRedoSlot(index)
        sUndo(index) = New UndoRedo.NoOperation
        sRedo(index) = New UndoRedo.NoOperation
    End Sub

    Private Sub RefreshUndoRedoEnabled()
        TBUndo.Enabled = CanUndo()
        TBRedo.Enabled = CanRedo()
        mnUndo.Enabled = TBUndo.Enabled
        mnRedo.Enabled = TBRedo.Enabled
    End Sub



    Private Sub TBUndo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBUndo.Click, mnUndo.Click
        EndKeyMove()
        KMouseOver = -1
        'KMouseDown = -1
        ReDim SelectedNotes(-1)
        If Not CanUndo() Then Exit Sub
        PerformCommand(sUndo(sI))
        sI = sIM()

        RefreshUndoRedoEnabled()
    End Sub

    Private Sub TBRedo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBRedo.Click, mnRedo.Click
        EndKeyMove()
        KMouseOver = -1
        'KMouseDown = -1
        ReDim SelectedNotes(-1)
        If Not CanRedo() Then Exit Sub
        PerformCommand(sRedo(sIA))
        sI = sIA()

        RefreshUndoRedoEnabled()
    End Sub

    'Undo appends before, Redo appends after.
    'After a sequence of Commands, 
    '   Undo will be the first one to execute, 
    '   Redo will be the last one to execute.
    'Remember to save the first Redo.

    'In case where undo is Nothing: Dont worry.
    'In case where redo is Nothing: 
    '   If only one redo is in a sequence, put Nothing.
    '   If several redo are in a sequence, 
    '       Create Void first. 
    '       Record its reference into a seperate copy. (xBaseRedo = xRedo)
    '       Use this xRedo as the BaseRedo.
    '       When calling AddUndo subroutine, use xBaseRedo.Next as cRedo.

    'Dim xUndo As UndoRedo.LinkedURCmd = Nothing
    'Dim xRedo As UndoRedo.LinkedURCmd = Nothing
    '... 'Me.RedoRemoveNote(K(xI1), True, xUndo, xRedo)
    'AddUndo(xUndo, xRedo)

    'Dim xUndo As UndoRedo.LinkedURCmd = Nothing
    'Dim xRedo As New UndoRedo.Void
    'Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo
    '... 'Me.RedoRemoveNote(K(xI1), True, xUndo, xRedo)
    'AddUndo(xUndo, xBaseRedo.Next)

    Private Sub TBOptions_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBVOptions.Click, mnVOptions.Click

        Dim xDiag As New OpVisual(vo, column, LWAV.Font)
        xDiag.ShowDialog(Me)
        CalculateGreatestColumn()
        RefreshPanelAll()
    End Sub

    Private Sub AddToPOWAV(ByVal xPath() As String)
        Dim xIndices(LWAV.SelectedIndices.Count - 1) As Integer
        LWAV.SelectedIndices.CopyTo(xIndices, 0)
        If xIndices.Length = 0 Then Exit Sub

        If WAVEmptyfill Then
            Dim i As Integer = 0
            Dim currWavIndex As Integer = xIndices(0)
            ReDim Preserve xIndices(UBound(xPath))

            Do While i < xIndices.Length And currWavIndex <= DefinitionLastListIndex()
                Do While currWavIndex <= DefinitionLastListIndex() AndAlso hWAV(currWavIndex + 1) <> ""
                    currWavIndex += 1
                Loop
                If currWavIndex > DefinitionLastListIndex() Then Exit Do

                xIndices(i) = currWavIndex
                currWavIndex += 1
                i += 1
            Loop

            If currWavIndex > DefinitionLastListIndex() Then
                ReDim Preserve xPath(i - 1)
                ReDim Preserve xIndices(i - 1)
            End If
        Else
            If xIndices.Length < xPath.Length Then
                Dim i As Integer = xIndices.Length
                Dim currWavIndex As Integer = xIndices(UBound(xIndices)) + 1
                ReDim Preserve xIndices(UBound(xPath))

                Do While i < xIndices.Length And currWavIndex <= DefinitionLastListIndex()
                    Do While currWavIndex <= DefinitionLastListIndex() AndAlso hWAV(currWavIndex + 1) <> ""
                        currWavIndex += 1
                    Loop
                    If currWavIndex > DefinitionLastListIndex() Then Exit Do

                    xIndices(i) = currWavIndex
                    currWavIndex += 1
                    i += 1
                Loop

                If currWavIndex > DefinitionLastListIndex() Then
                    ReDim Preserve xPath(i - 1)
                    ReDim Preserve xIndices(i - 1)
                End If
            End If
        End If

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo
        Dim xChanged As Boolean = False

        'Dim xI2 As Integer = 0
        For xI1 As Integer = 0 To UBound(xPath)
            'If xI2 > UBound(xIndices) Then Exit For
            'hWAV(xIndices(xI2) + 1) = GetFileName(xPath(xI1))
            'LWAV.Items.Item(xIndices(xI2)) = C10to36(xIndices(xI2) + 1) & ": " & GetFileName(xPath(xI1))
            If SetDefinitionValueWithUndo(True, xIndices(xI1) + 1, GetBMSRefPath(xPath(xI1)), xUndo, xRedo) Then xChanged = True
            'xI2 += 1
        Next

        LWAV.SelectedIndices.Clear()
        For xI1 As Integer = 0 To IIf(UBound(xIndices) < UBound(xPath), UBound(xIndices), UBound(xPath))
            LWAV.SelectedIndices.Add(xIndices(xI1))
        Next

        If xChanged Then AddUndo(xUndo, xBaseRedo.Next)
        RefreshPanelAll()
    End Sub

    Private Sub POWAV_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles POWAV.DragDrop
        ReDim DDFileName(-1)
        If Not e.Data.GetDataPresent(DataFormats.FileDrop) Then Return

        Dim xOrigPath() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
        Dim xPath() As String = FilterFileBySupported(xOrigPath, SupportedAudioExtension)
        Array.Sort(xPath)
        If xPath.Length = 0 Then
            RefreshPanelAll()
            Exit Sub
        End If

        AddToPOWAV(xPath)
    End Sub

    Private Sub POWAV_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles POWAV.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
            DDFileName = FilterFileBySupported(CType(e.Data.GetData(DataFormats.FileDrop), String()), SupportedAudioExtension)
        Else
            e.Effect = DragDropEffects.None
        End If
        RefreshPanelAll()
    End Sub

    Private Sub POWAV_DragLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles POWAV.DragLeave
        ReDim DDFileName(-1)
        RefreshPanelAll()
    End Sub

    Private Sub AddToPOBMP(ByVal xPath() As String)
        Dim xIndices(LBMP.SelectedIndices.Count - 1) As Integer
        LBMP.SelectedIndices.CopyTo(xIndices, 0)
        If xIndices.Length = 0 Then Exit Sub

        If WAVEmptyfill Then
            Dim i As Integer = 0
            Dim currBmpIndex As Integer = xIndices(0)
            ReDim Preserve xIndices(UBound(xPath))

            Do While i < xIndices.Length And currBmpIndex <= DefinitionLastListIndex()
                Do While currBmpIndex <= DefinitionLastListIndex() AndAlso hBMP(currBmpIndex + 1) <> ""
                    currBmpIndex += 1
                Loop
                If currBmpIndex > DefinitionLastListIndex() Then Exit Do

                xIndices(i) = currBmpIndex
                currBmpIndex += 1
                i += 1
            Loop

            If currBmpIndex > DefinitionLastListIndex() Then
                ReDim Preserve xPath(i - 1)
                ReDim Preserve xIndices(i - 1)
            End If
        Else
            If xIndices.Length < xPath.Length Then
                Dim i As Integer = xIndices.Length
                Dim currBmpIndex As Integer = xIndices(UBound(xIndices)) + 1
                ReDim Preserve xIndices(UBound(xPath))

                Do While i < xIndices.Length And currBmpIndex <= DefinitionLastListIndex()
                    Do While currBmpIndex <= DefinitionLastListIndex() AndAlso hBMP(currBmpIndex + 1) <> ""
                        currBmpIndex += 1
                    Loop
                    If currBmpIndex > DefinitionLastListIndex() Then Exit Do

                    xIndices(i) = currBmpIndex
                    currBmpIndex += 1
                    i += 1
                Loop

                If currBmpIndex > DefinitionLastListIndex() Then
                    ReDim Preserve xPath(i - 1)
                    ReDim Preserve xIndices(i - 1)
                End If
            End If
        End If

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo
        Dim xChanged As Boolean = False

        'Dim xI2 As Integer = 0
        For xI1 As Integer = 0 To UBound(xPath)
            'If xI2 > UBound(xIndices) Then Exit For
            'hBMP(xIndices(xI2) + 1) = GetFileName(xPath(xI1))
            'LBMP.Items.Item(xIndices(xI2)) = C10to36(xIndices(xI2) + 1) & ": " & GetFileName(xPath(xI1))
            If SetDefinitionValueWithUndo(False, xIndices(xI1) + 1, GetBMSRefPath(xPath(xI1)), xUndo, xRedo) Then xChanged = True
            'xI2 += 1
        Next

        LBMP.SelectedIndices.Clear()
        For xI1 As Integer = 0 To IIf(UBound(xIndices) < UBound(xPath), UBound(xIndices), UBound(xPath))
            LBMP.SelectedIndices.Add(xIndices(xI1))
        Next

        If xChanged Then AddUndo(xUndo, xBaseRedo.Next)
        RefreshPanelAll()
    End Sub

    Private Sub POBMP_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles POBMP.DragDrop
        ReDim DDFileName(-1)
        If Not e.Data.GetDataPresent(DataFormats.FileDrop) Then Return

        Dim xOrigPath() As String = CType(e.Data.GetData(DataFormats.FileDrop), String())
        Dim xPath() As String = FilterFileBySupported(xOrigPath, SupportedImageExtension)
        Array.Sort(xPath)
        If xPath.Length = 0 Then
            RefreshPanelAll()
            Exit Sub
        End If

        AddToPOBMP(xPath)
    End Sub

    Private Sub POBMP_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles POBMP.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            e.Effect = DragDropEffects.Copy
            DDFileName = FilterFileBySupported(CType(e.Data.GetData(DataFormats.FileDrop), String()), SupportedImageExtension)
        Else
            e.Effect = DragDropEffects.None
        End If
        RefreshPanelAll()
    End Sub

    Private Sub POBMP_DragLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles POBMP.DragLeave
        ReDim DDFileName(-1)
        RefreshPanelAll()
    End Sub

    Private Sub mn_DropDownClosed(ByVal sender As Object, ByVal e As System.EventArgs)
        sender.ForeColor = Color.White
    End Sub
    Private Sub mn_DropDownOpened(ByVal sender As Object, ByVal e As System.EventArgs)
        sender.ForeColor = Color.Black
    End Sub
    Private Sub mn_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs)
        If sender.Pressed Then Return
        sender.ForeColor = Color.Black
    End Sub
    Private Sub mn_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs)
        If sender.Pressed Then Return
        sender.ForeColor = Color.White
    End Sub

    Private Sub TBPOptions_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBPOptions.Click, mnPOptions.Click
        Dim xDOp As New OpPlayer(CurrentPlayer)
        If xDOp.ShowDialog(Me) = Windows.Forms.DialogResult.OK Then RefreshPlayerSelector()
    End Sub

    Private Sub THGenre_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles _
    THGenre.TextChanged, THTitle.TextChanged, THArtist.TextChanged, THPlayLevel.TextChanged, CHRank.SelectedIndexChanged, TExpansion.TextChanged,
    THSubTitle.TextChanged, THSubArtist.TextChanged, THStageFile.TextChanged, THBanner.TextChanged, THBackBMP.TextChanged,
    CHDifficulty.SelectedIndexChanged, THExRank.TextChanged, THTotal.TextChanged, THComment.TextChanged, THPreview.TextChanged, CHLnmode.SelectedIndexChanged
        If IsSaved Then SetIsSaved(False)

        If [Object].ReferenceEquals(sender, THLandMine) Then
            hWAV(0) = THLandMine.Text
        ElseIf [Object].ReferenceEquals(sender, THMissBMP) Then
            hBMP(0) = THMissBMP.Text
        End If
    End Sub

    Private Sub CHLnObj_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CHLnObj.SelectedIndexChanged
        If IsSaved Then SetIsSaved(False)
        LnObj = CHLnObj.SelectedIndex
        UpdatePairing()
        RefreshPanelAll()
    End Sub

    Private Sub NormalizeNoteType(ByRef note As Note)
        If note.Landmine Then note.Hidden = False
    End Sub

    Private Sub ConvertBMSE2NT()
        ReDim SelectedNotes(-1)
        SortByVPositionInsertion()

        For i2 As Integer = 0 To UBound(Notes)
            Notes(i2).Length = 0.0#
        Next

        Dim i As Integer = 1
        Dim j As Integer = 0
        Dim xUbound As Integer = UBound(Notes)

        Do While i <= xUbound
            If Not Notes(i).LongNote Then i += 1 : Continue Do

            For j = i + 1 To xUbound
                If Notes(j).ColumnIndex <> Notes(i).ColumnIndex Then Continue For

                If Notes(j).LongNote Then
                    Notes(i).Length = Notes(j).VPosition - Notes(i).VPosition
                    For j2 As Integer = j To xUbound - 1
                        Notes(j2) = Notes(j2 + 1)
                    Next
                    xUbound -= 1
                    Exit For

                ElseIf Notes(j).Value \ 10000 = LnObj Then
                    Exit For

                End If
            Next

            i += 1
        Loop

        ReDim Preserve Notes(xUbound)

        For i = 0 To xUbound
            Notes(i).LongNote = False
            NormalizeNoteType(Notes(i))
        Next

        SortByVPositionInsertion()
        UpdatePairing()
        CalculateTotalPlayableNotes()
    End Sub

    Private Sub ConvertNT2BMSE()
        ReDim SelectedNotes(-1)
        Dim xK(0) As Note
        xK(0) = Notes(0)

        For xI1 As Integer = 1 To UBound(Notes)
            NormalizeNoteType(Notes(xI1))
        Next

        For xI1 As Integer = 1 To UBound(Notes)
            ReDim Preserve xK(UBound(xK) + 1)
            With xK(UBound(xK))
                .ColumnIndex = Notes(xI1).ColumnIndex
                .LongNote = Notes(xI1).Length > 0
                .Landmine = Notes(xI1).Landmine
                .Value = Notes(xI1).Value
                .VPosition = Notes(xI1).VPosition
                .Selected = Notes(xI1).Selected
                .Hidden = Notes(xI1).Hidden
            End With

            If Notes(xI1).Length > 0 Then
                ReDim Preserve xK(UBound(xK) + 1)
                With xK(UBound(xK))
                    .ColumnIndex = Notes(xI1).ColumnIndex
                    .LongNote = True
                    .Landmine = False
                    .Value = Notes(xI1).Value
                    .VPosition = Notes(xI1).VPosition + Notes(xI1).Length
                    .Selected = Notes(xI1).Selected
                    .Hidden = Notes(xI1).Hidden
                End With
            End If
        Next

        Notes = xK

        SortByVPositionInsertion()
        UpdatePairing()
        CalculateTotalPlayableNotes()
    End Sub

    Private Sub SetWavIncreaseChecked(ByVal checked As Boolean)
        TBWavIncrease.Checked = checked
        mnWavIncrease.Checked = checked
    End Sub

    Private Sub TBWavIncrease_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBWavIncrease.Click, mnWavIncrease.Click
        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        Dim xChecked As Boolean = If([Object].ReferenceEquals(sender, mnWavIncrease),
                                     mnWavIncrease.Checked,
                                     Not TBWavIncrease.Checked)
        SetWavIncreaseChecked(xChecked)
        Me.RedoWavIncrease(TBWavIncrease.Checked, xUndo, xRedo)
        AddUndo(xUndo, xBaseRedo.Next)
    End Sub

    Private Sub TBNTInput_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBNTInput.Click, mnNTInput.Click
        'Dim xUndo As String = "NT_" & CInt(NTInput) & "_0" & vbCrLf & "KZ" & vbCrLf & sCmdKsAll(False)
        'Dim xRedo As String = "NT_" & CInt(Not NTInput) & "_1"
        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        Me.RedoRemoveNoteAll(False, xUndo, xRedo)

        NTInput = sender.Checked

        TBNTInput.Checked = NTInput
        mnNTInput.Checked = NTInput
        POBLong.Enabled = Not NTInput
        POBLongShort.Enabled = Not NTInput

        bAdjustLength = False
        bAdjustUpper = False

        Me.RedoNT(NTInput, False, xUndo, xRedo)
        If NTInput Then
            ConvertBMSE2NT()
        Else
            ConvertNT2BMSE()
        End If
        Me.RedoAddNoteAll(False, xUndo, xRedo)

        AddUndo(xUndo, xBaseRedo.Next)
        RefreshPanelAll()
    End Sub

    Private Sub THBPM_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles THBPM.ValueChanged
        If Notes IsNot Nothing Then Notes(0).Value = THBPM.Value * 10000 : RefreshPanelAll()
        If IsSaved Then SetIsSaved(False)
    End Sub

    Private Sub TWPosition_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TWPosition.ValueChanged
        wPosition = TWPosition.Value
        TWPosition2.Value = IIf(wPosition > TWPosition2.Maximum, TWPosition2.Maximum, wPosition)
        RefreshPanelAll()
    End Sub

    Private Sub TWLeft_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TWLeft.ValueChanged
        wLeft = TWLeft.Value
        TWLeft2.Value = IIf(wLeft > TWLeft2.Maximum, TWLeft2.Maximum, wLeft)
        RefreshPanelAll()
    End Sub

    Private Sub TWWidth_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TWWidth.ValueChanged
        wWidth = TWWidth.Value
        TWWidth2.Value = IIf(wWidth > TWWidth2.Maximum, TWWidth2.Maximum, wWidth)
        RefreshPanelAll()
    End Sub

    Private Sub TWPrecision_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TWPrecision.ValueChanged
        wPrecision = TWPrecision.Value
        TWPrecision2.Value = IIf(wPrecision > TWPrecision2.Maximum, TWPrecision2.Maximum, wPrecision)
        RefreshPanelAll()
    End Sub

    Private Sub TWTransparency_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TWTransparency.ValueChanged
        TWTransparency2.Value = TWTransparency.Value
        vo.pBGMWav.Color = Color.FromArgb(TWTransparency.Value, vo.pBGMWav.Color)
        RefreshPanelAll()
    End Sub

    Private Sub TWSaturation_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TWSaturation.ValueChanged
        Dim xColor As Color = vo.pBGMWav.Color
        TWSaturation2.Value = TWSaturation.Value
        vo.pBGMWav.Color = HSL2RGB(xColor.GetHue, TWSaturation.Value, xColor.GetBrightness * 1000, xColor.A)
        RefreshPanelAll()
    End Sub

    Private Sub TWPosition2_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TWPosition2.Scroll
        TWPosition.Value = TWPosition2.Value
    End Sub

    Private Sub TWLeft2_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TWLeft2.Scroll
        TWLeft.Value = TWLeft2.Value
    End Sub

    Private Sub TWWidth2_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TWWidth2.Scroll
        TWWidth.Value = TWWidth2.Value
    End Sub

    Private Sub TWPrecision2_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TWPrecision2.Scroll
        TWPrecision.Value = TWPrecision2.Value
    End Sub

    Private Sub TWTransparency2_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TWTransparency2.Scroll
        TWTransparency.Value = TWTransparency2.Value
    End Sub

    Private Sub TWSaturation2_Scroll(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TWSaturation2.Scroll
        TWSaturation.Value = TWSaturation2.Value
    End Sub

    Private Sub TBLangDef_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBLangDef.Click
        DispLang = ""
        MsgBox(Strings.Messages.PreferencePostpone, MsgBoxStyle.Information)
    End Sub

    Private Sub TBLangRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBLangRefresh.Click
        For xI1 As Integer = cmnLanguage.Items.Count - 1 To 3 Step -1
            Try
                cmnLanguage.Items.RemoveAt(xI1)
            Catch ex As Exception
            End Try
        Next

        If Not Directory.Exists(My.Application.Info.DirectoryPath & "\Data") Then My.Computer.FileSystem.CreateDirectory(My.Application.Info.DirectoryPath & "\Data")
        Dim xFileNames() As FileInfo = My.Computer.FileSystem.GetDirectoryInfo(My.Application.Info.DirectoryPath & "\Data").GetFiles("*.Lang.xml")

        For Each xStr As FileInfo In xFileNames
            LoadLocaleXML(xStr)
        Next
    End Sub


    Private Sub UpdateColumnLefts()
        column(0).Left = 0
        'If col(0).Width = 0 Then col(0).Visible = False

        For xI1 As Integer = 1 To UBound(column)
            column(xI1).Left = column(xI1 - 1).Left + IIf(column(xI1 - 1).isVisible, column(xI1 - 1).Width, 0)
            'If col(xI1).Width = 0 Then col(xI1).Visible = False
        Next
    End Sub

    Private Sub UpdateColumnsX()
        UpdateColumnLefts()
        Dim xMaximum As Integer = nLeft(gColumns) + column(niB).Width
        For i As Integer = 0 To SplitPanes.Count - 1
            SetHorizontalScrollMaximum(SplitPanes(i).HScroll, xMaximum)
        Next
    End Sub

    Private Sub UpdateHorizontalScrollMetrics()
        If SplitPanes.Count = 0 OrElse gxWidth <= 0 Then Return

        For i As Integer = 0 To SplitPanes.Count - 1
            SetHorizontalScrollLargeChange(SplitPanes(i).HScroll, SplitPanes(i).Canvas.Width)
        Next

        CalculateGreatestColumn()
    End Sub

    Private Sub SetHorizontalScrollMaximum(xScroll As EditorScrollBar, xMaximum As Integer)
        xMaximum = Math.Max(xScroll.Minimum, xMaximum)
        If xScroll.Value > xMaximum Then xScroll.Value = xMaximum

        xScroll.Maximum = xMaximum
        ClampHorizontalScrollValue(xScroll)
    End Sub

    Private Sub SetHorizontalScrollLargeChange(xScroll As EditorScrollBar, xPanelWidth As Integer)
        Dim xLargeChange As Integer = CInt(Math.Floor(Math.Max(0, xPanelWidth) / CDbl(gxWidth)))
        xScroll.LargeChange = Math.Max(1, xLargeChange)
        ClampHorizontalScrollValue(xScroll)
    End Sub

    Private Sub ClampHorizontalScrollValue(xScroll As EditorScrollBar)
        Dim xValueMax As Integer = xScroll.Maximum - xScroll.LargeChange + 1
        If xValueMax < xScroll.Minimum Then xValueMax = xScroll.Minimum
        If xScroll.Value > xValueMax Then xScroll.Value = xValueMax
    End Sub

    Private Sub CHPlayer_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CHPlayer.SelectedIndexChanged
        If CHPlayer.SelectedIndex = -1 Then CHPlayer.SelectedIndex = 0

        iPlayer = CHPlayer.SelectedIndex
        Dim xGP2 As Boolean = iPlayer <> 0
        column(niD8).isVisible = (xGP2 And column(niAA).isVisible)
        column(niD9).isVisible = (xGP2 And column(niAB).isVisible)
        column(niDA).isVisible = (xGP2 And column(niAC).isVisible)
        column(niDB).isVisible = (xGP2 And column(niAD).isVisible)
        column(niDC).isVisible = (xGP2 And column(niAE).isVisible)
        column(niDD).isVisible = (xGP2 And column(niAF).isVisible)
        column(niDE).isVisible = (xGP2 And column(niAG).isVisible)
        column(niDF).isVisible = (xGP2 And column(niAH).isVisible)
        column(niDG).isVisible = (xGP2 And column(niAI).isVisible)
        column(niDH).isVisible = (xGP2 And column(niAJ).isVisible)
        column(niDI).isVisible = (xGP2 And column(niAK).isVisible)
        column(niDJ).isVisible = (xGP2 And column(niAL).isVisible)
        column(niDK).isVisible = (xGP2 And column(niAM).isVisible)
        column(niDL).isVisible = (xGP2 And column(niAN).isVisible)
        column(niDM).isVisible = (xGP2 And column(niAO).isVisible)
        column(niDN).isVisible = (xGP2 And column(niAP).isVisible)
        column(niDO).isVisible = (xGP2 And column(niAQ).isVisible)
        If Rscratch Then
            column(niD1).isVisible = (xGP2 And column(niA1).isVisible)
            column(niD2).isVisible = (xGP2 And column(niA2).isVisible)
            column(niD3).isVisible = (xGP2 And column(niA3).isVisible)
            column(niD4).isVisible = (xGP2 And column(niA4).isVisible)
            column(niD5).isVisible = (xGP2 And column(niA5).isVisible)
            column(niD6).isVisible = (xGP2 And column(niA6).isVisible)
            column(niD7).isVisible = (xGP2 And column(niA7).isVisible)
            column(niDP).isVisible = (xGP2 And column(niA8).isVisible)
            column(niDQ).isVisible = (xGP2 And column(niA9).isVisible)
        Else
            column(niD1).isVisible = (xGP2 And column(niA3).isVisible)
            column(niD2).isVisible = (xGP2 And column(niA4).isVisible)
            column(niD3).isVisible = (xGP2 And column(niA5).isVisible)
            column(niD4).isVisible = (xGP2 And column(niA6).isVisible)
            column(niD5).isVisible = (xGP2 And column(niA7).isVisible)
            column(niD6).isVisible = (xGP2 And column(niA8).isVisible)
            column(niD7).isVisible = (xGP2 And column(niA9).isVisible)
            column(niDP).isVisible = (xGP2 And column(niA1).isVisible)
            column(niDQ).isVisible = (xGP2 And column(niA2).isVisible)
        End If
        column(niS3).isVisible = xGP2

        For xI1 As Integer = 1 To UBound(Notes)
            Notes(xI1).Selected = Notes(xI1).Selected And nEnabled(Notes(xI1).ColumnIndex)
        Next
        'AddUndo(xUndo, xRedo)
        CalculateGreatestColumn()

        If IsInitializing Then Exit Sub
        RefreshPanelAll()
    End Sub

    Private Sub CGB_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CGB.ValueChanged
        CalculateGreatestColumn()
        RefreshPanelAll()
    End Sub

    Private Sub TBGOptions_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBGOptions.Click, mnGOptions.Click
        Dim xTE As Integer
        Select Case UCase(EncodingToString(TextEncoding)) ' az: wow seriously? is there really no better way? 
            Case "SYSTEM ANSI" : xTE = 0
            Case "LITTLE ENDIAN UTF16" : xTE = 1
            Case "ASCII" : xTE = 2
            Case "BIG ENDIAN UTF16" : xTE = 3
            Case "LITTLE ENDIAN UTF32" : xTE = 4
            Case "UTF7" : xTE = 5
            Case "UTF8" : xTE = 6
            Case "SJIS" : xTE = 7
            Case "EUC-KR" : xTE = 8
            Case Else : xTE = 0
        End Select

        Dim xDiag As New OpGeneral(gWheel, gPgUpDn, MiddleButtonMoveMethod, xTE, 192.0R / BMSGridLimit,
            AutoSaveInterval, BeepWhileSaved, NewBMSUseBase62Definitions, BPMDefinitionMode, STOPDefinitionMode,
            AutoFocusMouseEnter, FirstClickDisabled, ClickStopPreview, SkipClippedMeasure, LaneHighlight, CInt(CGB.Value), UndoRedoMemoryLimitMB)

        If xDiag.ShowDialog() = Windows.Forms.DialogResult.OK Then
            With xDiag
                gWheel = .zWheel
                gPgUpDn = .zPgUpDn
                TextEncoding = .zEncoding
                'SortingMethod = .zSort
                MiddleButtonMoveMethod = .zMiddle
                AutoSaveInterval = .zAutoSave
                BMSGridLimit = 192.0R / .zGridPartition
                BeepWhileSaved = .cBeep.Checked
                NewBMSUseBase62Definitions = .cNewBMSUseBase62.Checked
                BPMDefinitionMode = .cBpm1296.SelectedIndex
                STOPDefinitionMode = .cStop1296.SelectedIndex
                AutoFocusMouseEnter = .cMEnterFocus.Checked
                FirstClickDisabled = .cMClickFocus.Checked
                ClickStopPreview = .cMStopPreview.Checked
                SkipClippedMeasure = .cSkipClippedMeasure.Checked
                LaneHighlight = .zLaneHighlight
                CGB.Value = Math.Min(CGB.Maximum, Math.Max(CGB.Minimum, CDec(.zBgmLaneCount)))
                UndoRedoMemoryLimitMB = .zUndoRedoMemoryLimitMB
                NormalizeUndoRedoMemoryLimit()
                EnforceUndoRedoHistoryLimit()
            End With
            If AutoSaveInterval Then AutoSaveTimer.Interval = AutoSaveInterval
            AutoSaveTimer.Enabled = AutoSaveInterval
            RefreshGridToolbar()
            RefreshPanelAll()
        End If
    End Sub

    Private Sub POBLong_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles POBLong.Click
        If NTInput Then Exit Sub

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        For xI1 As Integer = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For

            Me.RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition, True, xUndo, xRedo)
            Notes(xI1).LongNote = True
        Next
        AddUndo(xUndo, xBaseRedo.Next)
        SortByVPositionInsertion()
        UpdatePairing()
        RefreshPanelAll()
    End Sub

    Private Sub POBNormal_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles POBShort.Click
        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        If Not NTInput Then
            For xI1 As Integer = 1 To UBound(Notes)
                If Not Notes(xI1).Selected Then Continue For

                Me.RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition, 0, xUndo, xRedo)
                Notes(xI1).LongNote = False
            Next

        Else
            For xI1 As Integer = 1 To UBound(Notes)
                If Not Notes(xI1).Selected Then Continue For

                Me.RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition, 0, xUndo, xRedo)
                Notes(xI1).Length = 0
            Next
        End If

        AddUndo(xUndo, xBaseRedo.Next)
        SortByVPositionInsertion()
        UpdatePairing()
        RefreshPanelAll()
    End Sub

    Private Sub POBNormalLong_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles POBLongShort.Click
        If NTInput Then Exit Sub

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        For xI1 As Integer = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For

            Me.RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition, Not Notes(xI1).LongNote, xUndo, xRedo)
            Notes(xI1).LongNote = Not Notes(xI1).LongNote
        Next

        AddUndo(xUndo, xBaseRedo.Next)
        SortByVPositionInsertion()
        UpdatePairing()
        RefreshPanelAll()
    End Sub

    Private Sub POBHidden_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles POBHidden.Click
        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        For xI1 As Integer = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For

            Dim noteAfterModification = Notes(xI1)
            noteAfterModification.Hidden = True
            noteAfterModification.Landmine = False
            Me.RedoChangeNote(Notes(xI1), noteAfterModification, xUndo, xRedo)
            Notes(xI1) = noteAfterModification
        Next
        AddUndo(xUndo, xBaseRedo.Next)
        SortByVPositionInsertion()
        UpdatePairing()
        CalculateTotalPlayableNotes()
        RefreshPanelAll()
    End Sub

    Private Sub POBVisible_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles POBVisible.Click
        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        For xI1 As Integer = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For

            Dim noteAfterModification = Notes(xI1)
            noteAfterModification.Hidden = False
            noteAfterModification.Landmine = False
            Me.RedoChangeNote(Notes(xI1), noteAfterModification, xUndo, xRedo)
            Notes(xI1) = noteAfterModification
        Next
        AddUndo(xUndo, xBaseRedo.Next)
        SortByVPositionInsertion()
        UpdatePairing()
        CalculateTotalPlayableNotes()
        RefreshPanelAll()
    End Sub

    Private Sub POBHiddenVisible_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles POBHiddenVisible.Click
        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        For xI1 As Integer = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For

            Dim noteAfterModification = Notes(xI1)
            noteAfterModification.Hidden = Not noteAfterModification.Hidden
            noteAfterModification.Landmine = False
            Me.RedoChangeNote(Notes(xI1), noteAfterModification, xUndo, xRedo)
            Notes(xI1) = noteAfterModification
        Next
        AddUndo(xUndo, xBaseRedo.Next)
        SortByVPositionInsertion()
        UpdatePairing()
        CalculateTotalPlayableNotes()
        RefreshPanelAll()
    End Sub

    Private Sub POBLandmine_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles POBLandmine.Click
        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        For xI1 As Integer = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For

            Dim noteAfterModification = Notes(xI1)
            noteAfterModification.Hidden = False
            noteAfterModification.Landmine = True
            Me.RedoChangeNote(Notes(xI1), noteAfterModification, xUndo, xRedo)
            Notes(xI1) = noteAfterModification
        Next
        AddUndo(xUndo, xBaseRedo.Next)
        SortByVPositionInsertion()
        UpdatePairing()
        CalculateTotalPlayableNotes()
        RefreshPanelAll()
    End Sub

    Private Sub POBNormalLandmine_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles POBNormalLandmine.Click
        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        For xI1 As Integer = 1 To UBound(Notes)
            If Not Notes(xI1).Selected Then Continue For

            Dim noteAfterModification = Notes(xI1)
            noteAfterModification.Hidden = False
            noteAfterModification.Landmine = Not noteAfterModification.Landmine
            Me.RedoChangeNote(Notes(xI1), noteAfterModification, xUndo, xRedo)
            Notes(xI1) = noteAfterModification
        Next
        AddUndo(xUndo, xBaseRedo.Next)
        SortByVPositionInsertion()
        UpdatePairing()
        CalculateTotalPlayableNotes()
        RefreshPanelAll()
    End Sub

    Private Sub POBModify_Click(ByVal sender As System.Object, ByVal e As EventArgs) Handles POBModify.Click
        Dim xNum As Boolean = False
        Dim xLbl As Boolean = False
        Dim xI1 As Integer

        For xI1 = 1 To UBound(Notes)
            If Notes(xI1).Selected AndAlso IsColumnNumeric(Notes(xI1).ColumnIndex) Then xNum = True : Exit For
        Next
        For xI1 = 1 To UBound(Notes)
            If Notes(xI1).Selected AndAlso Not IsColumnNumeric(Notes(xI1).ColumnIndex) Then xLbl = True : Exit For
        Next
        If Not (xNum Or xLbl) Then Exit Sub

        If xNum Then
            Dim xD1 As Double = Val(InputBox(Strings.Messages.PromptEnterNumeric, Text)) * 10000
            If Not xD1 = 0 Then
                If xD1 <= 0 Then xD1 = 1

                Dim xUndo As UndoRedo.LinkedURCmd = Nothing
                Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
                Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

                For xI1 = 1 To UBound(Notes)
                    If Not IsColumnNumeric(Notes(xI1).ColumnIndex) Then Continue For
                    If Not Notes(xI1).Selected Then Continue For

                    Me.RedoRelabelNote(Notes(xI1), xD1, xUndo, xRedo)
                    Notes(xI1).Value = xD1
                Next
                AddUndo(xUndo, xBaseRedo.Next)
            End If
        End If

        If xLbl Then
            Dim xStr As String = Trim(InputBox(Strings.Messages.PromptEnter, Me.Text))
            If Not UseBase62Definitions Then xStr = UCase(xStr)

            If Len(xStr) = 0 Then GoTo Jump2
            If xStr = "00" Or xStr = "0" Then GoTo Jump1
            If Not Len(xStr) = 1 And Not Len(xStr) = 2 Then GoTo Jump1
            If Not IsDefinitionLabel(xStr) Then GoTo Jump1

            Dim xDefinitionIndex As Integer = DefinitionIndex(xStr)
            If xDefinitionIndex > DefinitionDisplayMax() Then GoTo Jump1
            Dim xVal As Integer = xDefinitionIndex * 10000

            Dim xUndo As UndoRedo.LinkedURCmd = Nothing
            Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
            Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

            For xI1 = 1 To UBound(Notes)
                If IsColumnNumeric(Notes(xI1).ColumnIndex) Then Continue For
                If Not Notes(xI1).Selected Then Continue For

                Me.RedoRelabelNote(Notes(xI1), xVal, xUndo, xRedo)
                Notes(xI1).Value = xVal
            Next
            AddUndo(xUndo, xBaseRedo.Next)
            GoTo Jump2
Jump1:
            MsgBox(Strings.Messages.InvalidLabel, MsgBoxStyle.Critical, Strings.Messages.Err)
Jump2:
        End If

        RefreshPanelAll()
    End Sub

    Private Sub mnMyO2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnMyO2.Click
        Dim xDiag As New dgMyO2
        xDiag.Show()
    End Sub


    Private Sub TBFind_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBFind.Click, mnFind.Click
        Dim xDiag As New diagFind(gColumns, Strings.Messages.Err, Strings.Messages.InvalidLabel)
        xDiag.Show()
    End Sub

    Private Function fdrCheck(ByVal xNote As Note) As Boolean
        Return xNote.VPosition >= MeasureBottom(fdriMesL) And xNote.VPosition < MeasureBottom(fdriMesU) + MeasureLength(fdriMesU) AndAlso
               IIf(IsColumnNumeric(xNote.ColumnIndex),
                   xNote.Value >= fdriValL And xNote.Value <= fdriValU,
                   xNote.Value >= fdriLblL And xNote.Value <= fdriLblU) AndAlso
               Array.IndexOf(fdriCol, xNote.ColumnIndex) <> -1
    End Function

    Private Function fdrRangeS(ByVal xbLim1 As Boolean, ByVal xbLim2 As Boolean, ByVal xVal As Boolean) As Boolean
        Return (Not xbLim1 And xbLim2 And xVal) Or (xbLim1 And Not xbLim2 And Not xVal) Or (xbLim1 And xbLim2)
    End Function

    Public Sub fdrSelect(ByVal iRange As Integer,
                         ByVal xMesL As Integer, ByVal xMesU As Integer,
                         ByVal xLblL As String, ByVal xLblU As String,
                         ByVal xValL As Integer, ByVal xValU As Integer,
                         ByVal iCol() As Integer)

        fdriMesL = xMesL
        fdriMesU = xMesU
        fdriLblL = DefinitionIndex(xLblL) * 10000
        fdriLblU = DefinitionIndex(xLblU) * 10000
        fdriValL = xValL
        fdriValU = xValU
        fdriCol = iCol

        Dim xbSel As Boolean = iRange Mod 2 = 0
        Dim xbUnsel As Boolean = iRange Mod 3 = 0
        Dim xbShort As Boolean = iRange Mod 5 = 0
        Dim xbLong As Boolean = iRange Mod 7 = 0
        Dim xbHidden As Boolean = iRange Mod 11 = 0
        Dim xbVisible As Boolean = iRange Mod 13 = 0

        Dim xSel(UBound(Notes)) As Boolean
        For xI1 As Integer = 1 To UBound(Notes)
            xSel(xI1) = Notes(xI1).Selected
        Next

        'Main process
        For xI1 As Integer = 1 To UBound(Notes)
            Dim bbba As Boolean = xbSel And xSel(xI1)
            Dim bbbb As Boolean = xbUnsel And Not xSel(xI1)
            Dim bbbc As Boolean = nEnabled(Notes(xI1).ColumnIndex)
            Dim bbbd As Boolean = fdrRangeS(xbShort, xbLong, IIf(NTInput, Notes(xI1).Length, Notes(xI1).LongNote))
            Dim bbbe As Boolean = fdrRangeS(xbVisible, xbHidden, Notes(xI1).Hidden)
            Dim bbbf As Boolean = fdrCheck(Notes(xI1))

            If ((xbSel And xSel(xI1)) Or (xbUnsel And Not xSel(xI1))) AndAlso
                    nEnabled(Notes(xI1).ColumnIndex) AndAlso fdrRangeS(xbShort, xbLong, IIf(NTInput, Notes(xI1).Length, Notes(xI1).LongNote)) And fdrRangeS(xbVisible, xbHidden, Notes(xI1).Hidden) Then
                Notes(xI1).Selected = fdrCheck(Notes(xI1))
            End If
        Next

        RefreshPanelAll()
        Beep()
    End Sub

    Public Sub fdrUnselect(ByVal iRange As Integer,
                           ByVal xMesL As Integer, ByVal xMesU As Integer,
                           ByVal xLblL As String, ByVal xLblU As String,
                           ByVal xValL As Integer, ByVal xValU As Integer,
                           ByVal iCol() As Integer)

        fdriMesL = xMesL
        fdriMesU = xMesU
        fdriLblL = DefinitionIndex(xLblL) * 10000
        fdriLblU = DefinitionIndex(xLblU) * 10000
        fdriValL = xValL
        fdriValU = xValU
        fdriCol = iCol

        Dim xbSel As Boolean = iRange Mod 2 = 0
        Dim xbUnsel As Boolean = iRange Mod 3 = 0
        Dim xbShort As Boolean = iRange Mod 5 = 0
        Dim xbLong As Boolean = iRange Mod 7 = 0
        Dim xbHidden As Boolean = iRange Mod 11 = 0
        Dim xbVisible As Boolean = iRange Mod 13 = 0

        Dim xSel(UBound(Notes)) As Boolean
        For xI1 As Integer = 1 To UBound(Notes)
            xSel(xI1) = Notes(xI1).Selected
        Next

        'Main process
        For xI1 As Integer = 1 To UBound(Notes)
            If ((xbSel And xSel(xI1)) Or (xbUnsel And Not xSel(xI1))) AndAlso
                        nEnabled(Notes(xI1).ColumnIndex) AndAlso fdrRangeS(xbShort, xbLong, IIf(NTInput, Notes(xI1).Length, Notes(xI1).LongNote)) And fdrRangeS(xbVisible, xbHidden, Notes(xI1).Hidden) Then
                Notes(xI1).Selected = Not fdrCheck(Notes(xI1))
            End If
        Next

        RefreshPanelAll()
        Beep()
    End Sub

    Public Sub fdrDelete(ByVal iRange As Integer,
                         ByVal xMesL As Integer, ByVal xMesU As Integer,
                         ByVal xLblL As String, ByVal xLblU As String,
                         ByVal xValL As Integer, ByVal xValU As Integer,
                         ByVal iCol() As Integer)

        fdriMesL = xMesL
        fdriMesU = xMesU
        fdriLblL = DefinitionIndex(xLblL) * 10000
        fdriLblU = DefinitionIndex(xLblU) * 10000
        fdriValL = xValL
        fdriValU = xValU
        fdriCol = iCol

        Dim xbSel As Boolean = iRange Mod 2 = 0
        Dim xbUnsel As Boolean = iRange Mod 3 = 0
        Dim xbShort As Boolean = iRange Mod 5 = 0
        Dim xbLong As Boolean = iRange Mod 7 = 0
        Dim xbHidden As Boolean = iRange Mod 11 = 0
        Dim xbVisible As Boolean = iRange Mod 13 = 0

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        'Main process
        Dim xI1 As Integer = 1
        Do While xI1 <= UBound(Notes)
            If ((xbSel And Notes(xI1).Selected) Or (xbUnsel And Not Notes(xI1).Selected)) AndAlso
                        fdrCheck(Notes(xI1)) AndAlso nEnabled(Notes(xI1).ColumnIndex) AndAlso fdrRangeS(xbShort, xbLong, IIf(NTInput, Notes(xI1).Length, Notes(xI1).LongNote)) And fdrRangeS(xbVisible, xbHidden, Notes(xI1).Hidden) Then
                RedoRemoveNote(Notes(xI1), xUndo, xRedo)
                RemoveNote(xI1, False)
            Else
                xI1 += 1
            End If
        Loop

        AddUndo(xUndo, xBaseRedo.Next)
        SortByVPositionInsertion()
        UpdatePairing()
        RefreshPanelAll()
        CalculateTotalPlayableNotes()
        Beep()
    End Sub

    Public Sub fdrReplaceL(ByVal iRange As Integer,
                           ByVal xMesL As Integer, ByVal xMesU As Integer,
                           ByVal xLblL As String, ByVal xLblU As String,
                           ByVal xValL As Integer, ByVal xValU As Integer,
                           ByVal iCol() As Integer, ByVal xReplaceLbl As String)

        fdriMesL = xMesL
        fdriMesU = xMesU
        fdriLblL = DefinitionIndex(xLblL) * 10000
        fdriLblU = DefinitionIndex(xLblU) * 10000
        fdriValL = xValL
        fdriValU = xValU
        fdriCol = iCol

        Dim xbSel As Boolean = iRange Mod 2 = 0
        Dim xbUnsel As Boolean = iRange Mod 3 = 0
        Dim xbShort As Boolean = iRange Mod 5 = 0
        Dim xbLong As Boolean = iRange Mod 7 = 0
        Dim xbHidden As Boolean = iRange Mod 11 = 0
        Dim xbVisible As Boolean = iRange Mod 13 = 0

        Dim xxLbl As Integer = DefinitionIndex(xReplaceLbl) * 10000

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        'Main process
        For xI1 As Integer = 1 To UBound(Notes)
            If ((xbSel And Notes(xI1).Selected) Or (xbUnsel And Not Notes(xI1).Selected)) AndAlso
                    fdrCheck(Notes(xI1)) AndAlso nEnabled(Notes(xI1).ColumnIndex) And Not IsColumnNumeric(Notes(xI1).ColumnIndex) AndAlso fdrRangeS(xbShort, xbLong, IIf(NTInput, Notes(xI1).Length, Notes(xI1).LongNote)) And fdrRangeS(xbVisible, xbHidden, Notes(xI1).Hidden) Then
                'xUndo &= sCmdKC(K(xI1).ColumnIndex, K(xI1).VPosition, xxLbl, IIf(NTInput, K(xI1).Length, K(xI1).LongNote), K(xI1).Hidden, 0, 0, K(xI1).Value, IIf(NTInput, K(xI1).Length, K(xI1).LongNote), K(xI1).Hidden, True) & vbCrLf
                'xRedo &= sCmdKC(K(xI1).ColumnIndex, K(xI1).VPosition, K(xI1).Value, IIf(NTInput, K(xI1).Length, K(xI1).LongNote), K(xI1).Hidden, 0, 0, xxLbl, IIf(NTInput, K(xI1).Length, K(xI1).LongNote), K(xI1).Hidden, True) & vbCrLf
                Me.RedoRelabelNote(Notes(xI1), xxLbl, xUndo, xRedo)
                Notes(xI1).Value = xxLbl
            End If
        Next

        AddUndo(xUndo, xBaseRedo.Next)
        RefreshPanelAll()
        Beep()
    End Sub

    Public Sub fdrReplaceV(ByVal iRange As Integer,
                           ByVal xMesL As Integer, ByVal xMesU As Integer,
                           ByVal xLblL As String, ByVal xLblU As String,
                           ByVal xValL As Integer, ByVal xValU As Integer,
                           ByVal iCol() As Integer, ByVal xReplaceVal As Integer)

        fdriMesL = xMesL
        fdriMesU = xMesU
        fdriLblL = DefinitionIndex(xLblL) * 10000
        fdriLblU = DefinitionIndex(xLblU) * 10000
        fdriValL = xValL
        fdriValU = xValU
        fdriCol = iCol

        Dim xbSel As Boolean = iRange Mod 2 = 0
        Dim xbUnsel As Boolean = iRange Mod 3 = 0
        Dim xbShort As Boolean = iRange Mod 5 = 0
        Dim xbLong As Boolean = iRange Mod 7 = 0
        Dim xbHidden As Boolean = iRange Mod 11 = 0
        Dim xbVisible As Boolean = iRange Mod 13 = 0

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        'Main process
        For xI1 As Integer = 1 To UBound(Notes)
            If ((xbSel And Notes(xI1).Selected) Or (xbUnsel And Not Notes(xI1).Selected)) AndAlso
                    fdrCheck(Notes(xI1)) AndAlso nEnabled(Notes(xI1).ColumnIndex) And IsColumnNumeric(Notes(xI1).ColumnIndex) AndAlso fdrRangeS(xbShort, xbLong, IIf(NTInput, Notes(xI1).Length, Notes(xI1).LongNote)) And fdrRangeS(xbVisible, xbHidden, Notes(xI1).Hidden) Then
                'xUndo &= sCmdKC(K(xI1).ColumnIndex, K(xI1).VPosition, xReplaceVal, IIf(NTInput, K(xI1).Length, K(xI1).LongNote), K(xI1).Hidden, 0, 0, K(xI1).Value, IIf(NTInput, K(xI1).Length, K(xI1).LongNote), K(xI1).Hidden, True) & vbCrLf
                'xRedo &= sCmdKC(K(xI1).ColumnIndex, K(xI1).VPosition, K(xI1).Value, IIf(NTInput, K(xI1).Length, K(xI1).LongNote), K(xI1).Hidden, 0, 0, xReplaceVal, IIf(NTInput, K(xI1).Length, K(xI1).LongNote), K(xI1).Hidden, True) & vbCrLf
                Me.RedoRelabelNote(Notes(xI1), xReplaceVal, xUndo, xRedo)
                Notes(xI1).Value = xReplaceVal
            End If
        Next

        AddUndo(xUndo, xBaseRedo.Next)
        RefreshPanelAll()
        Beep()
    End Sub

    Private Sub MInsert_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MInsert.Click
        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        Dim xMeasure As Integer = MeasureAtDisplacement(menuVPosition)
        Dim xMLength As Double = MeasureLength(xMeasure)
        Dim xVP As Double = MeasureBottom(xMeasure)

        If NTInput Then
            Dim xI1 As Integer = 1
            Do While xI1 <= UBound(Notes)
                If MeasureAtDisplacement(Notes(xI1).VPosition) >= 999 Then
                    Me.RedoRemoveNote(Notes(xI1), xUndo, xRedo)
                    RemoveNote(xI1, False)
                Else
                    xI1 += 1
                End If
            Loop

            Dim xdVP As Double
            For xI1 = 1 To UBound(Notes)
                If Notes(xI1).VPosition >= xVP And Notes(xI1).VPosition + Notes(xI1).Length <= MeasureBottom(999) Then
                    Me.RedoMoveNote(Notes(xI1), Notes(xI1).ColumnIndex, Notes(xI1).VPosition + xMLength, xUndo, xRedo)
                    Notes(xI1).VPosition += xMLength

                ElseIf Notes(xI1).VPosition >= xVP Then
                    xdVP = MeasureBottom(999) - 1 - Notes(xI1).VPosition - Notes(xI1).Length
                    Me.RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition + xMLength, Notes(xI1).Length + xdVP, xUndo, xRedo)
                    Notes(xI1).VPosition += xMLength
                    Notes(xI1).Length += xdVP

                ElseIf Notes(xI1).VPosition + Notes(xI1).Length >= xVP Then
                    xdVP = IIf(Notes(xI1).VPosition + Notes(xI1).Length > MeasureBottom(999) - 1, GetMaxVPosition() - 1 - Notes(xI1).VPosition - Notes(xI1).Length, xMLength)
                    Me.RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition, Notes(xI1).Length + xdVP, xUndo, xRedo)
                    Notes(xI1).Length += xdVP
                End If
            Next

        Else
            Dim xI1 As Integer = 1
            Do While xI1 <= UBound(Notes)
                If MeasureAtDisplacement(Notes(xI1).VPosition) >= 999 Then
                    Me.RedoRemoveNote(Notes(xI1), xUndo, xRedo)
                    RemoveNote(xI1, False)
                Else
                    xI1 += 1
                End If
            Loop

            For xI1 = 1 To UBound(Notes)
                If Notes(xI1).VPosition >= xVP Then
                    Me.RedoMoveNote(Notes(xI1), Notes(xI1).ColumnIndex, Notes(xI1).VPosition + xMLength, xUndo, xRedo)
                    Notes(xI1).VPosition += xMLength
                End If
            Next
        End If

        For xI1 As Integer = 999 To xMeasure + 1 Step -1
            MeasureLength(xI1) = MeasureLength(xI1 - 1)
        Next
        UpdateMeasureBottom()

        AddUndo(xUndo, xBaseRedo.Next)
        UpdatePairing()
        CalculateGreatestVPosition()
        CalculateTotalPlayableNotes()
        RefreshPanelAll()
    End Sub

    Private Sub MRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MRemove.Click
        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        Dim xMeasure As Integer = MeasureAtDisplacement(menuVPosition)
        Dim xMLength As Double = MeasureLength(xMeasure)
        Dim xVP As Double = MeasureBottom(xMeasure)

        If NTInput Then
            Dim xI1 As Integer = 1
            Do While xI1 <= UBound(Notes)
                If MeasureAtDisplacement(Notes(xI1).VPosition) = xMeasure And MeasureAtDisplacement(Notes(xI1).VPosition + Notes(xI1).Length) = xMeasure Then
                    Me.RedoRemoveNote(Notes(xI1), xUndo, xRedo)
                    RemoveNote(xI1, False)
                Else
                    xI1 += 1
                End If
            Loop

            Dim xdVP As Double
            xVP = MeasureBottom(xMeasure)
            For xI1 = 1 To UBound(Notes)
                If Notes(xI1).VPosition >= xVP + xMLength Then
                    Me.RedoMoveNote(Notes(xI1), Notes(xI1).ColumnIndex, Notes(xI1).VPosition - xMLength, xUndo, xRedo)
                    Notes(xI1).VPosition -= xMLength

                ElseIf Notes(xI1).VPosition >= xVP Then
                    xdVP = xMLength + xVP - Notes(xI1).VPosition
                    Me.RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition + xdVP - xMLength, Notes(xI1).Length - xdVP, xUndo, xRedo)
                    Notes(xI1).VPosition += xdVP - xMLength
                    Notes(xI1).Length -= xdVP

                ElseIf Notes(xI1).VPosition + Notes(xI1).Length >= xVP Then
                    xdVP = IIf(Notes(xI1).VPosition + Notes(xI1).Length >= xVP + xMLength, xMLength, Notes(xI1).VPosition + Notes(xI1).Length - xVP + 1)
                    Me.RedoLongNoteModify(Notes(xI1), Notes(xI1).VPosition, Notes(xI1).Length - xdVP, xUndo, xRedo)
                    Notes(xI1).Length -= xdVP
                End If
            Next

        Else
            Dim xI1 As Integer = 1
            Do While xI1 <= UBound(Notes)
                If MeasureAtDisplacement(Notes(xI1).VPosition) = xMeasure Then
                    Me.RedoRemoveNote(Notes(xI1), xUndo, xRedo)
                    RemoveNote(xI1, False)
                Else
                    xI1 += 1
                End If
            Loop

            xVP = MeasureBottom(xMeasure)
            For xI1 = 1 To UBound(Notes)
                If Notes(xI1).VPosition >= xVP Then
                    Me.RedoMoveNote(Notes(xI1), Notes(xI1).ColumnIndex, Notes(xI1).VPosition - xMLength, xUndo, xRedo)
                    Notes(xI1).VPosition -= xMLength
                End If
            Next
        End If

        For xI1 As Integer = 999 To xMeasure + 1 Step -1
            MeasureLength(xI1 - 1) = MeasureLength(xI1)
        Next
        UpdateMeasureBottom()

        AddUndo(xUndo, xBaseRedo.Next)
        SortByVPositionInsertion()
        UpdatePairing()
        CalculateGreatestVPosition()
        CalculateTotalPlayableNotes()
        RefreshPanelAll()
    End Sub

    Private Sub TBThemeDef_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBThemeDef.Click
        Dim xTempFileName As String = My.Application.Info.DirectoryPath & "\____TempFile.Theme.xml"
        My.Computer.FileSystem.WriteAllText(xTempFileName, My.Resources.O2Mania_Theme, False, System.Text.Encoding.Unicode)
        LoadSettings(xTempFileName)
        ChangePlaySideSkin(False)
        System.IO.File.Delete(xTempFileName)

        RefreshPanelAll()
    End Sub

    Private Sub TBThemeSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBThemeSave.Click
        Dim xDiag As New SaveFileDialog
        xDiag.Filter = Strings.FileType.THEME_XML & "|*.Theme.xml"
        xDiag.DefaultExt = "Theme.xml"
        xDiag.InitialDirectory = My.Application.Info.DirectoryPath & "\Data"
        If xDiag.ShowDialog = Windows.Forms.DialogResult.Cancel Then Exit Sub

        Me.SaveSettings(xDiag.FileName, True)
        If BeepWhileSaved Then Beep()
        TBThemeRefresh_Click(TBThemeRefresh, New System.EventArgs)
    End Sub

    Private Sub TBThemeRefresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBThemeRefresh.Click
        For xI1 As Integer = cmnTheme.Items.Count - 1 To 5 Step -1
            Try
                cmnTheme.Items.RemoveAt(xI1)
            Catch ex As Exception
            End Try
        Next

        If Not Directory.Exists(My.Application.Info.DirectoryPath & "\Data") Then My.Computer.FileSystem.CreateDirectory(My.Application.Info.DirectoryPath & "\Data")
        Dim xFileNames() As FileInfo = My.Computer.FileSystem.GetDirectoryInfo(My.Application.Info.DirectoryPath & "\Data").GetFiles("*.Theme.xml")
        For Each xStr As FileInfo In xFileNames
            cmnTheme.Items.Add(xStr.Name, Nothing, AddressOf LoadTheme)
        Next
    End Sub

    Private Sub TBThemeLoadComptability_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TBThemeLoadComptability.Click
        Dim xDiag As New OpenFileDialog
        xDiag.Filter = Strings.FileType.TH & "|*.th"
        xDiag.DefaultExt = "th"
        xDiag.InitialDirectory = My.Application.Info.DirectoryPath
        If My.Computer.FileSystem.DirectoryExists(My.Application.Info.DirectoryPath & "\Theme") Then xDiag.InitialDirectory = My.Application.Info.DirectoryPath & "\Theme"
        If xDiag.ShowDialog = Windows.Forms.DialogResult.Cancel Then Exit Sub

        Me.LoadThemeComptability(xDiag.FileName)
        RefreshPanelAll()
    End Sub

    ''' <summary>
    ''' Will return Double.PositiveInfinity if canceled.
    ''' </summary>
    Private Function InputBoxDouble(ByVal Prompt As String, ByVal LBound As Double, ByVal UBound As Double, Optional ByVal Title As String = "", Optional ByVal DefaultResponse As String = "") As Double
        Dim xStr As String = InputBox(Prompt, Title, DefaultResponse)
        If xStr = "" Then Return Double.PositiveInfinity

        InputBoxDouble = Val(xStr)
        If InputBoxDouble > UBound Then InputBoxDouble = UBound
        If InputBoxDouble < LBound Then InputBoxDouble = LBound
    End Function

    Private Sub FSSS_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FSSS.Click
        Dim xMax As Double = IIf(vSelLength > 0, GetMaxVPosition() - vSelLength, GetMaxVPosition)
        Dim xMin As Double = IIf(vSelLength < 0, -vSelLength, 0)
        Dim xDouble As Double = InputBoxDouble("Please enter a number between " & xMin & " and " & xMax & ".", xMin, xMax, , vSelStart)
        If xDouble = Double.PositiveInfinity Then Return

        vSelStart = xDouble
        ValidateSelection()
        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Private Sub FSSL_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FSSL.Click
        Dim xMax As Double = GetMaxVPosition() - vSelStart
        Dim xMin As Double = -vSelStart
        Dim xDouble As Double = InputBoxDouble("Please enter a number between " & xMin & " and " & xMax & ".", xMin, xMax, , vSelLength)
        If xDouble = Double.PositiveInfinity Then Return

        vSelLength = xDouble
        ValidateSelection()
        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Private Sub FSSH_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FSSH.Click
        Dim xMax As Double = IIf(vSelLength > 0, vSelLength, 0)
        Dim xMin As Double = IIf(vSelLength > 0, 0, -vSelLength)
        Dim xDouble As Double = InputBoxDouble("Please enter a number between " & xMin & " and " & xMax & ".", xMin, xMax, , vSelHalf)
        If xDouble = Double.PositiveInfinity Then Return

        vSelHalf = xDouble
        ValidateSelection()
        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Private Sub BVCReverse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BVCReverse.Click
        vSelStart += vSelLength
        vSelHalf -= vSelLength
        vSelLength *= -1
        ValidateSelection()
        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Private Sub AutoSaveTimer_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AutoSaveTimer.Tick
        Dim xTime As Date = Now
        Dim xFileName As String
        With xTime
            xFileName = My.Application.Info.DirectoryPath & "\AutoSave_" &
                              .Year & "_" & .Month & "_" & .Day & "_" & .Hour & "_" & .Minute & "_" & .Second & "_" & .Millisecond & ".NBMSC"
        End With
        'My.Computer.FileSystem.WriteAllText(xFileName, SaveNBMSC, False, System.Text.Encoding.Unicode)
        SaveNBMSC(xFileName)

        On Error Resume Next
        If PreviousAutoSavedFileName <> "" Then IO.File.Delete(PreviousAutoSavedFileName)
        On Error GoTo 0

        PreviousAutoSavedFileName = xFileName
    End Sub

    Private Sub CWAVMultiSelect_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CWAVMultiSelect.CheckedChanged
        WAVMultiSelect = CWAVMultiSelect.Checked
        LWAV.SelectionMode = IIf(WAVMultiSelect, SelectionMode.MultiExtended, SelectionMode.One)
        LBMP.SelectionMode = IIf(WAVMultiSelect, SelectionMode.MultiExtended, SelectionMode.One)
    End Sub

    Private Sub CWAVChangeLabel_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CWAVChangeLabel.CheckedChanged
        WAVChangeLabel = CWAVChangeLabel.Checked
    End Sub
    Private Sub CWAVEmptyfill_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CWAVEmptyfill.CheckedChanged
        WAVEmptyfill = CWAVEmptyfill.Checked
    End Sub

    Private Sub CBase62_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CWAVBase62.CheckedChanged, CBMPBase62.CheckedChanged
        Dim xValue As Boolean = CType(sender, CheckBox).Checked
        If UseBase62Definitions = xValue AndAlso CWAVBase62.Checked = xValue AndAlso CBMPBase62.Checked = xValue Then Return

        SetUseBase62Definitions(xValue)
        RefreshDefinitionLists()
        If Not IsInitializing AndAlso IsSaved Then SetIsSaved(False)
    End Sub

    Private Sub mnMain_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles mnMain.MouseDown ', TBMain.MouseDown  ', pttl.MouseDown, pIsSaved.MouseDown
        If e.Button = Windows.Forms.MouseButtons.Left Then
            ReleaseCapture()
            SendMessage(Me.Handle, &H112, &HF012, 0)
            If e.Clicks = 2 Then
                If Me.WindowState = FormWindowState.Maximized Then Me.WindowState = FormWindowState.Normal Else Me.WindowState = FormWindowState.Maximized
            End If
        ElseIf e.Button = Windows.Forms.MouseButtons.Right Then
            'mnSys.Show(sender, e.Location)
        End If
    End Sub

    Private Function IsAnyMainPanelFocused() As Boolean
        For i As Integer = 0 To SplitPanes.Count - 1
            If SplitPanes(i).Canvas.Focused Then Return True
        Next

        Return False
    End Function

    Private Sub mnSelectAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnSelectAll.Click
        If Not IsAnyMainPanelFocused() Then Exit Sub
        For xI1 As Integer = 1 To UBound(Notes)
            Notes(xI1).Selected = nEnabled(Notes(xI1).ColumnIndex)
        Next
        If TBTimeSelect.Checked Then
            CalculateGreatestVPosition()
            vSelStart = 0
            vSelLength = MeasureBottom(MeasureAtDisplacement(GreatestVPosition)) + MeasureLength(MeasureAtDisplacement(GreatestVPosition))
        End If
        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Private Sub mnDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnDelete.Click
        If Not IsAnyMainPanelFocused() Then Exit Sub

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        Me.RedoRemoveNoteSelected(True, xUndo, xRedo)
        RemoveNotes(True)

        AddUndo(xUndo, xBaseRedo.Next)
        CalculateGreatestVPosition()
        CalculateTotalPlayableNotes()
        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Private Enum UpdatePromptAction
        Later
        OpenRelease
        SkipVersion
    End Enum

    Private Async Sub mnUpdate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnUpdate.Click
        Dim xCursor As Cursor = Cursor
        mnUpdate.Enabled = False
        Cursor = Cursors.WaitCursor

        Try
            Dim xResult As UpdateCheckResult = Await System.Threading.Tasks.Task.Run(Function() UpdateChecker.Check(My.Application.Info.Version))
            ShowUpdateCheckResult(xResult, True)
        Finally
            Cursor = xCursor
            mnUpdate.Enabled = True
        End Try
    End Sub

    Private Sub mnUpdateStartup_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnUpdateStartup.CheckedChanged
        CheckUpdatesOnStartup = mnUpdateStartup.Checked
        If IsInitializing Then Return

        Try
            SaveSettings(My.Application.Info.DirectoryPath & "\nBMSC.Settings.xml", False)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, Strings.Messages.Err)
        End Try
    End Sub

    Private Sub StartStartupUpdateCheck()
        System.Threading.Tasks.Task.Run(Sub()
                                            Dim xResult As UpdateCheckResult = UpdateChecker.Check(My.Application.Info.Version)
                                            If Not xResult.IsSuccess OrElse Not xResult.CanCompare OrElse Not xResult.HasUpdate Then Return
                                            If IsUpdateTagSkipped(xResult.LatestTag) Then Return
                                            If IsDisposed OrElse Not IsHandleCreated Then Return

                                            Try
                                                BeginInvoke(New MethodInvoker(Sub()
                                                                                  If Not IsDisposed Then ShowUpdateCheckResult(xResult, False)
                                                                              End Sub))
                                            Catch ex As ObjectDisposedException
                                            Catch ex As InvalidOperationException
                                            End Try
                                        End Sub)
    End Sub

    Private Sub ShowUpdateCheckResult(ByVal xResult As UpdateCheckResult, ByVal xIsManual As Boolean)
        If Not xResult.IsSuccess Then
            If xIsManual Then MsgBox(String.Format(Strings.Messages.UpdateCheckFailed, xResult.ErrorMessage), MsgBoxStyle.Exclamation, Strings.Messages.UpdateCheckTitle)
            Return
        End If

        Dim xCurrentVersion As String = FormatVersionText(My.Application.Info.Version)
        Dim xLatestVersion As String = FormatLatestVersionText(xResult)

        If Not xResult.CanCompare Then
            If xIsManual AndAlso MsgBox(String.Format(Strings.Messages.UpdateVersionUnsupported, xCurrentVersion, xLatestVersion),
                                        MsgBoxStyle.Question Or MsgBoxStyle.YesNo,
                                        Strings.Messages.UpdateCheckTitle) = MsgBoxResult.Yes Then
                OpenUpdatePage(xResult.ReleaseUrl)
            End If
            Return
        End If

        If xResult.HasUpdate Then
            Select Case ShowUpdateAvailableDialog(xResult)
                Case UpdatePromptAction.OpenRelease
                    OpenUpdatePage(xResult.ReleaseUrl)
                Case UpdatePromptAction.SkipVersion
                    SkipUpdateVersion(xResult.LatestTag)
            End Select
        ElseIf xIsManual Then
            MsgBox(String.Format(Strings.Messages.UpdateLatest, xCurrentVersion, xLatestVersion),
                   MsgBoxStyle.Information,
                   Strings.Messages.UpdateCheckTitle)
        End If
    End Sub

    Private Function FormatLatestVersionText(ByVal xResult As UpdateCheckResult) As String
        If xResult IsNot Nothing AndAlso xResult.CanCompare AndAlso xResult.LatestVersion IsNot Nothing Then
            Return FormatVersionText(xResult.LatestVersion)
        End If

        If xResult Is Nothing Then Return ""

        Return xResult.LatestTag
    End Function

    Private Function ShowUpdateAvailableDialog(ByVal xResult As UpdateCheckResult) As UpdatePromptAction
        Using xForm As New Form()
            xForm.Text = Strings.Messages.UpdateCheckTitle
            xForm.FormBorderStyle = FormBorderStyle.FixedDialog
            xForm.StartPosition = FormStartPosition.CenterParent
            xForm.MinimizeBox = False
            xForm.MaximizeBox = False
            xForm.ShowInTaskbar = False
            xForm.Font = Font

            Dim xContentPadding As New Padding(22, 14, 22, 18)
            Dim xButtonPadding As New Padding(12, 8, 12, 8)
            Dim xIconSize As Integer = 32
            Dim xIconTextGap As Integer = 14
            Dim xMainDetailsGap As Integer = 6

            Dim xIcon As New PictureBox()
            xIcon.Image = SystemIcons.Information.ToBitmap()
            xIcon.Size = New Size(xIconSize, xIconSize)
            xIcon.SizeMode = PictureBoxSizeMode.CenterImage

            Dim xMessageText As String = String.Format(Strings.Messages.UpdateAvailable, FormatVersionText(My.Application.Info.Version), FormatLatestVersionText(xResult))
            Dim xMessageLines As String() = xMessageText.Replace(vbCrLf, vbLf).Split(ControlChars.Lf)

            Dim xMainInstruction As New Label()
            xMainInstruction.AutoSize = True
            xMainInstruction.ForeColor = SystemColors.HotTrack
            xMainInstruction.Font = New Font(Font.FontFamily, Font.Size + 2.0!, FontStyle.Regular)
            xMainInstruction.Text = If(xMessageLines.Length > 0, xMessageLines(0), "")

            Dim xContentText As String = ""
            If xMessageLines.Length > 1 Then
                Dim xContentLines(xMessageLines.Length - 2) As String
                Array.Copy(xMessageLines, 1, xContentLines, 0, xContentLines.Length)
                xContentText = String.Join(vbCrLf, xContentLines)
            End If

            Dim xDetails As New Label()
            xDetails.AutoSize = True
            xDetails.Text = xContentText

            Dim xOpen As New Button()
            xOpen.Text = Strings.Messages.UpdateOpenRelease
            xOpen.DialogResult = DialogResult.Yes
            xOpen.Size = MeasureUpdateDialogButton(xOpen.Text, xForm.Font)
            xOpen.Margin = New Padding(6, 0, 0, 0)

            Dim xLater As New Button()
            xLater.Text = Strings.Messages.UpdateLater
            xLater.DialogResult = DialogResult.No
            xLater.Size = MeasureUpdateDialogButton(xLater.Text, xForm.Font)
            xLater.Margin = New Padding(6, 0, 0, 0)

            Dim xSkip As New Button()
            xSkip.Text = Strings.Messages.UpdateSkipVersion
            xSkip.DialogResult = DialogResult.Ignore
            xSkip.Size = MeasureUpdateDialogButton(xSkip.Text, xForm.Font)
            xSkip.Margin = New Padding(6, 0, 0, 0)

            Dim xMainInstructionSize As Size = xMainInstruction.PreferredSize
            Dim xDetailsSize As Size = xDetails.PreferredSize
            Dim xContentTextWidth As Integer = Math.Max(xMainInstructionSize.Width, xDetailsSize.Width)
            Dim xContentTextHeight As Integer = xMainInstructionSize.Height + xMainDetailsGap + xDetailsSize.Height
            Dim xContentWidth As Integer = xContentPadding.Left + xIconSize + xIconTextGap + xContentTextWidth + xContentPadding.Right
            Dim xContentHeight As Integer = xContentPadding.Top + Math.Max(xIconSize, xContentTextHeight) + xContentPadding.Bottom

            Dim xButtons As New FlowLayoutPanel()
            xButtons.FlowDirection = FlowDirection.RightToLeft
            xButtons.Padding = xButtonPadding
            xButtons.WrapContents = False

            Dim xButtonControls As Button() = {xOpen, xSkip, xLater}
            Dim xButtonsWidth As Integer = xButtonPadding.Left + xButtonPadding.Right
            Dim xButtonHeight As Integer = 0
            For Each xButton As Button In xButtonControls
                xButtonsWidth += xButton.Width + xButton.Margin.Left + xButton.Margin.Right
                xButtonHeight = Math.Max(xButtonHeight, xButton.Height + xButton.Margin.Top + xButton.Margin.Bottom)
            Next
            Dim xButtonsHeight As Integer = xButtonPadding.Top + xButtonHeight + xButtonPadding.Bottom
            Dim xClientWidth As Integer = Math.Max(xContentWidth, xButtonsWidth)

            Dim xContent As New Panel()
            xContent.BackColor = SystemColors.Window
            xContent.Location = New Point(0, 0)
            xContent.Size = New Size(xClientWidth, xContentHeight)

            Dim xTextLeft As Integer = xContentPadding.Left + xIconSize + xIconTextGap
            Dim xTextTop As Integer = xContentPadding.Top
            xIcon.Location = New Point(xContentPadding.Left, xContentPadding.Top)
            xMainInstruction.Location = New Point(xTextLeft, xTextTop)
            xDetails.Location = New Point(xTextLeft, xTextTop + xMainInstructionSize.Height + xMainDetailsGap)
            xContent.Controls.Add(xIcon)
            xContent.Controls.Add(xMainInstruction)
            xContent.Controls.Add(xDetails)

            xButtons.Location = New Point(0, xContentHeight)
            xButtons.Size = New Size(xClientWidth, xButtonsHeight)
            xButtons.Controls.Add(xLater)
            xButtons.Controls.Add(xSkip)
            xButtons.Controls.Add(xOpen)

            xForm.ClientSize = New Size(xClientWidth, xContentHeight + xButtonsHeight)
            xForm.Controls.Add(xContent)
            xForm.Controls.Add(xButtons)
            xForm.AcceptButton = xOpen
            xForm.CancelButton = xLater

            Select Case xForm.ShowDialog(Me)
                Case DialogResult.Yes
                    Return UpdatePromptAction.OpenRelease
                Case DialogResult.Ignore
                    Return UpdatePromptAction.SkipVersion
                Case Else
                    Return UpdatePromptAction.Later
            End Select
        End Using
    End Function

    Private Function MeasureUpdateDialogButton(ByVal xText As String, ByVal xFont As Font) As Size
        Dim xTextSize As Size = TextRenderer.MeasureText(xText, xFont, New Size(Integer.MaxValue, Integer.MaxValue), TextFormatFlags.SingleLine)
        Return New Size(xTextSize.Width + 24, Math.Max(27, xTextSize.Height + 9))
    End Function

    Private Function IsUpdateTagSkipped(ByVal xTag As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(xTag) AndAlso
               String.Equals(SkippedUpdateTag, xTag, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Sub SkipUpdateVersion(ByVal xTag As String)
        If String.IsNullOrWhiteSpace(xTag) Then Return

        SkippedUpdateTag = xTag
        Try
            SaveSettings(My.Application.Info.DirectoryPath & "\nBMSC.Settings.xml", False)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, Strings.Messages.Err)
        End Try
    End Sub

    Private Sub OpenUpdatePage(ByVal xUrl As String)
        If String.IsNullOrWhiteSpace(xUrl) Then Return

        Try
            Process.Start(xUrl)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation, Strings.Messages.Err)
        End Try
    End Sub

    Private Sub mnQuit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnQuit.Click
        Close()
    End Sub


    Private Sub EnableDWM()
        mnMain.BackColor = Color.Black
        'TBMain.BackColor = Color.FromArgb(64, 64, 64)

        For Each xmn As ToolStripMenuItem In mnMain.Items
            xmn.ForeColor = Color.White
            AddHandler xmn.DropDownClosed, AddressOf mn_DropDownClosed
            AddHandler xmn.DropDownOpened, AddressOf mn_DropDownOpened
            AddHandler xmn.MouseEnter, AddressOf mn_MouseEnter
            AddHandler xmn.MouseLeave, AddressOf mn_MouseLeave
        Next
    End Sub

    Private Sub DisableDWM()
        mnMain.BackColor = SystemColors.Control
        'TBMain.BackColor = SystemColors.Control

        For Each xmn As ToolStripMenuItem In mnMain.Items
            xmn.ForeColor = SystemColors.ControlText
            RemoveHandler xmn.DropDownClosed, AddressOf mn_DropDownClosed
            RemoveHandler xmn.DropDownOpened, AddressOf mn_DropDownOpened
            RemoveHandler xmn.MouseEnter, AddressOf mn_MouseEnter
            RemoveHandler xmn.MouseLeave, AddressOf mn_MouseLeave
        Next
    End Sub

    Private Sub ttlIcon_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        'ttlIcon.Image = My.Resources.icon2_16
        'mnSys.Show(ttlIcon, 0, ttlIcon.Height)
    End Sub
    Private Sub ttlIcon_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs)
        'ttlIcon.Image = My.Resources.icon2_16_highlight
    End Sub
    Private Sub ttlIcon_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs)
        'ttlIcon.Image = My.Resources.icon2_16
    End Sub

    Private Sub mnSMenu_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnSMenu.CheckedChanged
        mnMain.Visible = mnSMenu.Checked
    End Sub
    Private Sub mnSTB_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnSTB.CheckedChanged
        TBMain.Visible = mnSTB.Checked
    End Sub
    Private Sub mnSOP_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnSOP.CheckedChanged
        POptionsScroll.Visible = mnSOP.Checked
        POptionsResizer.Visible = mnSOP.Checked
        KeepOptionsPanelDockedRight()
        ResizeSplitPanelsByRatio()
        If mnSOP.Checked Then
            ResetSelectedOptionsTabScroll()
            QueueResetSelectedOptionsTabScroll()
        End If
    End Sub
    Private Sub mnSStatus_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnSStatus.CheckedChanged
        pStatus.Visible = mnSStatus.Checked
    End Sub
    Private Sub AddSplitter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnSAddSplitter.Click, TBAddSplitter.Click
        AddRightSplitPane()
    End Sub

    Private Sub RemoveSplitter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnSRemoveSplitter.Click, TBRemoveSplitter.Click
        RemoveRightSplitPane()
    End Sub

    Private Sub SyncSplitterScroll_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnSyncSplitterScroll.CheckedChanged, TBSyncSplitterScroll.CheckedChanged
        If UpdatingSplitterControls Then Return
        SetSplitterScrollSync(sender.Checked)
    End Sub

    Private Sub InitializeSplitPanes()
        PMainIn.Tag = MainPanelIndex
        PMain.Tag = MainPanelIndex

        Dim xMainVScroll As EditorScrollBar = CreateEditorScrollBar(Orientation.Vertical, "MainEditorVScroll", MainPanelIndex, MainPanelScroll.Minimum, MainPanelScroll.Maximum, MainPanelScroll.LargeChange, MainPanelScroll.SmallChange)
        Dim xMainHScroll As EditorScrollBar = CreateEditorScrollBar(Orientation.Horizontal, "MainEditorHScroll", MainPanelIndex, HS.Minimum, HS.Maximum, HS.LargeChange, HS.SmallChange)
        Dim xMainView As New Panel With {
            .BackColor = PMain.BackColor,
            .Dock = DockStyle.Fill,
            .Name = "MainEditorView",
            .Tag = MainPanelIndex
        }
        Dim xMainHScrollStrip As Panel = CreateHorizontalScrollStrip("MainEditorHScrollStrip", xMainHScroll, MainPanelIndex)

        PMain.Controls.Remove(PMainIn)
        PMain.Controls.Remove(MainPanelScroll)
        PMain.Controls.Remove(HS)
        MainPanelScroll.Visible = False
        HS.Visible = False
        xMainView.Controls.Add(PMainIn)
        xMainView.Controls.Add(xMainVScroll)
        PMain.Controls.Add(xMainView)
        PMain.Controls.Add(xMainHScrollStrip)

        SpL.Visible = False
        SpR.Visible = False
        PMainL.Visible = False
        PMainR.Visible = False
        PMainL.Width = 0
        PMainR.Width = 0

        SplitPanes.Clear()
        SplitPanes.Add(New SplitPane With {
            .Container = PMain,
            .Canvas = PMainIn,
            .VScroll = xMainVScroll,
            .HScroll = xMainHScroll,
            .Ratio = 0.0!
        })
        AddHandler xMainVScroll.GotFocus, AddressOf VSGotFocus
        AddHandler xMainVScroll.ValueChanged, AddressOf VSValueChanged
        AddHandler xMainHScroll.GotFocus, AddressOf HSGotFocus
        AddHandler xMainHScroll.ValueChanged, AddressOf HSValueChanged
        RebuildPanelArrays()
        UpdateHorizontalScrollMetrics()
        RefreshSplitterControls()
    End Sub

    Private Function RightSplitPaneCount() As Integer
        Return Math.Max(0, SplitPanes.Count - 1)
    End Function

    Private Function IsValidPanelIndex(ByVal panelIndex As Integer) As Boolean
        Return panelIndex >= 0 AndAlso panelIndex < SplitPanes.Count
    End Function

    Private Function GetSplitterWidth() As Integer
        If SpR IsNot Nothing AndAlso SpR.Width > 0 Then Return SpR.Width
        Return 5
    End Function

    Private Function CanAddRightSplitPane() As Boolean
        If GetSplitLayoutWidth() <= 0 Then Return True
        Return GetMaxSplitPanelWidth(SplitPanes.Count) >= MinSplitPanelWidth
    End Function

    Private Sub AddRightSplitPane(Optional ByVal ratio As Single = DefaultSplitPanelRatio, Optional ByVal limitOnShow As Boolean = True)
        If UpdatingSplitterControls Then Return
        If Not CanAddRightSplitPane() Then Return

        UpdatingSplitterControls = True
        Try
            Dim xIndex As Integer = SplitPanes.Count
            Dim xPane As SplitPane = CreateRightSplitPane(xIndex, ratio)
            SplitPanes.Add(xPane)
            RebuildPanelArrays()

            xPane.Container.Width = GetSplitPanelWidth(xIndex, limitOnShow)
            SetPanelVScroll(xIndex, PanelVScroll(MainPanelIndex))
            UpdateHorizontalScrollMetrics()
            RefreshSplitterControls()
        Finally
            UpdatingSplitterControls = False
        End Try

        If Me.Created Then RefreshPanelAll()
    End Sub

    Private Function CreateRightSplitPane(ByVal panelIndex As Integer, ByVal ratio As Single) As SplitPane
        Dim xContainer As New Panel With {
            .BackColor = PMain.BackColor,
            .Dock = DockStyle.Right,
            .Font = PMain.Font,
            .ForeColor = PMain.ForeColor,
            .Name = "PMainRight" & panelIndex.ToString(),
            .TabIndex = PMain.TabIndex,
            .Tag = panelIndex
        }
        Dim xCanvas As New Panel With {
            .BackColor = PMainIn.BackColor,
            .Dock = DockStyle.Fill,
            .Font = PMainIn.Font,
            .ForeColor = PMainIn.ForeColor,
            .Name = "PMainInRight" & panelIndex.ToString(),
            .TabStop = True,
            .Tag = panelIndex
        }
        Dim xVScroll As EditorScrollBar = CreateEditorScrollBar(Orientation.Vertical, "RightPanelScroll" & panelIndex.ToString(), panelIndex,
                                                                 SplitPanes(MainPanelIndex).VScroll.Minimum,
                                                                 SplitPanes(MainPanelIndex).VScroll.Maximum,
                                                                 SplitPanes(MainPanelIndex).VScroll.LargeChange,
                                                                 SplitPanes(MainPanelIndex).VScroll.SmallChange)
        Dim xHScroll As EditorScrollBar = CreateEditorScrollBar(Orientation.Horizontal, "HSRight" & panelIndex.ToString(), panelIndex,
                                                                 SplitPanes(MainPanelIndex).HScroll.Minimum,
                                                                 SplitPanes(MainPanelIndex).HScroll.Maximum,
                                                                 SplitPanes(MainPanelIndex).HScroll.LargeChange,
                                                                 SplitPanes(MainPanelIndex).HScroll.SmallChange)
        Dim xView As New Panel With {
            .BackColor = xContainer.BackColor,
            .Dock = DockStyle.Fill,
            .Name = "PMainViewRight" & panelIndex.ToString(),
            .Tag = panelIndex
        }
        Dim xHScrollStrip As Panel = CreateHorizontalScrollStrip("HSRightStrip" & panelIndex.ToString(), xHScroll, panelIndex)
        Dim xSplitter As New TransparentSplitterGrip With {
            .Name = "SpRight" & panelIndex.ToString(),
            .Size = New Size(GetSplitterWidth(), 0),
            .Tag = panelIndex
        }
        If SplitterResizeCursor IsNot Nothing Then xSplitter.Cursor = SplitterResizeCursor

        xView.Controls.Add(xCanvas)
        xView.Controls.Add(xVScroll)
        xContainer.Controls.Add(xView)
        xContainer.Controls.Add(xHScrollStrip)
        xContainer.Controls.Add(xSplitter)
        PositionSplitterGrip(xSplitter, xContainer)
        xSplitter.BringToFront()
        ToolStripContainer1.ContentPanel.Controls.Add(xContainer)
        KeepOptionsPanelDockedRight()

        AddPanelHandlers(xCanvas, xVScroll, xHScroll)
        AddHandler xContainer.Resize, AddressOf SplitPaneContainer_Resize
        AddHandler xSplitter.MouseDown, AddressOf HorizontalResizer_MouseDown
        AddHandler xSplitter.MouseMove, AddressOf Splitter_MouseMove
        AddHandler xSplitter.MouseUp, AddressOf Splitter_MouseUp
        AddHandler xSplitter.MouseCaptureChanged, AddressOf Splitter_MouseCaptureChanged

        Return New SplitPane With {
            .Container = xContainer,
            .Canvas = xCanvas,
            .VScroll = xVScroll,
            .HScroll = xHScroll,
            .Splitter = xSplitter,
            .Ratio = If(ratio <= 0.0!, DefaultSplitPanelRatio, ClampSplitPanelRatio(ratio))
        }
    End Function

    Private Function CreateEditorScrollBar(ByVal orientation As Orientation, ByVal name As String, ByVal panelIndex As Integer,
                                           ByVal minimum As Integer, ByVal maximum As Integer, ByVal largeChange As Integer,
                                           ByVal smallChange As Integer) As EditorScrollBar
        Dim xScroll As New EditorScrollBar With {
            .Dock = If(orientation = Orientation.Vertical, DockStyle.Right, DockStyle.Bottom),
            .Name = name,
            .Tag = panelIndex,
            .Orientation = orientation,
            .Minimum = minimum,
            .Maximum = maximum,
            .LargeChange = largeChange,
            .SmallChange = smallChange
        }
        Return xScroll
    End Function

    Private Function CreateHorizontalScrollStrip(ByVal name As String, ByVal xHScroll As EditorScrollBar, ByVal panelIndex As Integer) As Panel
        Dim xStrip As New Panel With {
            .BackColor = xHScroll.BackColor,
            .Dock = DockStyle.Bottom,
            .Height = EditorScrollBarThickness,
            .Name = name,
            .Tag = panelIndex
        }
        Dim xCorner As New Panel With {
            .BackColor = xHScroll.BackColor,
            .Dock = DockStyle.Right,
            .Name = name & "Corner",
            .TabStop = False,
            .Width = EditorScrollBarThickness
        }

        xHScroll.Dock = DockStyle.Fill
        xStrip.Controls.Add(xHScroll)
        xStrip.Controls.Add(xCorner)

        Return xStrip
    End Function

    Private Sub PositionSplitterGrip(ByVal xSplitter As Control, ByVal xContainer As Control)
        xSplitter.Bounds = New Rectangle(0, 0, GetSplitterWidth(), Math.Max(0, xContainer.Height - EditorScrollBarThickness))
    End Sub

    Private Sub SplitPaneContainer_Resize(ByVal sender As Object, ByVal e As EventArgs)
        Dim xContainer As Control = DirectCast(sender, Control)
        For Each xControl As Control In xContainer.Controls
            If TypeOf xControl Is TransparentSplitterGrip Then
                PositionSplitterGrip(xControl, xContainer)
                xControl.BringToFront()
                Exit For
            End If
        Next
    End Sub

    Private Sub AddPanelHandlers(ByVal xCanvas As Panel, ByVal xVScroll As EditorScrollBar, ByVal xHScroll As EditorScrollBar)
        AddHandler xCanvas.PreviewKeyDown, AddressOf PMainInPreviewKeyDown
        AddHandler xCanvas.KeyUp, AddressOf PMainInKeyUp
        AddHandler xCanvas.Resize, AddressOf PMainInResize
        AddHandler xCanvas.LostFocus, AddressOf PMainInLostFocus
        AddHandler xCanvas.MouseDown, AddressOf PMainInMouseDown
        AddHandler xCanvas.MouseEnter, AddressOf PMainInMouseEnter
        AddHandler xCanvas.MouseLeave, AddressOf PMainInMouseLeave
        AddHandler xCanvas.MouseMove, AddressOf PMainInMouseMove
        AddHandler xCanvas.MouseWheel, AddressOf PMain_Scroll
        AddHandler xCanvas.MouseWheel, AddressOf PMainInMouseWheel
        AddHandler xCanvas.MouseUp, AddressOf PMainInMouseUp
        AddHandler xCanvas.Paint, AddressOf PMainInPaint
        AddHandler xVScroll.GotFocus, AddressOf VSGotFocus
        AddHandler xVScroll.ValueChanged, AddressOf VSValueChanged
        AddHandler xHScroll.GotFocus, AddressOf HSGotFocus
        AddHandler xHScroll.ValueChanged, AddressOf HSValueChanged
    End Sub

    Private Sub RemovePanelHandlers(ByVal xPane As SplitPane)
        RemoveHandler xPane.Canvas.PreviewKeyDown, AddressOf PMainInPreviewKeyDown
        RemoveHandler xPane.Canvas.KeyUp, AddressOf PMainInKeyUp
        RemoveHandler xPane.Canvas.Resize, AddressOf PMainInResize
        RemoveHandler xPane.Canvas.LostFocus, AddressOf PMainInLostFocus
        RemoveHandler xPane.Canvas.MouseDown, AddressOf PMainInMouseDown
        RemoveHandler xPane.Canvas.MouseEnter, AddressOf PMainInMouseEnter
        RemoveHandler xPane.Canvas.MouseLeave, AddressOf PMainInMouseLeave
        RemoveHandler xPane.Canvas.MouseMove, AddressOf PMainInMouseMove
        RemoveHandler xPane.Canvas.MouseWheel, AddressOf PMain_Scroll
        RemoveHandler xPane.Canvas.MouseWheel, AddressOf PMainInMouseWheel
        RemoveHandler xPane.Canvas.MouseUp, AddressOf PMainInMouseUp
        RemoveHandler xPane.Canvas.Paint, AddressOf PMainInPaint
        RemoveHandler xPane.Container.Resize, AddressOf SplitPaneContainer_Resize
        RemoveHandler xPane.VScroll.GotFocus, AddressOf VSGotFocus
        RemoveHandler xPane.VScroll.ValueChanged, AddressOf VSValueChanged
        RemoveHandler xPane.HScroll.GotFocus, AddressOf HSGotFocus
        RemoveHandler xPane.HScroll.ValueChanged, AddressOf HSValueChanged
        If xPane.Splitter IsNot Nothing Then
            RemoveHandler xPane.Splitter.MouseDown, AddressOf HorizontalResizer_MouseDown
            RemoveHandler xPane.Splitter.MouseMove, AddressOf Splitter_MouseMove
            RemoveHandler xPane.Splitter.MouseUp, AddressOf Splitter_MouseUp
            RemoveHandler xPane.Splitter.MouseCaptureChanged, AddressOf Splitter_MouseCaptureChanged
        End If
    End Sub

    Private Sub RemoveRightSplitPane(Optional ByVal panelIndex As Integer = -1)
        If UpdatingSplitterControls OrElse RightSplitPaneCount() = 0 Then Return
        If panelIndex = -1 Then panelIndex = SplitPanes.Count - 1
        If panelIndex <= MainPanelIndex OrElse panelIndex >= SplitPanes.Count Then Return

        UpdatingSplitterControls = True
        Try
            Dim xIndex As Integer = panelIndex
            Dim xPane As SplitPane = SplitPanes(xIndex)
            StoreSplitPanelRatio(xIndex)
            RemovePanelHandlers(xPane)
            ToolStripContainer1.ContentPanel.Controls.Remove(xPane.Container)
            xPane.Container.Dispose()
            SplitPanes.RemoveAt(xIndex)

            If PanelFocus = xIndex Then
                PanelFocus = MainPanelIndex
                PMainIn.Focus()
            ElseIf PanelFocus > xIndex Then
                PanelFocus -= 1
            End If
            If spMouseOver = xIndex Then
                spMouseOver = MainPanelIndex
            ElseIf spMouseOver > xIndex Then
                spMouseOver -= 1
            End If

            RebuildPanelArrays()
            ClearPanelBuffers()
            UpdateHorizontalScrollMetrics()
            RefreshSplitterControls()
        Finally
            UpdatingSplitterControls = False
        End Try

        If Me.Created Then RefreshPanelAll()
    End Sub

    Private Sub RebuildPanelArrays()
        ReDim spMain(SplitPanes.Count - 1)
        ReDim PanelWidth(SplitPanes.Count - 1)
        ReDim PanelhBMSCROLL(SplitPanes.Count - 1)
        ReDim PanelVScroll(SplitPanes.Count - 1)

        For i As Integer = 0 To SplitPanes.Count - 1
            Dim xPane As SplitPane = SplitPanes(i)
            xPane.Container.Tag = i
            xPane.Canvas.Tag = i
            xPane.VScroll.Tag = i
            xPane.HScroll.Tag = i
            If xPane.Splitter IsNot Nothing Then xPane.Splitter.Tag = i

            spMain(i) = xPane.Canvas
            PanelWidth(i) = xPane.Container.Width
            PanelhBMSCROLL(i) = xPane.HScroll.Value
            PanelVScroll(i) = xPane.VScroll.Value
        Next
    End Sub

    Private Sub RefreshSplitterControls()
        Dim xCanRemove As Boolean = RightSplitPaneCount() > 0
        Dim xCanAdd As Boolean = CanAddRightSplitPane()

        If TBAddSplitter IsNot Nothing Then TBAddSplitter.Enabled = xCanAdd
        If mnSAddSplitter IsNot Nothing Then mnSAddSplitter.Enabled = xCanAdd
        If TBRemoveSplitter IsNot Nothing Then TBRemoveSplitter.Enabled = xCanRemove
        If mnSRemoveSplitter IsNot Nothing Then mnSRemoveSplitter.Enabled = xCanRemove
    End Sub

    Private Sub ApplySplitPaneFont(ByVal xFont As Font)
        For i As Integer = 0 To SplitPanes.Count - 1
            SplitPanes(i).Container.Font = xFont
            SplitPanes(i).Canvas.Font = xFont
        Next
    End Sub

    Private Function GetOptionsPanelDockWidth() As Integer
        If POptionsScroll Is Nothing OrElse Not POptionsScroll.Visible Then Return 0

        Dim xWidth As Integer = POptionsScroll.Width
        If POptionsResizer IsNot Nothing AndAlso POptionsResizer.Visible Then xWidth += POptionsResizer.Width
        Return xWidth
    End Function

    Private Function GetSplitLayoutWidth() As Integer
        If ToolStripContainer1 Is Nothing Then Return 0
        Return Math.Max(0, ToolStripContainer1.ContentPanel.Width - GetOptionsPanelDockWidth())
    End Function

    Private Function GetSplitPanelWidth(ByVal panelIndex As Integer, Optional ByVal limitOnShow As Boolean = False) As Integer
        If panelIndex <= MainPanelIndex OrElse panelIndex >= SplitPanes.Count Then Return 0

        Dim xLayoutWidth As Integer = GetSplitLayoutWidth()
        Dim xRatio As Single = GetSplitPanelRatio(panelIndex)
        If limitOnShow AndAlso xRatio > MaxSplitPanelShowRatio Then
            SplitPanes(panelIndex).Ratio = MaxSplitPanelShowRatio
            xRatio = MaxSplitPanelShowRatio
        End If

        Dim xWidth As Integer = CInt(xLayoutWidth * xRatio)
        xWidth = Math.Max(MinSplitPanelWidth, xWidth)

        Dim xMaxWidth As Integer = GetMaxSplitPanelWidth(panelIndex)
        If limitOnShow Then
            Dim xMaxShowWidth As Integer = Math.Max(MinSplitPanelWidth, CInt(xLayoutWidth * MaxSplitPanelShowRatio))
            xMaxWidth = Math.Min(xMaxWidth, xMaxShowWidth)
        End If

        Return Math.Min(xWidth, xMaxWidth)
    End Function

    Private Function GetMaxSplitPanelWidth(ByVal panelIndex As Integer) As Integer
        If ToolStripContainer1 Is Nothing Then Return MinSplitPanelWidth

        Dim xOtherWidth As Integer = 0
        For i As Integer = 1 To SplitPanes.Count - 1
            If i = panelIndex Then Continue For
            xOtherWidth += SplitPanes(i).Container.Width
        Next

        Dim xMaxWidth As Integer = GetSplitLayoutWidth() - MinMainPanelWidth - xOtherWidth
        If panelIndex >= SplitPanes.Count Then Return Math.Max(0, xMaxWidth)
        Return Math.Max(MinSplitPanelWidth, xMaxWidth)
    End Function

    Private Sub StoreSplitPanelRatio(ByVal panelIndex As Integer)
        If panelIndex <= MainPanelIndex OrElse panelIndex >= SplitPanes.Count Then Return

        Dim xPanel As Panel = SplitPanes(panelIndex).Container
        Dim xLayoutWidth As Integer = GetSplitLayoutWidth()
        If xPanel.Width > 0 AndAlso xLayoutWidth > 0 Then
            SplitPanes(panelIndex).Ratio = ClampSplitPanelRatio(xPanel.Width / xLayoutWidth)
        End If
    End Sub

    Private Function GetSplitPanelRatio(ByVal panelIndex As Integer) As Single
        If panelIndex <= MainPanelIndex OrElse panelIndex >= SplitPanes.Count Then Return 0.0!

        Dim xRatio As Single = SplitPanes(panelIndex).Ratio
        If xRatio <= 0.0! Then xRatio = DefaultSplitPanelRatio
        Return ClampSplitPanelRatio(xRatio)
    End Function

    Private Function ClampSplitPanelRatio(ByVal xRatio As Single) As Single
        Return Math.Max(0.0!, xRatio)
    End Function

    Private Sub StoreVisibleSplitterRatios()
        For i As Integer = 1 To SplitPanes.Count - 1
            StoreSplitPanelRatio(i)
        Next
    End Sub

    Private Function GetSplitPanelRatiosSetting() As String
        StoreVisibleSplitterRatios()

        Dim xRatios As New List(Of String)
        For i As Integer = 1 To SplitPanes.Count - 1
            xRatios.Add(WriteDecimalWithDot(GetSplitPanelRatio(i)))
        Next

        Return String.Join(SplitPanelSettingSeparator.ToString(), xRatios.ToArray())
    End Function

    Private Sub LoadSplitPanelRatiosSetting(ByVal ratiosText As String)
        Do While RightSplitPaneCount() > 0
            RemoveRightSplitPane()
        Loop

        If ratiosText.Length = 0 Then
            RefreshSplitterControls()
            Return
        End If

        Dim xRatios() As String = ratiosText.Split(SplitPanelSettingSeparator)
        For Each xText As String In xRatios
            Dim xRatio As Single = DefaultSplitPanelRatio
            Try
                XMLLoadAttribute(xText.Trim(), xRatio)
            Catch ex As Exception
                xRatio = DefaultSplitPanelRatio
            End Try

            AddRightSplitPane(xRatio, False)
        Next

        RefreshSplitterControls()
    End Sub

    Private Sub ResizeSplitPanelsByRatio() Handles ToolStripContainer1.ContentPanel.Resize
        If UpdatingSplitterControls OrElse Not Me.Created Then Return
        If GetSplitLayoutWidth() <= 0 Then Return

        UpdatingSplitterControls = True
        Try
            For i As Integer = 1 To SplitPanes.Count - 1
                SplitPanes(i).Container.Width = GetSplitPanelWidth(i)
            Next
            RebuildPanelArrays()
            UpdateHorizontalScrollMetrics()
            RefreshSplitterControls()
        Finally
            UpdatingSplitterControls = False
        End Try
    End Sub

    Private Sub SetSplitterScrollSync(ByVal enabled As Boolean)
        UpdatingSplitterControls = True
        Try
            SyncSplitterScroll = enabled
            If mnSyncSplitterScroll IsNot Nothing Then mnSyncSplitterScroll.Checked = enabled
            If TBSyncSplitterScroll IsNot Nothing Then TBSyncSplitterScroll.Checked = enabled
        Finally
            UpdatingSplitterControls = False
        End Try

    End Sub
    Private Sub CGShow_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CGShow.CheckedChanged
        gShowGrid = CGShow.Checked
        RefreshPanelAll()
    End Sub
    Private Sub CGShowS_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CGShowS.CheckedChanged
        gShowSubGrid = CGShowS.Checked
        RefreshPanelAll()
    End Sub
    Private Sub CGShowBG_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CGShowBG.CheckedChanged
        gShowBG = CGShowBG.Checked
        RefreshPanelAll()
    End Sub
    Private Sub CGShowM_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CGShowM.CheckedChanged
        gShowMeasureNumber = CGShowM.Checked
        RefreshPanelAll()
    End Sub
    Private Sub CGShowV_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CGShowV.CheckedChanged
        gShowVerticalLine = CGShowV.Checked
        RefreshPanelAll()
    End Sub
    Private Sub CGShowMB_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CGShowMB.CheckedChanged
        gShowMeasureBar = CGShowMB.Checked
        RefreshPanelAll()
    End Sub
    Private Sub CGShowC_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CGShowC.CheckedChanged
        gShowC = CGShowC.Checked
        RefreshPanelAll()
    End Sub
    Private Sub CGBLP_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CGBLP.CheckedChanged
        gDisplayBGAColumn = CGBLP.Checked

        column(niBGA).isVisible = gDisplayBGAColumn
        column(niLAYER).isVisible = gDisplayBGAColumn
        column(niPOOR).isVisible = gDisplayBGAColumn
        column(niS4).isVisible = gDisplayBGAColumn

        If IsInitializing Then Exit Sub
        For xI1 As Integer = 1 To UBound(Notes)
            Notes(xI1).Selected = Notes(xI1).Selected And nEnabled(Notes(xI1).ColumnIndex)
        Next
        'AddUndo(xUndo, xRedo)
        CalculateGreatestColumn()
        RefreshPanelAll()
    End Sub
    Private Sub CGSCROLL_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CGSCROLL.CheckedChanged
        gSCROLL = CGSCROLL.Checked

        column(niSCROLL).isVisible = gSCROLL

        If IsInitializing Then Exit Sub
        For xI1 As Integer = 1 To UBound(Notes)
            Notes(xI1).Selected = Notes(xI1).Selected And nEnabled(Notes(xI1).ColumnIndex)
        Next
        'AddUndo(xUndo, xRedo)
        CalculateGreatestColumn()
        RefreshPanelAll()
    End Sub
    Private Sub CGSTOP_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CGSTOP.CheckedChanged
        gSTOP = CGSTOP.Checked

        column(niSTOP).isVisible = gSTOP

        If IsInitializing Then Exit Sub
        For xI1 As Integer = 1 To UBound(Notes)
            Notes(xI1).Selected = Notes(xI1).Selected And nEnabled(Notes(xI1).ColumnIndex)
        Next
        'AddUndo(xUndo, xRedo)
        CalculateGreatestColumn()
        RefreshPanelAll()
    End Sub
    Private Sub CGBPM_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CGBPM.CheckedChanged
        'Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        'Dim xRedo As UndoRedo.LinkedURCmd = Nothing
        'Me.RedoChangeVisibleColumns(gBLP, gSTOP, iPlayer, gBLP, CGSTOP.Checked, iPlayer, xUndo, xRedo)
        gBPM = CGBPM.Checked

        column(niBPM).isVisible = gBPM

        If IsInitializing Then Exit Sub
        For xI1 As Integer = 1 To UBound(Notes)
            Notes(xI1).Selected = Notes(xI1).Selected And nEnabled(Notes(xI1).ColumnIndex)
        Next
        'AddUndo(xUndo, xRedo)
        CalculateGreatestColumn()
        RefreshPanelAll()
    End Sub

    Private Sub CGDisableVertical_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CGDisableVertical.CheckedChanged
        DisableVerticalMove = CGDisableVertical.Checked
        RefreshDisableVerticalToolbar()
    End Sub

    Private Sub CBeatPreserve_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CBeatPreserve.Click, CBeatMeasure.Click, CBeatCut.Click, CBeatScale.Click
        'If Not sender.Checked Then Exit Sub
        Dim xBeatList() As RadioButton = {CBeatPreserve, CBeatMeasure, CBeatCut, CBeatScale}
        BeatChangeMode = Array.IndexOf(Of RadioButton)(xBeatList, sender)
        'For xI1 As Integer = 0 To mnBeat.Items.Count - 1
        'If xI1 <> BeatChangeMode Then CType(mnBeat.Items(xI1), ToolStripMenuItem).Checked = False
        'Next
        'sender.Checked = True
    End Sub


    Private Sub tBeatValue_LostFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles tBeatValue.LostFocus
        Dim a As Double
        If Double.TryParse(tBeatValue.Text, a) Then
            If a <= 0.0# Or a >= 1000.0# Then tBeatValue.BackColor = Color.FromArgb(&HFFFFC0C0) Else tBeatValue.BackColor = Nothing

            tBeatValue.Text = a
        End If
    End Sub



    Private Sub ApplyBeat(ByVal xRatio As Double, ByVal xDisplay As String)
        SortByVPositionInsertion()

        Dim xUndo As UndoRedo.LinkedURCmd = Nothing
        Dim xRedo As UndoRedo.LinkedURCmd = New UndoRedo.Void
        Dim xBaseRedo As UndoRedo.LinkedURCmd = xRedo

        Me.RedoChangeMeasureLengthSelected(192 * xRatio, xUndo, xRedo)

        Dim xIndices(LBeat.SelectedIndices.Count - 1) As Integer
        LBeat.SelectedIndices.CopyTo(xIndices, 0)


        For Each xI1 As Integer In xIndices
            Dim dLength As Double = xRatio * 192.0R - MeasureLength(xI1)
            Dim dRatio As Double = xRatio * 192.0R / MeasureLength(xI1)

            Dim xBottom As Double = 0
            For xI2 As Integer = 0 To xI1 - 1
                xBottom += MeasureLength(xI2)
            Next
            Dim xUpBefore As Double = xBottom + MeasureLength(xI1)
            Dim xUpAfter As Double = xUpBefore + dLength

            Select Case BeatChangeMode
                Case 1
case2:              Dim xI0 As Integer

                    If NTInput Then
                        For xI0 = 1 To UBound(Notes)
                            If Notes(xI0).VPosition >= xUpBefore Then Exit For
                            If Notes(xI0).VPosition + Notes(xI0).Length >= xUpBefore Then
                                Me.RedoLongNoteModify(Notes(xI0), Notes(xI0).VPosition, Notes(xI0).Length + dLength, xUndo, xRedo)
                                Notes(xI0).Length += dLength
                            End If
                        Next
                    Else
                        For xI0 = 1 To UBound(Notes)
                            If Notes(xI0).VPosition >= xUpBefore Then Exit For
                        Next
                    End If

                    For xI9 As Integer = xI0 To UBound(Notes)
                        Me.RedoLongNoteModify(Notes(xI9), Notes(xI9).VPosition + dLength, Notes(xI9).Length, xUndo, xRedo)
                        Notes(xI9).VPosition += dLength
                    Next

                Case 2
                    If dLength < 0 Then
                        If NTInput Then
                            Dim xI0 As Integer = 1
                            Dim xU As Integer = UBound(Notes)
                            Do While xI0 <= xU
                                If Notes(xI0).VPosition < xUpAfter Then
                                    If Notes(xI0).VPosition + Notes(xI0).Length >= xUpAfter And Notes(xI0).VPosition + Notes(xI0).Length < xUpBefore Then
                                        Dim nLen As Double = xUpAfter - Notes(xI0).VPosition - 1.0R
                                        Me.RedoLongNoteModify(Notes(xI0), Notes(xI0).VPosition, nLen, xUndo, xRedo)
                                        Notes(xI0).Length = nLen
                                    End If
                                ElseIf Notes(xI0).VPosition < xUpBefore Then
                                    If Notes(xI0).VPosition + Notes(xI0).Length < xUpBefore Then
                                        Me.RedoRemoveNote(Notes(xI0), xUndo, xRedo)
                                        RemoveNote(xI0)
                                        xI0 -= 1
                                        xU -= 1
                                    Else
                                        Dim nLen As Double = Notes(xI0).Length - xUpBefore + Notes(xI0).VPosition
                                        Me.RedoLongNoteModify(Notes(xI0), xUpBefore, nLen, xUndo, xRedo)
                                        Notes(xI0).Length = nLen
                                        Notes(xI0).VPosition = xUpBefore
                                    End If
                                End If
                                xI0 += 1
                            Loop
                        Else
                            Dim xI0 As Integer
                            Dim xI9 As Integer
                            For xI0 = 1 To UBound(Notes)
                                If Notes(xI0).VPosition >= xUpAfter Then Exit For
                            Next
                            For xI9 = xI0 To UBound(Notes)
                                If Notes(xI9).VPosition >= xUpBefore Then Exit For
                            Next

                            For xI8 As Integer = xI0 To xI9 - 1
                                Me.RedoRemoveNote(Notes(xI8), xUndo, xRedo)
                            Next
                            For xI8 As Integer = xI9 To UBound(Notes)
                                Notes(xI8 - xI9 + xI0) = Notes(xI8)
                            Next
                            ReDim Preserve Notes(UBound(Notes) - xI9 + xI0)
                        End If
                    End If

                    GoTo case2

                Case 3
                    If NTInput Then
                        For xI0 As Integer = 1 To UBound(Notes)
                            If Notes(xI0).VPosition < xBottom Then
                                If Notes(xI0).VPosition + Notes(xI0).Length > xUpBefore Then
                                    Me.RedoLongNoteModify(Notes(xI0), Notes(xI0).VPosition, Notes(xI0).Length + dLength, xUndo, xRedo)
                                    Notes(xI0).Length += dLength
                                ElseIf Notes(xI0).VPosition + Notes(xI0).Length > xBottom Then
                                    Dim nLen As Double = (Notes(xI0).Length + Notes(xI0).VPosition - xBottom) * dRatio + xBottom - Notes(xI0).VPosition
                                    Me.RedoLongNoteModify(Notes(xI0), Notes(xI0).VPosition, nLen, xUndo, xRedo)
                                    Notes(xI0).Length = nLen
                                End If
                            ElseIf Notes(xI0).VPosition < xUpBefore Then
                                If Notes(xI0).VPosition + Notes(xI0).Length > xUpBefore Then
                                    Dim nLen As Double = (xUpBefore - Notes(xI0).VPosition) * dRatio + Notes(xI0).VPosition + Notes(xI0).Length - xUpBefore
                                    Dim nVPos As Double = (Notes(xI0).VPosition - xBottom) * dRatio + xBottom
                                    Me.RedoLongNoteModify(Notes(xI0), nVPos, nLen, xUndo, xRedo)
                                    Notes(xI0).Length = nLen
                                    Notes(xI0).VPosition = nVPos
                                Else
                                    Dim nLen As Double = Notes(xI0).Length * dRatio
                                    Dim nVPos As Double = (Notes(xI0).VPosition - xBottom) * dRatio + xBottom
                                    Me.RedoLongNoteModify(Notes(xI0), nVPos, nLen, xUndo, xRedo)
                                    Notes(xI0).Length = nLen
                                    Notes(xI0).VPosition = nVPos
                                End If
                            Else
                                Me.RedoLongNoteModify(Notes(xI0), Notes(xI0).VPosition + dLength, Notes(xI0).Length, xUndo, xRedo)
                                Notes(xI0).VPosition += dLength
                            End If
                        Next
                    Else
                        Dim xI0 As Integer
                        Dim xI9 As Integer
                        For xI0 = 1 To UBound(Notes)
                            If Notes(xI0).VPosition >= xBottom Then Exit For
                        Next
                        For xI9 = xI0 To UBound(Notes)
                            If Notes(xI9).VPosition >= xUpBefore Then Exit For
                        Next

                        For xI8 As Integer = xI0 To xI9 - 1
                            Dim nVP As Double = (Notes(xI8).VPosition - xBottom) * dRatio + xBottom
                            Me.RedoLongNoteModify(Notes(xI0), nVP, Notes(xI0).Length, xUndo, xRedo)
                            Notes(xI8).VPosition = nVP
                        Next

                        'GoTo case2

                        For xI8 As Integer = xI9 To UBound(Notes)
                            Me.RedoLongNoteModify(Notes(xI8), Notes(xI8).VPosition + dLength, Notes(xI8).Length, xUndo, xRedo)
                            Notes(xI8).VPosition += dLength
                        Next
                    End If

            End Select

            MeasureLength(xI1) = xRatio * 192.0R
            LBeat.Items(xI1) = Add3Zeros(xI1) & ": " & xDisplay
        Next
        UpdateMeasureBottom()
        'xUndo &= vbCrLf & xUndo2
        'xRedo &= vbCrLf & xRedo2

        LBeat.SelectedIndices.Clear()
        For xI1 As Integer = 0 To UBound(xIndices)
            LBeat.SelectedIndices.Add(xIndices(xI1))
        Next

        AddUndo(xUndo, xBaseRedo.Next)
        SortByVPositionInsertion()
        UpdatePairing()
        CalculateTotalPlayableNotes()
        CalculateGreatestVPosition()
        RefreshPanelAll()
        POStatusRefresh()
    End Sub

    Private Sub BBeatApply_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BBeatApply.Click
        Dim xxD As Integer = nBeatD.Value
        Dim xxN As Integer = nBeatN.Value
        Dim xxRatio As Double = xxN / xxD

        ApplyBeat(xxRatio, xxRatio & " ( " & xxN & " / " & xxD & " ) ")
    End Sub

    Private Sub BBeatApplyV_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BBeatApplyV.Click
        Dim a As Double
        If Double.TryParse(tBeatValue.Text, a) Then
            If a <= 0.0# Or a >= 1000.0# Then System.Media.SystemSounds.Hand.Play() : Exit Sub

            Dim xxD As Long = GetDenominator(a)

            ApplyBeat(a, a & IIf(xxD > 10000, "", " ( " & CLng(a * xxD) & " / " & xxD & " ) "))
        End If
    End Sub


    Private Sub BHStageFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BHStageFile.Click, BHBanner.Click, BHBackBMP.Click, BHMissBMP.Click
        Dim xDiag As New OpenFileDialog
        xDiag.Filter = Strings.FileType._image & "|*.bmp;*.png;*.jpeg;*.jpg;*.gif|" &
                       Strings.FileType._all & "|*.*"
        xDiag.InitialDirectory = IIf(ExcludeFileName(FileName) = "", InitPath, ExcludeFileName(FileName))
        xDiag.DefaultExt = "png"

        If xDiag.ShowDialog = Windows.Forms.DialogResult.Cancel Then Exit Sub
        InitPath = ExcludeFileName(xDiag.FileName)

        If [Object].ReferenceEquals(sender, BHStageFile) Then
            THStageFile.Text = GetBMSRefPath(xDiag.FileName)
        ElseIf [Object].ReferenceEquals(sender, BHBanner) Then
            THBanner.Text = GetBMSRefPath(xDiag.FileName)
        ElseIf [Object].ReferenceEquals(sender, BHBackBMP) Then
            THBackBMP.Text = GetBMSRefPath(xDiag.FileName)
        ElseIf [Object].ReferenceEquals(sender, BHMissBMP) Then
            THMissBMP.Text = GetBMSRefPath(xDiag.FileName)
            hBMP(0) = THMissBMP.Text
        End If
    End Sub

    Private Sub BHWavFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BHLandMine.Click, BHPreview.Click
        Dim xDiag As New OpenFileDialog
        xDiag.Filter = Strings.FileType._wave & "|*.wav;*.ogg;*.mp3;*.flac|" &
                       Strings.FileType.WAV & "|*.wav|" &
                       Strings.FileType.OGG & "|*.ogg|" &
                       Strings.FileType.MP3 & "|*.mp3|" &
                       Strings.FileType.FLAC & "|*.flac|" &
                       Strings.FileType._all & "|*.*"
        xDiag.InitialDirectory = IIf(ExcludeFileName(FileName) = "", InitPath, ExcludeFileName(FileName))
        xDiag.DefaultExt = "wav"

        If xDiag.ShowDialog = Windows.Forms.DialogResult.Cancel Then Exit Sub

        If [Object].ReferenceEquals(sender, BHLandMine) Then
            InitPath = ExcludeFileName(xDiag.FileName)
            THLandMine.Text = GetBMSRefPath(xDiag.FileName)
            hWAV(0) = THLandMine.Text
        ElseIf [Object].ReferenceEquals(sender, BHPreview) Then
            InitPath = ExcludeFileName(xDiag.FileName)
            THPreview.Text = GetBMSRefPath(xDiag.FileName)
        End If
    End Sub

    Private Sub Switches_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles POGridSwitch.CheckedChanged

        Try
            Dim Source As CheckBox = CType(sender, CheckBox)
            Dim Target As Panel = Nothing

            If Object.ReferenceEquals(sender, Nothing) Then : Exit Sub
            ElseIf Object.ReferenceEquals(sender, POGridSwitch) Then : Target = POGridInner
            End If

            If Target Is Nothing Then Return

            If Source.Checked Then
                Target.Visible = True
            Else
                Target.Visible = False
            End If

        Catch ex As Exception

        End Try
    End Sub

    Private Sub Expanders_CheckChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles POGridExpander.CheckedChanged

        Try
            Dim Source As CheckBox = CType(sender, CheckBox)
            Dim Target As Panel = Nothing

            If Object.ReferenceEquals(sender, Nothing) Then : Exit Sub
            ElseIf Object.ReferenceEquals(sender, POGridExpander) Then : Target = POGridPart2
            End If

            If Target Is Nothing Then Return

            If Source.Checked Then
                Target.Visible = True
                'Source.Image = My.Resources.Collapse
            Else
                Target.Visible = False
                'Source.Image = My.Resources.Expand
            End If

        Catch ex As Exception

        End Try

    End Sub

    Private Sub HorizontalResizer_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles POptionsResizer.MouseDown, SpL.MouseDown, SpR.MouseDown
        tempResize = e.X
        BeginSplitterDrag(TryCast(sender, Control), e)
    End Sub

    Private Sub BeginSplitterDrag(ByVal xSplitter As Control, ByVal e As System.Windows.Forms.MouseEventArgs)
        SplitterDrag = Nothing

        If e.Button <> Windows.Forms.MouseButtons.Left Then Return
        If xSplitter Is Nothing OrElse Not TypeOf xSplitter Is TransparentSplitterGrip Then Return

        Dim panelIndex As Integer = CInt(xSplitter.Tag)
        If panelIndex <= MainPanelIndex OrElse panelIndex >= SplitPanes.Count Then Return

        Dim xPane As SplitPane = SplitPanes(panelIndex)
        Dim xDrag As New SplitterDragInfo With {
            .PanelIndex = panelIndex,
            .StartScreenX = xSplitter.PointToScreen(e.Location).X,
            .StartWidth = xPane.Container.Width
        }

        If panelIndex > MainPanelIndex + 1 Then
            Dim xLeftPane As SplitPane = SplitPanes(panelIndex - 1)
            xDrag.PairWidth = xLeftPane.Container.Width + xDrag.StartWidth
        End If

        SplitterDrag = xDrag
        xSplitter.Capture = True
    End Sub

    Private Sub FinishSplitterDrag(ByVal xSplitter As Control)
        Dim xWasDragging As Boolean = SplitterDrag IsNot Nothing
        SplitterDrag = Nothing

        If xSplitter IsNot Nothing AndAlso xSplitter.Capture Then
            xSplitter.Capture = False
        End If

        If xWasDragging Then RefreshSplitterControls()
    End Sub

    Private Sub POptionsResizer_MouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles POptionsResizer.MouseMove
        If e.Button <> Windows.Forms.MouseButtons.Left Then Exit Sub
        If e.X = tempResize Then Exit Sub

        Try
            Dim xWidth As Integer = POptionsScroll.Width - e.X + tempResize
            If xWidth < 230 Then xWidth = 230
            POptionsScroll.Width = xWidth

            Me.Refresh()
            Application.DoEvents()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub Splitter_MouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        Dim xSplitter As Control = DirectCast(sender, Control)
        If e.Button <> Windows.Forms.MouseButtons.Left Then
            FinishSplitterDrag(xSplitter)
            Exit Sub
        End If

        Try
            Dim panelIndex As Integer = CInt(xSplitter.Tag)
            If panelIndex <= MainPanelIndex OrElse panelIndex >= SplitPanes.Count Then Return
            If SplitterDrag Is Nothing OrElse SplitterDrag.PanelIndex <> panelIndex Then BeginSplitterDrag(xSplitter, e)
            If SplitterDrag Is Nothing Then Return

            Dim xPane As SplitPane = SplitPanes(panelIndex)
            Dim xDelta As Integer = xSplitter.PointToScreen(e.Location).X - SplitterDrag.StartScreenX
            Dim xWidth As Integer = SplitterDrag.StartWidth - xDelta
            Dim xChanged As Boolean = False

            ToolStripContainer1.ContentPanel.SuspendLayout()
            Try
                If panelIndex > MainPanelIndex + 1 Then
                    Dim xLeftPane As SplitPane = SplitPanes(panelIndex - 1)
                    Dim xPairWidth As Integer = SplitterDrag.PairWidth
                    Dim xMaxWidth As Integer = Math.Max(MinSplitPanelWidth, xPairWidth - MinSplitPanelWidth)
                    If xWidth < MinSplitPanelWidth Then xWidth = MinSplitPanelWidth
                    If xWidth > xMaxWidth Then xWidth = xMaxWidth

                    Dim xLeftWidth As Integer = xPairWidth - xWidth
                    If xPane.Container.Width <> xWidth OrElse xLeftPane.Container.Width <> xLeftWidth Then
                        xLeftPane.Container.Width = xLeftWidth
                        xPane.Container.Width = xWidth
                        xChanged = True
                    End If
                Else
                    If xWidth < MinSplitPanelWidth Then xWidth = MinSplitPanelWidth
                    If xWidth > GetMaxSplitPanelWidth(panelIndex) Then xWidth = GetMaxSplitPanelWidth(panelIndex)
                    If xPane.Container.Width <> xWidth Then
                        xPane.Container.Width = xWidth
                        xChanged = True
                    End If
                End If
            Finally
                ToolStripContainer1.ContentPanel.ResumeLayout(True)
            End Try

            If Not xChanged Then Return

            If panelIndex > MainPanelIndex + 1 Then StoreSplitPanelRatio(panelIndex - 1)
            StoreSplitPanelRatio(panelIndex)
            RebuildPanelArrays()
            UpdateHorizontalScrollMetrics()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub Splitter_MouseUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        If e.Button <> Windows.Forms.MouseButtons.Left Then Return

        FinishSplitterDrag(TryCast(sender, Control))
    End Sub

    Private Sub Splitter_MouseCaptureChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim xSplitter As Control = TryCast(sender, Control)
        If xSplitter IsNot Nothing AndAlso Not xSplitter.Capture Then FinishSplitterDrag(xSplitter)
    End Sub

    Private Sub SpR_MouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles SpR.MouseMove
    End Sub

    Private Sub SpL_MouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles SpL.MouseMove
    End Sub

    Private Sub mnGotoMeasure_Click(sender As Object, e As EventArgs) Handles mnGotoMeasure.Click
        Dim s = InputBox(Strings.Messages.PromptEnterMeasure, Strings.Messages.GoToMeasureTitle)

        Dim i As Integer
        If Int32.TryParse(s, i) Then
            If i < 0 Or i > 999 Then
                Exit Sub
            End If

            SetPanelVScroll(PanelFocus, -MeasureBottom(i))
        End If
    End Sub
End Class
