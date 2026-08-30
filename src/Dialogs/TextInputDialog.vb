Imports System.Windows.Forms

Public Class TextInputDialog
    Public Sub New(ByVal prompt As String,
                   ByVal title As String,
                   ByVal defaultResponse As String)
        InitializeComponent()

        Text = If(String.IsNullOrEmpty(title), Application.ProductName, title)
        PromptLabel.Text = prompt
        InputTextBox.Text = If(defaultResponse, "")
        OKInputButton.Text = Strings.OK
        CancelInputButton.Text = Strings.Cancel
    End Sub

    Public ReadOnly Property InputText As String
        Get
            Return InputTextBox.Text
        End Get
    End Property

    Protected Overrides Sub OnShown(ByVal e As EventArgs)
        MyBase.OnShown(e)
        SelectInputText()
    End Sub

    Private Sub SelectInputText()
        InputTextBox.Focus()
        InputTextBox.SelectAll()
    End Sub
End Class
