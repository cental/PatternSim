using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace AuthecoConsole {

    /// <summary>
    /// Represents a command line command of the Autheco
    /// </summary>
    public abstract class Command2 {
        public Command2() {
            init();
        }

        public string desc = "";
        public string name = "";

        [HelpOption(HelpText = "Display this help screen.")]
        public string GetUsage() {
            var help = new HelpText(desc);
            help.AdditionalNewLineAfterOption = true;
            help.AddOptions(this);
            return help;
        }

        public string description {
            get {
                return String.Format("[{0}]\n{1}\n\n", name, desc);
            }
        }

        protected void check(string[] args, Command2 options) {
            ICommandLineParser parser = new CommandLineParser(new CommandLineParserSettings(Console.Error));
            if (!parser.ParseArguments(args, options)) Environment.Exit(1);
        }

        protected abstract void run();
        
        public void run(string[] args) {
            check(args, this);
            run();
        }

        protected abstract void init();
    }
}
