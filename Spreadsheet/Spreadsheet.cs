// CS3500 PS5
// September, 30th, 2022
// MinGyu Jung
// PASSED ALL GRADING TEST VERSION.

using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpreadsheetUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SS
{
    /// <summary>
    /// An AbstractSpreadsheet object represents the state of a simple spreadsheet.  A 
    /// spreadsheet consists of an infinite number of named cells.
    /// 
    /// A string is a valid cell name if and only if:
    ///   (1) its first character is an underscore or a letter
    ///   (2) its remaining characters (if any) are underscores and/or letters and/or digits
    /// Note that this is the same as the definition of valid variable from the PS3 Formula class.
    /// 
    /// For example, "x", "_", "x2", "y_15", and "___" are all valid cell  names, but
    /// "25", "2x", and "&" are not.  Cell names are case sensitive, so "x" and "X" are
    /// different cell names.
    /// 
    /// A spreadshee252t contains a cell corresponding to every possible cell name.  (This
    /// means that a spreadsheet contains an infinite number of cells.)  In addition to 
    /// a name, each cell has a contents and a value.  The distinction is important.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected).
    /// 
    /// In a new spreadsheet, the contents of every cell is the empty string.
    ///  
    /// We are not concerned with values in PS4, but to give context for the future of the project,
    /// the value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid). 
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError,
    /// as reported by the Evaluate method of the Formula class.  The value of a Formula,
    /// of course, can depend on the values of variables.  The value of a variable is the 
    /// value of the spreadsheet cell it names (if that cell's value is a double) or 
    /// is undefined (otherwise).
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary>

    public class Spreadsheet : AbstractSpreadsheet
    {
        //Dictionary for contaning cell, and set it as JsonProperty.
        [JsonProperty(PropertyName = "cells")]
        private Dictionary<string, Cell> cells;
        //DependencyGraph for connecting cell's relaitionship.
        private DependencyGraph dependencyGraph;
        //bool for distinguish if it is modified or not.
        private bool changed;

        //===================================================PS5==========================================

        //Constructure.
        /// <summary>
        /// Your zero-argument constructor should create an empty spreadsheet 
        /// that imposes no extra validity conditions, normalizes every cell 
        /// name toitself, and has version "default".
        /// </summary>
        public Spreadsheet() : base(s => true, s => s, "default")
        {
            cells = new Dictionary<string, Cell>();
            dependencyGraph = new DependencyGraph();
            Changed = false;
        }

        // ADDED FOR PS5
        /// <summary>
        /// Constructs an abstract spreadsheet by recording its variable validity test,
        /// its normalization method, and its version information.  The variable validity
        /// test is used throughout to determine whether a string that consists of one or
        /// more letters followed by one or more digits is a valid cell name.  The variable
        /// equality test should be used thoughout to determine whether two variables are
        /// equal.
        /// </summary>
        /// 


        /// </summary>
        /// <param name="isValid"> to check format is valid or not. </param>
        /// <param name="normalize"> for nomalizing such as by using, toLower() or toUpper() </param>
        /// <param name="version">  for representing version of file. </param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version)
            : base(isValid, normalize, version)
        {
            cells = new Dictionary<string, Cell>();
            dependencyGraph = new DependencyGraph();
            Changed = false;
        }


        /// </summary>
        /// <param name="fileName"> loading file name. </param>
        /// <param name="isValid"> to check format is valid or not. </param>
        /// <param name="normalize"> for nomalizing such as by using, toLower() or toUpper() </param>
        /// <param name="version">  for representing version of file. </param>
        public Spreadsheet(string fileName, Func<string, bool> isValid, Func<string, string> normalize, string version)
    : base(isValid, normalize, version)
        {

            cells = new Dictionary<string, Cell>();
            dependencyGraph = new DependencyGraph();

            //If any of the names contained in the saved spreadsheet are invalid
            if (string.IsNullOrEmpty(fileName))
            {
                throw new SpreadsheetReadWriteException("file name cannot be empty or null");
            }
            //If filename is not exists, throw Exception.
            if (!File.Exists(fileName))
            {
                throw new SpreadsheetReadWriteException(fileName + " is not exist.");
            }
            else
            {

                try
                {
                    // Deseralize the json and make it object.
                    Spreadsheet? s = JsonConvert.DeserializeObject<Spreadsheet>(File.ReadAllText(fileName));

                    //If the version of the saved spreadsheet does not match the version
                    //parameter provided to the constructor, throw exception.
                    if (!this.Version.Equals(s?.Version))
                    {
                        throw new SpreadsheetReadWriteException(s?.Version + " is not valid version.");
                    }

                    //After deserializing, 
                    foreach (string key in s.cells.Keys)
                    {
                        SetContentsOfCell(key, s.cells[key].stringForm);
                    }
                }
                catch (Exception)
                {
                    throw new SpreadsheetReadWriteException("Invalid format.");
                }
            }
            //set to false. 
            Changed = false;
        }

        // ADDED FOR PS5
        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed
        {
            get
            {
                return this.changed;
            }
            protected set
            {
                this.changed = value;
            }
        }


        // ADDED FOR PS5
        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using a JSON format.
        /// The JSON object should have the following fields:
        /// "Version" - the version of the spreadsheet software (a string)
        /// "cells" - an object containing 0 or more cell objects
        ///           Each cell object has a field named after the cell itself 
        ///           The value of that field is another object representing the cell's contents
        ///               The contents object has a single field called "stringForm",
        ///               representing the string form of the cell's contents
        ///               - If the contents is a string, the value of stringForm is that string
        ///               - If the contents is a double d, the value of stringForm is d.ToString()
        ///               - If the contents is a Formula f, the value of stringForm is "=" + f.ToString()
        /// 
        /// For example, if this spreadsheet has a version of "default" 
        /// and contains a cell "A1" with contents being the double 5.0 
        /// and a cell "B3" with contents being the Formula("A1+2"), 
        /// a JSON string produced by this method would be:
        /// 
        /// {
        ///   "cells": {
        ///     "A1": {
        ///       "stringForm": "5"
        ///     },
        ///     "B3": {
        ///       "stringForm": "=A1+2"
        ///     }
        ///   },
        ///   "Version": "default"
        /// }
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>

        public override void Save(string filename)
        {
            try
            {
                // Referance : https://stackoverflow.com/questions/16921652/how-to-write-a-json-file-in-c
                //Serialize the current "object" to JsonObject, and change formatting to read more clearly.
                string? json = JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filename, json);
            }
            catch (Exception)
            {
                throw new SpreadsheetReadWriteException(filename + " is not valid.");
            }
            //After save, set to false.
            Changed = false;
        }

        // ADDED FOR PS5
        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>

        //There is a new abstract method GetCellValue. This function gets the value, as opposed to the contents, of the cell.
        public override object GetCellValue(string name)
        {
            //If it is not valid name, throw InvalidNameException.
            if (String.IsNullOrEmpty(name) || !IsValid(Normalize(name)))
            {
                throw new InvalidNameException();
            }
            //If cells contains the name, return the value.
            if (cells.ContainsKey(name))
            {
                return cells[name].value;
            }
            else
            {
                return "";
            }
        }
        // ADDED FOR PS5
        /// <summary>
        /// Otherwise, if name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown,
        ///       and no change is made to the spreadsheet.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a list consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell. The order of the list should be any
        /// order such that if cells are re-evaluated in that order, their dependencies 
        /// are satisfied by the time they are evaluated.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>

        public override IList<string> SetContentsOfCell(string name, string content)
        {

            /// If name is invalid, throws an InvalidNameException.
            if (!IsValid(Normalize(name)))
            {
                throw new InvalidNameException();
            }
            // The list of that we have to re-evaluated.
            List<string> dependencyList = new List<string>();
            //Normalized name
            String normalName = Normalize(name);
            //When content is null or empty, make it as initial version.
            if (String.IsNullOrEmpty(content))
            {
                dependencyList = new List<string>();
            }
            //if content parses as a double, the contents of the named
            else if (double.TryParse(content, out double cellvalue))
            {
                dependencyList = new List<string>(SetCellContents(normalName, cellvalue));
            }
            //If content start with '=', it should be formula.
            else if (content[0].Equals('='))
            {
                //substring '-' 
                string formulaString = content.Substring(1);
                //case 1. If it is invalid formula, Formula class will throw FormulaFormatException.
                //create new formula which is substring "=" version.
                Formula formula = new Formula(formulaString, Normalize, IsValid);

                //case 2. when content is formula.
                //SetCellContents will catch CircularException. 
                //add formula to list.
                dependencyList = new List<string>(SetCellContents(normalName, formula));
            }
            // content is string.   
            else
            {
                dependencyList = new List<string>(SetCellContents(normalName, content));
            }
            //set to change
            Changed = true;

            Cell cell;

            //Since SetcellContents returns GetCellsToRecalculate,
            //dependencyList will provide the list of what we have to recalculate.
            foreach (string s in dependencyList)
            {
                if (cells.ContainsKey(s))
                {
                    cell = cells[s];
                    //update the cell's "value" by using helper method
                    cell.valueUpdate(LookupValue);
                }
            }
            return dependencyList;
        }
        //===================================================PS5 END==========================================


        //=============================================PS4 parts=======================================

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            HashSet<string> nonEmptyCells = new HashSet<string>();

            foreach (KeyValuePair<string, Cell> entry in cells)
            {
                if (entry.Value.content is not "")
                {
                    nonEmptyCells.Add(entry.Key);
                }
            }
            return nonEmptyCells;
        }



        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        /// 
        public override object GetCellContents(string name)
        {
            //If it is not valid name, throw InvalidNameException.
            if (!IsValid(Normalize(name)))
            {
                throw new InvalidNameException();
            }

            object cellContents = "";

            if (cells.ContainsKey(Normalize(name)))
            {
                //return containing in cell's content
                return cells[Normalize(name)].content;
            }
            return cellContents;
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>

        protected override IList<string> SetCellContents(string name, double number)
        {

            // Double type of new cell
            Cell cell = new Cell(number);
            //Using helper method for adding.
            AddCell(cell, name);
            //Replace dependee do add and remove at the same time.
            dependencyGraph.ReplaceDependees(name, new HashSet<string>());

            return GetCellsToRecalculate(name).ToList();
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>

        protected override IList<string> SetCellContents(string name, string text)
        {
            //@511
            // Double type of new cell
            Cell cell = new Cell(text);
            //Using helper method for adding.
            AddCell(cell, name);
            //update dependencyGraph by sending 
            dependencyGraph.ReplaceDependees(name, new HashSet<string>());

            return GetCellsToRecalculate(name).ToList();
        }

        /// <summary>
        /// If name is invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException, and no change is made to the spreadsheet.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>

        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            //Tempory hold current dependee for cycle case.
            IEnumerable<string> currentDependee = dependencyGraph.GetDependees(name);
            //Tempory hold cuurent Cell for cycle case.
            Cell IdealCell = new Cell();
            //List for Recalculating version.
            List<string> reCalList = new List<string>();

            //new formula might contain different variables so update it. 
            dependencyGraph.ReplaceDependees(name, formula.GetVariables());

            try
            {
                reCalList = GetCellsToRecalculate(name).ToList();
            }
            catch (CircularException e)
            {
                //If CircularExcption happens, replace the dependees past dependees.
                dependencyGraph.ReplaceDependees(name, currentDependee);

                if (!cells.ContainsKey(name))
                {
                    //Edge case such as A1 = B1, B1 = A1.
                    //B1 cells' contents should be "".
                    cells.Add(name, IdealCell);
                }
                throw e;
                //STOP
            }
            //The process when circular does not happen. We need to update the Cells'
            //contents to new formula.
            IdealCell = new Cell(formula);
            AddCell(IdealCell, name);

            return reCalList;

        }

        /// <summary>
        /// Returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>

        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            return dependencyGraph.GetDependents(name);
        }

        //Helper method for saving cell to cells (Dictionary). 
        private void AddCell(Cell cell, string name)
        {
            // For replacing.
            if (cells.ContainsKey(name))
            {
                cells[name] = cell;
            }
            else
            {
                //add cell to cells (dictionary)
                cells.Add(name, cell);
            }
        }
        /// <summary>
        /// Private object class for Cell.
        /// </summary>

        [JsonObject(MemberSerialization.OptIn)]
        private class Cell
        {
            public object content { get; set; }

            public object value { get; set; }

            public CellType Type { get; set; }

            [JsonProperty]
            public string stringForm { get; set; }

            // Double type of Cell.
            public Cell(double content)
            {
                this.content = content;
                this.value = content;
                this.Type = CellType.Double;
                this.stringForm = content.ToString();

            }
            // string type of Cell. (text)
            public Cell(string content)
            {
                this.content = content;
                this.value = content;
                this.Type = CellType.Text;
                this.stringForm = content.ToString();
            }
            // Formula type of Cell.
            public Cell(Formula content)
            {
                this.content = content;
                this.Type = CellType.Formula;
                value = "";
                this.stringForm = "=" + content.ToString();
            }
            // Empty cell. 
            public Cell()
            {
                this.content = "";
                this.value = "";
                this.Type = CellType.temp;
                this.stringForm = "";
            }
            //======================Added for PS5=============================
            //Helper method
            /// <summary>
            /// For re-evaluating formulas.
            /// If cell type if Formula, we need to update the "value"
            /// 
            /// ex) A1 = 1
            ///     A2 = A1(content), 1(value)
            /// </summary>
            /// <param name="lookup"> get string and return double,delegate for value </param>
            public void valueUpdate(Func<string, double> lookup)
            {
                if (this.Type == CellType.Formula)
                {
                    Formula formula = (Formula)this.content;
                    this.value = formula.Evaluate(lookup);
                }
            }

        }

        public enum CellType
        {
            Double,

            Text,

            Formula,

            temp,

        };

        /// <summary>
        /// For evaluating function
        /// </summary>
        /// <param name="s"> Cell name that need to be looked up</param>
        /// <returns>The s name of cell's "value" </returns>
        /// <exception cref="ArgumentException"></exception>
        private double LookupValue(string s)
        {
            if (cells.ContainsKey(s))
            {
                Cell cell = cells[s];

                if (cell.value is double && (cell.value.GetType() != typeof(FormulaError)))
                {
                    return (double)cell.value;
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            else // if it does not contain 's' throw an exception
                throw new ArgumentException();
        }
    }
}