using SS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using SpreadsheetUtilities;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace SpreadsheetTest
{
    [TestClass]
    public class SpreadsheetTests
    {


        [TestMethod()]
        [TestCategory("Save&Load")]
        public void SaveLoadTest1()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            //add cell
            s.SetContentsOfCell("A1", "HELLOWORLD");
            // Save as test json.
            s.Save("test.json");
            //bring the test.json
            s = new Spreadsheet("test.json", s => true, s => s, "default");

            Assert.AreEqual("HELLOWORLD", s.GetCellContents("A1"));
        }

        [TestMethod()]
        [TestCategory("Save&Load")]
        public void SaveLoadTest2()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            //add cell
            s.SetContentsOfCell("Z7", "1.5");
            // Save as test json.
            Assert.IsTrue(s.Changed);

            s.Save("test.json");

            Assert.IsFalse(s.Changed);

            //bring the test.json
            s = new Spreadsheet("test.json", s => true, s => s, "default");

            Assert.AreEqual(1.5, (double)s.GetCellContents("Z7"), 1e-9);
        }

        [TestMethod()]
        [TestCategory("Save&Load")]
        public void SaveLoadTest3()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            //add cell
            s.SetContentsOfCell("A1", "1.0");
            s.SetContentsOfCell("A2", "1.0");
            s.SetContentsOfCell("A3", "A1+A2");

            // Save as test json.
            s.Save("test.json");
            //bring the test.json
            s = new Spreadsheet("test.json", s => true, s => s, "default");
            Assert.IsFalse(s.Changed);


            Assert.AreEqual("A1+A2", s.GetCellContents("A3"));
            Assert.AreEqual("A1+A2", s.GetCellValue("A3"));

        }

        [TestMethod()]
        [TestCategory("Save&Load")]
        public void SaveLoadTest4()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            //add cell
            s.SetContentsOfCell("A1", "=1+1");
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A2", "6");
            s.SetContentsOfCell("A3", "=A2+A4");
            s.SetContentsOfCell("A4", "=A2+A5");
            List<string> answer = new List<string>() { "B1" };
            Assert.IsTrue(s.Changed);
            // Save as test json.
            s.Save("test.json");
            Assert.IsFalse(s.Changed);
            //bring the test.json
            s = new Spreadsheet("test.json", s => true, s => s, "default");
            Assert.IsFalse(s.Changed);
            Assert.IsTrue(s.SetContentsOfCell("A5", "82.5").SequenceEqual(new List<string>() { "A5", "A4", "A3", "A1" }));

        }
        //Case1. Version does not match parameter provided
        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        [TestCategory("SpreadsheetReadWriteException")]
        public void SpreadException1()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            //add cell
            s.SetContentsOfCell("A1", "=1+1");
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A2", "6");
            s.SetContentsOfCell("A3", "=A2+A4");
            s.SetContentsOfCell("A4", "=A2+A5");
            List<string> answer = new List<string>() { "B1" };

            // Save as test json.
            s.Save("test.json");
            //bring the test.json
            s = new Spreadsheet("test.json", s => true, s => s, "wrong");
            Assert.IsTrue(s.SetContentsOfCell("A5", "82.5").SequenceEqual(new List<string>() { "A5", "A4", "A3", "A1" }));
        }

        //Case2. If any of the names contained in the saved spreadsheet are invalid
        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        [TestCategory("SpreadsheetReadWriteException")]
        public void SpreadException2()
        {
            AbstractSpreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "=1+1");

            s.Save("testjson");
            //bring the test.json
            s = new Spreadsheet("test.json", s => true, s => s, "wrong");
            Assert.IsTrue(s.SetContentsOfCell("A5", "82.5").SequenceEqual(new List<string>() { "A5", "A4", "A3", "A1" }));
        }

        //Case3. If there are any problems opening, reading, or closing the file (such as the path not existing)
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        [TestCategory("SpreadsheetReadWriteException")]
        public void SpreadException3()
        {
            // should not be able to read 
            AbstractSpreadsheet ss = new Spreadsheet("", s => true, s => s, "default");
        }

        //Case3. If there are any problems opening, reading, or closing the file (such as the path not existing)
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        [TestCategory("SpreadsheetReadWriteException")]
        public void SpreadException4()
        {
            // should not be able to read 
            AbstractSpreadsheet ss = new Spreadsheet("1:", s => true, s => s, "default");
        }

        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SpreadException5()
        {
            // should not be able to read 
            AbstractSpreadsheet ss = new Spreadsheet("test.json", s => true, s => s, "");
        }

        //Case3. If there are any problems opening, reading, or closing the file (such as the path not existing)
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        [TestCategory("SpreadsheetReadWriteException")]
        public void SpreadException6()
        {
            // should not be able to read 
            AbstractSpreadsheet s = new Spreadsheet();

            s.Save("");
        }
        //=========================================================================================================

        //InvalidNameException

        //

        [TestMethod()]
        [TestCategory("InvalidNameException")]
        [ExpectedException(typeof(InvalidNameException))]
        public void InvalidNameExceptionTest1()
        {
            AbstractSpreadsheet s = new Spreadsheet(s => false, s => s, "");
            s.GetCellContents("()@jfdsna");
        }

        [TestMethod()]
        [TestCategory("InvalidNameException")]
        [ExpectedException(typeof(InvalidNameException))]
        public void InvalidNameExceptionTest2()
        {
            AbstractSpreadsheet s = new Spreadsheet(s => false, s => s, "");
            s.GetCellValue("test");
        }


        [TestMethod()]
        [TestCategory("SetContentsOfCell")]
        public void emptyGetCellValue()
        {
            AbstractSpreadsheet s = new Spreadsheet();

            Assert.AreEqual("", s.GetCellValue("test"));
        }


        [TestMethod()]
        [TestCategory("SetContentsOfCell")]
        public void TestSimpleCell()
        {

            AbstractSpreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "1.0");
            s.SetContentsOfCell("B1", "hello");
            s.SetContentsOfCell("C1", "=A1");

            Assert.AreEqual(1.0, s.GetCellContents("A1"));
            Assert.AreEqual("hello", s.GetCellContents("B1"));
            Assert.AreEqual(1.0, s.GetCellValue("C1"));

        }




        [TestMethod()]
        [TestCategory("SetContentsOfCell")]
        public void TestSimpleEmptyContents()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "");
            Assert.AreEqual("", s.GetCellContents("A1"));
        }

        [TestMethod()]
        [TestCategory("SetContentsOfCell")]
        public void TestSetList()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("B1", "=A1*2");
            s.SetContentsOfCell("C1", "=B1+A1");

            List<string> answer = new List<string>() { "A1", "B1", "C1" };

            Assert.IsTrue(s.SetContentsOfCell("A1", "D1").SequenceEqual(answer));
        }

        [TestMethod()]
        [TestCategory("SetContentsOfCell")]
        public void TestSetList2()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("B1", "=A1*2");
            s.SetContentsOfCell("C1", "=B1+A1");
            s.SetContentsOfCell("D1", "2");

            List<string> answer = new List<string>() { "A1", "B1", "C1" };

            Assert.IsTrue(s.SetContentsOfCell("A1", "D1").SequenceEqual(answer));
        }

        [TestMethod()]
        [TestCategory("SetContentsOfCell")]
        public void TestSetList3()
        {
            Spreadsheet s = new Spreadsheet();

            List<string> answer = new List<string>() { "B1" };

            Assert.IsTrue(s.SetContentsOfCell("B1", "=A1+1").SequenceEqual(answer));
        }

        [TestMethod()]
        [TestCategory("SetContentsOfCell")]
        public void TestSetList4()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "=1+1");

            List<string> answer = new List<string>() { "B1" };

            Assert.IsTrue(s.SetContentsOfCell("B1", "=A1+1").SequenceEqual(answer));
        }

        [TestMethod()]
        [TestCategory("SetContentsOfCell")]
        public void TestSetList5()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "=1+1");
            s.SetContentsOfCell("C1", "=B1+A1");

            List<string> answer = new List<string>() { "B1", "C1" };

            Assert.IsTrue(s.SetContentsOfCell("B1", "=A1+1").SequenceEqual(answer));
        }

        [TestMethod()]
        [TestCategory("SetContentsOfCell")]
        public void TestSetList6()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "=D1");
            s.SetContentsOfCell("C1", "=A1");
            s.SetContentsOfCell("D1", "=B1+E1");
            s.SetContentsOfCell("E1", "=B1");
            s.SetContentsOfCell("F1", "=1+1");

            List<string> answer = new List<string>() { "E1", "B1", "D1", "A1", "C1" };
            IList<string> output = s.SetContentsOfCell("B1", "=F1");

            for (int i = 0; i < output.Count; i++)
            {
                Assert.IsTrue(answer.Contains(output[i]));
            }
        }

        [TestMethod()]
        [TestCategory("SetContentsOfCell")]
        public void TestSetList7()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "=C1");
            s.SetContentsOfCell("B1", "1+1");
            s.SetContentsOfCell("C1", "=D1");
            s.SetContentsOfCell("D1", "=E1");
            s.SetContentsOfCell("E1", "=F1+F1");

            List<string> answer = new List<string>() { "B1" };
            IList<string> output = s.SetContentsOfCell("E1", "=1+1");

            Assert.IsTrue(!output.Contains(answer.First()));

        }

        [TestMethod()]
        [TestCategory("SetContentsOfCell")]
        public void TestSetChange()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "=B1");
            s.SetContentsOfCell("B1", "=1+1");
            s.SetContentsOfCell("B1", "1");

            Assert.AreEqual(1.0, s.GetCellContents("B1"));

        }

        [TestMethod()]
        [TestCategory("SetContentsOfCell")]
        public void TestSetChange2()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "=B1");
            s.SetContentsOfCell("B1", "=1+1");
            s.SetContentsOfCell("B1", "Hello");

            Assert.AreEqual("Hello", s.GetCellContents("B1"));

        }

        [TestMethod()]
        [TestCategory("SetContentsOfCell")]
        public void TestSetChange3()
        {
            Spreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "=B1");
            s.SetContentsOfCell("B1", "=1+1");
            s.SetContentsOfCell("B1", "=2+2");
            Formula f = new Formula("2+2");
            Assert.AreEqual(f, s.GetCellContents("B1"));

        }
        //==========================PS5=====================

        [TestMethod]
        [TestCategory("SetContentsOfCell")]
        [ExpectedException(typeof(InvalidNameException))]
        public void IsValidTest1()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => s[0] != 'A', s => s, "");
            ss.SetContentsOfCell("A1", "x");
        }

        [TestMethod()]
        [TestCategory("Normalize")]
        public void TestNormalize1()
        {

            AbstractSpreadsheet s = new Spreadsheet(s => true, s => s.ToUpper(), "");

            s.SetContentsOfCell("a1", "1.0");
            s.SetContentsOfCell("b1", "hello");
            s.SetContentsOfCell("c1", "=a1");

            Assert.AreEqual(1.0, s.GetCellContents("A1"));
            Assert.AreEqual("hello", s.GetCellContents("B1"));
            Assert.AreEqual(1.0, s.GetCellValue("C1"));

        }

        [TestMethod()]
        [TestCategory("Normalize")]
        public void TestNormalize2()
        {

            AbstractSpreadsheet s = new Spreadsheet(s => true, s => s.ToUpper(), "");

            s.SetContentsOfCell("a1", "1.0");
            s.SetContentsOfCell("b1", "hello");
            s.SetContentsOfCell("c1", "= A1");

            Formula f = new Formula("A1");

            Assert.AreEqual(1.0, s.GetCellContents("A1"));
            Assert.AreEqual("hello", s.GetCellContents("B1"));
            Assert.AreEqual(f, s.GetCellContents("C1"));

        }

        [TestMethod()]
        [TestCategory("Normalize")]
        public void TestNormalize3()
        {

            AbstractSpreadsheet s = new Spreadsheet(s => true, s => s.ToLower(), "");

            s.SetContentsOfCell("A1", "1.0");
            s.SetContentsOfCell("B1", "hello");
            s.SetContentsOfCell("C1", "= A1");

            Formula f = new Formula("a1");

            Assert.AreEqual(1.0, s.GetCellContents("a1"));
            Assert.AreEqual("hello", s.GetCellContents("b1"));
            Assert.AreEqual(f, s.GetCellContents("c1"));

        }



        //=====================================================
        [TestMethod()]
        [TestCategory("Cycle")]
        [ExpectedException(typeof(CircularException))]
        public void TestCycleCase()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "=A1");
        }

        [TestMethod()]
        [TestCategory("Cycle")]
        [ExpectedException(typeof(CircularException))]
        public void TestCycleCase2()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "=A3");
            s.SetContentsOfCell("A3", "=A4");
            s.SetContentsOfCell("A4", "=A1");

        }

        [TestMethod()]
        [TestCategory("Cycle")]
        [ExpectedException(typeof(CircularException))]
        public void TestCycleCase3()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "=A3");
            s.SetContentsOfCell("A3", "=A1+A2");

        }

        [TestMethod()]
        [TestCategory("Cycle")]
        [ExpectedException(typeof(CircularException))]
        public void TestCycleCase4()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "=A4");
            s.SetContentsOfCell("A3", "=A4");
            s.SetContentsOfCell("A4", "=A1");
        }

        [TestMethod()]
        [TestCategory("Cycle")]
        [ExpectedException(typeof(CircularException))]
        public void TestCycleCaseWithDouble5()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+1");
            s.SetContentsOfCell("A2", "=A3+1");
            s.SetContentsOfCell("A3", "=A4+1");
            s.SetContentsOfCell("A4", "=A1+2");
        }


        [TestMethod()]
        [TestCategory("Cycle")]
        [ExpectedException(typeof(CircularException))]
        public void TestCycleCaseWithDouble6()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+1");
            s.SetContentsOfCell("A2", "=A3+1");
            s.SetContentsOfCell("A3", "=A4+1");
            s.SetContentsOfCell("A4", "=A5+A6+A7+A8+2.15151+A1+2");
        }

        [TestMethod()]
        [TestCategory("Cycle")]
        [ExpectedException(typeof(CircularException))]
        public void TestCycleCaseAfterChange()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+1");
            s.SetContentsOfCell("A2", "1");
            s.SetContentsOfCell("A3", "1");
            s.SetContentsOfCell("A2", "=A1+2");
        }

        [TestMethod()]
        [TestCategory("Cycle")]
        [ExpectedException(typeof(CircularException))]
        public void TestCycleUndo()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "1");
            s.SetContentsOfCell("A2", "=A1");
        }

        [TestMethod()]
        [TestCategory("Cycle")]
        [ExpectedException(typeof(CircularException))]
        public void TestCycleUndo2()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "HELLO");
            s.SetContentsOfCell("A2", "=A1");
        }

        [TestMethod()]
        [TestCategory("Cycle")]
        [ExpectedException(typeof(CircularException))]
        public void TestCycleUndo3()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2");
            s.SetContentsOfCell("A2", "=A3");
            s.SetContentsOfCell("A2", "=A1");
        }

        [TestMethod()]
        [TestCategory("Cycle")]
        [ExpectedException(typeof(CircularException))]
        public void TestCycleUndo4()
        {
            Spreadsheet s = new Spreadsheet();

            try
            {
                s.SetContentsOfCell("A1", "=A2");
                s.SetContentsOfCell("A2", "=A1");
            }
            catch (CircularException e)
            {
                throw e;
            }
            finally
            {
                Assert.AreEqual("", s.GetCellContents("A2"));
                HashSet<string> correctAnswer = new HashSet<string>() { "A1" };
                HashSet<string> result = new HashSet<string>(s.GetNamesOfAllNonemptyCells());

                Assert.IsTrue(correctAnswer.SetEquals(result));
            }
        }

        [TestMethod()]
        [TestCategory("31")]
        public void TestStress1()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=B1+B2");
            s.SetContentsOfCell("B1", "=C1-C2");
            s.SetContentsOfCell("B2", "=C3*C4");
            s.SetContentsOfCell("C1", "=D1*D2");
            s.SetContentsOfCell("C2", "=D3*D4");
            s.SetContentsOfCell("C3", "D5*D6");
            s.SetContentsOfCell("C4", "=D7*D8");
            s.SetContentsOfCell("D1", "=E1");
            s.SetContentsOfCell("D2", "=E1");
            s.SetContentsOfCell("D3", "=E1");
            s.SetContentsOfCell("D4", "=E1");
            s.SetContentsOfCell("D5", "=E1");
            s.SetContentsOfCell("D6", "=E1");
            s.SetContentsOfCell("D7", "=E1");
            s.SetContentsOfCell("D8", "=E1");
            IList<String> cells = s.SetContentsOfCell("E1", "0");
        }

        // Repeated for extra weight
        [TestMethod()]
        [TestCategory("32")]
        public void TestStress1a()
        {
            TestStress1();
        }
        [TestMethod()]
        [TestCategory("33")]
        public void TestStress1b()
        {
            TestStress1();
        }
        [TestMethod()]
        [TestCategory("34")]
        public void TestStress1c()
        {
            TestStress1();
        }

        [TestMethod()]
        [TestCategory("35")]
        public void TestStress2()
        {
            Spreadsheet s = new Spreadsheet();
            ISet<String> cells = new HashSet<string>();
            for (int i = 1; i < 200; i++)
            {
                cells.Add("A" + i);
                Assert.IsTrue(cells.SetEquals(s.SetContentsOfCell("A" + i, "=A" + (i + 1))));
            }
        }
        [TestMethod()]
        [TestCategory("36")]
        public void TestStress2a()
        {
            TestStress2();
        }
        [TestMethod()]
        [TestCategory("37")]
        public void TestStress2b()
        {
            TestStress2();
        }
        [TestMethod()]
        [TestCategory("38")]
        public void TestStress2c()
        {
            TestStress2();
        }

        [TestMethod()]
        [TestCategory("39")]
        public void TestStress3()
        {
            Spreadsheet s = new Spreadsheet();
            for (int i = 1; i < 200; i++)
            {
                s.SetContentsOfCell("A" + i, "=A" + (i + 1));
            }
            try
            {
                s.SetContentsOfCell("A150", "=A50");
                Assert.Fail();
            }
            catch (CircularException)
            {
            }
        }

        [TestMethod()]
        [TestCategory("40")]
        public void TestStress3a()
        {
            TestStress3();
        }
        [TestMethod()]
        [TestCategory("41")]
        public void TestStress3b()
        {
            TestStress3();
        }
        [TestMethod()]
        [TestCategory("42")]
        public void TestStress3c()
        {
            TestStress3();
        }

        [TestMethod()]
        [TestCategory("43")]
        public void TestStress4()
        {
            Spreadsheet s = new Spreadsheet();
            for (int i = 0; i < 500; i++)
            {
                s.SetContentsOfCell("A1" + i, "=A1" + (i + 1));
            }
            LinkedList<string> firstCells = new LinkedList<string>();
            LinkedList<string> lastCells = new LinkedList<string>();
            for (int i = 0; i < 250; i++)
            {
                firstCells.AddFirst("A1" + i);
                lastCells.AddFirst("A1" + (i + 250));
            }
            Assert.IsTrue(s.SetContentsOfCell("A1249", "25.0").SequenceEqual(firstCells));
            Assert.IsTrue(s.SetContentsOfCell("A1499", "0").SequenceEqual(lastCells));
        }

        [TestMethod()]
        [TestCategory("44")]
        public void TestStress4a()
        {
            TestStress4();
        }
        [TestMethod()]
        [TestCategory("45")]
        public void TestStress4b()
        {
            TestStress4();
        }
        [TestMethod()]
        [TestCategory("46")]
        public void TestStress4c()
        {
            TestStress4();
        }

        [TestMethod()]
        [TestCategory("47")]
        public void TestStress5()
        {
            RunRandomizedTest(47, 2519);
        }

        [TestMethod()]
        [TestCategory("48")]
        public void TestStress6()
        {
            RunRandomizedTest(48, 2521);
        }

        [TestMethod()]
        [TestCategory("49")]
        public void TestStress7()
        {
            RunRandomizedTest(49, 2526);
        }

        [TestMethod()]
        [TestCategory("50")]
        public void TestStress8()
        {
            RunRandomizedTest(50, 2521);
        }

        /// <summary>
        /// Sets random contents for a random cell 10000 times
        /// </summary>
        /// <param name="seed">Random seed</param>
        /// <param name="size">The known resulting spreadsheet size, given the seed</param>
        public void RunRandomizedTest(int seed, int size)
        {
            Spreadsheet s = new Spreadsheet();
            Random rand = new Random(seed);
            for (int i = 0; i < 10000; i++)
            {
                try
                {
                    switch (rand.Next(3))
                    {
                        case 0:
                            s.SetContentsOfCell(randomName(rand), "3.141592");
                            break;
                        case 1:
                            s.SetContentsOfCell(randomName(rand), "hello");
                            break;
                        case 2:
                            s.SetContentsOfCell(randomName(rand), randomFormula(rand));
                            break;
                    }
                }
                catch (CircularException)
                {
                }
            }
            ISet<string> set = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.AreEqual(size, set.Count);
        }

        /// <summary>
        /// Generates a random cell name with a capital letter and number between 1 - 99
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        private String randomName(Random rand)
        {
            return "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(rand.Next(26), 1) + (rand.Next(99) + 1);
        }

        /// <summary>
        /// Generates a random Formula
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        private String randomFormula(Random rand)
        {
            String f = randomName(rand);
            for (int i = 0; i < 10; i++)
            {
                switch (rand.Next(4))
                {
                    case 0:
                        f += "+";
                        break;
                    case 1:
                        f += "-";
                        break;
                    case 2:
                        f += "*";
                        break;
                    case 3:
                        f += "/";
                        break;
                }
                switch (rand.Next(2))
                {
                    case 0:
                        f += 7.2;
                        break;
                    case 1:
                        f += randomName(rand);
                        break;
                }
            }
            return f;
        }

        // Verifies cells and their values, which must alternate.
        public void VV(AbstractSpreadsheet sheet, params object[] constraints)
        {
            for (int i = 0; i < constraints.Length; i += 2)
            {
                if (constraints[i + 1] is double)
                {
                    Assert.AreEqual((double)constraints[i + 1], (double)sheet.GetCellValue((string)constraints[i]), 1e-9);
                }
                else
                {
                    Assert.AreEqual(constraints[i + 1], sheet.GetCellValue((string)constraints[i]));
                }
            }
        }


        // For setting a spreadsheet cell.
        public IEnumerable<string> Set(AbstractSpreadsheet sheet, string name, string contents)
        {
            List<string> result = new List<string>(sheet.SetContentsOfCell(name, contents));
            return result;
        }

        // Tests IsValid
        [TestMethod, Timeout(2000)]
        [TestCategory("1")]
        public void IsValidTest11()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "x");
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("2")]
        [ExpectedException(typeof(InvalidNameException))]
        public void IsValidTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => s[0] != 'A', s => s, "");
            ss.SetContentsOfCell("A1", "x");
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("3")]
        public void IsValidTest3()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "= A1 + C1");
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("4")]
        [ExpectedException(typeof(FormulaFormatException))]
        public void IsValidTest4()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => s[0] != 'A', s => s, "");
            ss.SetContentsOfCell("B1", "= A1 + C1");
        }

        // Tests Normalize
        [TestMethod, Timeout(2000)]
        [TestCategory("5")]
        public void NormalizeTest1()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            Assert.AreEqual("", s.GetCellContents("b1"));
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("6")]
        public void NormalizeTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => true, s => s.ToUpper(), "");
            ss.SetContentsOfCell("B1", "hello");
            Assert.AreEqual("hello", ss.GetCellContents("b1"));
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("7")]
        public void NormalizeTest3()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("a1", "5");
            s.SetContentsOfCell("A1", "6");
            s.SetContentsOfCell("B1", "= a1");
            Assert.AreEqual(5.0, (double)s.GetCellValue("B1"), 1e-9);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("8")]
        public void NormalizeTest4()
        {
            AbstractSpreadsheet ss = new Spreadsheet(s => true, s => s.ToUpper(), "");
            ss.SetContentsOfCell("a1", "5");
            ss.SetContentsOfCell("A1", "6");
            ss.SetContentsOfCell("B1", "= a1");
            Assert.AreEqual(6.0, (double)ss.GetCellValue("B1"), 1e-9);
        }

        // Simple tests
        [TestMethod, Timeout(2000)]
        [TestCategory("9")]
        public void EmptySheet()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            VV(ss, "A1", "");
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("10")]
        public void OneString()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            OneString(ss);
        }

        public void OneString(AbstractSpreadsheet ss)
        {
            Set(ss, "B1", "hello");
            VV(ss, "B1", "hello");
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("11")]
        public void OneNumber()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            OneNumber(ss);
        }

        public void OneNumber(AbstractSpreadsheet ss)
        {
            Set(ss, "C1", "17.5");
            VV(ss, "C1", 17.5);
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("12")]
        public void OneFormula()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            OneFormula(ss);
        }

        public void OneFormula(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "B1", "5.2");
            Set(ss, "C1", "= A1+B1");
            VV(ss, "A1", 4.1, "B1", 5.2, "C1", 9.3);
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("13")]
        public void ChangedAfterModify()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Assert.IsFalse(ss.Changed);
            Set(ss, "C1", "17.5");
            Assert.IsTrue(ss.Changed);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("13b")]
        public void UnChangedAfterSave()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Set(ss, "C1", "17.5");
            ss.Save("changed.txt");
            Assert.IsFalse(ss.Changed);
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("14")]
        public void DivisionByZero1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            DivisionByZero1(ss);
        }

        public void DivisionByZero1(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "B1", "0.0");
            Set(ss, "C1", "= A1 / B1");
            Assert.IsInstanceOfType(ss.GetCellValue("C1"), typeof(FormulaError));
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("15")]
        public void DivisionByZero2()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            DivisionByZero2(ss);
        }

        public void DivisionByZero2(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "5.0");
            Set(ss, "A3", "= A1 / 0.0");
            Assert.IsInstanceOfType(ss.GetCellValue("A3"), typeof(FormulaError));
        }



        [TestMethod, Timeout(2000)]
        [TestCategory("16")]
        public void EmptyArgument()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            EmptyArgument(ss);
        }

        public void EmptyArgument(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "C1", "= A1 + B1");
            Assert.IsInstanceOfType(ss.GetCellValue("C1"), typeof(FormulaError));
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("17")]
        public void StringArgument()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            StringArgument(ss);
        }

        public void StringArgument(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "B1", "hello");
            Set(ss, "C1", "= A1 + B1");
            Assert.IsInstanceOfType(ss.GetCellValue("C1"), typeof(FormulaError));
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("18")]
        public void ErrorArgument()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ErrorArgument(ss);
        }

        public void ErrorArgument(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "B1", "");
            Set(ss, "C1", "= A1 + B1");
            Set(ss, "D1", "= C1");
            Assert.IsInstanceOfType(ss.GetCellValue("D1"), typeof(FormulaError));
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("19")]
        public void NumberFormula1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            NumberFormula1(ss);
        }

        public void NumberFormula1(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.1");
            Set(ss, "C1", "= A1 + 4.2");
            VV(ss, "C1", 8.3);
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("20")]
        public void NumberFormula2()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            NumberFormula2(ss);
        }

        public void NumberFormula2(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "= 4.6");
            VV(ss, "A1", 4.6);
        }


        // Repeats the simple tests all together
        [TestMethod, Timeout(2000)]
        [TestCategory("21")]
        public void RepeatSimpleTests()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Set(ss, "A1", "17.32");
            Set(ss, "B1", "This is a test");
            Set(ss, "C1", "= A1+B1");
            OneString(ss);
            OneNumber(ss);
            OneFormula(ss);
            DivisionByZero1(ss);
            DivisionByZero2(ss);
            StringArgument(ss);
            ErrorArgument(ss);
            NumberFormula1(ss);
            NumberFormula2(ss);
        }

        // Four kinds of formulas
        [TestMethod, Timeout(2000)]
        [TestCategory("22")]
        public void Formulas()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Formulas(ss);
        }

        public void Formulas(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "4.4");
            Set(ss, "B1", "2.2");
            Set(ss, "C1", "= A1 + B1");
            Set(ss, "D1", "= A1 - B1");
            Set(ss, "E1", "= A1 * B1");
            Set(ss, "F1", "= A1 / B1");
            VV(ss, "C1", 6.6, "D1", 2.2, "E1", 4.4 * 2.2, "F1", 2.0);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("23")]
        public void Formulasa()
        {
            Formulas();
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("24")]
        public void Formulasb()
        {
            Formulas();
        }


        // Are multiple spreadsheets supported?
        [TestMethod, Timeout(2000)]
        [TestCategory("25")]
        public void Multiple()
        {
            AbstractSpreadsheet s1 = new Spreadsheet();
            AbstractSpreadsheet s2 = new Spreadsheet();
            Set(s1, "X1", "hello");
            Set(s2, "X1", "goodbye");
            VV(s1, "X1", "hello");
            VV(s2, "X1", "goodbye");
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("26")]
        public void Multiplea()
        {
            Multiple();
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("27")]
        public void Multipleb()
        {
            Multiple();
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("28")]
        public void Multiplec()
        {
            Multiple();
        }

        // Reading/writing spreadsheets
        [TestMethod, Timeout(2000)]
        [TestCategory("29")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest1()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.Save(Path.GetFullPath("/missing/save.txt"));
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("30")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest2()
        {
            AbstractSpreadsheet ss = new Spreadsheet(Path.GetFullPath("/missing/save.txt"), s => true, s => s, "");
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("31")]
        public void SaveTest3()
        {
            AbstractSpreadsheet s1 = new Spreadsheet();
            Set(s1, "A1", "hello");
            s1.Save("save1.txt");
            s1 = new Spreadsheet("save1.txt", s => true, s => s, "default");
            Assert.AreEqual("hello", s1.GetCellContents("A1"));
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("32")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest4()
        {
            using (StreamWriter writer = new StreamWriter("save2.txt"))
            {
                writer.WriteLine("This");
                writer.WriteLine("is");
                writer.WriteLine("a");
                writer.WriteLine("test!");
            }
            AbstractSpreadsheet ss = new Spreadsheet("save2.txt", s => true, s => s, "");
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("33")]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void SaveTest5()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            ss.Save("save3.txt");
            ss = new Spreadsheet("save3.txt", s => true, s => s, "version");
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("35")]
        public void SaveTest7()
        {
            var sheet = new
            {
                cells = new
                {
                    A1 = new { stringForm = "hello" },
                    A2 = new { stringForm = "5.0" },
                    A3 = new { stringForm = "4.0" },
                    A4 = new { stringForm = "= A2 + A3" }
                },
                Version = ""
            };

            File.WriteAllText("save5.txt", JsonConvert.SerializeObject(sheet));


            AbstractSpreadsheet ss = new Spreadsheet("save5.txt", s => true, s => s, "");
            VV(ss, "A1", "hello", "A2", 5.0, "A3", 4.0, "A4", 9.0);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("36")]
        public void SaveTest8()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Set(ss, "A1", "hello");
            Set(ss, "A2", "5.0");
            Set(ss, "A3", "4.0");
            Set(ss, "A4", "= A2 + A3");
            ss.Save("save6.txt");

            string fileContents = File.ReadAllText("save6.txt");

            dynamic o = JObject.Parse(fileContents);

            Assert.AreEqual("default", o.Version.ToString());
            Assert.AreEqual("hello", o.cells.A1.stringForm.ToString());
            Assert.AreEqual(5.0, double.Parse(o.cells.A2.stringForm.ToString()), 1e-9);
            Assert.AreEqual(4.0, double.Parse(o.cells.A3.stringForm.ToString()), 1e-9);
            Assert.AreEqual("=A2+A3", o.cells.A4.stringForm.ToString().Replace(" ", ""));
        }


        // Fun with formulas
        [TestMethod, Timeout(2000)]
        [TestCategory("37")]
        public void Formula1()
        {
            Formula1(new Spreadsheet());
        }
        public void Formula1(AbstractSpreadsheet ss)
        {
            Set(ss, "a1", "= a2 + a3");
            Set(ss, "a2", "= b1 + b2");
            Assert.IsInstanceOfType(ss.GetCellValue("a1"), typeof(FormulaError));
            Assert.IsInstanceOfType(ss.GetCellValue("a2"), typeof(FormulaError));
            Set(ss, "a3", "5.0");
            Set(ss, "b1", "2.0");
            Set(ss, "b2", "3.0");
            VV(ss, "a1", 10.0, "a2", 5.0);
            Set(ss, "b2", "4.0");
            VV(ss, "a1", 11.0, "a2", 6.0);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("38")]
        public void Formula2()
        {
            Formula2(new Spreadsheet());
        }
        public void Formula2(AbstractSpreadsheet ss)
        {
            Set(ss, "a1", "= a2 + a3");
            Set(ss, "a2", "= a3");
            Set(ss, "a3", "6.0");
            VV(ss, "a1", 12.0, "a2", 6.0, "a3", 6.0);
            Set(ss, "a3", "5.0");
            VV(ss, "a1", 10.0, "a2", 5.0, "a3", 5.0);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("39")]
        public void Formula3()
        {
            Formula3(new Spreadsheet());
        }
        public void Formula3(AbstractSpreadsheet ss)
        {
            Set(ss, "a1", "= a3 + a5");
            Set(ss, "a2", "= a5 + a4");
            Set(ss, "a3", "= a5");
            Set(ss, "a4", "= a5");
            Set(ss, "a5", "9.0");
            VV(ss, "a1", 18.0);
            VV(ss, "a2", 18.0);
            Set(ss, "a5", "8.0");
            VV(ss, "a1", 16.0);
            VV(ss, "a2", 16.0);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("40")]
        public void Formula4()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            Formula1(ss);
            Formula2(ss);
            Formula3(ss);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("41")]
        public void Formula4a()
        {
            Formula4();
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("42")]
        public void MediumSheet()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            MediumSheet(ss);
        }

        public void MediumSheet(AbstractSpreadsheet ss)
        {
            Set(ss, "A1", "1.0");
            Set(ss, "A2", "2.0");
            Set(ss, "A3", "3.0");
            Set(ss, "A4", "4.0");
            Set(ss, "B1", "= A1 + A2");
            Set(ss, "B2", "= A3 * A4");
            Set(ss, "C1", "= B1 + B2");
            VV(ss, "A1", 1.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 3.0, "B2", 12.0, "C1", 15.0);
            Set(ss, "A1", "2.0");
            VV(ss, "A1", 2.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 4.0, "B2", 12.0, "C1", 16.0);
            Set(ss, "B1", "= A1 / A2");
            VV(ss, "A1", 2.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 1.0, "B2", 12.0, "C1", 13.0);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("43")]
        public void MediumSheeta()
        {
            MediumSheet();
        }


        [TestMethod, Timeout(2000)]
        [TestCategory("44")]
        public void MediumSave()
        {
            AbstractSpreadsheet ss = new Spreadsheet();
            MediumSheet(ss);
            ss.Save("save7.txt");
            ss = new Spreadsheet("save7.txt", s => true, s => s, "default");
            VV(ss, "A1", 2.0, "A2", 2.0, "A3", 3.0, "A4", 4.0, "B1", 1.0, "B2", 12.0, "C1", 13.0);
        }

        [TestMethod, Timeout(2000)]
        [TestCategory("45")]
        public void MediumSavea()
        {
            MediumSave();
        }


        // A long chained formula. Solutions that re-evaluate 
        // cells on every request, rather than after a cell changes,
        // will timeout on this test.
        // This test is repeated to increase its scoring weight
        [TestMethod, Timeout(6000)]
        [TestCategory("46")]
        public void LongFormulaTest()
        {
            object result = "";
            LongFormulaHelper(out result);
            Assert.AreEqual("ok", result);
        }

        [TestMethod, Timeout(6000)]
        [TestCategory("47")]
        public void LongFormulaTest2()
        {
            object result = "";
            LongFormulaHelper(out result);
            Assert.AreEqual("ok", result);
        }

        [TestMethod, Timeout(6000)]
        [TestCategory("48")]
        public void LongFormulaTest3()
        {
            object result = "";
            LongFormulaHelper(out result);
            Assert.AreEqual("ok", result);
        }

        [TestMethod, Timeout(6000)]
        [TestCategory("49")]
        public void LongFormulaTest4()
        {
            object result = "";
            LongFormulaHelper(out result);
            Assert.AreEqual("ok", result);
        }

        [TestMethod, Timeout(6000)]
        [TestCategory("50")]
        public void LongFormulaTest5()
        {
            object result = "";
            LongFormulaHelper(out result);
            Assert.AreEqual("ok", result);
        }

        public void LongFormulaHelper(out object result)
        {
            try
            {
                AbstractSpreadsheet s = new Spreadsheet();
                s.SetContentsOfCell("sum1", "= a1 + a2");
                int i;
                int depth = 100;
                for (i = 1; i <= depth * 2; i += 2)
                {
                    s.SetContentsOfCell("a" + i, "= a" + (i + 2) + " + a" + (i + 3));
                    s.SetContentsOfCell("a" + (i + 1), "= a" + (i + 2) + "+ a" + (i + 3));
                }
                s.SetContentsOfCell("a" + i, "1");
                s.SetContentsOfCell("a" + (i + 1), "1");
                Assert.AreEqual(Math.Pow(2, depth + 1), (double)s.GetCellValue("sum1"), 1.0);
                s.SetContentsOfCell("a" + i, "0");
                Assert.AreEqual(Math.Pow(2, depth), (double)s.GetCellValue("sum1"), 1.0);
                s.SetContentsOfCell("a" + (i + 1), "0");
                Assert.AreEqual(0.0, (double)s.GetCellValue("sum1"), 0.1);
                result = "ok";
            }
            catch (Exception e)
            {
                result = e;
            }
        }
    }

}