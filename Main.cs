using System;
using System.IO;

//ASSEMBLER PLAN:
//We do two passes on the file.
//The first pass is to check for labels.
//  There are two types of labels.
//  The first is a variable label. This contains a value set at the beginning of the code (or anywhere in the code like D6 VARIABLE VALUE)
//  The second is a jump label. These are the labels you're used to seeing in assembly code (except the colon is at the beginning like :LABEL).
//      When it comes across one of these, it stores it along with the programCounter - 1 in the label tabel.
//The second pass is when the parsing happens. This is just matching mneumonics/labels up with their values
//  If it sees something unexpected, it spits out some error
//  If it sees an empty line, or a line starting with : or D6, it ignores it.
//  It also ignores spaces in lines

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
        //Parts of strings can work two ways.
        //First, if it contains binary, then we can skip parsing.
        //Second, if it contains a mneumonic, then we check the correct table for that mneumonic.
        //However, if the instruction is written in binary, then we must locate that instruction's properties.
        //As a result, I think it should pull the instruction's value from the table, and then reference another something for the properties.
        //By properties, I mean that which the program should expect to see after the instruction.

        //If we see a blank line, we discard it and do nothing.
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
