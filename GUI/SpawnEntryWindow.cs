﻿using EnhancedMap.Core;
using EnhancedMap.Core.MapObjects;
using System;
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
        public BindingList<SpawnObject> Spawns { get; private set; }

        public SpawnDefinition GetGUISpawnDefinition()
        {
            var spawnDefinition = new SpawnDefinition();

            spawnDefinition.HomeRange = this.homeRangeTextBox.Text;
            spawnDefinition.MaxTime = this.maxTimeTextBox.Text;
            spawnDefinition.MinTime = this.minTimeTextBox.Text;
            spawnDefinition.NPCCount = this.npcCountTextBox.Text;
            spawnDefinition.SpawnerName = this.spawnNameTextBox.Text;
            spawnDefinition.Team = this.teamTextBox.Text;
            
            foreach (var mobile in spawnMobilesListBox.Items) { spawnDefinition.Mobiles.Add(mobile.ToString()); }

            return spawnDefinition;
        }

        public void AddNewSpawnEntry(SpawnObject spawner)
        {
            Spawns.Add(spawner);

            RefreshUI();
        }

        public void RefreshUI()
        {
            allSpawnsListBox.Refresh();
            allSpawnsListBox.Update();
        }

        public SpawnEntryWindow()
        {
            InitializeComponent();
            InitMobileTypes();
            Spawns = new BindingList<SpawnObject>();
            allSpawnsListBox.DataSource = Spawns;
            allSpawnsListBox.DisplayMember = "SpawnerName";

            if (Spawns == null) Spawns = new BindingList<SpawnObject>();
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

        private void mobilesTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var clickedNode = e.Node;
            if (clickedNode.Nodes.Count > 0) return;

            spawnMobilesListBox.Items.Add(clickedNode.Text);
        }

        private void addMobileTypeButton_Click(object sender, EventArgs e)
        {
            spawnMobilesListBox.Items.Add(mobileTypeNameTextBox.Text);
        }
    }
}
