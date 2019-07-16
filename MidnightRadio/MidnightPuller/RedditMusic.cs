using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace MidnightPuller
{
    public static class RedditMusic
    {
        [FunctionName("RedditMusic")]
        public static void Run([TimerTrigger("0 30 9 * * *")]TimerInfo myTimer, TraceWriter log)
        {
           string superSecret = System.Environment.GetEnvironmentVariable("sqlsecret");
            
        }
    }
}
