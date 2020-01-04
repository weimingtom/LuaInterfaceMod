/*
 * Created by SharpDevelop.
 * User: 
 * Date: 2020/1/4
 * Time: 10:16
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using LuaInterface511;
using AT.MIN;

namespace LuaInterface
{
	using lua_State = System.IntPtr;
	using CharPtr = System.String;
	
	class Program
	{
//		public static void Main(string[] args)
//		{
//			Console.WriteLine("Hello World!");
//			
//			// TODO: Implement Functionality Here
//			Lua lua = new Lua();
//			GC.Collect();  // runs GC to expose unprotected delegates
//			object[] res=lua.DoString("a=2\nreturn a * 3,3");
//			Console.WriteLine("res == " + res[0] + ", " + res[1]);
//			
//			lua=null;
//			
//			Console.Write("Press any key to continue . . . ");
//			Console.ReadKey(true);
//		}
		
		public static void Main(string[] args)
		{
			Main_(args);
		}
		
		private const string LUA_PROGNAME = "lua";
		static CharPtr progname = LUA_PROGNAME;
		public const int LUA_MINSTACK = 20;
		private const int stdout = 0;
		private const int LUA_OK = 0;
		private const int LUA_MULTRET = -1;
		private const int LUA_GCCOLLECT = 2;
		
		public static CharPtr LUA_QL(CharPtr str)
		{
			return "\"" + str + "\"";
		}
		
		private static void luai_writestringerror(CharPtr str, params object[] argv)
		{
			string result = Tools.sprintf(str.ToString(), argv);
			Console.WriteLine(result);
		}
		
		public static int fprintf(int stream, CharPtr str, params object[] argv)
		{
			string result = Tools.sprintf(str.ToString(), argv);
			Console.WriteLine(result);
			return 1;			
		}
		
		public static CharPtr lua_pushfstring(lua_State luaState, CharPtr str, params object[] argv)
		{
			CharPtr result = Tools.sprintf(str.ToString(), argv);
			Lua.lua_pushstring(luaState, result);
			return result;
		}
		
		public static int strlen(CharPtr str)
		{
			return str.Length;
		}
		
		public const bool DEBUG = false;
		public static int docall_(lua_State L, int narg, int nres) {
			int status;
			int base_ = Lua.lua_gettop(L) - narg;  /* function index */
			status = Lua.lua_pcall(L, narg, nres, base_);
			return status;
		}
		public static void l_message_(CharPtr pname, CharPtr msg) {
			if (pname != null) luai_writestringerror("%s: ", pname);
  			luai_writestringerror("%s\n", msg);
		}
		public static lua_State L_;
		public static string dolua_(string message)
		{
			if (DEBUG)
			{
				fprintf(stdout, "%s\n", "==============>" + message);
			}
			if (L_ == null) 
			{
				L_ = Lua.luaL_newstate();
				Lua.luaL_openlibs(L_);
			}

			if (DEBUG)
			{
				fprintf(stdout, "%s\n", "==============>2");
			}

			string output = null;
			bool printResult = true;
			int status = Lua.luaL_loadbuffer(L_, message, strlen(message), "=stdin");
			if (status == LUA_OK) {
				if (DEBUG)
				{
					fprintf(stdout, "%s\n", "==============>3");
				}
				status = docall_(L_, 0, printResult ? LUA_MULTRET : 0);
			}
			if ((status != LUA_OK) && !Lua.lua_isnil(L_, -1)) {
				if (DEBUG)
				{
					fprintf(stdout, "%s\n", "==============>4");
				}
				CharPtr msg = Lua.lua_tostring(L_, -1);
				if (msg == null) msg = "(error object is not a string)";
				output = msg.ToString();
				Lua.lua_pop(L_, 1);
				/* force a complete garbage collection in case of errors */
				Lua.lua_gc(L_, (LuaGCOptions)LUA_GCCOLLECT, 0);
			} 
			if (printResult)
			{
				//see Lua.LUA_MULTRET
				if (status == LUA_OK && Lua.lua_gettop(L_) > 0) {  /* any result to print? */
					//FIXME://Lua.luaL_checkstack(L_, LUA_MINSTACK, "too many results to print");
				    Lua.lua_getglobal(L_, "print");
					Lua.lua_insert(L_, 1);
					if (Lua.lua_pcall(L_, Lua.lua_gettop(L_) - 1, 0, 0) != LUA_OK)
						l_message_(progname, lua_pushfstring(L_,
											   "error calling " + LUA_QL("print").ToString() + " (%s)",
											   Lua.lua_tostring(L_, -1)));
				}
			}

			return output;
		}		
		
		public static int Main_(string[] args) {
			fprintf(stdout, "%s\n", "hello");
			string result;
			result = dolua_("a = 100");
			if (result != null)
			{
				fprintf(stdout, "%s\n", result);
			}
			result = dolua_("print(a)");
			if (result != null)
			{
				fprintf(stdout, "%s\n", result);
			}
			return 0;
		}		
	}
}