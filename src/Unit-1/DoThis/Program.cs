using System;
using Akka.Actor;
using WinTail.Actors;

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
            Props consoleWriterProps = Props.Create<ConsoleWriterActor>();
            IActorRef consoleWriterActor = mainActorSystem.ActorOf(consoleWriterProps);

            Props tailCoordinatorProps = Props.Create<TailCoordinatorActor>();
            IActorRef tailCoordinatorActor = mainActorSystem.ActorOf(tailCoordinatorProps);

            Props validationActorProps = Props.Create<ValidationActor>(consoleWriterActor, tailCoordinatorActor);
            IActorRef validationActor = mainActorSystem.ActorOf(validationActorProps);

            Props consoleReaderProps = Props.Create<ConsoleReaderActor>(validationActor);
            IActorRef consoleReaderActor = mainActorSystem.ActorOf(consoleReaderProps);

            // tell console reader to begin
            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            // blocks the main thread from exiting until the actor system is shut down
            mainActorSystem.WhenTerminated.Wait();
        }
    }
    #endregion
}
