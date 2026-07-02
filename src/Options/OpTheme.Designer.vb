<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class OpTheme
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.RootLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.AutoSelectCheckBox = New System.Windows.Forms.CheckBox()
        Me.HeaderLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.ModeHeaderLabel = New System.Windows.Forms.Label()
        Me.ThemeHeaderLabel = New System.Windows.Forms.Label()
        Me.RowsScrollPanel = New System.Windows.Forms.Panel()
        Me.RowsPanel = New System.Windows.Forms.TableLayoutPanel()
        Me.ButtonPanel = New System.Windows.Forms.FlowLayoutPanel()
        Me.Cancel_Button = New System.Windows.Forms.Button()
        Me.OK_Button = New System.Windows.Forms.Button()
        Me.AddButton = New System.Windows.Forms.Button()
        Me.IconToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.RootLayout.SuspendLayout()
        Me.HeaderLayout.SuspendLayout()
        Me.RowsScrollPanel.SuspendLayout()
        Me.ButtonPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'RootLayout
        '
        Me.RootLayout.ColumnCount = 1
        Me.RootLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.RootLayout.Controls.Add(Me.AutoSelectCheckBox, 0, 0)
        Me.RootLayout.Controls.Add(Me.HeaderLayout, 0, 1)
        Me.RootLayout.Controls.Add(Me.RowsScrollPanel, 0, 2)
        Me.RootLayout.Controls.Add(Me.ButtonPanel, 0, 3)
        Me.RootLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.RootLayout.Location = New System.Drawing.Point(0, 0)
        Me.RootLayout.Name = "RootLayout"
        Me.RootLayout.Padding = New System.Windows.Forms.Padding(12)
        Me.RootLayout.RowCount = 4
        Me.RootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30.0!))
        Me.RootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24.0!))
        Me.RootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.RootLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48.0!))
        Me.RootLayout.Size = New System.Drawing.Size(520, 300)
        Me.RootLayout.TabIndex = 0
        '
        'AutoSelectCheckBox
        '
        Me.AutoSelectCheckBox.AutoSize = True
        Me.AutoSelectCheckBox.Location = New System.Drawing.Point(15, 15)
        Me.AutoSelectCheckBox.Name = "AutoSelectCheckBox"
        Me.AutoSelectCheckBox.Size = New System.Drawing.Size(121, 19)
        Me.AutoSelectCheckBox.TabIndex = 0
        Me.AutoSelectCheckBox.Text = "Auto select theme"
        Me.AutoSelectCheckBox.UseVisualStyleBackColor = True
        '
        'HeaderLayout
        '
        Me.HeaderLayout.ColumnCount = 3
        Me.HeaderLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120.0!))
        Me.HeaderLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.HeaderLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32.0!))
        Me.HeaderLayout.Controls.Add(Me.ModeHeaderLabel, 0, 0)
        Me.HeaderLayout.Controls.Add(Me.ThemeHeaderLabel, 1, 0)
        Me.HeaderLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.HeaderLayout.Location = New System.Drawing.Point(12, 42)
        Me.HeaderLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.HeaderLayout.Name = "HeaderLayout"
        Me.HeaderLayout.RowCount = 1
        Me.HeaderLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.HeaderLayout.Size = New System.Drawing.Size(496, 24)
        Me.HeaderLayout.TabIndex = 1
        '
        'ModeHeaderLabel
        '
        Me.ModeHeaderLabel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ModeHeaderLabel.Location = New System.Drawing.Point(3, 0)
        Me.ModeHeaderLabel.Name = "ModeHeaderLabel"
        Me.ModeHeaderLabel.Size = New System.Drawing.Size(114, 24)
        Me.ModeHeaderLabel.TabIndex = 0
        Me.ModeHeaderLabel.Text = "Mode"
        Me.ModeHeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'ThemeHeaderLabel
        '
        Me.ThemeHeaderLabel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ThemeHeaderLabel.Location = New System.Drawing.Point(124, 0)
        Me.ThemeHeaderLabel.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.ThemeHeaderLabel.Name = "ThemeHeaderLabel"
        Me.ThemeHeaderLabel.Size = New System.Drawing.Size(336, 24)
        Me.ThemeHeaderLabel.TabIndex = 1
        Me.ThemeHeaderLabel.Text = "Theme"
        Me.ThemeHeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'RowsScrollPanel
        '
        Me.RowsScrollPanel.AutoScroll = True
        Me.RowsScrollPanel.Controls.Add(Me.RowsPanel)
        Me.RowsScrollPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.RowsScrollPanel.Location = New System.Drawing.Point(12, 66)
        Me.RowsScrollPanel.Margin = New System.Windows.Forms.Padding(0)
        Me.RowsScrollPanel.Name = "RowsScrollPanel"
        Me.RowsScrollPanel.Size = New System.Drawing.Size(496, 174)
        Me.RowsScrollPanel.TabIndex = 2
        '
        'RowsPanel
        '
        Me.RowsPanel.AutoSize = True
        Me.RowsPanel.ColumnCount = 3
        Me.RowsPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120.0!))
        Me.RowsPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.RowsPanel.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32.0!))
        Me.RowsPanel.Dock = System.Windows.Forms.DockStyle.Top
        Me.RowsPanel.Location = New System.Drawing.Point(0, 0)
        Me.RowsPanel.Margin = New System.Windows.Forms.Padding(0)
        Me.RowsPanel.Name = "RowsPanel"
        Me.RowsPanel.RowCount = 0
        Me.RowsPanel.Size = New System.Drawing.Size(496, 0)
        Me.RowsPanel.TabIndex = 0
        '
        'ButtonPanel
        '
        Me.ButtonPanel.Controls.Add(Me.Cancel_Button)
        Me.ButtonPanel.Controls.Add(Me.OK_Button)
        Me.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ButtonPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft
        Me.ButtonPanel.Location = New System.Drawing.Point(12, 240)
        Me.ButtonPanel.Margin = New System.Windows.Forms.Padding(0)
        Me.ButtonPanel.Name = "ButtonPanel"
        Me.ButtonPanel.Padding = New System.Windows.Forms.Padding(0, 8, 0, 0)
        Me.ButtonPanel.Size = New System.Drawing.Size(496, 48)
        Me.ButtonPanel.TabIndex = 3
        Me.ButtonPanel.WrapContents = False
        '
        'Cancel_Button
        '
        Me.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Cancel_Button.Location = New System.Drawing.Point(415, 8)
        Me.Cancel_Button.Margin = New System.Windows.Forms.Padding(3, 0, 3, 0)
        Me.Cancel_Button.Name = "Cancel_Button"
        Me.Cancel_Button.Size = New System.Drawing.Size(78, 27)
        Me.Cancel_Button.TabIndex = 1
        Me.Cancel_Button.Text = "Cancel"
        Me.Cancel_Button.UseVisualStyleBackColor = True
        '
        'OK_Button
        '
        Me.OK_Button.Location = New System.Drawing.Point(331, 8)
        Me.OK_Button.Margin = New System.Windows.Forms.Padding(3, 0, 3, 0)
        Me.OK_Button.Name = "OK_Button"
        Me.OK_Button.Size = New System.Drawing.Size(78, 27)
        Me.OK_Button.TabIndex = 0
        Me.OK_Button.Text = "OK"
        Me.OK_Button.UseVisualStyleBackColor = True
        '
        'AddButton
        '
        Me.AddButton.Image = Global.nBMSC.My.Resources.Resources.x16Add
        Me.AddButton.Location = New System.Drawing.Point(0, 0)
        Me.AddButton.Margin = New System.Windows.Forms.Padding(2, 4, 0, 0)
        Me.AddButton.Name = "AddButton"
        Me.AddButton.Size = New System.Drawing.Size(27, 27)
        Me.AddButton.TabIndex = 0
        Me.AddButton.UseVisualStyleBackColor = True
        '
        'OpTheme
        '
        Me.AcceptButton = Me.OK_Button
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.CancelButton = Me.Cancel_Button
        Me.ClientSize = New System.Drawing.Size(520, 300)
        Me.Controls.Add(Me.RootLayout)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "OpTheme"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Theme Options"
        Me.RootLayout.ResumeLayout(False)
        Me.RootLayout.PerformLayout()
        Me.HeaderLayout.ResumeLayout(False)
        Me.RowsScrollPanel.ResumeLayout(False)
        Me.RowsScrollPanel.PerformLayout()
        Me.ButtonPanel.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents RootLayout As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents AutoSelectCheckBox As System.Windows.Forms.CheckBox
    Friend WithEvents HeaderLayout As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents ModeHeaderLabel As System.Windows.Forms.Label
    Friend WithEvents ThemeHeaderLabel As System.Windows.Forms.Label
    Friend WithEvents RowsScrollPanel As System.Windows.Forms.Panel
    Friend WithEvents RowsPanel As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents ButtonPanel As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents Cancel_Button As System.Windows.Forms.Button
    Friend WithEvents OK_Button As System.Windows.Forms.Button
    Friend WithEvents AddButton As System.Windows.Forms.Button
    Friend WithEvents IconToolTip As System.Windows.Forms.ToolTip
End Class
