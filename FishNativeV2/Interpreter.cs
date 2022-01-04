using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using java.lang;
using IKVM;

namespace FishNativeV2
{
    public enum commands
    {
        CALL,
        PRINT
    }
    
    class variable
    {
        public enum types
        {
            INT,
            STRING,
            DOUBLE
        }
        public types type;
        public string data;
        public int dataI;
        public double dataD;
        public string title;
    }
    class command
    {
        public static double round(double value, int places)
        {
            if (places < 0) throw new IllegalArgumentException();

            long factor = (long)System.Math.Pow(10, places);
            value = value * factor;
            long tmp = (long)System.Math.Round(value);
            return (double)tmp / factor;
        }
        public static double stringMath(string expression)
        {
            var loDataTable = new DataTable();
            var loDataColumn = new DataColumn("Eval", typeof(double), expression);
            loDataTable.Columns.Add(loDataColumn);
            loDataTable.Rows.Add(0);
            return (double)(loDataTable.Rows[0]["Eval"]);
        }
        public commands cmd;
        public string arg;
        public void Run(List<variable> variables)
        {
            List<string> args = arg.Split(",").ToList();
            if(cmd == commands.PRINT)
            {
                string temp = "";
                foreach(string arg_ in args)
                {
                    if (arg_.Contains("\""))
                    {
                        temp += arg_.Replace("\"", "");
                        temp += arg_.Replace("\"", "");
                    }
                    else if (arg_.Contains("m:"))
                    {
                        Console.WriteLine(stringMath(arg_.Replace("m:","")).ToString().Replace(",", "."));
                    }
                    else if (arg_.Contains("+") && !arg_.Contains("m:"))
                    {
                        string _arg = arg_.Replace(" ", "");
                        List<string> _args = _arg.Split("+").ToList();
                        string tempStr = "";
                        foreach (string str in _args)
                        {
                            if (str.Contains("\""))
                            {
                                tempStr += str.Replace("\"", "");
                            }
                            else
                            {
                                foreach (variable Variable in variables)
                                {
                                    if (Variable.title == str.Replace(" ", ""))
                                    {
                                        switch (Variable.type)
                                        {
                                            case variable.types.STRING:
                                                tempStr += Variable.data;
                                                break;
                                            case variable.types.DOUBLE:
                                                tempStr += Variable.dataD.ToString().Replace(",",".");
                                                break;
                                            case variable.types.INT:
                                                tempStr += Variable.dataI;
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (variable Variable in variables)
                        {
                            if (Variable.title == arg_.Replace(" ", ""))
                            {
                                switch (Variable.type)
                                {
                                    case variable.types.DOUBLE:
                                        Console.WriteLine(Variable.dataD.ToString().Replace(",","."));
                                        break;
                                    case variable.types.STRING:
                                        Console.WriteLine(Variable.data);
                                        break;
                                    case variable.types.INT:
                                        Console.WriteLine(Variable.dataI);
                                        break;

                                }
                            }
                        }
                    }
                }
            }
        }
    }
    class func
    {
        public List<command> cmds = new List<command>();
        public string title;
        public List<string> args = new List<string> ();
        public List<variable> internalVariables = new List<variable> ();
        public List<variable> externalVariables = new List<variable> ();
    }

    class parser
    {
        public static List<string> lines;
        public static List<command> commands = new List<command>();
        public static List<func> functions = new List<func>();
        public static List<variable> variables = new List<variable>();
        public parser(string source)
        {
            lines = source.Split("\n").ToList<string>();
            int lineInd = 0;
            foreach (string line in lines)
            {
                ++lineInd;
                var symbols = line.Split(" ");
                if (symbols[0] == "string")
                {
                    variable Variable = new variable();
                    Variable.type = variable.types.STRING;
                    string title = symbols[1];
                    Variable.title = title.Replace(" ","");
                    int index = 2;
                    string temp = "";
                    for (; index < symbols.Length; ++index)
                    {
                        temp += " " + symbols[index];
                    }
                    if (temp.Contains("\""))
                    {
                        string data = "";
                        int scope = 0;
                        int i = 0;
                        foreach (char c in temp)
                        {
                            if (scope == 1 && c != '\"')
                            {
                                data += c.ToString();
                            }
                            if (c == '\"' && scope == 0)
                            {
                                scope += 1;
                            }
                            else if (c == '\"' && scope == 1)
                            {
                                scope = 0;
                            }
                            ++i;

                        }
                        Variable.data = data;
                        variables.Add(Variable);
                    }
                    else
                    {
                        if (line.Contains("+"))
                        {
                            List<string> variableNames = new List<string>();
                            string symbol = "";
                            bool flag = false;
                            foreach(char c in line)
                            {
                                if (flag && c != ' ')
                                {
                                    symbol += c;
                                }
                                if(c == '=')
                                {
                                    flag = true;
                                }
                            }
                            variableNames = symbol.Split("+").ToList();
                            List<string> dataList = new List<string>();
                            string data_ = "";
                            foreach(string name in variableNames)
                            {
                                int i = 0;
                                while (i < variables.Count())
                                {
                                    if (name.Contains(variables[i].title))
                                    {
                                        dataList.Add(variables[i].data);
                                    }
                                    ++i;
                                }
                            }
                            foreach(string str in dataList)
                            {
                                data_ += str;
                            }
                            Variable.data = data_;
                            variables.Add(Variable);

                        }
                        else
                        {
                            string data = "";
                            string temp2 = "";
                            foreach (string symbol in symbols)
                            {
                                temp2 += (" " + symbol);
                            }
                            string titleRef = "";
                            bool flag = false;
                            foreach (char c in temp2)
                            {
                                if (c == '=')
                                {
                                    flag = true;
                                }
                                if (c != ' ' && c != '=' && flag)
                                {
                                    titleRef += c;
                                }
                            }
                            int i = 0;
                            while (i < variables.Count())
                            {
                                if (titleRef.Contains(variables[i].title))
                                {
                                    data = variables[i].data;
                                }
                                ++i;
                            }
                            Variable.data = data;
                            variables.Add(Variable);
                        }
                    }
                }
                else if (symbols[0] == "double")
                {
                    variable Variable = new variable();
                    string name = symbols[1];
                    Variable.title = name;
                    Variable.type = variable.types.DOUBLE;

                    int index = 2;
                    string temp = "";
                    while(index < symbols.Length)
                    {
                        temp += symbols[index];
                        ++index;
                    }
                    string res = "";
                    bool flag = false;
                    foreach(char c in temp)
                    {
                        if (flag)
                        {
                            res += c;
                        }
                        if(c == '=')
                        {
                            flag = true;
                        }
                    }
                    double end = command.stringMath(res);
                    Variable.dataD = end;
                    variables.Add(Variable);
                }
                else if(symbols[0] == "int")
                {
                    variable Variable = new variable();
                    string name = symbols[1];
                    Variable.title = name;
                    Variable.type = variable.types.INT;
                    string temp2 = "";
                    int index = 2;
                    string temp = "";
                    while (index < symbols.Length)
                    {
                        temp += symbols[index];
                        ++index;
                    }
                    string res = "";
                    bool flag = false;
                    bool has_var = false;
                    List<char> alphabet = new char[]{ 'a', 'b', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'y', 'x', 'z' }.ToList<char>();
                    foreach (char c in temp)
                    {
                        if (flag)
                        {
                            res += c;
                            if (!has_var)
                            {
                                foreach (char f in alphabet) 
                                {
                                    if(c == f)
                                    {
                                        has_var = true;
                                    }
                                }
                            }
                        }
                        if (c == '=')
                        {
                            flag = true;
                        }
                    }
                    if (!has_var)
                    {
                        double end = command.stringMath(res);
                    
                        Variable.dataI = (int)command.round(end,3);
                        variables.Add(Variable);
                    }
                    else
                    {
                        int varIndex = 0;
                        List<string> VarName = new List<string>();
                        string final = res;
                        int ind_ = 0;
                        foreach(char c in res)
                        {
                            ++ind_;
                            if (c == '+' || c == '-' || c == '/' || c == '*' || c == '(' || c == ')')
                            {
                                VarName.Add(temp);
                                varIndex += 1;
                                temp2 = "";
                            }
                            else
                            {
                                foreach(char c_ in alphabet)
                                {
                                    if( c == c_)
                                    {
                                        temp2 += c;
                                    }
                                }
                            }
                        }
                        foreach(string _name in VarName)
                        {
                            double data = 0;
                            foreach(variable Var in variables)
                            {
                                if(Var.title == _name)
                                {
                                    if(Var.type == variable.types.DOUBLE)
                                    {
                                        data = Var.dataD;
                                        final.Replace(_name, Var.dataD.ToString());
                                    }
                                    else
                                    {
                                        data = Var.dataI;
                                        final.Replace(_name, Var.dataI.ToString());
                                    }
                                } 
                            }
                        }
                        int end = (int)command.stringMath(final);
                        Variable.dataI = end;
                    }
                }
                else if (symbols[0].Contains("print"))
                {
                    command cmd = new command();
                    cmd.cmd = FishNativeV2.commands.PRINT;
                    string symbol = "";
                    foreach (string s in symbols)
                    {
                        symbol += " " + s;
                    }
                    int flag = 0;
                    int i = 0;
                    foreach (char c in symbol)
                    {
                        if (c == '(' || c == 'm' && symbol[i + 1] == ':' && symbol[i + 2] == '(')
                        {
                            flag = 1;
                        }
                        if (c == ')')
                        {
                            flag = 0;
                        }
                        if (flag == 1 && c != '(')
                        {
                            cmd.arg += c;
                        }
                        ++i;
                    }
                    commands.Add(cmd);
                }
            }
        }
    }   
    class interpreter
    {
        public interpreter(parser Parser) 
        {
            foreach(command cmd in parser.commands)
            {
                cmd.Run(parser.variables);
            }
        }
    }
}
