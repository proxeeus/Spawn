﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EnhancedMap.GUI
{
    public partial class SpawnEntryWindow : Form
    {
        public SpawnEntryWindow()
        {
            InitializeComponent();
            InitMobileTypes();
        }

        private void InitMobileTypes()
        {
            var mobilesPath = @"D:\RunUO.T2A\Scripts\Mobiles";
            var mobilesDirectory = new DirectoryInfo(mobilesPath);
            

                ListDirectory(mobilesTreeView, mobilesPath);

        }

        private void ListDirectory(TreeView treeView, string path)
        {
            treeView.Nodes.Clear();
            var rootDirectoryInfo = new DirectoryInfo(path);
            treeView.Nodes.Add(CreateDirectoryNode(rootDirectoryInfo));
        }

        private static TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeNode(directoryInfo.Name);
            foreach (var directory in directoryInfo.GetDirectories())
                directoryNode.Nodes.Add(CreateDirectoryNode(directory));
            foreach (var file in directoryInfo.GetFiles())
                directoryNode.Nodes.Add(new TreeNode(Path.GetFileNameWithoutExtension(file.Name)));
            return directoryNode;
        }
    }
}