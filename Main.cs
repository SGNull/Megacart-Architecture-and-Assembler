using System;
using System.IO;



public class Main
{
    public int programCounter = 0;
    public string fileLocation = "TestFile.txt";

    public Main()
    { 
        if (File.Exists(fileLocation))
        {
            text = File.ReadAllLines(fileLocation);
            foreach (string line in text)
            {
                Console.WriteLine(line);
            }
            //ParseStringForLabels();
            //ParseString();
        }
	}

    public ParseStringForLabels()
    {

    }

    public ParseString()
    {

    }

    public ParseLine1()
    {
        
    }

    public ParseInstruction()
    {

    }

    public ParseALUOpcode()
    {

    }

    public ParseJumpLocation()
    {
        //This is important: The assembler should support labels for jumping.
    }

    public ParseRead()
    {

    }

    public ParseWrite()
    {

    }

    public ParseCondition()
    {

    }

    public ParseMemoryAddress()
    {

    }
}
