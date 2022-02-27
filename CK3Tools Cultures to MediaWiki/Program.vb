Imports System.IO
Friend Module Props
    'Property BaseDir As String = "D:\Programs\Steam\steamapps\workshop\content\1158310\2326030123"
    Property BaseDir As String = Environment.CurrentDirectory
End Module
Module Program
#Disable Warning IDE0044 ' Add readonly modifier
    Dim GameConceptLocalisations As New SortedList(Of String, String)
#Enable Warning IDE0044 ' Add readonly modifier
    Sub Main()

        Dim FileList As List(Of String) = Directory.GetFiles(BaseDir & "\common\culture\pillars", "*.txt", SearchOption.AllDirectories).ToList
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
            Dim Text() As String = File.ReadAllText(TextFile).Split(vbCrLf & "}" & vbCrLf)
            For Each Block In Text
                If Block.Contains("type") AndAlso Block.Split("type", 2)(1).Split(vbCrLf, 2)(0).Contains("heritage") Then
                    Dim Hold() As String = {Block.Split("{"c, 2)(0).Trim}
                    Dim Heritage As String
                    If Hold(0).Contains("="c) Then
                        Heritage = Hold(0).Split("="c, 2)(0).Trim
                    Else
                        Hold = Hold(0).Split
                        Heritage = Hold(Hold.Length - 2)
                    End If
                    If Heritage.Contains(vbCrLf) Then
                        Heritage = Heritage.Split(vbCrLf).Last
                    End If
                    Heritages.Add(Heritage)
                End If
            Next
        Next

        Heritages.Sort()

        Dim CultureFiles As List(Of String) = Directory.GetFiles(BaseDir & "\common\culture\cultures", "*.txt", SearchOption.AllDirectories).ToList
        Dim RawCultures As New List(Of String)
        For Each TextFile In CultureFiles
            Dim Text As List(Of String) = File.ReadAllText(TextFile).Split(vbCrLf & "}", StringSplitOptions.RemoveEmptyEntries).ToList
            For TextCount = 0 To Text.Count - 1
                If Text(TextCount).Contains("#"c) Then
                    Dim Hold As List(Of String) = Text(TextCount).Split(vbCrLf).ToList
                    Hold.RemoveAll(Function(x) x.TrimStart.StartsWith("#"c))
                    Text(TextCount) = String.Join(vbCrLf, Hold)
                End If
            Next
            Text.RemoveAll(Function(x) Not x.Contains("}"c))
            RawCultures = RawCultures.Concat(Text).ToList
        Next

        Dim Cultures, Colours, Ethoses, Traditions, Languages, MartialCustoms, CultureDescriptions As New List(Of String)
        Dim HeritageCultures As New SortedList(Of Integer, String)

        For Each Block In RawCultures
            Dim Culture As String = Block.Split({" ", "=", "{"}, StringSplitOptions.RemoveEmptyEntries)(0).Trim
            If Culture.Contains(vbCrLf) Then
                Culture = Culture.Split(vbCrLf).Last
            End If
            Cultures.Add(Culture.Trim(vbTab))

            Dim Heritage As String = Block.Split("heritage", 2)(1).Split("="c, 2)(1).Split(vbCrLf, 2)(0).Trim
            Dim Count As Integer = Heritages.FindIndex(Function(x) x.Equals(Heritage))
            If Not HeritageCultures.ContainsKey(Count) Then
                HeritageCultures.Add(Count, Cultures.Count - 1)
            Else
                HeritageCultures(Count) &= $" {Cultures.Count - 1}"
            End If

            If Block.Contains("color") Then
                Dim Colour As String = Block.Split("color", 2)(1).Split("{"c, 2)(1).Split("}"c)(0).Trim
                If Colour.Contains("."c) Then
                    Dim Hold() As String = Colour.Split
                    For Count = 0 To Hold.Length - 1
                        If Hold(Count).Contains("."c) Then
                            Hold(Count) *= 256
                            If Hold(Count).Contains("."c) Then
                                Hold(Count) = Hold(Count).Split("."c)(0)
                            End If
                            If Hold(Count) = 256 Then
                                Hold(Count) -= 1
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

            Dim Ethos As String = Block.Split("ethos", 2)(1).Split("="c, 2)(1).Split(vbCrLf, 2)(0).Trim
            Ethoses.Add(Ethos.Trim(vbTab))


            Dim DeComment As List(Of String) = Block.Split("traditions")(1).Split({"{"c, "}"c}, 3)(1).Split({vbCrLf, vbTab}, StringSplitOptions.RemoveEmptyEntries).ToList
            For Count = 0 To DeComment.Count - 1
                If DeComment(Count).TrimStart.StartsWith("#") Then
                    DeComment(Count) = ""
                ElseIf DeComment(Count).Contains("#") Then
                    Do
                        If DeComment(Count).Split("#").Contains(vbCrLf) Then
                            DeComment(Count) = String.Concat({DeComment(Count).Split("#"c, 2)(0), DeComment(Count).Split("#"c, 2)(1).Split(vbCrLf, 2)(1)})
                        Else
                            DeComment(Count) = DeComment(Count).Split("#"c, 2)(0)
                        End If
                    Loop While DeComment(Count).Contains("#")
                End If
            Next
            DeComment.RemoveAll(Function(x) x.Trim.Equals(""))
            Dim Tradition As String = String.Join(" "c, DeComment)
            Traditions.Add(Tradition.Trim(vbTab))

            Dim Language As String = Block.Split("language", 2)(1).Split("="c, 2)(1).Split(vbCrLf, 2)(0).Trim
            Languages.Add(Language.Trim(vbTab))

            Dim MartialCustom As String = Block.Split("martial_custom", 2)(1).Split("="c, 2)(1).Split(vbCrLf, 2)(0).Trim
            MartialCustoms.Add(MartialCustom.Trim(vbTab))
        Next

        If File.ReadAllText(BaseDir & "/descriptor.mod").Contains($"name={Chr(34)}Godherja: The Dying World{Chr(34)}") Then
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

        Dim LocalisationFiles As List(Of String) = Directory.GetFiles(BaseDir & "\localization\english", "*.yml", SearchOption.AllDirectories).Concat(Directory.GetFiles(BaseDir & "\localization\replace\english", "*.yml", SearchOption.AllDirectories)).ToList

        Dim RawGameConceptLocalisations As New List(Of String)
        For Each TextFile In LocalisationFiles
            Using SR As New StreamReader(TextFile)
                Dim LineData As String
                While Not SR.EndOfStream
                    LineData = SR.ReadLine
                    If LineData Like "*game_concept*" AndAlso Not LineData Like "*$game_concept*" Then
                        RawGameConceptLocalisations = RawGameConceptLocalisations.Concat(File.ReadAllLines(TextFile)).ToList
                        Exit While
                    End If
                End While
            End Using
        Next

        For Each Item In RawGameConceptLocalisations
            If Item Like "*game_concept*" Then
                Dim GameConcept As String = Item.Split(":")(0).Split("game_concept_")(1)
                If Not GameConceptLocalisations.ContainsKey(GameConcept) Then
                    GameConceptLocalisations.Add(Item.Split(":")(0).Split("game_concept_")(1), Item.Split(Chr(34))(1))
                Else
                    GameConceptLocalisations(GameConcept) = Item.Split(Chr(34))(1)
                End If
            End If
        Next

        Dim RawLocalisation As List(Of String) = VanillaLoc()

        GetLocalisation(Heritages, RawLocalisation, LocalisationFiles, "_name")
        GetLocalisation(Cultures, RawLocalisation, LocalisationFiles)
        GetLocalisation(Ethoses, RawLocalisation, LocalisationFiles, "_name")
        GetTraditionLocalisation(Traditions, RawLocalisation, LocalisationFiles, "_name")
        GetLocalisation(Languages, RawLocalisation, LocalisationFiles, "_name")
        GetLocalisation(MartialCustoms, RawLocalisation, LocalisationFiles, "_name")
        GetLocalisation(CultureDescriptions, RawLocalisation, LocalisationFiles)

        Dim OutputFile As String = File.ReadAllLines(BaseDir & "/descriptor.mod").ToList.Find(Function(x) x.StartsWith("name=")).Split(Chr(34), 3)(1)
        OutputFile = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{String.Concat(OutputFile.Split(Path.GetInvalidFileNameChars))} Cultures.txt"
        Using SW As New StreamWriter(OutputFile)
            SW.WriteLine("{| class=""wikitable sortable""")
            If CultureDescriptions.Count = 0 Then
                SW.WriteLine("! Heritage !! Culture !! Ethos !! Traditions !! Language !! Martial Custom")
            Else
                SW.WriteLine("! Heritage !! Culture !! Ethos !! Traditions !! Language !! Martial Custom !! Description")
            End If
            For Each Heritage In HeritageCultures.Keys
                Dim Value As String
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
        Console.ReadKey()
    End Sub
    Private Sub GetLocalisation(ByRef Code As List(Of String), ByRef RawLocalisation As List(Of String), ByRef LocalisationFiles As List(Of String), Optional Suffix As String = "")
        For Count = 0 To Code.Count - 1
            If Not Code(Count) = "" Then
                Dim RawCode As String = Code(Count) & Suffix
                If Not RawLocalisation.Exists(Function(x) x.TrimStart.StartsWith($"{RawCode}:")) Then
                    For Each TextFile In LocalisationFiles
                        Using SR As New StreamReader(TextFile)
                            While Not SR.EndOfStream
                                Dim LineData As String = SR.ReadLine
                                If LineData.TrimStart.StartsWith($"{RawCode}:") Then
                                    RawLocalisation = RawLocalisation.Concat(File.ReadAllLines(TextFile)).ToList
                                    Code(Count) = LineData.Split(Chr(34), 3)(1)
                                    If Code(Count).Contains("#"c) Then
                                        Code(Count) = DeFormat(Code(Count))
                                    End If
                                    If Code(Count).Contains("|E]") Then
                                        Code(Count) = DeConcept(Code(Count))
                                    End If
                                    Exit For
                                End If
                            End While
                        End Using
                    Next
                Else
                    Code(Count) = RawLocalisation.FindLast(Function(x) x.TrimStart.StartsWith($"{RawCode}:")).Split(Chr(34), 3)(1)
                    If Code(Count).Contains("#"c) Then
                        Code(Count) = DeFormat(Code(Count))
                    End If
                    If Code(Count).Contains("|E]") Then
                        Code(Count) = DeConcept(Code(Count))
                    End If
                    If Code(Count).Contains("$") Then
                        Dim RawLoc As List(Of String) = {Code(Count)}.ToList
                        Do
                            RawLoc(0) = RawLoc(0).Split("$"c, 2).Last
                            Code(Count) = Code(Count).Replace($"${Code(Count).Split("$"c, 3)(1)}$", RawLoc(0))
                            GetLocalisation(RawLoc, RawLocalisation, LocalisationFiles)
                        Loop While RawLoc(0).Contains("$")
                    End If
                End If
            End If
        Next
    End Sub
    Private Sub GetTraditionLocalisation(ByRef Code As List(Of String), ByRef RawLocalisation As List(Of String), ByRef LocalisationFiles As List(Of String), Optional Suffix As String = "")
        For Count = 0 To Code.Count - 1
            Dim Traditions As List(Of String)
            If Code(Count).Contains(" "c) Then
                Traditions = Code(Count).Split.ToList
            Else
                Traditions = {Code(Count)}.ToList
            End If
            For TraditionCount = 0 To Traditions.Count - 1
                Dim RawCode As String = Traditions(TraditionCount) & Suffix
                If Not RawLocalisation.Exists(Function(x) x.TrimStart.StartsWith($"{RawCode}:")) Then
                    For Each TextFile In LocalisationFiles
                        Using SR As New StreamReader(TextFile)
                            While Not SR.EndOfStream
                                Dim LineData As String = SR.ReadLine
                                If LineData.TrimStart.ToLower.StartsWith($"{RawCode}:") Then
                                    RawLocalisation = RawLocalisation.Concat(File.ReadAllLines(TextFile)).ToList
                                    Traditions(TraditionCount) = LineData.Split(Chr(34), 3)(1)
                                    Exit For
                                End If
                            End While
                        End Using
                    Next
                Else
                    Traditions(TraditionCount) = RawLocalisation.FindLast(Function(x) x.TrimStart.StartsWith($"{RawCode}:")).Split(Chr(34), 3)(1)
                End If
            Next
            Code(Count) = "* " & String.Join(vbCrLf & "* ", Traditions)
            If Code(Count).Contains("#"c) Then
                Code(Count) = DeFormat(Code(Count))
            End If
            If Code(Count).Contains("|E]") Then
                Code(Count) = DeConcept(Code(Count))
            End If
            If Code(Count).Contains("$") Then
                Dim RawLoc As List(Of String) = {Code(Count)}.ToList
                Do
                    RawLoc(0) = RawLoc(0).Split("$"c, 2).Last
                    Code(Count) = Code(Count).Replace($"${Code(Count).Split("$"c, 3)(1)}$", RawLoc(0))
                    GetLocalisation(RawLoc, RawLocalisation, LocalisationFiles)
                Loop While RawLoc(0).Contains("$")
            End If
        Next
    End Sub
    Function VanillaLoc() As List(Of String)
        Return New List(Of String)({
" ethos_bellicose_name:0 ""Bellicose""",
" ethos_stoic_name:0 ""Stoic""",
" ethos_bureaucratic_name:1 ""Bureaucratic""",
" ethos_spiritual_name:0 ""Spiritual""",
" ethos_courtly_name:0 ""Courtly""",
" ethos_egalitarian_name:0 ""Egalitarian""",
" ethos_communal_name:0 ""Communal""",
" martial_custom_equal_name:0 ""Equal""",
" martial_custom_male_only_name:0 ""Men Only""",
" martial_custom_female_only_name:0 ""Women Only""",
" tradition_court_eunuchs_name:0 ""Court Eunuchs""",
" tradition_byzantine_succession_name:1 ""Byzantine Traditions""",
" tradition_african_tolerance_name:0 ""African Tolerance""",
" tradition_memories_of_bactria_name:0 ""Memories of Bactria""",
" tradition_equal_inheritance_name:0 ""Equal Inheritance""",
" tradition_the_witenagemot_name:0 ""The Witenagemot""",
" tradition_things_name:1 ""Ting-Meet""",
" tradition_caravaneers_name:0 ""Caravaneers""",
" tradition_xenophilic_name:0 ""Xenophilic""",
" tradition_legalistic_name:0 ""Legalistic""",
" tradition_warrior_culture_name:0 ""Warrior Culture""",
" tradition_philosopher_culture_name:0 ""Philosopher Culture""",
" tradition_chivalry_name:0 ""Chivalry""",
" tradition_hit_and_run_name:0 ""Hit-and-Run Tacticians""",
" tradition_stand_and_fight_name:0 ""Stand and Fight!""",
" tradition_horse_lords_name:0 ""Horse Lords""",
" tradition_adaptive_skirmishing_name:0 ""Adaptive Skirmishers""",
" tradition_formation_fighting_name:0 ""Formation Fighting Experts""",
" tradition_republican_legacy_name:0 ""Republican Legacy""",
" tradition_hereditary_hierarchy_name:0 ""Hereditary Hierarchy""",
" tradition_theocratic_autonomy_name:0 ""Theocratic Autonomy""",
" tradition_saharan_nomads_name:0 ""Saharan Nomads""",
" tradition_himalayan_settlers_name:0 ""Himalayan Settlers""",
" tradition_esteemed_hospitality_name:0 ""Esteemed Hospitality""",
" tradition_hard_working_name:0 ""Industrious""",
" tradition_loyal_soldiers_name:1 ""Loyal Subjects""",
" tradition_female_only_inheritance_name:0 ""Matriarchal""",
" tradition_pacifism_name:0 ""Pacifists""",
" tradition_scientific_curiosity_name:0 ""Scientific Curiosity""",
" tradition_spartan_name:0 ""Spartan""",
" tradition_tribe_unity_name:0 ""Tribal Unity""",
" tradition_astute_diplomats_name:0 ""Astute Diplomats""",
" tradition_collective_lands_name:0 ""Collective Lands""",
" tradition_horse_breeder_name:0 ""Horse Breeders""",
" tradition_hunters_name:0 ""Prolific Hunters""",
" tradition_sacred_mountains_name:0 ""Sacred Mountains""",
" tradition_culinary_art_name:0 ""Culinary Artists""",
" tradition_festivities_name:0 ""Frequent Festivities""",
" tradition_tea_ceremony_name:0 ""Tea Ceremonies""",
" tradition_vegetarianism_name:0 ""Vegetarians""",
" tradition_alpine_supremacy_name:0 ""Alpine Supremacy""",
" tradition_seafaring_name:0 ""Seafarers""",
" tradition_strength_display_name:0 ""Displays of Strength""",
" tradition_mystical_ancestors_name:0 ""Mystical Ancestors""",
" tradition_priestly_caste_name:0 ""Priestly Caste""",
" tradition_religion_blending_name:0 ""Religion Blending""",
" tradition_religious_festivities_name:0 ""Religious Festivities""",
" tradition_religious_patronage_name:0 ""Religious Patronage""",
" tradition_medicinal_plants_name:0 ""Medicinal Herbalists""",
" tradition_storytellers_name:0 ""Storytellers""",
" tradition_sacred_hunts_name:0 ""Sacred Hunts""",
" tradition_wedding_ceremonies_name:1 ""Marital Ceremonies""",
" tradition_music_theory_name:0 ""Musical Theorists""",
" tradition_poetry_name:0 ""Refined Poetry""",
" tradition_culture_blending_name:0 ""Culture Blending""",
" tradition_family_entrepreneurship_name:0 ""Family Business""",
" tradition_fishermen_name:0 ""Dexterous Fishermen""",
" tradition_metal_craftsmanship_name:0 ""Metalworkers""",
" tradition_isolationist_name:0 ""Isolationist""",
" tradition_winter_warriors_name:0 ""Winter Warriors""",
" tradition_forest_fighters_name:0 ""Forest Fighters""",
" tradition_mountaineers_name:0 ""Mountaineers""",
" tradition_warriors_of_the_dry_name:0 ""Warriors of the Dry""",
" tradition_highland_warriors_name:0 ""Highland Warriors""",
" tradition_jungle_warriors_name:0 ""Jungle Warriors""",
" tradition_only_the_strong_name:0 ""Only the Strong""",
" tradition_warriors_by_merit_name:0 ""Warriors by Merit""",
" tradition_warrior_monks_name:0 ""Warrior Priests""",
" tradition_talent_acquisition_name:0 ""Recognition of Talent""",
" tradition_strength_in_numbers_name:0 ""Strength in Numbers""",
" tradition_frugal_armorsmiths_name:0 ""Frugal Armorers""",
" tradition_malleable_invaders_name:0 ""Malleable Invaders""",
" tradition_quarrelsome_name:0 ""Quarrelsome""",
" tradition_swords_for_hire_name:0 ""Swords for Hire""",
" tradition_reverence_for_veterans_name:0 ""Reverence for Veterans""",
" tradition_stalwart_defenders_name:0 ""Stalwart Defenders""",
" tradition_battlefield_looters_name:0 ""Battlefield Looters""",
" tradition_fervent_temple_builders_name:0 ""Fervent Temple Builders""",
" tradition_lords_of_the_elephant_name:0 ""Lords of the Elephant""",
" tradition_zealous_people_name:0 ""Strong Believers""",
" tradition_welcoming_name:1 ""Charismatic""",
" tradition_agrarian_name:0 ""Agrarian""",
" tradition_eye_for_an_eye_name:0 ""Eye for an Eye""",
" tradition_forbearing_name:0 ""Forbearing""",
" tradition_equitable_name:0 ""Equitable""",
" tradition_charitable_name:0 ""Charitable""",
" tradition_modest_name:0 ""Modest""",
" tradition_hill_dwellers_name:0 ""Hill Dwellers""",
" tradition_forest_folk_name:0 ""Forest Folk""",
" tradition_mountain_homes_name:0 ""Mountain Homes""",
" tradition_dryland_dwellers_name:0 ""Dryland Dwellers""",
" tradition_jungle_dwellers_name:0 ""Jungle Dwellers""",
" tradition_faith_bound_name:0 ""Bound by Faith""",
" tradition_by_the_sword_name:0 ""By the Sword""",
" tradition_language_scholars_name:0 ""Linguists""",
" tradition_pastoralists_name:0 ""Pastorialists""",
" tradition_desert_nomads_name:1 ""Desert Travelers""",
" tradition_gardening_name:0 ""Garden Architects""",
" tradition_monogamous_name:0 ""Monogamous""",
" tradition_polygamous_name:0 ""Polygamous""",
" tradition_concubines_name:1 ""Concubines""",
" tradition_mendicant_mystics_name:0 ""Mendicant Mystics""",
" tradition_parochialism_name:0 ""Parochialism""",
" tradition_martial_admiration_name:0 ""Martial Admiration""",
" tradition_chanson_de_geste_name:0 ""Chanson de Geste""",
" tradition_ruling_caste_name:0 ""Ruling Caste""",
" tradition_staunch_traditionalists_name:0 ""Staunch Traditionalists""",
" tradition_sacred_groves_name:0 ""Sacred Groves""",
" tradition_hold_the_line_name:0 ""Hold the Line""",
" tradition_legendary_noble_families_name:0 ""Legendary Noble Families""",
" tradition_castle_keepers_name:0 ""Castle Keepers""",
" tradition_city_keepers_name:0 ""City Keepers""",
" tradition_fractious_name:0 ""Fractious""",
" tradition_runestones_name:1 ""Runestone Raisers""",
" tradition_insular_spirit_name:0 ""Insular Spirit""",
" tradition_monastic_communities_name:0 ""Monastic Communities""",
" tradition_roman_legacy_name:1 ""Eastern Roman Legacy""",
" tradition_longbow_competitions_name:0 ""Longbow Competitions""",
" tradition_illyrian_grit_name:0 ""Illyrian Grit""",
" tradition_maritime_mercantilism_name:0 ""Maritime Mercantilism""",
" tradition_sacral_kingship_name:0 ""Sacral Kingship""",
" tradition_reavers_name:0 ""Reavers""",
" tradition_practiced_pirates_name:0 ""Practiced Pirates""",
" tradition_determined_independence_name:0 ""Determined Independence""",
" tradition_merciful_blindings_name:0 ""Merciful Blindings""",
" tradition_reindeer_hunters_name:0 ""Reindeer Hunters""",
" tradition_mountaineer_ruralism_name:0 ""Mountaineer Ruralism""",
" tradition_life_is_just_a_joke_name:0 ""Life is just a Joke""",
" tradition_steppe_tolerance_name:0 ""Steppe Tolerance""",
" tradition_nubian_warrior_queens_name:0 ""Warrior Queens""",
" tradition_maritime_mangroves_name:0 ""Maritime Mangroves""",
" tradition_mixed_governance_name:0 ""Mixed Governance""",
" tradition_ritual_scarrification_name:0 ""Ritual Scarification""",
" tradition_hidden_cities_name:0 ""Hidden Cities""",
" tradition_hereditary_bards_name:0 ""Hereditary Bards""",
" tradition_ancient_miners_name:0 ""Ancient Miners""",
" tradition_wetlanders_name:0 ""Wetlanders""",
" tradition_diasporic_name:0 ""Diasporic""",
" tradition_sorcerous_metallurgy_name:0 ""Sorcerous Metallurgy""",
" tradition_polders_name:0 ""Polders""",
" tradition_caucasian_wolves_name:0 ""Caucasian Wolves""",
" tradition_artisans_name:0 ""Expert Artisans""",
" tradition_fp1_coastal_warriors_name:0 ""Coastal Warriors""",
" tradition_fp1_performative_honour_name:0 ""Performative Honor""",
" tradition_fp1_northern_stories_name:0 ""Northern Stories""",
" tradition_fp1_trials_by_combat_name:0 ""Trials-by-Combat""",
" tradition_fp1_the_right_to_prove_name:0 ""The Right to Prove""",
" tradition_strong_kinship_name:0 ""Strong Kinship""",
" tradition_amharic_highlanders_name:0 ""Amharic Highlanders"""})
    End Function
    Function DeConcept(Input As String) As String
        Dim Output As String = Input
        Do
            Dim GameConcept As String = Split(Split(Output, "[", 2)(1), "|", 2)(0)
            Dim Finder As String
            If GameConceptLocalisations.ContainsKey(GameConcept.ToLower) Then
                Finder = GameConceptLocalisations(GameConcept.ToLower)
            Else
                Finder = GameConcept
            End If
            Output = Output.Replace($"[{GameConcept}|E]", Finder)
        Loop While Output.Contains("|E]")
        Return Output
    End Function
    Function DeFormat(Input As String) As String
        Dim Output As List(Of String) = Input.Split("#"c, StringSplitOptions.RemoveEmptyEntries).ToList
        For Count = 0 To Output.Count - 1
            Dim Formatted() As String = Output(Count).Split({" "c, vbTab}, 2, StringSplitOptions.None)
            Output(Count) = Output(Count).Split({" "c, vbTab}, 2, StringSplitOptions.None)(1)
        Next
        Return String.Concat(Output).Trim
    End Function
End Module
