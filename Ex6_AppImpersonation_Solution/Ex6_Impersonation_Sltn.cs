using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Security;
using System.Security.Principal;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;
using System.Web;

using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.PI;


namespace Exercise6
{
    public class AppImpersonation : IDisposable
    {
        private WindowsImpersonationContext _impersonationContext;

        public AppImpersonation()
        {
            WindowsImpersonationContext impersonationContext;

            Console.WriteLine("   Before impersonation: "
                + WindowsIdentity.GetCurrent().Name);

            if (HttpContext.Current != null) //WebApp
            {
                ImpersonateCurrentUser(out impersonationContext);
            }
            else //ConsoleApp
            {
                ImpersonateUser("pischool", "test01", "pass#word01", out impersonationContext);
            }
            _impersonationContext = impersonationContext;

            Console.WriteLine("   After impersonation: "
                + WindowsIdentity.GetCurrent().Name);
        }

        public void Dispose()
        {
            StopImpersonating(ref _impersonationContext);

            Console.WriteLine("   After closing the context: " + WindowsIdentity.GetCurrent().Name);
        }

        #region Impersonation Functions
        //
        public static void ImpersonateCurrentUser(out WindowsImpersonationContext impersonationContext)
        {
            impersonationContext = null;

            if (HttpContext.Current != null)
            {
                WindowsIdentity identity = ((WindowsIdentity)HttpContext.Current.User.Identity);
                impersonationContext = identity.Impersonate();
            }
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public static void ImpersonateUser(string domainName, string userName, string password, out WindowsImpersonationContext context)
        {
            SafeTokenHandle safeTokenHandle;
            IntPtr tokenHandle = IntPtr.Zero;
            try
            {
                const int LOGON32_PROVIDER_DEFAULT = 0;
                //This parameter causes LogonUser to create a primary token.
                const int LOGON32_LOGON_INTERACTIVE = 2; // token not elevated
                //const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8; // Win2K or higher, cannot use with namedpipe

                bool retval = LogonUser(userName, domainName, password,
                    LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                    out safeTokenHandle);

                if (!retval)
                {
                    int ret = Marshal.GetLastWin32Error();
                    throw new System.ComponentModel.Win32Exception(ret);
                }
                using (safeTokenHandle)
                {
                    context = WindowsIdentity.Impersonate(safeTokenHandle.DangerousGetHandle());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }           
            finally
            {
                if (tokenHandle != IntPtr.Zero) CloseHandle(tokenHandle);
            }
        }
        public static void StopImpersonating(ref WindowsImpersonationContext context)
        {
            if (context != null)
            {
                context.Undo();
                context.Dispose();
                context = null;
            }
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            private SafeTokenHandle()
                : base(true)
            {
            }

            [DllImport("kernel32.dll")]
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }
        //
        #endregion
    }
}
