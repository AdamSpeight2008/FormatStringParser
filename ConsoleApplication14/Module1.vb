Module Module1

    Sub Main()
        Dim p As New FSD.FormatStringParser
        Dim fs = "A { 0,-1:x2}bb{1,aaa}"
        Dim s = p.Parse(fs)
        Colorise(s)
        Dim Holes = ArgHoles(s).ToArray
        Dim HoleIndice = ArgIndice(Holes).ToArray
    End Sub

    Public Iterator Function ArgHoles(s As FSD.FormatStringParser.Span) As IEnumerable(Of FSD.FormatStringParser.Span)
        For Each p In s.Contents
            If p.Kind = FSD.FormatStringParser.SpanKind.Arg_Hole OrElse
               p.Kind = FSD.FormatStringParser.SpanKind.Error_Arg_Hole Then
                Yield p
            End If
        Next
    End Function

    Public Iterator Function ArgIndice(xs As IEnumerable(Of FSD.FormatStringParser.Span)) As IEnumerable(Of Integer?)
        For Each h In xs
            If h.Kind <> FSD.FormatStringParser.SpanKind.Error_Arg_Hole AndAlso h.Kind <> FSD.FormatStringParser.SpanKind.Arg_Hole Then Continue For
            For Each p In h.Contents
                If p.Kind <> FSD.FormatStringParser.SpanKind.Arg_Index AndAlso p.Kind <> FSD.FormatStringParser.SpanKind.Error_Arg_Index Then Continue For
                Dim r = p.Contents.FirstOrDefault(Function(x) x.Kind = FSD.FormatStringParser.SpanKind.Digits)
                If r Is Nothing Then Continue For
                Dim value As Integer
                If Integer.TryParse(r.GetSpanText, value) = False Then Continue For
                Yield value
            Next
        Next
    End Function

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
                Console.BackgroundColor = ConsoleColor.White
                Console.ForegroundColor = ConsoleColor.red
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


