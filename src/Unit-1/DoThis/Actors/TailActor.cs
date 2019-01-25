using System.IO;
using System.Text;
using Akka.Actor;

namespace WinTail.Actors
{
    class TailActor : UntypedActor
    {
        private readonly string _filePath;
        private readonly IActorRef _reporterActor;
        private FileObserver _observer;
        private Stream _fileStream;
        private StreamReader _fileStreamReader;

        public TailActor(IActorRef reporterActor, string filePath)
        {
            _reporterActor = reporterActor;
            _filePath = filePath;

        }

        protected override void OnReceive(object message)
        {
            // move file cursor forward
            // pull results from cursor to end of file and write to output
            // (this is assuming a log file type format that is append-only)
            if (message is FileWrite)
            {
                var text = _fileStreamReader.ReadToEnd();

                if (!string.IsNullOrEmpty(text))
                {
                    _reporterActor.Tell(text);
                }
            }

            else if (message is FileError)
            {
                var fileError = message as FileError;
                _reporterActor.Tell(string.Format("Tail error: {0}", fileError.Reason));
            }

            else if (message is InitialRead)
            {
                var initialRead = message as InitialRead;
                _reporterActor.Tell(initialRead.Text);
            }
        }

        //before the actor begins receiving messages, initialise these objects first. 
        protected override void PreStart()
        {
            //start watching files for changes
            _observer = new FileObserver(Self, Path.GetFullPath(_filePath));
            _observer.Start();

            // open the file stream with shared read/write permissions
            // (so file can be written to while open)
            _fileStream = new FileStream(Path.GetFullPath(_filePath),
                                         FileMode.Open, FileAccess.Read,
                                         FileShare.ReadWrite);
            _fileStreamReader = new StreamReader(_fileStream, Encoding.UTF8);

            // read the initial contents of the file and send it to console as first msg
            var text = _fileStreamReader.ReadToEnd();
            Self.Tell(new InitialRead(_filePath, text));
        }

        //do cleanup after the actor is not receiving messages anymore
        protected override void PostStop()
        {
            _observer.Dispose();
            _observer = null;
            _fileStreamReader.Close();
            _fileStreamReader.Dispose();
            base.PostStop();
        }

        #region Messages

        public class FileWrite
        {
            public FileWrite(string fileName)
            {
                FileName = fileName;
            }

            public string FileName { get; private set; }
        }

        /// <summary>
        /// Signal that the OS had an error accessing the file.
        /// </summary>
        public class FileError
        {
            public FileError(string fileName, string reason)
            {
                FileName = fileName;
                Reason = reason;
            }

            public string FileName { get; private set; }

            public string Reason { get; private set; }
        }

        /// <summary>
        /// Signal to read the initial contents of the file at actor startup.
        /// </summary>
        public class InitialRead
        {
            public InitialRead(string fileName, string text)
            {
                FileName = fileName;
                Text = text;
            }

            public string FileName { get; private set; }
            public string Text { get; private set; }
        }
        #endregion 
    }
}
