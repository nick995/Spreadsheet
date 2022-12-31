/// Min-Gyu Jung
/// CS 3500
/// September 02, 2022


using System.Text.RegularExpressions;

namespace FormulaEvaluator
{

    /// <summary>
    /// 
    /// Evaluate the providen expressions using standard infix notation.
    /// Variable will be converted to integer through the Lookup method.
    /// 
    /// </summary>

    public static class Evaluator
    {
        /// <summary>
        /// Delegate Lookup method for converting variables to Integer for arithmetic expression.
        /// </summary>
        /// <param name="v"> The name of the variables. </param>
        /// <returns> An Integer that is allocated .  </returns>
        public delegate int Lookup(String v);



        /// <summary>
        /// 
        /// Evaluate the expression that is provided through parameter. Formula only can accept of non-negative
        /// integer numbers, variables, left and right parentheses, and the four operator symbols
        /// +, -, * and /. Using standard infix notation, and it should respect the usual precedence rules.
        /// 
        /// </summary>
        /// <param name="exp">  arithmetic expressions that will be evaluated </param>
        /// <param name="variableEvaluator"> Lookup method that is used for converting variable to Integer </param>
        /// <returns> Evaluated value. </returns>
        /// <exception cref="ArgumentException"> If there is any invaild expression, throw ArgumentException.   </exception>


        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            //
            if (exp == null)
            {
                throw new ArgumentException("Explanation cannot be null");
            }


            string tempOpr = "";

            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            //remove all of White space.
            substrings = substrings.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            //List for saving Tuples that are containing Token and Token type.
            List<Tuple<string, TokenType>> TokenList = new List<Tuple<string, TokenType>>();

            int answer = 0;

            Regex Integer = new Regex("^\\d+$");

            int LParen = 0;
            int RParen = 0;

            //Possible error. Expression is empty.
            if (substrings.Length == -0)
            {
                throw new ArgumentException("Expression cannot be empty.");
            }

            for (int i = 0; i < substrings.Length; i++)
            {
                // to check Left Parentheses
                if (substrings[i] == "(")
                {
                    LParen++;
                    TokenList.Add(Tuple.Create(substrings[i].Trim(), TokenType.LeftParen));
                }//to check Right Parentheses
                else if (substrings[i] == ")")
                {
                    RParen++;
                    TokenList.Add(Tuple.Create(substrings[i].Trim(), TokenType.RightParen));
                }
                //to check Variable
                else if (Regex.IsMatch(substrings[i], "^[a-zA-Z][0-9]"))
                {
                    string tempVar = "";
                    //Possible Errors. Variable is not defined.
                    try
                    {
                        // Convert Variable to Integer through LookUp method.
                        tempVar = variableEvaluator(substrings[i]).ToString();
                    }
                    catch (ArgumentException)
                    {
                    }
                    TokenList.Add(Tuple.Create(tempVar, TokenType.Variable));
                }
                //to check Int (Integer)
                else if (Regex.IsMatch(substrings[i].Trim(), "^\\d+$"))
                {
                    TokenList.Add(Tuple.Create(substrings[i].Trim(), TokenType.Integer));
                }
                //to check arithmetical operators. (https://math.hws.edu/eck/cs229_f07/regular_expressions.html)
                else if (Regex.IsMatch(substrings[i].Trim(), "[+\\-*/]"))
                {
                    TokenList.Add(Tuple.Create(substrings[i].Trim(), TokenType.Operation));
                }
                else
                {
                    throw new ArgumentException("Invaild Format.");
                }
            }

            // Number of LParen and RParen must be same.
            if (LParen != RParen)
            {
                throw new ArgumentException("LParen and RParen");
            }

            //stack for value.
            Stack<int> valueStack = new Stack<int>();
            //stack for operation.
            Stack<string> operStack = new Stack<string>();

            Boolean isFirst = true;            //boolean for checking if it is first token.

            Boolean isLast = false;            //boolean for checking if it is last token.

            int firstVal = 0;
            int secondVal = 0;


            // foreach loop for itereating all tokens in TokenList.
            foreach (Tuple<string, TokenType> token in TokenList)
            {
                string currentToken = token.Item1;
                TokenType currentType = token.Item2;

                if (isFirst)
                {
                    // ), and operation cannot come at first.
                    if (currentType == TokenType.RightParen || currentType == TokenType.Operation)
                    {
                        throw new ArgumentException("Invaild expression" + currentToken + "cannot come first");
                    }

                    isFirst = false;
                }

                if (token == TokenList[TokenList.Count - 1])
                {
                    isLast = true;
                }

                if (isLast)
                {
                    //Possible error. last Token cannot be operation or LParen.
                    if (currentType == TokenType.Operation || currentType == TokenType.LeftParen)
                    {
                        throw new ArgumentException(currentToken + "cannot come at the end");
                    }

                    isLast = false;

                }

                int tempVal = 0;

                // ======================================= TokenType is Integer or Variable =======================================
                if (currentType == TokenType.Integer || currentType == TokenType.Variable)
                {

                    int currentVal = 0;

                    Int32.TryParse(currentToken, out currentVal);

                    if (operStack.Count != 0 && (OnTop(operStack, "*") || OnTop(operStack, "/")))
                    {
                        //Possible Errors 
                        //The value stack is empty if thrying to pop it.
                        if (valueStack.Count == 0)
                        {
                            throw new ArgumentException("value stack is empty.");
                        }
                        tempOpr = operStack.Pop();
                        tempVal = valueStack.Pop();

                        valueStack.Push(Calculate(tempVal, currentVal, tempOpr));
                    }
                    else
                    {
                        valueStack.Push(currentVal);
                    }
                    if (TokenList.Count == 1 && valueStack.Count == 1)
                    {
                        return valueStack.Pop();
                    }
                }//Integer variables end.

                //=======================================when TokenTpye is operation================================================
                else if (currentType == TokenType.Operation)
                {
                    if ((operStack.Count > 0) && (currentToken == "+" || currentToken == "-") && ((OnTop(operStack, "+") || OnTop(operStack, "-"))))
                    {
                        //Possible error.
                        //The value stack contains fewer than 2 values.
                        if (valueStack.Count < 2)
                        {
                            throw new ArgumentException("Value stack contains fewer than 2 values.");
                        }

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

                //===================================if currentType is leftParen===================================
                else if (currentType == TokenType.LeftParen)
                {
                    operStack.Push(currentToken);
                }//LeftParen end.

                //=================================== If currentType is rightParen===================================
                else if (currentType == TokenType.RightParen)
                {
                    if (operStack.Count > 0 && ((OnTop(operStack, "+") || OnTop(operStack, "-"))))
                    {
                        //Possible Error. The stack contains fewer than 2 values.                        
                        if (valueStack.Count < 2)
                        {
                            throw new ArgumentException("Value stack contains fewer than 2 values");
                        }

                        secondVal = valueStack.Pop();
                        firstVal = valueStack.Pop();

                        tempOpr = operStack.Pop();

                        valueStack.Push(Calculate(firstVal, secondVal, tempOpr));

                    }

                    if (operStack.Pop() != "(")
                    {
                        throw new ArgumentException("LeftParn is not found");
                    }

                    if (operStack.Count > 0 && ((OnTop(operStack, "*") || OnTop(operStack, "/"))))
                    {

                        if (valueStack.Count < 2)
                        {
                            throw new ArgumentException("Value stack contains fewer than 2 values");
                        }


                        secondVal = valueStack.Pop();
                        firstVal = valueStack.Pop();
                        tempOpr = operStack.Pop();

                        valueStack.Push(Calculate(firstVal, secondVal, tempOpr));
                    }

                }//RightParen end.
            }
            if (operStack.Count == 0)
            {
                if (valueStack.Count != 1)
                {
                    throw new ArgumentException("Value stack should contain only one value");
                }
                else
                {
                    answer = valueStack.Pop();
                }
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
                else
                {
                    throw new ArgumentException("Number of value should be 2.");
                }
            }
            return answer;

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
        /// <exception cref="ArgumentException"></exception>
        private static int Calculate(int firstVal, int secondVal, string oper)
        {
            if (oper == "+")
            {
                return firstVal + secondVal;
            }
            else if (oper == "-")
            {
                return firstVal - secondVal;
            }
            else if (oper == "*")
            {
                return firstVal * secondVal;
            }
            else if (oper == "/")
            {

                if (secondVal == 0)
                {
                    throw new ArgumentException("Division cannot 0");
                }

                return firstVal / secondVal;
            }

            return 0;
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
        //========================Helper Method End=============================

    }

    /// <summary>
    /// 
    /// An enmeration types for identifing token types.
    /// 
    /// </summary>
    public enum TokenType
    {
        Operation,

        Integer,

        Variable,

        LeftParen,

        RightParen,
    };

}


