using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace MegacartAssembler
{
    class Assembler
    {
        public static int ProgramCounter;
        public static int EndOfMemory = 63;

        public static string TargetFilePath;
        public static string DestinationFilePath;

        public static string ALUOperationsFilePath;
        public static string InstructionsFilePath;
        public static string ConditionsFilePath;
        public static string InternalAddressesFilePath;

        public static LookupTable LabelTable = new LookupTable("Labels");
        public static LookupTable VariableTable = new LookupTable("Variables");
        public static LookupTable ALUOperationsTable = new LookupTable("ALU Operations");
        public static LookupTable InstructionsTable = new LookupTable("Instructions");
        public static LookupTable ConditionsTable = new LookupTable("Conditions");
        public static LookupTable InternalAddressesTable = new LookupTable("Internal Addresses");

        public static Regex LabelPattern = new Regex(@":[\w]+");
        public static Regex CommentPattern = new Regex(@"//[\w]*");
        public static Regex Binary = new Regex("[0||1]+");

        public static List<string> NewFileLines = new List<string>();

        public static void Main(String[] args)
        {
            string mnemonicsPath;

            if (args.Length == 0)
            {
                //Prompt the user for arguments
                Console.WriteLine("Please input the mnemonics folder file path");
                mnemonicsPath = Console.ReadLine();

                Console.WriteLine("Please input the source code file path (the file to be assembled):");
                TargetFilePath = Console.ReadLine();
            }
            else if (args.Length == 1)
            {
                //Target file only
                TargetFilePath = Path.GetFullPath(args[0]);
                mnemonicsPath = Path.GetDirectoryName(TargetFilePath) + "\\Mnemonics";

            }
            else if (args.Length == 2)
            {
                //Target and Mnemonics
                TargetFilePath = args[0];
                mnemonicsPath = args[1];
            }
            else
            {
                mnemonicsPath = null; //I do this throughout the code, because ReSharper doesn't seem to understand that Environment.Exit(0) exits the program.
                Console.WriteLine("Bad number of arguments passed to the assembler.");
                Environment.Exit(0);
            }

            //Check if files exist
            ALUOperationsFilePath = mnemonicsPath + "\\ALUOperations.txt";
            if (!File.Exists(ALUOperationsFilePath))
            {
                Console.WriteLine(
                    "ALU Operations mnemonics file does not exist. Remember, the mnemonics folder should be in the same place as the code.");
                Environment.Exit(0);
            }

            InstructionsFilePath = mnemonicsPath + "\\Instructions.txt";
            if (!File.Exists(InstructionsFilePath))
            {
                Console.WriteLine("Instructions mnemonics file does not exist");
                Environment.Exit(0);
            }

            ConditionsFilePath = mnemonicsPath + "\\Conditions.txt";
            if (!File.Exists(ConditionsFilePath))
            {
                Console.WriteLine("Conditions mnemonics file does not exist.");
                Environment.Exit(0);
            }

            InternalAddressesFilePath = mnemonicsPath + "\\InternalAddresses.txt";
            if (!File.Exists(InternalAddressesFilePath))
            {
                Console.WriteLine("Internal Addresses mnemonics file does not exist.");
                Environment.Exit(0);
            }

            if (!File.Exists(TargetFilePath))
            {
                Console.WriteLine("Source code file does not exist.");
                Environment.Exit(0);
            }

            Regex period = new Regex(@"\.txt");
            string pathNoFileExtension = period.Split(TargetFilePath)[0];
            DestinationFilePath = pathNoFileExtension + ".machinecode.txt";

            //Run the assembler
            PopulateMnemonicTables();
            ParseFileForSpecialLines();
            TranslateCodeToMachineCode();
            WriteVariables();
            WriteToNewFile();
            
            Console.WriteLine("Done!");
        }

        public static void PopulateMnemonicTables()
        {
            LookupTable[] tables = new LookupTable[]
                {ALUOperationsTable, InstructionsTable, ConditionsTable, InternalAddressesTable};
            string[] filePaths = new string[]
                {ALUOperationsFilePath, InstructionsFilePath, ConditionsFilePath, InternalAddressesFilePath};
            string[] names = new string[]
                {"ALU Operations", "Instructions", "Conditions", "Internal Addresses"};

            for (int index = 0; index < 4; index++)
            {
                string[] fileLines = File.ReadAllLines(filePaths[index]);
                string currentFile = names[index];
                LookupTable currentTable = tables[index];

                foreach (string line in fileLines)
                {
                    string[] lineParts = SplitLine(line);

                    if (lineParts.Length != 0)
                    {
                        if (lineParts.Length != 2)
                        {
                            Console.WriteLine("Bad line in " + currentFile + " file at line: " + line);
                            Environment.Exit(0);
                        }

                        bool isLine = names[index] == "ALU Operations";
                        string mnemonic = lineParts[0];
                        string value = lineParts[1];

                        TableEntry newTableEntry;
                        try
                        {
                            newTableEntry = new TableEntry(mnemonic, value, isLine);
                        }
                        catch (FormatException)
                        {
                            newTableEntry = null;
                            Console.WriteLine(
                                "Value given in " + currentFile + " for the mnemonic: " + mnemonic + "is not binary.");
                            Environment.Exit(0);
                        }
                        catch (OverflowException)
                        {
                            newTableEntry = null;
                            Console.WriteLine(
                                "Value given in " + currentFile + " for the mnemonic: " + mnemonic + "is too large");
                            Environment.Exit(0);
                        }

                        currentTable.AddEntry(newTableEntry);
                    }
                }
            }
        }

        public static void ParseFileForSpecialLines()
        {
            string[] fileLines = File.ReadAllLines(TargetFilePath);
            ProgramCounter = 0;

            foreach (string line in fileLines)
            {
                string[] lineParts = SplitLine(line);

                if (!LineIsIgnorable(lineParts))
                {
                    if (LineIsLabel(lineParts))
                    {
                        string label;
                        if (lineParts[0] == ":") //Line looks like ': LABEL'
                        {
                            if (lineParts.Length != 2)
                            {
                                Console.WriteLine("Bad label at line: '" + line + "'");
                                Environment.Exit(0);
                            }

                            label = lineParts[1];
                        }
                        else //Line looks like ':LABEL'
                        {
                            if (lineParts.Length != 1)
                            {
                                Console.WriteLine("Bad label at line: '" + line + "'");
                                Environment.Exit(0);
                            }

                            label = lineParts[0].Substring(1);
                        }

                        if (LabelTable.HasEntryWithKeyword(label))
                        {
                            Console.WriteLine("Label: " + label + " already exists in the program.");
                            Environment.Exit(0);
                        }

                        TableEntry newLabelEntry = new TableEntry(label, ProgramCounter, true);
                        LabelTable.AddEntry(newLabelEntry);
                    } 
                    else if (LineIsVariable(lineParts))
                    {
                        if (lineParts.Length != 3)
                        {
                            Console.WriteLine("Bad variable declaration at line: '" + line + "'");
                            Environment.Exit(0);
                        }

                        string variable = lineParts[1];
                        string value;

                        if (lineParts[0].ToLower() == "d6") //Line looks like 'D6 VARIABLE 101101'
                        {
                            value = lineParts[2];
                        }
                        else //Line looks like 'D10 VARIABLE 23'
                        {
                            int valueAsInt = Int32.Parse(lineParts[2]);
                            value = IntToBinaryLine(valueAsInt);
                        }

                        TableEntry newVariableEntry = new TableEntry(variable, value, true);
                        VariableTable.AddEntry(newVariableEntry);
                        EndOfMemory--;
                    } 
                    else if (lineParts.Length == 3)
                    {
                        ProgramCounter++;
                        ProgramCounter++;
                    }
                    else
                    {
                        ProgramCounter++;
                    }
                }
            }
        }

        public static void TranslateCodeToMachineCode()
        {
            string[] fileLines = File.ReadAllLines(TargetFilePath);
            ProgramCounter = 0;

            NewFileLines.Add("ADRESS:  DATA");
            NewFileLines.Add("------- ------");

            foreach (string line in fileLines)
            {
                string[] lineParts = SplitLine(line);
                bool isCode = !(LineIsIgnorable(lineParts) || LineIsVariable(lineParts) || LineIsLabel(lineParts));

                if (isCode)
                {
                    string instruction = lineParts[0].ToLower();
                    string instructionValue;

                    try
                    {
                        instructionValue = InstructionsTable.GetValueForKeyword(instruction);
                    }
                    catch (KeyNotFoundException)
                    {
                        instructionValue = null;
                        Console.WriteLine("No known instruction: '" + instruction + "' at line: " + line);
                        Environment.Exit(0);
                    }

                    bool isTwoLine = instructionValue.StartsWith('1');
                    string operand = "";
                    string secondary = "";

                    if (isTwoLine)
                    {
                        if (lineParts.Length != 3)
                        {
                            Console.WriteLine("Wrong number of arguments at line: " + line);
                            Environment.Exit(0);
                        }

                        if (instructionValue == "111") //Is an ALU instruction
                        {
                            operand = GetOperandFromTable(InternalAddressesTable, line);
                            string operation;
                            try
                            {
                                operation = ALUOperationsTable.GetValueForKeyword(lineParts[2].ToLower());
                            }
                            catch (KeyNotFoundException)
                            {
                                operation = null;
                                Console.WriteLine("No known operation: '" + lineParts[2] + "' at line :" + line);
                                Environment.Exit(0);
                            }

                            secondary = operation;
                        } 
                        else if (instructionValue == "110") //Is a jump instruction
                        {
                            operand = GetOperandFromTable(ConditionsTable, line);
                            secondary = GetAddressFromLine(line);
                        }
                        else //Is a memory read/write instruction
                        {
                            operand = GetOperandFromTable(InternalAddressesTable, line);
                            secondary = GetAddressFromLine(line);
                        }
                    }
                    else {
                        if(lineParts.Length != 2)
                        {
                            Console.WriteLine("Wrong number of arguments at line: " + line);
                            Environment.Exit(0);
                        }

                        if(instructionValue == "000") //Is a halt instruction
                            operand = GetOperandFromTable(ConditionsTable, line);
                        else //Is a MOV instruction
                            operand = GetOperandFromTable(InternalAddressesTable, line);
                    }

                    string machineCodeLine1 = IntToBinaryLine(ProgramCounter) + ": " + instructionValue + operand;
                    NewFileLines.Add(machineCodeLine1);
                    ProgramCounter++;

                    if (ProgramCounter > EndOfMemory)
                    {
                        Console.WriteLine(
                            "Code is too large to assemble. This could be due to a large number of variables declared.");
                        Environment.Exit(0);
                    }

                    if (isTwoLine)
                    {
                        string machineCodeLine2 = IntToBinaryLine(ProgramCounter) + ": " + secondary;
                        NewFileLines.Add(machineCodeLine2);
                        ProgramCounter++;
                    }

                    if (ProgramCounter > EndOfMemory)
                    {
                        Console.WriteLine(
                            "Code is too large to assemble. This could be due to a large number of variables declared.");
                        Environment.Exit(0);
                    }
                }
            }
        }

        public static void WriteVariables()
        {
            if (EndOfMemory != 63){
                NewFileLines.Add("--------------");
                NewFileLines.Add("--------------");

                List<TableEntry> variableTable = VariableTable.Table;

                for (int index = (variableTable.Count - 1); index >= 0; index--)
                {
                    string currentEntryValue = variableTable[index].Value;
                    int memoryAddress = 63 - index;
                    string memoryAddressBinary = IntToBinaryLine(memoryAddress);

                    string machineCodeLine = memoryAddressBinary + ": " + currentEntryValue;
                    NewFileLines.Add(machineCodeLine);
                }
            }
        }

        public static void WriteToNewFile()
        {
            File.WriteAllLines(DestinationFilePath, NewFileLines);
        }

        public static string[] SplitLine(string input)
        {
            string[] lineParts = Regex.Split(input, "[ ]+");

            if (lineParts.Length == 1)
            {
                if (lineParts[0] == "")
                    return new string[0];
                return lineParts;
            }

            string[] output;
            int length = lineParts.Length;
            int beginningOffset = 0;
            int endOffset = 0;

            if (lineParts[0] == "")
                beginningOffset = 1;
            if (lineParts[length - 1] == "")
                endOffset = 1;

            if (beginningOffset != 0 || endOffset != 0)
            {
                output = new string[length - (beginningOffset + endOffset)];
                for (int index = 0; index < output.Length; index++)
                {
                    output[index] = lineParts[index + beginningOffset];
                }

                return output;
            }

            return lineParts;
        }

        public static string IntToBinaryLine(int input)
        {
            string binaryString = Convert.ToString(input, 2);

            int zeros = 6 - binaryString.Length;

            if (zeros < 0)
                throw new OverflowException("Integer passed to IntToBinaryLine method too large: " + input);

            while (zeros > 0)
            {
                binaryString = "0" + binaryString;
                zeros--;
            }

            return binaryString;
        }

        public static int BinaryLineToInt(string input)
        {
            int output = Convert.ToInt32(input, 2);
            return output;
        }

        public static bool LineIsIgnorable(string[] lineParts)
        {
            if (lineParts.Length == 0)
                return true;
            if (CommentPattern.IsMatch(lineParts[0]))
                return true;
            return false;
        }

        public static bool LineIsLabel(string[] lineParts)
        {
            if (lineParts[0] == ":")
                return true;
            if (LabelPattern.IsMatch(lineParts[0]))
                return true;
            return false;
        }

        public static bool LineIsVariable(string[] lineParts)
        {
            if (lineParts[0].ToLower() == "d6")
                return true;
            if (lineParts[0].ToLower() == "d10")
                return true;
            return false;
        }

        public static string GetOperandFromTable(LookupTable table, string line)
        {
            string targetLinePart = SplitLine(line)[1].ToLower();
            string operandBinary;

            try
            {
                operandBinary = table.GetValueForKeyword(targetLinePart);
            }
            catch (KeyNotFoundException)
            {
                operandBinary = null;
                Console.WriteLine("No known operand: '" + targetLinePart + "' at line: " + line);
                Environment.Exit(0);
            }

            return operandBinary;
        }

        public static string GetAddressFromLine(string line)
        {
            string targetLinePart = SplitLine(line)[2];
            string output;

            if (Binary.IsMatch(targetLinePart))
                output = targetLinePart;

            else if (LabelTable.HasEntryWithKeyword(targetLinePart))
                output = LabelTable.GetValueForKeyword(targetLinePart);

            else if (VariableTable.HasEntryWithKeyword(targetLinePart))
            {
                int index = VariableTable.GetPositionOfKeyword(targetLinePart);
                int outputInt = 63 - index;
                output = IntToBinaryLine(outputInt);
            }
            else
            {
                output = null;
                Console.WriteLine("No known address: '" + targetLinePart + "' at line: " + line);
                Environment.Exit(0);
            }

            return output;
        }
    }
}