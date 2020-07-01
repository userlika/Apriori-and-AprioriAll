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
    public partial class FormApriori : Form
    {
        int supportThreshold; // минимальное значение поддержки
        TransactionDB transact;

        public FormApriori(TransactionDB _transact)
        {
            InitializeComponent();
           // transact = new TransactionDB();
            this.transact = _transact;
            textBox_Support.Text = "2";
        }

        private void FormApriori_Load(object sender, EventArgs e)
        {

        }

        private void button_CalculateApriori_Click(object sender, EventArgs e)
        {
            flowLayoutPanel_Tables.Controls.Clear();

            supportThreshold = Convert.ToInt32(textBox_Support.Text);

            if (supportThreshold < 0)
                MessageBox.Show("Значение поддержки не может быть отрицательным! ");

            Apriori apriori = new Apriori(transact, supportThreshold);
            ItemSet firstItemSetC = new ItemSet(); // одноэлементный набор-кандидат
            ItemSet firstItemSetL = new ItemSet(); // одноэлементный частый набор

            firstItemSetC = apriori.FirstGenCandidateCounting(transact.listOfProducts);
            firstItemSetL = apriori.FirstGenLargeItemSet(firstItemSetC);

            //flowLayoutPanel_Tables.Controls.Add(AddDGWItemSet(firstItemSetC));
            //flowLayoutPanel_Tables.Controls.Add(AddDGWItemSet(firstItemSetC));
            flowLayoutPanel_Tables.Controls.Add(AddFlowPanel(AddDGWItemSet(firstItemSetC), AddDGWItemSet(firstItemSetL), 1));

            bool isNext;
            int k = 2;
            var prevItemSetL = new ItemSet();
            do
            {
                
                isNext = false;
                List<AssociationRule> rules = new List<AssociationRule>();

                if (k == 2)
                {
                    var secondItemSetC = apriori.CandidateGeneration(firstItemSetL);
                    var secondItemSetL = apriori.LargeItemSet(secondItemSetC);
                    prevItemSetL = secondItemSetL;
                    rules = apriori.GetListOfRules(prevItemSetL);
                    //flowLayoutPanel_Tables.Controls.Add(AddGroupBox(AddDGWItemSet(secondItemSetC), AddDGWItemSet(secondItemSetL)));
                    ////flowLayoutPanel_Tables.Controls.Add(AddGroupBox(AddDGWItemSet(secondItemSetL)));

                    flowLayoutPanel_Tables.Controls.Add(AddFlowPanel(AddDGWItemSet(secondItemSetC), AddDGWItemSet(secondItemSetL), AddDGWRules(rules),k));
                    //flowLayoutPanel_Tables.Controls.Add(AddPanelItems(AddDGWItemSet(secondItemSetL)));
                    //flowLayoutPanel_Tables.Controls.Add(AddDGWItemSet(secondItemSetC));
                    //flowLayoutPanel_Tables.Controls.Add(AddDGWItemSet(secondItemSetL));

                    k++;
                    isNext = true;
                }
                else
                {
                    var nextItemSetC = apriori.CandidateGeneration(prevItemSetL);
                    var nextItemSetL = apriori.LargeItemSet(nextItemSetC);
                    prevItemSetL = nextItemSetL;
                    rules = apriori.GetListOfRules(prevItemSetL);
                    //flowLayoutPanel_Tables.Controls.Add(AddDGWItemSet(nextItemSetC));
                    if (prevItemSetL.Count > 0)
                    {
                        flowLayoutPanel_Tables.Controls.Add(AddFlowPanel(AddDGWItemSet(nextItemSetC), AddDGWItemSet(nextItemSetL), AddDGWRules(rules), k));
                        //flowLayoutPanel_Tables.Controls.Add(AddDGWItemSet(nextItemSetL));
                        k++;
                        isNext = true;
                    }
                    else
                    {
                        isNext = false;
                    }
                }

            } while (isNext);           

        }

        //private GroupBox AddGroupBox(DataGridView dgw1, DataGridView dgw2, int k)
        //{
        //    GroupBox gb = new GroupBox();
        //    var size = dgw1.Size;
        //    //groupBox.Size = new Size(200, 50);
        //    gb.Size = size;
        //    gb.BackColor = Color.Aqua;
        //    gb.AutoSize = true;
        //    gb.Text = Convert.ToString(k) + "элементный набор";
        //    gb.FlatStyle = FlatStyle.Standard;
        //    gb.Controls.Add(dgw1);
        //    gb.Controls.Add(dgw2);          

        //    return gb;
        //}
        private FlowLayoutPanel AddFlowPanel(DataGridView dgw1, DataGridView dgw2, int k)
        {
            Label labelK = new Label();
            labelK.AutoSize = true;
            labelK.Text = Convert.ToString(k) + "-элементный";
            labelK.Text += Environment.NewLine + "набор";
            FlowLayoutPanel flp = new FlowLayoutPanel();
            flp.BackColor = Color.Honeydew;
            flp.FlowDirection = FlowDirection.LeftToRight;
            flp.AutoSize = true;
            //flp.WrapContents = true;     
            //flp.AutoScroll = true;
            flp.Controls.Add(labelK);
            flp.Controls.Add(dgw1);
            flp.Controls.Add(dgw2);
            return flp;
        }

        private FlowLayoutPanel AddFlowPanel(DataGridView dgw1, DataGridView dgw2, DataGridView dgw3, int k)
        {
            Label labelK = new Label();
            labelK.AutoSize = true;
            labelK.Text = Convert.ToString(k) + "-элементный";
            labelK.Text += Environment.NewLine + "набор";

            Label labelRules = new Label();
            labelRules.AutoSize = true;
            labelRules.Text = "Ассоциативные";
            labelRules.Text += Environment.NewLine + " правила";
 
            FlowLayoutPanel flp = new FlowLayoutPanel();
            flp.BackColor = Color.Honeydew;
            flp.FlowDirection = FlowDirection.LeftToRight;
            flp.AutoSize = true;
            //flp.WrapContents = true;  
            //flp.AutoScroll = true;
            flp.Controls.Add(labelK);
            flp.Controls.Add(dgw1);
            flp.Controls.Add(dgw2);
            flp.Controls.Add(labelRules);
            flp.Controls.Add(dgw3);
            return flp;
        }

        //private Panel AddPanelItems(DataGridView dgw1, DataGridView dgw2)
        //{
        //    Panel pan = new Panel();
        //    pan.BorderStyle = BorderStyle.Fixed3D;
        //    pan.BackColor = Color.Blue;
        //    pan.Controls.Add(dgw1);
        //    pan.Controls.Add(dgw2);
        //    return pan;
        //}


        private DataGridView AddDGWItemSet(ItemSet itemset)
        {
            DataGridView dgw = new DataGridView();
            dgw.DataSource = null;
            dgw.AutoGenerateColumns = false;
            dgw.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgw.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgw.ColumnCount = 2;
            dgw.Columns[0].Name = "Product";
            dgw.Columns[1].Name = "Support";

            int i = 0;
            foreach (KeyValuePair<List<string>, int> KeyValue in itemset)
            {
                dgw.Rows.Add(); // добавить строку, чтобы можно было обращаться к уже существующей строке по индексу

                foreach (var el in KeyValue.Key)
                {
                    dgw.Rows[i].Cells["Product"].Value += el + " ";
                }

                dgw.Rows[i].Cells["Support"].Value = KeyValue.Value;
                i++;
            }
            return dgw;
        }

        private DataGridView AddDGWRules(List<AssociationRule> rules)
        {
            DataGridView dgw = new DataGridView();
            dgw.DataSource = null;
            dgw.AutoGenerateColumns = false;
            dgw.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgw.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgw.ColumnCount = 3;
            dgw.Columns[0].Name = "Rule";
            dgw.Columns[1].Name = "Rule support";
            dgw.Columns[2].Name = "Confidence";
            int i = 0;
            foreach (var rule in rules)
            {
                dgw.Rows.Add(); // добавить строку, чтобы можно было обращаться к уже существующей строке по индексу

                dgw.Rows[i].Cells["Rule"].Value += rule.ruleString;

                dgw.Rows[i].Cells["Rule Support"].Value = rule.support + " %";

                dgw.Rows[i].Cells["Confidence"].Value = rule.confidence + "%";
                i++;
            }
            return dgw;
        }
    }

    /*----------------------------------CLASS ItemSet------------------------------*/
    class ItemSet: Dictionary<List<string>, int>
    {
        public string itemsetType { get; set; } // кандидат или частый набор
        public int support { get; set; }
    }

    /*----------------------------------CLASS AssociationRules------------------------------*/
    public class AssociationRule
    {
        public string ruleString { get; set; }
        public double support { get; set; }
        public double confidence { get; set; }
    }

    /*----------------------------------CLASS Apriori------------------------------*/
    class Apriori
    {
        /*--------------------------variables--------------------------------------*/
        int transCount; // количество транзакций
        int minSupp; // минимальная поддержка

        TransactionDB transactions;
        List<ItemSet> itemSetList; // список всех сгенерированных наборов
        Dictionary<string, int> firstGen; // одноэлементный набор
        ItemSet firstItemSCandidates; // одноэлементные наборы (кандидаты)

        Dictionary<int, List<List<string>>> sequenceDict; // словарь для секвенциального анализа

        /*-------------------------methods----------------------------------------*/
        public Apriori(TransactionDB trans, int min_supp)
        {
            itemSetList = new List<ItemSet>();
            transactions = new TransactionDB();
            this.transactions = trans;
            this.minSupp = min_supp;
            this.transCount = trans.transCount-1;
        }

        public Apriori(Dictionary<int, List<List<string>>> seqDict, int cidCount, int min_supp)
        {
            this.sequenceDict = seqDict;
            this.transCount = cidCount;
            this.minSupp = min_supp;

        }

        //Генерация одноэлементного набора-кандидата
        public ItemSet FirstGenCandidateCounting(List<string> allProducts)
        {
            firstGen = new Dictionary<string, int>();
            List<string> tempList = new List<string>(); // вспомогательный список для просмотра значений в словаре через значение
            List<string> itemToList;
            firstItemSCandidates = new ItemSet();

            foreach (string item in allProducts)
            {
                itemToList = new List<string>();
                itemToList.Add(item);
                int itemCount = 0;
                tempList.Clear();

                foreach (KeyValuePair<int, List<string>> KeyValue in transactions.tidDict)
                {
                    tempList = KeyValue.Value; // набор из каждой транзакции
                    if (tempList.Contains(item)) itemCount++;

                }

                firstItemSCandidates.Add(itemToList, itemCount);
            }
            itemSetList.Add(firstItemSCandidates);
            return firstItemSCandidates;
        }

        // генерация одноэлементного частого набора
        public ItemSet FirstGenLargeItemSet(ItemSet firstC)
        {
            ItemSet firstL = new ItemSet();

            foreach (var item in firstC)
            {
                if (SupportCount(item.Key) > minSupp)
                    firstL.Add(item.Key, item.Value);
            }

            //foreach (var item in firstL)
            //{
            //    itemsList.Add(item.Key.ToString());
            //}
            return firstL;
        }

        // Генерация множества k-элементных кандидатов
        public ItemSet CandidateGeneration(ItemSet prevItemSet)
        {
            ItemSet nextCandidate = new ItemSet();

            List<string> nextItemList = new List<string>();
            List<List<string>> ItemSetList = new List<List<string>>(); //список предыдущих наборов, без первого элемента
            //List<IEnumerable<string>> candidate = CandidateGeneration(itemsList, k).ToList();
            int k = 0;
            foreach (var itemset in prevItemSet)
            {
                ItemSetList.Add(itemset.Key); // !!! добавить пропуск первого элемента
                k = itemset.Key.Count; // количество элементов в предыдущем списке
            }
                
  
            if (k == 1)
            {
                for (int i = 0; i < prevItemSet.Count - 1; i++)
                {
                    for (int j = i + 1; j < prevItemSet.Count; j++)
                    {
                        if (!ItemSetList[i].Equals(ItemSetList[j]))
                        {
                            var tempList = UnionOfItems(ItemSetList[i], ItemSetList[j]);
                            int supp = SupportCount(tempList);

                            nextCandidate.Add(tempList, supp);
                        }
                        else continue;
                    }
                }
            }
            else
            {
                for (int i = 0; i < prevItemSet.Count - 1; i++)
                {
                    for (int j = i + 1; j < prevItemSet.Count; j++)
                    {
                        if (!ItemSetList[i].Equals(ItemSetList[j]))
                        {
                            bool isSub = false;
                            for (int l = 0; l < k - 1; l++)
                            {
                                if (ItemSetList[i][l] == ItemSetList[j][l])
                                    isSub = true;
                                else
                                {
                                    isSub = false;
                                    break;
                                }
                                                                  
                            }
                            if (isSub)
                            {
                                if (ItemSetList[i].Count == ItemSetList[j].Count)
                                {
                                    var tempList = CombineItems(ItemSetList[i], ItemSetList[j]);
                                    int supp = SupportCount(tempList);
                                    if (!(tempList.Count == 0))
                                        nextCandidate.Add(tempList, supp);
                                }

                            }
                            //var tempList = CombineItems(ItemSetList[i], ItemSetList[j]);
                            //int supp = SupportCount(tempList);
                            //if (!(tempList.Count == 0))
                            //    nextCandidate.Add(tempList, supp);

                        }
                        else continue;
                    }
                }
            }

            return nextCandidate;
        }
        // Генерация множества k-элементных частых наборов
        public ItemSet LargeItemSet(ItemSet candidate)
        {
            ItemSet largeItemSet = new ItemSet();

            foreach (var itemset in candidate)
            {
                if (SupportCount(itemset.Key) > minSupp)
                    largeItemSet.Add(itemset.Key, itemset.Value);
            }

            return largeItemSet;
        }
        // расчет минимальной поддержки набора
        public int SupportCount(List<string> set)
        {
            int supp = 0;
            foreach (var kvp in transactions.tidDict)
            {
                if (IsSubset(set, kvp.Value)) supp++;
            }

            return supp;
        }

        // проверка, содержится ли поднабор в наборе
        public bool IsSubset(List<string> subset, List<string> set)
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

        // Объединение наборов в один набор
        public List<string> UnionOfItems(List<string> firstList, List<string> secondList)
        {
            List<string> resList = new List<string>();

            resList = firstList.Union(secondList).ToList();

            return resList;
        }

        public List<string> CombineItems(List<string> firstList, List<string> secondList)
        {
            List<string> resList = new List<string>();
            int k = firstList.Count; // количесво элементов в наборе
            resList = firstList;
            resList.Add(secondList[k - 1]);

            //for(int i = 0; i < k - 1; i++)
            //{
            //    if (firstList[i] == secondList[i])
            //        resList.Add(firstList[i]);     
            //}

            //if (resList.Count == k - 1)
            //    resList.Add(secondList[k - 1]);


            return resList;  
        }

        public List<AssociationRule> GetListOfRules(ItemSet itemset)
        {
            List<AssociationRule> rules = new List<AssociationRule>();

            foreach (var item in itemset)
            {
                foreach (var set in item.Key)
                {
                    rules.Add(GetRule(set, item));

                }
            }
            return rules;
        }

        private AssociationRule GetRule(string set, KeyValuePair<List<string>, int> itemSupp)
        {
            var setItems = set.Split(',');
            for (int i = 0; i < setItems.Count(); i++)
            {
                setItems[i] = setItems[i].Trim();
            }

            AssociationRule rule = new AssociationRule();
            StringBuilder sb = new StringBuilder();
            sb.Append(set).Append(" => ");
            List<string> secondList = new List<string>();
            string seconditem = "";

            foreach (var set2 in itemSupp.Key)
            {
                if (setItems.Contains(set2)) continue;
                seconditem += " " + set2;
                secondList.Add(set2);

            }
            sb.Append(seconditem);
            rule.ruleString = sb.ToString();
            rule.support = Math.Round(((double)itemSupp.Value / transCount) * 100, 2);
            rule.confidence = Math.Round((double)itemSupp.Value / SupportCount(secondList) * 100, 2);
            return rule;
        }

    }


}
