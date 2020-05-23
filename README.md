# Megacart_Architecture_Assembler
An assembler for my Megacart Architecture's instruction set.

This is a project that I have been working on for a while now, and finally I have a computer designed for this architecture. The process went something like this, for those interested:

Initial Documentation > Hardware Development > Refine Documentation > Hardware Integration > Further Refine Documentation > Phase 1 Testing > Phase 2 Testing > Phase 3 Testing > Finalization

Currently I am at the phase 3 testing stage. Here are the testing phases:
* Phase 1: Try executing a single instruction.
* Phase 2: Try performing some kind of computation.
* Phase 3: Actually try running a program on the computer.

Like I said, I'm at stage 3 testing. Right now I have to do all the assembling by myself. This means painstakingly going through the documentation, finding the codes for mnemonics, and replacing them with binary, not to mention all the labels that are being jumped to. I can't do this right away, of course, because I have to have both the source code and the machine code to know what is happening. So, this means having two copies of the program, each individually typed out, along with the spots in memory that they are at. It's just an overall mess.
So, I've decided to try my hand at building an assembler for it. Recently I just got done with a software engineering class, and a class about languages and automata, so hopefully this turns out OK.

## Functionality Plans:
#### We do two passes on the file.

The first pass is to check for labels.
There are two types of labels.
The first is a variable label. This contains a value set at the beginning of the code (or anywhere in the code like D6 VARIABLE VALUE).
The second is a jump label. These are the labels you're used to seeing in assembly code (except the colon is at the beginning like :LABEL).
When it comes across one of these, it stores it along with the programCounter in the label tabel.

The second pass is when the parsing happens. This is just matching mnemonics/labels up with their values.
If it sees something unexpected, it spits out some error.
If it sees an empty line, or a line starting with : or D6, it ignores it.
It also ignores spaces in lines.

#### Variables work a bit differently though.
Variables will be treated as labels to jump to.
Now, the following solution (I think) has a certain bad-coding-practices smell to it, but bear with me, as this is the simplest solution I can think of.
Unlike with every other entry, variables need three pieces of information stored with them: Their name, their value, and then their position in memory.
I would rather not create a special object that will exclusively be used by variables, just to store one extra piece of data.
Instead it would be easier to use their position in the table as their position in memory, because this is exactly how it will work.
The first variable declared will be at the beginning of the table, and the end of the memory. Index = 0; MemoryPosition = 63-Index;
The next will be at Index = 1, and so on.
This might seem like a bad thing, but this is exactly how I want their memory position to be interpreted by people reading the code.

#### The final parse through the file will write to a .machinecode.txt file
For this final parse, I have decided against interpreting the source code at multiple levels (of abstraction).
My thought process behind this is: If you're using an assembler, why would you want to type at a lower level?
The only instance where I could see this being useful is with variables, but that does not involve the instructions themselves, and it already allows for this.
I'm not opposed to doing this because it's a hard problem to solve, I just feel like it would be a waste of my time.
Also, because I made the program to support nearly every possible mnemonic, you could just add an instruction's value to the mnemonic files as a mnemonic.

## Types of mnemonics:
* Instructions
* ALU Operations
* Labels
* Internal Addresses
* Conditions

## Architecture/Computer Details:
I created the components from scratch. Some of the designs I borrowed or were influenced by others, and they'll be credited when the build is done.
* 6-bit CPU, data bus, and registers
* 6-bit memory addressing
* Seperate memory reading/writing (but not dual read)
* 48-byte RAM
* 8 instructions.
* 4 two-line instructions, which take 3 clock cycles.
* 4 one-line instructions, which take 2 clock cycles.
* The accumulator, or the A register, was originally not meant to be directly written to. However, some of my recent modifications allow this to happen.
* The temp register, or the B register, can be directly written to.
* Unknown cycle speed. This is part of finalization.
* Instructions: Conditional halt, conditional jump, read/write from memory, read/write from temp/B, read from acc/A, ALU operations.

#### Instruction Format:

IsTwoLine?[1-bit] Instruction[2-bit] Operand[3-bit] | OptionalSecondLine[6-bit]

#### ALU operations are performed a bit wierdly.
First you have to select the ALU instruction (111), then tell it where you want the result to go to in the next 3 bits. Then, the second line contains the opcode, but it's not a normal opcode. 
The bottom 4-bits of this opcode is the actual code that gets fed into the ALU Decoder to generate the control signals for the ALU. The top two bits control whether A/B is inverted respectively. 
I chose to do this, because it provided the maximum amount of instructions to the user, without requiring a giant 6-bit decoder.

#### The components:

CPU - Contains A and B registers, and the ALU.

Secondary Controller - Takes the operand and converts it into whatever signals the instruction needs.

ALU Decoder - Takes the code part of the opcode and converts it into control signals for the ALU.

Fetch Registers - Stores the instructions to be executed.

Primary Controller - The main logic of the computer. Contains the state counter, and generates all of the high-level control signals. Is responsible for fetching and executing.

IO - The input/output port of the computer. Has two sets of read/write signals, one coming from the computer, the other going to the computer.

Program Counter - Self explanitory, points to the current instruction's location in memory.

#### I'm planning on adding some sort of hard-drive port.
This would allow the user to easily punch code into the HDD, then attach the drive to the computer in some way that it would automatically load the data into RAM, then start the program.
I really want this, so that the most amount of people would be able to use the computer without having to have too much knowledge about how to actually load data into RAM.
The way that it currently is done is by directly changing the bits on the data bus, and the write address, then sending the write signal to RAM.
This of course requires knowledge about the inner-workings of the computer, which I don't like.