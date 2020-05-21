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
When it comes across one of these, it stores it along with the programCounter - 1 in the label tabel.

The second pass is when the parsing happens. This is just matching mnemonics/labels up with their values.
If it sees something unexpected, it spits out some error.
If it sees an empty line, or a line starting with : or D6, it ignores it.
It also ignores spaces in lines.

#### Variables work a bit differently though.
Variables should be treated as labels to jump to. What will happen is:
The first variable is stored in the label table as NAME 111111
The next is stored as NAME 111110
And so on, such that they all are stored at the end of memory. This requires no knowledge of how large the program will turn out to be.
You could also have a global counter variable to store how many spots at the end of memory are variables.
This would allow you to tell the user that their program is too long.

#### Parts of strings can work two ways.

First, if it contains binary, then we can skip parsing.
Second, if it contains a mnemonic, then we check the correct table for that mnemonic.
However, if the instruction is written in binary, then we must locate that instruction's format.
By format, I mean that which the program should expect to see after the instruction.
As a result, I think it should pull the instruction's value from the table, and then have a seperate thing for the format.

If we see a blank line, we discard it and do nothing.

## Types of mnemonics:
* Instructions
* ALU Operations
* Labels
* Internal Addresses
* Conditions

## Architecture Details:
I created the components from scratch. Some of the designs I borrowed or were influenced by others, and they'll be credited when the build is done.
* 6-bit CPU.
* 8 instructions.
* 4 two-line instructions, which take 3 clock cycles.
* 4 one-line instructions, which take 2 clock cycles.
* Acc, or the A register, cannot be written directly to. It must be written to from an ALU operation.
* The temp register, or the B register, can be directly written to.
* Unknown cycle speed. This is part of finalization.
* Instructions: Conditional halt, conditional jump, read/write from memory, read/write from temp/B, read from acc/A, ALU operation.

#### Instruction Format:

IsTwoLine?[1-bit] Instruction[2-bit] Operand[3-bit] | OptionalSecondLine[6-bit]

#### ALU operations are performed a bit wierdly.
First you have to select the ALU instruction (111), then tell it where you want the result to go to in the next 3 bits. Then, the second line contains the opcode, but it's not a normal opcode. The bottom 4-bits of this opcode is the actual code that gets fed into the ALU Decoder to generate the control signals for the ALU. The top two bits control whether A/B is inverted respectively. I chose to do this, because it provided the maximum amount of instructions to the user, without requiring a giant 6-bit decoder.

#### The components:

CPU - Contains A and B registers, and the ALU.

Secondary Controller - Takes the operand and converts it into whatever signals the instruction needs.

ALU Decoder - Takes the code part of the opcode and converts it into control signals for the ALU.

Fetch Registers - Stores the instructions to be executed.

Primary Controller - The main logic of the computer. Contains the state counter, and generates all of the high-level control signals. Is responsible for fetching and executing.

IO - The input/output port of the computer. Has two sets of read/write signals, one coming from the computer, the other going to the computer.
