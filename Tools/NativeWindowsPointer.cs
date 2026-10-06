using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
public static class ZtmWindowsPointer {
 public delegate bool EnumCallback(IntPtr h,IntPtr p);
 [StructLayout(LayoutKind.Sequential)] public struct POINT{public int x,y;}
 [StructLayout(LayoutKind.Sequential)] public struct RECT{public int left,top,right,bottom;}
 [StructLayout(LayoutKind.Sequential)] public struct MOUSEINPUT{public int dx,dy;public uint mouseData,dwFlags,time;public UIntPtr dwExtraInfo;}
 [StructLayout(LayoutKind.Sequential)] public struct INPUT{public uint type;public MOUSEINPUT mouse;}
 [DllImport("user32.dll")] public static extern bool EnumWindows(EnumCallback c,IntPtr p);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h,out uint p);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern int GetClassName(IntPtr h,StringBuilder s,int n);
 [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h,int c);
 [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
 [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr h);
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
 [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int w,int t,uint flags);
 [DllImport("user32.dll")] public static extern bool GetClientRect(IntPtr h,out RECT r);
 [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr h,ref POINT p);
 [DllImport("user32.dll")] public static extern bool SetCursorPos(int x,int y);
 [DllImport("user32.dll")] public static extern bool GetCursorPos(out POINT p);
 [DllImport("user32.dll",SetLastError=true)] public static extern uint SendInput(uint n,INPUT[] i,int size);
 [DllImport("user32.dll")] public static extern IntPtr GetThreadDesktop(uint id);
 [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
 [DllImport("user32.dll",SetLastError=true)] public static extern IntPtr OpenInputDesktop(uint flags,bool inherit,uint desired);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] public static extern bool GetUserObjectInformation(IntPtr obj,int index,StringBuilder s,int n,out uint need);
 [DllImport("user32.dll")] public static extern bool CloseDesktop(IntPtr obj);
 public static string DesktopName(IntPtr obj){uint need;var b=new StringBuilder(512);return GetUserObjectInformation(obj,2,b,1024,out need)?b.ToString():"error:"+Marshal.GetLastWin32Error();}
 public static IntPtr Find(int target){IntPtr found=IntPtr.Zero;EnumWindows((h,p)=>{uint id;GetWindowThreadProcessId(h,out id);if(id==target){var c=new StringBuilder(256);GetClassName(h,c,256);if(c.ToString()=="UnityWndClass")found=h;}return true;},IntPtr.Zero);return found;}
 public static uint Button(bool down){var i=new INPUT{type=0,mouse=new MOUSEINPUT{dwFlags=down?2u:4u,dwExtraInfo=new UIntPtr(0x5A544D)}};return SendInput(1,new[]{i},Marshal.SizeOf(typeof(INPUT)));}
 public static POINT ClientPoint(IntPtr h,int x,int y){var p=new POINT{x=x,y=y};ClientToScreen(h,ref p);return p;}
 public static void Capture(IntPtr h,string path){RECT r;GetClientRect(h,out r);var p=ClientPoint(h,0,0);using(var b=new Bitmap(r.right,r.bottom)){using(var g=Graphics.FromImage(b))g.CopyFromScreen(p.x,p.y,0,0,b.Size);b.Save(path,ImageFormat.Png);}}
}