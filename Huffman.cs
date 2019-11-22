using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace Custom_Client_raspberry
{
    static class Huffman1
    {
        static private string _stringa;
        static private List<byte> _simboli;
        static private List<double> _ripetizioni;
        static private double _contLettere;
        static private byte _nCaratteri;
        static private List<Dati> Lista;
        static private strCodifica[] codifiche;
        struct Dati
        {
            public string carattere;
            public double ripetizione;
        }
        public struct strCodifica
        {
            public char s;
            public string c;
        }
        private struct Div
        {
            public strCodifica[] dizionario;
            public string messaggio;
        }

        private static void Init()
        {
            Lista = new List<Dati>();
            _ripetizioni = new List<double>();
            _simboli = new List<byte>();
            _nCaratteri = 0;
            _contLettere = 0;
        }
		
		//Codifica una stringa secondo l'algoritmo di Huffman e ritorna un array di byte 
        public static byte[] Encode(string stringa)
        {
            Init();
            _stringa = stringa;
            elaboraStringa();
            ordinaECodifica();
            return MergeDictionaryAndMessage(CodificaStringa(), EncodeDictionary(codifiche));
        }       
		
		//Conta i caratteri della stringa e quante volte si è ripetuto un carattere
        private static void elaboraStringa()
        {
            //Processamento della stringa
            for (int i = 0; i < _stringa.Length; i++)
            {
                byte carattere = Convert.ToByte(_stringa[i]);
                if (!_simboli.Contains(carattere))
                {
                    _simboli.Add(carattere);
                    _ripetizioni.Add(1);
                    _nCaratteri++;
                }
                else
                {
                    for (int j = 0; j < _simboli.Count; j++)
                        if (_simboli[j] == carattere)
                            _ripetizioni[j]++;
                }
                _contLettere++;
            }
            //Ordinamento dei simboli in ordine di frequenza
            for (int i = 0; i < _ripetizioni.Count - 1; i++)
                for (int j = i + 1; j < _ripetizioni.Count; j++)
                    if (_ripetizioni[i] < _ripetizioni[j])
                    {
                        double temp = _ripetizioni[i];
                        _ripetizioni[i] = _ripetizioni[j];
                        _ripetizioni[j] = temp;
                        byte temp1 = _simboli[i];
                        _simboli[i] = _simboli[j];
                        _simboli[j] = temp1;
                    }
            for (int i = 0; i < _simboli.Count; i++)
            {
                Dati temp2;
                temp2.carattere = null;
                temp2.carattere += (char)_simboli[i];
                temp2.ripetizione = _ripetizioni[i];
                Lista.Add(temp2);
            }
            codifiche = new strCodifica[Lista.Count];
            for (int i = 0; i < codifiche.Length; i++)
            {
                strCodifica temp;
                temp.s = Lista[i].carattere[0];
                temp.c = null;
                codifiche[i] = temp;
            }
        }               
        public static void Entropia()           //Metodo per calcolare l'entropia del file/stringa
        {
            double entropia = 0;
            for (int i = 0; i < _ripetizioni.Count; i++)
            {
                double test = (_ripetizioni[i] / _contLettere);
                entropia += (test * Math.Log((1 / test), 2));
            }
        }
        static private void ordinaECodifica()          //Metodo per ordinare ogni volta la lista e codificare i simboli secondo l'algoritmo di Huffman
        {
            if (Lista.Count > 1)
            {
                for (int i = 0; i < Lista.Count - 1; i++)           //Ordinamento della lista
                    for (int j = i + 1; j < Lista.Count; j++)
                        if (Lista[i].ripetizione < Lista[j].ripetizione)
                        {
                            Dati swap = Lista[i];
                            Lista[i] = Lista[j];
                            Lista[j] = swap;
                        }
                string temp = Lista[Lista.Count - 1].carattere, temp2 = Lista[Lista.Count - 2].carattere;
                CodificaSimboli(temp, temp2);
                Dati temp1;
                temp1.carattere = Lista[Lista.Count - 2].carattere + Lista[Lista.Count - 1].carattere;
                temp1.ripetizione = Lista[Lista.Count - 1].ripetizione + Lista[Lista.Count - 2].ripetizione;
                Lista[Lista.Count - 2] = temp1;
                Lista.RemoveAt(Lista.Count - 1);

                ordinaECodifica();
            }
            else
            {
                for (int i = 0; i < codifiche.Length; i++)          //Aggiungo 1 in modo che gli 0 non vengano "persi nella fase di decodifica"
                    codifiche[i].c += "1";
                for (int i = 0; i < codifiche.Length; i++)          //Riverso tutte le codifiche dei simboli
                {
                    codifiche[i].c = riversaStringa(codifiche[i].c);
                }
                return;
            }
        }
        static private void CodificaSimboli(string temp, string temp2)     //Codifica dei simboli
        {
            for (int i = 0; i < codifiche.Length; i++)
                for (int j = 0; j < temp2.Length; j++)
                    if (codifiche[i].s == temp2[j])
                    {
                        codifiche[i].c += "1";
                    }
            for (int i = 0; i < codifiche.Length; i++)
                for (int j = 0; j < temp.Length; j++)
                    if (codifiche[i].s == temp[j])
                    {
                        codifiche[i].c += "0";
                    }
        }
        static private string riversaStringa(string s)         //Metodo per riversare una stringa
        {
            char[] arrayTemp = s.ToCharArray();
            Array.Reverse(arrayTemp);
            return new string(arrayTemp);
        }
		
		//Ritorna una stringa con la percentuale di compressione della stringa
        public static double GetPerCompressione()
        {
            double somma = 0, somma2 = 0;
            for (int i = 0; i < _ripetizioni.Count; i++)
            {
                somma += _ripetizioni[i] * codifiche[i].c.Length;
                somma2 += _ripetizioni[i] * 8;
            }
            return ((somma2 - somma) / somma2) * 100;
        }           
		
		//Codifica la stringa in un array di byte
        static private byte[] CodificaStringa()
        {
            string encodedString = getEncodedString();
            int diff = encodedString.Length % 8;
            if (diff != 0)
            {
                diff = 8 - diff;
                for (int i = 0; i < diff; i++)
                    encodedString += "0";
                strCodifica[] temp = codifiche;
                codifiche = new strCodifica[temp.Length + 1];
                for (int i = 0; i < codifiche.Length - 1; i++)
                    codifiche[i] = temp[i];
                codifiche[codifiche.Length - 1].s = Convert.ToChar(5);
                codifiche[codifiche.Length - 1].c = diff.ToString();
            }
            byte[] output = new byte[encodedString.Length / 8];
            for (int i = 0; i < output.Length; i++)
            {
                for (int b = 0; b <= 7; b++)
                    output[i] |= (byte)((encodedString[i * 8 + b] == '1' ? 1 : 0) << (7 - b));
            }
            return output;
        }           

		//Converte un byte in una stringa binaria
        static string convertiByteInStringa(byte num)
        {
            string temp = Convert.ToString(num, 2);
            if (temp.Length < 8)
            {
                int lung = temp.Length;
                temp = riversaStringa(temp);
                for (int i = 0; i < 8 - lung; i++)
                    temp += "0";
                temp = riversaStringa(temp);
            }
            return temp;
        }

		//Ritorna la stringa codificata secondo l'algoritmo di Huffman
        public static string getEncodedString()
        {
            string encodedString = null;
            for (int k = 0; k < _stringa.Length; k++)
            {
                for (int i = 0; i < codifiche.Length; i++)
                    if (_stringa[k] == codifiche[i].s)
                        for (int j = 0; j < codifiche[i].c.Length; j++)
                            if (codifiche[i].c[j] == '0')
                                encodedString += '0';
                            else
                                encodedString += '1';
            }
            return encodedString;
        }               
		
		//Ritorna il dizionario
        public static strCodifica[] GetDictionary()
        {
            return codifiche;
        }       

		//Codifica il dizionario in un array di byte
        private static byte[] EncodeDictionary(strCodifica[] dizionario)
        {
            string stringaSer = "";
            for (int i = 0; i < dizionario.Length; i++)
            {
                stringaSer += "s:" + dizionario[i].s + "c:" + dizionario[i].c;
                if (i != dizionario.Length - 1)
                    stringaSer += Convert.ToChar(7);
            }
            return Encoding.ASCII.GetBytes(stringaSer);
        }       
		
		 //Unisce i byte del messaggio e i byte del dizionario in un unico array di byte
        private static byte[] MergeDictionaryAndMessage(byte[] messaggio, byte[] dizionario)
        {
            byte[] totale = new byte[messaggio.Length + dizionario.Length + 3];     //Lascio 2 elementi dell'array = 0 in modo che si capisca la fine del messaggio e l'inizio del dizionario
            for (int i = 0; i < messaggio.Length; i++)
                totale[i] = messaggio[i];
            totale[messaggio.Length] = totale[messaggio.Length + 1] = 0;
            for (int i = messaggio.Length + 2, k = 0; i < totale.Length-1; i++, k++)
                totale[i] = dizionario[k];
            totale[totale.Length - 1] = Convert.ToByte(Convert.ToChar(4));
            return totale;

        }      

		//Decodifica la stringa a partire da un array di byte
        public static string Decode(byte[] payload)
        {
            Div test = DividiMesEDiz(payload);
            string buffer = "";
            string messaggioDecod = "";
            int lunghDiz = test.dizionario.Length;
            if (test.dizionario[test.dizionario.Length - 1].s == Convert.ToChar(5))
            {
                test.messaggio = test.messaggio.Remove(test.messaggio.Length - Convert.ToInt16(test.dizionario[test.dizionario.Length - 1].c), Convert.ToInt16(test.dizionario[test.dizionario.Length - 1].c));
                lunghDiz -= 1;
            }
            for (int i = 0; i < test.messaggio.Length; i++)
            {
                buffer += test.messaggio[i];
                for (int j = 0; j < lunghDiz; j++)
                    if (buffer == test.dizionario[j].c)
                    {
                        messaggioDecod += test.dizionario[j].s;
                        j = test.dizionario.Length;
                        buffer = "";
                    }
            }
            return messaggioDecod;
        }            

		//Metodo per dividere il messaggio e il dizionario
        private static Div DividiMesEDiz(byte[] payload)
        {
            byte[] messaggio;
            byte[] diz;
            long temp = indiceDivisione(payload);
            messaggio = new byte[temp];
            diz = new byte[payload.Length - temp - 2];
            for (int i = 0; i < messaggio.Length; i++)
                messaggio[i] = payload[i];
            for (int i = messaggio.Length + 2, k = 0; k < diz.Length; i++, k++)
                diz[k] = payload[i];
            Div split;
            split.messaggio = PrendiStringa(messaggio);
            split.dizionario = PrendiDizionario(diz);
            return split;
        }       

		//Metodo per convertire un array di byte in una stringa
        private static string PrendiStringa(byte[] messaggio)
        {
            string mes = "";
            for (int i = 0; i < messaggio.Length; i++)
                mes += convertiByteInStringa(messaggio[i]);
            return mes;
        }       
		
		//Metodo per prendere il dizionario a partire da un array di byte
        private static strCodifica[] PrendiDizionario(byte[] diz)
        {
            string dizStringa = Encoding.ASCII.GetString(diz);
            string[] temp = dizStringa.Split(new[] { Convert.ToChar(7) }, StringSplitOptions.RemoveEmptyEntries);
            strCodifica[] Dizionario = new strCodifica[temp.Length];
            for (int i = 0; i < temp.Length; i++)
            {
                string strTemp = temp[i];
                Dizionario[i].s = strTemp[2];
                strTemp = strTemp.Remove(0, 5);
                Dizionario[i].c = strTemp;
            }
            return Dizionario;
        }               
        private static long indiceDivisione(byte[] payload)                      //Da ottimizzare. Ritorna l'indice che indica quando finisce la parte del messaggio vero e proprio
        {
            long inizio = 0, fine = 0, temp = 0;
            for (long i = 0; i < payload.Length; i++)
                if (payload[i] == 0)
                {
                    inizio = i;
                    i = payload.LongLength;
                }
            for (long i = inizio; i < payload.Length; i++)
                if (payload[i] != 0)
                {
                    fine = i;
                    i = payload.Length;
                }
            temp = fine - 2;
            return temp;
        }
    }
}
