Namespace Editor
    Public NotInheritable Class ChartCalculations
        Public Const RecommendedTotalMin As Integer = 260

        Private Sub New()
        End Sub

        Public Shared Function CalculateRecommendedTotal(ByVal notes As Integer) As Integer
            Return Math.Max(RecommendedTotalMin, CInt(Math.Floor(760.5R * notes / (notes + 650.0R))))
        End Function
    End Class
End Namespace
