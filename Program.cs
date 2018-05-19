using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;


//Program_Dezimal_nach_ulong_Array_P_For_Vorselektion.cs
namespace Primzahlen
{
    //++++  Parallel.For  ++++++++++++++++++++++++++++++++++++++++++++++++++
    //Wird schnellstes Programm für mehr als 20 Stellen+++++++++++++++++++++
    class CPrimzahlen
    {
        //Reine Info:
        //27 Stellen :  100000000000000000000000000
        //25 Stellen :  1000000000000000000000000
        //21 Stellen :  100000000000000000000
        //20 Stellen:   10000000000000000000
        //17 Stellen:   10000000000000000
        //16 Stellen:   1000000000000000
        //Max ulong =  18446744073709551615;
        //Max decimal = 79228162514264337593543950335M;

        //Field
        static Stopwatch s = new Stopwatch();
        static int P_Nr = 0;


        static void Main()
        {
            Decimal anfang = 0;
            Decimal ende = 0;

            Console.Write("\n   Program_Dezimal_nach_ulong_Array_P_For_Vorselektion\n\n");
            Console.Write("\n\n   Primzahlenauflisten!\n\n");
            Console.Write("\n   Untere Grenze Eingeben? ");
            anfang = Convert.ToDecimal(Console.ReadLine());

            Console.Write("   Obere  Grenze Eingeben? ");
            ende = Convert.ToDecimal(Console.ReadLine());
            Console.WriteLine();//Leerzeile

            SuchePrimzahlen(anfang, ende);
        }

        public static void SuchePrimzahlen(Decimal anfang, Decimal ende)
        {
            label1:

            //Aeussere Schleife Ersetzt Hand Eingabe.
            for (; anfang <= ende; anfang++)
            {
                s.Start();
                //Orientierung wo gerade ist?
                //Console.WriteLine("Bin hier am Rechnen: {0:#,#}", anfang);


                //Kein Flaschenhals!! :O
                long Wurzel_anfang = (long)Math.Pow(Convert.ToDouble(anfang), 0.5);


                // Exklusiv zu Inklusiv
                // Ohne das funktioniert nicht richtig!!!! :((((
                ++Wurzel_anfang;


                // Hier wird bei Bedarf in ulong Stücke aufgeteilt
                ulong[] anfang2 = Zerteilen(anfang);


                //********Abbruch der Parallel.For***********************************
                CancellationTokenSource cts = new CancellationTokenSource();

                // Use ParallelOptions instance to store the CancellationToken
                ParallelOptions po = new ParallelOptions();
                po.CancellationToken = cts.Token;
                po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
                //*******************************************************************

                try
                {
                    // Neue Primzahlen Engine! :)
                    Parallel.For(2L, Wurzel_anfang, po, (teiler) =>
                    {
                        //2 Einzig Gerade Primzahl darum (... || teiler == 2) gibt true bei 2!!!!
                        //Das ist am Schnellsten nur gerade Zahlen weglassen
                        if (teiler % 2 != 0 || teiler == 2)
                        {

                            //Wenn (anfang mod i == 0) dann ist keine Primzahl
                            //Beachte Typ Castin von long zu (ulong)!!!!!!!!!!!! 
                            if (Prüfung(anfang2, (ulong)teiler))
                            {
                                cts.Cancel();
                            }
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                    ++anfang;
                    goto label1;
                }
                finally
                {
                    cts.Dispose();
                }


                //1 und 0 rausputzen! ;)
                if (anfang == 1 || anfang == 0)
                    continue;

                s.Stop();
                TimeSpan timeSpan = s.Elapsed;

                ++P_Nr;
                //Ausgabe
                string ausgString = String.Format("\nPrimzahl {0} :)  {1:#,#}\n", P_Nr, anfang);
                ausgString += String.Format("Time: {0}h {1}m {2}s {3}ms\n", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);

                Console.WriteLine(ausgString);
                //MailSenden(ausgString);
            }

            Console.WriteLine("Fertig! :)");
            Console.WriteLine("\n\tCopyright © Nicolas Sauter");

            //Programm wird beendet nach Tastendruck:
            Console.ReadKey();
        }

        //Kann Modulo auf den zusammengesetzten Wert 
        //der Array Werte berechnen und wenn 0 dann return true
        static bool Prüfung(ulong[] wert, ulong teiler)
        {
            ulong zwischenRest = 0;

            for (int i = 0; i < wert.Length; i++)
            {
                zwischenRest = (zwischenRest + wert[i]) % teiler;
            }
            return (zwischenRest == 0);
        }

        //Macht aus decimal ein ulong Array um mit ulong zu Rechnen 
        //z.B. im Primzahlenprogramm(weil viel Schneller!!!)
        static ulong[] Zerteilen(decimal wert)
        {
            if (wert <= ulong.MaxValue)
            {
                //Array mit einem Wert
                return new ulong[] { (ulong)wert };
            }
            else
            {
                decimal i = 2;
                ulong rest;
                decimal teilmenge = 0;

                //Anzahl des Teiler bestimmen bis im ulong bereich nach Teilung von wert
                for (; i < 1000000000; i++)
                {
                    teilmenge = wert / i;
                    if (teilmenge <= ulong.MaxValue)
                    {
                        break;
                    }
                }
                //Rest
                rest = (ulong)(wert % i);

                ulong[] arr_erg_sum = new ulong[(int)i];

                //Initialisieren Array
                for (int y = 0; y < i; y++)
                {
                    arr_erg_sum[y] = (ulong)teilmenge;
                }
                //Rest am letzten Eintrag dazuzählen
                arr_erg_sum[(arr_erg_sum.Length - 1)] += rest;

                return arr_erg_sum;
            }
        }

        static void MailSenden(string message)
        {
            SmtpClient client = new SmtpClient("<SMTP-Server>", 587);
            try
            {
                client.Credentials = new NetworkCredential("<Benutzername>", "<Passwort>");
                client.Send("<MaîlAdresse>", "<MaîlAdresse>", "Betreff: Primzahl", message);
                //Console.WriteLine("Mail Gesendet! :)))");
            }
            catch (Exception)
            {
                //Console.Write(e.Message);
            }
            finally
            {
                client.Dispose();
                //Console.ReadKey();
            }
        }
    }
}
