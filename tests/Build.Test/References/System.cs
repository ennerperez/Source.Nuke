using System.Linq;
using System.Reflection;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;

namespace System
{
    public static class ExceptionsExtension
    {
        public static string GetMessage(this Exception ex)
        {
            if (ex.GetType().Name == "ProcessException")
            {
                var process = (Process2)ex.GetType()
                    .GetProperty("Process",
                        BindingFlags.Instance |
                        BindingFlags.NonPublic |
                        BindingFlags.Public)?
                    .GetValue(ex);
                return process?.Output.Select(x => x.Text).JoinNewLine();
            }

            return ex.Message;
        }
    }
}
