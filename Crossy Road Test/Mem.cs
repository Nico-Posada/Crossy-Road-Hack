using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using static ModuleHelper.Helper;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Memory
{
    public class Mem
    {
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, long lpBaseAddress, [Out] byte[] lpBuffer, uint nSize, out uint lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, long lpBaseAddress, byte[] lpBuffer, uint nSize, out uint lpNumberOfBytesRead);

        public bool IsProcessRunning(string processName)
        {
            Process process = Process.GetProcessesByName(processName).FirstOrDefault();
            if (process != default && process.Handle.ToInt64() != 0L)
            {
                BaseAddress = process.MainModule.BaseAddress.ToInt64();
                ProcessID = process.Id;
                ProcessHandle = process.Handle;
                SizeOfImage = (uint)process.WorkingSet64;
                return true;
            }
            else
            {
                BaseAddress = 0L;
                ProcessID = 0;
                ProcessHandle = IntPtr.Zero;
                SizeOfImage = 0U;
                return false;
            }
        }

        public bool IsProcessRunning(string processName, string modToFindInProcess)
        {
            Process proc = Process.GetProcessesByName(processName).FirstOrDefault();

            if (proc == default || !Native.EnumProcessModulesEx(proc.Handle, null, 0, out int bytesNeeded, (uint)Native.ModuleFilter.ListModulesAll))
            {
                Console.WriteLine("Something Went Wrong, Try Restarting App.");
                BaseAddress = 0L;
                ProcessID = 0;
                ProcessHandle = IntPtr.Zero;
                SizeOfImage = 0U;
                return false;
            }

            int totalNumberofModules = bytesNeeded / IntPtr.Size;
            var modulePointers = new IntPtr[totalNumberofModules];

            if (Native.EnumProcessModulesEx(proc.Handle, modulePointers, bytesNeeded, out _, (uint)Native.ModuleFilter.ListModulesAll))
            {
                foreach(var ptr in modulePointers)
                {
                    StringBuilder moduleFilePath = new StringBuilder(1024);
                    Native.GetModuleFileNameEx(proc.Handle, ptr, moduleFilePath, (uint)moduleFilePath.Capacity);

                    string moduleName = Path.GetFileName(moduleFilePath.ToString());
                    Native.GetModuleInformation(proc.Handle, ptr, out Native.ModuleInformation moduleInformation, (uint)(IntPtr.Size * modulePointers.Length));
                    if (moduleName == modToFindInProcess)
                    {
                        if (IsProcessRunning("Crossy Road"))
                        {
                            BaseAddress = moduleInformation.lpBaseOfDll.ToInt64();
                            SizeOfImage = moduleInformation.SizeOfImage;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void WriteBytes(long address, byte[] bytes)
        {
            if (!WriteProcessMemory(ProcessHandle, address, bytes, (uint)bytes.Length, out _))
                throw new ArgumentException(new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message);
        }

        public byte[] ReadBytes(long pAddress, int length)
        {
            byte[] array = new byte[length];
            if (!ReadProcessMemory(ProcessHandle, pAddress, array, (uint)length, out _))
                throw new ArgumentException(new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message);
            return array;
        }

        /// <summary>
        /// Read memory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <returns></returns>
        public unsafe T Read<T>(long address)
        {
            int size = Unsafe.SizeOf<T>();
            var buffer = ReadBytes(address, size);
            fixed (byte* b = buffer)
            {
                return Unsafe.Read<T>(b);
            }
        }

        /// <summary>
        /// Write memory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="address"></param>
        /// <returns></returns>
        public unsafe void Write<T>(long address, T value)
        {
            int size = Unsafe.SizeOf<T>();
            byte[] buffer = new byte[size];
            fixed (byte* b = buffer)
            {
                Unsafe.Write(b, value);
            }

            WriteBytes(address, buffer);
        }

        public string ReadString(long pAddress, int len = 512)
        {
            byte[] buffer = ReadBytes(pAddress, len);
            var sb = new StringBuilder();
            for (int i = 0; buffer[i] != 0 && i < len; ++i)
                sb.Append((char)buffer[i]);
            return sb.ToString();
        }

        public bool IsValidAddr(long addr) => addr >= BaseAddress && SizeOfImage + BaseAddress >= addr;

        public long[] FindBytes(string sig, long startAddress, long endAddress, bool firstMatch = false, int bufferSize = 0xFFFF)
        {
            var results = new List<long>();
            string[] array = sig.Split(' ');
            byte?[] needle = new byte?[array.Length];
            for (int i = 0; i < needle.Length; i++)
                needle[i] = (array[i] == "?" || array[i] == "??") ? null : byte.Parse(array[i], System.Globalization.NumberStyles.HexNumber);

            long searchAddress = startAddress;
            int needleIndex = 0;

            while (true)
            {
                try
                {
                    byte[] buffer = ReadBytes(searchAddress, bufferSize);

                    for (int bufferIndex = 0; bufferIndex < buffer.Length; bufferIndex++)
                    {
                        if (needle[needleIndex] == null)
                        {
                            needleIndex++;
                            continue;
                        }

                        if (needle[needleIndex] == buffer[bufferIndex])
                        {
                            needleIndex++;

                            if (needleIndex == needle.Length)
                            {
                                results.Add(searchAddress + bufferIndex - needle.Length + 1);

                                if (firstMatch)
                                    return results.ToArray();

                                needleIndex = 0;
                            }
                        }
                        else
                        {
                            needleIndex = 0;
                        }
                    }
                }
                catch
                {
                    break;
                }

                searchAddress += bufferSize;

                if (searchAddress > endAddress)
                    break;
            }

            return results.ToArray();
        }

        public long BaseAddress;

        public int ProcessID;

        public uint SizeOfImage;

        public IntPtr ProcessHandle;
    }
}
