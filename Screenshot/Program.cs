using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing.Imaging;

class Program
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int VK_F1 = 0x70;

    private static IntPtr _hookID = IntPtr.Zero;
    private static LowLevelKeyboardProc _proc;

    static void Main()
    {
        BindF1(() => TakeScreenshot());
        RunMessageLoop(); 
    }

    
    //=========================================================================
    //=========================================================================
    static void RunMessageLoop()
    {
        
        MSG msg;
        while (GetMessage(out msg, IntPtr.Zero, 0, 0))
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }
    }
    //=========================================================================
    //=========================================================================

    static void TakeScreenshot()
    {
        
        string screenshotsFolder = @"C:\Users\User\Desktop\CheckCode";
        if (!System.IO.Directory.Exists(screenshotsFolder))
            System.IO.Directory.CreateDirectory(screenshotsFolder);

        string fileName = System.IO.Path.Combine(
            screenshotsFolder,
            $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png"
        );

        int width = GetSystemMetrics(0);
        int height = GetSystemMetrics(1);

        IntPtr hdcScreen = GetDC(IntPtr.Zero);                      
        IntPtr hdcMem = CreateCompatibleDC(hdcScreen);               
        IntPtr hBitmap = CreateCompatibleBitmap(hdcScreen, width, height); 
        IntPtr oldBitmap = SelectObject(hdcMem, hBitmap);             

        
        BitBlt(hdcMem, 0, 0, width, height, hdcScreen, 0, 0, TernaryRasterOperations.SRCCOPY);

       
        using (Bitmap bmp = Image.FromHbitmap(hBitmap))
        {
            bmp.Save(fileName, ImageFormat.Png);
        }

        
        SelectObject(hdcMem, oldBitmap);
        DeleteObject(hBitmap);
        DeleteDC(hdcMem);
        ReleaseDC(IntPtr.Zero, hdcScreen);

        Console.WriteLine($"Скриншот сохранён в: {fileName}");
    }



    static void BindF1(Action action)
    {
        _proc = (nCode, wParam, lParam) =>
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode == VK_F1)
                    action();
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        };

        _hookID = SetHook(_proc);
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll")]
    static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

    [DllImport("gdi32.dll")]
    static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
        IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);
    
    [DllImport("gdi32.dll")]
    static extern bool DeleteObject(IntPtr hObject);
    
    [DllImport("gdi32.dll")]
    static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

    enum TernaryRasterOperations : uint
    {
        SRCCOPY = 0x00CC0020
    }


    //=========================================================================
    //=========================================================================
    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT { public int x; public int y; }

    [DllImport("user32.dll")]
    public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    public static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    public static extern IntPtr DispatchMessage(ref MSG lpMsg);
    
    //=========================================================================
    //=========================================================================
}
