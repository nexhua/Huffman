using System;
using System.Collections.Generic;
class Node
{
    public String characters = String.Empty;
    public int freq = 0;
    public Node left = null, right = null;
    
    public Node(String S ,int F ,Node left ,Node right)
    {
        characters = S;
        freq = F;
        this.left = left;
        this.right = right;
    }
    public Node(Tuple<String ,int ,String> T1)
    {
        characters = T1.Item1;
        freq = T1.Item2;
        left = right = null;
    }
    public Node(Node node1 ,Node node2)
    {
        characters = node1.characters + node2.characters;
        freq = node1.freq + node2.freq;
        left = node1;
        right = node2;
    }

    public Boolean Contains(Char chr)
    {
        return characters.Contains(chr, StringComparison.InvariantCultureIgnoreCase);
    }
}
class Table
{
    public List<Tuple<String, int ,String>> table = new List<Tuple<String, int ,String>>();//Characters - Frequency - Bit Representation
    public List<Node> nodeList = new List<Node>();//Node list used to build Huffmann tree.
    public IDictionary<Char, String> bitTable = null;//Characters as key to bit values.
    
    public Table(String text)
    {
        IDictionary<String, int> freqTable = new Dictionary<String, int>();
        foreach (char chr in text)
        {
            if (freqTable.ContainsKey(chr.ToString()))
            {
                freqTable[chr.ToString()] += 1;
            }
            else
            {
                freqTable.Add(chr.ToString(), 1);
            }
        }

        foreach (var pair in freqTable)
        {
            table.Add(new Tuple<String, int ,String>(pair.Key, pair.Value ,String.Empty));
        }
        Sort();
    }

    public Node Get()//Returns the first node in nodeList and deletes it from the list.
    {
        if (nodeList.Count == 0)
            return null;
        String tempChar = nodeList[0].characters;
        int tempInt = nodeList[0].freq;
        Node left = nodeList[0].left;
        Node right = nodeList[0].right;
        nodeList.RemoveAt(0);
        return new Node(tempChar ,tempInt ,left ,right);
    }

    public void InsertNode(Node node)//Inserts new node according to its frequency value.
    {
        for(int i=0;i<nodeList.Count;i++)
        {
            if(node.freq < nodeList[i].freq)
            {
                nodeList.Insert(i, node);
                return;
            }
        }
        nodeList.Add(node);
    }

    public void Prepare()//Adds initial values of table which are our leaves we build upon. (Ex :Char :'S' Freq : 5)
    {
        foreach (var tup in table)
        {
            nodeList.Add(new Node(tup));
        }
    }

    public Node Merge(Node node1 ,Node node2)//Creates a new node which has these 2 nodes as children.
    {
        return new Node(node1, node2);
    }

    public void Sort() //Sortes table according to frequency. (Ascending order)
    {
        table.Sort((a, b) => a.Item2.CompareTo(b.Item2));
    }

    //After building the Huffmann tree we can find the bit values of every character in string.
    //Since C# doesn't let us change tuple values after we initialize them we create a tempTable and calculate the
    //bit values of every char. Then we create a Dictionary key as char and value as its bit equivalent.
    public void UpdateTable(Node root)
    {
        List<Tuple<String, int, String>> tempTable = new List<Tuple<String, int, String>>();
        foreach(var pair in table)
        {
            AlterBitValue(root, tempTable, pair);
        }
        table = tempTable;
        Sort();
        bitTable = new Dictionary<Char, String>();
        foreach(var pair in table)
        {
            bitTable.Add(Convert.ToChar(pair.Item1), pair.Item3);
        }
    }
    //This function calculates the bit value.
    public void AlterBitValue(Node root, List<Tuple<String, int, String>> table, Tuple<String, int, String> T1)
    {
        String bit = String.Empty;
        char chr = Convert.ToChar(T1.Item1);
        int freq = T1.Item2;
        while( !(root.left == null && root.right == null) )
        {
            if(root.left.Contains(chr))
            {
                root = root.left;
                bit += "0";
            }
            else
            {
                root = root.right;
                bit += "1";
            }
        }
        table.Add(new Tuple<String, int, String>(chr.ToString(), freq, bit));
    }
    //Prints the table values.(Char ,Frequency, Haufmann Bits) Prints ? if we havent calculated the bit version.
    public void PrintTable()
    {
        Console.WriteLine("FREQUENCY TABLE");
        foreach (var pair in table)
        {
            Console.WriteLine("{0} - {1} - {2}", pair.Item1, pair.Item2 ,pair.Item3 == String.Empty ? "?" : pair.Item3);
        }
        Console.WriteLine();
    }
}

namespace Huffmann
{
    class Program
    {
        static void Main(string[] args)
        {
            String text = "MISSISSIPPI RIVER";
            Node root = new Node("", 0, null, null);
            Table huff = new Table(text);
            huff.PrintTable();

            String encodedText = Encode(huff ,text ,ref root);
            String decodedText = Decode(huff, encodedText, root);

            Console.WriteLine("ORIGINAL STRING : {0}", text);
            Console.WriteLine("ENCODED STRING  : {0}", encodedText);
            Console.WriteLine("DECODED STRING  : {0}", decodedText);

            //Getting the regular binary version of the text to compare with Huffmann encoding.
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(text);
            String regularBinaryEncoding = String.Empty;
            Console.WriteLine("\nREGULAR BINARY CODING : ");
            foreach(byte b in bytes)
            {
                Console.Write(Convert.ToString(b ,2).PadLeft(8 ,'0'));
                regularBinaryEncoding = String.Concat(regularBinaryEncoding, Convert.ToString(b, 2).PadLeft(8, '0'));
            }
            double len1 = encodedText.Length;
            double len2 = regularBinaryEncoding.Length;
            Console.WriteLine("\n\nHAUFMANN ENCODING LENGTH : {0}", len1);
            Console.WriteLine("NORMAL BINARY LENGTH     : {0}", len2);
            Console.WriteLine("HAUFMANN IS %{0:F2} MORE EFFICIENT", 100 - (len1 / len2 * 100));
        }

        static String Encode(Table huff ,String text ,ref Node root)
        {
            huff.Prepare();
            root = BuildTree(huff);
            printTree(root, 0);
            Console.WriteLine();
            huff.UpdateTable(root);
            huff.PrintTable();
            return BuildEncodedString(huff.bitTable, text);
        }

        static String Decode(Table huff, String encoded, Node root)
        {
            String decodedText = String.Empty;
            Node temp = root;
            for (int i = 0; i < encoded.Length; i++)
            {
                if (encoded[i].Equals('0'))
                {
                    if (temp.left == null && temp.right == null)
                    {
                        decodedText = String.Concat(decodedText, temp.characters);
                        temp = root;
                    }
                    temp = temp.left;
                }
                else
                {
                    if (temp.left == null && temp.right == null)
                    {
                        decodedText = String.Concat(decodedText, temp.characters);
                        temp = root;
                    }
                    temp = temp.right;
                }
            }
            decodedText = String.Concat(decodedText, temp.characters);//For some reason it finds the last char after loop.
            return decodedText;
        }
        
        //After setting dictionary of bitTable builds the encoded string accordingly.
        static String BuildEncodedString(IDictionary<Char, String> bitTable, String text)
        {
            String encoded = String.Empty;
            foreach(Char chr in text)
            {
                encoded = String.Concat(encoded, bitTable[chr]);
            }
            return encoded;
        }

        //Continuously merges 2 nodes with min priority until 1 node is left which is root and then returns root node.
        static Node BuildTree(Table huff)
        {
            while (huff.nodeList.Count > 1)
            {
                Node node1 = huff.Get();
                Node node2 = huff.Get();
                Node newNode = huff.Merge(node1, node2);
                huff.InsertNode(newNode);
            }
            Console.WriteLine("HUFFMANN TREE");
            return huff.nodeList[0];  //root node 
        }

        //Helper function to print binary tree.Prints \t after every 1 depth so we can understand it better.
        static void printTab(int level)
        {
            for (int i = 0; i < level; i++)
            {
                Console.Write("\t");
            }
        }

        //Prints tree.
        static void printTree(Node root, int level)
        {
            if (root == null)
            {
                return;
            }
            printTab(level);
            Console.WriteLine("{0}", root.characters+root.freq);
            printTree(root.left, level + 1);
            printTree(root.right, level + 1);
        }
    }
}
