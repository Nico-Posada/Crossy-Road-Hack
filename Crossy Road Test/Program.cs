using Memory;
using System;
using System.Linq;
using System.Threading;
using ExtensionMethods;

namespace Crossy_Road_Test
{
    public class Program
    {
        static readonly Mem m = new();

        private struct Offsets
        {
            public static long score_function { get; set; }
            public static long user_info      { get; set; }

            public const int instruction_offset  = 0x06;
            public const int coin_offset         = 0xC4;
            public const int high_score_offset   = 0x04;
        }

        private static bool Init()
        {
            // Scan for our addresses
            var score_func_scan = m.FindBytes("55 8B EC 56 8B F1 ? ? ? ? ? ? ? ? 89 86 ? ? ? ? 8B 8E ? ? ? ? 3B C8", m.BaseAddress + 0x14F0000, m.BaseAddress + 0x1580000, true).FirstOrDefault();
            var user_info_scan = m.FindBytes("B8 ? ? ? ? 8B 08 80 39 00 8B 16 E8", m.BaseAddress + 0x14F0000, m.BaseAddress + 0x1580000, true).FirstOrDefault();

            // check to make sure scans were found
            if (user_info_scan == default || score_func_scan == default)
            {
                Console.WriteLine("Something went wrong with the pattern scan.");
                Thread.Sleep(4000);
                return false;
            }

            // set struct values
            Offsets.score_function = score_func_scan + Offsets.instruction_offset;
            Offsets.user_info = m.Read<int>(user_info_scan + 1); // resolving mov instruction

            // make sure those modified values make sense (should almost always be fine)
            bool result = m.IsValidAddr(Offsets.score_function) && m.IsValidAddr(Offsets.user_info);

            if (!result)
            {
                Console.WriteLine("Unknown error occured.");
                Thread.Sleep(4000);
            }

            return result;
        }

        // When hack is inactive, the first two bytes of the address will be 0x8B 0x86.
        // If you convert that to ushort, you get 34443, so if we read ushort
        // On that address and it isnt 34443, then we know the hack is active.
        private static bool IsActive(long addr) => m.Read<ushort>(addr) != 34443;

        static void Main()
        {
            if (!m.IsProcessRunning("Crossy Road", "Crossy Road.dll"))
            {
                Console.WriteLine("Process not found.");
                Thread.Sleep(4000);
                return;
            }

            if (!Init())
                return;

            while (true)
            {
                Console.Clear();
                Console.Write("Crossy Road Console Menu:\n[1] Max Score Hack\n[2] Set Coin Count\n[3] Set High Score\n\nInput: ");
                string read = Console.ReadLine();

                if (!int.TryParse(read, out int input) || 0 >= input || input > 3)
                    continue;

                switch (input)
                {
                    case 1:
                        bool active = IsActive(Offsets.score_function);
                        Console.Clear();
                        Console.Write("Max Score Hack\nHack: ");
                        Console.ForegroundColor = active ? ConsoleColor.Green : ConsoleColor.Red;
                        Console.WriteLine(active ? "Active" : "Inactive");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("\nPress any key to toggle hack...");
                        Console.ReadKey();

                        if (!active) m.WriteBytes(Offsets.score_function, new byte[] { 0xB8, 0x0F, 0x27, 0x00, 0x00, 0x90, 0x90, 0x90 });
                        // ASSEMBLY
                        //  mov eax, 9999
                        //  nop
                        //  nop
                        //  nop

                        else m.WriteBytes(Offsets.score_function, new byte[] { 0x8B, 0x86, 0xA4, 0x00, 0x00, 0x00, 0x03, 0xC2 });
                        // ASSEMBLY
                        //  mov eax,[esi+000000A4]
                        //  add eax,edx

                        Console.WriteLine("Successfully toggled!");
                        Thread.Sleep(4000);

                        break;
                    case 2:
                        while (true)
                        {
                            Console.Clear();
                            Console.Write("Set Coin Count\nInput: ");
                            string coin_str = Console.ReadLine();

                            if (!int.TryParse(coin_str, out int coins))
                                continue;

                            m.Write(m.Read<int>(Offsets.user_info) + Offsets.coin_offset, coins.encrypt());
                            break;
                        }

                        Console.WriteLine("Successfully set coins!");
                        Thread.Sleep(4000);

                        break;
                    case 3:
                        // Initialize registry values
                        int eax, ebx, ecx, edx, esi, edi, r8;

                        // r8 isnt actually the registry used here, but it overlaps with another registry so idc
                        r8 = m.Read<int>(Offsets.user_info);
                        r8 = m.Read<int>(r8 + 0xAC);

                        // recreating the decryption in a SharedLibrary.dll function that gets a ptr value that helps get the gamemode
                        #region SharedLibrary.dll+33AE70 func recreation

                        // ecx is the parameter passed to the function, a little debugging and you can figure out its just this
                        ecx = m.Read<int>(m.Read<int>(m.BaseAddress + 0xAAFF88) + 0x154);

                        edx = ecx;
                        if (edx != 0)
                            edx += 8;
                        eax = 0x15051505;
                        esi = eax;
                        edi = m.Read<int>(ecx + 4);

                        while (edi > 0)
                        {
                            ecx = eax;
                            ecx <<= 5;
                            ecx += eax;
                            eax >>= 0x1B;
                            eax += ecx;
                            eax ^= m.Read<int>(edx);
                            if (edi <= 2) break;
                            ecx = esi;
                            ecx <<= 5;
                            ecx += esi;
                            esi >>= 0x1B;
                            esi += ecx;
                            esi ^= m.Read<int>(edx + 4);
                            edx += 8;
                            edi -= 4;
                        }

                        ecx = esi * 0x5D588B65;
                        eax += ecx;
                        // EAX IS THE FINAL RESULT
                        #endregion

                        ebx = eax & 0x7FFFFFFF;
                        esi = m.Read<int>(r8 + 4);
                        ecx = m.Read<int>(esi + 4);
                        eax = ebx;
                        edx = eax % ecx;
                        esi = m.Read<int>(esi + edx * 4 + 8);

                        while (true)
                        {
                            edx = m.Read<int>(r8 + 8);
                            eax = esi;
                            eax += eax;
                            if (m.Read<int>(edx + eax * 8 + 8) == ebx) break;
                            eax = m.Read<int>(r8 + 8);
                            esi = m.Read<int>(eax + esi * 16 + 0xC);
                        }

                        eax = m.Read<int>(r8 + 8);
                        eax = m.Read<int>(eax + esi * 16 + 0x14);
                        // END DECRYPTION

                        string mode = "";
                        switch (esi) // gamemode index
                        {
                            case 0:
                                mode = "Default";
                                break;
                            case 1:
                                mode = "Psy";
                                break;
                            case 2:
                                mode = "Pac-Man";
                                break;
                            default:
                                mode = "Unknown";
                                break;
                        }

                        while (true)
                        {
                            Console.Clear();
                            Console.Write($"Set High Score (Will Set Score For Mode You're Currently In)\nCurrent Mode: {mode}\nInput: ");
                            string high_score_str = Console.ReadLine();

                            if (!int.TryParse(high_score_str, out int score))
                                continue;

                            m.Write(eax + Offsets.high_score_offset, score.encrypt());
                            break;
                        }

                        Console.WriteLine("Successfully set high score!");
                        Thread.Sleep(4000);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
