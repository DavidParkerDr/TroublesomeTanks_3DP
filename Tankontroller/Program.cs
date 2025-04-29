using System;

namespace Tankontroller
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = (Tankontroller)Tankontroller.Instance())
            {
                game.Run();
                game.GetControllerManager().DisconnectAllControllers();
            }
        }
    }
}
