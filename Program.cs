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

namespace LuaInterface
{
	class Program
	{
		public static void Main(string[] args)
		{
			Main_(args);
            Console.ReadKey(true);
		}
		
		public const bool DEBUG = false;
		public static int docall_(lua_State L, int narg, int nres) {
			int status;
            int base_ = Lua.lua_gettop(L) - narg;  /* function index */
            status = Lua.lua_pcall(L, narg, nres, base_);
			return status;
		}
		public static void l_message_(CharPtr pname, CharPtr msg) {
			if (pname != null) Lua.luai_writestringerror("%s: ", pname);
            Lua.luai_writestringerror("%s\n", msg);
		}
		public static lua_State L_;
		public static string dolua_(string message)
		{
			if (DEBUG)
			{
                Lua.fprintf(Lua.stdout, "%s\n", "==============>" + message);
			}
			if (L_ == null) 
			{
                L_ = Lua.luaL_newstate();
                Lua.luaL_openlibs(L_);
			}

			if (DEBUG)
			{
				Lua.fprintf(Lua.stdout, "%s\n", "==============>2");
			}

			string output = null;
			bool printResult = true;
            int status = Lua.luaL_loadbuffer(L_, message, Lua.strlen(message), "=stdin");
			if (status == Lua.LUA_OK) {
				if (DEBUG)
				{
					Lua.fprintf(Lua.stdout, "%s\n", "==============>3");
				}
				status = docall_(L_, 0, printResult ? Lua.LUA_MULTRET : 0);
			}
			if ((status != Lua.LUA_OK) && !Lua.lua_isnil(L_, -1)) {
				if (DEBUG)
				{
					Lua.fprintf(Lua.stdout, "%s\n", "==============>4");
				}
				CharPtr msg = Lua.lua_tostring(L_, -1);
				if (msg == null) msg = "(error object is not a string)";
				output = msg.ToString();
				Lua.lua_pop(L_, 1);
				/* force a complete garbage collection in case of errors */
                Lua.lua_gc(L_, (LuaGCOptions)Lua.LUA_GCCOLLECT, 0);
			} 
			if (printResult)
			{
				//see Lua.LUA_MULTRET
                if (status == Lua.LUA_OK && Lua.lua_gettop(L_) > 0)
                {  /* any result to print? */
					//FIXME://Lua.luaL_checkstack(L_, LUA_MINSTACK, "too many results to print");
				    Lua.lua_getglobal(L_, "print");
                    Lua.lua_insert(L_, 1);
                    if (Lua.lua_pcall(L_, Lua.lua_gettop(L_) - 1, 0, 0) != Lua.LUA_OK)
						l_message_(Lua.progname, Lua.lua_pushfstring(L_,
											   "error calling " + Lua.LUA_QL("print").ToString() + " (%s)",
											   Lua.lua_tostring(L_, -1)));
				}
			}

			return output;
		}		
		
		public static int Main_(string[] args) {
            Lua.fprintf(Lua.stdout, "%s\n", "hello");
			string result;
			result = dolua_("a = 12321");
			if (result != null)
			{
                Lua.fprintf(Lua.stdout, "%s\n", result);
			}
			result = dolua_("print(a); return a, 1");
			if (result != null)
			{
                Lua.fprintf(Lua.stdout, "%s\n", result);
			}
			return 0;
		}		
	}
}