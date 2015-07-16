Namespace Global.FSD
    Public Class FormatStringParser

        Public Sub New()
        End Sub

        Public Function Parse(fs As String) As Span
            Return Parse_FormatString(fs)
        End Function

        Private Function NextChar(fs As String, x As Integer) As Char?
            x = x + 1
            If x >= fs.Length Then Return New Char?()
            Return New Char?(fs(x))
        End Function

        Private Function Parse_FormatString(fs As String) As Span
            Dim content As New LinkedList(Of Span)
            Dim c As Char
            Dim n As Char?
            Dim x = 0
            Dim bx = x
            While x < fs.Length
                c = fs(x)
                Select Case c
                    Case "{"c
                        ' Is it an escaped opening brace?
                        n = NextChar(fs, x)
                        If n.HasValue Then
                            ' Possibly
                            If n.Value = "{" Then
                                x = Add_EOB(fs, content, x) ' Escaped Opening Brace
                            Else
                                x = content.AddLast(Parse_ArgHole(fs, x)).Value.ex
                            End If
                        Else
                            ' Opening brace found at end of text
                            ' Error as it can't be either ;=
                            ' a) Escaped
                            ' b) Start of an Arg Hole
                            x = Add_UOB(fs, content, x)
                        End If

                    Case "}"c
                        ' Is it an escaped closing brace?
                        n = NextChar(fs, x)
                        If n.HasValue Then
                            ' Possibly
                            If n.Value = "}"c Then
                                x = Add_ECB(fs, content, x) ' Escape Closing Brace
                            Else
                                x = Add_UCB(fs, content, x)
                            End If
                        Else
                            x = Add_UCB(fs, content, x)
                        End If
                    Case Else
                        ' Treat as normal text
                        x += 1
                End Select
            End While
            Return New Span(fs, SpanKind.FormatString, bx, x, content)
        End Function
        Private Function Add_EOT(fs As String, ByRef content As LinkedList(Of Span), x As Integer) As Integer
            Return content.AddLast(New Span(fs, SpanKind.Unexpected_EOT, x, x)).Value.ex
        End Function
        Private Function Add_CB(fs As String, ByRef content As LinkedList(Of Span), x As Integer) As Integer
            Return content.AddLast(New Span(fs, SpanKind.Closing_Brace, x, x + 1)).Value.ex
        End Function
        Private Function Add_OB(fs As String, ByRef content As LinkedList(Of Span), x As Integer) As Integer
            Return content.AddLast(New Span(fs, SpanKind.Opening_Brace, x, x + 1)).Value.ex
        End Function
        Private Function Add_UC(fs As String, ByRef content As LinkedList(Of Span), x As Integer) As Integer
            Return content.AddLast(New Span(fs, SpanKind.Error_Unexpected_Char, x, x + 1)).Value.ex
        End Function
        Private Function Add_UCB(fs As String, ByRef content As LinkedList(Of Span), x As Integer) As Integer
            Return content.AddLast(New Span(fs, SpanKind.Error_Unexpected_Closing_Brace, x, x + 1)).Value.ex
        End Function
        Private Function Add_UOB(fs As String, ByRef content As LinkedList(Of Span), x As Integer) As Integer
            Return content.AddLast(New Span(fs, SpanKind.Error_Unexpected_Opening_Brace, x, x + 1)).Value.ex
        End Function
        Private Function Add_ECB(fs As String, ByRef content As LinkedList(Of Span), x As Integer) As Integer
            Return content.AddLast(New Span(fs, SpanKind.Escaped_Closing_Brace, x, x + 2)).Value.ex
        End Function
        Private Function Add_EOB(fs As String, ByRef content As LinkedList(Of Span), x As Integer) As Integer
            Return content.AddLast(New Span(fs, SpanKind.Escaped_Opening_Brace, x, x + 2)).Value.ex
        End Function
        Private Function Parse_ArgHole(fs As String, x As Integer) As Span
            Dim bx = x
            Dim contents As New LinkedList(Of Span)
            x = contents.AddLast(New Span(fs, SpanKind.Opening_Brace, bx, bx + 1)).Value.ex
            x = contents.AddLast(Parse_Whitespace(fs, x)).Value.ex
            x = contents.AddLast(Parse_Arg_Index(fs, x)).Value.ex
            x = contents.AddLast(Parse_Whitespace(fs, x)).Value.ex
            If x >= fs.Length Then Return New Span(fs, SpanKind.Error_Arg_Hole, bx, Add_EOT(fs, contents, x), contents)
            Dim ch = fs(x)
            ' Is it the Alignment seperator glyph?
            If ch = "," Then
                x = contents.AddLast(Parse_Arg_Align(fs, x)).Value.ex
                If x >= fs.Length Then Return New Span(fs, SpanKind.Error_Arg_Hole, bx, Add_EOT(fs, contents, x), contents)
                ch = fs(x)
            End If
            ' Is it the format seperator glyph?
            If ch = ":" Then
                x = contents.AddLast(Parse_Arg_Format(fs, x)).Value.ex
                If x >= fs.Length Then Return New Span(fs, SpanKind.Error_Arg_Hole, bx, Add_EOT(fs, contents, x), contents)
                ch = fs(x)
            End If
            ' Is it the opening brace glyph?
            If ch = "{" Then
                Dim nc = NextChar(fs, x)
                If nc.HasValue Then
                    If nc.Value = "{"c Then Return New Span(fs, SpanKind.Error_Arg_Hole, bx, Add_EOB(fs, contents, x), contents)
                    Return New Span(fs, SpanKind.Error_Arg_Hole, bx, Add_UC(fs, contents, x), contents)
                Else
                    Return New Span(fs, SpanKind.Error_Arg_Hole, bx, Add_EOT(fs, contents, x), contents)
                End If
            End If
            ' Is it the closing brace glyph?
            If ch = "}" Then
                Dim nc = NextChar(fs, x)
                If nc.HasValue AndAlso nc.Value = "}"c Then Return New Span(fs, SpanKind.Error_Arg_Hole, bx, Add_ECB(fs, contents, x), contents)
                Return New Span(fs, SpanKind.Arg_Hole, bx, Add_CB(fs, contents, x), contents)
            End If
            Return New Span(fs, SpanKind.Error_Arg_Hole, bx, Add_UC(fs, contents, x), contents)
        End Function
        Private Function Parse_Arg_Format(fs As String, x As Integer) As Span
            Dim bx = x
            Dim contents As New LinkedList(Of Span)
            x = contents.AddLast(New Span(fs, SpanKind.Colon, bx, x + 1)).Value.ex
            Dim ch As Char
            While True
                If x >= fs.Length Then Return New Span(fs, SpanKind.Error_Arg_Format, bx, Add_EOT(fs, contents, x), contents) ' Unexpected EOT
                ch = fs(x)
                Select Case ch
                    Case "}"c
                        Dim nc = NextChar(fs, x)
                        If nc.HasValue = False Then Return New Span(fs, SpanKind.Arg_Format, bx, x, contents)
                        If nc.Value <> "}"c Then Return New Span(fs, SpanKind.Arg_Format, bx, x, contents)
                        x = Add_ECB(fs, contents, x) ' Escaped Closing Brace
                    Case "{"c
                        Dim nc = NextChar(fs, x)
                        If nc.HasValue = False Then Return New Span(fs, SpanKind.Error_Arg_Format, bx, Add_EOT(fs, contents, x), contents)
                        If nc.Value = "{"c Then
                            ' Escaped Opening Brace
                            x = Add_EOB(fs, contents, x)
                        Else
                            x = Add_UOB(fs, contents, x)
                        End If
                    Case Else
                        x = contents.AddLast(New Span(fs, SpanKind.Text, bx, x + 1)).Value.ex
                End Select
            End While
            ' Unexpectedly reached impossible place
            Return New Span(fs, SpanKind.Error_Arg_Format, bx,
                        contents.AddLast(New Span(fs, SpanKind.Error_Arg_Format_Impossible, x, x)).Value.ex, contents)
        End Function
        Private Function Parse_Arg_Align(fs As String, x As Integer) As Span
            Dim bx = x
            Dim contents As New LinkedList(Of Span)
            x = contents.AddLast(New Span(fs, SpanKind.Comma, x, x + 1)).Value.ex
            If x > fs.Length Then Return New Span(fs, SpanKind.Error_Arg_Align, bx, Add_EOT(fs, contents, x), contents)  ' Unexpect EOT
            Dim ch = fs(x)
            If ch = "-"c Then
                x = contents.AddLast(New Span(fs, SpanKind.Minus, x, x + 1)).Value.ex
                x = contents.AddLast(Parse_Digits(fs, x)).Value.ex
                Return New Span(fs, SpanKind.Arg_Alignment, bx, x, contents)
            ElseIf IsDigit(ch) Then
                Return New Span(fs, SpanKind.Arg_Alignment, bx, contents.AddLast(Parse_Digits(fs, x)).Value.ex, contents)
            Else
                Return New Span(fs, SpanKind.Error_Arg_Align, bx, x, contents)
            End If
        End Function
        Private Function Parse_Arg_Index(fs As String, x As Integer) As Span
            Dim bx = x
            Dim digits = Parse_Digits(fs, x)
            x = digits.ex
            If digits.Kind = SpanKind.Empty Then Return New Span(fs, SpanKind.Error_Arg_Index, bx, x, digits)
            Return New Span(fs, SpanKind.Arg_Index, bx, x, digits)
        End Function
        Private Function Parse_Whitespace(fs As String, x As Integer) As Span
            Dim bx = x
            Dim c As Char
            While True
                If x >= fs.Length Then Exit While
                c = fs(x)
                If c <> " "c Then Exit While
                x += 1
            End While
            Dim d = x - bx
            If d = 0 Then Return New Span(fs, SpanKind.Empty, bx, x)
            Return New Span(fs, SpanKind.Whitespace, bx, x)
        End Function
        Private Function IsDigit(ch As Char) As Boolean
            Return ("0"c <= ch) AndAlso (ch <= "9"c)
        End Function
        Private Function Parse_Digits(fs As String, x As Integer) As Span
            Dim bx = x
            Dim c As Char
            While True
                If x >= fs.Length Then Exit While
                c = fs(x)
                If IsDigit(c) = False Then Exit While
                x += 1
            End While
            Dim d = x - bx
            If d = 0 Then Return New Span(fs, SpanKind.Empty, bx, x)
            Return New Span(fs, SpanKind.Digits, bx, x)
        End Function

        Public Class Span
            Public ReadOnly Property fs As String
            Public ReadOnly Property bx As Integer
            Public ReadOnly Property ex As Integer
            Public ReadOnly Property Kind As SpanKind
            Public ReadOnly Property Contents As New LinkedList(Of Span)
            Friend Sub New(fs As String, Kind As SpanKind, bx As Integer, ex As Integer)
                Me.fs = fs
                Me.Kind = Kind
                Me.bx = bx
                Me.ex = ex
            End Sub
            Friend Sub New(fs As String, Kind As SpanKind, bx As Integer, ex As Integer, s As Span)
                Me.New(fs, Kind, bx, ex)
                If s IsNot Nothing Then Me.Contents.AddLast(s)
            End Sub
            Friend Sub New(fs As String, Kind As SpanKind, bx As Integer, ex As Integer, c As LinkedList(Of Span))
                Me.New(fs, Kind, bx, ex)
                Me.Contents = New LinkedList(Of Span)(c)
            End Sub
            Friend Sub New(fs As String, Kind As SpanKind, bx As Integer, ex As Integer, c As LinkedList(Of Span), s As Span)
                Me.New(fs, Kind, bx, ex, c)
                If s IsNot Nothing Then Me.Contents.AddLast(s)
            End Sub
            Public Overrides Function ToString() As String
                Return $"({Kind}) [{GetSpanText()}]"
            End Function
            Public Function GetSpanText() As String
                Dim d = ex - bx
                If d <= 0 Then Return ""
                Return fs.Substring(bx, d)
            End Function
        End Class

        Public Enum SpanKind As Integer
            Empty = 0
            FormatString
            Digits
            Whitespace
            Text
            Escaped_Opening_Brace
            Escaped_Closing_Brace
            Arg_Hole
            Opening_Brace
            Closing_Brace
            Arg_Index
            Comma
            Arg_Alignment
            Colon
            Arg_Format
            Minus
            Unexpected_EOT
            Error_Arg_Index
            Error_Arg_Align
            Error_Arg_Format
            Error_Arg_Hole
            Error_FormatString
            Error_Arg_Format_Impossible
            Error_Unexpected_Opening_Brace
            Error_Unexpected_Closing_Brace
            Error_Unexpected_Char
        End Enum

    End Class

End Namespace