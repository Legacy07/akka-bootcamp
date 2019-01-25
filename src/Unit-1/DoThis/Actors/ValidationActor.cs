using Akka.Actor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTail.Actors
{
    class ValidationActor : UntypedActor
    {
        private IActorRef _consoleWriterActor;
        private IActorRef _tailsCoordinatorActor;

        public ValidationActor(IActorRef consoleWriterActor, IActorRef tailscoordinatorActor)
        {
            this._consoleWriterActor = consoleWriterActor;
            this._tailsCoordinatorActor = tailscoordinatorActor;

        }

        protected override void OnReceive(object message)
        {
            ValidateInput(message);
        }

        // Reads input from console which is sent from the reader actor, validates it, then signals appropriate response
        private void ValidateInput(object message)
        {
            var msg = message as string;
            if (message.Equals(ConsoleReaderActor.StartCommand))
            {
                _consoleWriterActor.Tell(ConsoleReaderActor.StartCommand);
            }
            else if (String.Equals(msg, ConsoleReaderActor.ExitCommand, StringComparison.OrdinalIgnoreCase))
            {
                Context.System.Terminate();
            }
            else if (string.IsNullOrEmpty(msg))
            {
                // signal that the user needs to supply an input, as previously
                // received input was blank
                _consoleWriterActor.Tell(new Messages.NullInputError("Enter something."));
                // tell the sender to continue doing its thing
                Sender.Tell(new Messages.ContinueProcessing());
            }
            else
            {
                var valid = IsFileExists(msg);

                if (valid)
                {
                    // singal input if successfull 
                    _consoleWriterActor.Tell(new Messages.InputSuccess(string.Format("Starting processing for {0}", msg)));
                    // start coordinator
                    _tailsCoordinatorActor.Tell(new TailCoordinatorActor.StartTail(msg, _consoleWriterActor));
                }

                else
                {
                    _consoleWriterActor.Tell(new Messages.ValidationError(string.Format("{0} cannot be found!", msg)));
                    // tell the sender to continue doing its thing
                    Sender.Tell(new Messages.ContinueProcessing());
                }
            }

        }
        private bool IsFileExists(string path)
        {
            return File.Exists(path);
        }
    }
}
