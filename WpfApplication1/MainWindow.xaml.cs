using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        InstructionTemplate[] templates;
        List<Instruction> instructions;
        Instruction currentInstruction;

        const string TYPE_INSTRUCTION_SEPERATOR = "###";

        int currentInstructionIndex;

        public MainWindow()
        {
            instructions = new List<Instruction>();
            InitializeComponent();
            templates = InitializeInstructions();
            InitializeComboBox();
        }
        private void ClearUI ()
        {
            // Clear the stackpanels -- this does mean that any values inputted will be erased
            stackPanel1.Children.Clear();
            stackPanel2.Children.Clear();
        }
        private void CreateUIFromTemplate(InstructionTemplate template, Instruction listInstruction = null)
        {
            ClearUI();

            // First make sure to create labels for the type
            Label typeLabel = new Label();
            typeLabel.Content = "type";
            stackPanel1.Children.Add(typeLabel);

            Label typeLabel2 = new Label();
            typeLabel2.Content = template.type;
            stackPanel2.Children.Add(typeLabel2);

            foreach (string parameter in template.parameters.Keys)
            {
                Label l = new Label();
                l.Content = parameter;
                l.Height = 30;
                stackPanel1.Children.Add(l);

                TextBox tb = new TextBox();
                tb.Height = 30;
                tb.Name = parameter;
                tb.LostFocus += EditedParameter;
                if (listInstruction != null)
                {
                    tb.Text = listInstruction.instructionParameters[parameter];
                }
                stackPanel2.Children.Add(tb);
            }

            if (listInstruction == null)
            {
                Instruction instruction = new Instruction(template);

                if (!(bool)checkBox.IsChecked && instructions.Count > 0)
                {
                    if (listBox.SelectedIndex != -1)
                    {
                        instructions.Insert(listBox.SelectedIndex + 1, instruction);
                        listBox.Items.Insert(listBox.SelectedIndex + 1, instruction);
                        currentInstructionIndex = listBox.SelectedIndex + 1;
                        listBox.SelectedItem = listBox.Items.GetItemAt(currentInstructionIndex);
                    }
                }
                else
                {
                    instructions.Add(instruction);
                    listBox.Items.Add(instruction);
                    currentInstructionIndex = listBox.Items.Count - 1;
                    listBox.SelectedItem = listBox.Items.GetItemAt(currentInstructionIndex);
                }
                
                currentInstruction = instruction;
                
            }
            else
            {
                currentInstruction = listInstruction;
            }
            
        }

        private void EditedParameter (object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            // I mean, the user hasn't even put anything there. Sooooooooo...
            if (string.IsNullOrWhiteSpace(textBox.Text))
                return;

            // Test to see if the value is even valid
            if (InstructionTemplate.CheckValidParameter(currentInstruction.template.parameters[textBox.Name], textBox.Text))
            {
                // Good job end user I'm so proud
                currentInstruction.instructionParameters[textBox.Name] = textBox.Text;
                textBox.Background = Brushes.White;
            }
            else
            {
                // AHFJSKHJDLHSJHKJHK
                textBox.Background = Brushes.Red;
            }
        }

        private void InitializeComboBox()
        {
            if (templates == null)
            {
                Console.WriteLine("Templates are null, cannot initialize combo box");
                return;
            }
            foreach (InstructionTemplate template in templates) {
                // Assume that template has type, should be guaranteed but this still isn't the best
                comboBox.Items.Add(template.type);
            }
        }

        private InstructionTemplate[] InitializeInstructions()
        {
            List<InstructionTemplate> instructionsList = new List<InstructionTemplate>();

            string text = "";

            // Attempt to read instructions.txt
            try
            {
                using (StreamReader reader = new StreamReader("instructions.txt"))
                {
                    text = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Read of instructions.txt failed. Make sure it exists in the same file as this executable");
                Console.Write(e.Message);
            }
            

            string[] instructions = text.Split(new char[] { '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string instruction in instructions)
            {
                InstructionTemplate template = new InstructionTemplate();

                bool hasType = false;
                Dictionary<string, InstructionParameterType> parameters = new Dictionary<string, InstructionParameterType>();
                string[] elements = instruction.Split(',');

                foreach (string element in elements)
                {
                    string[] subelements = element.Split('=');
                    if (subelements.Length != 2)
                    {
                        Console.WriteLine("One element doesn't have two subelements");
                        continue;
                    }

                    subelements[0] = subelements[0].Trim(' ');
                    subelements[1] = subelements[1].Trim(' ');

                    try
                    {
                        if (subelements[0] == "type")
                        {
                            hasType = true;
                            template.type = subelements[1];
                        }
                        else
                        {
                            parameters.Add(subelements[0], (InstructionParameterType)Enum.Parse(typeof(InstructionParameterType), subelements[1]));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                if (!hasType)
                {
                    Console.Write("Instruction invalid : no type!");
                    continue;
                }

                template.parameters = parameters;
                instructionsList.Add(template);
            }
            

            return instructionsList.ToArray();
        }

        public JsonObject[] ReadTxtFile (string textFile)
        {
            List<JsonObject> jsonObjects = new List<JsonObject>();


            textFile = Regex.Replace(textFile, @"\t|\n|\r", "");
            textFile = textFile.Split(new string[] { TYPE_INSTRUCTION_SEPERATOR }, StringSplitOptions.RemoveEmptyEntries)[1];
            string[] instructions = textFile.Split(new string[] { "{", "}", "[", "]", "---" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string instruction in instructions)
            {
                JsonObject jsonObject = new JsonObject() { elements = new List<ObjectElement>() };

                string[] elements = instruction.Split(',');

                foreach (string element in elements)
                {
                    ObjectElement objectElement = new ObjectElement();

                    string[] subelements = element.Split('=');
                    if (subelements.Length != 2)
                    {
                        Console.WriteLine("One element doesn't have two subelements");
                        continue;
                    }

                    subelements[0] = subelements[0].Trim(' ', '"');
                    subelements[1] = subelements[1].Trim(' ', '"');


                    objectElement.element = new KeyValuePair<string, string>(subelements[0], subelements[1]);
                    jsonObject.elements.Add(objectElement);
                    
                }
                jsonObjects.Add(jsonObject);

            }
            return jsonObjects.ToArray();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            // There shouldn't be more than one template for an instruction (i think)
            InstructionTemplate template;
            try
            {
                template = templates.Single(x => x.type == (string)comboBox.SelectedItem);
            }
            catch (InvalidOperationException i)
            {
                Console.WriteLine("what? there isn't a template for the item selected.");
                return;
            }

            CreateUIFromTemplate(template);

        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Instruction instruction = (Instruction)((ListBox)sender).SelectedItem;
            if (instruction != null)
            {
                CreateUIFromTemplate(instruction.template, instruction);
                currentInstructionIndex = ((ListBox)sender).SelectedIndex;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            instructions.RemoveAt(currentInstructionIndex);
            listBox.Items.RemoveAt(currentInstructionIndex);
            currentInstructionIndex--;
            currentInstructionIndex = currentInstructionIndex < 0 ? 0 : currentInstructionIndex;
            if (instructions.Count == 0)
            {
                ClearUI();
                return;
            }
            CreateUIFromTemplate(instructions[currentInstructionIndex].template, instructions[currentInstructionIndex]);
        }

        // Saving private Ryan (dunno why i put that there)
        private void SaveInstructions ()
        {
            if (instructions.Count > 0)
            {
                // Go through one more time and make sure everything is guud
                foreach (Instruction instruction in instructions)
                {
                    foreach (string key in instruction.instructionParameters.Keys)
                    {
                        if (!InstructionTemplate.CheckValidParameter(instruction.template.parameters[key], instruction.instructionParameters[key]))
                        {
                            MessageBoxResult messageBox = MessageBox.Show(("Parameter " + key + " in " + instruction.template.type + " is invalid."));
                            // Ewww a goto
                            goto INVALID;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("wtf are you doing there aren't even any instructions to begin with calm down jesus");
                goto INVALID;
            }

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "instructions";
            dlg.DefaultExt = ".json";
            dlg.Filter = "JSON Files (*.json)|*.json";

            // Show dialog
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string fileName = dlg.FileName;
                StringBuilder sb = new StringBuilder();


                // Maybe make this slightly more sensible?
                sb.Append("[ ");
                // First pass to print out types
                foreach (Instruction ins in instructions)
                {
                    sb.Append(ins.PrintType());
                    if (ins != instructions.Last())
                        sb.Append(",");
                }

                sb.Append(" ]");
                sb.AppendLine();
                sb.AppendLine(TYPE_INSTRUCTION_SEPERATOR);
                sb.Append("[");
                foreach (Instruction ins in instructions)
                {
                    sb.Append(ins.WriteToJson());
                }
                sb.Append("]");
                
                File.WriteAllText(fileName, sb.ToString());
            }

            INVALID:;
        }

        private void button2_Copy_Click(object sender, RoutedEventArgs e)
        {
            SaveInstructions();
        }

        // TODO Add loading (that's pretty important lol)
        public void LoadFile ()
        {
            
            MessageBoxResult mb = MessageBox.Show("Do you wish to save first?", "kek", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (mb == MessageBoxResult.Yes)
            {
                return;
            }

            instructions.Clear();
            listBox.Items.Clear();
            stackPanel1.Children.Clear();
            stackPanel2.Children.Clear();
            currentInstruction = null;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "JSON Files (*.json)|*.json";
            if (dlg.ShowDialog() == true)
            {
                // Actually load the file
                string path = dlg.FileName;
                string text = File.ReadAllText(path);

                JsonObject[] objects = ReadTxtFile(text);

                foreach (JsonObject jsonObject in objects)
                {
                    Instruction instruction = null;
                    Dictionary<string, string> instructionParameters = new Dictionary<string, string>();
                    foreach (ObjectElement element in jsonObject.elements)
                    {
                        if (element.element.Key == "type")
                        {
                            instruction = new Instruction(templates.Single(x => x.type == element.element.Value));
                        }
                        else
                        {
                            instructionParameters.Add(element.element.Key, element.element.Value);
                        }
                    }
                    if (instruction == null)
                    {
                        Console.WriteLine("Probably no type in one of the instructions");
                    }
                    else
                    {
                        instruction.instructionParameters = instructionParameters;
                        instructions.Add(instruction);
                        listBox.Items.Add(instruction);
                    }
                }
            }
        }

        private void button2_Copy1_Click(object sender, RoutedEventArgs e)
        {
            LoadFile();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (currentInstructionIndex == 0)
            {
                return;
            }
            else
            {
                // Swap the two
                Instruction current = currentInstruction;
                listBox.Items.RemoveAt(currentInstructionIndex);
                currentInstructionIndex--;
                Instruction swap = (Instruction)listBox.Items.GetItemAt(currentInstructionIndex);
                listBox.Items.RemoveAt(currentInstructionIndex);
                if (currentInstructionIndex == 0)
                {
                    listBox.Items.Insert(0, swap);
                    listBox.Items.Insert(0, current);
                }
                else
                {
                    listBox.Items.Insert(currentInstructionIndex, current);
                    listBox.Items.Insert(currentInstructionIndex + 1, swap);
                }
                listBox.SelectedItem = current;
            }
        }

        private void button1_Copy_Click(object sender, RoutedEventArgs e)
        {
            if (currentInstructionIndex == listBox.Items.Count)
            {
                return;
            }
            else
            {
                // Swap the two
                Instruction current = currentInstruction;
                listBox.Items.RemoveAt(currentInstructionIndex);
                Instruction swap = (Instruction)listBox.Items.GetItemAt(currentInstructionIndex);
                listBox.Items.RemoveAt(currentInstructionIndex);
                if (currentInstructionIndex == listBox.Items.Count)
                {
                    listBox.Items.Add(swap);
                    listBox.Items.Add(current);
                }
                else
                {
                    listBox.Items.Insert(currentInstructionIndex, current);
                    listBox.Items.Insert(currentInstructionIndex, swap);
                }
                listBox.SelectedItem = current;
            }
        }
    }
}

