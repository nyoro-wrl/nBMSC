<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class diagFind
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(diagFind))
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.mr1 = New System.Windows.Forms.NumericUpDown
        Me.mr2 = New System.Windows.Forms.NumericUpDown
        Me.lr1 = New System.Windows.Forms.TextBox
        Me.lr2 = New System.Windows.Forms.TextBox
        Me.vr2 = New System.Windows.Forms.NumericUpDown
        Me.vr1 = New System.Windows.Forms.NumericUpDown
        Me.cb1 = New System.Windows.Forms.CheckBox
        Me.cb2 = New System.Windows.Forms.CheckBox
        Me.cb3 = New System.Windows.Forms.CheckBox
        Me.cb4 = New System.Windows.Forms.CheckBox
        Me.cb5 = New System.Windows.Forms.CheckBox
        Me.cb6 = New System.Windows.Forms.CheckBox
        Me.cbb1 = New System.Windows.Forms.CheckBox
        Me.Panel1 = New System.Windows.Forms.Panel
        Me.BSAll = New System.Windows.Forms.Button
        Me.BSInv = New System.Windows.Forms.Button
        Me.BSNone = New System.Windows.Forms.Button
        Me.Label3 = New System.Windows.Forms.Label
        Me.Label4 = New System.Windows.Forms.Label
        Me.TBSelect = New System.Windows.Forms.Button
        Me.TBClose = New System.Windows.Forms.Button
        Me.TBDelete = New System.Windows.Forms.Button
        Me.TBrl = New System.Windows.Forms.Button
        Me.TBrv = New System.Windows.Forms.Button
        Me.Label5 = New System.Windows.Forms.Label
        Me.Label6 = New System.Windows.Forms.Label
        Me.Label7 = New System.Windows.Forms.Label
        Me.Ttv = New System.Windows.Forms.NumericUpDown
        Me.Ttl = New System.Windows.Forms.TextBox
        Me.Label8 = New System.Windows.Forms.Label
        Me.Label9 = New System.Windows.Forms.Label
        Me.PictureBox3 = New System.Windows.Forms.PictureBox
        Me.PictureBox2 = New System.Windows.Forms.PictureBox
        Me.PictureBox1 = New System.Windows.Forms.PictureBox
        Me.cbx1 = New System.Windows.Forms.CheckBox
        Me.cbx2 = New System.Windows.Forms.CheckBox
        Me.cbx3 = New System.Windows.Forms.CheckBox
        Me.TBUnselect = New System.Windows.Forms.Button
        Me.cbx4 = New System.Windows.Forms.CheckBox
        Me.cbx5 = New System.Windows.Forms.CheckBox
        Me.cbx6 = New System.Windows.Forms.CheckBox
        CType(Me.mr1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.mr2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.vr2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.vr1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.Panel1.SuspendLayout()
        CType(Me.Ttv, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(26, 20)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(120, 17)
        Me.Label1.TabIndex = 2
        Me.Label1.Text = "Note Range"
        Me.Label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(26, 73)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(120, 17)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Measure Range"
        Me.Label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'mr1
        '
        Me.mr1.Location = New System.Drawing.Point(152, 72)
        Me.mr1.Maximum = New Decimal(New Integer() {999, 0, 0, 0})
        Me.mr1.Name = "mr1"
        Me.mr1.Size = New System.Drawing.Size(70, 23)
        Me.mr1.TabIndex = 5
        '
        'mr2
        '
        Me.mr2.Location = New System.Drawing.Point(258, 72)
        Me.mr2.Maximum = New Decimal(New Integer() {999, 0, 0, 0})
        Me.mr2.Name = "mr2"
        Me.mr2.Size = New System.Drawing.Size(70, 23)
        Me.mr2.TabIndex = 6
        Me.mr2.Value = New Decimal(New Integer() {999, 0, 0, 0})
        '
        'lr1
        '
        Me.lr1.Location = New System.Drawing.Point(152, 101)
        Me.lr1.MaxLength = 2
        Me.lr1.Name = "lr1"
        Me.lr1.Size = New System.Drawing.Size(70, 23)
        Me.lr1.TabIndex = 7
        Me.lr1.Text = "01"
        '
        'lr2
        '
        Me.lr2.Location = New System.Drawing.Point(258, 101)
        Me.lr2.MaxLength = 2
        Me.lr2.Name = "lr2"
        Me.lr2.Size = New System.Drawing.Size(70, 23)
        Me.lr2.TabIndex = 8
        Me.lr2.Text = "ZZ"
        '
        'vr2
        '
        Me.vr2.DecimalPlaces = 4
        Me.vr2.Location = New System.Drawing.Point(258, 130)
        Me.vr2.Maximum = New Decimal(New Integer() {655359999, 0, 0, 262144})
        Me.vr2.Minimum = New Decimal(New Integer() {1, 0, 0, 262144})
        Me.vr2.Name = "vr2"
        Me.vr2.Size = New System.Drawing.Size(100, 23)
        Me.vr2.TabIndex = 10
        Me.vr2.Value = New Decimal(New Integer() {655359999, 0, 0, 262144})
        '
        'vr1
        '
        Me.vr1.DecimalPlaces = 4
        Me.vr1.Location = New System.Drawing.Point(152, 130)
        Me.vr1.Maximum = New Decimal(New Integer() {655359999, 0, 0, 262144})
        Me.vr1.Minimum = New Decimal(New Integer() {1, 0, 0, 262144})
        Me.vr1.Name = "vr1"
        Me.vr1.Size = New System.Drawing.Size(70, 23)
        Me.vr1.TabIndex = 9
        Me.vr1.Value = New Decimal(New Integer() {1, 0, 0, 262144})
        '
        'cb1
        '
        Me.cb1.Appearance = System.Windows.Forms.Appearance.Button
        Me.cb1.Checked = True
        Me.cb1.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cb1.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cb1.Location = New System.Drawing.Point(3, 2)
        Me.cb1.Name = "cb1"
        Me.cb1.Size = New System.Drawing.Size(60, 25)
        Me.cb1.TabIndex = 0
        Me.cb1.Tag = "2"
        Me.cb1.Text = "BPM"
        Me.cb1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.cb1.UseVisualStyleBackColor = False
        '
        'cb2
        '
        Me.cb2.Appearance = System.Windows.Forms.Appearance.Button
        Me.cb2.Checked = True
        Me.cb2.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cb2.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cb2.Location = New System.Drawing.Point(63, 2)
        Me.cb2.Name = "cb2"
        Me.cb2.Size = New System.Drawing.Size(60, 25)
        Me.cb2.TabIndex = 1
        Me.cb2.Tag = "3"
        Me.cb2.Text = "STOP"
        Me.cb2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.cb2.UseVisualStyleBackColor = True
        '
        'cb3
        '
        Me.cb3.Appearance = System.Windows.Forms.Appearance.Button
        Me.cb3.Checked = True
        Me.cb3.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cb3.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cb3.Location = New System.Drawing.Point(123, 2)
        Me.cb3.Name = "cb3"
        Me.cb3.Size = New System.Drawing.Size(60, 25)
        Me.cb3.TabIndex = 2
        Me.cb3.Tag = "1"
        Me.cb3.Text = "SCROLL"
        Me.cb3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.cb3.UseVisualStyleBackColor = True
        '
        'cb4
        '
        Me.cb4.Appearance = System.Windows.Forms.Appearance.Button
        Me.cb4.Checked = True
        Me.cb4.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cb4.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cb4.Location = New System.Drawing.Point(3, 25 * 3 + 2)
        Me.cb4.Name = "cb4"
        Me.cb4.Size = New System.Drawing.Size(55, 25)
        Me.cb4.TabIndex = 19
        Me.cb4.Tag = "59"
        Me.cb4.Text = "BGA"
        Me.cb4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.cb4.UseVisualStyleBackColor = True
        '
        'cb4
        '
        Me.cb5.Appearance = System.Windows.Forms.Appearance.Button
        Me.cb5.Checked = True
        Me.cb5.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cb5.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cb5.Location = New System.Drawing.Point(58, 25 * 3 + 2)
        Me.cb5.Name = "cb5"
        Me.cb5.Size = New System.Drawing.Size(55, 25)
        Me.cb5.TabIndex = 20
        Me.cb5.Tag = "60"
        Me.cb5.Text = "LAYER"
        Me.cb5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.cb5.UseVisualStyleBackColor = True
        '
        'cb6
        '
        Me.cb6.Appearance = System.Windows.Forms.Appearance.Button
        Me.cb6.Checked = True
        Me.cb6.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cb6.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cb6.Location = New System.Drawing.Point(113, 25 * 3 + 2)
        Me.cb6.Name = "cb6"
        Me.cb6.Size = New System.Drawing.Size(55, 25)
        Me.cb6.TabIndex = 21
        Me.cb6.Tag = "61"
        Me.cb6.Text = "POOR"
        Me.cb6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.cb6.UseVisualStyleBackColor = True
        '
        'Panel1
        '
        Me.Panel1.AutoScroll = True
        Me.Panel1.Location = New System.Drawing.Point(26, 186)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(302, 259)
        Me.Panel1.TabIndex = 22
        '
        'BSAll
        '
        Me.BSAll.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.BSAll.Location = New System.Drawing.Point(334, 189)
        Me.BSAll.Name = "BSAll"
        Me.BSAll.Size = New System.Drawing.Size(120, 23)
        Me.BSAll.TabIndex = 23
        Me.BSAll.Text = "Select All"
        Me.BSAll.UseVisualStyleBackColor = True
        '
        'BSInv
        '
        Me.BSInv.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.BSInv.Location = New System.Drawing.Point(334, 218)
        Me.BSInv.Name = "BSInv"
        Me.BSInv.Size = New System.Drawing.Size(120, 23)
        Me.BSInv.TabIndex = 24
        Me.BSInv.Text = "Select Inverse"
        Me.BSInv.UseVisualStyleBackColor = True
        '
        'BSNone
        '
        Me.BSNone.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.BSNone.Location = New System.Drawing.Point(334, 247)
        Me.BSNone.Name = "BSNone"
        Me.BSNone.Size = New System.Drawing.Size(120, 23)
        Me.BSNone.TabIndex = 25
        Me.BSNone.Text = "Unselect All"
        Me.BSNone.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.Location = New System.Drawing.Point(26, 103)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(120, 17)
        Me.Label3.TabIndex = 26
        Me.Label3.Text = "Label Range"
        Me.Label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'Label4
        '
        Me.Label4.Location = New System.Drawing.Point(26, 131)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(120, 17)
        Me.Label4.TabIndex = 27
        Me.Label4.Text = "Value Range"
        Me.Label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'TBSelect
        '
        Me.TBSelect.Location = New System.Drawing.Point(298, 482)
        Me.TBSelect.Name = "TBSelect"
        Me.TBSelect.Size = New System.Drawing.Size(85, 23)
        Me.TBSelect.TabIndex = 28
        Me.TBSelect.Text = "Select"
        Me.TBSelect.UseVisualStyleBackColor = True
        '
        'TBClose
        '
        Me.TBClose.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.TBClose.Location = New System.Drawing.Point(389, 511)
        Me.TBClose.Name = "TBClose"
        Me.TBClose.Size = New System.Drawing.Size(65, 23)
        Me.TBClose.TabIndex = 29
        Me.TBClose.Text = "Close"
        Me.TBClose.UseVisualStyleBackColor = True
        '
        'TBDelete
        '
        Me.TBDelete.Location = New System.Drawing.Point(389, 482)
        Me.TBDelete.Name = "TBDelete"
        Me.TBDelete.Size = New System.Drawing.Size(65, 23)
        Me.TBDelete.TabIndex = 30
        Me.TBDelete.Text = "Delete"
        Me.TBDelete.UseVisualStyleBackColor = True
        '
        'TBrl
        '
        Me.TBrl.Location = New System.Drawing.Point(26, 482)
        Me.TBrl.Name = "TBrl"
        Me.TBrl.Size = New System.Drawing.Size(178, 23)
        Me.TBrl.TabIndex = 33
        Me.TBrl.Text = "Replace with Label:"
        Me.TBrl.UseVisualStyleBackColor = True
        '
        'TBrv
        '
        Me.TBrv.Location = New System.Drawing.Point(26, 511)
        Me.TBrv.Name = "TBrv"
        Me.TBrv.Size = New System.Drawing.Size(178, 23)
        Me.TBrv.TabIndex = 35
        Me.TBrv.Text = "Replace with Value:"
        Me.TBrv.UseVisualStyleBackColor = True
        '
        'Label5
        '
        Me.Label5.Location = New System.Drawing.Point(221, 103)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(39, 16)
        Me.Label5.TabIndex = 50
        Me.Label5.Text = "to"
        Me.Label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label6
        '
        Me.Label6.Location = New System.Drawing.Point(221, 74)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(39, 16)
        Me.Label6.TabIndex = 51
        Me.Label6.Text = "to"
        Me.Label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Label7
        '
        Me.Label7.Location = New System.Drawing.Point(221, 132)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(39, 16)
        Me.Label7.TabIndex = 52
        Me.Label7.Text = "to"
        Me.Label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'Ttv
        '
        Me.Ttv.DecimalPlaces = 4
        Me.Ttv.Location = New System.Drawing.Point(210, 511)
        Me.Ttv.Maximum = New Decimal(New Integer() {655359999, 0, 0, 262144})
        Me.Ttv.Minimum = New Decimal(New Integer() {1, 0, 0, 262144})
        Me.Ttv.Name = "Ttv"
        Me.Ttv.Size = New System.Drawing.Size(70, 23)
        Me.Ttv.TabIndex = 34
        Me.Ttv.Value = New Decimal(New Integer() {120, 0, 0, 0})
        '
        'Ttl
        '
        Me.Ttl.Location = New System.Drawing.Point(210, 482)
        Me.Ttl.MaxLength = 2
        Me.Ttl.Name = "Ttl"
        Me.Ttl.Size = New System.Drawing.Size(70, 23)
        Me.Ttl.TabIndex = 32
        Me.Ttl.Text = "01"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(12, 165)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(50, 15)
        Me.Label8.TabIndex = 56
        Me.Label8.Text = "Column"
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(12, 455)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(60, 15)
        Me.Label9.TabIndex = 57
        Me.Label9.Text = "Operation"
        '
        'PictureBox3
        '
        Me.PictureBox3.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.PictureBox3.Location = New System.Drawing.Point(289, 482)
        Me.PictureBox3.Name = "PictureBox3"
        Me.PictureBox3.Size = New System.Drawing.Size(1, 52)
        Me.PictureBox3.TabIndex = 55
        Me.PictureBox3.TabStop = False
        '
        'PictureBox2
        '
        Me.PictureBox2.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.PictureBox2.Location = New System.Drawing.Point(12, 463)
        Me.PictureBox2.Name = "PictureBox2"
        Me.PictureBox2.Size = New System.Drawing.Size(450, 1)
        Me.PictureBox2.TabIndex = 49
        Me.PictureBox2.TabStop = False
        '
        'PictureBox1
        '
        Me.PictureBox1.BackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.PictureBox1.Location = New System.Drawing.Point(12, 173)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(450, 1)
        Me.PictureBox1.TabIndex = 48
        Me.PictureBox1.TabStop = False
        '
        'cbx1
        '
        Me.cbx1.Appearance = System.Windows.Forms.Appearance.Button
        Me.cbx1.Checked = True
        Me.cbx1.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbx1.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cbx1.Location = New System.Drawing.Point(152, 16)
        Me.cbx1.Name = "cbx1"
        Me.cbx1.Size = New System.Drawing.Size(100, 25)
        Me.cbx1.TabIndex = 58
        Me.cbx1.Tag = "1"
        Me.cbx1.Text = "Selected"
        Me.cbx1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.cbx1.UseVisualStyleBackColor = False
        '
        'cbx2
        '
        Me.cbx2.Appearance = System.Windows.Forms.Appearance.Button
        Me.cbx2.Checked = True
        Me.cbx2.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbx2.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cbx2.Location = New System.Drawing.Point(152, 41)
        Me.cbx2.Name = "cbx2"
        Me.cbx2.Size = New System.Drawing.Size(100, 25)
        Me.cbx2.TabIndex = 59
        Me.cbx2.Tag = "1"
        Me.cbx2.Text = "Unselected"
        Me.cbx2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.cbx2.UseVisualStyleBackColor = False
        '
        'cbx3
        '
        Me.cbx3.Appearance = System.Windows.Forms.Appearance.Button
        Me.cbx3.Checked = True
        Me.cbx3.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbx3.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cbx3.Location = New System.Drawing.Point(258, 16)
        Me.cbx3.Name = "cbx3"
        Me.cbx3.Size = New System.Drawing.Size(70, 25)
        Me.cbx3.TabIndex = 60
        Me.cbx3.Tag = "1"
        Me.cbx3.Text = "Short"
        Me.cbx3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.cbx3.UseVisualStyleBackColor = False
        '
        'TBUnselect
        '
        Me.TBUnselect.Location = New System.Drawing.Point(298, 511)
        Me.TBUnselect.Name = "TBUnselect"
        Me.TBUnselect.Size = New System.Drawing.Size(85, 23)
        Me.TBUnselect.TabIndex = 31
        Me.TBUnselect.Text = "Unselect"
        Me.TBUnselect.UseVisualStyleBackColor = True
        '
        'cbx4
        '
        Me.cbx4.Appearance = System.Windows.Forms.Appearance.Button
        Me.cbx4.Checked = True
        Me.cbx4.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbx4.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cbx4.Location = New System.Drawing.Point(258, 41)
        Me.cbx4.Name = "cbx4"
        Me.cbx4.Size = New System.Drawing.Size(70, 25)
        Me.cbx4.TabIndex = 61
        Me.cbx4.Tag = "1"
        Me.cbx4.Text = "Long"
        Me.cbx4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.cbx4.UseVisualStyleBackColor = False
        '
        'cbx5
        '
        Me.cbx5.Appearance = System.Windows.Forms.Appearance.Button
        Me.cbx5.Checked = True
        Me.cbx5.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbx5.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cbx5.Location = New System.Drawing.Point(334, 16)
        Me.cbx5.Name = "cbx5"
        Me.cbx5.Size = New System.Drawing.Size(80, 25)
        Me.cbx5.TabIndex = 62
        Me.cbx5.Tag = "1"
        Me.cbx5.Text = "Hidden"
        Me.cbx5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.cbx5.UseVisualStyleBackColor = False
        '
        'cbx6
        '
        Me.cbx6.Appearance = System.Windows.Forms.Appearance.Button
        Me.cbx6.Checked = True
        Me.cbx6.CheckState = System.Windows.Forms.CheckState.Checked
        Me.cbx6.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cbx6.Location = New System.Drawing.Point(334, 41)
        Me.cbx6.Name = "cbx6"
        Me.cbx6.Size = New System.Drawing.Size(80, 25)
        Me.cbx6.TabIndex = 63
        Me.cbx6.Tag = "1"
        Me.cbx6.Text = "Visible"
        Me.cbx6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.cbx6.UseVisualStyleBackColor = False
        '
        'diagFind
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.CancelButton = Me.TBClose
        Me.ClientSize = New System.Drawing.Size(474, 550)
        Me.Controls.Add(Me.cbx6)
        Me.Controls.Add(Me.cbx5)
        Me.Controls.Add(Me.cbx4)
        Me.Controls.Add(Me.TBUnselect)
        Me.Controls.Add(Me.cbx3)
        Me.Controls.Add(Me.cbx2)
        Me.Controls.Add(Me.cbx1)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.PictureBox3)
        Me.Controls.Add(Me.Ttv)
        Me.Controls.Add(Me.Ttl)
        Me.Controls.Add(Me.PictureBox2)
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.TBrv)
        Me.Controls.Add(Me.TBrl)
        Me.Controls.Add(Me.TBDelete)
        Me.Controls.Add(Me.TBClose)
        Me.Controls.Add(Me.TBSelect)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.BSNone)
        Me.Controls.Add(Me.BSInv)
        Me.Controls.Add(Me.BSAll)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.vr2)
        Me.Controls.Add(Me.vr1)
        Me.Controls.Add(Me.lr2)
        Me.Controls.Add(Me.lr1)
        Me.Controls.Add(Me.mr2)
        Me.Controls.Add(Me.mr1)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label5)
        Me.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "diagFind"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Find / Delete / Replace"
        Me.TopMost = True
        CType(Me.mr1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.mr2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.vr2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.vr1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.Panel1.ResumeLayout(False)
        CType(Me.Ttv, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox3, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox2, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents mr1 As System.Windows.Forms.NumericUpDown
    Friend WithEvents mr2 As System.Windows.Forms.NumericUpDown
    Friend WithEvents lr1 As System.Windows.Forms.TextBox
    Friend WithEvents lr2 As System.Windows.Forms.TextBox
    Friend WithEvents vr2 As System.Windows.Forms.NumericUpDown
    Friend WithEvents vr1 As System.Windows.Forms.NumericUpDown
    Friend WithEvents cb1 As System.Windows.Forms.CheckBox
    Friend WithEvents cb2 As System.Windows.Forms.CheckBox
    Friend WithEvents cb3 As System.Windows.Forms.CheckBox
    Friend WithEvents cb4 As System.Windows.Forms.CheckBox
    Friend WithEvents cb5 As System.Windows.Forms.CheckBox
    Friend WithEvents cb6 As System.Windows.Forms.CheckBox
    Friend WithEvents cbb1 As System.Windows.Forms.CheckBox
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents BSAll As System.Windows.Forms.Button
    Friend WithEvents BSInv As System.Windows.Forms.Button
    Friend WithEvents BSNone As System.Windows.Forms.Button
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents TBSelect As System.Windows.Forms.Button
    Friend WithEvents TBClose As System.Windows.Forms.Button
    Friend WithEvents TBDelete As System.Windows.Forms.Button
    Friend WithEvents TBrl As System.Windows.Forms.Button
    Friend WithEvents TBrv As System.Windows.Forms.Button
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents PictureBox2 As System.Windows.Forms.PictureBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Ttv As System.Windows.Forms.NumericUpDown
    Friend WithEvents Ttl As System.Windows.Forms.TextBox
    Friend WithEvents PictureBox3 As System.Windows.Forms.PictureBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents cbx1 As System.Windows.Forms.CheckBox
    Friend WithEvents cbx2 As System.Windows.Forms.CheckBox
    Friend WithEvents cbx3 As System.Windows.Forms.CheckBox
    Friend WithEvents TBUnselect As System.Windows.Forms.Button
    Friend WithEvents cbx4 As System.Windows.Forms.CheckBox
    Friend WithEvents cbx5 As System.Windows.Forms.CheckBox
    Friend WithEvents cbx6 As System.Windows.Forms.CheckBox
End Class
