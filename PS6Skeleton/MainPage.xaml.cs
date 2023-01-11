using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.UI.Xaml.Input;
using SpreadsheetUtilities;
using SS;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Windows.Storage.Pickers;

/**
 *  MinGyu Jung & SangYoon Cho.
 *  Oct, 21, 2022
 * 
 * Version 1.0   update- when user complete the enter in EditBox, it goes down just like spreadsheet.
 * Version 1.1   update- After updating edit box, if user click other cell,
 *                       previous edited cell will be updated too. just like spreadsheet
 * Version 1.2   update- when cell is updated, all of depends cells are updated too.
 * 
 * 
 * Version 1.2  update- Complete save, load implementation.
 *      1.Safety Features
 *           - Ask save in every 3 minutes
 *           - Ask overwrite feature
 *      2. Save, Load implementation
 *          - SaveAs => ask path and file name
 *          - Save => save with naming "temp"
 *          - User can choose file format by writting format at the end of the file name.
 *          - Open, New -> Ask user save spreadsheet if spreadsheet is not saved.
 * 
 *  Version 1.3 update- A little bit of bug fixed
 *          - Editbox updated when exception occurred
 *          - Scrollbar updated: fixed xaml => arranged entry and editor into the grid
 *          - Cell visible bug updated => fix the row num
 *          
 * Version 1.4 update- color change is updated. (extra feture.)
 * 
 *      1. Bright Mode.
 *          - Back ground color will be white.
 *          - Col and Row labels back ground color will be LightGrey
 *          - Text color will be black.
 *      2. Dark Mode.
 *          - Back ground color will be white.
 *          - Col and Row labels back ground color will be DarkGrey
 *          - Text color will be white. 
 *      3. EyeComfy Mode.
 *          - Back ground color will be Cookies And Cream. RGB (230,223,175)
 *          - Col and Row labels back ground color will be Opal. RGB(163.204.190)
 *          - Text color will be black. 
 *
 * Version 1.4 update - EditBox focusing. When user double click the cell or right click,
 *                      Edit box will be focused automatically.
 *      
 */

namespace SpreadsheetGUI;

/// <summary>
/// Example of using a SpreadsheetGUI object
/// </summary>
public partial class MainPage : ContentPage
{
    Spreadsheet spreadsheet;

    // Previous cell data (previous cell address and previous cell editbox)
    public string prevAddress;
    public string prevEditBox;

    // Same filename overwritting checker
    private string _fileName;

    /// <summary>
    /// Constructor for the demo
    /// </summary>
    public MainPage()
    {
        InitializeComponent();

        // Set the filename as temporary name.
        _fileName = "temp";

        spreadsheetGrid.SelectionChanged += displaySelection;

        // First selection for new spreadsheet
        spreadsheetGrid.SetSelection(2, 3);
        spreadsheetGrid.Editbox = EditBox;

        prevAddress = "B3";
        prevEditBox = "";

        // Constructor with Version:"ps6"
        spreadsheet = new Spreadsheet(s => IsValidVariable(s), s => s.ToUpper(), "ps6");
        // Operates Timer
        SaveWarningOverTime();
    }


    public void focusEditBox(object sender, EventArgs e)
    {
        Entry entry = this.FindByName<Entry>("EditBox");
        entry.Focus();
    }


    /// <summary>
    /// When EditBox's words are changed, this method will be ran.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void OnEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        string myText = EditBox.Text;
        prevEditBox = myText;
        //when user type the word on edit box, cell's words are changed too.
        EditBoxUpdate(myText);
    }



    /// <summary>
    /// When user press the enter (EditBox completed condition.)
    /// This method will be ran.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void OnEntryCompleted(object sender, EventArgs e)
    {
        try
        {
            //when user press the enter, the text is returned.
            string text = ((Entry)sender).Text;

            //current selection address.
            spreadsheetGrid.GetSelection(out int col, out int row);
            //update prevAddress.
            prevAddress = CellNameConverter(col, row);
            //get current value by current col and row
            spreadsheetGrid.GetValue(col, row, out string value);
            //get current cell name ex) A1
            string cell = GetCurrentCellName();


            spreadsheet.SetContentsOfCell(cell, text);

            //when user press the enter, update the cellValue and EditBox.
            cellValue.Text = (string)spreadsheet.GetCellValue(cell).ToString();
            EditBox.Text = GetContents();
            
            //
            AllCellsUpdate();

            //MOVE TO DOWN
            row += 1;

            EditBoxUpdate(col, row);
            SetcellNameUpdate(col, row);
            spreadsheetGrid.SetValue(col, row, cellValue.Text);
            prevAddress = CellNameConverter(col, row);
        }
        catch (Exception)
        {
            ExceptionPopUP();
        }
    }

    /// <summary>
    /// Helper method for updating all of related cell's value.
    /// </summary>
    private void AllCellsUpdate()
    {
        //Gettign list of cells
        HashSet<string> list = (HashSet<string>)spreadsheet.GetNamesOfAllNonemptyCells();

        foreach (string point in list)
        {
            int colValue = (int)point[0] % 32 - 1;

            int rowValue = Convert.ToInt32(Regex.Match(point, @"\d+").Value) - 1;

            spreadsheetGrid.SetValue(colValue, rowValue, (string)spreadsheet.GetCellValue(point).ToString());
        }
    }

    /// <summary>
    /// Helper Method.
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    private void SetcellNameUpdate(int col, int row)
    {
        cellName.Text = (char)(col + 'A') + (row + 1).ToString();
    }

    //return current cellName
    private string GetCurrentCellName()
    {
        spreadsheetGrid.GetSelection(out int col, out int row);

        string cellName = (char)(col + 'A') + (row + 1).ToString();

        return cellName;
    }

    /// <summary>
    /// Helper method to convert address to string.
    /// ex) col = 1, row =1 return "A1"
    /// </summary>
    /// <param name="col">  col value </param>
    /// <param name="row">  row value </param>
    /// <returns> converted address  </returns>
    private string CellNameConverter(int col, int row)
    {
        string cellAddress = (char)(col + 'A') + (row + 1).ToString();

        return cellAddress;
    }

    /// <summary>
    /// Helper method to update EditBox's word.
    /// by getting string value as paraemter
    /// </summary>
    /// <param name="value"></param>
    private void EditBoxUpdate(string value)
    {
        spreadsheetGrid.GetSelection(out int col, out int row);

        spreadsheetGrid.SetValue(col, row, value);
    }
    /// <summary>
    /// Helper method to update EditBox's word.
    /// by getting col and row as parameter
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    /// 
    private void EditBoxUpdate(int col, int row)
    {
        spreadsheetGrid.SetSelection(col, row);
        //get new value
        spreadsheetGrid.GetValue(col, row, out string value);
        EditBox.Text = GetContents();
        cellValue.Text = value;
    }

    private void displaySelection(SpreadsheetGrid grid)
    {
        try
        {
            //get col and row
            spreadsheetGrid.GetSelection(out int col, out int row);
            //get value and return to value string.
            spreadsheetGrid.GetValue(col, row, out string value);

            //update cell's information when user click the other cell.
            if (!(prevAddress.Equals(GetCurrentCellName().ToString()) || String.IsNullOrEmpty(prevEditBox)))
            {
                spreadsheet.SetContentsOfCell(prevAddress, prevEditBox);
                AllCellsUpdate();
            }
            SetcellNameUpdate(col, row);

            //If cell is empty
            if (value == "")
            {
                EditBox.UpdateText("");
                cellValue.UpdateText("");
            }
            else
            {
                //If cell is not empty, update CellValue(Entry) 
                cellValue.UpdateText((string)spreadsheet.GetCellValue(GetCurrentCellName()).ToString());
                //If cell is not empty, update EditBox's word.(Entry) 
                EditBox.UpdateText(GetContents());
                //Set the cell's value as user's input.
                spreadsheetGrid.SetValue(col, row, cellValue.Text);
            }
        }catch (Exception)
        {
            ExceptionPopUP();
        }
        //updating the previous address. 
        prevAddress = GetCurrentCellName();
    }

    /// <summary>
    /// Exception alert window popup.
    /// </summary>
    private async void ExceptionPopUP()
    {
        await DisplayAlert("Alert", "Invalid Formula", "OK");
    }

    /// <summary>
    /// Helper method for displaying EditBox for Forumla case.
    /// </summary>
    /// <returns></returns>
    private string GetContents()
    {
        //checking type 
        if (spreadsheet.GetCellContents(GetCurrentCellName()).GetType() == typeof(Formula))
            return "=" + (string)spreadsheet.GetCellContents(GetCurrentCellName()).ToString();
        else
            return (string)spreadsheet.GetCellContents(GetCurrentCellName()).ToString();
    }

    /// <summary>
    /// Create new spreadsheet.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void NewClicked(Object sender, EventArgs e)
    {
        // If spreadsheet is changed, ask user if he/she wants to save current spreadsheet
        if (spreadsheet.Changed)
            SaveAsClicked(sender, e);
        

        // Clear current spreadsheet and change it to new one.
        spreadsheet = new Spreadsheet(s => IsValidVariable(s), s => s.ToUpper(), "ps6");
        spreadsheetGrid.Clear();
        EditBox.UpdateText("");
        _fileName = "temp";
    }

    /// <summary>
    /// Opens any file as text and prints its contents.
    /// </summary>
    private async void OpenClicked(Object sender, EventArgs e)
    {
        // If spreadsheet is changed, ask user if he/she wants to save current spreadsheet
        if (spreadsheet.Changed)
            SaveAsClicked(sender, e);

        try
        {
            var customFileType =
                new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {   // Can also open txt and sprd files.
                    {DevicePlatform.WinUI, new[] { ".sprd", ".txt" } },
                    {DevicePlatform.macOS, new[] { "sprd", "txt" } },
                });

            FileResult fileResult = await FilePicker.Default.PickAsync();
            if (fileResult != null)
            {
                // Update spreadsheet with opened one.
                spreadsheet = new Spreadsheet(fileResult.FullPath, s => IsValidVariable(s), s => s.ToUpper(), "ps6");
                spreadsheetGrid.Clear();
                EditBox.UpdateText("");
                AllCellsUpdate();
                _fileName = fileResult.FileName;
            }
            else
            {
                await DisplayAlert("Nothing", "No file selected", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Invalid File", "Error opening file:" + ex, "OK");
        }
    }

    /// <summary>
    /// Save spreadsheet into the specified path, where is the LocalCacheFolder directory.
    /// Win => C:\Users\black\AppData\Local\Packages\F4F42848-9040-41A1-BF68-3183E0E99EF5_9zz4h110yvjzm\LocalState
    /// mac => https://developer.apple.com/library/archive/documentation/FileManagement/Conceptual/FileSystemProgrammingGuide/FileSystemOverview/FileSystemOverview.html
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveClicked(Object sender, EventArgs e)
    {
        // Filepath into the AppData
        string appdataDir = FileSystem.Current.AppDataDirectory;
        // It would be saved by json format temporarily.
        spreadsheet.Save(appdataDir + "\\" + _fileName + ".json");
    }

    /// <summary>
    /// Save spreadsheet where user want to.
    /// User can set the path and filename.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param> 
    private async void SaveAsClicked(Object sender, EventArgs e)
    {
        // Check whether user want to overwrite or not
        bool DoOverwrite = false;
        // Get filename
        string result = await DisplayPromptAsync("Save", "Write File name");
        // If user doesn't write filename, ignore saving.
        if (result != null)
        {
            // Get file full path
            string filepath = await DisplayPromptAsync("Save", "Write file path where you want to save (full path)");
            // If user doesn't write filepath, ignore saving.
            if (filepath != null)
            {
                // Overwritting checker. If file already saved in the same path, 
                if (File.Exists(filepath + "\\" + result))
                {
                    // Ask overwritting
                    DoOverwrite = await DisplayAlert("Warning", "File already exists. Do you want to overwrite it?", "Yes", "No");
                }
                if (DoOverwrite)
                {
                    // If user writes file format in the filename, save with format string.
                    if (result.Contains(".sprd") || result.Contains(".json") || result.Contains(".txt"))
                        spreadsheet.Save(filepath + "\\" + result);
                    else
                        spreadsheet.Save(filepath + "\\" + result + ".json");
                    // filename overwritting checker updated
                    _fileName = result;
                }
            }
        }
    }

    /// <summary>
    /// Safety Feature.
    /// Ask user to save if he/she doesn't save current spreadsheet in every 3 minutes.
    /// </summary>
    private async void SaveWarningOverTime()
    {
        // Check whether user want to overwrite or not
        bool DoOverwrite = false;

        // Set the period of the timer.
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(180));

        // Alert time in every 'timer' minutes
        while (await timer.WaitForNextTickAsync())
        {
            // If user changed or wrote spreadsheet and if it doesn't save, ask user save.
            if (spreadsheet.Changed)
            {
                // Check whether user want to overwrite or not
                bool answer = await DisplayAlert("Warning", "Changed detected. Do you want to save?", "Yes", "No");
                if (answer)
                {
                    // If user doesn't write filename, ignore saving.
                    string result = await DisplayPromptAsync("Save", "Write File name");
                    if (result != null)
                    {
                        // Get file full path
                        string filepath = await DisplayPromptAsync("Save", "Write file path where you want to save (full path)");
                        // Overwritting checker. If file already saved in the same path, 
                        if (File.Exists(filepath + "\\" + result))
                        {
                            // Ask overwritting
                            DoOverwrite = await DisplayAlert("Warning", "File already exists. Do you want to overwrite it?", "Yes", "No");
                        }
                        if (DoOverwrite)
                        {
                            // If user writes file format in the filename, save with format string.
                            if (result.Contains(".sprd") || result.Contains(".json") || result.Contains(".txt"))
                                spreadsheet.Save(filepath + "\\" + result);
                            else
                                spreadsheet.Save(filepath + "\\" + result + ".json");
                            // filename overwritting checker updated
                            _fileName = result;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// This is a help information on the menubar.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Instructor(Object sender, EventArgs e)
    {
        await DisplayAlert("Help", "#Version: ps6\n" +
                                   "#Alert will notice you to save in every 3 minutes.\n\n" +
                                   "======Basic Info======\n" +
                                   "Save          : It would be saved temporary path and name. Read detail in README.txt\n" +
                                   "SaveAs        : After you write file name and click ok,\n" +
                                   "You should write FULL PATH where you want to save. Read detail in README.txt\n\n" +
                                   "Cell name     : It shows a cell name\n" +
                                   "Cell value    : It shows calculated cell value\n" +
                                   "Cell Editbox  : Write content here, Press enter or click other cell to update cell\n" +
                                   "If you double click a cell, you can edit cell\n\n" +
                                   "======Extra Features======\n" +
                                   "Normal mode   : White spreadsheet, Black font\n" +
                                   "Dark mode     : Dark spreadsheet, White font\n" +
                                   "EyeConfy mode : C&C yellow spreadsheet, Black font", "OK");
    }

    /// <summary>
    /// Helper method for checking valid variables.
    /// </summary>
    /// <param name="variable"></param>
    /// <returns> true      if string is valid.</returns>
    private static bool IsValidVariable(string variable)
    {
        string varPattern = @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*";

        if (Regex.IsMatch(variable, varPattern, RegexOptions.Singleline))
            return true;

        return false;
    }

    //=======================EXTRA FEATRUE===========================   

    /// <summary>
    /// By using RadioButton. change the Theme to Bright Mode
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BrightMode(object sender, CheckedChangedEventArgs e)
    {
        spreadsheetGrid._backGroundColor = Colors.LightGrey;
        spreadsheetGrid._textColor = Colors.Black;
        spreadsheetGrid._fillColor = Colors.White;
        spreadsheetGrid._lineColor = Colors.Black;
        spreadsheetGrid.changeColor();

    }
    /// <summary>
    /// By using RadioButton. change the Theme to Dark Mode
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DarkMode(object sender, CheckedChangedEventArgs e)
    {
        spreadsheetGrid._backGroundColor = Color.FromRgb(30,30,30);
        spreadsheetGrid._fillColor = Color.FromRgb(65, 65, 65);
        spreadsheetGrid._textColor = Colors.White;
        spreadsheetGrid._lineColor = Colors.White;
        spreadsheetGrid.changeColor();
    }

    /// <summary>
    /// By using RadioButton. change the Theme to eye comfortable Mode
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void EyeComfy(object sender, CheckedChangedEventArgs e)
    {
        spreadsheetGrid._backGroundColor = Color.FromRgb(163, 204, 190);
        spreadsheetGrid._fillColor = Color.FromRgb(230, 223, 175);
        spreadsheetGrid._textColor = Colors.Black;
        spreadsheetGrid._lineColor = Colors.Black;
        spreadsheetGrid.changeColor();
    }
}

