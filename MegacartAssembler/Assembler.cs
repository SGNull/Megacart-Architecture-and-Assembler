using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MegacartAssembler
{
    class Assembler
    {
        public static int ProgramCounter; //Used during the passes
        public static int EndOfHardDriveMemory = 63; //Useful for new architectural changes! HDD size is not necessarily the same size as RAM
        public static int CodeEndsHere; //Is important for generating variable pointers.
        public static int NumberOfMachineCodeLines; //Used when writing to file

        public static string TargetFilePath; //This is the file to parse
        public static string DestinationFilePath; // This is the new .machinecode.txt file's path

        public static string MnemonicsPath;
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

        public static Regex LabelPattern = new Regex("^:[\\S]+$");
        public static Regex CommentPattern = new Regex("^//[\\S]*$");
        public static Regex Binary = new Regex("^[0||1]+$");

        public static List<string> NewFileLines = new List<string>(); //Where the file's lines are stored before writing them to the file.

        public static void Main(String[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(
                    "Assuming Assembler.exe is in the same location as the Mnemonics and Programs folder, is this correct? yes/no?");
                
                while (true){
                    string userInput = Console.ReadLine().ToLower();

                    if (userInput == "yes" || userInput == "y")
                    {
                        //Code from https://stackoverflow.com/questions/837488/how-can-i-get-the-applications-path-in-a-net-console-application
                        string directory = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                        string localDirectory = Path.GetDirectoryName(new Uri(directory).LocalPath);
                        
                        MnemonicsPath = localDirectory + "\\Mnemonics";

                        Console.WriteLine("Enter the name of your program.");
                        string programName = Console.ReadLine();
                        
                        TargetFilePath = localDirectory + "\\Programs\\" + programName + ".txt";
                        
                        break;
                    }
                    else if (userInput == "no" || userInput == "n")
                    {
                        //Prompt the user for arguments
                        Console.WriteLine("Please input the mnemonics folder file path");
                        MnemonicsPath = Console.ReadLine();

                        Console.WriteLine("Please input the source code file path (the file to be assembled):");
                        TargetFilePath = Console.ReadLine();

                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input, type 'yes' or 'no'");
                    }
                }
            }
            else if (args.Length == 1)
            {
                //Target file only
                //Code from https://stackoverflow.com/questions/837488/how-can-i-get-the-applications-path-in-a-net-console-application
                string directory = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                string localDirectory = Path.GetDirectoryName(new Uri(directory).LocalPath);

                TargetFilePath = Path.GetFullPath(args[0]);
                MnemonicsPath = localDirectory + "\\Mnemonics";
            }
            else if (args.Length == 2)
            {
                //Target and Mnemonics
                TargetFilePath = args[0];
                MnemonicsPath = args[1];
            }
            else
            {
                MnemonicsPath = null; //I do this throughout the code, because ReSharper doesn't seem to understand that Environment.Exit(0) stops the program.
                StopWithErrorMessage("Bad number of arguments passed to the assembler.");
            }

            CheckIfFilesExist();

            Regex period = new Regex(@"\.txt");
            string pathNoFileExtension = period.Split(TargetFilePath)[0];
            DestinationFilePath = pathNoFileExtension + ".machinecode.txt";

            //Run the assembler
            PopulateMnemonicTables();
            ParseFileForSpecialLines();
            DoAssembling();
            WriteVariables();
            File.WriteAllLines(DestinationFilePath, NewFileLines);
            
            Console.WriteLine("Done!");
        }

        public static void CheckIfFilesExist()
        {
            if (!Directory.Exists(MnemonicsPath))
            {
                StopWithErrorMessage(
                    "The mnemonics folder does not exist. It should be where the assembler is.");
            }
            
            ALUOperationsFilePath = MnemonicsPath + "\\ALUOperations.txt";
            if (!File.Exists(ALUOperationsFilePath))
            {
                StopWithErrorMessage(
                    "ALU Operations mnemonics file does not exist.");
            }

            InstructionsFilePath = MnemonicsPath + "\\Instructions.txt";
            if (!File.Exists(InstructionsFilePath))
            {
                StopWithErrorMessage("Instructions mnemonics file does not exist");
            }

            ConditionsFilePath = MnemonicsPath + "\\Conditions.txt";
            if (!File.Exists(ConditionsFilePath))
            {
                StopWithErrorMessage("Conditions mnemonics file does not exist.");
            }

            InternalAddressesFilePath = MnemonicsPath + "\\InternalAddresses.txt";
            if (!File.Exists(InternalAddressesFilePath))
            {
                StopWithErrorMessage("Internal Addresses mnemonics file does not exist.");
            }

            if (!File.Exists(TargetFilePath))
            {
                StopWithErrorMessage("Source code file does not exist.");
            }
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
                            StopWithErrorMessage("Bad line in " + currentFile + " file at line: '" + line + "'");
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
                            StopWithErrorMessage("Value given in " + currentFile + " for the mnemonic: '" + mnemonic + "' is not binary.");
                        }
                        catch (OverflowException)
                        {
                            newTableEntry = null;
                            StopWithErrorMessage("Value given in " + currentFile + " for the mnemonic: '" + mnemonic + "' is too large");
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
                                StopWithErrorMessage("Bad label at line: '" + line + "'");
                            }

                            label = lineParts[1];
                        }
                        else //Line looks like ':LABEL'
                        {
                            if (lineParts.Length != 1)
                            {
                                StopWithErrorMessage("Bad label at line: '" + line + "'");
                            }

                            label = lineParts[0].Substring(1);
                        }

                        if (LabelTable.HasEntryWithKeyword(label))
                        {
                            StopWithErrorMessage("Label: " + label + " already exists in the program.");
                        }

                        TableEntry newLabelEntry = new TableEntry(label, ProgramCounter, true);
                        LabelTable.AddEntry(newLabelEntry);
                    } 
                    else if (LineIsVariable(lineParts))
                    {
                        if (lineParts.Length != 3)
                        {
                            StopWithErrorMessage("Bad variable declaration at line: '" + line + "'");
                        }

                        string variable = lineParts[1];
                        string value;

                        if (lineParts[0].ToLower() == "d6") //Line looks like 'D6 VARIABLE 101101'
                        {
                            value = lineParts[2];
                        }
                        else if (lineParts[0].ToLower() == "d10") //Line looks like 'D10 VARIABLE 23'
                        {
                            int valueAsInt = Int32.Parse(lineParts[2]);
                            try
                            {
                                value = IntToBinaryLine(valueAsInt);
                            }
                            catch (OverflowException)
                            {
                                value = null;
                                StopWithErrorMessage("Variable '" + variable + "' is too large at line: '" + line + "'");
                            }
                        } 
                        else {
                            if(LabelTable.HasEntryWithKeyword(lineParts[2])){
                                value = null;
                                StopWithErrorMessage("The label '" + lineParts[2] + "' is not before this in the code. Make sure this variable declaration is at the end!");
                                //Note: a solution to this is parsing for labels, then parsing for variables. However, this would add an additional pass to the assembling process.
                                //      I have decided against this, but I am open to changing my mind in the future if this proves to be too difficult to work with.
                            }
                            value = LabelTable.GetValueForKeyword(lineParts[2]);
                        }

                        TableEntry newVariableEntry = new TableEntry(variable, value, true);
                        VariableTable.AddEntry(newVariableEntry);
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

            CodeEndsHere = ProgramCounter;
        }

        public static void DoAssembling()
        {
            string[] fileLines = File.ReadAllLines(TargetFilePath);

            NewFileLines.Add("#   DATA");
            NewFileLines.Add("-- ------");

            //This I had to do, because I need to locate the first code line of the program, not the first actual line.
            bool isFirstLine = true;

            foreach (string line in fileLines)
            {
                string[] lineParts = SplitLine(line);
                bool isCode = !(LineIsIgnorable(lineParts) || LineIsVariable(lineParts) || LineIsLabel(lineParts));

                if (isCode && isFirstLine)
                {
                    isFirstLine = false;
                    bool isCorrect = "RAMSlot" == lineParts[0] || "RAM" == lineParts[0];
                    if (!isCorrect)
                        StopWithErrorMessage("RAM Slot not defined at beginning of code.");
                    
                    if(lineParts.Length == 2)
                    {

                    } 
                    else if (lineParts.Length == 3)
                    {

                    }
                    else
                    {
                        StopWithErrorMessage("Bad RAM slot declaration. Follow this format: 'RAM = [RAM slot # 0/3]'");
                    }
                }
                else if (isCode)
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
                        StopWithErrorMessage("No known instruction: '" + instruction + "' at line: '" + line + "'");
                    }

                    bool isTwoLine = instructionValue.StartsWith('1');
                    string operand = "";
                    string secondary = "";

                    if (isTwoLine)
                    {
                        if (lineParts.Length != 3)
                        {
                            StopWithErrorMessage("Wrong number of arguments at line: '" + line + "'");
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
                                StopWithErrorMessage("No known operation: '" + lineParts[2] + "' at line: '" + line + "'");
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
                    else 
                    {
                        if(lineParts.Length != 2)
                        {
                            StopWithErrorMessage("Wrong number of arguments at line: '" + line + "'");
                        }

                        if(instructionValue == "000") //Is a halt instruction
                            operand = GetOperandFromTable(ConditionsTable, line);
                        else //Is a MOV instruction
                            operand = GetOperandFromTable(InternalAddressesTable, line);
                    }

                    string machineCodeLine1 = instructionValue + operand;
                    AddMachineCodeLineToFile(machineCodeLine1);

                    if (isTwoLine)
                    {
                        AddMachineCodeLineToFile(secondary);
                    }
                }
            }
        }

        public static void WriteVariables()
        {
            List<TableEntry> variableTable = VariableTable.Table;

            for (int index = 0; index < variableTable.Count; index++)
            {
                string currentEntryValue = variableTable[index].Value;
                AddMachineCodeLineToFile(currentEntryValue);
            }
            
        }

        public static string[] SplitLine(string input)
        {
            string[] lineParts = Regex.Split(input, "[ ||\t]+");

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
            string firstPart = lineParts[0].ToLower();
            if (firstPart == "d6" || firstPart == "d10" || firstPart == "dl")
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
                StopWithErrorMessage("No known operand: '" + targetLinePart + "' at line: " + line);
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
                //What it's doing here is generating the pointer for the variable. Since it occurs in the memory at index + CodeEndsHere, it uses that as the pointer.
                int index = VariableTable.GetPositionOfKeyword(targetLinePart);
                int outputInt = CodeEndsHere + index;
                output = IntToBinaryLine(outputInt);
            }
            else
            {
                output = null;
                StopWithErrorMessage("No known address: '" + targetLinePart + "' at line: " + line);
            }

            return output;
        }

        public static void StopWithErrorMessage(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }
    
        public static void AddMachineCodeLineToFile(string line)
        {
            string space;
            if(NumberOfMachineCodeLines < 10)
                space = "  ";
            else
                space = " ";

            string newLine = NumberOfMachineCodeLines + space + line;

            NewFileLines.Add(newLine);
            NumberOfMachineCodeLines++;

            if (NumberOfMachineCodeLines > EndOfHardDriveMemory)
            {
                StopWithErrorMessage("Code is too large to assemble. This could be due to a large number of variables declared.");
            }
        }
    }
}