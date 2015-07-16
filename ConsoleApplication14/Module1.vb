Module Module1

    Sub Main()
        Dim p As New FSD.FormatStringParser
        Dim fs = "A { 0,-1:x2}{1}}}"
        Dim s = p.Parse(fs)
    End Sub
End Module


