# Megacart Architecture Assembler
An assembler for my Megacart Architecture's instruction set.

This is a project that I have been working on for a while now, and finally I have a computer designed for this architecture. With the computer designed, an assembler was the next logical step. This made it much easier to test the computer, since I didn't have to painstakingly comb through my documentation to translate from mnemonics to machine code. Also, this allows programmers to be more expressive than just simple mnemonics like in other assembly languages. Mnemonics can be anything or any word(s) you want!

## Need-to-Know
The assembler gets the mnemonics from the files in the Mnemonics folder. This folder must be in the same place as the executable. The assembler accepts three different sets of arguments:
* If you pass it 0 arguments: It will ask for you to specify the target file and the mnemonics folder path.
* If you pass it 1: It will assume you are passing it the location of the target file, and will look for the Mnemonics folder where the executable is.
* And for 2: It will assume that you gave it the target file and the Mnemonics folder location, in that order.

Any other number of arguments will result in an error. It also spits out a variety of errors if the code you type is incorrect. This was a big focus of the program, because the user should be easily able to find the problem if one exists.

If you have a certain way that you want to type something out, that makes better sense in that context than some of the other mnemonic combinations do, you can add some new mnemonics to the files. You can look for the mnemonic that means the same thing and add your new one next to it with the same binary after it. For example, if I wanted to use register 2 for a counter in my program, I would add this to the InternalAddresses.txt file:
```
...

rg2  110
reg2 110
counter 110

...
```

Though, I'd recommend keeping the binary at a consistant distance from the mnemonics. It doesn't really matter, but it looks nicer.

#### Finally, some stuff you can do:
```
//Comments are written like this
// Or like this
:LabelsWorkLikeThis
: OrLikeThis
```

Jumps to labels work as you'd expect (but remember that jump is always conditional, so at least put a period down)

```JMP . LABEL```

Variables work in these two ways:
```
D6 BinaryVariable 011000
D10 DecimalVariable 23
```

And work the same way that you're used to. You can read them and write to them through the memory read/write instructions.
```
MRD Reg0 BinaryVariable
MWT Acc DecimalVariable
```

I think you can even jump to them, but I don't know why you'd want to do that.

\*You can now get the actual value of labels in memory, by doing this:
```
DL MyLabelValue MYLABEL
```
This was added in as a result of some new architecture changes. Sometimes I found myself wanting the raw value of a label for certain things, so I added this in. It works a bit odd though. You must put it after the label in question. So, as a rule of thumb, always put these special declarations at the end of your code.

Additionally, if, for some reason, you'd like to program in binary, *on an assembler*, multiple D6 variable declarations work.

The computer lacks a call instruction and a multiplication instruction. While you can achieve these two things through the existing instruction set, it is far too large for my liking. See the plans section at the bottom for how I plan to fix this, and my thoughts on the computer.

Function call code:
```
D10 six 6
WTB PC
MRD A six
ALU [LOCATION] A+B
JMP TRUE [FUNCTIONLABEL]
```

Return from a function call:
```
WTB [LOCATION]
RDB PC
```

## Functionality:
#### We do two passes on the file.

The first pass is to check for labels.
There are two types of labels.
The first is a variable label. This contains a value set at the beginning of the code (or anywhere in the code like D6 VARIABLE VALUE).
The second is a jump label. These are the labels you're used to seeing in assembly code (except the colon is at the beginning like :LABEL).
When it comes across one of these, it stores it along with the programCounter in the label tabel.

#### Variables work a bit differently though.
Variables will be treated as labels to jump to.
Unlike with every other entry, variables need three pieces of information stored with them: Their name, their value, and then their position in memory.
I would rather not create a special object that will exclusively be used by variables, just to store one extra piece of data.
Instead it would be easier to use their position in the table as their position in memory, because this is exactly how it will work.
The first variable declared will be at the beginning of the table, and the end of the memory. Index = 0; MemoryPosition = 63-Index;
The next will be at Index = 1, and so on.
This might seem like a bad thing, but this is exactly how I want their memory position to be interpreted by people reading the code.

#### The second pass is when the translating from assembly to machine code happens.
This is just matching mnemonics/labels up with their values.
If it sees something unexpected, it spits out some error.
If it sees an empty line, or a line starting with : or D6/D10, it ignores it.
It also ignores spaces in lines.
Additionally, I've added the ability to make comments, so it also ignores any line like "//Comment" or "// Here's another comment".

#### Types of mnemonics:
* Instructions
* ALU Operations
* Labels
* Internal Addresses
* Conditions
