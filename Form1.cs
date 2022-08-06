using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PLTCWbyArtar
{
    public partial class Form1 : Form
    {
        public DFSM machine;

        public Form1()
        {
            InitializeComponent();
        }

        private void AppExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void InfoShow(object sender, EventArgs e)
        {
            MessageBox.Show("Это приложение позволяет определить, подходит ли цепочка заданному языку, предварительно" +
                " строя для некоторого описания языка ДКА в виде таблицы и графа." +
                "\nАвтор: Астраханцев А.М.\nГруппа: ИП-815");
        }

        private void DFSMBuildGraph(object sender, EventArgs e)
        {
            DFSMCreate(sender, e);
            if (machine == null || machine.conditions == null ||
                machine.alphabet == null || machine.translates == null)
            {
                MessageBox.Show("Не инициализированы необходимые данные!");
                return;
            }
            int sectorW = 200;
            int offsetW = 150;
            int W = (machine.conditions.Count + 1) * sectorW;

            Bitmap img;

            if (W > 0)
                img = new Bitmap(W, W);
            else
            {
                MessageBox.Show("Не инициализированы необходимые данные!");
                return;
            }

            Graphics g = Graphics.FromImage(img);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Pen pen = new Pen(Brushes.Chocolate);
            int i;

            char[] alphabet = machine.alphabet.ToArray();
            char[] conditions = machine.conditions.Keys.ToArray();

            //R круга - состояния
            int cR = 25;
            //инкремент высоты петель графа
            int heightIncrement = 35;

            i = 0;
            foreach (char[] chars in machine.translates)
            {
                //символы, по которым происходит переход в то же состояние
                string loopSymbols = "";
                Dictionary<char, string> otherSymbols = new Dictionary<char, string>();

                //формируем списки для каждого символа алфавита
                for (int j = 0; j < chars.Length; j++)
                {
                    //переход в то же состояние
                    if (chars[j] == conditions[i])
                        loopSymbols += alphabet[j].ToString() + ",";
                    //если символ не пустой
                    else if (chars[j] != '~')
                    {
                        if (otherSymbols.ContainsKey(chars[j]))
                        {
                            otherSymbols[chars[j]] += alphabet[j].ToString() + ",";
                            continue;
                        }
                        otherSymbols.Add(chars[j], alphabet[j].ToString() + ",");
                    }
                }

                if (loopSymbols.Length > 0)
                {
                    loopSymbols = loopSymbols.Substring(0, loopSymbols.Length - 1);
                    //рисуем дугу "в себя"
                    RectangleF rectf = new RectangleF(i * sectorW - 26 + offsetW, W / 2, 30, 50);
                    g.DrawEllipse(pen, rectf);
                    //палочки
                    g.DrawLine(pen, i * sectorW + offsetW, W / 2, i * sectorW + 3 + offsetW, W / 2 + 12);
                    g.DrawLine(pen, i * sectorW + 3 + offsetW, W / 2 + 12, i * sectorW - 6 + offsetW, W / 2 + 6);
                    //текст
                    int texL = loopSymbols.Length * 5 + (loopSymbols.Length / 2) * 6 + 25;
                    rectf = new RectangleF(i * sectorW - texL + offsetW, W / 2, texL, 50);
                    g.DrawString(loopSymbols, new Font("Arial", 12), Brushes.Black, rectf);
                }

                //отрисовка дуг в другие графы
                foreach (KeyValuePair<char, string> s in otherSymbols)
                {
                    string info = s.Value.Substring(0, s.Value.Length - 1);

                    //если перехода нет
                    if (s.Key == '~')
                        continue;

                    //получаем индекс состояния
                    int it;
                    for (it = 0; it < conditions.Length; it++)
                        if (conditions[it] == s.Key)
                            break;

                    //текущая область отрисовки
                    //RectangleF rectf = new RectangleF(i * sectorW + offsetW, W/2, 50, 50);
                    PointF[] points = new PointF[3];

                    //дистанция между текущими состояниями
                    int distance = Math.Abs((i - it) * sectorW);
                    //высшая точка графа (максимальное отклонение от середины)
                    int maxH = Math.Abs(i - it);
                    if (i >= conditions.Length || it >= conditions.Length) 
                    {
                        MessageBox.Show("Возникла критическая ошибка в процессе работы программы.");
                        return;
                    }
                    //вершина находится справа -> петля в верхней части графа
                    if (conditions[i] < conditions[it])
                    {
                        //первая точка - верхняя часть круга
                        points[0] = new PointF(i * sectorW + offsetW + cR, W / 2);
                        //вторая точка - высшая точка, центр дуги
                        int px = i * sectorW + offsetW + distance / 2 + cR, py = W / 2 - heightIncrement * maxH;
                        points[1] = new PointF(px, py);
                        //третья - верзняя часть второго состояния
                        points[2] = new PointF(i * sectorW + offsetW + cR + distance, W / 2);
                        //сразу рисуем "палочки - стрелочки"
                        pen.Width = 3;
                        g.DrawLine(pen, px, py, px - 7, py - 5);
                        g.DrawLine(pen, px, py, px - 7, py + 5);
                    }
                    //вершина слева
                    else
                    {
                        //реверс первого условия, петля идёт по нижней части графа
                        points[0] = new PointF(i * sectorW + offsetW + cR, W / 2 + cR * 2);
                        int px = i * sectorW + offsetW - distance / 2 + cR, py = W / 2 + heightIncrement * maxH + cR * 2;
                        points[1] = new PointF(px, py);
                        points[2] = new PointF(i * sectorW + offsetW + cR - distance, W / 2 + cR * 2);
                        //сразу рисуем "палочки - стрелочки"
                        pen.Width = 3;
                        g.DrawLine(pen, px, py, px + 7, py - 5);
                        g.DrawLine(pen, px, py, px + 7, py + 5);
                    }
                    pen.Width = 1;
                    //рисуем кривую
                    g.DrawCurve(pen, points);
                    //пишем текст
                    int texL = info.Length * 5 + (info.Length / 2) * 6 + 25;
                    RectangleF rectf = new RectangleF(points[1].X - texL / 2, points[1].Y, texL, 50);
                    g.DrawString(info, new Font("Arial", 12), Brushes.Black, rectf);
                }

                i++;
            }

            i = 0;
            foreach (KeyValuePair<char, bool> ch in machine.conditions)
            {
                //рисуем круг
                RectangleF rectf = new RectangleF(i * sectorW + offsetW, W / 2, cR * 2, cR * 2);
                g.DrawEllipse(pen, rectf);
                g.FillEllipse(Brushes.White, rectf);
                rectf.X += 15;
                rectf.Y += 15;

                //пишем состояние
                g.DrawString(ch.Key.ToString(), new Font("Arial", 15), Brushes.Black, rectf);

                //рисуем ещё один круг, если состояние конечно
                if (ch.Value)
                {
                    rectf.X -= 20;
                    rectf.Y -= 20;
                    rectf.Width = 60;
                    rectf.Height = 60;
                    g.DrawEllipse(pen, rectf);
                }
                //MessageBox.Show(ch.Key.ToString()+ch.Value.ToString());
                i++;
            }

            //применить все операции рисования
            g.Flush();
            //открываем форму с графом
            Form3 f = new Form3(img);
            f.Show();
        }

        private void DFSMTableOpen(object sender, EventArgs e)
        {
            DFSMCreate(sender, e);
            if (machine == null)
                return;
            Form2 f = new Form2(machine);
            f.Show();
        }

        private void DFSMCreate(object sender, EventArgs e)
        {
            machine = null;

            if (tbAlph.Text.Length == 0 || tbModSymbol.Text.Length == 0 ||
                tbModValue.Text.Length == 0 || tbSubStr.Text.Length == 0) 
            {
                MessageBox.Show("Каждое поле описания языка должно быть заполнено!");
                return;
            }

            //алфавит дополнительно парсится в конструкторе ДКА
            string[] alph = tbAlph.Text.Split(',');
            for(int i = 0; i < alph.Length; i++)
            {
                if (!DFSM.symbolCanBeInAlphabet(alph[i][0]))
                {
                    MessageBox.Show("Неверный символ в алфавите!");
                    return;
                }
            }

            //проверяем символ кратности на правильность
            char modSymbol = tbModSymbol.Text[0];
            
            bool symIsCorrect = false;
            for(int i = 0; i < alph.Length; i++)
            {
                if (alph[i][0] == modSymbol)
                {
                    symIsCorrect = true;
                    break;
                }
            }

            if (!symIsCorrect)
            {
                MessageBox.Show("В поле символа кратности введено недопустимое значение!");
                return;
            }

            int modValue = 1;

            //если значение кратности неверное - то выбрасывается exception
            try
            {
                modValue = Convert.ToInt32(tbModValue.Text);
                if (modValue < 2)
                {
                    MessageBox.Show("Значение кратности должно быть больше 1!");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("В поле значения кратности введен недопустимый символ!");
                return;
            }

            //парсим подстроку (должна принадлежать алфавиту)
            for(int i = 0; i < tbSubStr.Text.Length; i++)
            {
                char symbol = tbSubStr.Text[i];
                symIsCorrect = false;
                for (int j = 0; j < alph.Length; j++)
                {
                    if (alph[j][0] == symbol)
                    {
                        symIsCorrect = true;
                        break;
                    }
                }

                if (!symIsCorrect)
                {
                    MessageBox.Show("Недопустимый символ в конечной подстроке!");
                    return;
                }
            }

            string endSubstring = tbSubStr.Text;

            //создаем новый дка с параметрами, введенными пользователем
            //алфавит, вводится пользователем
            //состояния ДКА задаются программой
            string conditions = "", endConditions = "";
            //переходы
            HashSet<char[]> translates = new HashSet<char[]>();
            Dictionary<char,char[]> tmpTranslates = new Dictionary<char, char[]>();
            char currentCondition = 'z';

            //проверяем, является ли символ кратности частью подстроки
            //если да - то это... усложняет дка.
            bool modSymbolInSubstring = false, singleCharacter = false;
            foreach(char c in endSubstring)
            {
                if(modSymbol == c)
                {
                    modSymbolInSubstring = true;
                    break;
                }
            }

            Dictionary<char, int> dAlphabet = new Dictionary<char, int>();
            for (int i = 0; i < alph.Length; i++)
                dAlphabet.Add(alph[i][0], i);


            //****************************************************************
            //СЛУЧАЙ НУЛЕВОЙ - АЛФАВИТ СОСТОИТ ИЗ ОДНОГО СИМВОЛА
            //****************************************************************
            
            //сначала проверим, состоит ли алфавит из одного символа.
            //если да, то граф строится совсем элементарно
            if (alph.Length == 1)
            {
                singleCharacter = true;
                int modValueModifier = endSubstring.Length;
                if (modValueModifier <= modValue)
                    modValueModifier = modValue - modValueModifier;
                else
                    modValueModifier = modValue * (modValueModifier / modValue + 1) - modValueModifier;
                char last = (char)('A' + endSubstring.Length + modValueModifier);
                for (currentCondition = 'A'; currentCondition <= last; currentCondition++) 
                {
                    conditions += currentCondition.ToString() + ",";
                    bool conditionIsEnd = false;
                    
                    char[] translate = new char[alph.Length];
                    if (currentCondition != last)
                    {
                        translate[0] = (char)(currentCondition + (char)1);
                    }
                    else
                    {
                        conditionIsEnd = true;
                        translate[0] = (char)(currentCondition + 1 - modValue);
                    }

                    translates.Add(translate);
                    if (conditionIsEnd)
                        endConditions += currentCondition.ToString() + ",";
                }
            }

            //****************************************************************
            //СЛУЧАЙ ПЕРВЫЙ - СИМВОЛ КРАТНОСТИ НЕ ВХОДИТ В КОНЕЧНУЮ ПОДЦЕПОЧКУ
            //****************************************************************

            if (!modSymbolInSubstring && !singleCharacter)
            {
                //стадия разработки графа
                //(определение общего количества нужных нам состояний) 
                // > 'Z' -> программа не может построить цепочку
                //0 - минимальная цепочка
                //1 - конечное состояние. оно всегда одно.
                //2 - цепочка кратности
                //3 - выход из цикла
                int stage = 0;
                int currentLen = endSubstring.Length;

                //спец. состояния
                //0 - начальный символ A
                //1 - конечное состояние (конец конечной подцепочки)
                //2 - состояние, когда кратность modSymbol == modValue - 1 (последнее состояние в графе)
                char[] specialConditions = new char[3] { 'A', 'A', 'A' };

                for (currentCondition = 'A'; currentCondition <= 'Z'; currentCondition++)
                {
                    //подготовка
                    conditions += currentCondition.ToString() + ",";
                    bool conditionIsEnd = false;
                    char[] translate = new char[alph.Length];

                    //изначально переход по i-тому символу невозможен из текущего состояния
                    for (int it = 0; it < alph.Length; it++)
                        translate[it] = '~';
                    //основная часть

                    if(stage == 0)
                    {
                        char curC = endSubstring[endSubstring.Length - currentLen];
                        int curCIndex = dAlphabet[curC];
                        translate[curCIndex] = (char)(currentCondition + (char)1);

                        currentLen--;
                        if(currentLen <= 0)
                        {
                            stage = 1;
                            //modValue состояний для modSymbol
                            currentLen = modValue - 1;
                        }
                    }
                    else if(stage == 1)
                    {
                        stage = 2;
                        conditionIsEnd = true;
                        specialConditions[1] = currentCondition;
                    }
                    else
                    {
                        currentLen--;
                        if (currentLen <= 0)
                            stage = 3;
                    }

                    //присваивание
                    tmpTranslates.Add(currentCondition, translate);
                    if (conditionIsEnd)
                        endConditions += currentCondition.ToString() + ",";
                    //выход из цикла
                    if (stage == 3)
                    {
                        specialConditions[2] = currentCondition;
                        break;
                    }
                }
                //вышли за алфавит
                if (currentCondition > 'Z')
                {
                    MessageBox.Show("Не вышло построить ДКА - не хватило символов латинского алфавита!");
                    return;
                }
                //MessageBox.Show(specialConditions[0] + ", " + specialConditions[1] + ", " + specialConditions[2]);
                bool symbolTriggered = false;
                char prevSymbol = endSubstring[0];
                int index = -1;
                //парсим состояния минимальной цепочки
                for(char cond = specialConditions[0]; cond < specialConditions[1]; cond++)
                {
                    //получаем ранее сформированный translate
                    char[] translate = tmpTranslates[cond];
                    index++;
                    //MessageBox.Show((cond != specialConditions[0]) +""+ (endSubstring[index] == prevSymbol) +""+ !symbolTriggered);
                    //исключительный случай - возможный спам первых символов цепочки
                    if ((cond != specialConditions[0] && endSubstring[index] != prevSymbol && !symbolTriggered))
                    {
                        //MessageBox.Show(cond + " "+endSubstring[index]);
                        symbolTriggered = true;
                        for (int i = 0; i < alph.Length; i++)
                        {
                            //встретили пустой символ
                            if (translate[i] == '~')
                            {
                                //символ алфавита соответствует modSymbol - переходим в состояние specialConditions[1] + 1
                                if (alph[i][0] == modSymbol)
                                {
                                    translate[i] = (char)(specialConditions[1] + (char)1);
                                    continue;
                                }
                                //любой другой символ, не являющийся символом конечной подстроки 
                                //или символом кратности - возврат к началу
                                translate[i] = specialConditions[0];
                                if (alph[i][0] == endSubstring[index - 1])
                                    translate[i] = cond;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < alph.Length; i++)
                        {
                            //встретили пустой символ
                            if (translate[i] == '~')
                            {
                                //символ алфавита соответствует modSymbol - переходим в состояние specialConditions[1]+1
                                if (alph[i][0] == modSymbol)
                                {
                                    translate[i] = (char)(specialConditions[1] + (char)1);
                                    continue;
                                }
                                //любой другой символ, не являющийся символом конечной подстроки 
                                //или символом кратности - возврат к началу
                                translate[i] = specialConditions[0];
                            }
                        }
                    }
                    prevSymbol = endSubstring[index];
                    translates.Add(translate);
                }
                //если все символы - одни и те же в цепочке, то в финальном состоянии можем еще их погенерировать
                if (!symbolTriggered)
                {
                    for (int i = 0; i < alph.Length; i++)
                    {
                        //символ алфавита соответствует modSymbol - переходим в состояние specialConditions[1]+1
                        if (alph[i][0] == endSubstring[endSubstring.Length - 1]) 
                        {
                            tmpTranslates[specialConditions[1]][i] = specialConditions[1];
                        }
                        else if(alph[i][0] == modSymbol)
                        {
                            tmpTranslates[specialConditions[1]][i] = (char)(specialConditions[1] + 1);
                        }
                        else
                        {
                            tmpTranslates[specialConditions[1]][i] = specialConditions[0];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < alph.Length; i++)
                    {
                        if (alph[i][0] == modSymbol)
                        {
                            tmpTranslates[specialConditions[1]][i] = (char)(specialConditions[1] + 1);
                        }
                        else if(alph[i][0] == endSubstring[0])
                        {
                            tmpTranslates[specialConditions[1]][i] = (char)(specialConditions[0] + 1);
                        }
                        else
                        {
                            tmpTranslates[specialConditions[1]][i] = specialConditions[0];
                        }
                    }
                }
                translates.Add(tmpTranslates[specialConditions[1]]);
                //парсим состояния цепочки кратности
                for (char cond = (char)(specialConditions[1] + (char)1); cond <= specialConditions[2]; cond++)
                {
                    //MessageBox.Show(cond+"");
                    //получаем ранее сформированный translate
                    char[] translate = tmpTranslates[cond];
                    for (int i = 0; i < alph.Length; i++)
                    {
                        //встретили пустой символ
                        if (translate[i] == '~')
                        {
                            //символ алфавита соответствует modSymbol - переходим в текущ. состояние + 1
                            if (alph[i][0] == modSymbol)
                            {
                                //исключение - для последнего состояния по символу кратности совершем переход в А
                                if(cond == specialConditions[2])
                                {
                                    translate[i] = specialConditions[0];
                                    continue;
                                }
                                translate[i] = (char)(cond + (char)1);
                                continue;
                            }
                            //любой другой символ - остаёмся в состоянии
                            translate[i] = cond;
                        }
                    }
                    translates.Add(translate);
                }
                //MessageBox.Show(translates.Count + " " + conditions);
            }

            //**************************************************************
            //СЛУЧАЙ ВТОРОЙ (ГОРАЗДО ХУЖЕ) - СИМВОЛ КРАТНОСТИ ВХОДИТ В КОНЕЧНУЮ ПОДЦЕПОЧКУ
            //**************************************************************

            else if(!singleCharacter)
            {
                //Нужно узнать, сколько раз входит символ кратности в подцепочку, и уже исходя из этого уменьшить
                //количество состояний, которые идут после specialCondition[1]
                int modValueModifier = 0;
                foreach(char c in endSubstring)
                {
                    if (c == modSymbol)
                        modValueModifier++;
                }
                //отлично, мы узнали то количество символов, которое
                //должны добавить до подцепочки
                if (modValueModifier <= modValue)
                    modValueModifier = modValue - modValueModifier;
                else
                    modValueModifier = modValue * (modValueModifier / modValue + 1) - modValueModifier;
                //теперь, действуем от обратного: СНАЧАЛА добавляем нужное нам количество а, 
                //потом идём по конечной подцепочке

                //шаг 1 - формируем нужное количество состояний
                //0 - минимальная цепочка
                //1 - конечное состояние. оно всегда одно.
                //2 - цепочка кратности
                //3 - выход из цикла
                int stage = 0;
                string minStr = endSubstring;
                //минимальная цепочка
                for (int i = 0; i < modValueModifier; i++)
                    minStr = modSymbol + minStr;
                int currentLen = minStr.Length;

                //спец. состояния
                //0 - начальный символ A
                //1 - конечное состояние (конец конечной подцепочки)
                //2 - состояние, когда кратность modSymbol == modValue - modValueModifier (последнее состояние в графе)
                char[] specialConditions = new char[3] { 'A', 'A', 'A' };

                for (currentCondition = 'A'; currentCondition <= 'Z'; currentCondition++)
                {
                    //подготовка
                    conditions += currentCondition.ToString() + ",";
                    bool conditionIsEnd = false;
                    char[] translate = new char[alph.Length];

                    //изначально переход по i-тому символу невозможен из текущего состояния
                    for (int it = 0; it < alph.Length; it++)
                        translate[it] = '~';
                    //основная часть

                    if (stage == 0)
                    {
                        char curC = minStr[minStr.Length - currentLen];
                        int curCIndex = dAlphabet[curC];
                        translate[curCIndex] = (char)(currentCondition + (char)1);

                        currentLen--;
                        if (currentLen <= 0)
                        {
                            stage = 1;
                            //modValue-modValueModifier состояний для modSymbol
                            currentLen = modValue - modValueModifier;
                            //currentLen = modValueModifier;
                            //если первый символ состояния - modSymbol, то нужно обязательно это учесть
                            if (endSubstring[0] == modSymbol)
                                currentLen--;
                        }
                    }
                    else if (stage == 1)
                    {
                        stage = 2;
                        conditionIsEnd = true;
                        specialConditions[1] = currentCondition;
                    }
                    else
                    {
                        currentLen--;
                        if (currentLen <= 0)
                            stage = 3;
                    }

                    //присваивание
                    tmpTranslates.Add(currentCondition, translate);
                    if (conditionIsEnd)
                        endConditions += currentCondition.ToString() + ",";
                    //выход из цикла
                    if (stage == 3)
                    {
                        specialConditions[2] = currentCondition;
                        break;
                    }
                }

                //вышли за алфавит
                if (currentCondition > 'Z')
                {
                    MessageBox.Show("Не вышло построить ДКА - не хватило символов латинского алфавита!");
                    return;
                }

                //парсим состояния минимальной цепочки (первые valModifier)
                for (char cond = specialConditions[0]; cond < (char)(specialConditions[0] + (char)modValueModifier); cond++) 
                {
                    //получаем ранее сформированный translate
                    char[] translate = tmpTranslates[cond];
                    for (int i = 0; i < alph.Length; i++)
                    {
                        //встретили пустой символ - идём сами в себя, 
                        //так как мы тут пытаемся сделать необходимое кол-во modsymbol
                        if (translate[i] == '~')
                        {
                            translate[i] = cond;
                        }
                    }
                    translates.Add(translate);
                }

                //MessageBox.Show(specialConditions[0] + " " + specialConditions[1] + " " + specialConditions[2]);
                //парсим символы подцепочки
                //+ придётся обрабатывать случай спама одинаковых символов, следующие 3 переменные как раз для этого
                //(пример: a%3==0, конечная: bc, цепочка aaabbc должна быть валидна)
                bool symbolTriggered = false;
                if (endSubstring[0] == modSymbol)
                    symbolTriggered = true;
                char prevSymbol = endSubstring[0];
                int index = -1;
                //введём переменную текущей кратности modSymbol, для удобства
                int modSymbolCount = modValueModifier;
                char startCond = (char)(specialConditions[0] + (char)modSymbolCount);

                for (char cond = startCond; cond < specialConditions[1]; cond++) 
                {
                    index++;
                    //получаем ранее сформированный translate
                    char[] translate = tmpTranslates[cond];
                    //находим индекс символа алфавита, который на данный момент обрабатывается конечной подцепочкой
                    int currentSubIndex = 0;
                    for (int i = 0; i < alph.Length; i++)
                    {
                        if (alph[i][0] == endSubstring[cond - startCond])
                        {
                            currentSubIndex = i;
                            break;
                        }
                    }
                    //MessageBox.Show(cond + " modSymbolCount=" + modSymbolCount +" sym=" +alph[currentSubIndex][0]);
                    //проверка на возможность спама символов (если это не modSymbol)
                    //MessageBox.Show(cond + " "+ endSubstring[index]+" "+  prevSymbol+" " +!symbolTriggered);
                    if (endSubstring[index] != prevSymbol && !symbolTriggered && prevSymbol != modSymbol)
                    {
                        symbolTriggered = true;
                        //MessageBox.Show(cond + ", this:" + endSubstring[index] + ", prev:" + prevSymbol);
                        for (int i = 0; i < alph.Length; i++)
                        {
                            //cимвол конечной подстроки 
                            if (alph[i][0] == prevSymbol)
                            {
                                translate[i] = cond;
                                continue;
                            }
                            //встретили пустой символ
                            if (translate[i] == '~')
                            {
                                //обработали как обычно
                                if (alph[i][0] != modSymbol)
                                {
                                    if (modSymbolCount == modValue || (modSymbolCount > -1 && modSymbolCount <= modValueModifier))
                                        translate[i] = (char)(specialConditions[0] + (char)(modSymbolCount % modValue));
                                    else
                                        translate[i] = (char)(specialConditions[1] + (char)(modValue - modSymbolCount));
                                    continue;
                                }
                            }
                        }

                        //обработка последнего оставшегося перехода по символу кратности
                        modSymbolCount++;
                        if (modSymbolCount > modValue)
                            modSymbolCount = modSymbolCount % modValue;
                        if (modSymbolCount == modValue || (modSymbolCount > -1 && modSymbolCount <= modValueModifier))
                        {
                            for (int i = 0; i < alph.Length; i++)
                            {
                                if (translate[i] == '~')
                                {
                                    translate[i] = (char)(specialConditions[0] + (char)(modSymbolCount % modValue));
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < alph.Length; i++)
                            {
                                if (translate[i] == '~')
                                {
                                    translate[i] = (char)(specialConditions[1] + (char)(modValue - modSymbolCount));
                                }
                            }
                        }
                        modSymbolCount--;
                        if (modSymbolCount < 0)
                            modSymbolCount = modValue;

                        translates.Add(translate);
                        prevSymbol = endSubstring[index];
                        if (endSubstring[index] == modSymbol)
                            modSymbolCount++;
                        if (modSymbolCount > modValue)
                            modSymbolCount = modSymbolCount % modValue;
                        continue;
                    }

                    //если символ есть символ кратности
                    //все остальные переходы в translate заменяем переходом в другое состояние
                    if (alph[currentSubIndex][0] == modSymbol)
                    {
                        //MessageBox.Show(cond + " " + modSymbolCount + " " + modValue + " " + modValueModifier);
                        //Имеется 2 случая:
                        //1 - остаток от деления находится в пределах от нуля до modValueModifier - отправляем просто
                        //в состояние sc[0] + остаток!
                        //2 - остаток > modValueModifier - sc[1] + modValue - modsymbolcount
                        if (modSymbolCount == modValue || (modSymbolCount > -1 && modSymbolCount <= modValueModifier))
                        {
                            //MessageBox.Show(cond + " " + modSymbolCount + " " + (char)(specialConditions[0] + (char)(modSymbolCount % modValue)));
                            for (int i = 0; i < alph.Length; i++)
                            {
                                if (translate[i] == '~')
                                {
                                    translate[i] = (char)(specialConditions[0] + (char)(modSymbolCount % modValue));
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < alph.Length; i++)
                            {
                                if (translate[i] == '~')
                                {
                                    translate[i] = (char)(specialConditions[1] + (char)(modValue - modSymbolCount));
                                }
                            }
                        }
                    }
                    //иначе - переход по всему, что не символ кратности, остаётся таким же, переход по символу кратности
                    //обрабатывается отдельно
                    else
                    {
                        //MessageBox.Show(cond + " " + modSymbolCount);
                        if (modSymbolCount == modValue || (modSymbolCount > -1 && modSymbolCount <= modValueModifier))
                        {
                            for (int i = 0; i < alph.Length; i++)
                            {
                                if (translate[i] == '~')
                                {
                                    if(alph[i][0]!=modSymbol)
                                        translate[i] = (char)(specialConditions[0] + (char)(modSymbolCount % modValue));
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < alph.Length; i++)
                            {
                                if (translate[i] == '~')
                                {
                                    if (alph[i][0] != modSymbol)
                                        translate[i] = (char)(specialConditions[1] + (char)(modValue - modSymbolCount));
                                }
                            }
                        }
                        //обработка последнего оставшегося перехода по символу кратности
                        modSymbolCount++;
                        if (modSymbolCount > modValue)
                            modSymbolCount = modSymbolCount % modValue;
                        if (modSymbolCount == modValue || (modSymbolCount > -1 && modSymbolCount <= modValueModifier))
                        {
                            for (int i = 0; i < alph.Length; i++)
                            {
                                if (translate[i] == '~')
                                {
                                    translate[i] = (char)(specialConditions[0] + (char)(modSymbolCount % modValue));
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < alph.Length; i++)
                            {
                                if (translate[i] == '~')
                                {
                                    translate[i] = (char)(specialConditions[1] + (char)(modValue - modSymbolCount));
                                }
                            }
                        }
                        modSymbolCount--;
                        if (modSymbolCount < 0)
                            modSymbolCount = modValue;
                    }
                    if (endSubstring[index] == modSymbol)
                        modSymbolCount++;
                    if (modSymbolCount > modValue)
                        modSymbolCount = modSymbolCount % modValue;
                    
                    translates.Add(translate);
                    prevSymbol = endSubstring[index];
                }

                for (int ind = 0; ind < tmpTranslates[specialConditions[1]].Length; ind++)
                {
                    tmpTranslates[specialConditions[1]][ind] = specialConditions[0];
                }
                int subIndex = 0;
                for (int i = 0; i < alph.Length; i++)
                {
                    if(alph[i][0] == endSubstring[0])
                    {
                        subIndex = i;
                        break;
                    }
                }
                tmpTranslates[specialConditions[1]][subIndex] = (char)(specialConditions[0] + 1);

                //добавляем финальное состояние с пустыми переходами
                translates.Add(tmpTranslates[specialConditions[1]]);

                //заполняем, начиная с конца, поэтому нам тут пригодится стек
                Stack<char[]> stack = new Stack<char[]>();
                for (char cond = specialConditions[2]; cond > specialConditions[1]; cond--)
                {
                    //получаем ранее сформированный translate
                    char[] translate = tmpTranslates[cond];
                    for (int i = 0; i < alph.Length; i++)
                    {
                        if (alph[i][0] == modSymbol)
                        {
                            //исключение - для последнего состояния по символу кратности совершем переход в А
                            if (cond == (char)(specialConditions[1] + (char)1))
                            {
                                translate[i] = specialConditions[0];
                                continue;
                            }
                            translate[i] = (char)(cond - (char)1);
                        }
                        else
                            translate[i] = cond;
                    }
                    stack.Push(translate);
                }
                while(stack.Count != 0)
                {
                    translates.Add(stack.Pop());
                }
            }

            HashSet<char> tmp = new HashSet<char>();
            char cChar = 'A';
            foreach(char [] s in translates)
            {
                foreach(char c in s)
                {
                    if(c != cChar)
                        tmp.Add(c);
                }
                cChar = (char)(cChar + (char)1);
            }
            tmp.Remove('~');

            //есть ли избыточное состояние?
            int value = (char)(currentCondition - 'A' + 1);
            if (value > tmp.Count && !singleCharacter)
            {
                while(value != tmp.Count)
                {
                    translates.Remove(translates.ElementAt(translates.Count - 1));
                    conditions = conditions.Substring(0, conditions.Length - 2);
                    value--;
                    currentCondition--;
                }
            }

            //вышли за алфавит
            if (currentCondition > 'Z') 
            {
                MessageBox.Show("Не вышло построить ДКА - не хватило символов латинского алфавита!");
                return;
            }
            conditions = conditions.Substring(0, conditions.Length - 1);

            //нет конечных состояний
            if (endConditions.Length == 0)
            {
                MessageBox.Show("Не вышло построить ДКА - нет конечных состояний!");
                return;
            }
            endConditions = endConditions.Substring(0, endConditions.Length - 1);

            //начальное состояние - ВСЕГДА символ 'А'
            machine = new DFSM(conditions, tbAlph.Text, null, "A", endConditions);
            machine.translates = translates;

            string info = "M=({";
            string endC = "";
            foreach(KeyValuePair<char,bool> c in machine.conditions)
            {
                info += c.Key + ",";
                if (c.Value)
                    endC += c.Key + ",";
            }
            info = info.Substring(0, info.Length - 1) + "},{";
            foreach (char c in machine.alphabet)
            {
                info += c + ",";
            }
            info = info.Substring(0, info.Length - 1) + "},A,delta,{" + endC.Substring(0, endC.Length - 1) + "})";
            lbMachine.Text = info;
        }

        private void MOpenFromFile(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Language l = Serializer.Deserialize(dlg.FileName);
                string[] s = l.strings;
                tbAlph.Text = s[0];
                tbSubStr.Text = s[1];
                tbModSymbol.Text = s[2];
                tbModValue.Text = s[3];
            }
            dlg.Dispose();
        }

        private void MLoadToFile(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                string[] s = new string[4];
                s[0] = tbAlph.Text;
                s[1] = tbSubStr.Text;
                s[2] = tbModSymbol.Text;
                s[3] = tbModValue.Text;
                Language language = new Language(s);
                Serializer.Serialize(language, dlg.FileName);
            }
            dlg.Dispose();
        }

        private void StartCheck(object sender, EventArgs e)
        {
            DFSMCreate(sender, e);
            if (machine == null)
                return;

            //DialogResult result = MessageBox.Show("Сохраняем результат в файл?", 
            //    "Уточнение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            string path = null;

            //if(result == DialogResult.Yes)
            if(cbSave.Checked)
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                    path = dlg.FileName;
                else
                {
                    MessageBox.Show("Путь до файла не выбран! Отмена проверки цепочки.");
                    return;
                }
            }

                listView.Clear();
                listView.View = View.Details;

                listView.LabelEdit = false;
                listView.FullRowSelect = true;
                listView.CheckBoxes = false;
                listView.GridLines = true;
                listView.Font = new Font(listView.Font.FontFamily, 18);

                ColumnHeader columnHeader = new ColumnHeader();
                columnHeader.Text = "Результаты разбора цепочки:";
                listView.Columns.AddRange(new ColumnHeader[] { columnHeader });
                listView.Columns[0].Width = 1920;

                if (tbSeq.Text.Length > 0)
                {
                    //исходная цепочка
                    listView.Items.Add(new ListViewItem("Исходная цепочка: " + tbSeq.Text));
                    //формируем вывод по цепочкам в виде массива строк
                    string[] strings = FormChainSteps(tbSeq.Text);
                    foreach (string s in strings)
                    {
                        //добавляем их в список
                        listView.Items.Add(new ListViewItem(s));
                    }
                }
                else if (machine.conditions[machine.startCondition])
                    listView.Items.Add(new ListViewItem("Начальное состояние - конечное, пустая цепочка возможна"));
                else
                    listView.Items.Add(new ListViewItem("Начальное состояние - не конечное, пустая цепочка невозможна"));

            //если надо - сохраняем в файл
            //if (result == DialogResult.Yes)
            if(cbSave.Checked)
            {
                string[] strings = new string[listView.Items.Count];
                int i = 0;
                foreach(ListViewItem item in listView.Items)
                {
                    strings[i] = item.Text;
                    i++;
                }
                File.WriteAllLines(path, strings);
            }
            MessageBox.Show(listView.Items[listView.Items.Count - 1].Text);
        }

        private string[] FormChainSteps(string seq)
        {
            List<string> strings = new List<string>();

            char currentCondition = machine.startCondition;

            for (int curIndex = 0; curIndex < seq.Length; curIndex++)
            {
                strings.Add(String.Format("({0}, {1})", currentCondition, seq.Substring(curIndex)));

                if (!machine.alphabet.Contains(seq[curIndex]) && seq[curIndex] != '~')
                {
                    strings.Add(String.Format("Ошибка - символ {0} не представлен в алфавите! Цепочку построить с помощью данного ДКА невозможно.", seq[curIndex]));
                    return strings.ToArray();
                }

                //индекс текущего состояния
                int cIndex = 0;
                foreach (char ch in machine.conditions.Keys)
                {
                    if (ch == currentCondition)
                        break;
                    cIndex++;
                }

                //индекс текущего символа алфавита
                int sIndex = 0;
                foreach (char ch in machine.alphabet)
                {
                    if (ch == seq[curIndex])
                        break;
                    sIndex++;
                }

                char[] chars = machine.translates.ElementAt(cIndex);
                if (chars[sIndex] == '~')
                {
                    strings.Add(String.Format("В состоянии {0} нет перехода по символу {1}. Цепочку построить с помощью данного ДКА невозможно.",
                        currentCondition, seq[curIndex]));
                    return strings.ToArray();
                }
                currentCondition = chars[sIndex];

            }
            strings.Add(String.Format("({0}, lambda)", currentCondition));

            if (machine.conditions[currentCondition])
                strings.Add("Текущее состояние - конечно. Цепочка валидна");
            else
                strings.Add("Текущее состояние - не конечно. Цепочку построить с помощью данного ДКА невозможно.");

            return strings.ToArray();
        }
    }

    public class MyException : Exception
    {
        public MyException(string s) : base(s)
        { }
    }

    [Serializable()]
    public class Language
    {
        public string[] strings;

        public Language(string[] s)
        {
            strings = s;
        }
    }

    public class Serializer
    {
        public static void Serialize(Language field, string path)
        {
            Stream stream = File.Open(path, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, field);
            stream.Close();
        }

        public static Language Deserialize(string path)
        {
            Stream stream = File.Open(path, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();

            Language field = (Language)formatter.Deserialize(stream);
            stream.Close();

            return field;
        }
    }

    //deterministic finite-state machine
    [Serializable()]
    public class DFSM
    {
        //key - состояние (большая буква обычно)
        //value - true - конечное, иначе - нет
        public Dictionary<char, bool> conditions;
        //алфавит языка
        public HashSet<char> alphabet;
        //начальное состояние - одна буква!
        public char startCondition;
        //OMEGALUL функция переходов, по строкам - состояния, по столбам - алфавитыне символы
        //символ состояния всегда максимум (да и минимум) только один, так как это ДКА всё-таки
        public HashSet<char[]> translates = null;


        //~~~~~~AB
        //~A~B~C~
        //~~~~~~~~~~~~~   ТИЛЬДА - ЭТО ЗНАЧИТ НЕТ ПЕРЕХОДА ПО i-тому СИМВОЛУ !!!
        //~~~ABC~~~
        //~ABCABC


        public static bool symbolCanBeACondition(char c)
        {
            return c >= 'A' && c <= 'Z';
        }

        public static bool symbolCanBeInAlphabet(char c)
        {
            //не пробел, не запятая, переход на новую строку, не тильда
            //или набор от A до Z
            return ((c >= '!' && c <= '@') || (c >= '[' && c < '~')) && c != ',' && c != '\n';
        }

        public DFSM(string conds, string alph, string transl, string startC, string endC)
        {
            conditions = new Dictionary<char, bool>();
            alphabet = new HashSet<char>();
            translates = new HashSet<char[]>();

            //парсим старт кондишн
            if (startC.Length != 1)
            {
                throw new MyException("Начальный символ - это 1 символ, не 0 символов... как бы");
            }
            if (symbolCanBeACondition(startC[0]))
                startCondition = startC[0];
            else
                throw new MyException("Неверный начальный символ");

            //парсим все кондишн
            string[] tmp = conds.Split(',');
            foreach (string s in tmp)
            {
                if (s.Length != 1)
                    throw new MyException("Символ состояния должен иметь длину 1.");

                if (symbolCanBeACondition(s[0]) && !conditions.ContainsKey(s[0]))
                    conditions.Add(s[0], false);
                else
                    throw new MyException("В состояниях могут присутствовать только символы от А до Z, причем единожды!");
            }

            //получаем алфавит
            tmp = alph.Split(',');
            foreach (string s in tmp)
            {
                if (s.Length != 1)
                    throw new MyException("Символ алфавита должен иметь длину 1.");

                if (symbolCanBeInAlphabet(s[0]) && !conditions.ContainsKey(s[0]))
                    alphabet.Add(s[0]);
                else
                    throw new MyException(String.Format("В алфавите не могут присутствовать невидимые символы, а также символы состояний! ({0})", s[0]));
            }

            //узнаем, какие состояния конечны
            tmp = endC.Split(',');
            foreach (string s in tmp)
            {
                if (s.Length != 1)
                    throw new MyException("Символ состояния должен иметь длину 1.");

                if (conditions.ContainsKey(s[0]))
                    conditions[s[0]] = true;
                else
                    throw new MyException("В алфавите состояний нет данного символа: " + s[0]);
            }

            //парсим (о, боже) транслэйшены
            //формат входной строки НКА: 
            //символ,символ,символ символ,символ\nсимвол,символ символ,символ
            //для текущего ДКА:
            //символ символ\nсимвол символ
            //и так далее

            //UPD ::: УБРАЛ ВСЕ ИСПОЛЬЗОВАНИЯ ДАННОГО ПОДКОНСТРУКТОРА, ИНИЦИАЛИЗАЦИЯ ПРОИСХОДИТ
            //ИСКЛЮЧИТЕЛЬНО В FORM2 СРАЗУ ЖЕ В НУЖНЫЙ КОНТЕЙНЕР БЕЗ ФОРМИРОВАНИЯ ПОДСТРОКИ

            //возврат для конструктора без переходов
            if (transl == null)
                return;

            tmp = transl.Split('\n');

            if (tmp.Length != conditions.Count)
                throw new MyException("Функция переходов должна иметь то же количество строк, что и состояний");

            foreach (string s in tmp)
            {
                string[] tmp1 = s.Split(' ');

                if (tmp1.Length != alphabet.Count)
                    throw new MyException("Функция переходов должна иметь то же количество столбцов, что и символов алфавита");

                char[] set = new char[tmp1.Length];
                int index = 0;

                foreach (string st in tmp1)
                {
                    if (st.Length != 1)
                        throw new MyException("Символ состояния должен иметь длину 1.");

                    //если символ - состояние, то добавляем, даже если есть в сете - неважно
                    if (conditions.ContainsKey(st[0]))
                        set[index] = st[0];
                    else
                        throw new MyException("В алфавите состояний нет данного символа.");
                    index++;
                }
                translates.Add(set);
            }

        }

    private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
