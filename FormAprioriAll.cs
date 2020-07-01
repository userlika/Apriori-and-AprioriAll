using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AprioriDiploma
{
    public partial class FormAprioriAll : Form
    {
        TransactionDB tidDataBase;
        int supportThreshold; // минимальное значение поддержки

        public FormAprioriAll(TransactionDB tidDB)
        {
            InitializeComponent();
            textBox_minSuppSeq.Text = "2";
            this.tidDataBase = tidDB;
        }

        private void button_CalculateSeq_Click(object sender, EventArgs e)
        {
            flowLayoutPanelSeq.Controls.Clear();
            flowLayoutPanelSeq.AutoScroll = true;
            //flowLayoutPanelSeq.Dock = System.Windows.Forms.DockStyle.None;
            //flowLayoutPanelSeq.AutoSize = true;

            supportThreshold = Convert.ToInt32(textBox_minSuppSeq.Text);
            AprioriAll aprAll = new AprioriAll(tidDataBase, supportThreshold);

            flowLayoutPanelSeq.Controls.Add(AddFlowPanel(AddDGWTable(aprAll.SortPhase()),"Sort"));
            flowLayoutPanelSeq.Controls.Add(AddFlowPanel(AddDGWLitemset(aprAll.LitemsetPhase(),false),"LitemSet"));
            flowLayoutPanelSeq.Controls.Add(AddFlowPanel(AddDGWLitemset(aprAll.TransformationPhase(),true), "Transformation"));
            flowLayoutPanelSeq.Controls.Add(AddFlowPanel(AddDGWTransformed(aprAll.TransformationPhase2()), "Transformation2.0"));
            flowLayoutPanelSeq.Controls.Add(AddFlowPanel(AddDGWLitemset(aprAll.SequencePhase(), false), "Sequence"));
            flowLayoutPanelSeq.Controls.Add(AddFlowPanel(AddDGWLitemset(aprAll.MaximalPhase(), false), "Maximal"));

        }

        private FlowLayoutPanel AddFlowPanel(DataGridView dgw1, string phasename)
        {
            Label labelK = new Label();
            labelK.AutoSize = true;
            labelK.Text = phasename + " Phase";
            labelK.Text += Environment.NewLine + "";
            FlowLayoutPanel flp = new FlowLayoutPanel();
            flp.BackColor = Color.BurlyWood;
            flp.FlowDirection = FlowDirection.LeftToRight;
            flp.AutoSize = true;
            //flp.WrapContents = true;     
            //flp.AutoScroll = true;
            flp.Controls.Add(labelK);
            flp.Controls.Add(dgw1);

            return flp;
        }

        private DataGridView AddDGWTable(Dictionary<int, List<List<string>>> transactionTable)
        {
            DataGridView dgw = new DataGridView();
            dgw.DataSource = null;
            dgw.AutoGenerateColumns = false;
            dgw.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgw.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgw.AutoSize = true;
            dgw.ColumnCount = 2;
            dgw.Columns[0].Name = "CID";
            dgw.Columns[1].Name = "Последовательность набора покупок";

            int i = 0;
            foreach (var KeyValue in transactionTable)
            {
                dgw.Rows.Add(); // добавить строку, чтобы можно было обращаться к уже существующей строке по индексу
                dgw.Rows[i].Cells["CID"].Value = KeyValue.Key;
                foreach (var value in KeyValue.Value)
                {
                    dgw.Rows[i].Cells["Последовательность набора покупок"].Value += "{";
                    foreach(string str in value)
                    {
                        dgw.Rows[i].Cells["Последовательность набора покупок"].Value += str + " ";
                    }
                    dgw.Rows[i].Cells["Последовательность набора покупок"].Value += "}";
                }
                i++;
            }
            return dgw;
        }

        private DataGridView AddDGWLitemset(Dictionary<List<string>, int> lit, bool isTransformationPhase)
        {
            DataGridView dgw = new DataGridView();
            dgw.DataSource = null;
            dgw.AutoGenerateColumns = false;
            dgw.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgw.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgw.AutoSize = true;
            dgw.ColumnCount = 2;
            dgw.Columns[0].Name = "Large Itemsets";
            dgw.Columns[1].Name = "Support";
            if (isTransformationPhase)
                dgw.Columns[1].Name = "Mapped To";

            int i = 0;
            foreach (var KeyValue in lit)
            {
                dgw.Rows.Add(); // добавить строку, чтобы можно было обращаться к уже существующей строке по индексу
                foreach(var value in KeyValue.Key)
                {
                    dgw.Rows[i].Cells["Large Itemsets"].Value += value + " ";
                }
                if(!isTransformationPhase)
                    dgw.Rows[i].Cells["Support"].Value = KeyValue.Value;
                else
                    dgw.Rows[i].Cells["Mapped To"].Value = KeyValue.Value;


                i++;
            }
            return dgw;
        }
        private DataGridView AddDGWTransformed(Dictionary<int, List<List<int>>> transform)
        {
            DataGridView dgw = new DataGridView();
            dgw.DataSource = null;
            dgw.AutoGenerateColumns = false;
            dgw.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgw.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgw.AutoSize = true;
            dgw.ColumnCount = 2;
            dgw.Columns[0].Name = "CID";
            dgw.Columns[1].Name = "Transformed sequences";

            int i = 0;
            foreach (var KeyValue in transform)
            {
                dgw.Rows.Add(); // добавить строку, чтобы можно было обращаться к уже существующей строке по индексу
                dgw.Rows[i].Cells["CID"].Value = KeyValue.Key;
                foreach (var value in KeyValue.Value)
                {
                    dgw.Rows[i].Cells["Transformed sequences"].Value += "{";
                    foreach (int str in value)
                    {
                        dgw.Rows[i].Cells["Transformed sequences"].Value += str + " ";
                    }
                    dgw.Rows[i].Cells["Transformed sequences"].Value += "}";
                }
                i++;
            }
            return dgw;
        }
    }

    /*----------------------------------CLASS ItemSet------------------------------*/
    class ItemSetSeq : Dictionary<int, List<List<string>>>
    {
        public string itemsetType { get; set; } // кандидат или частый набор
        public int support { get; set; }
    }

    /*----------------------------------CLASS AprioriAll------------------------------*/
    public class AprioriAll
    {
        int minSupp;
        int transCount; // количество транзакций
        int cidCount = 0; // количество покупателей

        Dictionary<int, List<string>> tidDictionary;
        Dictionary<int, List<int>> cidDictionary; // Key:cidID, Value: список номеров транзакций у покупателя
        Dictionary<int, List<List<string>>> sortedSeqDict; // отсортированный словарь 
        Dictionary<int, List<List<int>>> sortedIDsDict; // отсортированная преобразованная последовательность
        Dictionary<List<string>, int> numSeqDict; // одноэлементные последовательности и их идентификатор
        Dictionary<List<string>, int> Litemsets;
        Dictionary<List<string>, int> sequencePhaseDict;
        ItemSetSeq firstItemSet;

        public AprioriAll(TransactionDB tidDataBase, int min_supp)
        {
            tidDictionary = tidDataBase.tidDict;
            cidDictionary = tidDataBase.cidDict;
            sortedSeqDict = new Dictionary<int, List<List<string>>>();
            numSeqDict = new Dictionary<List<string>, int>();
            Litemsets = new Dictionary<List<string>, int>();
            sequencePhaseDict = new Dictionary<List<string>, int>();
            sortedIDsDict = new Dictionary<int, List<List<int>>>();

            sortedSeqDict = SortPhase();
            
              
            minSupp = min_supp;
            firstItemSet = new ItemSetSeq();
            transCount = tidDataBase.transCount;

        }

        public Dictionary<int, List<List<string>>> SortPhase() // Фаза сортировки
        {
            Dictionary<int, List<List<string>>> sequencesDict = new Dictionary<int, List<List<string>>>();
            
            foreach(var kvp in cidDictionary)
            {
                List<List<string>> sequence = new List<List<string>>();
                foreach (var tid_num in kvp.Value)
                {
                    sequence.Add(tidDictionary[tid_num]);
                }               
                
                sequencesDict.Add(kvp.Key, sequence);
                cidCount++;
            }

            return sequencesDict;
        }

        public Dictionary<List<string>, int> LitemsetPhase() // Фаза отбора кандидатов
        {
            Dictionary<List<string>, int> litemsets = new Dictionary<List<string>, int>();
            foreach (var cidSeq in sortedSeqDict)
            {
                foreach (var seq in cidSeq.Value)
                {
                    List<string> subSequences = new List<string>();
                    foreach (var subseq in seq)
                    {
                        subSequences.Add(subseq);
                    }

                    var cand = GenerateCandidates(subSequences); // все возможные кандидаты для каждой последовательности
                    var large = LargeItemSets(cand);

                    foreach(var kvp in large)
                    {
                        bool isConsist = false;
                        foreach (var key in litemsets.Keys)
                        {
                            
                            if (key.SequenceEqual(kvp.Key))
                                isConsist = true;
                        }
                        if (!isConsist)
                            litemsets.Add(kvp.Key, kvp.Value);
                    }
     
                }
            }

            return litemsets;
        }

        List<List<string>> GenerateCandidates(List<string> seq)
        {
            List<List<string>> cand = new List<List<string>>();
            List<string> l = seq;
            var test = l.GetAllOptionsWithoutRepetition();
            foreach (var strs in test)
            {
                cand.Add(strs.ToList());
            }

            return cand;

        }

        Dictionary<List<string>, int> LargeItemSets(List<List<string>> candidates)
        {
            Dictionary<List<string>, int> lit = new Dictionary<List<string>, int>();

            foreach (var sets in candidates)
            {
                int support = SupportCount(sets, sortedSeqDict);
                if (support > minSupp)
                    lit.Add(sets, support);
            }
            return lit;
        }

        public Dictionary<List<string>, int> TransformationPhase() // Фаза трансформации
        {
            var litemsetsWithID = LitemsetPhase();
            List<List<string>> litList = new List<List<string>>(); 
            foreach(var item in litemsetsWithID)
            {
                litList.Add(item.Key);
            }
            for(int i = 0; i < litList.Count; i++)
            {
                numSeqDict.Add(litList[i], i);
            }

            return numSeqDict;
        }

        public Dictionary<int, List<List<int>>> TransformationPhase2() // Фаза трансформации
        {
            var litemsetsWithID = LitemsetPhase();
            List<List<string>> litList = new List<List<string>>(); // список наборов-кандидатов
            //Dictionary<int, List<List<int>>> transformed = new Dictionary<int, List<List<int>>>();
            foreach (var item in litemsetsWithID)
            {
                litList.Add(item.Key);
            }
            numSeqDict = new Dictionary<List<string>, int>(); //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            for (int i = 0; i < litList.Count; i++)
            {
                numSeqDict.Add(litList[i], i); // кандидат - идентификатор кандидата
            }

            foreach(var cidSeq in sortedSeqDict)
            {
                List<List<int>> selectedSeq = new List<List<int>>(); // отобранные преобразованные последовательности (замененные идентификатором)

                foreach(var seq in cidSeq.Value)
                {
                    List<int> temp = new List<int>(); // отобранные наборы внутри одной последовательности
                    foreach(var litem in numSeqDict)
                    {
                        var isSub = litem.Key.All(seq.Contains);

                        if(isSub)
                            temp.Add(litem.Value);

                        //foreach (var el in litem.Key) 
                        //{
                        //    if (seq.Contains(el))
                        //        temp.Add(litem.Value);
                        //}

                    }
                    selectedSeq.Add(temp);
                }

                sortedIDsDict.Add(cidSeq.Key, selectedSeq);
            }
            return sortedIDsDict;
        }

        public Dictionary<List<string>, int> SequencePhase()
        {
            
            List<string> identificators = new List<string>(); // список идентификаторов в строковом представлении
            foreach(var kvp in numSeqDict)
            {
                identificators.Add(Convert.ToString(kvp.Value));
            }

            List<List<string>> candidates = new List<List<string>>();
            var temp = identificators.GetAllOptionsWithoutRepetition();
            foreach (var strs in temp)
            {
                candidates.Add(strs.ToList());
            }
            
            foreach (var sets in candidates)
            {
                int support = SupportCount(sets,sortedIDsDict);
                if (support > minSupp)
                    sequencePhaseDict.Add(sets, support);
            }
            return sequencePhaseDict;

        }

        public Dictionary<List<string>, int> MaximalPhase()
        {
            Dictionary<List<string>, int> maxSequences = new Dictionary<List<string>, int>();
            Dictionary<List<string>, int> tempSequences = new Dictionary<List<string>, int>();
            int maxLength = 0; // длина максимальной последовательности
            foreach(var kvp in sequencePhaseDict)
            {
                int listLength = kvp.Key.Count;
                maxLength = listLength;
                if (listLength > maxLength)
                    maxLength = listLength;
            }
            foreach (var setMapped in numSeqDict)
            {
                List<string> forKeys = new List<string>();
                string key = Convert.ToString(setMapped.Value);
                forKeys.Add(key);
                
                foreach(var kvp in sequencePhaseDict)
                {
                    if (kvp.Key.Contains(key))
                        tempSequences.Add(setMapped.Key, kvp.Value);
                }
                //if (sequencePhaseDict.ContainsKey(forKeys))
                //    tempSequences.Add(setMapped.Key, sequencePhaseDict[forKeys]);

            }
            
            foreach(var kvp in tempSequences)
            {
                bool isContainsSeq = false; 
                foreach(var kvp2 in tempSequences)
                {
                    if (!kvp.Key.Equals(kvp2.Key))
                    {
                        if (IsSubset(kvp.Key, kvp2.Key))
                        {
                            isContainsSeq = true;
                            break;
                        }
                            
                    }
                }
                if (!isContainsSeq)
                    maxSequences.Add(kvp.Key, kvp.Value);
            }

            return maxSequences;
        }

        // проверка, содержится ли поднабор в наборе
        bool IsSubset(List<string> subset, List<string> set)
        {
            int counter = 0;
            int strcount = 0;
            foreach (var str in subset)
            {
                strcount++;

                if (set.Contains(str)) counter++;

            }
            if (counter == strcount) return true;
            else return false;
        }

        // расчет поддержки кандидата

        int SupportCount(List<string> set, Dictionary<int, List<List<string>>> Dict)
        {
            int supp = 0;

            foreach (var KeyValue in Dict)
            {
                foreach(var value in KeyValue.Value)
                {
                    if (IsSubset(set, value))
                    {
                        supp++;
                        break;
                    }

                }
            }

            return supp;
        }
        int SupportCount(List<string> set, Dictionary<int, List<List<int>>> Dict)
        {
            int supp = 0;

            foreach (var KeyValue in Dict)
            {
                
                List<List<string>> tempListList = new List<List<string>>();
                foreach (var value in KeyValue.Value)
                {
                    var temp = value;
                    List<string> tempList = new List<string>();
                    foreach (var el in value)
                    {
                        tempList.Add(Convert.ToString(el));
                    }// конвертирование числа в строку
                    tempListList.Add(tempList);
                }

                int equalK = 0;
                int k = 0;
                int seqId = 0; // индекс последовательности, в которой найден набор
                foreach (var id in set)
                {
                    List<string> idList = new List<string>();
                    idList.Add(id); // конвертирование идентификатора в список, чтобы передать в isSubset        
                    k++; //количество элементов в последовательности
                    
                    for (int i = seqId; i < KeyValue.Value.Count; i++)
                    {
                        if (IsSubset(idList, tempListList[i]))
                        {
                            equalK++;
                            seqId = i+1;
                            break;
                        }

                    }

                }
                if (equalK == k)
                {
                    supp++;
                }

            }

            return supp;
        }
    }

    public static class Helper
    {
        // Возвращает список списков элементов переданного списка без повторений

        public static IList<IList<T>> GetAllOptionsWithoutRepetition<T>(this List<T> list)
        {
            // для хранения двоичных чисел
            var templates = new List<string>();
            // считаю сколько чисел будет в ответе и перевожу их в двоичные, начиная с 0
            for (int i = 0; i < Math.Pow(2, list.Count); i++)
            {
                var bin = Convert.ToString(i, 2);
                // так же дополняю нулями спереди для полноты двоичного числа
                templates.Add($"{"0".Repeat(list.Count - bin.Length)}{bin}");
            }

            // будущий результат
            var resAll = new List<IList<T>>();

            foreach (var template in templates)
            {
                var res = new List<T>();
                for (int ch = 0; ch < template.Length; ch++)
                {
                    // если что-то должно стоять на этом месте, то ставлю (в соответсвиии с положением во входящем массиве)
                    if (template[ch] == '1') res.Add(list[ch]);
                }
                // если массив не пустой, то добавляю к основному результату
                if (res.Count > 0) resAll.Add(res);
            }

            // возвращаю реузльтат, слортируя его ля красивости
            return resAll.OrderBy(s => s.Count).ThenBy(s => String.Join(" ", s)).ToList();
        }

        /// Возвращает n раз одну и ту же строку
        public static string Repeat(this string str, int num)
        {
            string result = string.Empty;
            for (int i = 0; i < num; i++)
            {
                result += str;
            }
            return result;
        }
    }

}
