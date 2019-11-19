using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;

namespace WindowsFormsGRID
{
    public partial class Form1 : Form
    {
        private string InitialDirectory= Path.GetDirectoryName(Application.ExecutablePath);
        private FileContainer container;
        private int rowIndex = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void SetupDataGridView()
        {
            //dataGridView1.ColumnCount = 5;

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Navy;
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);

            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            //dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            //dataGridView1.GridColor = Color.Black;
            dataGridView1.RowHeadersVisible = false;

            dataGridView1.Columns[2].DefaultCellStyle.Font = new Font(dataGridView1.DefaultCellStyle.Font, FontStyle.Italic);

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.Dock = DockStyle.Fill;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "File Container";
            label2.Text = "Container file not defined";
            label3.Text = "0";
            label5.Text = "0";
            label8.Text = "";
            //textBox1.Text = "";
            //textBox1.PasswordChar = '*';
            SetupDataGridView();
            //panel1.Visible = false;

            container = new FileContainer();
        }

        private void dataGridView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }
        private void dataGridView1_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            //int i;
 
            foreach (string file in s)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string ext = Path.GetExtension(file);
                
                FileInfo fileinfo = new FileInfo(file);

                string[] row = {
                                    Path.GetFileNameWithoutExtension(file),
                                    Path.GetExtension(file),
                                    fileinfo.Length.ToString(),
                                    fileinfo.LastWriteTime.ToString(),
                                    Path.GetFullPath(file)
                                };

                dataGridView1.Rows.Add(row);
                container.AddEntry(new FileEntry(file));
                label3.Text = container.Count.ToString();
                label5.Text = container.Size.ToString();

            }
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var filename = string.Empty;
            DialogResult result=DialogResult.OK;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = System.IO.Directory.GetParent(@"./").FullName; /*Environment.CurrentDirectory;*/ /*"c:\\"*/;
                openFileDialog.InitialDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
                openFileDialog.Filter = "container's files (*.cnt)|*.cnt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.InitialDirectory = InitialDirectory;
                openFileDialog.FileName = "mycontainer.cnt";
                openFileDialog.Title = "Select Container File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filename = openFileDialog.FileName;

                    container = new FileContainer(filename);
                    dataGridView1.Rows.Clear();

                    string pass = container.getPassword();
                    bool tmpb = string.IsNullOrEmpty(pass);
                    label8.Text = (tmpb) ? "Container unprotected" : "Container protected";

                    if (!tmpb)
                    {
                        using (Form2 formPassword = new Form2(pass))
                        {
                            if ((formPassword.ShowDialog() != DialogResult.OK)|| (formPassword.password != pass))
                                 result = DialogResult.No;
                         }
                    }

                    if (result == DialogResult.OK)
                    {
                        foreach (FileEntry foo in container/*.files*/)
                        {
                            string[] row = 
                                {
                                    foo.Name,
                                    foo.Extension,
                                    foo.Size.ToString(),
                                    foo.FIleTime.ToString(),
                                    foo.FileRelativePath.ToString()
                                };
                            dataGridView1.Rows.Add(row);
                        }
                        label2.Text = filename.ToString();
                        label3.Text = container.Count.ToString();
                        label5.Text = container.Size.ToString();
                    }
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "container's files (*.cnt)|*.cnt|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.InitialDirectory = InitialDirectory;
                saveFileDialog.FileName = "mycontainer.cnt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filename = saveFileDialog.FileName;
                    container.SaveContainer(filename);
                    label2.Text = filename.ToString();
                    label3.Text = container.Count.ToString();
                    label5.Text = container.Size.ToString();
                }
            }
        }

        private void dataGridView1_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Right) && !container.IsEmpty)
            {
                dataGridView1.Rows[e.RowIndex].Selected = true;
                rowIndex = e.RowIndex;
                //dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells[1];
                contextMenuStrip1.Show(dataGridView1, e.Location);
                contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void deleteFileFromContainerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!this.dataGridView1.Rows[rowIndex].IsNewRow)
            {
                string name = this.dataGridView1.Rows[rowIndex].Cells["Column1"].Value.ToString();
                string ext = this.dataGridView1.Rows[rowIndex].Cells["Column2"].Value.ToString();
                dataGridView1.Rows.RemoveAt(rowIndex);
                container.Remove(name+ext);

                label3.Text = container.Count.ToString();
                label5.Text = container.Size.ToString();
            }
        }

        private void extractFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {

                string name = dataGridView1.Rows[rowIndex].Cells["Column1"].Value.ToString();
                string ext = dataGridView1.Rows[rowIndex].Cells["Column2"].Value.ToString();
                ext = ext.Remove(0, 1);
                //string path = dataGridView1.Rows[rowIndex].Cells["Column4"].Value.ToString();

                string path = Path.GetDirectoryName(dataGridView1.Rows[rowIndex].Cells["Column4"].Value.ToString());


                saveFileDialog.Filter = ext+" files (*"+ext+")|*."+ext+"|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.InitialDirectory = path;
                saveFileDialog.FileName = name/* + ext*/;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //string filename = Path.GetFileName( saveFileDialog.FileName);
                    //container.Extract(filename);
                    container.Extract(saveFileDialog.FileName);
                }
            }
        }

        private void addFileToContainerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var filename = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = System.IO.Directory.GetParent(@"./").FullName; /*Environment.CurrentDirectory;*/ /*"c:\\"*/;
                openFileDialog.InitialDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())));
                openFileDialog.Filter = "All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.InitialDirectory = InitialDirectory;
                openFileDialog.FileName = "";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string file = openFileDialog.FileName;
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string ext = Path.GetExtension(file);

                    FileInfo fileinfo = new FileInfo(file);

                    string[] row = {
                                    Path.GetFileNameWithoutExtension(file),
                                    Path.GetExtension(file),
                                    fileinfo.Length.ToString(),
                                    fileinfo.LastWriteTime.ToString(),
                                    Path.GetFullPath(file)
                                };

                    dataGridView1.Rows.Add(row);
                    container.AddEntry(new FileEntry(file));
                    label3.Text = container.Count.ToString();
                    label5.Text = container.Size.ToString();
                }
            }
        }

        private void passwordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 formPassword = new Form3();
            if (formPassword.ShowDialog() == DialogResult.OK)
                container.setPassword(formPassword.Password);
        }

    }
}

