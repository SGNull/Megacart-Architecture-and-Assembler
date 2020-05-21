using System;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using MegacartAssembler;
using Microsoft.VisualBasic.CompilerServices;

namespace MegacartAssembler
{
    class Assembler
    {
        public static int ProgramCounter;
        public static int EndOfMemory = 63;

        public static string TargetFileName = "TestFile";
        public static string PathFromUser = "\\source\\repos\\MegacartAssembler\\";
        public static string PathToUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static string TargetFilePath = PathToUser + PathFromUser + TargetFileName + ".txt";
        public static string DestinationFilePath = PathToUser + PathFromUser + TargetFileName + ".machinecode.txt"

        public static string ALUOperationsFilePath = PathToUser + PathFromUser + "\\Mnemonics\\ALUOperations.txt";
        public static string InstructionsFilePath = PathToUser + PathFromUser + "\\Mnemonics\\Instructions.txt";
        public static string ConditionsFilePath = PathToUser + PathFromUser + "\\Mnemonics\\Conditions.txt";
        public static string InternalAddressesFilePath = PathToUser + PathFromUser + "\\Mnemonics\\InternalAddresses.txt";

        public static LookupTable LabelTable = new LookupTable("Labels");
        public static LookupTable VariableTable = new LookupTable("Variables");
        public static LookupTable ALUOperationsTable = new LookupTable("ALU Operations");
        public static LookupTable InstructionsTable = new LookupTable("Instructions");
        public static LookupTable ConditionsTable = new LookupTable("Conditions");
        public static LookupTable InternalAddressesTable = new LookupTable("Internal Addresses");

        public static Regex LabelPattern = new Regex(":[\\w]+");
        public static Regex CommentPattern = new Regex("//[\\w]*");

        public static void Main(String[] args)
        {
            if (File.Exists(TargetFilePath))
            {
                PopulateMnemonicTables();
                ParseFileForSpecialLines();
                ParseFile(); //Variables are substituted with '63 - VariableTable.GetIndexOfKeyword(variable);'
                ApplyVariables();
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
                        if(lineParts.Length != 2)
                            throw new FormatException("Bad line in " + currentFile + " file at line: " + line);

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
                            throw new FormatException(
                                "Value given in " + currentFile + " for the mnemonic: " + mnemonic + "is not binary.");
                        }
                        catch (OverflowException)
                        {
                            throw new OverflowException(
                                "Value given in " + currentFile + " for the mnemonic: " + mnemonic + "is too large");
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
                                throw new FormatException("Bad label at line: '" + line + "'");

                            label = lineParts[1].ToLower();
                        }
                        else //Line looks like ':LABEL'
                        {
                            if (lineParts.Length != 1)
                                throw new FormatException("Bad label at line: '" + line + "'");

                            label = lineParts[0].Substring(1).ToLower();
                        }

                        if (LabelTable.HasEntryWithKeyword(label))
                            throw new DuplicateNameException("Label: " + label + " already exists in the program.");

                        TableEntry newLabelEntry = new TableEntry(label, ProgramCounter, true);
                        LabelTable.AddEntry(newLabelEntry);
                    }

                    if (LineIsVariable(lineParts))
                    {
                        if (lineParts.Length != 3)
                            throw new FormatException("Bad variable declaration at line: '" + line + "'");

                        string variable = lineParts[1];
                        string value;

                        if (lineParts[0] == "d6") //Line looks like 'D6 VARIABLE 101101'
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
                    ProgramCounter++;
                }
            }
        }

        public static void ParseFile()
        {
            string[] fileLines = File.ReadAllLines(TargetFilePath);
            ProgramCounter = 0;

            foreach (string line in fileLines)
            {
                
            }
        }

        public static void ApplyVariables()
        {

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
            if (lineParts[0] == "d6")
                return true;
            if (lineParts[0] == "d10")
                return true;
            return false;
        }
    }
}