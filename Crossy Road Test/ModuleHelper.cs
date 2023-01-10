using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace ModuleHelper
{
    public class Helper
    {
        public class Native
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct ModuleInformation
            {
                public IntPtr lpBaseOfDll;
                public uint SizeOfImage;
                public IntPtr EntryPoint;
            }

            public enum ModuleFilter
            {
                ListModulesDefault = 0x0,
                ListModules32Bit = 0x01,
                ListModules64Bit = 0x02,
                ListModulesAll = 0x03,
            }

            [DllImport("psapi.dll")]
            public static extern bool EnumProcessModulesEx(IntPtr hProcess, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)][In][Out][AllowNull] IntPtr[] lphModule, int cb, [MarshalAs(UnmanagedType.U4)] out int lpcbNeeded, uint dwFilterFlag);

            [DllImport("psapi.dll")]
            public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In][MarshalAs(UnmanagedType.U4)] uint nSize);

            [DllImport("psapi.dll", SetLastError = true)]
            public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out ModuleInformation lpmodinfo, uint cb);
        }
    }
}
