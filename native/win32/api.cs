using System;
using System.Runtime.InteropServices;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WinInfo
{
  [StructLayout(LayoutKind.Sequential)]
  public struct RECT
  {
    public int Left;        // x position of upper-left corner
    public int Top;         // y position of upper-left corner
    public int Right;       // x position of lower-right corner
    public int Bottom;      // y position of lower-right corner
  }

  [StructLayout(LayoutKind.Sequential)] //, CharSet=CharSet.Auto)]
  public struct MonitorInfoInternal
  {
    public UInt32 size;
    public RECT monitor;
    public RECT work;
    public UInt32 flags;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
    public string deviceName;
  }

  public struct MonitorInfo
  {
    public RECT monitor;
    public RECT work;
    public UInt32 flags;
    public string deviceName;
    public double[] scale;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct WINDOWINFO
  {
    public uint cbSize;
    public RECT rcWindow;
    public RECT rcClient;
    public uint dwStyle;
    public uint dwExStyle;
    public uint dwWindowStatus;
    public uint cxWindowBorders;
    public uint cyWindowBorders;
    public ushort atomWindowType;
    public ushort wCreatorVersion;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct DEVMODE
  {
    private const int CCHDEVICENAME = 0x20;
    private const int CCHFORMNAME = 0x20;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
    public string dmDeviceName;
    public short dmSpecVersion;
    public short dmDriverVersion;
    public short dmSize;
    public short dmDriverExtra;
    public int dmFields;
    public int dmPositionX;
    public int dmPositionY;
    public ScreenOrientation dmDisplayOrientation;
    public int dmDisplayFixedOutput;
    public short dmColor;
    public short dmDuplex;
    public short dmYResolution;
    public short dmTTOption;
    public short dmCollate;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
    public string dmFormName;
    public short dmLogPixels;
    public int dmBitsPerPel;
    public int dmPelsWidth;
    public int dmPelsHeight;
    public int dmDisplayFlags;
    public int dmDisplayFrequency;
    public int dmICMMethod;
    public int dmICMIntent;
    public int dmMediaType;
    public int dmDitherType;
    public int dmReserved1;
    public int dmReserved2;
    public int dmPanningWidth;
    public int dmPanningHeight;
  }
  
  public class WinApi 
  {
    public static uint QueryLimitedInformation = 0x1000;
    public static uint WS_THICKFRAME  = 0x00040000;
    public static uint WS_OVERLAPPED  = 0x00000000;
    public static uint WS_MINIMIZEBOX = 0x00020000;
    public static uint WS_MAXIMIZEBOX = 0x00010000;
    public static uint WS_SYSMENU     = 0x00080000;
    public static uint WS_CAPTION     = 0x00C00000;     /* WS_BORDER | WS_DLGFRAME  */

    public static uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | 
      WS_CAPTION     | 
      WS_SYSMENU     | 
      WS_THICKFRAME  | 
      WS_MINIMIZEBOX | 
      WS_MAXIMIZEBOX;

    delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    #region User32
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.SysUInt)]
    public static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetMonitorInfo(IntPtr hMptr, ref MonitorInfoInternal info);

    [DllImport("user32.dll")]
    public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsZoomed(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowInfo(IntPtr hWnd, ref WINDOWINFO info);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.SysUInt)]
    public static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint flags);

    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms633505(v=vs.85).aspx
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.SysUInt)]
    public static extern IntPtr GetForegroundWindow();

    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms633520(v=vs.85).aspx
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.I4)]
    public static extern int GetWindowTextW(
      IntPtr hwnd, 
      [MarshalAs(UnmanagedType.LPWStr)]
      StringBuilder buffer, 
      int maxCount 
    );

    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms633521(v=vs.85).aspx
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.I4)]
    public static extern int GetWindowTextLengthW(IntPtr hwnd);	

    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms633522(v=vs.85).aspx
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.I4)]
    public static extern int GetWindowThreadProcessId(IntPtr hwd, ref int proccessId);
    #endregion

    #region Kernel32
    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms684320(v=vs.85).aspx
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.SysUInt)]
    public static extern IntPtr OpenProcess(uint access, bool inheritHandle, int processId);

    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms724211(v=vs.85).aspx
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr hwnd);

    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms684919(v=vs.85).aspx
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool QueryFullProcessImageNameW(
      IntPtr hproc,
      uint flags,
      [MarshalAs(UnmanagedType.LPWStr)]
      StringBuilder lpExeName, 
      ref int size
    );
    #endregion

    public static void checkError(bool good) {
      if (!good) {
        System.Console.Error.WriteLine(new Win32Exception().Message);
        Environment.Exit(1);
      }
    }

    public static List<MonitorInfo> getMonitors() {
      const int ENUM_CURRENT_SETTINGS = -1;

      Dictionary<string, RECT> nativeSize = new Dictionary<string, RECT>();
      foreach (Screen screen in Screen.AllScreens)
      {
          DEVMODE dm = new DEVMODE();
          dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
          EnumDisplaySettings(screen.DeviceName, ENUM_CURRENT_SETTINGS, ref dm);
          RECT r = new RECT();
          r.Right = dm.dmPelsWidth;
          r.Bottom = dm.dmPelsHeight;
          nativeSize[screen.DeviceName] = r;
      }

      List<MonitorInfo> col = new List<MonitorInfo>();
      EnumDisplayMonitors( IntPtr.Zero, IntPtr.Zero, delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) {
          MonitorInfoInternal mii = new MonitorInfoInternal();
          mii.size = (uint)Marshal.SizeOf( mii );
          bool success = GetMonitorInfo(hMonitor, ref mii);
          checkError(success);
          if (success) {
            MonitorInfo mi = new MonitorInfo();
            mi.monitor = mii.monitor;
            mi.work = mii.work;
            mi.flags = mii.flags;
            mi.deviceName = mii.deviceName;
            mi.scale = new double[] {
              (double)nativeSize[mii.deviceName].Right / (double)mi.monitor.Right,
              (double)nativeSize[mii.deviceName].Bottom / (double)mi.monitor.Bottom
            };
            col.Add(mi);
          }          
          return true;
        }, 
        IntPtr.Zero );

      foreach (MonitorInfo info in col) 
      {
          Console.WriteLine($"Device: {info.deviceName}");
          Console.WriteLine();
      }
      return col;
    }
  }
}     