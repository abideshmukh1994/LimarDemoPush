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

namespace LimarDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFolderPath.Text = folderDialog.SelectedPath;

                    // Get image files and bind to DataGridView
                    var imageFiles = Directory.GetFiles(folderDialog.SelectedPath, "*.*")
                                               .Where(f => f.EndsWith(".jpg") || f.EndsWith(".png") || f.EndsWith(".jpeg"))
                                               .Select(f => new { FileName = Path.GetFileName(f), FilePath = f })
                                               .ToList();

                    dataGridViewImages.DataSource = imageFiles;
                }
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            string sourceFolder = txtFolderPath.Text;
            if (string.IsNullOrEmpty(sourceFolder) || !Directory.Exists(sourceFolder))
            {
                MessageBox.Show("Please select a valid folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string destinationFolder = Path.Combine(AppContext.BaseDirectory, "UploadedImages");
            Directory.CreateDirectory(destinationFolder);

            using (var dbContext = new AppDbContext())
            {
                var rows = dataGridViewImages.Rows.Cast<DataGridViewRow>().ToList();
                int totalFiles = rows.Count;
                int processedFiles = 0;

                progressBarUpload.Value = 0;
                progressBarUpload.Maximum = totalFiles;

                foreach (var row in rows)
                {
                    var fileName = Path.GetFileNameWithoutExtension(row.Cells["FileName"].Value.ToString());
                    var filePath = row.Cells["FilePath"].Value.ToString();

                    // Match file name with database item
                    var item = dbContext.Items.FirstOrDefault(i => i.ItemName == fileName);
                    if (item != null)
                    {
                        string destPath = Path.Combine(destinationFolder, Path.GetFileName(filePath));
                        File.Copy(filePath, destPath, true);

                        // Update database
                        item.FilePath = destPath;
                        dbContext.Entry(item).State = EntityState.Modified;
                    }

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
