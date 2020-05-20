using System;
using System.IO;
using System.Text.RegularExpressions;
using MegacartAssembler;

namespace MegacartAssembler
{
    class Assembler
    {
        public static int ProgramCounter;
        public static int EndOfMemory = 63;

        public static string TargetFileName = "TestFile.txt";
        public static string PathFromUser = "\\source\\repos\\MegacartAssembler\\";
        public static string PathToUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static string TargetFilePath = PathToUser + PathFromUser + TargetFileName;

        public static string ALUOperationsFilePath = PathToUser + PathFromUser + "\\Mnemonics\\ALUOperations.txt";
        public static string InstructionsFilePath = PathToUser + PathFromUser + "\\Mnemonics\\Instructions.txt";
        public static string ConditionsFilePath = PathToUser + PathFromUser + "\\Mnemonics\\Conditions.txt";
        public static string InternalAddressesFilePath = PathToUser + PathFromUser + "\\Mnemonics\\InternalAddresses.txt";

        public static LookupTable LabelTable;
        public static LookupTable ALUOperationsTable = new LookupTable("ALU Operations");
        public static LookupTable InstructionsTable = new LookupTable("Instructions");
        public static LookupTable ConditionsTable = new LookupTable("Conditions");
        public static LookupTable InternalAddressesTable = new LookupTable("Internal Addresses");

        public static void Main(String[] args)
        {
            if (File.Exists(TargetFilePath))
            {
                PopulateMnemonicTables();
                ParseFileForLabels();
                ParseFile();
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
                    string[] lineParts = Regex.Split(line, "[ ]+");
                    string[] fixedLineParts = RemoveEmptyStrings(lineParts);

                    if (fixedLineParts.Length != 0)
                    {
                        if(fixedLineParts.Length != 2)
                            throw new FormatException("Bad line in " + currentFile + " file at line: " + line);

                        bool isLine = names[index] == "ALU Operations";
                        string mnemonic = fixedLineParts[0];
                        string value = fixedLineParts[1];

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

        public static void ParseFileForLabels()
        {
            string[] fileLines = File.ReadAllLines(TargetFilePath);
            ProgramCounter = 0;

            foreach (string line in fileLines)
            {
                string[] lineParts = Regex.Split(line, "[ ]+");
                lineParts = RemoveEmptyStrings(lineParts);

                if (lineParts.Length != 0)
                {

                }
            }
        }

        public static void ParseFile()
        {
            ProgramCounter = 0;

        }

        public static void ApplyVariables()
        {

        }

        public static string[] RemoveEmptyStrings(string[] input)
        {
            if (input.Length == 1)
            {
                if (input[0] == "")
                    return new string[0];
                return input;
            }

            string[] output;
            int length = input.Length;
            int beginningOffset = 0;
            int endOffset = 0;

            if (input[0] == "")
                beginningOffset = 1;
            if (input[length - 1] == "")
                endOffset = 1;

            if (beginningOffset != 0 || endOffset != 0)
            {
                output = new string[length - (beginningOffset + endOffset)];
                for (int index = 0; index < output.Length; index++)
                {
                    output[index] = input[index + beginningOffset];
                }

                return output;
            }

            return input;
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
            }

            return binaryString;
        }
    }
}