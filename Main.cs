using System;
using System.IO;

public class Main
{
    public int programCounter = 0;
    public string fileLocation = "TestFile.txt";

    public Main(String[] args)
    { 
        if (File.Exists(fileLocation))
        {
            string[] text = File.ReadAllLines(fileLocation);
            foreach (string line in text)
            {
                Console.WriteLine(line);
            }
            //ParseStringForLabels();
            //ParseString();
        }
	}

    public void ParseStringForLabels()
    {

    }

    public void ParseString()
    {

    }

    public string ParseLine1()
    {
        return null;
    }
}
