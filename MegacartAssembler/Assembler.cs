using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using MegacartAssembler;

namespace MegacartAssembler
{
    class Assembler
    {
        public static int ProgramCounter;
        public static int EndOfMemory = 63;

        public static string TargetFileName = "TestFile.txt";
        public static string PathFromUser = "\\source\\repos\\MegacartAssembler\\MegacartAssembler";
        public static string PathToUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static string TargetFilePath = PathToUser + PathFromUser + TargetFileName;

        public static string ALUOperationsFilePath = PathToUser + PathFromUser + "\\Mnemonics\\ALUOperations.txt";
        public static string InstructionsFilePath = PathToUser + PathFromUser + "\\Mnemonics\\Instructions.txt";
        public static string ConditionsFilePath = PathToUser + PathFromUser + "\\Mnemonics\\Conditions.txt";
        public static string InternalAddressesFilePath = PathToUser + PathFromUser + "\\Mnemonics\\InternalAddresses.txt";

        public static LookupTable LabelTable;
        public static LookupTable ALUOperationsTable;
        public static LookupTable InstructionsTable;
        public static LookupTable ConditionsTable;
        public static LookupTable InternalAddressesTable;

        public static void Main(String[] args)
        {
            if (File.Exists(TargetFilePath))
            {
                PopulateTables();
                ParseFileForLabels();
                ParseFile();
                ApplyVariables(); //Variables get added to the end of the program as :NAME \n VALUE
            }
        }

        public static void PopulateTables()
        {
            LookupTable[] tables = new LookupTable[]
                {ALUOperationsTable, InstructionsTable, ConditionsTable, InternalAddressesTable};
            string[] filePaths = new string[]
                {ALUOperationsFilePath, InstructionsFilePath, ConditionsFilePath, InternalAddressesFilePath};

            foreach (string filePath in filePaths)
            {

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
    }
}