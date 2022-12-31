// Skeleton written by Profs Zachary, Kopta and Martin for CS 3500
// Read the entire skeleton carefully and completely before you
// do anything else!

// Change log:
// Last updated: 9/8, updated for non-nullable types

/// Min-Gyu Jung
/// CS 3500
/// September 16, 2022

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax (without unary preceeding '-' or '+'); 
    /// variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        private HashSet<string> nomalizedHashSet = new HashSet<string>();
        private List<Tuple<string, TokenType>> tokenList;

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {

        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            // One token rule.
            if (formula == null || formula.Length == 0)
            {
                throw new FormulaFormatException("formula cannot be null. Please add string for formula");
            }
            tokenList = new List<Tuple<string, TokenType>>();

            Boolean isFirst = true;
            Boolean isLast = false;

            int lParenCount = 0;
            int RParenCount = 0;

            TokenType prevTokenType = TokenType.WhiteSpace;
            //for tracking index.
            int i = 0;

            foreach (Tuple<string, TokenType> token in TokenConvert(formula))
            {
                string currentToken = token.Item1;
                TokenType currentType = token.Item2;

                if (isFirst)
                {
                    //Starting Token Rule.
                    if (currentType == TokenType.RightParen || currentType == TokenType.Operation)
                    {
                        throw new FormulaFormatException("Invaild Formula. " + currentToken + " cannot come first");
                    }
                    isFirst = false;
                }
                if (i == formula.Length - 1)
                {
                    isLast = true;
                }

                if (isLast)
                {
                    //Ending Token Rule
                    if (currentType == TokenType.Operation || currentType == TokenType.LeftParen)
                    {
                        throw new FormulaFormatException(currentToken + " cannot come at the end");
                    }
                    isLast = false;
                }

                if (currentType == TokenType.RightParen)
                {
                    RParenCount++;
                    //Right Parentheses Rule
                    if (RParenCount > lParenCount)
                    {
                        throw new FormulaFormatException("Right Parenthess cannot be greater than the number of Left Parenthess");
                    }
                }
                // Parenthesis & operator following rule.
                if (prevTokenType == TokenType.LeftParen || prevTokenType == TokenType.Operation)
                {
                    if (currentType == TokenType.Operation || currentType == TokenType.RightParen)
                    {
                        throw new FormulaFormatException(currentToken + " cannot come after opening parenthesis");
                    }
                }

                if (prevTokenType == TokenType.Double || prevTokenType == TokenType.Variable || prevTokenType == TokenType.RightParen)
                {
                    if (!(currentType == TokenType.Operation || currentType == TokenType.RightParen))
                    {
                        throw new FormulaFormatException("After" + prevTokenType + ", " + currentToken + " cannot come");
                    }
                }

                if (currentType == TokenType.LeftParen)
                {
                    lParenCount++;
                }

                if (currentType == TokenType.Double)
                {
                    //Convert number to double type and add again. 
                    tokenList.Add(Tuple.Create(Convert.ToDouble(currentToken).ToString(), currentType));
                }
                else if (currentType == TokenType.Variable)
                {
                    if (!isValid(currentToken))
                    {
                        throw new FormulaFormatException(currentToken + "is not valid");
                    }
                    else if (isValid(normalize(currentToken)))
                    {
                        //Add currentToken to nomalizedHashSet for GetVariables method
                        nomalizedHashSet.Add(normalize(currentToken));
                        tokenList.Add(Tuple.Create(normalize(currentToken), currentType));
                    }
                }
                else
                {
                    tokenList.Add(Tuple.Create(currentToken, currentType));
                }

                //update previous TokenType.
                prevTokenType = currentType;
                i++;
            }
            // Balanced Parentheses Rule.
            if (lParenCount != RParenCount)
            {
                throw new FormulaFormatException("Total number of Opening and Closing parentheses must be same");
            }
        }
        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            //stack for value.
            Stack<double> valueStack = new Stack<double>();
            //stack for operation.
            Stack<string> operStack = new Stack<string>();

            double firstVal = 0;
            double secondVal = 0;

            string tempOpr = "";
            double answer = 0;

            // foreach loop for itereating all tokens in TokenList.
            foreach (Tuple<string, TokenType> token in tokenList)
            {
                string currentToken = token.Item1;
                TokenType currentType = token.Item2;

                double tempVal = 0;

                // ======================================= TokenType is Double or Variable =======================================
                if (currentType == TokenType.Double || currentType == TokenType.Variable)
                {
                    if (currentType == TokenType.Variable)
                    {
                        double tempLook;
                        //try if lookup is vaild, if not, return FormulaError.
                        try
                        {
                            tempLook = lookup(currentToken);

                            currentToken = "" + tempLook;
                            currentType = TokenType.Double;
                        }
                        catch (ArgumentException)
                        {
                            return new FormulaError(currentToken + " is not defined");
                        }
                    }

                    double currentVal = 0;

                    double.TryParse(currentToken, out currentVal);

                    if (operStack.Count != 0 && (OnTop(operStack, "*") || OnTop(operStack, "/")))
                    {
                        tempOpr = operStack.Pop();
                        tempVal = valueStack.Pop();

                        if (currentVal == 0)
                        {
                            return new FormulaError(tempVal + " cannot divide by 0");
                        }

                        valueStack.Push(Calculate(tempVal, currentVal, tempOpr));
                    }
                    else
                    {
                        valueStack.Push(currentVal);
                    }
                    if (tokenList.Count == 1 && valueStack.Count == 1)
                    {
                        return valueStack.Pop();
                    }
                }//Double variables end.

                //=======================================TokenTpye is operation================================================
                else if (currentType == TokenType.Operation)
                {
                    if ((operStack.Count > 0) && (currentToken == "+" || currentToken == "-") && ((OnTop(operStack, "+") || OnTop(operStack, "-"))))
                    {
                        secondVal = valueStack.Pop();
                        firstVal = valueStack.Pop();
                        tempOpr = operStack.Pop();

                        valueStack.Push(Calculate(firstVal, secondVal, tempOpr));

                        operStack.Push(currentToken);
                    }
                    else // push * or / to the operStack.
                    {
                        operStack.Push(currentToken);
                    }
                }//Operation end.

                //===================================TokenType is leftParen===================================
                else if (currentType == TokenType.LeftParen)
                {
                    operStack.Push(currentToken);
                }//LeftParen end.

                //===================================TokenType is rightParen===================================
                else if (currentType == TokenType.RightParen)
                {
                    if (operStack.Count > 0 && ((OnTop(operStack, "+") || OnTop(operStack, "-"))))
                    {
                        secondVal = valueStack.Pop();
                        firstVal = valueStack.Pop();
                        tempOpr = operStack.Pop();

                        valueStack.Push(Calculate(firstVal, secondVal, tempOpr));
                    }
                    //Remove Last LeftParen.
                    operStack.Pop();

                    if (operStack.Count > 0 && ((OnTop(operStack, "*") || OnTop(operStack, "/"))))
                    {
                        secondVal = valueStack.Pop();
                        firstVal = valueStack.Pop();
                        tempOpr = operStack.Pop();

                        if (secondVal == 0)
                        {
                            return new FormulaError(firstVal + " cannot divide by 0");
                        }
                        valueStack.Push(Calculate(firstVal, secondVal, tempOpr));
                    }

                }//RightParen end.
            }
            if (operStack.Count == 0)
            {
                answer = valueStack.Pop();
            }
            else if (operStack.Count == 1)
            {
                tempOpr = operStack.Pop();

                if (valueStack.Count == 2)
                {
                    secondVal = valueStack.Pop();
                    firstVal = valueStack.Pop();
                    answer = Calculate(firstVal, secondVal, tempOpr);
                }
            }
            return answer;
        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            return nomalizedHashSet;
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Tuple<string, TokenType> s in tokenList)
            {
                sb.Append(s.Item1);
            }
            return sb.ToString();
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens and variable tokens.
        /// Numeric tokens are considered equal if they are equal after being "normalized" 
        /// by C#'s standard conversion from string to double, then back to string. This 
        /// eliminates any inconsistencies due to limited floating point precision.
        /// Variable tokens are considered equal if their normalized forms are equal, as 
        /// defined by the provided normalizer.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object? obj)
        {
            //https://docs.microsoft.com/en-us/dotnet/api/system.object.equals?view=net-6.0

            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Formula formula = (Formula)obj;
                //If count is different, it is not equal.
                if (this.tokenList.Count != formula.tokenList.Count)
                {
                    return false;
                }
                for (int i = 0; i < this.tokenList.Count; i++)
                {
                    // Convert string to double and comapre it.
                    if (this.tokenList[i].Item2 == TokenType.Double || formula.tokenList[i].Item2 == TokenType.Double)
                    {
                        if (!Convert.ToDouble(this.tokenList[i].Item1).Equals(Convert.ToDouble(formula.tokenList[i].Item1)))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!this.tokenList[i].Equals(formula.tokenList[i]))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            if (ReferenceEquals(f1, null) || ReferenceEquals(f2, null))
            {
                return false;
            }
            return f1.Equals(f2);
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that f1 and f2 cannot be null, because their types are non-nullable
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            if (ReferenceEquals(f1, null) || ReferenceEquals(f2, null))
            {
                return false;
            }
            return !f1.Equals(f2);
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            int hashCode = this.ToString().GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";
            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);
            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }
        //========================Helper Method=============================

        /// <summary>
        /// 
        /// Helper method for calculating 
        /// 
        /// </summary>
        /// <param name="a"> first value </param>
        /// <param name="b"> second value </param>
        /// <param name="oper"> operation for calcualting. </param>
        /// <returns> Calculated value. </returns>
        private static double Calculate(double firstVal, double secondVal, string oper)
        {
            double result = 0.0;
            if (oper == "+")
            {
                result = firstVal + secondVal;
            }
            else if (oper == "-")
            {
                result = firstVal - secondVal;
            }
            else if (oper == "*")
            {
                result = firstVal * secondVal;
            }
            else if (oper == "/")
            {
                result = firstVal / secondVal;
            }
            return result;
        }
        /// <summary>
        /// 
        /// Helper method to get operation which is located at the top of the stack.
        /// 
        /// </summary>
        /// <param name="operStack"></param>
        /// <param name="oper"></param>
        /// <returns></returns>
        private static bool OnTop(Stack<string> operStack, string oper)
        {
            if (operStack.Peek().Equals(oper))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Convert strings to Tuple Types for convinent and easy to track the TokenType. 
        /// </summary>
        /// <param name="formula"></param>
        /// <returns> tempTokenList </returns>
        private static List<Tuple<string, TokenType>> TokenConvert(String formula)
        {

            List<Tuple<string, TokenType>> tempTokenList = new List<Tuple<string, TokenType>>();

            foreach (String token in GetTokens(formula))
            {
                if (Regex.IsMatch(token, @"\("))
                {
                    tempTokenList.Add(Tuple.Create(token, TokenType.LeftParen));
                }
                else if (Regex.IsMatch(token, @"\)"))
                {
                    tempTokenList.Add(Tuple.Create(token, TokenType.RightParen));
                }
                else if (Regex.IsMatch(token, @"^[\+\-*/]"))
                {
                    tempTokenList.Add(Tuple.Create(token, TokenType.Operation));
                }
                else if (Regex.IsMatch(token, @"^[a-zA-Z_](?: [a-zA-Z_]|\d)*"))
                {
                    tempTokenList.Add(Tuple.Create(token, TokenType.Variable));
                }
                else
                {
                    tempTokenList.Add(Tuple.Create(token, TokenType.Double));
                }
            }
            return tempTokenList;
        }

        /// <summary>
        /// For assigning token types.
        /// </summary>
        public enum TokenType
        {
            Operation,

            Double,

            Variable,

            LeftParen,

            RightParen,

            WhiteSpace
        };
        //========================Helper Method End=============================
    }
}

/// <summary>
/// Used to report syntactic errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    /// Constructs a FormulaFormatException containing the explanatory message.
    /// </summary>
    public FormulaFormatException(String message)
        : base(message)
    {
    }
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public struct FormulaError
{
    /// <summary>
    /// Constructs a FormulaError containing the explanatory reason.
    /// </summary>
    /// <param name="reason"></param>
    public FormulaError(String reason)
        : this()
    {
        Reason = reason;
    }

    /// <summary>
    ///  The reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}