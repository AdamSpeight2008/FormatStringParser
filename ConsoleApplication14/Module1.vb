Module Module1

    Sub Main()
        Dim p As New FSD.FormatStringParser
        Dim fs = "A { 0,-1:x2}bb{1:}"
        Dim s = p.Parse(fs)
        Colorise(s)
    End Sub

    Public Sub Colorise(s As FSD.FormatStringParser.Span)
        Select Case s.Kind
            Case FSD.FormatStringParser.SpanKind.Closing_Brace,
                 FSD.FormatStringParser.SpanKind.Opening_Brace
                Console.ForegroundColor = ConsoleColor.Green
                Console.Write(s.GetSpanText)
            Case FSD.FormatStringParser.SpanKind.Colon,
                 FSD.FormatStringParser.SpanKind.Comma
                Console.ForegroundColor = ConsoleColor.Green
                Console.Write(s.GetSpanText)
            Case FSD.FormatStringParser.SpanKind.Escaped_Closing_Brace,
                 FSD.FormatStringParser.SpanKind.Escaped_Opening_Brace
                Console.ForegroundColor = ConsoleColor.DarkGray
                Console.Write(s.GetSpanText)
            Case FSD.FormatStringParser.SpanKind.Error_Arg_Format,
                 FSD.FormatStringParser.SpanKind.Error_Arg_Align,
                 FSD.FormatStringParser.SpanKind.Error_Arg_Hole,
                 FSD.FormatStringParser.SpanKind.Error_Arg_Index
                Console.BackgroundColor = ConsoleColor.DarkRed
                Console.ForegroundColor = ConsoleColor.Black
                Console.Write(s.GetSpanText)
                Console.ResetColor()
            Case FSD.FormatStringParser.SpanKind.FormatString,
                 FSD.FormatStringParser.SpanKind.Arg_Hole,
                 FSD.FormatStringParser.SpanKind.Arg_Index,
                 FSD.FormatStringParser.SpanKind.Arg_Alignment,
                 FSD.FormatStringParser.SpanKind.Arg_Format

                For Each c In s.Contents
                    Colorise(c)
                Next
            Case Else
                'If s.Contents.Any = False Then
                Console.ForegroundColor = ConsoleColor.White
                    Console.Write(s.GetSpanText)

                'End If
        End Select
    End Sub
End Module


