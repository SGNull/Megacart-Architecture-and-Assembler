## Megacart Architecture and Assembler
### Preface
For quite some time I have been interested in computer architecture and designing the architecture using digital circuitry, and this is the most modern of all the architectures that I have designed.

Funnily enough, I happened to design this one in Minecraft.

Previously, I had developed a fairly capable computer system and very small-scale network to go along with it in Logisim. It worked very well (even if Logisim wasn't a big fan of simulating entire computer networks). I even wrote multiple assemblers that ran on the computer! But it still wasn't a "real computer". At the time, and up until only very recently, I didn't really know what I meant when I said "real computer". I don't have any formal knowledge of computer architecture, so I was just picking up on things as I made more and more advancements. Now I know what was off about my previous architectures, and, by extention, what makes this one different.

My previous architectures were all a lot like the ["Simple Computer"](https://imgur.com/gallery/tFAgH) I made back a few years ago. None of them had RAM, and if they did, it was only used as a sort-of giant register bank that was a bit more difficult to access. They had a very simple IO with only one or two ports. This was OK at the time, because I didn't really plan on doing anything more with them, but now I realize this is not a sustainable practice. But there were two really big problems that stand out to me now:

### Problems

1. I didn't know what a CPU really was. I just new what the parts to one were, and how they should interact. This lead to most of my "computer" architectures being only CPU's with some ROM attatched.
 
2. Even more rudamentary was the way that instructions were handled. Instructions were read directly from the hard drive, and acted on within one clock cycle. This severely limited the possibilities for many of my architectures. You can see the issue in the Simple Computer. I have two instructions with absolutely no operands, yet they take the exact amount of bits that the other instructions do. The solution to this is obviously variable length instructions, but this is not possible under such a rigid memory system. Additionally, this meant that the computer could only be programmed by messing with the hard drive. Finally, there's no possibility for pipelining!
 
### What's Different with Megacart?
The Megacart Architecture fixes most of these things. It gets its instructions directly from RAM, and it fetches them. This means that instructions are either two or three clock cycles. The variance here is because of another thing: variable length instructions. Some instructions only need one operand, so they only take up one line of memory, or 6-bits. Some require more, like a conditional jump instruction, which needs two. These take up two lines of memory.

Instructions are loaded using the CPU's data bus, so there sadly isn't room for pipelining. However, this means that instructions can actually be loaded into the RAM from a hard drive. That's where this assembler comes in.

## The Assembler
I had learned from my previous project in Logisim two things.

* It's way easier to use a computer when it has an assembler.
* It's very difficult to write an assembler in machine code.

So I didn't do the latter. Instead, I wrote the assembler here, in C#. I hadn't worked with C# very much before, and I struggled using it last year for a project, so I thought I would add a new language to my toolbelt. Plus, I kinda like C# the most out of the couple languages I know.

Here's how this assembler works:
1. It reads the mnemonics from the Mnemonics folder into tables.
2. It parses the file for labels and variables, which it will then treat as mnemonics.
3. Then it parses the file again and does the actual assembling, matching mnemonics with values and sending the results to a list of machine code lines.
4. After that, it adds the variables to the machine code list.
5. Lastly, it writes that list to the new .machinecode.txt file.

In order to use it, you have to pass it the file you want it to assemble as an argument in the command line. If there's a bug, it will tell you. Here's some example code:

```
//Here's a comment
D10 DecimalVariable 23
D6 BinaryVariable 010111

:ThisIsALabel
JumpIf a=b ThisIsALabel

DL LabelValueForThisIsALabel ThisIsALabel

// The assembler allows you to edit and add mnemonics. There are currently quite a few alternatives to certain ones.
END .
HLT TRUE
halt true
```

### Some Time Later
I eventually came back to this project after school had started again. I really wanted to _use_ the computer, not just leave it sit like most of my other designs. However, there were a few things preventing me from doing this.

* The computer had some hiccups when loading in. Certain gates would be tripped, or the program counter would go haywire, or a value might get inverted.
* Things like multiplication and function calls took up a lot of memory, and the latter required the user to know how their assembly code was turned into machine code.
* The hard-drive was practically smashed on to the side of the computer. It was really poorly designed.
* There was only one IO slot, and I couldn't do much with that.

So I went back in and started working on the architecture again. What I'm currently working on I'm calling the Megacart V2 Architecture. It comes with a few new things that will shape how you program.

## New Hardware Developments

1. The BIOD is like a simple switch attatched to the IO. You tell it what port you want to interact with, and how many times you want to interact with it. Then, everything you read/write to the BIOD will be sent to that port.
2. There's the "scaffolding" now which, while it doesn't alter programming, it makes loading the machine infinitely easier. It also fixes loading bugs!
3. There are now 4 RAM slots that you can load data into. You switch between them using port 1 of the BIOD
4. The RLS is the idea that started all of this. It's short for RAM Loading System, and it is a chip that comes pre-installed on the computer when you get it. When you turn on the computer, the RLS will be active by default. Assuming the hard drive is on port 0 of the BIOD, it will automatically load the data into RAM.

So far, 1,2, and part of 3 is done. I'm currently working on the RLS and there are a few concerns that I have. For one, it might be really..._really_ slow. Hopefully not, hopefully an algorithm that I've thought of will make it run quicker, but we'll see. Also, I still have to work out all the interactions between the computer and the RLS. You're able to switch to it at any time using BIOD port 0. I have to figure out program counter logic things. I got inspiration for 4 slots from my Logisim project, but that architecture gave each slot its own program counter, making switching easier. I don't think I could do that here, since the CPU is more of a set-in-stone piece of hardware than it was in that architecture.
