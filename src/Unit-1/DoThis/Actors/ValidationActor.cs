using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTail.Actors
{
    class ValidationActor : UntypedActor
    {
        private IActorRef _consoleWriterActor;

        public ValidationActor(IActorRef consoleWriterActor)
        {
            this._consoleWriterActor = consoleWriterActor;
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
                _consoleWriterActor.Tell(new Messages.NullInputError("No Input Received"));
            }
            else
            {
                var valid = IsValid(msg);
                if (valid)
                {
                    _consoleWriterActor.Tell(new Messages.InputSuccess("Valid: even number of characters!"));
                }

                else
                {
                    _consoleWriterActor.Tell(new Messages.ValidationError("Invalid: input had odd number of characters!"));
                }
            }
            // tell the sender to continue doing its thing
            Sender.Tell(new Messages.ContinueProcessing());
        }
        private bool IsValid(string message)
        {
            var valid = message.Length % 2 == 0;

            return valid;

        }
    }
}
