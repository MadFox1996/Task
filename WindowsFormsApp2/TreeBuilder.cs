using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public class TreeBuilder
    {
        private bool _cancelled = false;
        private bool _paused = false;
        public void Cancel()
        {
            _cancelled = true;
        }
        public void SetPause()
        {
            if (_paused)
                _paused = false;
            else
                _paused = true;
        }

        public void Execute(object obj)
        {
            Work(obj);
            WorkCompleted(_cancelled);
        }
        public void Work(object obj)
        {
            while (_paused)
            {
                if (_cancelled)
                    return;
            }
               
            Tuple<DirectoryInfo, TreeNodeCollection, TextBox, TextBox> t = (Tuple<DirectoryInfo, TreeNodeCollection, TextBox, TextBox>)obj;

          
            TreeNode curNode = SetCur(t.Item2, t.Item1.Name);
            if (_cancelled)
                return;

            foreach (FileInfo file in t.Item1.GetFiles(t.Item3.Text))
            {
                InfoUpdate(file.FullName);

                if (File.ReadAllText(file.FullName, Encoding.Default).Contains(t.Item4.Text))
                {
                    while (_paused)
                    {
                        if (_cancelled)
                            return;
                    }
                    TreeUpdate(curNode, file.Name);
                    Thread.Sleep(2000);
                }
                if (_cancelled)
                    return;
            }
            foreach (DirectoryInfo subdir in t.Item1.GetDirectories())
            {
                while (_paused)
                {
                    if (_cancelled)
                        return;
                }
                Tuple<DirectoryInfo, TreeNodeCollection, TextBox, TextBox> tuple =
                    new Tuple<DirectoryInfo, TreeNodeCollection, TextBox, TextBox>(subdir, curNode.Nodes, t.Item3, t.Item4);
                Work(tuple);
                if (_cancelled)
                    return;
            }
        }  

        public event Action<string> InfoUpdate;
        public event Action<TreeNode, string> TreeUpdate;
        public event Func<TreeNodeCollection,string, TreeNode> SetCur;
        public event Action<bool> WorkCompleted;
    }
}
