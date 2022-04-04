using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using static ModuleHelper.Helper;
using System.Collections.Generic;

namespace Memory
{
    public class Mem
    {
        public bool IsProcessRunning(string processName)
        {
            try
            {
                Process process = Process.GetProcessesByName(processName)[0];
                if (process.Handle.ToInt64() != 0L)
                {
                    this.BaseAddress = process.MainModule.BaseAddress.ToInt64();
                    this.ProcessID = process.Id;
                    this.ProcessHandle = process.Handle;
                    return true;
                }
            }
            catch (Exception)
            {
                this.BaseAddress = 0L;
                this.ProcessID = 0;
                this.ProcessHandle = IntPtr.Zero;
                return false;
            }
            return false;
        }

        public bool IsProcessWithModulesRunning(string processName, string modToFindInProcess)
        {
            IntPtr[] modulePointers = Array.Empty<IntPtr>();
            int bytesNeeded = 0;
            Process[] proc = Process.GetProcessesByName(processName);

            if (proc.Length.Equals(0)) return false;

            Process p = proc[0];
            if (!Native.EnumProcessModulesEx(p.Handle, modulePointers, 0, out bytesNeeded, (uint)Native.ModuleFilter.ListModulesAll))
            {
                Console.WriteLine("Something Went Wrong, Try Restarting App.");
                this.BaseAddress = 0L;
                this.ProcessID = 0;
                this.ProcessHandle = IntPtr.Zero;
                this.SizeOfImage = 0U;
                return false;
            }

            int totalNumberofModules = bytesNeeded / IntPtr.Size;
            modulePointers = new IntPtr[totalNumberofModules];

            if (Native.EnumProcessModulesEx(p.Handle, modulePointers, bytesNeeded, out bytesNeeded, (uint)Native.ModuleFilter.ListModulesAll))
            {
                for (int index = 0; index < totalNumberofModules; index++)
                {
                    StringBuilder moduleFilePath = new StringBuilder(1024);
                    Native.GetModuleFileNameEx(p.Handle, modulePointers[index], moduleFilePath, (uint)(moduleFilePath.Capacity));

                    string moduleName = Path.GetFileName(moduleFilePath.ToString());
                    Native.ModuleInformation moduleInformation = new Native.ModuleInformation();
                    Native.GetModuleInformation(p.Handle, modulePointers[index], out moduleInformation, (uint)(IntPtr.Size * modulePointers.Length));
                    if (String.Compare(moduleName, modToFindInProcess) == 0)
                    {
                        if (IsProcessRunning("Crossy Road"))
                        {
                            this.BaseAddress = (long)moduleInformation.lpBaseOfDll;
                            this.SizeOfImage = moduleInformation.SizeOfImage;
                            return true;
                        }
                    }
                }
                return false;
            }
            else return false;
        }

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, long lpBaseAddress, [Out] byte[] lpBuffer, uint nSize, out uint lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, long lpBaseAddress, byte[] lpBuffer, uint nSize, out uint lpNumberOfBytesRead);

        public void WriteInt(long pAddress, int value)
        {
            try
            {
                uint num = 0U;
                Mem.WriteProcessMemory(this.ProcessHandle, pAddress, BitConverter.GetBytes(value), 4U, out num);
            }
            catch (Exception)
            {
            }
        }

        public void WriteByte(long _lpBaseAddress, byte _Value)
        {
            byte[] bytes = BitConverter.GetBytes((short)_Value);
            uint num = 0u;
            Mem.WriteProcessMemory(this.ProcessHandle, _lpBaseAddress, bytes, (uint)(bytes.Length - 1), out num);
        }

        public void WriteUInt(long pAddress, uint value)
        {
            try
            {
                uint num = 0U;
                Mem.WriteProcessMemory(this.ProcessHandle, pAddress, BitConverter.GetBytes(value), 4U, out num);
            }
            catch (Exception)
            {
            }
        }

        public long GetPointerInt(long add, long[] offsets, int level)
        {
            long num = add;
            for (int i = 0; i < level; i++)
            {
                num = this.ReadInt64(num) + offsets[i];
            }
            return num;
        }

        public void WriteBytes(long _lpBaseAddress, byte[] _Value)
        {
            for (int i = 0; i < _Value.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes((short)_Value[i]);
                uint num = 0u;
                Mem.WriteProcessMemory(this.ProcessHandle, _lpBaseAddress, bytes, (uint)(bytes.Length - 1), out num);
            }
        }

        public void WriteXBytes(long _lpBaseAddress, byte[] _Value)
        {
            uint zero = 0u;
            Mem.WriteProcessMemory(this.ProcessHandle, _lpBaseAddress, _Value, (uint)_Value.Length, out zero);
        }

        public long ReadInt64(long pAddress)
        {
            try
            {
                uint num = 0U;
                byte[] array = new byte[8];
                if (Mem.ReadProcessMemory(this.ProcessHandle, pAddress, array, 8U, out num))
                {
                    return BitConverter.ToInt64(array, 0);
                }
            }
            catch (Exception)
            {
            }
            return 0L;
        }

        public string ReadString(long pAddress)
        {
            try
            {
                byte[] array = new byte[1280];
                uint num = 0U;
                if (Mem.ReadProcessMemory(this.ProcessHandle, pAddress, array, 1280U, out num))
                {
                    string text = "";
                    int num2 = 0;
                    while (array[num2] != 0)
                    {
                        string str = text;
                        char c = (char)array[num2];
                        text = str + c.ToString();
                        num2++;
                    }
                    return text;
                }
            }
            catch (Exception)
            {
            }
            return "";
        }

        public int ReadInt(long pAddress)
        {
            try
            {
                uint num = 0U;
                byte[] array = new byte[4];
                if (Mem.ReadProcessMemory(this.ProcessHandle, pAddress, array, 4U, out num))
                {
                    return BitConverter.ToInt32(array, 0);
                }
            }
            catch (Exception)
            {
            }
            return 0;
        }

        public void WriteFloat(long pAddress, float value)
        {
            try
            {
                uint num = 0U;
                Mem.WriteProcessMemory(this.ProcessHandle, pAddress, BitConverter.GetBytes(value), 4U, out num);
            }
            catch (Exception)
            {
            }
        }

        public float ReadFloat(long pAddress)
        {
            try
            {
                uint num = 0U;
                byte[] array = new byte[4];
                if (Mem.ReadProcessMemory(this.ProcessHandle, pAddress, array, 4U, out num))
                {
                    return BitConverter.ToSingle(array, 0);
                }
            }
            catch (Exception)
            {
            }
            return 0f;
        }

        public byte[] ReadBytes(long pAddress, int length)
        {
            byte[] array = new byte[length];
            uint num = 0U;
            Mem.ReadProcessMemory(this.ProcessHandle, pAddress, array, (uint)length, out num);
            return array;
        }

        public void WriteBool(long pAddress, bool value)
        {
            try
            {
                byte[] buff = new byte[] { value ? ((byte)1) : ((byte)0) };
                uint num = 0U;
                Mem.WriteProcessMemory(this.ProcessHandle, pAddress, buff, (uint)buff.Length, out num);
            }
            catch (Exception)
            {
            }
        }

        public void WriteString(long pAddress, string pString)
        {
            try
            {
                uint num = 0U;

                if (Mem.WriteProcessMemory(this.ProcessHandle, pAddress, Encoding.UTF8.GetBytes(pString), (uint)pString.Length, out num))
                {
                    byte[] lpBuffer = new byte[1];
                    Mem.WriteProcessMemory(this.ProcessHandle, pAddress + (long)pString.Length, lpBuffer, 1U, out num);
                }
            }
            catch (Exception) { }
        }

        public bool IsValidAddr(long addr)
        {
            return addr >= BaseAddress && SizeOfImage + BaseAddress >= addr;
        }

        public long[] FindBytes(byte?[] needle, long startAddress, long endAddress, bool firstMatch = false, int bufferSize = 0xFFFF)
        {
            List<long> results = new List<long>();
            long searchAddress = startAddress;

            int needleIndex = 0;
            int bufferIndex = 0;

            while (true)
            {
                try
                {
                    byte[] buffer = this.ReadBytes(searchAddress, bufferSize);

                    for (bufferIndex = 0; bufferIndex < buffer.Length; bufferIndex++)
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

        public void universalResolve(ref long offset, int addBeforeDeref, int addAfterDeref)
        {
            long Base = offset + addBeforeDeref;
            long readint = ReadInt(Base);
            long final = Base + readint + addAfterDeref;
            offset = final;
        }

        public long BaseAddress;

        public int ProcessID;

        public uint SizeOfImage;

        public IntPtr ProcessHandle;
    }
}