Namespace My

    Partial Friend Class MyApplication
        Private Const EmbeddedAssemblyPrefix As String = "EmbeddedAssemblies."
        Private Shared ReadOnly EmbeddedAssemblyCache As New Dictionary(Of String, Reflection.Assembly)(StringComparer.OrdinalIgnoreCase)

        Private Sub MyApplication_Shutdown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shutdown

        End Sub

        Private Sub MyApplication_Startup(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup
            AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf ResolveEmbeddedAssembly
        End Sub

        Private Shared Function ResolveEmbeddedAssembly(ByVal sender As Object, ByVal args As ResolveEventArgs) As Reflection.Assembly
            Dim xAssemblyName As New Reflection.AssemblyName(args.Name)
            Dim xResourceName As String = EmbeddedAssemblyPrefix & xAssemblyName.Name & ".dll"

            SyncLock EmbeddedAssemblyCache
                If EmbeddedAssemblyCache.ContainsKey(xAssemblyName.Name) Then
                    Return EmbeddedAssemblyCache(xAssemblyName.Name)
                End If

                Dim xAssembly As Reflection.Assembly = Reflection.Assembly.GetExecutingAssembly()
                Using xStream As IO.Stream = xAssembly.GetManifestResourceStream(xResourceName)
                    If xStream Is Nothing Then
                        Return Nothing
                    End If

                    Dim xBytes(CInt(xStream.Length) - 1) As Byte
                    Dim xOffset As Integer = 0
                    Do While xOffset < xBytes.Length
                        Dim xRead As Integer = xStream.Read(xBytes, xOffset, xBytes.Length - xOffset)
                        If xRead = 0 Then Exit Do
                        xOffset += xRead
                    Loop

                    Dim xLoadedAssembly As Reflection.Assembly = Reflection.Assembly.Load(xBytes)
                    EmbeddedAssemblyCache(xAssemblyName.Name) = xLoadedAssembly
                    Return xLoadedAssembly
                End Using
            End SyncLock
        End Function

        Private Sub MyApplication_UnhandledException(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException
            Dim xRes As MsgBoxResult = MsgBox("An unhandled exception has occurred in the application: " & vbCrLf & _
                                       e.Exception.Message & vbCrLf & _
                                       vbCrLf & _
                                       "Click Yes to save a back-up, click No otherwise, or click Cancel to ignore this exception and continue.", _
                                       MsgBoxStyle.YesNoCancel + MsgBoxStyle.Critical, _
                                       "Unhandled Exception")
            If xRes = MsgBoxResult.Cancel Then e.ExitApplication = False
            If xRes = MsgBoxResult.Yes Then
                Dim xFN As String
                Dim xDate As Date = DateTime.Now
                With xDate
                    xFN = "\AutoSave_" & .Year & "_" & .Month & "_" & .Day & "_" & .Hour & "_" & .Minute & "_" & .Second & "_" & .Millisecond & ".NBMSC"
                End With

                'My.Computer.FileSystem.WriteAllText(My.Application.Info.DirectoryPath & xFN, Form1.ExceptionSave, False)
                MainWindow.ExceptionSave(My.Application.Info.DirectoryPath & xFN)
                MsgBox("A back-up has been saved to " & My.Application.Info.DirectoryPath & xFN, MsgBoxStyle.Information)
            End If
        End Sub
    End Class

End Namespace

