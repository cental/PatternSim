using System;
using CommandLine;
using CommandLine.Text;
using AuthecoConsole;
        
namespace PatternSim {    
    
    public class Program {        
        static DateTime _startTime;
       
        private static void initialize() {
            _startTime = DateTime.Now;
        }

        static void terminate() {
            DateTime stopTime = DateTime.Now;
            Console.WriteLine("Stopped: {0}", stopTime);
            TimeSpan elapsedTime = stopTime - _startTime;
            Console.WriteLine("Elapsed: {0}", elapsedTime);            
        }

        static void Main(string[] args) {
            initialize();
        	PatternSimRerankCommand cmd = new PatternSimRerankCommand();
			cmd.run(args);
		    terminate();
        }
	}
}