# Megacart_Architecture_Assembler
An assembler for my Megacart Architecture's instruction set.

This is a project that I have been working on for a while now, and finally I have a computer designed for this architecture. The process went something like this:
Initial Documentation > Hardware Development > Refine Documentation > Hardware Integration > Further Refine Documentation > Phase 1 Testing > Phase 2 Testing > Phase 3 Testing > Finalization

Currently I am at phase 3 testing. The way I've defined the testing phases is as such:
Phase 1: Try executing a single instruction, and put duct-tape everywhere when everything inevitably breaks
Phase 2: Try executing multiple instructions to perform some kind of computation. Hopefully less things are breaking here.
Phase 3: Actually try running a program on the computer.

Right now I have to do all the assembling by myself. This means painstakingly going through the documentation, finding the codes for mneumonics, and replacing them with binary, not to mention all the labels that are being jumped to. I can't do this right away, of course, because I have to have both the source code and the machine code to know what is happening. So, this means having two copies of the program, each individually typed out, along with the spots in memory that they are at. It's just an overall mess.
So, I've decided to try my hand at building an assembler for it. Recently I just got done with a software engineering class, and a class about languages and automata, so hopefully this turns out OK.

### Functionality Plans:
#####We do two passes on the file.

The first pass is to check for labels.
There are two types of labels.
The first is a variable label. This contains a value set at the beginning of the code (or anywhere in the code like D6 VARIABLE VALUE)
The second is a jump label. These are the labels you're used to seeing in assembly code (except the colon is at the beginning like :LABEL).
When it comes across one of these, it stores it along with the programCounter - 1 in the label tabel.

The second pass is when the parsing happens. This is just matching mneumonics/labels up with their values
If it sees something unexpected, it spits out some error
If it sees an empty line, or a line starting with : or D6, it ignores it.
It also ignores spaces in lines

#####Parts of strings can work two ways.

First, if it contains binary, then we can skip parsing.
Second, if it contains a mneumonic, then we check the correct table for that mneumonic.
However, if the instruction is written in binary, then we must locate that instruction's properties.
By properties, I mean that which the program should expect to see after the instruction.
As a result, I think it should pull the instruction's value from the table, and then have a seperate thing for the properties.

If we see a blank line, we discard it and do nothing.