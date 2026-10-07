using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.Json;
record Instruction(int Offset,string Op,string Operand);
record Method(string Type,string Name,List<Instruction> IL);
class Program {
 static readonly Dictionary<short,OpCode> Ops=typeof(OpCodes).GetFields(BindingFlags.Public|BindingFlags.Static).Where(x=>x.FieldType==typeof(OpCode)).Select(x=>(OpCode)x.GetValue(null)).ToDictionary(x=>x.Value);
 static string Token(MetadataReader m,int token){
  if((token&unchecked((int)0xff000000))==0x70000000)return m.GetUserString(MetadataTokens.UserStringHandle(token&0xffffff));
  var h=MetadataTokens.EntityHandle(token);
  return h.Kind switch {
   HandleKind.MethodDefinition=>m.GetString(m.GetTypeDefinition(m.GetMethodDefinition((MethodDefinitionHandle)h).GetDeclaringType()).Name)+"."+m.GetString(m.GetMethodDefinition((MethodDefinitionHandle)h).Name),
   HandleKind.MemberReference=>m.GetString(m.GetMemberReference((MemberReferenceHandle)h).Name),
   HandleKind.FieldDefinition=>m.GetString(m.GetFieldDefinition((FieldDefinitionHandle)h).Name),
   HandleKind.TypeDefinition=>m.GetString(m.GetTypeDefinition((TypeDefinitionHandle)h).Name),
   HandleKind.TypeReference=>m.GetString(m.GetTypeReference((TypeReferenceHandle)h).Name),
   _=>"token:"+token.ToString("x8")};
 }
 static List<Method> Read(string path){
  using var f=File.OpenRead(path);using var pe=new PEReader(f);var m=pe.GetMetadataReader();var all=new List<Method>();
  foreach(var th in m.TypeDefinitions){var t=m.GetTypeDefinition(th);string type=m.GetString(t.Name);
   foreach(var mh in t.GetMethods()){var md=m.GetMethodDefinition(mh);if(md.RelativeVirtualAddress==0)continue;var b=pe.GetMethodBody(md.RelativeVirtualAddress).GetILBytes();var il=new List<Instruction>();int p=0;
    while(p<b.Length){int at=p;short op=b[p++];if(op==0xfe)op=(short)(0xfe00|b[p++]);var code=Ops[op];string value="";int count=0;
     switch(code.OperandType){
      case OperandType.InlineNone:break;
      case OperandType.ShortInlineBrTarget:value=(p+1+(sbyte)b[p]).ToString();count=1;break;
      case OperandType.ShortInlineI:value=((sbyte)b[p]).ToString();count=1;break;
      case OperandType.ShortInlineVar:value=b[p].ToString();count=1;break;
      case OperandType.InlineVar:value=BitConverter.ToUInt16(b,p).ToString();count=2;break;
      case OperandType.InlineI:value=BitConverter.ToInt32(b,p).ToString();count=4;break;
      case OperandType.InlineI8:value=BitConverter.ToInt64(b,p).ToString();count=8;break;
      case OperandType.ShortInlineR:value=BitConverter.ToSingle(b,p).ToString();count=4;break;
      case OperandType.InlineR:value=BitConverter.ToDouble(b,p).ToString();count=8;break;
      case OperandType.InlineBrTarget:value=(p+4+BitConverter.ToInt32(b,p)).ToString();count=4;break;
      case OperandType.InlineSwitch:int n=BitConverter.ToInt32(b,p);count=4+4*n;value="switch:"+n;break;
      default:value=Token(m,BitConverter.ToInt32(b,p));count=4;break;
     }p+=count;il.Add(new(at,code.Name,value));
    }all.Add(new(type,m.GetString(md.Name),il));
   }
  }return all;
 }
 static void Require(bool ok,string why){if(!ok)throw new Exception(why);}
 static void Main(string[] args){
  string managed=args[0],output=args[1],source=args[2];var corePath=Path.Combine(managed,"ZeroToMillionaire.Core.dll");var uiPath=Path.Combine(managed,"Assembly-CSharp.dll");var core=Read(corePath);var ui=Read(uiPath);
  Method Get(List<Method>a,string t,string n)=>a.Single(x=>x.Type==t&&x.Name==n);
  var preserve=Get(core,"SaveStore","PreserveBeforeMigration");var remember=Get(core,"SaveStore","RememberMigration");var stake=Get(core,"GameSession","TrySelectStake");var choose=Get(ui,"GameView","SelectStake");
  int selected=stake.IL.FindIndex(x=>x.Operand.Contains("get_SelectedStake")),clear=stake.IL.FindIndex(x=>x.Operand.Contains("set_Error"));
  Require(selected>=0&&clear>selected,"No-op must compare current stake before clearing errors");
  var noOp=stake.IL.Skip(selected+1).Take(clear-selected-1).ToArray();Require(noOp.Any(x=>x.Op.StartsWith("bne"))&&noOp.Any(x=>x.Op=="ldc.i4.1")&&noOp.Any(x=>x.Op.StartsWith("leave")),"Compiled no-op must return success before Error clear/write");
  int update=choose.IL.FindLastIndex(x=>x.Operand.Contains("UpdateSaveStatus"));Require(choose.IL.Any(x=>x.Operand.Contains("get_SelectedStake"))&&choose.IL.Any(x=>x.Op=="ceq")&&update>0&&choose.IL.Skip(Math.Max(0,update-3)).Take(Math.Min(3,update)).Any(x=>x.Op.StartsWith("ldloc")),"UI success branch must pass actual changed result to UpdateSaveStatus");
  Require(preserve.IL.Count(x=>x.Operand.Contains("ReadOriginal"))>=2&&preserve.IL.Count(x=>x.Operand.Contains("DecodeOriginal"))>=2,"Compiled prewrite must strictly read/classify both originals");
  Require(preserve.IL.Any(x=>x.Operand=="migrationRequired")&&preserve.IL.Any(x=>x.Operand=="migrationCaptureFailed")&&remember.IL.Any(x=>x.Operand=="migrationRequired"),"Compiled pending migration guards required");
  Require(preserve.IL.Any(x=>x.Operand=="Save changed on disk. Close and reopen the game.")&&preserve.IL.Any(x=>x.Operand.Contains("Could not protect the old save")),"Compiled actionable fail-closed errors required");
  Require(ui.Any(x=>x.IL.Any(y=>y.Operand=="Complete earlier tasks first.")),"Compiled shop prerequisite copy required");
  var methods=new[]{preserve,remember,stake,choose};var result=new {status="PASS",sourceCommit=source,scope="Metadata/IL of actual candidate binaries; paired with native runtime snapshot evidence; no physical input",assemblies=new[]{corePath,uiPath}.Select(x=>new{file=Path.GetFileName(x),sha256=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(x))).ToLowerInvariant()}),methods};
  File.WriteAllText(output,JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine("PASS actual compiled fail-closed/no-op/UI/prerequisite checks");
 }
}
