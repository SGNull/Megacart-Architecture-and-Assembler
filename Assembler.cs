using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Megacart_Assembler;

public class Assembler
{
    public static int ProgramCounter = 0;

    public static string FileName = "TestFile.txt";
    public static string PathFromUser = "\\source\\repos\\Megacart_Architecture_Assembler\\";
    public static string PathToUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    public static string FileLocation = PathToUser + PathFromUser + FileName;

    public static LookupTable LabelTable;
    public static LookupTable ALUOperationsTable;
    public static LookupTable InstructionsTable;
    public static LookupTable ConditionsTable;
    public static LookupTable InternalAddressesTable;

    public static void Main(String[] args)
    {
        if (File.Exists(FileLocation))
        {
            PopulateTables();
            ParseFileForLabels();
            ParseFile();
        }
	}

    public static void PopulateTables()
    {

    }

    public static void ParseFileForLabels()
    {
        string[] fileLines = File.ReadAllLines(FileLocation);
        ProgramCounter = 0;

        foreach (string line in fileLines)
        {
            string[] lineParts = Regex.Split(line, "[ ]+");

            if (lineParts[0] != "") //Line is not empty
            {
                if (lineParts.Length > 3 && lineParts[3] != "")
                    throw new FormatException("Line " + ProgramCounter + " too long: " + line);

                TableEntry newTableEntry;

                if (lineParts[0].ToLower() == "d6")
                {
                    newTableEntry = new TableEntry(lineParts[1], lineParts[2], true);
                }
                else if (lineParts[0].ToLower() == "d10")
                {
                    int intValue = Int32.Parse(lineParts[2]);
                    string binValue = Convert.ToString(intValue, 2);
                    newTableEntry = new TableEntry(lineParts[1], binValue, true);
                }
                else if (lineParts[0].ToLower() == ":")
                {

                }

                ProgramCounter++;
            }
        }
    }

    public static void ParseFile()
    {
        ProgramCounter = 0;

    }

    public static string ParseLine1()
    {
        return null;
    }
}
