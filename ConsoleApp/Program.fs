open System
open System.IO
open System.Text.Json
open System.Windows.Forms
open System.Drawing

type DictionaryEntry = { Word: string; Definition: string }
let dictionaryFilePath = "dictionary.json"

 // readJson Function


 // writeJson Function


 // initializeDictionaryFile Function


module Dictionary =

    // loadDictionary Function


    // saveDictionary Function



    // addWord Function



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
            dictionary <- Dictionary.deleteWord dictionary word
            Dictionary.saveDictionary dictionary
            lblStatus.Text <- $"Deleted: {word}"
            lblStatus.ForeColor <- Color.DarkGreen
            updateResults (Dictionary.searchWord dictionary ""))

    btnSearch.Click.Add(fun _ ->
        let keyword = textWord.Text
        let results = Dictionary.searchWord dictionary keyword
        if List.isEmpty results then
            lblStatus.Text <- "No matching words found."
            lblStatus.ForeColor <- Color.Red
        else
            lblStatus.Text <- $"Found {List.length results} results."
            lblStatus.ForeColor <- Color.DarkGreen
    
        updateResults results)

    form.Controls.AddRange([| lblWord; textWord; lblDefinition; textDefinition; btnAdd; btnUpdate; btnDelete; btnSearch; lblStatus; listResults |])
    form

[<EntryPoint>]
let main argv =
    Application.Run(createDictionaryForm())
    0
