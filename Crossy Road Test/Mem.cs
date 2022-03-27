using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using static ModuleHelper.Helper;

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
            if (proc.Length.Equals(0))
            {
                Console.WriteLine("Failed To Find Crossy Road's Process.");
                Console.ReadKey();
                return false;
            }
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
                    if (moduleName == modToFindInProcess)
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

        public long PatternScan(long startAddress, long endAddress, byte[] sigInBytes, string[] sigInString, int skip)
        {
            byte[] array = new byte[endAddress - startAddress];
            long num = endAddress - startAddress;
            if (num >= 3899391L) // This number might be off for 32bit games since doing big numbers fucks up the scans for some reason
            {
                long num2 = 0L;
                while (num2 < num)
                {
                    if (num - num2 > 3899391L)
                    {
                        byte[] array2 = this.ReadBytes(startAddress + num2, 3899391);
                        array2.CopyTo(array, (int)num2);
                        num2 += 3899391L;
                    }
                    else
                    {
                        long num3 = num - num2;
                        byte[] array3 = this.ReadBytes(startAddress + num2, (int)num3);
                        array3.CopyTo(array, (int)num3);
                        num2 += 3899391L;
                    }
                }
            }
            else
            {
                array = this.ReadBytes(startAddress, (int)(endAddress - startAddress));
            }
            for (int i = 0; i < array.Length; i += skip)
            {
                int num4 = 0;
                int num5 = i;
                while (num5 < sigInBytes.Length + i && (array[num5] == sigInBytes[num4] || sigInString[num4] == "?"))
                {
                    num4++;
                    if (num4 == sigInBytes.Length)
                    {
                        return startAddress + i;
                    }
                    num5++;
                }
            }
            return 0L;
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