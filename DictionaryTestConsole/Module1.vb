Imports System.Deployment.Application

Module Module1

    Dim config As New Dictionary(Of String, String)
    Dim configDelimiter As Char = "="c

    Sub Main()

        'Configの読み込み
        config = ReadConfig("Config.txt")

        'Configの列挙
        For Each kvp As KeyValuePair(Of String, String) In config
            Console.WriteLine("{0} - {1}", kvp.Key, kvp.Value)
        Next

        'Configの追加
        config.Add("Today", Today.ToShortDateString)
        config.Add("Now", Now.ToString)

        'プログラムの改竄チェック的なもの
        'あらかじめconfigにプログラムのハッシュを入れて配布し、
        'それを起動時に自分自身のアセンブリをSHA256とかのハッシュを取って
        '比較して、差異が無ければプログラム改竄なしと判断する処理

        Select Case ProgramTamperCheck(config)
            Case True   '改竄無し
                Console.WriteLine("Program is not Tampered")
            Case False  '改竄の可能性有り
                Console.WriteLine("Program is Tampered!!!")
        End Select

        'SHA256ハッシュを計算して格納してみるテスト
        config.Add("ProgramPath", System.Reflection.Assembly.GetExecutingAssembly().Location)

        If config.ContainsKey("ProgramHash") Then
            config("ProgramHash") = ComputeSHA512Hash(IO.File.ReadAllBytes(System.Reflection.Assembly.GetExecutingAssembly().Location))
        Else
            config.Add("ProgramHash", ComputeSHA512Hash(IO.File.ReadAllBytes(System.Reflection.Assembly.GetExecutingAssembly().Location)))
        End If



        'Configの書き出し
        WriteConfig("OutConfig.txt", config)


        Console.ReadLine()

    End Sub


    Function ReadConfig(filename As String) As Dictionary(Of String, String)

        'configファイルの読み込み
        Dim configLines As String() = IO.File.ReadAllLines(filename)

        Dim dict As New Dictionary(Of String, String)

        '読み込んだconfigのDictionari型への追加
        For Each configLine As String In configLines

            dict.Add(configLine.Split(configDelimiter)(0).Trim(" "c), configLine.Split(configDelimiter)(1).Trim(" "c))

        Next

        Return dict

    End Function

    Sub WriteConfig(filename As String, config As Dictionary(Of String, String))

        Dim writeData As String = ""
        For Each kvp As KeyValuePair(Of String, String) In config
            writeData &= kvp.Key & "=" & kvp.Value & vbCrLf
        Next

        IO.File.WriteAllText(filename, writeData)

    End Sub

    Function ComputeSHA512Hash(bytes() As Byte) As String
        Dim sha512 As New System.Security.Cryptography.SHA512CryptoServiceProvider()

        Dim hashbyte As Byte() = sha512.ComputeHash(bytes)

        sha512.Clear()

        Dim result As New Text.StringBuilder()

        For Each b As Byte In hashbyte
            result.Append(b.ToString("x2"))
        Next

        Return result.ToString

    End Function

    Function ComputeSHA256Hash(bytes() As Byte) As String
        Dim sha256 As New System.Security.Cryptography.SHA256CryptoServiceProvider()

        Dim hashbyte As Byte() = sha256.ComputeHash(bytes)

        sha256.Clear()

        Dim result As New Text.StringBuilder()

        For Each b As Byte In hashbyte
            result.Append(b.ToString("x2"))
        Next

        Return result.ToString

    End Function

    Function ProgramTamperCheck(config As Dictionary(Of String, String)) As Boolean

        Dim configHash As String = ""
        Dim programHash As String = ""

        If config.ContainsKey("ProgramHash") Then
            configHash = config("ProgramHash")
        Else
            Console.WriteLine("ProgramHash key is not found.")
            Return False
        End If

        programHash = ComputeSHA512Hash(IO.File.ReadAllBytes(System.Reflection.Assembly.GetExecutingAssembly().Location))

        If configHash = programHash Then
            Return True
        Else
            Return False
        End If


    End Function


End Module
