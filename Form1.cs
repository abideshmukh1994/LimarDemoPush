using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LimarDemo.Model;

namespace LimarDemo
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
        }

        private enum FileMatched 
        {
            Matched,
            Unmatched,
            None
        }

        public class StockItemImage
        {
            public string FileName { get; set; }
            public string FileNameWithExtension { get; set; }
            public string FileMatched { get; set; }
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        public void InitializeDataGrid()
        {
            dataGridViewImages.Rows.Clear();
            dataGridViewImages.Columns.Clear();
            dataGridViewImages.Columns.Add("FileName", "File Name");
            dataGridViewImages.Columns.Add("FileNameWithExtension", "Full File Name");
            dataGridViewImages.Columns.Add("FileMatched", "File Matched");
        }

        private List<StockItemImage> ItemImageFiles = new List<StockItemImage>();

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFolderPath.Text = folderDialog.SelectedPath;

                    // Get image files and bind to DataGridView
                    ItemImageFiles = Directory.GetFiles(folderDialog.SelectedPath, "*.*")
                                               .Where(imageFile => imageFile.EndsWith(".jpg") || imageFile.EndsWith(".png") || imageFile.EndsWith(".jpeg"))
                                               .Select(imageFile => new StockItemImage { FileName = Path.GetFileNameWithoutExtension(imageFile), FileNameWithExtension = Path.GetFileName(imageFile), FileMatched = FileMatched.None.ToString() })
                                               .ToList();

                }
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            InitializeDataGrid();
            string sourceFolder = txtFolderPath.Text;
            if (string.IsNullOrEmpty(sourceFolder) || !Directory.Exists(sourceFolder))
            {
                MessageBox.Show("Please select a valid folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var dbContext = new AppDbContext())
            {
                var imageFiles = ItemImageFiles;
                int totalFiles = imageFiles.Count;
                int processedFiles = 0;

                progressBarUpload.Value = 0;
                progressBarUpload.Maximum = totalFiles;

                //Get items without file path
                var itemsWithoutItemImagePath = dbContext.Items.Where(i => i.ItemPath == null);

                foreach (var imageFile in imageFiles)
                {
                    // Match file name with fetched item
                    var item = itemsWithoutItemImagePath.FirstOrDefault(i => i.ItemName == imageFile.FileName);
                    if (item != null)
                    {
                        // Update database
                        item.ItemPath = imageFile.FileNameWithExtension;
                        dbContext.Entry(item).State = EntityState.Modified;
                    }

                    imageFile.FileMatched = item != null ? FileMatched.Matched.ToString() : FileMatched.Unmatched.ToString();
                    dataGridViewImages.Rows.Add(imageFile.FileName, imageFile.FileNameWithExtension, imageFile.FileMatched);
                    // Update progress
                    processedFiles++;
                    progressBarUpload.Value = processedFiles;

                    // Refresh the progress bar to reflect updates
                    progressBarUpload.Refresh();
                }


                dbContext.SaveChanges();
            }

            MessageBox.Show("Files uploaded and database updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            progressBarUpload.Value = 0; // Reset progress bar
        }
    }
}
