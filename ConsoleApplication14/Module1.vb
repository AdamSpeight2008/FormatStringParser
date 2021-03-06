﻿Module Module1

    Sub Main()
        Dim p As New FSD.FormatStringParser
        Dim fs = "a: {0,-1:X2}" ' "A { 0,-1:x2}bb{1,aaa}"
        Dim s = p.Parse(fs)
        Colorise(s)
        Dim Holes = ArgHoles(s).ToArray
        Dim HoleIndice = ArgIndice(Holes).ToArray
        Dim ec = ErrorCount(s)
        Dim f = Flatten(s).ToArray
    End Sub

    Function ErrorCount(s As FSD.FormatStringParser.Span) As Integer?
        If s?.Kind <> FSD.FormatStringParser.SpanKind.FormatString Then Return Nothing
        Return s.Contents.Where(Function(x) IsErrorKind(x)).Count
    End Function

    Public Function IsErrorKind(s As FSD.FormatStringParser.Span) As Boolean
        Select Case s.Kind
            Case FSD.FormatStringParser.SpanKind.Error_Arg_Align,
                 FSD.FormatStringParser.SpanKind.Error_Arg_Format,
                 FSD.FormatStringParser.SpanKind.Error_Arg_Format_Impossible,
                 FSD.FormatStringParser.SpanKind.Error_Arg_Hole,
                 FSD.FormatStringParser.SpanKind.Error_Arg_Index,
                 FSD.FormatStringParser.SpanKind.Error_FormatString,
                 FSD.FormatStringParser.SpanKind.Error_Unexpected_Char,
                 FSD.FormatStringParser.SpanKind.Error_Unexpected_Closing_Brace,
                 FSD.FormatStringParser.SpanKind.Error_Unexpected_Opening_Brace,
                 FSD.FormatStringParser.SpanKind.Unexpected_EOT
                Return True
        End Select
        Return False
    End Function

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

    Iterator Function Flatten(s As FSD.FormatStringParser.Span) As IEnumerable(Of FSD.FormatStringParser.Span)
        If s Is Nothing Then Return ' Enumerable.Empty(Of FSD.FormatStringParser.Span)
        Dim xs As New LinkedList(Of FSD.FormatStringParser.Span)
        xs.AddFirst(s)
        While xs.Any
            Dim xn = xs.First
            Dim x = xn.Value
            Yield x
            xs.RemoveFirst()
            If x.Contents.Any = False Then Continue While
            Dim n = x.Contents.Count - 1
            Dim f = xs.AddFirst(x.Contents(0))
            For i = 1 To n
                f = xs.AddAfter(f, x.Contents(i))
            Next
        End While
    End Function
End Module


