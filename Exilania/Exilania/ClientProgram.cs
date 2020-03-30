using System;

namespace Exilania
{
#if WINDOWS || XBOX
    static class ClientProgram
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Exilania game = new Exilania())
            {
                game.Run();
            }
        }
    }
#endif
}

