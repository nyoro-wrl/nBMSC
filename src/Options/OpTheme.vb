Option Strict On

Imports System.Collections.Generic
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports nBMSC.Editor

Public Class OpTheme
    Inherits Form

    Private Class ComboMouseWheelBlocker
        Inherits NativeWindow

        Private Const WM_MOUSEWHEEL As Integer = &H20A
        Private ReadOnly Target As Control

        Public Sub New(ByVal targetControl As Control)
            Target = targetControl
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
            If xScrollable Is Nothing OrElse xDelta = 0 Then Return

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

    Private Class ThemeChoice
        Public ReadOnly Text As String
        Public ReadOnly Path As String

        Public Sub New(ByVal text As String, ByVal path As String)
            Me.Text = text
            Me.Path = path
        End Sub

        Public Overrides Function ToString() As String
            Return Text
        End Function
    End Class

    Private Class ModeThemeRow
        Public ReadOnly IsBuiltIn As Boolean
        Public ReadOnly ModeLabel As Label
        Public ReadOnly ModeText As TextBox
        Public ReadOnly ThemeCombo As ComboBox
        Public ReadOnly DeleteButton As Button

        Public Sub New(ByVal modeName As String, ByVal isBuiltIn As Boolean)
            Me.IsBuiltIn = isBuiltIn

            ModeLabel = New Label()
            ModeLabel.Anchor = AnchorStyles.Right
            ModeLabel.AutoSize = True
            ModeLabel.Text = modeName
            ModeLabel.TextAlign = ContentAlignment.MiddleRight

            ModeText = New TextBox()
            ModeText.Dock = DockStyle.Fill
            ModeText.Text = modeName

            ThemeCombo = New ComboBox()
            ThemeCombo.Dock = DockStyle.Fill
            ThemeCombo.DropDownStyle = ComboBoxStyle.DropDownList
            ThemeCombo.FlatStyle = FlatStyle.System
            ThemeCombo.Margin = New Padding(4, 1, 4, 0)

            DeleteButton = New Button()
            DeleteButton.Image = My.Resources.Resources.x16Remove
            DeleteButton.Size = New Size(27, 27)
            DeleteButton.Margin = New Padding(2, 1, 0, 0)
            DeleteButton.Enabled = Not isBuiltIn
            DeleteButton.Visible = Not isBuiltIn
        End Sub

        Public Function ModeName() As String
            If IsBuiltIn Then Return ModeLabel.Text.Trim()
            Return ModeText.Text.Trim()
        End Function
    End Class

    Private ReadOnly BuiltInRows As New List(Of ModeThemeRow)
    Private ReadOnly CustomRows As New List(Of ModeThemeRow)
    Private ReadOnly ThemeComboWheelBlockers As New List(Of ComboMouseWheelBlocker)
    Private ReadOnly ThemeNames() As String
    Private ReadOnly ThemePaths() As String
    Private SelectedThemePaths() As String
    Private SelectedCustomModeThemes() As ThemeModeSetting

    Public Sub New(ByVal xThemeNames() As String,
                   ByVal xThemePaths() As String,
                   ByVal xDefaultThemePaths() As String,
                   ByVal xCustomModeThemes() As ThemeModeSetting,
                   ByVal xAutoSelect As Boolean)
        ThemeNames = xThemeNames
        ThemePaths = xThemePaths
        SelectedThemePaths = New String(ChartModes.Count() - 1) {}
        SelectedCustomModeThemes = New ThemeModeSetting() {}

        InitializeComponent()
        InitializeLayout(xDefaultThemePaths, xCustomModeThemes, xAutoSelect)
    End Sub

    Public Function ThemePath(ByVal mode As ChartMode) As String
        Return SelectedThemePaths(ChartModes.IndexOf(mode))
    End Function

    Public Function CustomModeThemes() As ThemeModeSetting()
        Return SelectedCustomModeThemes
    End Function

    Public Function AutoSelectTheme() As Boolean
        Return AutoSelectCheckBox.Checked
    End Function

    Private Sub InitializeLayout(ByVal xDefaultThemePaths() As String,
                                 ByVal xCustomModeThemes() As ThemeModeSetting,
                                 ByVal xAutoSelect As Boolean)
        Text = Strings.Get("ThemeOptions.Title")
        Font = MainWindow.Font
        AutoSelectCheckBox.Checked = xAutoSelect
        AutoSelectCheckBox.Text = Strings.Get("ThemeOptions.AutoSelect")
        ModeHeaderLabel.Text = Strings.Get("ThemeOptions.Mode")
        ThemeHeaderLabel.Text = Strings.Get("ThemeOptions.Theme")
        OK_Button.Text = Strings.OK
        Cancel_Button.Text = Strings.Cancel

        For i As Integer = 0 To ChartModes.Count() - 1
            Dim xMode As ChartMode = ChartModes.FromIndex(i)
            Dim xRow As ModeThemeRow = CreateRow(ChartModes.DisplayName(xMode), If(i < xDefaultThemePaths.Length, xDefaultThemePaths(i), ""), True)
            BuiltInRows.Add(xRow)
        Next

        If xCustomModeThemes IsNot Nothing Then
            For Each xMode As ThemeModeSetting In xCustomModeThemes
                If xMode Is Nothing Then Continue For
                CustomRows.Add(CreateRow(xMode.ModeName, xMode.ThemePath, False))
            Next
        End If

        IconToolTip.SetToolTip(AddButton, Strings.Get("ThemeOptions.Add"))
        RebuildRows()
    End Sub

    Private Function CreateRow(ByVal modeName As String, ByVal themePath As String, ByVal isBuiltIn As Boolean) As ModeThemeRow
        Dim xRow As New ModeThemeRow(modeName, isBuiltIn)
        FillThemeCombo(xRow.ThemeCombo, themePath)
        ThemeComboWheelBlockers.Add(New ComboMouseWheelBlocker(xRow.ThemeCombo))
        IconToolTip.SetToolTip(xRow.DeleteButton, Strings.Get("ThemeOptions.Delete"))
        AddHandler xRow.DeleteButton.Click,
            Sub(sender As Object, e As EventArgs)
                CustomRows.Remove(xRow)
                RebuildRows()
            End Sub

        Return xRow
    End Function

    Private Sub RebuildRows()
        RowsPanel.SuspendLayout()
        RowsPanel.Controls.Clear()
        RowsPanel.RowStyles.Clear()
        RowsPanel.RowCount = 0

        For Each xRow As ModeThemeRow In BuiltInRows
            AddRowControls(xRow)
        Next
        For Each xRow As ModeThemeRow In CustomRows
            AddRowControls(xRow)
        Next
        AddAppendRowControls()

        RowsPanel.ResumeLayout()
    End Sub

    Private Sub AddRowControls(ByVal row As ModeThemeRow)
        Dim xRowIndex As Integer = RowsPanel.RowCount
        RowsPanel.RowCount += 1
        RowsPanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 30.0F))

        If row.IsBuiltIn Then
            RowsPanel.Controls.Add(row.ModeLabel, 0, xRowIndex)
        Else
            RowsPanel.Controls.Add(row.ModeText, 0, xRowIndex)
        End If
        RowsPanel.Controls.Add(row.ThemeCombo, 1, xRowIndex)
        RowsPanel.Controls.Add(row.DeleteButton, 2, xRowIndex)
    End Sub

    Private Sub AddAppendRowControls()
        Dim xRowIndex As Integer = RowsPanel.RowCount
        RowsPanel.RowCount += 1
        RowsPanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 34.0F))

        Dim xPanel As New FlowLayoutPanel()
        xPanel.Dock = DockStyle.Fill
        xPanel.Margin = New Padding(0)
        xPanel.Padding = New Padding(0, 4, 0, 0)
        xPanel.FlowDirection = FlowDirection.LeftToRight
        xPanel.Controls.Add(AddButton)

        RowsPanel.Controls.Add(xPanel, 2, xRowIndex)
    End Sub

    Private Sub AddButton_Click(ByVal sender As Object, ByVal e As EventArgs) Handles AddButton.Click
        Dim xRow As ModeThemeRow = CreateRow("", "", False)
        CustomRows.Add(xRow)
        RebuildRows()
        xRow.ModeText.Focus()
    End Sub

    Private Sub FillThemeCombo(ByVal combo As ComboBox, ByVal defaultPath As String)
        Dim xDefaultFullPath As String = SafeFullPath(defaultPath)
        Dim xSelected As Integer = -1

        For i As Integer = 0 To ThemePaths.Length - 1
            Dim xChoice As New ThemeChoice(ThemeNames(i), ThemePaths(i))
            combo.Items.Add(xChoice)
            If xSelected = -1 AndAlso String.Equals(SafeFullPath(ThemePaths(i)), xDefaultFullPath, StringComparison.OrdinalIgnoreCase) Then
                xSelected = combo.Items.Count - 1
            End If
        Next

        If xSelected = -1 AndAlso defaultPath <> "" Then
            combo.Items.Add(New ThemeChoice(Path.GetFileNameWithoutExtension(defaultPath), defaultPath))
            xSelected = combo.Items.Count - 1
        End If

        If xSelected = -1 AndAlso combo.Items.Count > 0 Then xSelected = 0
        If xSelected >= 0 Then combo.SelectedIndex = xSelected
    End Sub

    Private Function SelectedThemePath(ByVal row As ModeThemeRow) As String
        Dim xChoice As ThemeChoice = TryCast(row.ThemeCombo.SelectedItem, ThemeChoice)
        If xChoice Is Nothing Then Return ""

        Return xChoice.Path
    End Function

    Private Function SafeFullPath(ByVal filePath As String) As String
        If filePath Is Nothing OrElse filePath = "" Then Return ""

        Try
            Return Path.GetFullPath(filePath)
        Catch ex As Exception
            Return filePath
        End Try
    End Function

    Private Function IsBuiltInModeName(ByVal modeName As String) As Boolean
        For i As Integer = 0 To ChartModes.Count() - 1
            If String.Equals(modeName.Trim(), ChartModes.DisplayName(ChartModes.FromIndex(i)), StringComparison.OrdinalIgnoreCase) Then Return True
        Next

        Return False
    End Function

    Private Sub OK_Button_Click(ByVal sender As Object, ByVal e As EventArgs) Handles OK_Button.Click
        Dim xUsedModes As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        SelectedThemePaths = New String(ChartModes.Count() - 1) {}

        For i As Integer = 0 To BuiltInRows.Count - 1
            xUsedModes.Add(BuiltInRows(i).ModeName())
            SelectedThemePaths(i) = SelectedThemePath(BuiltInRows(i))
        Next

        Dim xCustomModeThemes As New List(Of ThemeModeSetting)
        For Each xRow As ModeThemeRow In CustomRows
            Dim xModeName As String = xRow.ModeName()
            Dim xThemePath As String = SelectedThemePath(xRow)

            If xModeName = "" Then
                MessageBox.Show(Strings.Get("ThemeOptions.ModeRequired"), Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                xRow.ModeText.Focus()
                Return
            End If

            If IsBuiltInModeName(xModeName) OrElse xUsedModes.Contains(xModeName) Then
                MessageBox.Show(Strings.Get("ThemeOptions.ModeDuplicated"), Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                xRow.ModeText.Focus()
                Return
            End If

            If xThemePath = "" Then
                MessageBox.Show(Strings.Get("ThemeOptions.ThemeRequired"), Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                xRow.ThemeCombo.Focus()
                Return
            End If

            xUsedModes.Add(xModeName)
            xCustomModeThemes.Add(New ThemeModeSetting(xModeName, xThemePath))
        Next

        SelectedCustomModeThemes = xCustomModeThemes.ToArray()
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Protected Overrides Sub OnFormClosed(ByVal e As FormClosedEventArgs)
        For Each xBlocker As ComboMouseWheelBlocker In ThemeComboWheelBlockers
            xBlocker.Release()
        Next
        ThemeComboWheelBlockers.Clear()
        If IconToolTip IsNot Nothing Then IconToolTip.Dispose()

        MyBase.OnFormClosed(e)
    End Sub
End Class
