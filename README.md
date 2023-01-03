# Spreadsheet GUI

This is the first version of the program that implements a simple spreadsheet for PS6. This program allows you to work on simple Excel documents. 

- Editor: Mingyu Jung
- Version: ps6

## Features

To run program, please set SpreadsheetGUI as a startup project

1. Menu bar

From the menu bar, you can create, open, or save a new spreadsheet.

- New: Create a new spreadsheet. The spreadsheet is basically made of ps6 version. If the spreadsheet is not saved, check if user wants to save.
- Open: Load the saved file. It can open those file format; txt, sprd, and json. If the file is successfully loaded, the spreadsheet is replaced with the imported spreadsheet.
- Save: Save the current spreadsheet you created. At this time, the file is named "temp" and format is "."json". The temporary route is as follows. 

>Window	=>	C:\Users\Username\AppData\Local\Packages\F4F42848-9040-41A1-BF68-3183E0E99EF5_9zz4h110yvjzm\LocalState
>
>macOS	=>	https://developer.apple.com/library/archive/documentation/FileManagement/Conceptual/FileSystemProgrammingGuide/FileSystemOverview/FileSystemOverview.html


If the file path has the same filename, the file will be overwritten without any warning.

- SaveAs: Saves the spreadsheet you created. You can set the name, path, and format of the file by typing at the end of the file name, such as ".txt", ".sprd", and ".json". Because the folder selection utility does not exist, you must copy and write down the 'FULL path' you want to save.
- Help: Show a brief information manual.

2. Cell Information Grid

In the Cell Information grid, there is a grid box where you can see the location of a cell, the values stored in a cell, and the editbox in a cell. There are also three mode selection next to editbox that allow you to change the color mode of the spreadsheet. This allows you to work with the color of the spreadsheet you want.
- Cell name box: Shows the location of a cell. For example, if a cell is located in column 'A' row '11', it will print "A11".
- Cell value box: Shows the value currently stored in a cell. If a cell contains a double value, show the double value. Shows the string value if value is string. If a cell stored Formula type, it shows the results that calculates the Formula expression. If the result value of the Formula is divided by 0 or contains an invalid cell name, it will show "FormulaError" instead of value.
- Cell Editbox: Where you can type contents in a cell. To enter a value in a cell, you must either click Editbox and edit what you want, or double-click a cell. If you press Enter or click another cell after you type a value, the value will be saved successfully. If you write invalid content and click the other cell or push enter key, an exception alert will occur and a warning window will pop up. If you want to type Formula, you must put "=" in front of the content in editbox.
- Normal Mode: White sheet, black font
- Dark Mode: Black sheet, white font
- EyeComfy: Cookie-and-Cream yellow sheet, black font

3. Spreadsheet Grid
It's the grid section where the values that you wrote are actually printed and shown. Here you can see the results of the document you wrote. Each time you click on a cell, the name of a cell and the resulting value is displayed appropriately above layout box. If the type entered in a cell is Formula, it shows the result of Formula.
- Row of a cell: Shows the number of rows. There are 99 rows in total.
- Columns in Cells: Shows the name of columns. Composed of alphabets, columns exist up to A-Z.
