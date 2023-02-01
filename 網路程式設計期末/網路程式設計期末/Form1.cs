using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;//匯入網路通訊協定相關函數
using System.Net.Sockets;//匯入網路插座功能函數
using System.Threading;//匯入多執行緒功能函數

namespace GuessNumber
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        UdpClient U;//宣告UDP通訊物件
        Thread Th;//宣告監聽用執行緒
        //啟動監聽按鍵程序
        private void Button1_Click(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false; //忽略跨執行緒操作的錯誤
            Th = new Thread(Listen); //建立監聽執行緒，目標副程序→Listen
            Th.Start(); //啟動監聽執行緒
            Button1.Enabled = false; //按鍵失效，不能(也不需要)重複開啟監聽
        }
        //監聽副程序
        private void Listen()
        {
            int Port = int.Parse(TextBox1.Text); //設定監聽用的通訊埠
            U = new UdpClient(Port); //建立UDP監聽器
            IPEndPoint EP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port); //建立本機端點資訊
            while (true) //持續監聽的無限迴圈→有訊息(True)就處理，無訊息就等待！
            {
                byte[] B = U.Receive(ref EP);//訊息到達時讀取資訊到B陣列
                string R = Encoding.Default.GetString(B);//翻譯B陣列為字串R
                if (R.Substring(1, 1) == "A")//檢視第二字元(是否為"A")
                {//收到對手回應我的猜測提示("?A?B")
                    ListBox2.Items.Add(TextBox5.Text + ">" + R);//紀錄猜測提示
                    Button2.Enabled = false;//暫停猜數字按鍵功能，輪到對手猜數字
                    if (R == "4A0B") { MessageBox.Show("你贏了！"); }//全部猜對，顯示我方勝
                }
                else
                {//收到對手猜測我的數字
                    string K = chk(R);//檢測數字對位的狀況，產生提示字串K
                    Send(K);//用網路送出提示
                    ListBox1.Items.Add(R + ">" + K);//紀錄對手猜過的數字
                    Button2.Enabled = true;//啟動按鍵功能，輪到我猜了！
                    if (K == "4A0B") { MessageBox.Show("你輸了！"); }//對手全部猜對，顯示我方敗
                }
            }
        }
        //找出本機IP
        private string MyIP()
        {
            string hn = Dns.GetHostName();//取得本機電腦名稱
            IPAddress[] ip = Dns.GetHostEntry(hn).AddressList; //取得本機IP陣列(可能有多個)
            foreach (IPAddress it in ip)//列舉各個IP
            {
                if (it.AddressFamily == AddressFamily.InterNetwork)//如果是IPv4格式
                {
                    return it.ToString();//回傳此IP字串
                }
            }
            return ""; //找不到合格IP，回傳空字串
        }
        //表單載入
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text += " " + MyIP();//顯示本機IP於標題列
        }
        //關閉接聽執行續(如果有的話)
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Th.Abort(); //關閉監聽執行緒
                U.Close(); //關閉監聽器
            }
            catch
            {
                //如果監聽執行緒未建立會出現錯誤，程式跳到此處執行，
                //此處不寫程式就是忽略錯誤，程式將繼續執行！
            }
        }

        //傳送字串給對手
        private void Send(string A)
        {
            int Port = int.Parse(TextBox3.Text);//對手通訊埠
            UdpClient S = new UdpClient(TextBox2.Text, Port);//宣告UDP通訊物件
            byte[] B = Encoding.Default.GetBytes(A);//轉譯字串為Byte陣列
            S.Send(B, B.Length);//送出資訊
            S.Close();//關閉通訊物件
        }
        //檢測對位狀況回傳提示字串
        private string chk(string G)
        {
            int A = 0, B = 0;//數字對位變數
            char[] C = TextBox4.Text.ToCharArray();//我的密碼的字元陣列
            char[] D = G.ToCharArray();//猜測數字的字元陣列
            //C與D陣列元素交叉比對
            for (int i = 0; i <= 3; i++)
            {
                for (int j = 0; j <= 3; j++)
                {
                    if (C[i] == D[j])//字元正確
                    {
                        if (i == j)//位置正確
                        {
                            A += 1;//A加一
                        }
                        else//位置不正確
                        {
                            B += 1;//B加一
                        }
                    }
                }
            }
            return A.ToString() + "A" + B.ToString() + "B";//回傳"?A?B"字串
        }
        //送出猜測數字
        private void Button2_Click(object sender, EventArgs e)
        {
            int rv;//宣告一整數
            bool isnum = int.TryParse(TextBox5.Text, out rv);//可否翻譯為整數？
            if (TextBox5.Text.Length != 4 || isnum == false)
            {
                MessageBox.Show("必須是四個數字！");
                return;//跳出副程序
            }
            char[] C = TextBox5.Text.ToCharArray();//將你猜的數字轉成字元陣列
            bool rpt = false;//數字是否重複的變數
            //交叉比對C陣列各元素
            for (int i = 0; i <= 3; i++)
            {
                for (int j = 0; j <= 3; j++)
                {
                    if (C[i] == C[j] && i != j)//字元相同
                    {
                        rpt = true;//有重複
                    }
                }
            }
            if (rpt)//如果數字重複
            {
                MessageBox.Show("數字不能重複！");
                return;//跳出副程序
            }
            Send(TextBox5.Text);//用網路送出字串
        }
    }
}
