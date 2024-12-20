﻿open System
open System.IO
open System.Text.Json
open System.Windows.Forms
open System.Drawing

type DictionaryEntry = { Word: string; Definition: string }
let dictionaryFilePath = "dictionary.json"

 // readJson Function
let readJson<'T> (filePath: string) : 'T list =
    if File.Exists(filePath) then
        let json = File.ReadAllText(filePath)
        if String.IsNullOrWhiteSpace(json) then
            [] // Return an empty list if the file is empty
        else
            try
                match JsonSerializer.Deserialize<'T list>(json) with
                | result -> result
            with
            | :? System.Text.Json.JsonException ->
                printfn "Warning: Invalid JSON in file '%s'. Returning an empty list." filePath
                []
    else
        []

 // writeJson Function
let writeJson<'T> (filePath: string) (data: 'T list) =
    let json = JsonSerializer.Serialize(data)
    File.WriteAllText(filePath, json)


 // initializeDictionaryFile Function
let initializeDictionaryFile () =
    if not (File.Exists(dictionaryFilePath)) then
        File.WriteAllText(dictionaryFilePath, "[]")


module Dictionary =

 // loadDictionary Function
    let loadDictionary () =
        let entries = readJson<DictionaryEntry> dictionaryFilePath
        entries 
        |> List.map (fun e -> e.Word.ToLowerInvariant(), e.Definition) 
        |> Map.ofList
   

 // saveDictionary Function
    let saveDictionary (dict: Map<string, string>) =
        let entries = dict |> Map.toList |> List.map (fun (word, definition) -> { Word = word; Definition = definition })
        writeJson dictionaryFilePath entries

 // addWord Function
    let addWord (dict: Map<string, string>) (word: string) (definition: string) : Map<string, string> =
        let wordKey = word.ToLowerInvariant()
        if dict.ContainsKey(wordKey) then dict
        else dict.Add(wordKey, definition)


 // updateWord Function
    let updateWord (dict: Map<string, string>) (oldWord: string) (newWord: string) (newDefinition: string) : Map<string, string> =
        let oldKey = oldWord.ToLowerInvariant()
        let newKey = newWord.ToLowerInvariant()
        if dict.ContainsKey(oldKey) then
            dict
            |> Map.remove oldKey
            |> Map.add newKey newDefinition
        else
            dict


 // deleteWord Function
    let deleteWord (dict: Map<string, string>) (word: string) : Map<string, string> =
        let wordKey = word.ToLowerInvariant()
        if dict.ContainsKey(wordKey) then dict.Remove(wordKey)
        else dict


 // searchWord Function
    let searchWord (dict: Map<string, string>) (keyword: string) : (string * string) list =
        let keyword = keyword.ToLowerInvariant()
        dict 
        |> Map.filter (fun key value -> 
            key.Contains(keyword))
        |> Map.toList



// Create GUI
let createDictionaryForm () =
    initializeDictionaryFile()
    let mutable dictionary = Dictionary.loadDictionary()

    let form = new Form(Text = "Digital Dictionary", Width = 600, Height = 600)
    form.FormBorderStyle <- FormBorderStyle.FixedDialog
    form.MaximizeBox <- false
    form.StartPosition <- FormStartPosition.CenterScreen

    let lblWord = new Label(Text = "Word:", Top = 20, Left = 20, Width = 50)
    let textWord = new TextBox(Top = 20, Left = 150, Width = 300)

    let lblDefinition = new Label(Text = "Definition:", Top = 60, Left = 20, Width = 70)
    let textDefinition = new TextBox(Top = 60, Left = 150, Width = 300)

    let btnAdd = new Button(Text = "Add Word", Top = 100, Left = 20, Width = 120)
    let btnUpdate = new Button(Text = "Update Word", Top = 100, Left = 160, Width = 120)
    let btnDelete = new Button(Text = "Delete Word", Top = 100, Left = 300, Width = 120)
    let btnSearch = new Button(Text = "Search", Top = 100, Left = 440, Width = 120)

    let lblStatus = new Label(Text = "", Top = 150, Left = 20, Width = 550, Height = 30, ForeColor = Color.DarkGreen)
    let listResults = new ListView(Top = 200, Left = 20, Width = 540, Height = 320)
    listResults.View <- View.Details
    listResults.Columns.Add("Word", 200) |> ignore
    listResults.Columns.Add("Definition", 400) |> ignore

    let updateResults results =
        listResults.Items.Clear()
        results
        |> List.iter (fun (word, definition) ->
            let item = new ListViewItem([| word; definition |])
            listResults.Items.Add(item) |> ignore)

    btnAdd.Click.Add(fun _ ->
        let word = textWord.Text
        let definition = textDefinition.Text
        if String.IsNullOrWhiteSpace(word) || String.IsNullOrWhiteSpace(definition) then
            lblStatus.Text <- "Please enter both a word and its definition."
            lblStatus.ForeColor <- Color.Red
        elif dictionary.ContainsKey(word.ToLowerInvariant()) then
            lblStatus.Text <- $"The word '{word}' already exists in the dictionary."
            lblStatus.ForeColor <- Color.Orange
        else
            dictionary <- Dictionary.addWord dictionary word definition
            Dictionary.saveDictionary dictionary
            lblStatus.Text <- $"Added: {word}"
            lblStatus.ForeColor <- Color.DarkGreen
            updateResults (Dictionary.searchWord dictionary ""))

    btnUpdate.Click.Add(fun _ ->
        let word = textWord.Text
        let definition = textDefinition.Text
        if String.IsNullOrWhiteSpace(word) || String.IsNullOrWhiteSpace(definition) then
            lblStatus.Text <- "Please enter a word and its new definition."
            lblStatus.ForeColor <- Color.Red
        else
            let wordExists = dictionary.ContainsKey(word.ToLowerInvariant())
            if not wordExists then
                lblStatus.Text <- $"The word '{word}' does not exist in the dictionary."
                lblStatus.ForeColor <- Color.Red
            else
                dictionary <- Dictionary.updateWord dictionary word word definition
                Dictionary.saveDictionary dictionary
                lblStatus.Text <- $"Updated: {word}"
                lblStatus.ForeColor <- Color.DarkGreen
                updateResults (Dictionary.searchWord dictionary ""))

    btnDelete.Click.Add(fun _ ->
        let word = textWord.Text
        if String.IsNullOrWhiteSpace(word) then
            lblStatus.Text <- "Please enter a word to delete."
            lblStatus.ForeColor <- Color.Red
        else
            let wordExists = dictionary.ContainsKey(word.ToLowerInvariant())
            if not wordExists then
                lblStatus.Text <- $"The word '{word}' does not exist in the dictionary."
                lblStatus.ForeColor <- Color.Red
            else
                dictionary <- Dictionary.deleteWord dictionary word
                Dictionary.saveDictionary dictionary
                lblStatus.Text <- $"Deleted: {word}"
                lblStatus.ForeColor <- Color.DarkGreen
                updateResults (Dictionary.searchWord dictionary ""))

    btnSearch.Click.Add(fun _ ->
        let keyword = textWord.Text
        if String.IsNullOrWhiteSpace(keyword) then
            lblStatus.Text <- "Please enter a word or definition to search."
            lblStatus.ForeColor <- Color.Red
        else
            let results = Dictionary.searchWord dictionary keyword
            if List.isEmpty results then
                lblStatus.Text <- $"No matches found for '{keyword}'."
                lblStatus.ForeColor <- Color.Red
            else
                lblStatus.Text <- $"Found {List.length results} result(s) for '{keyword}'."
                lblStatus.ForeColor <- Color.DarkGreen

            updateResults results)


    form.Controls.AddRange([| lblWord; textWord; lblDefinition; textDefinition; btnAdd; btnUpdate; btnDelete; btnSearch; lblStatus; listResults |])
    form

[<EntryPoint>]
let main argv =
    Application.Run(createDictionaryForm())
    0
