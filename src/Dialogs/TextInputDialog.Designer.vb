<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class TextInputDialog
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
        Me.PromptLabel = New System.Windows.Forms.Label()
        Me.InputTextBox = New System.Windows.Forms.TextBox()
        Me.CancelInputButton = New System.Windows.Forms.Button()
        Me.OKInputButton = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'PromptLabel
        '
        Me.PromptLabel.AutoEllipsis = True
        Me.PromptLabel.Location = New System.Drawing.Point(10, 10)
        Me.PromptLabel.Name = "PromptLabel"
        Me.PromptLabel.Size = New System.Drawing.Size(250, 70)
        Me.PromptLabel.TabIndex = 1
        Me.PromptLabel.Text = "Prompt"
        '
        'InputTextBox
        '
        Me.InputTextBox.Location = New System.Drawing.Point(10, 90)
        Me.InputTextBox.Name = "InputTextBox"
        Me.InputTextBox.Size = New System.Drawing.Size(335, 20)
        Me.InputTextBox.TabIndex = 0
        '
        'CancelInputButton
        '
        Me.CancelInputButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.CancelInputButton.Location = New System.Drawing.Point(290, 40)
        Me.CancelInputButton.Name = "CancelInputButton"
        Me.CancelInputButton.Size = New System.Drawing.Size(60, 22)
        Me.CancelInputButton.TabIndex = 3
        Me.CancelInputButton.Text = "Cancel"
        Me.CancelInputButton.UseVisualStyleBackColor = True
        '
        'OKInputButton
        '
        Me.OKInputButton.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.OKInputButton.Location = New System.Drawing.Point(290, 12)
        Me.OKInputButton.Name = "OKInputButton"
        Me.OKInputButton.Size = New System.Drawing.Size(60, 22)
        Me.OKInputButton.TabIndex = 2
        Me.OKInputButton.Text = "OK"
        Me.OKInputButton.UseVisualStyleBackColor = True
        '
        'TextInputDialog
        '
        Me.AcceptButton = Me.OKInputButton
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.CancelButton = Me.CancelInputButton
        Me.ClientSize = New System.Drawing.Size(353, 120)
        Me.Controls.Add(Me.InputTextBox)
        Me.Controls.Add(Me.PromptLabel)
        Me.Controls.Add(Me.OKInputButton)
        Me.Controls.Add(Me.CancelInputButton)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "TextInputDialog"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Input"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents PromptLabel As System.Windows.Forms.Label
    Friend WithEvents InputTextBox As System.Windows.Forms.TextBox
    Friend WithEvents OKInputButton As System.Windows.Forms.Button
    Friend WithEvents CancelInputButton As System.Windows.Forms.Button
End Class
