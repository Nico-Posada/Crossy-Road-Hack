using System;
using System.Threading;

namespace Crossy_Road_Test
{
    internal class Program
    {
        static void Main()
        {
            Memory.Mem m = new Memory.Mem();
            Start: Console.Clear();
            Console.Write("Crossy Road Instant Max Score Hack\n[1] Activate\n[2] Deactivate\n\nInput: ");
            string read = Console.ReadLine();
            bool? getResponse = read == "1" ? true : read == "2" ? false : null;

            if (getResponse.Equals(null))
            {
                Console.WriteLine("\nPlease Input Either a 1 or a 2 don't be dumb.");
                Thread.Sleep(4000);
                goto Start;
            }

            if (m.IsProcessWithModulesRunning("Crossy Road", "Crossy Road.dll"))
            {
                // scan for xref to the main function I'm writing the code cave in (using xref for consistent sig that hopefully wont die on updates)
                long addr = m.PatternScan(m.BaseAddress + 0x14F0000, m.BaseAddress + 0x1580000, new byte[] { 0xE8, 0, 0, 0, 0, 0xE9, 0, 0, 0, 0, 0xBA, 0, 0, 0, 0, 0xFF, 0x15, 0, 0, 0, 0, 0xB8, 0, 0, 0, 0, 0x8B, 0xF8 }, new string[] { "E8", "?", "?", "?", "?", "E9", "?", "?", "?", "?", "BA", "?", "?", "?", "?", "FF", "15", "?", "?", "?", "?", "B8", "?", "?", "?", "?", "8B", "F8" }, 1);
                if (m.IsValidAddr(addr))//                                                                   E8 ? ? ? ? E9 ? ? ? ? BA ? ? ? ? FF 15 ? ? ? ? B8 ? ? ? ? 8B F8  -  if game updates and the tool doesnt work, search for this sig and make sure that 1) its not dead and 2) it falls into the pattern scan range
                {
                    m.universalResolve(ref addr, 1, 0x80); // Resolve, added 0x80 because I'm not going to the start of the function rather to the trampoline
                    if (getResponse.Equals(true))
                    {
                        long trampolineTo = addr + 0x56;
                        m.WriteXBytes(addr, new byte[] { 0xEB, 0x54 }); // jmp to code cave
                        m.WriteXBytes(trampolineTo, new byte[] { 5, 0x0F, 0x27, 0, 0, 0xE9 }); // assembly code (ew)
                        m.WriteInt(trampolineTo + 6, (int)(addr - trampolineTo - 8));  // Calculate and write relative address
                        Console.WriteLine("\nSuccessfully Activated!");
                        Thread.Sleep(4000);
                        goto Start; // We did it! Go back to the start (:
                    }
                    else
                    {
                        m.WriteXBytes(addr, new byte[] { 3, 0xC2 }); // This is the original code that was overwriten, just reverts it so it doesnt redirect to our cave
                        Console.WriteLine("\nSuccessfully Deactivated!");
                        Thread.Sleep(4000);
                        goto Start;
                    }
                }
                else
                {
                    Console.WriteLine("Something went wrong with the pattern scan.");
                    Thread.Sleep(4000);
                    goto Start;
                }
            }
            else
            {
                Console.WriteLine("Something went wrong");
                Thread.Sleep(4000);
                goto Start;
            }
        }
    }
}
