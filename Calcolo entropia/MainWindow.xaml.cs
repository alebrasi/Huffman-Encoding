using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Threading;

namespace Calcolo_entropia
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string percorso;
        double contLettere = 0;
        byte nCaratteri = 0;
        List<byte> simboli = new List<byte>();
        List<double> ripetizioni = new List<double>();
        List<Dati> Lista = new List<Dati>();

        Codifica[] codifiche;

        Thread leggi;
        Thread comprimi;
        Thread decomprimi;

        struct Dati
        {
            public string carattere;
            public double ripetizione;
        }

        struct Codifica
        {
            public char simbolo;
            public string codifica;
        }

        private void btnCaricaFile_Click(object sender, RoutedEventArgs e)
        {
            simboli.Clear();
            ripetizioni.Clear();
            contLettere = 0;
            nCaratteri = 0;
            OpenFileDialog dialog = new OpenFileDialog();
            if ((bool)dialog.ShowDialog())
            {
                percorso = dialog.FileName;
                leggi = new Thread(leggiFile);
                leggi.Start();
            }
        }

        private void leggiFile()
        {
            FileStream fs = new FileStream(percorso, FileMode.Open);
            StreamReader sr = new StreamReader(fs, Encoding.ASCII);
            statoOperazioni("Lettura del file...", true);
            while (!sr.EndOfStream)
            {
                byte carattere = Convert.ToByte(sr.Read());
                if (!simboli.Contains(carattere))
                {
                    simboli.Add(carattere);
                    ripetizioni.Add(1);
                    nCaratteri++;
                }
                else
                {
                    for (int i = 0; i < simboli.Count; i++)
                        if (simboli[i] == carattere)
                            ripetizioni[i]++;
                }
                contLettere++;
            }
            sr.Close();
            Dispatcher.Invoke(() =>
            {
                lstRipetizione.Items.Clear();
            });
            for (int i = 0; i < ripetizioni.Count - 1; i++)
                for (int j = i + 1; j < ripetizioni.Count; j++)
                    if (ripetizioni[i] < ripetizioni[j])
                    {
                        double temp = ripetizioni[i];
                        ripetizioni[i] = ripetizioni[j];
                        ripetizioni[j] = temp;
                        byte temp1 = simboli[i];
                        simboli[i] = simboli[j];
                        simboli[j] = temp1;
                    }
            for (int i = 0; i < simboli.Count; i++)
            {
                Dispatcher.Invoke(() =>
                {
                    lstRipetizione.Items.Add(Convert.ToChar(simboli[i]) + "      (" + simboli[i] + ")" + "       " + ripetizioni[i]);
                });
            }
            statoOperazioni("", false);
            Dispatcher.Invoke(() =>
            {
                lblNCaratteri.Content = "Numero di caratteri totale: " + contLettere + "\n" + "Numero di caratteri diversi " + nCaratteri;
                prgBar.IsIndeterminate = false;
                prgBar.Value = prgBar.Maximum;
            });
            leggi.Abort();
        }

        private void btnEntropia_Click(object sender, RoutedEventArgs e)
        {
            double entropia = 0;
            for (int i = 0; i < ripetizioni.Count; i++)
            {
                double test = (ripetizioni[i] / contLettere);
                entropia += (test * Math.Log((1 / test), 2));
            }
            lblEntropia.Content = string.Format("L'entropia è: {0:0.00}", entropia);
        }

        private void btnHuffman_Click(object sender, RoutedEventArgs e)
        {
            lstCodiciHuffman.Items.Clear();
            statoOperazioni("Creazione del vettore...", true);
            for (int i = 0; i < simboli.Count; i++)
            {
                Dati temp2;
                temp2.carattere = null;
                temp2.carattere += (char)simboli[i];
                temp2.ripetizione = ripetizioni[i];
                Lista.Add(temp2);
            }
            codifiche = new Codifica[Lista.Count];
            for (int i = 0; i < codifiche.Length; i++)
            {
                Codifica temp;
                temp.simbolo = Lista[i].carattere[0];
                temp.codifica = null;
                codifiche[i] = temp;
            }
            statoOperazioni("Codificazio dei caratteri...", true);
            Huffman();
        }

        private void Huffman()
        {
            if (Lista.Count > 1)
            {
                for (int i = 0; i < Lista.Count - 1; i++)
                    for (int j = i + 1; j < Lista.Count; j++)
                        if (Lista[i].ripetizione < Lista[j].ripetizione)
                        {
                            Dati swap = Lista[i];
                            Lista[i] = Lista[j];
                            Lista[j] = swap;
                        }
                string temp = Lista[Lista.Count - 1].carattere, temp2 = Lista[Lista.Count - 2].carattere;
                CodificaCaratteri(temp, temp2);
                Dati temp1;
                temp1.carattere = Lista[Lista.Count - 2].carattere + Lista[Lista.Count - 1].carattere;
                temp1.ripetizione = Lista[Lista.Count - 1].ripetizione + Lista[Lista.Count - 2].ripetizione;
                Lista[Lista.Count - 2] = temp1;
                Lista.RemoveAt(Lista.Count - 1);

                Huffman();
            }
            else
            {
                statoOperazioni("", false);
                MessageBox.Show("Codifica eseguita!");
                for (int i = 0; i < codifiche.Length; i++)
                {
                    codifiche[i].codifica = riversaStringa(codifiche[i].codifica);
                    lstCodiciHuffman.Items.Add(codifiche[i].simbolo + "  =   " + codifiche[i].codifica);
                }
                return;
            }
        }

        private string riversaStringa(string s)
        {
            char[] arrayTemp = s.ToCharArray();
            Array.Reverse(arrayTemp);
            return new string(arrayTemp);
        }

        private void CodificaCaratteri(string temp, string temp2)
        {
            for (int i = 0; i < codifiche.Length; i++)
                for (int j = 0; j < temp2.Length; j++)
                    if (codifiche[i].simbolo == temp2[j])
                    {
                        codifiche[i].codifica += "1";
                    }
            for (int i = 0; i < codifiche.Length; i++)
                for (int j = 0; j < temp.Length; j++)
                    if (codifiche[i].simbolo == temp[j])
                    {
                        codifiche[i].codifica += "0";
                    }
        }

        private void btnCodificaDocumento_Click(object sender, RoutedEventArgs e)
        {
            comprimi = new Thread(CodificaDocumento);
            comprimi.Start();
        }

        private void CodificaDocumento()
        {
            StreamReader sr = new StreamReader(percorso, Encoding.ASCII);
            BinaryWriter bw = new BinaryWriter(File.Open("Codifica.dat", FileMode.Create));
            statoOperazioni("Codifica del file...", true);
            while (!sr.EndOfStream)
            {
                char car = (char)sr.Read();
                for (int i = 0; i < codifiche.Length; i++)
                    if (car == codifiche[i].simbolo)
                        for (int j = 0; j < codifiche[i].codifica.Length; j++)
                            if (codifiche[i].codifica[j] == '0')
                                bw.Write(false);
                            else
                                bw.Write(true);
            }
            sr.Close();
            bw.Close();
            statoOperazioni("Creazione del file di decodifica...", true);
            StreamWriter sw = new StreamWriter("Codifica.decode");
            for (int i = 0; i < codifiche.Length; i++)
            {
                string codificaCar = "";
                for (int j = 0; j < codifiche[i].codifica.Length; j++)
                {
                    codificaCar += codifiche[i].codifica[j];
                }
                sw.WriteLine(Convert.ToInt32(codifiche[i].simbolo) + "_" + codificaCar);
            }
            sw.Close();
            statoOperazioni("", false);
            MessageBox.Show("Codifica effettuata!");
        }

        private void btnDecodificaDocumento_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog1 = new OpenFileDialog();
            dialog1.Filter = "File codifica Huffman (*.dat; *.decode)|*.dat;*.decode";
            dialog1.Multiselect = true;
            if ((bool)dialog1.ShowDialog())
            {
                string[] percorso = dialog1.FileNames;
                decomprimi = new Thread(() => DecodificaDocumento(percorso));
                decomprimi.Start();
            }
        }

        private void DecodificaDocumento(string[] percorsi)
        {
            StreamWriter sw = new StreamWriter("File decodificato.txt");
            if (!percorsi[0].Contains(".dat"))
            {
                string temp = percorsi[0];
                percorsi[0] = percorsi[1];
                percorsi[1] = temp;
            }
            StreamReader sr = new StreamReader(percorsi[1]);
            List<Codifica> listaDecodifica = new List<Codifica>();
            statoOperazioni("Importazione lista di decodifica...", true);
            while (!sr.EndOfStream)
            {
                string[] riga = sr.ReadLine().Split('_');
                Codifica temp;
                temp.simbolo = (char)Convert.ToInt32(riga[0]);
                temp.codifica = riga[1];
                listaDecodifica.Add(temp);
            }
            sr.Close();
            FileStream fs = new FileStream(percorsi[0], FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            string buffer = "";
            long cont = 0;
            statoOperazioni("Decodifica in corso...", true);
            while (cont < fs.Length)
            {
                buffer += (br.ReadBoolean() == true) ? "1" : "0";
                for (int i = 0; i < listaDecodifica.Count; i++)
                    if (listaDecodifica[i].codifica == buffer)
                    {
                        sw.Write(listaDecodifica[i].simbolo);
                        buffer = "";
                    }
                cont++;
            }
            sr.Close();
            sw.Close();
            br.Close();
            statoOperazioni("", false);
            MessageBox.Show("Decodifica completata");
        }

        private void statoOperazioni(string contenuto, bool inizioFine)     //inizio = true, fine = false
        {
            if (inizioFine)
                Dispatcher.Invoke(() =>
                {
                    prgBar.IsIndeterminate = true;
                    lblStatoOp.Content = contenuto;
                    Thread.Sleep(250);
                });
            else
                Dispatcher.Invoke(() =>
                {
                    prgBar.IsIndeterminate = false;
                    prgBar.Value = prgBar.Maximum;
                    lblStatoOp.Content = "Operazione completata!";
                });
        }
    }
}