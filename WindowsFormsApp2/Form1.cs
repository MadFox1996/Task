using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
       
        private int _filescount = 0;

        private bool _timerpause = false;
       
        private TreeBuilder _treebuilder;
            
        private DateTime _date;

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        private DateTime _accumulated_time;

        DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(FormerValues));

        private FileInfo _formerValues;
        

        public Form1()
        {
            InitializeComponent();

            using (FileStream fstream = new FileStream(@"../FormerValues.json", FileMode.Open))
            {
                _formerValues = new FileInfo(@"../FormerValues.json");
            }

            if (new FileInfo(_formerValues.FullName).Length == 0)
            {
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
            }
            else
            {
                using(FileStream sr = new FileStream(_formerValues.FullName, FileMode.Open))
                {
                    FormerValues fv = (FormerValues)jsonFormatter.ReadObject(sr);
                    textBox1.Text = fv.path;
                    textBox2.Text = fv.template;
                    textBox3.Text = fv.content;
                }
            }
            timer.Interval = 10;
            timer.Tick += new EventHandler(tickTimer);
            button2.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            timeLabel.Text = "00:00:00:00";
        }

        public void TimerPause()
        {
            if (_timerpause)
            {
                _timerpause = false;
            }
            else
            {
                _timerpause = true;
            }
        }

        private void tickTimer(object sender, EventArgs e)
        {
            long tick = DateTime.Now.Ticks - _date.Ticks + _accumulated_time.Ticks;
            DateTime stopWatch = new DateTime();
            stopWatch = stopWatch.AddTicks(tick);
            timeLabel.Text = String.Format("{0:HH:mm:ss:ff}", stopWatch);
            if (_timerpause)
            {
                _accumulated_time = stopWatch;
                timer.Stop();
            }
        }

        private void folderBrowserDialog1_HelpRequest_1(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            DirectoryInfo dir = new DirectoryInfo(textBox1.Text);
            _treebuilder = new TreeBuilder();
            treeView1.Nodes.Clear();
            _filescount = 0;
            _accumulated_time = new DateTime();

            _treebuilder.InfoUpdate+=TreeBuilder_InfoUpdate;
            _treebuilder.TreeUpdate+=TreeBuilder_TreeUpdate;
            _treebuilder.SetCur += TreeBuilder_SetCur;
            _treebuilder.WorkCompleted += TreeBuilder_WorkCompleted;

            FormerValues formerValues = new FormerValues(textBox1.Text, textBox2.Text, textBox3.Text);

            using (FileStream fs = new FileStream(_formerValues.FullName, FileMode.Create))
            {
                jsonFormatter.WriteObject(fs, formerValues);
            }

            _date = DateTime.Now;
            timer.Start();
           
            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = false;
            button5.Enabled = true;

            Tuple<DirectoryInfo, TreeNodeCollection, TextBox, TextBox> tuple 
                = new Tuple<DirectoryInfo, TreeNodeCollection, TextBox, TextBox>(dir,treeView1.Nodes,textBox2,textBox3);
            Thread thread = new Thread(_treebuilder.Execute);
            thread.Start(tuple);
        }//start

        private void button2_Click_1(object sender, EventArgs e)//pause
        {
            button2.Enabled = false;
            button1.Enabled = false;
            button4.Enabled = true;
            _treebuilder.SetPause();
            TimerPause();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                this.textBox1.Text = this.folderBrowserDialog1.SelectedPath;
        }//browse

        private void button4_Click_1(object sender, EventArgs e)//continue
        {
            button2.Enabled = true;
            button1.Enabled = false;
            button4.Enabled = false;

            _treebuilder.SetPause();
            TimerPause();
            _date = DateTime.Now;
            timer.Start();
        }

        private void button5_Click(object sender, EventArgs e)//stop
        {
            button5.Enabled = false;
            button4.Enabled = false;
            button2.Enabled = false;
            button1.Enabled = true;
            _treebuilder.Cancel();
            _timerpause = false;
            timer.Stop();
        }

        private void TreeBuilder_WorkCompleted(bool result)
        {
            Action action = () =>
            {
                if (result)
                {
                    button3.Enabled = true;
                }
                else
                {
                    timer.Stop();
                    button1.Enabled = true;
                    button2.Enabled = false;
                    button3.Enabled = true;
                    button4.Enabled = false;
                    button5.Enabled = false;
                }
            };
            Invoke(action);
        }
        private void TreeBuilder_InfoUpdate(string filename)
        {
            Action action = () =>
            {
                _filescount++;
                label5.Text = filename;
                label4.Text = Convert.ToString(_filescount);
            };
            Invoke(action);
        }
        private void TreeBuilder_TreeUpdate(TreeNode curNode, string nodename)
        {
            
            Action action = () =>
            {
                curNode.Nodes.Add(nodename);
                treeView1.ExpandAll();
            };
            Invoke(action);
        }
        private TreeNode TreeBuilder_SetCur(TreeNodeCollection tree,string name)
        {
            Func<TreeNode> func = delegate ()
            {
                TreeNode curNode = tree.Add(name);
                treeView1.ExpandAll();
                return curNode;
            };
            return (TreeNode)Invoke(func);
        }
    }
}