using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace Client_Side
{
    public partial class Form1 : Form
    {
        TcpClient _client;
        byte[] _buffer = new byte[4096];
        public Form1()
        {

            InitializeComponent();

            IPAddress[] localIP = Dns.GetHostAddresses(Dns.GetHostName()); ///local ip alır ve iptextbox'a yazar
            foreach (IPAddress adress in localIP)
            {
                if (adress.AddressFamily == AddressFamily.InterNetwork)
                {
                    YourIPtextBox.Text = adress.ToString();
                }
            }
        }
        private void Server_MessageReceived(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                ///Mesajı alma kısmı
                ///Bu kısımda mesajların hangi thread de gelip gittiğini bilmiyoruz(IAsyncResult) aslında ne zaman hangi dataya ulaştığımız önemli
                var bytesIn = _client.GetStream().EndRead(ar); ///veriyi al okumayı bitir.
                if (bytesIn > 0)
                {
                    ///bu server için gelen datadan bir string oluşturur
                    /// datamız basit bir string ,ama başka çeşit de olabilir.(mesela Binary şeklinde)

                    var tmp = new byte[bytesIn];               ///Dizimizin bir kopyasını oluştup başlama zamanlarına 0 atadık.
                    Array.Copy(_buffer, 0, tmp, 0, bytesIn);
                    var str = Encoding.ASCII.GetString(tmp); ///SERVER'DAN alacağımız her şey Ascii şeklinde olacak böylece farklı dilde yazılmış serverlar ile de çalışabilir bu program.

                    /// Kopyaları Delegate olarak atamamız lazım ,kullanıcı arayüzünde gerçekleştirdiğmiz işlemler ana thread içinde olmalı.
                    ///ana thread içinde olmadığımızdan BeginInvoke metodu sayesinde ana thread içine delegate atarız.
                    BeginInvoke((Action)(() =>
                    {
                        listBox1.Items.Add(str);
                        listBox1.SelectedIndex = listBox1.Items.Count - 1; ///listbox a her eklenen itemden sonra aşağı doğru hareket etsin ve yeni itemleri göstersin
                    }));
                }
                // Buffer temizle tekrar dinle
                Array.Clear(_buffer, 0, _buffer.Length);
                _client.GetStream().BeginRead(_buffer,
                                                0,
                                                _buffer.Length,
                                                Server_MessageReceived,
                                                null);
            }

        }

   

        private void label2_Click(object sender, EventArgs e)
        {

        }
    

        private void ConnectButton_Click_1(object sender, EventArgs e) ///textboxa yazılan ip oku girilen portu al servera bağlan
        {

            _client = new TcpClient();
            IPEndPoint IpEnd = new IPEndPoint(IPAddress.Parse(ConnectIPtextBox.Text), 80);
            _client.Connect(IpEnd);

            _client.GetStream().BeginRead(_buffer,
                                           0,
                                           _buffer.Length,
                                           Server_MessageReceived,
                                           null);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var msg = Encoding.ASCII.GetBytes(textBox1.Text);  ///mesajı ascii ye ceviriyoruz Getstream metodu ile ağda data alış-verişi yapıyoruz.
            _client.GetStream().Write(msg, 0, msg.Length);

            textBox1.Text = "";   ///SEND 'e basınca textbox daki mesajı temizle ve odağı textbox a al
            textBox1.Focus();
        }
    }
}
