using System;
using HandlerTests.Bully;

namespace HandlerTests
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
			var t = new BullyTest();
			t.Setup();
			t.WhenCallerIsHighestIdCallerGetsUpdateTest();
			//using (var game = new Game1())
            //    game.Run();
        }
    }
}
