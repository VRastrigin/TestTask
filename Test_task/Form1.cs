using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test_task
{
    public partial class Find_files : Form
    {
        bool temp_stop = false;
        int number_of_processed = 0;
        int new_number_of_processed = 0;
        DateTime dold = new DateTime();
        TimeSpan sp = new TimeSpan();
        bool timer1_exe = false;
        bool check = false;     
        public Find_files()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = Properties.Settings.Default.StartFolder;
            textBox2.Text = Properties.Settings.Default.MaskFind;
            textBox3.Text = Properties.Settings.Default.InFileFind; 

            if (textBox1.Text == "" && textBox2.Text == "" && textBox3.Text == "")
            {
                button4.Enabled = false;
            }
            else
            {
                button4.Enabled = true;
            }            
            if (textBox1.Text == "")
            {
                button2.Enabled = false;
            }
            else if (textBox1.Text != "")
            {
                button2.Enabled = true;
            }
            button3.Enabled = false;
        }

        private void FileSearchFunction(string Dir, TreeNode rootNode)             // http://blog.foolsoft.ru/c-perebor-vsex-fajlov-v-direktorii-s-uchetom-poddirektorij-i-otobrazheniem-tekushhego-fajla/
        {                                                                          // http://msdn.microsoft.com/en-us/library/ms171645.aspx
            DirectoryInfo DI = new DirectoryInfo((string)Dir);
            DirectoryInfo[] SubDir = null;
            try
            {
                SubDir = DI.GetDirectories();
            }
            catch
            {
                return;
            }
            TreeNode[] rootNode2 = new TreeNode[SubDir.Length];
            for (int i = 0; i < SubDir.Length; ++i)
            {
                this.Invoke(new System.Threading.ThreadStart(delegate
                {
                rootNode2[i] = new TreeNode(Path.GetFileName(SubDir[i].FullName));                
                rootNode.Nodes.Add(rootNode2[i]);
                this.FileSearchFunction(SubDir[i].FullName, rootNode2[i]);
                }));
            }
            FileInfo[] FI = DI.GetFiles();
            for (int i = 0; i < FI.Length; ++i)
            {
                //Thread.Sleep(100);
                if (temp_stop != true)
                {
                    this.Invoke(new System.Threading.ThreadStart(delegate
                    {
                        label1.Text = FI[i].FullName;
                        string value = FI[i].FullName;
                        string fname = Path.GetFileName(value);                        
                        bool GoodFile = true;
                        if (textBox2.Text != "")
                        {
                            GoodFile = PatternMatch(fname, "*" + textBox2.Text + "*");
                        }
                        else if (textBox2.Text == "")
                        {
                            GoodFile = true;
                        }
                        if (textBox3.Text == "")
                        {
                            GoodFile = GoodFile;
                        }
                        else if (textBox3.Text != "" && GoodFile == true)
                        {
                            GoodFile = (File.ReadAllText(value, Encoding.Default).Contains(textBox3.Text));
                        }
                        if (GoodFile == true)
                        {
                            string[] words = value.Split('\\');
                            TreeNode aNode = new TreeNode(fname);
                            aNode.BackColor = Color.Azure;
                            rootNode.Nodes.Add(aNode);                  
                        }
                        number_of_processed++;
                        label3.Text = number_of_processed.ToString();
                        sp = DateTime.Now - dold;
                    }));
                }
            }          
        }
        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog();
            if (FBD.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = FBD.SelectedPath;
                button2.Enabled = true;
                button4.Enabled = true;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = true;
            button4.Enabled = false;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            textBox3.Enabled = false;
            treeView1.Nodes.Clear();
            dold = DateTime.Now; 
            timer1.Start();
            timer1_exe = true;                               
            temp_stop = false;
            label1.Text = "";
            number_of_processed = 0;
            check = false;
            if (textBox1.Text != String.Empty && Directory.Exists(textBox1.Text))
            {
                TreeNode MainRootNode;
                MainRootNode = new TreeNode(textBox1.Text);
                //System.Threading.Thread T = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(FileSearchFunction));              
                //T.Start(textBox1.Text);
                System.Threading.Thread T = new System.Threading.Thread(delegate() { FileSearchFunction(textBox1.Text, MainRootNode); });
                T.Start();
                treeView1.Nodes.Add(MainRootNode);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            button3.Enabled = false;
            button4.Enabled = true;            
            textBox2.Enabled = true;
            textBox3.Enabled = true;
            temp_stop = true;
            timer1.Stop();
            timer1_exe = false;
        }
        private void button4_Click(object sender, EventArgs e)
        {           
            button4.Enabled = false;
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            label1.Text = "-";
            label3.Text = "0";
            label5.Text = "-";
            timer1_exe = false;
            treeView1.Nodes.Clear();            
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timer1_exe == true)
            {
                if (label5.Text == sp.ToString(@"d\.hh\:mm\:ss\.ff"))
                {
                    button1.Enabled = true;
                    button2.Enabled = true;
                    button3.Enabled = false;
                    button4.Enabled = true;
                    textBox2.Enabled = true;
                    textBox3.Enabled = true;
                    if (check == false)
                    {
                        treeView1.ExpandAll();
                        check = true;
                    }
                }
                label5.Text = sp.ToString(@"d\.hh\:mm\:ss\.ff");
                new_number_of_processed = number_of_processed;
            }
            else
            {
                timer1.Stop();
                timer1_exe = false;
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.StartFolder = textBox1.Text;
            Properties.Settings.Default.Save();
            if (textBox1.Text == "")
            {
                button2.Enabled = false;
            }
            else if (textBox1.Text != "")
            {
                button2.Enabled = true;
            }
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MaskFind = textBox2.Text;
            Properties.Settings.Default.Save();           
            if (textBox2.Text != "")
            {
                button4.Enabled = true;
            }
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.InFileFind = textBox3.Text;
            Properties.Settings.Default.Save();
            if (textBox3.Text != "")
            {
                button4.Enabled = true;
            }
        }
        private bool PatternMatch(string Source, string Mask)           // http://rsdn.ru/forum/src/92822.flat#92822 
        {
            char[] strSource = new char[Source.Length + 1];
            char[] strMask = new char[Mask.Length + 1];
            Source.CopyTo(0, strSource, 0, Source.Length);
            Mask.CopyTo(0, strMask, 0, Mask.Length);
            int SourceIndex = 0;
            int MaskIndex = 0;
            for (; SourceIndex < strSource.Length && strMask[MaskIndex] != '*'; MaskIndex++, SourceIndex++)
                if (strMask[MaskIndex] != strSource[SourceIndex] && strMask[MaskIndex] != '?')
                    return false;
            int pSourceIndex = 0;
            int pMaskIndex = 0;
            for (; ; )
            {
                if (strSource[SourceIndex] == 0)
                {
                    while (strMask[MaskIndex] == '*')
                        MaskIndex++;
                    return strMask[MaskIndex] == 0 ? true : false;
                }
                if (strMask[MaskIndex] == '*')
                {
                    if (strMask[++MaskIndex] == 0)
                        return true;
                    pMaskIndex = MaskIndex;
                    pSourceIndex = SourceIndex + 1;
                    continue;
                }
                if (strMask[MaskIndex] == strSource[SourceIndex] || strMask[MaskIndex] == '?')
                {
                    MaskIndex++;
                    SourceIndex++;
                    continue;
                }
                MaskIndex = pMaskIndex; SourceIndex = pSourceIndex++;
            }
        }
    }
}