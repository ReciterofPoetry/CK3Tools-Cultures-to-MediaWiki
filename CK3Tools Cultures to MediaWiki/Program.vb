Imports System.IO

Friend Module Props
    'Property BaseDir As String = "D:\Programs\Steam\steamapps\workshop\content\1158310\2326030123"
    'Property BaseDir As String = "D:\Programs\Steam\steamapps\common\Crusader Kings III\game"
    Property BaseDir As String = Environment.CurrentDirectory
    Property GameDir As String
End Module
Module Program
#Disable Warning IDE0044 ' Add readonly modifier
    Dim GameConceptLocalisations As New Hashtable()
    Dim SavedLocalisation As New Hashtable()
    Dim LocalisationFiles As List(Of String)
#Enable Warning IDE0044 ' Add readonly modifier
    Sub Main()

        SetGameDir()

        Dim FileList As New List(Of String)
        'Get heritages in code.
        If Directory.Exists(BaseDir & "\common\culture\pillars") AndAlso Not Directory.GetFiles(BaseDir & "\common\culture\pillars").Length = 0 Then
                FileList = Directory.GetFiles(BaseDir & "\common\culture\pillars", "*.txt", SearchOption.AllDirectories).ToList
            End If
        FileList.Reverse() 'Reverse due to load order demanding that the last occurrence of an object be the relevant one in code.
        Dim BaseFileList As List(Of String) = Directory.GetFiles(GameDir & "\common\culture\pillars", "*.txt", SearchOption.AllDirectories).ToList
        BaseFileList.Reverse()
        FileList = FileList.Concat(BaseFileList).ToList


        Dim HeritageFiles As New List(Of String)
        For Each TextFile In FileList
            Using SR As New StreamReader(TextFile)
                While Not SR.EndOfStream
                    If SR.ReadLine.Contains("heritage") Then
                        HeritageFiles.Add(TextFile)
                        Exit While
                    End If
                End While
            End Using
        Next

        Dim Heritages As New List(Of String)
        For Each TextFile In HeritageFiles
            Dim Text As String = File.ReadAllText(TextFile)
            Dim Blocks As New SortedList(Of String, String)
            Do
                Dim RawObject As String = Text.Split("{"c, 2).First
                Dim Block As String = Text.Split("{"c, 2).Last
                RawObject = RawObject.Split("="c, 2).First.Trim.Split({" "c, vbTab, vbCrLf, vbCr, vbLf}, StringSplitOptions.None).Last
                If Block.Split("}"c, 2).First.Contains("{"c) Then
                    Do
                        Block = String.Join(">"c, String.Join("<"c, Block.Split("{"c, 2)).Split("}"c, 2))
                    Loop While Split("}"c, 2).First.Contains("{"c)
                End If
                Block = Block.Split("}"c, 2).First.Replace("<", "{").Replace(">", "}")
                If Not RawObject.StartsWith("#"c) Then
                    If Not Blocks.ContainsKey(RawObject) Then
                        Blocks.Add(RawObject, Block)
                    Else
                        Blocks(RawObject) = Block
                    End If
                End If
                Text = Text.Split(Block, 2).Last.Split("}", 2).Last
            Loop While Text.Split("}"c, 2).First.Contains("{"c)
            For Each Block In Blocks.Keys
                If Blocks(Block).Contains("type") AndAlso Blocks(Block).Split("type", 2)(1).Split(vbCrLf, 2)(0).Contains("heritage") Then
                    Heritages.Add(Block)
                End If
            Next
        Next

        Heritages.Sort()

        Dim NamedColours As New SortedList(Of String, String)
        Dim NamedColourFiles As List(Of String) = Directory.GetFiles(GameDir & "\common\named_colors").ToList
        If Directory.Exists(BaseDir & "\common\named_colors") Then
            NamedColourFiles = NamedColourFiles.Concat(Directory.GetFiles(BaseDir & "\common\named_colors")).ToList
        End If

        For Each ColourFile In NamedColourFiles
            Dim Text As String = File.ReadAllText(ColourFile).Split("colors", 2).Last.Split("{"c, 2).Last.TrimEnd.TrimEnd("}"c)
            Do
                Dim Name As String = Text.Split("="c, 2).First.TrimEnd.Split({" "c, vbTab, vbCrLf, vbCr, vbLf}, StringSplitOptions.None).Last
                Dim Colour As String = Text.Split("{"c, 2).Last.Split("}"c, 2).First.Trim
                If Not Text.Split(Name, 2).Last.Split("}"c, 2).First.Contains("hsv") Then
                    If Not NamedColours.ContainsKey(Name) Then
                        NamedColours.Add(Name, Colour)
                    Else
                        NamedColours(Name) = Colour
                    End If
                End If
                Text = Text.Split(Colour, 2).Last.Split("}"c, 2).Last
            Loop While Text.Contains("="c)
        Next

        Dim CultureFiles As List(Of String) = Directory.GetFiles(BaseDir & "\common\culture\cultures", "*.txt", SearchOption.AllDirectories).ToList
        Dim RawCultures As New List(Of String)
        For Each TextFile In CultureFiles
            Dim Blocks As New List(Of String)
            Dim RawText As String = File.ReadAllText(TextFile)
            Blocks = DeNest(RawText)

            For TextCount = 0 To Blocks.Count - 1
                If Blocks(TextCount).Contains("#"c) Then
                    Dim Hold As List(Of String) = Blocks(TextCount).Split(vbCrLf).ToList
                    Hold.RemoveAll(Function(x) x.TrimStart.StartsWith("#"c))
                    Blocks(TextCount) = String.Join(vbCrLf, Hold)
                End If
            Next
            Blocks.RemoveAll(Function(x) Not x.Contains("}"c))
            RawCultures = RawCultures.Concat(Blocks).ToList
        Next

        Dim Cultures, Colours, Ethoses, Traditions, Languages, MartialCustoms, CultureDescriptions As New List(Of String)
        Dim HeritageCultures As New SortedList(Of Integer, String)

        For Each Block In RawCultures
            Dim DeComment As List(Of String) = Block.Split({vbCrLf, vbCr, vbLf}, StringSplitOptions.None).ToList
            DeComment.RemoveAll(Function(x) x.TrimStart.StartsWith("#"c))
            For Count = 0 To DeComment.Count - 1
                If DeComment(Count).Contains("#"c) Then
                    DeComment(Count) = DeComment(Count).Split("#"c, 2).First.TrimEnd
                End If
            Next
            Block = String.Join(vbCrLf, DeComment)

            Dim Culture As String = Block.Split({" ", "=", "{"}, StringSplitOptions.RemoveEmptyEntries)(0).Trim
            If Culture.Contains(vbCrLf) Then
                Culture = Culture.Split(vbCrLf).Last
            End If
            Cultures.Add(Culture.Trim(vbTab))

            Dim Heritage As String = ""
            If Block.Contains("heritage") Then
                Heritage = Block.Split("heritage", 2)(1).Split("="c, 2).Last.TrimStart.Split({" "c, vbTab, vbCrLf, vbCr, vbLf}, 2, StringSplitOptions.None).First.Trim
            Else
                Debug.Print("No heritage found for: " & Culture)
                Heritage = "None"
                If Not Heritages.Contains("None") Then
                    Heritages.Add("None")
                End If
            End If
            Dim HeritageIndex As Integer = Heritages.FindIndex(Function(x) x.Equals(Heritage))

            If Not HeritageCultures.ContainsKey(HeritageIndex) Then
                HeritageCultures.Add(HeritageIndex, Cultures.Count - 1)
            Else
                HeritageCultures(HeritageIndex) &= $" {Cultures.Count - 1}"
            End If

            If Block.Contains("color") Then
                Dim Colour As String = Block.Split("color", 2).Last.Split("="c, 2).Last.Trim.Split({" "c, vbTab, vbCrLf, vbCr, vbLf}, 2, StringSplitOptions.None).First.TrimEnd
                If Not Colour.Replace(" "c, "").Replace("."c, "").All(AddressOf Char.IsDigit) Then
                    If NamedColours.ContainsKey(Colour) Then
                        Colour = NamedColours(Colour)
                    Else
                        Colour = "255 255 255"
                    End If
                End If
                If Colour.Contains("."c) Then
                    Dim Hold() As String = Colour.Split
                    For HeritageIndex = 0 To Hold.Length - 1
                        If Hold(HeritageIndex).Contains("."c) Then
                            Hold(HeritageIndex) *= 256
                            If Hold(HeritageIndex).Contains("."c) Then
                                Hold(HeritageIndex) = Hold(HeritageIndex).Split("."c)(0)
                            End If
                            If Hold(HeritageIndex) = 256 Then
                                Hold(HeritageIndex) -= 1
                            End If
                        End If
                    Next
                    Colour = String.Join(" "c, Hold)
                End If
                Colours.Add(Colour)
            Else
                Debug.Print($"{Cultures.Last} has no colour.")
                Colours.Add("255 255 255")
            End If

            If Block.Contains("ethos") Then
                Dim Ethos As String = Block.Split("ethos", 2).Last.Split("="c, 2).Last.Trim.Split({" "c, vbTab, vbCrLf, vbCr, vbLf}, 2, StringSplitOptions.None).First.TrimEnd
                Ethoses.Add(Ethos.Trim(vbTab))
            Else
                Debug.Print("No ethos found for: " & Culture)
                Ethoses.Add("")
            End If

            If Block.Contains("traditions") Then
                DeComment = Block.Split("traditions", 2).Last.Split({"{"c, "}"c}, 3)(1).Split({vbCrLf, vbTab}, StringSplitOptions.RemoveEmptyEntries).ToList
                For HeritageIndex = 0 To DeComment.Count - 1
                    If DeComment(HeritageIndex).TrimStart.StartsWith("#") Then
                        DeComment(HeritageIndex) = ""
                    ElseIf DeComment(HeritageIndex).Contains("#") Then
                        Do
                            If DeComment(HeritageIndex).Split("#").Contains(vbCrLf) Then
                                DeComment(HeritageIndex) = String.Concat({DeComment(HeritageIndex).Split("#"c, 2)(0), DeComment(HeritageIndex).Split("#"c, 2)(1).Split(vbCrLf, 2)(1)})
                            Else
                                DeComment(HeritageIndex) = DeComment(HeritageIndex).Split("#"c, 2)(0)
                            End If
                        Loop While DeComment(HeritageIndex).Contains("#")
                    End If
                Next
                DeComment.RemoveAll(Function(x) x.Trim.Length = 0)
                Dim Tradition As String = String.Join(" "c, DeComment)
                Traditions.Add(Tradition.Trim(vbTab))
            Else
                Debug.Print("No traditions found for: " & Culture)
                Traditions.Add("")
            End If

            If Block.Contains("language") Then
                Dim Language As String = Block.Split("language", 2).Last.Split("="c, 2).Last.Trim.Split({" "c, vbTab, vbCrLf, vbCr, vbLf}, 2, StringSplitOptions.None).First.TrimEnd
                Languages.Add(Language.Trim(vbTab))
            Else
                Debug.Print("No language found for: " & Culture)
                Languages.Add("")
            End If

            If Block.Contains("martial_custom") Then
                Dim MartialCustom As String = Block.Split("martial_custom", 2).Last.Split("="c, 2).Last.Trim.Split({" "c, vbTab, vbCrLf, vbCr, vbLf}, 2, StringSplitOptions.None).First.TrimEnd
                MartialCustoms.Add(MartialCustom.Trim(vbTab))
            Else
                Debug.Print("No martial custom found for: " & Culture)
                MartialCustoms.Add("")
            End If
        Next

        If File.Exists(BaseDir & "/descriptor.mod") AndAlso File.ReadAllText(BaseDir & "/descriptor.mod").Contains($"name={Chr(34)}Godherja: The Dying World{Chr(34)}") Then
            Dim Lines As List(Of String) = File.ReadAllLines($"{BaseDir}\common\scripted_guis\gh_culture_desc.txt").ToList
            For Each Culture In Cultures
                Dim CultureDescription As String = ""
                If Lines.Exists(Function(x) x.Contains($"culture:{Culture}") AndAlso Not x.Contains("#culture")) Then
                    CultureDescription = Lines.Find(Function(x) x.Contains($"culture:{Culture}"))
                    CultureDescription = CultureDescription.Split("custom_tooltip", 2)(1).Split({"="c, "}"c, " "c}, StringSplitOptions.RemoveEmptyEntries)(0)
                End If
                CultureDescriptions.Add(CultureDescription)
            Next
        End If

        Dim StartTime As DateTime = DateTime.Now

        CollectLocalisations()

        GetLocalisation(Heritages, "_name")
        GetLocalisation(Cultures)
        GetLocalisation(Ethoses, "_name")
        For Count = 0 To Traditions.Count - 1
            Dim Code As List(Of String) = Traditions(Count).Split(" "c).ToList
            GetLocalisation(Code, "_name")
            Traditions(Count) = $"* {String.Join(vbCrLf & "* ", Code)}"
        Next
        GetLocalisation(Languages, "_name")
        GetLocalisation(MartialCustoms, "_name")
        GetLocalisation(CultureDescriptions)

        Dim EndTime As DateTime = DateTime.Now
        Debug.Print(EndTime.Subtract(StartTime).TotalSeconds.ToString) 'This is to measure how long it took to collect and parse the localisations.

        Dim OutputFile As String 'Name the output text file. Use the name of the mod if possible.
        If File.Exists(BaseDir & "/descriptor.mod") Then
            OutputFile = File.ReadAllLines(BaseDir & "/descriptor.mod").ToList.Find(Function(x) x.StartsWith("name=")).Split(Chr(34), 3)(1)
            OutputFile = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{String.Concat(OutputFile.Split(Path.GetInvalidFileNameChars))} Cultures.txt"
        Else
            OutputFile = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\CK3Tools Cultures to MediaWiki.txt"
        End If

        Using SW As New StreamWriter(OutputFile)
            SW.WriteLine("{| class=""wikitable sortable""")
            If CultureDescriptions.Count = 0 Then
                SW.WriteLine("! Heritage !! Culture !! Ethos !! Traditions !! Language !! Martial Custom")
            Else
                SW.WriteLine("! Heritage !! Culture !! Ethos !! Traditions !! Language !! Martial Custom !! Description")
            End If
            For Each Heritage In HeritageCultures.Keys
                Dim CultureList As List(Of String) = HeritageCultures(Heritage).Split().ToList
                SW.WriteLine($"|-{vbCrLf}| rowspan=""{CultureList.Count}"" | '''{Heritages(Heritage)}'''")
                For Count = 0 To CultureList.Count - 1
                    Dim Culture As Integer = CultureList(Count)
                    If CultureDescriptions.Count = 0 Then
                        CultureList(Count) = $"!style=""background:rgb({String.Join(", ", Colours(Culture).Split)})""| {Cultures(Culture)}{vbCrLf}| {Ethoses(Culture)}{vbCrLf}| {vbCrLf}{Traditions(Culture)}{vbCrLf}| {Languages(Culture)}{vbCrLf}| {MartialCustoms(Culture)}"
                    Else
                        CultureList(Count) = $"!style=""background:rgb({String.Join(", ", Colours(Culture).Split)})""| {Cultures(Culture)}{vbCrLf}| {Ethoses(Culture)}{vbCrLf}| {vbCrLf}{Traditions(Culture)}{vbCrLf}| {Languages(Culture)}{vbCrLf}| {MartialCustoms(Culture)}{vbCrLf}| {String.Join(vbCrLf, CultureDescriptions(Culture).Replace("/n", vbCrLf).Replace("\n", vbCrLf).Replace("\", "").Split(vbCrLf, StringSplitOptions.TrimEntries))}"
                    End If
                Next
                SW.WriteLine(String.Join($"{vbCrLf}|-{vbCrLf}", CultureList))
            Next
            SW.WriteLine("|}")
        End Using

        Console.WriteLine("Successfully deposited output to desktop. Press any key to close.")
        Console.ReadKey(True)
    End Sub
    Sub CollectLocalisations()
        Dim RawGameConceptLocalisations As New Dictionary(Of String, String)

        Dim BaseFiles As List(Of String) = Directory.GetFiles(GameDir & "\localization\english", "*.yml", SearchOption.AllDirectories).ToList
        For Each TextFile In BaseFiles
            SaveLocs(TextFile, RawGameConceptLocalisations)
        Next
        If Directory.Exists(BaseDir & "\localization\english") Then
            LocalisationFiles = Directory.GetFiles(BaseDir & "\localization\english", "*.yml", SearchOption.AllDirectories).ToList
        Else
            Console.WriteLine("Sorry, non-English localisation not currently supported. Press any key to exit.")
            Console.ReadKey()
            Exit Sub
        End If
        If Directory.Exists(BaseDir & "\localization\replace\english") Then
            LocalisationFiles = LocalisationFiles.Concat(Directory.GetFiles(BaseDir & "\localization\replace\english", "*.yml", SearchOption.AllDirectories)).ToList
        End If

        For Each Textfile In LocalisationFiles
            SaveLocs(Textfile, RawGameConceptLocalisations)
        Next

        For Each Item In RawGameConceptLocalisations.Keys
            If Not GameConceptLocalisations.Contains(Item) Then
                GameConceptLocalisations.Add(Item, DeFormat(DeComment(RawGameConceptLocalisations(Item).Split(Chr(34), 2).Last).TrimEnd.TrimEnd(Chr(34))).TrimEnd)
            Else
                GameConceptLocalisations(Item) = DeFormat(DeComment(RawGameConceptLocalisations(Item).Split(Chr(34), 2).Last).TrimEnd.TrimEnd(Chr(34))).TrimEnd
            End If
        Next
        For Count = 0 To GameConceptLocalisations.Count - 1
            If GameConceptLocalisations.Values(Count).Contains("$"c) Then
                Dim Key As String = GameConceptLocalisations.Keys(Count)
                GameConceptLocalisations(Key) = DeReference(GameConceptLocalisations.Values(Count))
            End If
        Next
    End Sub
    Private Sub GetLocalisation(ByRef Code As List(Of String), Optional Suffix As String = "")
        For Count = 0 To Code.Count - 1
            If Not Code(Count) = "" AndAlso Not Code(Count).TrimStart.StartsWith("game_concept") Then
                Dim RawCode As String = Code(Count) & Suffix 'Modify the code if the object id has a suffix in the loc code.
                If SavedLocalisation.Contains(RawCode) Then 'If the locs stored to dictionary contain this loc then...
                    Code(Count) = SavedLocalisation(RawCode)

                    'Process the loc for internal code.

                    If Code(Count).Split(Chr(34)).Last.Contains("#") Then
                        Code(Count) = DeComment(Code(Count)) 'Remove comments if any.
                    End If
                    Code(Count) = Code(Count).Split(Chr(34), 2).Last.TrimEnd.TrimEnd(Chr(34))
                    If Code(Count).Contains("#"c) Then
                        Code(Count) = DeFormat(Code(Count)) 'Remove style formatting if any.
                    End If
                    If Code(Count).Contains("|E]") OrElse Code(Count).Contains("|e]") Then
                        Code(Count) = DeConcept(Code(Count)) 'Find the appropriate locs for any game concepts referred.
                    End If
                    If Code(Count).Contains("$") Then
                        Code(Count) = DeReference(Code(Count)) 'Find the appropriate locs for any other locs referred.
                    End If
                ElseIf GameConceptLocalisations.Contains(RawCode) Then 'If game concept locs stored to dictionary contain this loc then...
                    Code(Count) = GameConceptLocalisations(RawCode) 'Get the loc from the game concept dictionary.
                Else 'If the locs stored to memory or the game concept locs stored to memory don't contain this loc then...
                    Code(Count) = RawCode 'Write down the code without any localisation.
                End If
            ElseIf Code(Count).TrimStart.StartsWith("game_concept") AndAlso GameConceptLocalisations.Contains(Code(Count).Split("game_concept_", 2).Last.Split(":"c, 2).First) Then 'If the loc starts with game_concept then look for it in the game concept dictionary.
                Code(Count) = GameConceptLocalisations(Code(Count).Split("game_concept_").Last)
            End If
        Next
    End Sub
    Sub SetGameDir()
        If BaseDir.Contains("steamapps") Then
            GameDir = BaseDir.Split("steamapps", 2).First & "steamapps\common\Crusader Kings III\game\"
        Else
            GameDirPrompt()
        End If
    End Sub
    Sub GameDirPrompt()
        Dim DirFound As Boolean = False
        Do
            Console.WriteLine("Please enter your Crusader Kings 3 installation's root directory.")
            GameDir = Console.ReadLine
            GameDir = Path.TrimEndingDirectorySeparator(GameDir)
            If Directory.GetDirectories(GameDir, SearchOption.TopDirectoryOnly).ToList.Exists(Function(x) x.Contains("binaries")) Then
                GameDir &= Path.DirectorySeparatorChar & "binaries" & Path.DirectorySeparatorChar
            End If
            If Directory.GetFiles(GameDir, SearchOption.AllDirectories).ToList.Contains(GameDir & "ck3.exe") Then
                DirFound = True
            End If
        Loop While DirFound = False

        Using SW As New StreamWriter(Path.GetTempPath() & Path.DirectorySeparatorChar & "CK3Tools.txt", False)
            SW.WriteLine(GameDir)
        End Using
    End Sub
    Sub SaveLocs(TextFile As String, RawGameConceptLocalisations As Dictionary(Of String, String))
        Using SR As New StreamReader(TextFile)
            Dim LineData As String
            While Not SR.EndOfStream
                LineData = SR.ReadLine
                If Not LineData.TrimStart.StartsWith("#"c) AndAlso LineData.Contains(":"c) AndAlso Not LineData.Split(":"c, 2).Last.Length = 0 Then

                    Dim Key As String = LineData.TrimStart.Split(":"c, 2).First
                    Dim Value As String = LineData.Split(":"c, 2).Last.Substring(1).TrimStart

                    If Not SavedLocalisation.Contains(Key) Then
                        SavedLocalisation.Add(Key, Value)
                    Else
                        SavedLocalisation(Key) = Value
                    End If
                End If
                If LineData.TrimStart.StartsWith("game_concept") Then
                    Dim Key As String = LineData.TrimStart.Split(":"c, 2).First.Split("game_concept_", 2).Last
                    Dim Value As String = LineData.Split(":"c, 2).Last.Substring(1).TrimStart

                    If Not RawGameConceptLocalisations.ContainsKey(Key) Then
                        RawGameConceptLocalisations.Add(Key, Value)
                    Else
                        RawGameConceptLocalisations(Key) = Value
                    End If
                End If
            End While
        End Using
    End Sub
    Function DeComment(Input As String) As String
        Dim Output As List(Of String) = Input.Split(Chr(34)).ToList 'Find the boundaries of the actual loc code by splitting it up according to its quotation marks.
        Output(Output.Count - 1) = Output(Output.Count - 1).Split("#"c).First 'Take the last part of the split input, and split it off from the comment.
        Return String.Join(Chr(34), Output).TrimEnd 'Rejoin the input with quotation marks and return it.
    End Function
    Function DeConcept(Input As String) As String
        If Input.Contains("]"c) AndAlso Input.Split("]"c, 2).First.Contains("["c) AndAlso Input.Split("]"c, 2).First.Split("["c, 2).Last.Contains("|"c) Then
            Dim GameConcepts As New SortedList(Of String, String) 'Collect each game concept contained in string here.
            Do While Input.Contains("]"c) AndAlso Input.Split("]"c, 2).First.Contains("["c) AndAlso Input.Split("]"c, 2).First.Split("["c, 2).Last.Contains("|"c) 'Loop while input loc string contains any non-parsed game concepts.
                Dim GameConcept As String = Input.Split("["c, 2).Last.Split("|"c, 2).First 'Get the game concept object id.
                Dim Suffix As String = "|"c & Input.Split("|"c, 2).Last.Split("]"c, 2).First & "]"c
                If Not GameConcepts.ContainsKey(GameConcept) Then 'If it has not already been collected then...
                    Dim ReplaceString As String
                    If GameConceptLocalisations.Contains(GameConcept.ToLower) Then 'Find its loc in the SortedList.
                        ReplaceString = GameConceptLocalisations(GameConcept.ToLower)
                    Else
                        ReplaceString = GameConcept 'If it cannot be found then assign the replace string to be the raw code.
                    End If
                    GameConcepts.Add(GameConcept, ReplaceString) 'Add it to the sortedlist and find the rest of the game concepts in this loc string.
                    Input = Input.Replace($"[{GameConcept}" & Suffix, ReplaceString) 'Remove it from the input string so it is not reparsed into the SortedList.
                Else 'If it has already been collected...
                    'Input = String.Concat(Input.Split({"[", "|E]"}, 3, StringSplitOptions.None)) 'Remove it from the input string.
                    Input = Input.Replace($"[{GameConcept}" & Suffix, GameConcepts(GameConcept))
                End If
            Loop

            Return Input 'Return loc.
        Else
            Return Input 'Redundancy in case a loc was falsely found to contain a game concept.
        End If
    End Function
    Function DeReference(Input As String) As String
        If Input.Contains("$"c) Then
            Dim Output As String = Input
            Dim Locs As New List(Of String)
            Do
                If Not Locs.Contains(Input.Split("$", 3)(1)) Then
                    Locs.Add(Input.Split("$", 3)(1))
                End If
                Input = Input.Split("$", 3).Last
            Loop While Input.Contains("$"c)
            Dim Code As List(Of String) = Locs.ToList
            GetLocalisation(Locs)
            For Count = 0 To Code.Count - 1
                Output = Output.Replace($"${Code(Count)}$", Locs(Count))
            Next
            Return Output
        Else
            Return Input
        End If
    End Function
    Function DeFormat(Input As String) As String
        If Input.Contains("#"c) Then
            Dim Output As String = Input
            Do
                Dim FormattedLoc As String = Output.Split("#"c, 2).Last 'Find the styled part of the loc and extract it.
                Dim DeFormatted As String
                With FormattedLoc
                    Dim CloserIndex As Integer
                    If .Contains("#!") Then
                        CloserIndex = .IndexOf("#!") + 2
                        DeFormatted = .Substring(0, CloserIndex - 2).Split(" "c, 2).Last
                    ElseIf .Contains("#"c) Then
                        CloserIndex = .IndexOf("#"c) + 1
                        DeFormatted = .Substring(0, CloserIndex - 1).Split(" "c, 2).Last
                    Else
                        CloserIndex = .Length
                        DeFormatted = FormattedLoc.Split(" "c, 2).Last
                    End If
                    FormattedLoc = "#" & .Substring(0, CloserIndex)
                End With
                'Remove the style code and store it in a string.

                Output = Output.Replace(FormattedLoc, DeFormatted) 'Use replace function to replace the formatted part of the loc with the deformatted string.
            Loop While Output.Contains("#"c) 'Loop if there are more.
            Return Output 'Return when there are no more.
        Else
            Return Input
        End If
    End Function
    Function DeNest(Input As String) As List(Of String)
        Dim Output As New List(Of String)
        Input = String.Join(vbCrLf, Input.Split({vbCrLf, vbCr, vbLf}, StringSplitOptions.None).ToList.FindAll(Function(x) Not x.TrimStart.StartsWith("#"c)))
        If Input.Contains("="c) AndAlso Input.Contains("{"c) Then
            Do
                Dim RawCodeID As String = Input.Split("{", 2).First 'Get the code id of the object the block is assigned to.
                Input = Input.Substring(RawCodeID.Length - 1) 'Split off the extracted data.
                Dim RawCodeBlock As String = Input
                RawCodeID = RawCodeID.Split({vbCrLf, vbCr, vbLf}, StringSplitOptions.None).Last
                Do While RawCodeBlock.Split("}"c, 2)(0).Contains("{"c) 'Designate subsidiary objects designated with curly brackets as such by replacing their {} with <>.
                    RawCodeBlock = String.Join(">"c, String.Join("<"c, RawCodeBlock.Split("{"c, 2)).Split("}"c, 2))
                Loop  'Loop until no more subsidiary objects.
                'End If
                RawCodeBlock = RawCodeBlock.Split("}"c)(0).Replace("<", "{").Replace(">", "}") & "}" 'Get the data of this object by splitting it off of the overall code after its own { closing bracket.
                If RawCodeID.Contains("="c) Then
                    Output.Add(String.Join("{", {RawCodeID, RawCodeBlock})) 'Add to List
                End If
                If Input.Length > RawCodeBlock.Length Then 'Split off the extracted code block.
                    Input = Input.Substring(RawCodeBlock.Length - 1)
                Else
                    Input = ""
                End If
            Loop While Input.Split("}", 2)(0).Contains("{"c) 'Continue to parse the data until no more faiths can be found by looking for a { starting bracket.
        End If
        Return Output
    End Function
End Module
