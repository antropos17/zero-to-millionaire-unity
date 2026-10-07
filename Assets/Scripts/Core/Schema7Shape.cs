using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ZeroToMillionaire
{
    // Small strict JSON shape reader: no new runtime serialization dependency.
    public static class Schema7Shape
    {
        sealed class Node{public char Kind;public string Value;public Dictionary<string,Node> Members;public List<Node> Items;}
        sealed class Reader
        {
            readonly string text;int at;
            public Reader(string s){text=s.Length>0&&s[0]=='\ufeff'?s.Substring(1):s;if(text.Length>65536)throw new FormatException();}
            void Space(){while(at<text.Length&&(text[at]==' '||text[at]=='\t'||text[at]=='\r'||text[at]=='\n'))at++;}
            char Peek(){Space();if(at>=text.Length)throw new FormatException();return text[at];}
            void Take(char c){if(Peek()!=c)throw new FormatException();at++;}
            string Quoted()
            {
                Take('"');var b=new StringBuilder();bool closed=false;
                while(at<text.Length){char c=text[at++];if(c=='"'){closed=true;break;}if(c<32)throw new FormatException();
                    if(c!='\\'){b.Append(c);continue;}if(at==text.Length)throw new FormatException();c=text[at++];
                    switch(c){case '"':case '\\':case '/':b.Append(c);break;case 'b':b.Append('\b');break;case 'f':b.Append('\f');break;case 'n':b.Append('\n');break;case 'r':b.Append('\r');break;case 't':b.Append('\t');break;
                        case 'u':if(at+4>text.Length)throw new FormatException();ushort code;if(!ushort.TryParse(text.Substring(at,4),NumberStyles.AllowHexSpecifier,CultureInfo.InvariantCulture,out code))throw new FormatException();b.Append((char)code);at+=4;break;default:throw new FormatException();}
                }
                if(!closed)throw new FormatException();return b.ToString();
            }
            Node Read(int depth)
            {
                if(depth>32)throw new FormatException();char c=Peek();var n=new Node{Kind=c};
                if(c=='{'){Take('{');n.Members=new Dictionary<string,Node>(StringComparer.Ordinal);if(Peek()=='}'){at++;return n;}
                    while(true){string key=Quoted();Take(':');if(n.Members.ContainsKey(key))throw new FormatException();n.Members.Add(key,Read(depth+1));char next=Peek();if(next=='}'){at++;break;}Take(',');}return n;}
                if(c=='['){Take('[');n.Items=new List<Node>();if(Peek()==']'){at++;return n;}while(true){n.Items.Add(Read(depth+1));char next=Peek();if(next==']'){at++;break;}Take(',');}return n;}
                if(c=='"'){n.Value=Quoted();return n;}
                foreach(string word in new[]{"true","false","null"})if(text.Length-at>=word.Length&&string.CompareOrdinal(text,at,word,0,word.Length)==0){at+=word.Length;n.Kind=word=="null"?'n':'b';n.Value=word;return n;}
                var m=Regex.Match(text.Substring(at),@"\A-?(?:0|[1-9][0-9]*)(?:\.[0-9]+)?(?:[eE][+-]?[0-9]+)?");if(!m.Success)throw new FormatException();at+=m.Length;n.Kind='d';n.Value=m.Value;return n;
            }
            public Node Root(){var n=Read(0);Space();if(at!=text.Length)throw new FormatException();return n;}
        }
        static Node Field(Node n,string name,char kind){Node value;return n!=null&&n.Kind=='{'&&n.Members.TryGetValue(name,out value)&&value.Kind==kind?value:null;}
        static bool Exact(Node n,string[] names){if(n==null||n.Kind!='{'||n.Members.Count!=names.Length)return false;foreach(string name in names)if(!n.Members.ContainsKey(name))return false;return true;}
        static bool Integer(Node n)=>n!=null&&n.Kind=='d'&&Regex.IsMatch(n.Value,@"\A-?(?:0|[1-9][0-9]*)\z")&&int.TryParse(n.Value,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out _);
        static bool Ints(Node root,string name,int length){var n=Field(root,name,'[');if(n==null||n.Items.Count!=length)return false;foreach(var v in n.Items)if(!Integer(v))return false;return true;}
        static bool Bools(Node root,string name,int length){var n=Field(root,name,'[');if(n==null||n.Items.Count!=length)return false;foreach(var v in n.Items)if(v.Kind!='b')return false;return true;}
        public static int Version(string text){try{var n=Field(new Reader(text).Root(),"version",'d');return Integer(n)?int.Parse(n.Value,CultureInfo.InvariantCulture):-1;}catch(FormatException){return -1;}catch(ArgumentException){return -1;}}
        public static bool Valid(string text)
        {
            try
            {
                var root=new Reader(text).Root();if(!Exact(root,new[]{"version","cash","stakeMultiplier","freeSpins","spins","recoveries","heroineU","heroineV","fragments","purchased","lastGrid","quests","tutorial","decorStarted","decorFragments","decorPurchased"})||!Integer(Field(root,"version",'d'))||Field(root,"version",'d').Value!="7")return false;
                foreach(string name in new[]{"cash","stakeMultiplier","freeSpins","spins","recoveries"})if(!Integer(Field(root,name,'d')))return false;
                foreach(string name in new[]{"heroineU","heroineV"})if(Field(root,name,'d')==null)return false;
                if(Field(root,"decorStarted",'b')==null||!Ints(root,"fragments",8)||!Bools(root,"purchased",8)||!Ints(root,"lastGrid",15)||!Ints(root,"decorFragments",4)||!Bools(root,"decorPurchased",4))return false;
                var quests=Field(root,"quests",'[');if(quests==null||quests.Items.Count!=8)return false;
                foreach(var q in quests.Items){if(!Exact(q,new[]{"id","status","cashPrice","fragmentGoal"})||Field(q,"id",'"')==null)return false;foreach(string name in new[]{"status","cashPrice","fragmentGoal"})if(!Integer(Field(q,name,'d')))return false;}
                var tutorial=Field(root,"tutorial",'{');if(!Exact(tutorial,new[]{"version","step","storySeenMask","completed","skipped"}))return false;
                foreach(string name in new[]{"version","step","storySeenMask"})if(!Integer(Field(tutorial,name,'d')))return false;
                foreach(string name in new[]{"completed","skipped"})if(Field(tutorial,name,'b')==null)return false;
                return true;
            }
            catch(FormatException){return false;}catch(ArgumentException){return false;}
        }
    }
    public interface ILevel1CodecV7{string Encode(Level1StateV7 state);Level1StateV7 Decode(string text);}
    public interface ILevel1StoreV7{bool Save(Level1StateV7 state,out string error);Level1StateV7 Load(out string notice);bool RecoveryRequired{get;}}
}
