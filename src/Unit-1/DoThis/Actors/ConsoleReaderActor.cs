using System;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Actor responsible for reading FROM the console. 
    /// Also responsible for calling <see cref="ActorSystem.Terminate"/>.
    /// </summary>
    class ConsoleReaderActor : UntypedActor
    {
        public const string ExitCommand = "exit";
        private IActorRef _validationActor;
        public const string StartCommand = "start";

        public ConsoleReaderActor(IActorRef validationActor)
        {
            _validationActor = validationActor;
        }

        protected override void OnReceive(object message)
        {
            SendMessage(message);
        }

        //read the message from the console
        private string ReadInput()
        {
            return Console.ReadLine();
        }
        // send the message to validation actor
        private void SendMessage(object message)
        {
            if (message is StartCommand)
            {
                _validationActor.Tell(StartCommand);
            }
            else
            {
                _validationActor.Tell(ReadInput());
            }
            //this will keep doing its thing, in this case it will be re-reading the console. 
            Self.Tell(new Messages.ContinueProcessing());

        }

    }
}