using System;
using Akka.Actor;

namespace WinTail
{
    #region Program
    class Program
    {
        public static ActorSystem mainActorSystem;
       
        static void Main(string[] args)
        {
            // initialize MyActorSystem
            mainActorSystem = ActorSystem.Create("MainActorSystem");

            // time to make your first actors!
            IActorRef consoleWriterActor = mainActorSystem.ActorOf(Props.Create(() => new ConsoleWriterActor()));
            IActorRef consoleReaderActor =
                mainActorSystem.ActorOf(Props.Create(() => new ConsoleReaderActor(consoleWriterActor)));

            // tell console reader to begin
            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);
            
            // blocks the main thread from exiting until the actor system is shut down
            mainActorSystem.WhenTerminated.Wait();
        }
    }
    #endregion
}
