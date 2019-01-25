using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTail.Actors
{
    //parent actor for tailactor, supervises it.
    class TailCoordinatorActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            if (message is StartTail)
            {
                var msg = message as StartTail;

                //Props tailActorProps = Props.Create<TailActor>(msg.ReporterActor, msg.FilePath);
                //Context.ActorOf(tailActorProps);

                Context.ActorOf(Props.Create(() => new TailActor(msg.ReporterActor, msg.FilePath)));
            }
        }

        //Supervisor Strategy 
        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                       10, //number of retries
                       TimeSpan.FromSeconds(30), //within time range 
                       x =>
                       {
                           //Maybe we consider ArithmeticException to not be application critical
                           //so we just ignore the error and keep going.
                           if (x is ArithmeticException) return Directive.Resume;

                           //Error that we cannot recover from, stop the failing actor
                           else if (x is NotSupportedException) return Directive.Stop;

                           else return Directive.Restart;

                       });
        }

        public class StartTail
        {
            public StartTail(string filePath, IActorRef reporterActor)
            {
                FilePath = filePath;
                ReporterActor = reporterActor;
            }
            public string FilePath { get; private set; }
            public IActorRef ReporterActor { get; private set; }
        }

        public class StopTail
        {
            public StopTail(string filepath)
            {
                Filepath = filepath;
            }

            public string Filepath { get; private set; }
        }

    }
}
