using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PLTCWbyArtar
{
    public partial class Form2 : Form
    {
        public Form2(DFSM m)
        {
            InitializeComponent();

            dataGrid.ReadOnly = false;

            dataGrid.RowCount = m.conditions.Count;
            dataGrid.ColumnCount = m.alphabet.Count;

            int i = 0;
            foreach (KeyValuePair<char, bool> condition in m.conditions)
            {
                dataGrid.Rows[i].HeaderCell.Value = condition.Key.ToString();
                i++;
            }

            i = 0;
            foreach (char symbol in m.alphabet)
            {
                dataGrid.Columns[i].HeaderCell.Value = symbol.ToString();
                dataGrid.Columns[i].ValueType = typeof(char);
                ((DataGridViewTextBoxColumn)dataGrid.Columns[i]).MaxInputLength = 1;
                i++;
            }

            if (m.translates.Count == m.conditions.Count)
            {
                if (m.translates.First().Length != m.alphabet.Count)
                {
                    MessageBox.Show("Ошибка при создании таблицы - код Epsilon");
                    return;
                }
                    

                i = 0;
                foreach (char[] chars in m.translates)
                {
                    for (int j = 0; j < dataGrid.ColumnCount; j++)
                    {
                        dataGrid.Rows[i].Cells[j].Value = chars[j];
                    }
                    i++;
                }
            }
            else
            {
                MessageBox.Show("Ошибка при создании таблицы - код Gamma");
                return;
            }

            dataGrid.ReadOnly = true;
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
