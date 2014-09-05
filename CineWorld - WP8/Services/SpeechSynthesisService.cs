using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.Speech.Synthesis;

namespace Cineworld.Services
{
    public class SpeechSynthesisService
    {
        static SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();

        public static async Task SpeakOutLoud(string data)
        {
            try
            {
                speechSynthesizer.CancelAll();
                await speechSynthesizer.SpeakTextAsync(data);
            }
            catch { }
            
        }

        public static void CancelExistingRequests()
        {
            try
            {
                speechSynthesizer.CancelAll();
            }
            catch { }
        }
    }
}
