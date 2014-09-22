using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace CineworldWebJob
{
    class Program
    {
        static void Main(string[] args)
        {
            //JobHost host = new JobHost();
            //host.RunAndBlock();

            try
            {
                GenerateData();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Processing complete");

            //JobHost jost = new JobHost();
            //jost.Stop();
        }

        public static void GenerateData()
        {
            CineworldWorker worker = new CineworldWorker();
            worker.Run();
        }

    }
}
