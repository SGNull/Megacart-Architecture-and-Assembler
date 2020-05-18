using System;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

public class Assembler
{
    public static int ProgramCounter = 0;

    public static string FileName = "TestFile.txt";
    public static string PathFromUser = "\\source\\repos\\Megacart_Architecture_Assembler\\";
    public static string PathToUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    public static string FileLocation = PathToUser + PathFromUser + FileName;

    public static void Main(String[] args)
    {
        if (File.Exists(FileLocation))
        {
            ParseStringForLabels();
            ParseString();
        }
	}

    public static void ParseStringForLabels()
    {
        string[] fileLines = File.ReadAllLines(FileLocation);

        foreach (string line in fileLines)
        {
            string[] lineParts = Regex.Split(line, "[ ]+");
            foreach (string mnemonic in lineParts)
            {
                if(mnemonic != "")
                    Debug.Print(mnemonic);
            }
        }
    }

    public static void ParseString()
    {

    }

    public static string ParseLine1()
    {
        return null;
    }
}
