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
using System.Diagnostics;

namespace OS1
{
    public partial class Form1 : Form
    {
        int size;
        int funcCounter;
        string fileName = @"materials\input.txt";
        string algorithm = "";

        List <int> fileID = new List<int>();
        List <string> fileOperation = new List<string>();
        List<int> fileSize = new List<int>();

        List <int> diskSpace = new List<int>();

        StreamWriter sw;

        public Form1()
        {
            InitializeComponent();
        }

        public TextBox createTextBox(int index, int element)
        {
            TextBox textbox = new TextBox();
            textbox.Text = element == 0 ? index.ToString() : index.ToString() + " (" + element.ToString() + ")";
            textbox.Dock = DockStyle.Fill;
            textbox.Multiline = true;
            textbox.Font = new Font(textbox.Font.FontFamily, 8, textbox.Font.Style);

            if (element != 0)
            {
                Random rnd = new Random(element * 50000);
                textbox.BackColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            }
            else
            {
                textbox.BackColor = Color.FromArgb(255, 255, 255);
            }

            return textbox;
        }

        public void readFile()
        {
            StreamReader sr = new StreamReader(fileName);
            string fileText = sr.ReadLine();
            string keyWord = "";

            while (fileText != null)
            {

                int i = 0;
                while (fileText[i] != ' ')
                {
                    keyWord = keyWord + fileText[i];
                    i++;
                }
                fileID.Add(Convert.ToInt32(keyWord));
                keyWord = "";
                i++;

                while (i != fileText.Length && fileText[i] != ' ')
                {
                    keyWord = keyWord + fileText[i];
                    i++;
                }
                fileOperation.Add(keyWord);
                keyWord = "";
                i++;

                while (i < fileText.Length)
                {
                    keyWord = keyWord + fileText[i];
                    i++;
                }

                if (keyWord != "")
                {
                    fileSize.Add(Convert.ToInt32(keyWord));
                }
                else
                {
                    fileSize.Add(0);
                }
                keyWord = "";

                fileText = sr.ReadLine();
            }
        }

        public void createTable()
        {
            tableLayoutPanel1.Controls.Clear();
            size = Convert.ToInt32(numericUpDown1.Value);

            for (int i = 0; i < size; i++)
            {
                tableLayoutPanel1.Controls.Add(createTextBox(i, diskSpace[i]));
            }
        }

        public bool freeSpaceChecker(int size)
        {
            int freeSpace = 0;
            for (int i = 0; i < diskSpace.Count; i++)
            {
                if (diskSpace[i] == 0)
                {
                    freeSpace++;
                }
            }

            if (freeSpace < size)
            {
                MessageBox.Show("out of disk space");
                return false;
            }
            return true;
        }

        public void averageStat()
        {
            StreamWriter statWriter = new StreamWriter(@"materials\statistic.txt", true, System.Text.Encoding.Default);
            List<int> id = new List<int>();
            float segSize = 0, segCount = 0, segPerFile = 0;
            bool check = false;
            diskSpace.Add(-1);

            for (int i = 0; i < diskSpace.Count; i++)
            {
                if (diskSpace[i] != 0)
                {
                    check = false;
                    for (int j = 0; j < id.Count; j++)
                    {
                        if (id[j] == diskSpace[i])
                        {
                            segPerFile++;
                            check = true;
                        }
                    }

                    if (!check && diskSpace[i] != -1 && diskSpace[i] != 0)
                    {
                        id.Add(diskSpace[i]);
                        segPerFile++;
                    }

                    while (i != diskSpace.Count - 1 && diskSpace[i] == diskSpace[i+1])
                    {
                        segSize++;
                        i++;
                    }

                    segSize++;
                    segCount++;
                }
            }

            diskSpace.RemoveAt(diskSpace.Count - 1);

            string stat1, stat2;

            stat1 = (segPerFile / id.Count).ToString();
            stat2 = (segSize / segCount).ToString();
            segSize--;
            segCount--;
            textBox1.Text = stat1;
            textBox2.Text = stat2;
            statWriter.WriteLine("Сегментов на файл: " + stat1);
            statWriter.WriteLine("Размер сегмента: " + stat2);

            textBox3.Text = funcCounter.ToString();
            statWriter.WriteLine("Количество обращений к функции: " + funcCounter);

            statWriter.WriteLine("");

            statWriter.Close();
        }

        public bool enoughSpaceController(int start, int element)
        {
            funcCounter++;
            int count = 0;
            for (int i = start; i < start + fileSize[element]; i++)
            {
                if (diskSpace[i] == 0)
                    count++;
            }

            if (count == fileSize[element])
            {
                for (int i = start; i < start + count; i++)
                {
                    diskSpace[i] = fileID[element];
                    sw.Write(i + " ");
                }
                return true;
            }
            return false;
        }

        public void notEnoughSpaceController(int element)
        {
            funcCounter++;
            int[] max = {0,0};
            int count;
            int start;

            for (int i = 0; i < diskSpace.Count; i++)
            {
                count = 0;

                if (diskSpace[i] == 0)
                {
                    start = i;
                    while (diskSpace[i] == 0)
                    {
                        count++;
                        i++;
                    }
                    if (count > max[0])
                    {
                        max[0] = count;
                        max[1] = start;
                    }
                }
            }

            for (int i = max[1]; i < max[1] + max[0]; i++)
            {
                diskSpace[i] = fileID[element];
                sw.Write(i + " ");
            }
            fileSize[element] = fileSize[element] - max[0];
        }

        public bool updateController(int element)
        {
            funcCounter++;
            int minusSize = 0;
            for (int i = element - 1; i >= 0; i--)
            {
                if (fileID[i] == fileID[element] && fileOperation[i] == "create")
                {
                    minusSize = fileSize[i] - fileSize[element];
                }
            }

            if (minusSize > 0)
            {
                for (int i = 0; i < diskSpace.Count; i++)
                {
                    if (diskSpace[i] == fileID[element])
                    {
                        diskSpace[i] = 0;
                        minusSize -= 1;
                        sw.Write(i + "(del) ");

                        if (minusSize == 0)
                            break;
                    }
                }
            }
            else
            {
                minusSize = -minusSize;
                for (int i = 0; i < diskSpace.Count; i++)
                {
                    if (diskSpace[i] == fileID[element] && i + 1 != diskSpace.Count && diskSpace[i + 1] == 0)
                    {
                        i++;
                        while(diskSpace[i] == 0 && minusSize > 0)
                        {
                            diskSpace[i] = fileID[element];
                            sw.Write(i + " ");

                            minusSize -= 1;
                        }

                        if (minusSize > 0)
                        {
                            fileOperation[element] = "create";
                            fileSize[element] -= minusSize;
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            fileSize[element] = minusSize;
            fileOperation[element] = "create";
            return false;
        }

        public void deleteController(int element)
        {
            funcCounter++;
            for (int i = 0; i < diskSpace.Count; i++)
            {
                if (diskSpace[i] == fileID[element])
                {
                    diskSpace[i] = 0;
                    sw.Write(i + "(del) ");
                }
            }
        }

        public void writeFromAddress(int start, int element)
        {
            funcCounter++;
            for (int i = start; i < start + fileSize[element]; i++)
            {
                diskSpace[i] = fileID[element];
                sw.Write(i + " ");
            }
        }

        public bool work()
        {
            int lastCreate = 0;
            int counter = 0;

            for (int i = 0; i < fileID.Count; i++)
            {
                sw.Write("file №" + fileID[i].ToString() + " " + fileOperation[i] + "\t");
            Another:

                switch (algorithm)
                {
                    case "first":
                        switch (fileOperation[i])
                        {

                            case "create":
                                if (!freeSpaceChecker(fileSize[i]))
                                    return false;
                                int j = 0;
                                for (j = 0; j <= diskSpace.Count; j++)
                                {
                                    if (j == diskSpace.Count)
                                    {
                                        notEnoughSpaceController(i);
                                        goto Another;
                                    }
                                    if (diskSpace[j] == 0 && enoughSpaceController(j, i))
                                    {
                                        break;
                                    }
                                }
                                break;
                            case "update":
                                if (!updateController(i))
                                    goto Another;
                                break;
                            case "delete":
                                deleteController(i);
                                break;
                        }
                        break;

                    case "second":
                        switch (fileOperation[i])
                        {
                            case "create":
                                if (!freeSpaceChecker(fileSize[i]))
                                    return false;

                                for (int j = lastCreate; j <= diskSpace.Count; j++)
                                {
                                    if (j == diskSpace.Count)
                                    {
                                        j = 0;
                                    }
                                    if (counter == diskSpace.Count)
                                    {
                                        notEnoughSpaceController(i);
                                        counter = 0;
                                        goto Another;
                                    }
                                    if (diskSpace[j] == 0 && enoughSpaceController(j, i))
                                    {
                                        lastCreate = j + 1;
                                        counter = 0;
                                        break;
                                    }
                                    counter++;
                                }
                                break;
                            case "update":
                                if (!updateController(i))
                                    goto Another;
                                break;
                            case "delete":
                                deleteController(i);
                                break;
                        }
                        break;

                    case "best":
                        switch (fileOperation[i])
                        {
                            case "create":
                                if (!freeSpaceChecker(fileSize[i]))
                                    return false;
                                int j = 0;
                                int[] min = { 9999, -1 };
                                int count, start;

                                for (j = 0; j <= diskSpace.Count; j++)
                                {
                                    if (j == diskSpace.Count && min[1] == -1)
                                    {
                                        notEnoughSpaceController(i);
                                        goto Another;
                                    }
                                    else if (j == diskSpace.Count && min[1] != -1)
                                    {
                                        writeFromAddress(min[1], i);
                                        break;
                                    }
                                    if (diskSpace[j] == 0)
                                    {
                                        count = 0;
                                        start = j;
                                        while (j < diskSpace.Count && diskSpace[j] == 0)
                                        {
                                            count++;
                                            j++;
                                        }
                                        if (count < min[0] && count >= fileSize[i])
                                        {
                                            min[0] = count;
                                            min[1] = start;
                                            j--;
                                        }
                                    }
                                }
                                break;
                            case "update":
                                if (!updateController(i))
                                    goto Another;
                                break;
                            case "delete":
                                deleteController(i);
                                break;
                        }
                        break;

                    case "worst":
                        switch (fileOperation[i])
                        {
                            case "create":
                                if (!freeSpaceChecker(fileSize[i]))
                                    return false;
                                int j = 0;
                                int[] max = { 0, -1 };
                                int count, start;

                                for (j = 0; j <= diskSpace.Count; j++)
                                {
                                    if (j == diskSpace.Count && max[1] == -1)
                                    {
                                        notEnoughSpaceController(i);
                                        goto Another;
                                    }
                                    else if (j == diskSpace.Count && max[1] != -1)
                                    {
                                        writeFromAddress(max[1], i);
                                        break;
                                    }
                                    if (diskSpace[j] == 0)
                                    {
                                        count = 0;
                                        start = j;
                                        while (j < diskSpace.Count && diskSpace[j] == 0)
                                        {
                                            count++;
                                            j++;
                                        }
                                        if (count > max[0] && count >= fileSize[i])
                                        {
                                            max[0] = count;
                                            max[1] = start;
                                            j--;
                                        }
                                    }
                                }
                                break;
                            case "update":
                                if (!updateController(i))
                                    goto Another;
                                break;
                            case "delete":
                                deleteController(i);
                                break;
                        }
                        break;

                    default:
                        break;
                }
                sw.WriteLine("");
            }
            return true;
        }

        public void init()
        {
            funcCounter = 0;
            sw = new StreamWriter(@"materials\log.txt", false, System.Text.Encoding.Default);
            fileID.Clear();
            fileOperation.Clear();
            fileSize.Clear();
            diskSpace.Clear();

            for (int i = 0; i < numericUpDown1.Value; i++)
            {
                diskSpace.Add(0);
            }

            algorithm = 
                radioButton1.Checked ? "first" : 
                radioButton2.Checked ? "second" : 
                radioButton3.Checked ? "best" : 
                radioButton4.Checked ? "worst" : "first";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button2.Focus();

            StreamWriter statWriter = new StreamWriter(@"materials\statistic.txt", false, System.Text.Encoding.Default);
            statWriter.Write("");
            statWriter.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            init();
            readFile();
            work();
            createTable();
            averageStat();

            sw.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start("materials");
        }
    }
}
