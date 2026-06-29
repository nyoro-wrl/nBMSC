Imports nBMSC.Editor

Module BMS
    Public Function IsChannelLongNote(ByVal I As String) As Boolean
        Dim LongStart = C36ChannelTo10("50")
        Dim LongEnd = C36ChannelTo10("8Z")

        Dim xI As Integer = C36ChannelTo10(I)

        Return xI > LongStart And xI < LongEnd
    End Function

    Public Function IsChannelHidden(ByVal I As String) As Boolean
        Dim HiddenStart = C36ChannelTo10("30")
        Dim HiddenEnd = C36ChannelTo10("4Z")
        Dim OptionStart = C36ChannelTo10("70")
        Dim OptionEnd = C36ChannelTo10("8Z")

        Dim xI As Integer = C36ChannelTo10(I)

        Return (xI > HiddenStart And xI < HiddenEnd) Or (xI > OptionStart And xI < OptionEnd)
    End Function

    Public Function IsChannelLandmine(ByVal I As String) As Boolean
        Dim LandmineStart = C36ChannelTo10("D0")
        Dim LandmineEnd = C36ChannelTo10("EZ")

        Dim xI As Integer = C36ChannelTo10(I)

        Return xI > LandmineStart And xI < LandmineEnd
    End Function
End Module
